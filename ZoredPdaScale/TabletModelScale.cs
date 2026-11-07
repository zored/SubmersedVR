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
        private Vector3 meshCenter;
        private bool widthIsY;
        private PDA owner;
        internal static float CurrentShift { get; private set; }

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
                if (filter == null || filter.sharedMesh == null) { Debug.LogWarning("Zored PDA: tablet mesh bounds unavailable; scaling skipped."); mesh = null; owner = null; return; }
                meshCenter = filter.sharedMesh.bounds.center;
                // In this model X is height. Of the remaining axes the larger
                // dimension is the tablet face; the smaller is its thickness.
                var dimensions = Vector3.Scale(filter.sharedMesh.bounds.size, originalScale);
                widthIsY = Mathf.Abs(dimensions.y) >= Mathf.Abs(dimensions.z);
                Debug.Log("Zored PDA: height axis=X, width axis=" + (widthIsY ? "Y" : "Z") + ", mesh center=" + meshCenter + ", dimensions=" + dimensions);
            }
            var scale = Mathf.Clamp(Scale.Value, 0.5f, 3f);
            var width = Mathf.Clamp(Width.Value, 0.5f, 3f);
            var newScale = Vector3.Scale(originalScale, widthIsY ? new Vector3(scale, scale * width, 1f) : new Vector3(scale, 1f, scale * width));
            // Compensate for the mesh pivot so its bounds center stays in place.
            mesh.localScale = newScale;
            mesh.localPosition = originalPosition + mesh.localRotation * Vector3.Scale(meshCenter, originalScale - newScale);
            var ui = FindObjectOfType<uGUI_PDA>();
            var currentFilter = mesh.GetComponent<MeshFilter>();
            if (ui != null && currentFilter != null && currentFilter.sharedMesh != null)
            {
                var right = ui.transform.right.normalized;
                var size = currentFilter.sharedMesh.bounds.size;
                float faceWidth = Mathf.Abs(Vector3.Dot(mesh.TransformVector(new Vector3(size.x, 0f, 0f)), right))
                    + Mathf.Abs(Vector3.Dot(mesh.TransformVector(new Vector3(0f, size.y, 0f)), right))
                    + Mathf.Abs(Vector3.Dot(mesh.TransformVector(new Vector3(0f, 0f, size.z)), right));
                float shift = faceWidth * Mathf.Clamp(Plugin.TabletShift.Value, -.5f, .5f);
                mesh.position += right * shift;
                if (Mathf.Abs(CurrentShift - shift) > .00001f)
                {
                    CurrentShift = shift;
                    var scaler = ui.GetComponent<uGUI_CanvasScaler>();
                    if (scaler != null) scaler.SetDirty();
                }
            }
        }

        private void Restore()
        {
            if (mesh != null) { mesh.localScale = originalScale; mesh.localPosition = originalPosition; }
            mesh = null;
            owner = null;
            CurrentShift = 0f;
        }

        private void OnDestroy() { Restore(); }
    }
}

