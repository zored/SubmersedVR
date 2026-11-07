using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ZoredPdaScale
{
    // A separate camera samples the UI directly beneath the animated fingertip.
    // It never moves or rotates the controller's laser camera.
    internal static class PdaTouchProbe
    {
        private static readonly System.Type rigType = AccessTools.TypeByName("SubmersedVR.VRCameraRig");
        private static readonly FieldInfo instance = rigType == null ? null : AccessTools.Field(rigType, "instance");
        private static readonly FieldInfo pointer = rigType == null ? null : AccessTools.Field(rigType, "laserPointerUI");
        private static readonly FieldInfo guiCameraField = AccessTools.Field(typeof(SNCameraRoot), "guiCamera");
        private static readonly List<RaycastResult> results = new List<RaycastResult>();
        private static Camera touchCamera;
        private static bool probing, dispatching;
        private static string lastTouch = "unavailable";
        internal static string DescribeTouch() { return lastTouch; }
        internal static bool Dispatching { get { return dispatching; } }
        internal static Vector2 TouchScreenPoint { get { return touchCamera == null ? Vector2.zero : new Vector2(touchCamera.pixelWidth * .5f, touchCamera.pixelHeight * .5f); } }
        internal static Camera EventCamera(uGUI_GraphicRaycaster raycaster)
        {
            return (probing || dispatching) && touchCamera != null && raycaster != null && raycaster.GetComponentInParent<uGUI_PDA>() != null ? touchCamera : null;
        }
        internal static void BeginDispatch() { dispatching = true; }
        internal static void EndDispatch() { dispatching = false; }
        internal static bool TryGet(bool held, out RaycastResult hit, out float distance)
        {
            hit = default(RaycastResult); distance = 0f; lastTouch = "no hit; frame=" + Time.frameCount;
            Vector3 tip;
            var ui = UnityEngine.Object.FindObjectOfType<uGUI_PDA>();
            var root = SNCameraRoot.main;
            var uiCamera = root == null || guiCameraField == null ? null : guiCameraField.GetValue(root) as Camera;
            var rig = instance == null ? null : instance.GetValue(null);
            var component = rig == null || pointer == null ? null : pointer.GetValue(rig) as Component;
            var laserCamera = component == null ? null : component.GetComponent<Camera>();
            var events = EventSystem.current;
            if (ui == null || uiCamera == null || laserCamera == null || events == null || !PdaFingerPointer.TryGetTip(out tip)) return false;
            if (touchCamera == null)
            {
                var go = new GameObject("Zored PDA touch camera");
                UnityEngine.Object.DontDestroyOnLoad(go);
                touchCamera = go.AddComponent<Camera>();
                touchCamera.CopyFrom(laserCamera);
                touchCamera.enabled = false;
                touchCamera.stereoTargetEye = StereoTargetEyeMask.None;
                touchCamera.nearClipPlane = .001f;
            }
            var normal = ui.transform.forward.normalized;
            if (Vector3.Dot(uiCamera.transform.position - ui.transform.position, normal) < 0f) normal = -normal;
            var planeDistance = Vector3.Dot(tip - ui.transform.position, normal);
            var startDistance = PdaSurfaceRules.ProbeStartDistance(planeDistance, held);
            touchCamera.transform.SetPositionAndRotation(tip + normal * (startDistance - planeDistance), Quaternion.LookRotation(-normal, ui.transform.up));
            var data = new PointerEventData(events) { position = TouchScreenPoint };
            results.Clear();
            try { probing = true; events.RaycastAll(data, results); }
            finally { probing = false; }
            foreach (var candidate in results)
            {
                var target = candidate.gameObject;
                if (target == null || candidate.module == null || candidate.module.eventCamera != laserCamera) continue;
                // After probing, the module's normal event camera is again the laser;
                // temporarily use the touch camera only to identify PDA raycasts.
                if (target.GetComponentInParent<uGUI_PDA>() == null) return false;
                if (!held && ExecuteEvents.GetEventHandler<IPointerClickHandler>(target) == null && ExecuteEvents.GetEventHandler<IPointerDownHandler>(target) == null && ExecuteEvents.GetEventHandler<IDragHandler>(target) == null) return false;
                distance = Vector3.Dot(tip - candidate.worldPosition, normal);
                if (!PdaSurfaceRules.AcceptDepth(distance, held)) return false;
                hit = candidate;
                lastTouch = "frame=" + Time.frameCount + ", time=" + Time.unscaledTime.ToString("F4") + ", tipUI=" + tip.ToString("F5") + ", target=" + target.name + ", hitUI=" + candidate.worldPosition.ToString("F5") + ", distance=" + distance.ToString("F5");
                return true;
            }
            return false;
        }
        internal static void Dispose()
        {
            probing = false; dispatching = false;
            if (touchCamera != null) UnityEngine.Object.Destroy(touchCamera.gameObject);
            touchCamera = null; results.Clear();
        }
    }
    [HarmonyPatch(typeof(uGUI_GraphicRaycaster), "eventCamera", MethodType.Getter)]
    internal static class PdaTouchEventCamera
    {
        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        private static void Postfix(uGUI_GraphicRaycaster __instance, ref Camera __result)
        {
            var camera = PdaTouchProbe.EventCamera(__instance);
            if (camera != null) __result = camera;
        }
    }
    [HarmonyPatch(typeof(FPSInputModule), "ProcessMouseEvent")]
    internal static class PdaTouchDispatchScope
    {
        private static void Prefix() { PdaTouchProbe.EndDispatch(); }
        private static Exception Finalizer(Exception __exception) { PdaTouchProbe.EndDispatch(); return __exception; }
    }
}
