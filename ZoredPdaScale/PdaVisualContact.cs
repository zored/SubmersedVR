using System;
using UnityEngine;
namespace ZoredPdaScale
{
    // Visual-only surface constraint. Input continues to use the unmodified tip.
    internal sealed class PdaVisualContact
    {
        private MeshRenderer renderer;
        private Bounds face;
        private bool valid, useBoundsFace;
        private readonly VisualContactRules contact = new VisualContactRules();
        internal void Reset() { renderer = null; valid = false; contact.Step(false, 0f); }
        internal void Apply(PDA pda, Transform hand, Vector3 trackedTip)
        {
            var mesh = pda == null ? null : pda.transform.Find("Mesh");
            var current = mesh == null ? null : mesh.GetComponent<MeshRenderer>();
            if (current != renderer) LoadFace(current);
            var camera = Camera.main;
            if (!valid || camera == null) { contact.Step(false, 0f); return; }
            var local = mesh.InverseTransformPoint(trackedTip);
            if (local.x < face.min.x || local.x > face.max.x || local.z < face.min.z || local.z > face.max.z)
            { contact.Step(false, 0f); return; }
            float planeY = face.center.y;
            if (useBoundsFace) planeY = mesh.InverseTransformPoint(camera.transform.position).y >= face.center.y ? face.max.y : face.min.y;
            var plane = mesh.TransformPoint(new Vector3(local.x, planeY, local.z));
            var normal = mesh.TransformDirection(Vector3.up).normalized;
            if (Vector3.Dot(camera.transform.position - plane, normal) < 0f) normal = -normal;
            float distance = Vector3.Dot(trackedTip - plane, normal);
            if (contact.Step(true, distance) && distance < 0f) hand.position -= normal * distance;
        }
        // DEBUG(ChatGPT): glassUv is the fingertip projected onto the screen face, normalized over face X/Z.
        // mode=bounds means the mesh was unreadable and the whole-model bounds stand in for the glass.
        internal string Describe(Transform mesh, Vector3 world)
        {
            if (mesh == null || !valid) return "glass=unavailable";
            var local = mesh.InverseTransformPoint(world);
            var uv = new Vector2((local.x - face.min.x) / face.size.x, (local.z - face.min.z) / face.size.z);
            return "glassMode=" + (useBoundsFace ? "bounds" : "submesh") + ", faceMin=" + face.min.ToString("F5") + ", faceMax=" + face.max.ToString("F5") +
                ", glassUv=" + uv.ToString("F4") + ", glassDepthLocal=" + (local.y - face.center.y).ToString("F5");
        }
        private void LoadFace(MeshRenderer current)
        {
            Reset(); renderer = current;
            if (current == null) return;
            var filter = current.GetComponent<MeshFilter>();
            var materials = current.sharedMaterials;
            var mesh = filter == null ? null : filter.sharedMesh;
            if (mesh == null) return;
            useBoundsFace = !mesh.isReadable;
            if (useBoundsFace) { face = mesh.bounds; valid = face.size.x > .0001f && face.size.z > .0001f; return; }
            try
            {
                int index = -1;
                for (int i = 0; i < materials.Length; ++i)
                    if (materials[i] != null && (materials[i].name.StartsWith("PDA_screen", StringComparison.OrdinalIgnoreCase) || (materials[i].shader != null && materials[i].shader.name == "Zored/PDARefraction"))) { index = i; break; }
                if (index < 0 || index >= mesh.subMeshCount) return;
                var indices = mesh.GetTriangles(index); var vertices = mesh.vertices;
                if (indices.Length == 0) return;
                face = new Bounds(vertices[indices[0]], Vector3.zero);
                foreach (var i in indices) face.Encapsulate(vertices[i]);
                valid = face.size.x > .0001f && face.size.z > .0001f;
            }
            catch (Exception e) { Debug.LogWarning("Zored PDA visual contact unavailable: " + e.Message); }
        }
    }
}
