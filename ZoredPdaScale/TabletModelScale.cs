using BepInEx.Configuration;
using UnityEngine;

namespace ZoredPdaScale
{
    // Scale only the tablet mesh, leaving its hand and the screen anchor intact.
    public sealed class TabletModelScale : MonoBehaviour
    {
        internal static ConfigEntry<float> Scale;
        internal static ConfigEntry<float> Width;
        private Transform mesh;
        private Vector3 originalScale;
        private Vector3 originalPosition;
        private float leftEdge;
        private PDA owner;

        private void Update()
        {
            if (Plugin.AdjustmentKey(KeyCode.I)) Scale.Value = Mathf.Clamp(Scale.Value + 0.1f, 0.5f, 3f);
            if (Plugin.AdjustmentKey(KeyCode.K)) Scale.Value = Mathf.Clamp(Scale.Value - 0.1f, 0.5f, 3f);
            if (Plugin.AdjustmentKey(KeyCode.U)) { Width.Value = Mathf.Clamp(Width.Value - 0.1f, 0.5f, 3f); Debug.Log("Zored PDA width: " + Width.Value.ToString("F1")); }
            if (Plugin.AdjustmentKey(KeyCode.O)) { Width.Value = Mathf.Clamp(Width.Value + 0.1f, 0.5f, 3f); Debug.Log("Zored PDA width: " + Width.Value.ToString("F1")); }
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
                originalPosition = mesh.localPosition;
                var filter = mesh.GetComponent<MeshFilter>();
                leftEdge = filter != null && filter.sharedMesh != null ? filter.sharedMesh.bounds.min.x : 0f;
            }
            var scale = Mathf.Clamp(Scale.Value, 0.5f, 3f);
            var width = Mathf.Clamp(Width.Value, 0.5f, 3f);
            mesh.localScale = Vector3.Scale(originalScale, new Vector3(scale * width, scale, 1f));
            // Keep the local minimum-X edge fixed while adding width.
            mesh.localPosition = originalPosition + mesh.localRotation * new Vector3(leftEdge * originalScale.x * scale * (1f - width), 0f, 0f);
        }

        private void Restore()
        {
            if (mesh != null) { mesh.localScale = originalScale; mesh.localPosition = originalPosition; }
            mesh = null;
            owner = null;
        }

        private void OnDestroy() { Restore(); }
    }
}