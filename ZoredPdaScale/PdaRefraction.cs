using System;
using System.IO;
using BepInEx;
using UnityEngine;

namespace ZoredPdaScale
{
    // Change only the screen slot on this PDA instance. All shared game
    // materials are restored when the PDA closes or this companion unloads.
    public sealed class PdaRefraction : MonoBehaviour
    {
        private AssetBundle bundle;
        private Shader shader;
        private PdaHandReflection reflected;
        private Material glass;
        private Material original;
        private int screenIndex = -1;
        private MeshRenderer renderer;
        private bool unavailable;
        private static PdaRefraction active;
        private readonly RippleRules rippleRules = new RippleRules();
        private readonly Vector4[] ripple = new Vector4[4];
        private readonly int[] rippleIds = { Shader.PropertyToID("_Ripple0"), Shader.PropertyToID("_Ripple1"), Shader.PropertyToID("_Ripple2"), Shader.PropertyToID("_Ripple3") };
        private readonly int timeId = Shader.PropertyToID("_RippleTime");
        private int nextRipple;
        private bool pendingPress, touching;
        internal static void ObserveTouch(bool pressed, bool held)
        {
            if (active == null) return;
            active.touching = held;
            if (pressed) active.pendingPress = true;
        }
        private void Awake()
        {
            active = this;
            var path = Path.Combine(Paths.BepInExRootPath, "plugins", "ZoredPdaGlass-v8.bundle");
            try
            {
                if (!File.Exists(path)) { Debug.LogWarning("Zored PDA glass: bundle not found; retaining original material."); unavailable = true; return; }
                bundle = AssetBundle.LoadFromFile(path);
                if (bundle != null)
                    foreach (var candidate in bundle.LoadAllAssets<Shader>())
                        if (candidate != null && candidate.name == "Zored/PDARefraction") { shader = candidate; break; }
                if (shader == null || !shader.isSupported) { Debug.LogWarning("Zored PDA glass: shader unavailable or unsupported; retaining original material."); unavailable = true; return; }
                glass = new Material(shader);
                var mask = bundle.LoadAsset<Shader>("Assets/Shaders/PdaHandReflectionMask.shader");
                if (mask != null && mask.isSupported) reflected = new PdaHandReflection(mask);
                else Debug.LogWarning("Zored PDA reflection: mask shader unavailable.");
                Debug.Log("Zored PDA glass: shader loaded.");
            }
            catch (Exception e) { unavailable = true; Debug.LogWarning("Zored PDA glass: load failed: " + e.Message); }
        }
        private void LateUpdate()
        {
            if (unavailable) return;
            var player = Player.main;
            var pda = player == null ? null : player.GetPDA();
            if (pda == null || !pda.isInUse) { Restore(); return; }
            var mesh = pda.transform.Find("Mesh");
            var current = mesh == null ? null : mesh.GetComponent<MeshRenderer>();
            if (current == null) return;
            if (current == renderer) { UpdateRipples(); if (reflected != null) reflected.Render(renderer, glass); return; }
            Restore();
            var materials = current.sharedMaterials;
            int screen = -1;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null && materials[i].name.Replace(" (Instance)", "").Equals("PDA_screen", StringComparison.OrdinalIgnoreCase)) { screen = i; break; }
            }
            if (screen < 0) { Debug.LogWarning("Zored PDA glass: PDA_screen material not found; retaining original."); unavailable = true; return; }
            original = materials[screen];
            if (original.HasProperty("_MainTex")) glass.SetTexture("_MainTex", original.GetTexture("_MainTex"));
            if (original.HasProperty("_Illum")) glass.SetTexture("_Illum", original.GetTexture("_Illum"));
            screenIndex = screen;
            var replacement = (Material[])materials.Clone();
            replacement[screen] = glass;
            renderer = current;
            renderer.sharedMaterials = replacement;
            SetFaceBounds(current, screen);
            UpdateRipples();
            if (reflected != null) reflected.Render(renderer, glass);
            Debug.Log("Zored PDA glass: screen material applied.");
        }
        private void SetFaceBounds(MeshRenderer current, int submesh)
        {
            var filter = current.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null) return;
            var mesh = filter.sharedMesh;
            var bounds = mesh.bounds;
            try
            {
                if (mesh.isReadable && submesh < mesh.subMeshCount)
                {
                    var vertices = mesh.vertices;
                    var indices = mesh.GetTriangles(submesh);
                    if (indices.Length > 0)
                    {
                        bounds = new Bounds(vertices[indices[0]], Vector3.zero);
                        foreach (int i in indices) bounds.Encapsulate(vertices[i]);
                    }
                }
            }
            catch (Exception e) { Debug.LogWarning("Zored PDA ripples: using mesh bounds: " + e.Message); }
            var b = bounds;
            glass.SetVector("_FaceBounds", new Vector4(b.min.x, b.min.z, Mathf.Max(b.size.x, .001f), Mathf.Max(b.size.z, .001f)));
        }
        private void UpdateRipples()
        {
            if (glass == null || renderer == null) return;
            var now = Time.unscaledTime;
            Vector3 tip;
            var root = SNCameraRoot.main;
            if (touching && root != null && PdaFingerPointer.TryGetTip(out tip))
            {
                var local = renderer.transform.InverseTransformPoint(tip + root.transform.position);
                var bounds = glass.GetVector("_FaceBounds");
                float x = (local.x - bounds.x) / bounds.z, y = (local.z - bounds.y) / bounds.w;
                if (rippleRules.Update(pendingPress, true, x, y, now))
                {
                    ripple[nextRipple] = new Vector4(x, y, now, pendingPress ? 1f : .45f);
                    nextRipple = (nextRipple + 1) % ripple.Length;
                }
            }
            else rippleRules.Update(false, false, 0f, 0f, now);
            pendingPress = false;
            for (int i = 0; i < ripple.Length; i++) glass.SetVector(rippleIds[i], ripple[i]);
            glass.SetFloat(timeId, now);
        }
        private void Restore()
        {
            if (renderer != null && original != null && screenIndex >= 0)
            {
                var materials = renderer.sharedMaterials;
                if (screenIndex < materials.Length && materials[screenIndex] == glass)
                {
                    materials[screenIndex] = original;
                    renderer.sharedMaterials = materials;
                }
            }
            renderer = null; original = null; screenIndex = -1;
            touching = pendingPress = false; rippleRules.Clear(); Array.Clear(ripple, 0, ripple.Length);
        }
        private void OnDestroy()
        {
            Restore();
            if (active == this) active = null;
            if (reflected != null) { reflected.Dispose(); reflected = null; }
            if (glass != null) Destroy(glass);
            if (bundle != null) bundle.Unload(false);
            glass = null; shader = null; bundle = null;
        }
    }
}
