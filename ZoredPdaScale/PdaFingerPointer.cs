using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace ZoredPdaScale
{
    // Keep the controller's aim direction, but move the UI ray origin to the animated fingertip.
    public sealed class PdaFingerPointer : MonoBehaviour
    {
        private static PdaFingerPointer active;
        private Transform pointer;
        private Vector3 savedLocalPosition;
        private bool positioned;
        private readonly FingerPoseCache<Vector3> pose = new FingerPoseCache<Vector3>();
        private static readonly System.Type rigType = AccessTools.TypeByName("SubmersedVR.VRCameraRig");
        private static readonly FieldInfo instanceField = rigType == null ? null : AccessTools.Field(rigType, "instance");
        private static readonly FieldInfo pointerField = rigType == null ? null : AccessTools.Field(rigType, "laserPointerUI");

        private void Awake() { active = this; }
        private void OnDestroy() { Restore(); if (active == this) active = null; }

        internal static void AfterHandUpdate() { if (active != null) active.Sync(); }
        internal static void BeforeRaycast()
        {
            if (active == null) return;
            var player = Player.main;
            var pda = player == null ? null : player.GetPDA();
            if (pda == null || !pda.isInUse) { active.Restore(); return; }
            Vector3 localPosition;
            if (active.positioned && active.pointer != null && active.pose.TryGet(out localPosition))
                active.pointer.localPosition = localPosition;
        }

        private void Sync()
        {
            var player = Player.main;
            var pda = player == null ? null : player.GetPDA();
            var root = SNCameraRoot.main;
            var rig = instanceField == null ? null : instanceField.GetValue(null);
            var component = rig == null || pointerField == null ? null : pointerField.GetValue(rig) as Component;
            var animator = player == null ? null : player.playerAnimator;
            Transform tip = null;
            if (animator != null)
            {
                const string prefix = "export_skeleton/head_rig/neck/chest/clav_R/clav_R_aim/shoulder_R/";
                const string suffix = "hand_R/hand_R_point_base/hand_R_point_mid/hand_R_point_tip_rig";
                tip = animator.transform.Find(prefix + suffix);
                if (tip == null) tip = animator.transform.Find(prefix + "elbow_R/" + suffix);
            }
            if (pda == null || !pda.isInUse || root == null || component == null || tip == null)
            {
                Restore();
                return;
            }
            if (positioned && pointer != component.transform) Restore();
            if (!positioned)
            {
                pointer = component.transform;
                savedLocalPosition = pointer.localPosition;
                positioned = true;
            }
            // SubmersedVR maps the PDA into UI space by subtracting the camera root position.
            pointer.position = tip.position - root.transform.position;
            pose.Save(pointer.localPosition);
        }

        private void Restore()
        {
            if (positioned && pointer != null) pointer.localPosition = savedLocalPosition;
            positioned = false;
            pointer = null;
            pose.Clear();
        }
    }

    [HarmonyPatch]
    internal static class PdaFingerPointerHandUpdate
    {
        private static MethodBase TargetMethod() { return AccessTools.Method(AccessTools.TypeByName("SubmersedVR.VRHands"), "LateUpdate"); }
        private static void Postfix() { PdaFingerPointer.AfterHandUpdate(); }
    }

    [HarmonyPatch(typeof(FPSInputModule), "GetCursorScreenPosition")]
    internal static class PdaFingerPointerRaycast
    {
        private static void Prefix() { PdaFingerPointer.BeforeRaycast(); }
    }
}
