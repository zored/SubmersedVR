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
            var player = Player.main;
            var gamePda = player == null ? null : player.GetPDA();
            if (gamePda != null)
            {
                log.LogInfo("PDA state: isInUse=" + gamePda.isInUse + ", isOpen=" + gamePda.isOpen + ", active=" + gamePda.gameObject.activeInHierarchy);
                log.LogInfo("PDA timing: timeScale=" + Time.timeScale + ", time=" + Time.time + ", unscaledTime=" + Time.unscaledTime + ", focused=" + Application.isFocused + ", gameRunInBackground=" + MiscSettings.runInBackground + ", unityRunInBackground=" + Application.runInBackground);
                log.LogInfo("PDA freeze reasons: " + UWE.FreezeTime.Debug());
                log.LogInfo("PDA object: " + gamePda.name + ", localPosition=" + gamePda.transform.localPosition + ", localRotation=" + gamePda.transform.localEulerAngles + ", scale=" + gamePda.transform.localScale);
                var animator = player.playerAnimator;
                if (animator != null && animator.layerCount > 0)
                {
                    var state = animator.GetCurrentAnimatorStateInfo(0);
                    log.LogInfo("PDA player animation: state=" + state.fullPathHash + ", normalizedTime=" + state.normalizedTime + ", inTransition=" + animator.IsInTransition(0));
                    log.LogInfo("PDA animator: enabled=" + animator.enabled + ", active=" + animator.gameObject.activeInHierarchy + ", speed=" + animator.speed + ", updateMode=" + animator.updateMode + ", nextState=" + animator.GetNextAnimatorStateInfo(0).fullPathHash + ", transitionTime=" + animator.GetAnimatorTransitionInfo(0).normalizedTime);
                }
                var anchor = gamePda.screenAnchor;
                for (int i = 0; anchor != null && i < 8; i++, anchor = anchor.parent)
                    log.LogInfo("PDA anchor ancestor " + i + ": " + anchor.name + ", scale=" + anchor.localScale);
                var renderers = gamePda.GetComponentsInChildren<Renderer>(true);
                foreach (var renderer in renderers)
                    log.LogInfo("PDA renderer: " + renderer.name + ", type=" + renderer.GetType().Name + ", parent=" + (renderer.transform.parent == null ? "none" : renderer.transform.parent.name));
                log.LogInfo("PDA renderer count: " + renderers.Length);
            }
            foreach (var camera in Camera.allCameras)
            {
                log.LogInfo("Camera " + camera.name + ": depth=" + camera.depth + ", clear=" + camera.clearFlags + ", mask=" + camera.cullingMask + ", stereo=" + camera.stereoTargetEye + ", target=" + (camera.targetTexture == null ? "screen" : camera.targetTexture.name));
            }
            var rigType = HarmonyLib.AccessTools.TypeByName("SubmersedVR.VRCameraRig");
            var rigField = rigType == null ? null : HarmonyLib.AccessTools.Field(rigType, "instance");
            var pointerField = rigType == null ? null : HarmonyLib.AccessTools.Field(rigType, "laserPointerUI");
            var rig = rigField == null ? null : rigField.GetValue(null);
            var pointer = rig == null || pointerField == null ? null : pointerField.GetValue(rig) as Component;
            var input = FindObjectOfType<FPSInputModule>();
            if (pointer != null)
            {
                var eventCamera = pointer.GetComponent<Camera>();
                log.LogInfo("Pointer origin=" + pointer.transform.position + ", forward=" + pointer.transform.forward + ", rotation=" + pointer.transform.eulerAngles + ", camera=" + (eventCamera == null ? "none" : eventCamera.name) + ", root=" + (SNCameraRoot.main == null ? "none" : SNCameraRoot.main.transform.position.ToString()));
            }
            if (input != null)
            {
                var hitField = HarmonyLib.AccessTools.Field(typeof(FPSInputModule), "lastRaycastResult");
                var timeField = HarmonyLib.AccessTools.Field(typeof(FPSInputModule), "lastValidRaycastTime");
                var hit = hitField == null ? default(UnityEngine.EventSystems.RaycastResult) : (UnityEngine.EventSystems.RaycastResult)hitField.GetValue(input);
                var hitTime = timeField == null ? 0f : (float)timeField.GetValue(input);
                log.LogInfo("Pointer hit: object=" + (hit.gameObject == null ? "none" : hit.gameObject.name) + ", world=" + hit.worldPosition + ", distance=" + hit.distance + ", age=" + (Time.unscaledTime - hitTime));
            }
            var pda = FindObjectOfType<uGUI_PDA>();
            if (pda != null) log.LogInfo("PDA UI transform: position=" + pda.transform.position + ", rotation=" + pda.transform.eulerAngles + ", scale=" + pda.transform.lossyScale);
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