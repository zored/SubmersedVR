using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace ZoredPdaScale
{
    // Read-only snapshot for diagnosing PDA rendering in the actual VR session.
    public sealed class DepthProbe : MonoBehaviour
    {
        private readonly ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("Zored PDA Depth");

        private void Update()
        {
            if (!UnityEngine.Input.GetKeyDown(KeyCode.F10)) return;
            log.LogInfo("Snapshot begin");
            foreach (var camera in Camera.allCameras)
            {
                log.LogInfo("Camera " + camera.name + ": depth=" + camera.depth + ", clear=" + camera.clearFlags + ", mask=" + camera.cullingMask + ", stereo=" + camera.stereoTargetEye + ", target=" + (camera.targetTexture == null ? "screen" : camera.targetTexture.name));
            }
            var pda = FindObjectOfType<uGUI_PDA>();
            if (pda == null)
            {
                log.LogWarning("PDA UI not found; open the PDA and press F10 again.");
                return;
            }
            foreach (var canvas in pda.GetComponentsInChildren<Canvas>(true))
            {
                log.LogInfo("Canvas " + canvas.name + ": active=" + canvas.gameObject.activeInHierarchy + ", layer=" + canvas.gameObject.layer + ", mode=" + canvas.renderMode + ", camera=" + (canvas.worldCamera == null ? "none" : canvas.worldCamera.name) + ", sortingLayer=" + canvas.sortingLayerID + ", order=" + canvas.sortingOrder + ", override=" + canvas.overrideSorting);
            }
            var seen = new HashSet<int>();
            foreach (var graphic in pda.GetComponentsInChildren<Graphic>(true))
            {
                if (!graphic.gameObject.activeInHierarchy || graphic.materialForRendering == null) continue;
                var material = graphic.materialForRendering;
                if (!seen.Add(material.GetInstanceID())) continue;
                var shader = material.shader;
                log.LogInfo("Material " + material.name + ": graphic=" + graphic.name + ", shader=" + (shader == null ? "none" : shader.name) + ", queue=" + material.renderQueue + ", ZTest=" + (material.HasProperty("_ZTest") ? material.GetFloat("_ZTest").ToString() : "unset") + ", ZWrite=" + (material.HasProperty("_ZWrite") ? material.GetFloat("_ZWrite").ToString() : "unset"));
                if (seen.Count >= 30) { log.LogInfo("Material list capped at 30"); break; }
            }
            log.LogInfo("Snapshot end");
        }
    }
}