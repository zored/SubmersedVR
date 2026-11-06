using BepInEx.Configuration;
using UnityEngine;

namespace ZoredPdaScale
{
    // Scale only the tablet mesh, leaving its hand and the screen anchor intact.
    public sealed class TabletModelScale : MonoBehaviour
    {
        internal static ConfigEntry<float> Scale;
        private Transform mesh;
        private Vector3 originalScale;
        private PDA owner;

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.KeypadPlus)) Scale.Value = Mathf.Clamp(Scale.Value + 0.1f, 0.5f, 3f);
            if (UnityEngine.Input.GetKeyDown(KeyCode.KeypadMinus)) Scale.Value = Mathf.Clamp(Scale.Value - 0.1f, 0.5f, 3f);
        }

        private void LateUpdate()
        {
            var player = Player.main;
            var pda = player == null ? null : player.GetPDA();
            if (pda == null || !pda.isInUse) { Restore(); return; }
            if (owner != pda || mesh == null)
            {
                Restore();
                var candidate = pda.transform.Find("Mesh");
                if (candidate == null || candidate.GetComponent<MeshRenderer>() == null) return;
                owner = pda;
                mesh = candidate;
                originalScale = mesh.localScale;
            }
            var scale = Mathf.Clamp(Scale.Value, 0.5f, 3f);
            mesh.localScale = Vector3.Scale(originalScale, new Vector3(scale, scale, 1f));
        }

        private void Restore()
        {
            if (mesh != null) mesh.localScale = originalScale;
            mesh = null;
            owner = null;
        }

        private void OnDestroy() { Restore(); }
    }
}