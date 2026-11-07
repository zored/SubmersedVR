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
            Snapshot();
        }

        internal void Snapshot()
        {
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
                try
                {
                    var actions = HarmonyLib.AccessTools.TypeByName("Valve.VR.SteamVR_Actions");
                    var actionProperty = actions == null ? null : HarmonyLib.AccessTools.Property(actions, "subnautica_UIMenu");
                    var action = actionProperty == null ? null : actionProperty.GetValue(null, null);
                    if (action != null)
                    {
                        var active = HarmonyLib.AccessTools.Property(action.GetType(), "active");
                        var binding = HarmonyLib.AccessTools.Property(action.GetType(), "activeBinding");
                        log.LogInfo("Pause SteamVR action: active=" + (active == null ? "unknown" : active.GetValue(action, null)) + ", binding=" + (binding == null ? "unknown" : binding.GetValue(action, null)));
                    }
                    else log.LogInfo("Pause SteamVR action unavailable");
                }
                catch (System.Exception e) { log.LogWarning("Pause SteamVR action diagnostic failed: " + e.Message); }
                var lockField = HarmonyLib.AccessTools.Field(typeof(FPSInputModule), "lockPauseMenu");
                log.LogInfo("Pause diagnostic: UIMenu held=" + GameInput.GetButtonHeld(GameInput.Button.UIMenu) + ", lockPauseMenu=" + (lockField == null ? "unknown" : lockField.GetValue(input).ToString()) + ", menu=" + (IngameMenu.main == null ? "none" : IngameMenu.main.gameObject.activeInHierarchy.ToString()));
                log.LogInfo("Raycast at event: " + PdaRaycastCapture.Describe());
                var hitField = HarmonyLib.AccessTools.Field(typeof(FPSInputModule), "lastRaycastResult");
                var timeField = HarmonyLib.AccessTools.Field(typeof(FPSInputModule), "lastValidRaycastTime");
                var hit = hitField == null ? default(UnityEngine.EventSystems.RaycastResult) : (UnityEngine.EventSystems.RaycastResult)hitField.GetValue(input);
                var hitTime = timeField == null ? 0f : (float)timeField.GetValue(input);
                log.LogInfo("Pointer hit: object=" + (hit.gameObject == null ? "none" : hit.gameObject.name) + ", world=" + hit.worldPosition + ", distance=" + hit.distance + ", age=" + (Time.unscaledTime - hitTime));
                var module = hit.module;
                var camera = module == null ? null : module.eventCamera;
                log.LogInfo("Pointer raycast: module=" + (module == null ? "none" : module.GetType().FullName + "/" + module.name) + ", camera=" + (camera == null ? "none" : camera.name) + ", screen=" + hit.screenPosition.ToString("F3") + ", world=" + hit.worldPosition.ToString("F5") + ", distance=" + hit.distance.ToString("F5"));
                if (camera != null)
                {
                    var ray = camera.ScreenPointToRay(hit.screenPosition);
                    log.LogInfo("Pointer event ray: origin=" + ray.origin.ToString("F5") + ", direction=" + ray.direction.ToString("F5") + ", cameraPosition=" + camera.transform.position.ToString("F5") + ", cameraForward=" + camera.transform.forward.ToString("F5"));
                }
                if (pointer != null)
                {
                    var origin = pointer.transform.position;
                    var direction = pointer.transform.forward;
                    var delta = hit.worldPosition - origin;
                    log.LogInfo("Pointer geometry: origin=" + origin.ToString("F5") + ", forward=" + direction.ToString("F5") + ", hitAlong=" + Vector3.Dot(delta, direction).ToString("F5") + ", hitOffAxis=" + Vector3.Cross(delta, direction).magnitude.ToString("F5") + ", root=" + (SNCameraRoot.main == null ? "none" : SNCameraRoot.main.transform.position.ToString("F5")));
                }
            }
            Vector3 diagnosticTip;
            if (PdaFingerPointer.TryGetTip(out diagnosticTip) && SNCameraRoot.main != null)
            {
                var tipWorld = diagnosticTip + SNCameraRoot.main.transform.position;
                log.LogInfo("Touch sample: " + PdaTouchProbe.DescribeTouch());
                log.LogInfo("Finger coordinates: ui=" + diagnosticTip.ToString("F5") + ", world=" + tipWorld.ToString("F5"));
                var mesh = gamePda == null ? null : gamePda.transform.Find("Mesh");
                var filter = mesh == null ? null : mesh.GetComponent<MeshFilter>();
                if (mesh != null && filter != null && filter.sharedMesh != null)
                {
                    var bounds = filter.sharedMesh.bounds;
                    var local = mesh.InverseTransformPoint(tipWorld);
                    log.LogInfo("Tablet geometry: position=" + mesh.position.ToString("F5") + ", rotation=" + mesh.rotation.ToString("F5") + ", scale=" + mesh.lossyScale.ToString("F5") + ", tipLocal=" + local.ToString("F5") + ", boundsCenter=" + bounds.center.ToString("F5") + ", boundsSize=" + bounds.size.ToString("F5"));
                }
            }
            else log.LogInfo("Finger coordinates unavailable");
            var pda = FindObjectOfType<uGUI_PDA>();
            if (pda != null) log.LogInfo("PDA UI transform: position=" + pda.transform.position + ", rotation=" + pda.transform.eulerAngles + ", scale=" + pda.transform.lossyScale);
            if (pda != null && PdaFingerPointer.TryGetTip(out diagnosticTip)) log.LogInfo("Finger UI local=" + pda.transform.InverseTransformPoint(diagnosticTip).ToString("F5"));
            if (pda != null) log.LogInfo("PDA UI precise: position=" + pda.transform.position.ToString("F5") + ", rotation=" + pda.transform.rotation.ToString("F5") + ", scale=" + pda.transform.lossyScale.ToString("F7"));
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

    // Capture the camera at the same instant as the sorted EventSystem result.
    [HarmonyLib.HarmonyPatch(typeof(FPSInputModule), "UpdateMouseState")]
    internal static class PdaRaycastCapture
    {
        private static bool captured;
        private static float time;
        private static Vector3 origin, direction, world;
        private static Vector2 screen;
        private static string target, moduleName, cameraName;

        private static void Prefix(UnityEngine.EventSystems.PointerEventData leftData)
        {
            var hit = leftData.pointerCurrentRaycast;
            var module = hit.module;
            var camera = module == null ? null : module.eventCamera;
            if (camera == null || camera.name != "laserPointerUI") return;
            captured = true;
            time = Time.unscaledTime;
            origin = camera.transform.position;
            direction = camera.transform.forward;
            world = hit.worldPosition;
            screen = hit.screenPosition;
            target = hit.gameObject == null ? "none" : hit.gameObject.name;
            moduleName = module.GetType().FullName + "/" + module.name;
            cameraName = camera.name;
        }

        internal static string Describe()
        {
            if (!captured) return "no event captured";
            var delta = world - origin;
            return "age=" + (Time.unscaledTime - time).ToString("F4") + ", target=" + target + ", module=" + moduleName + ", camera=" + cameraName + ", screen=" + screen.ToString("F3") + ", origin=" + origin.ToString("F5") + ", forward=" + direction.ToString("F5") + ", hit=" + world.ToString("F5") + ", offAxis=" + Vector3.Cross(delta, direction).magnitude.ToString("F5");
        }
    }
}
