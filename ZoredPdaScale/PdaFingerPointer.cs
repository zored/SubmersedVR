using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace ZoredPdaScale
{
    // Anchor the laser to the palm at the uncurled index fingertip position.
    // Cache the actual animated fingertip separately for direct touch.
    public sealed class PdaFingerPointer : MonoBehaviour
    {
        private static PdaFingerPointer active;
        // DEBUG(ChatGPT): pose captured in LateUpdate; compare with press-time values to detect frame/transform drift.
        private static Vector3 savedTipWorld, savedRoot, savedTabletLocal;
        private static int savedFrame = -1;
        private Transform pointer;
        private Vector3 savedLocalPosition;
        private bool positioned;
        private readonly PdaVisualContact visualContact = new PdaVisualContact();
        private readonly FingerPoseCache<Vector3> laserPose = new FingerPoseCache<Vector3>();
        private readonly FingerPoseCache<Vector3> tipPose = new FingerPoseCache<Vector3>();
        private static readonly System.Type rigType = AccessTools.TypeByName("SubmersedVR.VRCameraRig");
        private static readonly FieldInfo instanceField = rigType == null ? null : AccessTools.Field(rigType, "instance");
        private static readonly FieldInfo pointerField = rigType == null ? null : AccessTools.Field(rigType, "laserPointerUI");

        private void Awake() { active = this; }
        private void OnDestroy() { Restore(); if (active == this) active = null; }
        internal static void AfterHandUpdate(object instance)
        {
            var field = instance == null ? null : AccessTools.Field(instance.GetType(), "ik");
            var ik = field == null ? null : field.GetValue(instance) as Behaviour;
            if (active != null) active.Sync(ik != null && !ik.enabled);
        }
        internal static void BeforeRaycast()
        {
            if (active == null) return;
            var player = Player.main;
            var pda = player == null ? null : player.GetPDA();
            if (pda == null || !pda.isInUse) { active.Restore(); return; }
            Vector3 position;
            if (active.positioned && active.pointer != null && active.laserPose.TryGet(out position))
                active.pointer.localPosition = position;
        }
        internal static bool TryGetTip(out Vector3 position)
        {
            position = Vector3.zero;
            Vector3 local;
            if (active == null || !active.positioned || active.pointer == null || active.pointer.parent == null || !active.tipPose.TryGet(out local)) return false;
            position = active.pointer.parent.TransformPoint(local);
            return true;
        }
        private void Sync(bool canMoveVisualHand)
        {
            var player = Player.main;
            var pda = player == null ? null : player.GetPDA();
            var root = SNCameraRoot.main;
            var rig = instanceField == null ? null : instanceField.GetValue(null);
            var component = rig == null || pointerField == null ? null : pointerField.GetValue(rig) as Component;
            var animator = player == null ? null : player.playerAnimator;
            Transform hand = null, first = null, middle = null, tip = null;
            if (animator != null)
            {
                const string prefix = "export_skeleton/head_rig/neck/chest/clav_R/clav_R_aim/shoulder_R/";
                hand = animator.transform.Find(prefix + "hand_R");
                if (hand == null) hand = animator.transform.Find(prefix + "elbow_R/hand_R");
                if (hand != null) first = hand.Find("hand_R_point_base");
                if (first != null) middle = first.Find("hand_R_point_mid");
                if (middle != null) tip = middle.Find("hand_R_point_tip_rig");
            }
            if (pda == null || !pda.isInUse || root == null || component == null || component.transform.parent == null || hand == null || first == null || middle == null || tip == null)
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
            // VRHands uses zero local Euler rotation for the uncurled index bones.
            Vector3 straightTipInHand = PdaPalmGeometry.StraightTipInHand(first, middle, tip);
            var rootPosition = root.transform.position;
            pointer.position = hand.TransformPoint(straightTipInHand) - rootPosition;
            laserPose.Save(pointer.localPosition);
            tipPose.Save(pointer.parent.InverseTransformPoint(tip.position - rootPosition));
            savedTipWorld = tip.position; savedRoot = rootPosition; savedFrame = Time.frameCount;
            var tabletMesh = pda.transform.Find("Mesh");
            savedTabletLocal = tabletMesh == null ? Vector3.zero : tabletMesh.InverseTransformPoint(tip.position);
            if (canMoveVisualHand) visualContact.Apply(pda, hand, tip.position);
        }
        // DEBUG(ChatGPT): one line per press in LogOutput.log, prefix "Zored PDA contact precise".
        // ageFrames>0 or tabletLocalNow!=tabletLocalSaved means the tablet moved between pose capture and input.
        // glassUv vs uiUv (0..1 across the screen) is the direct measure of the glass-to-UI mapping error.
        internal static string DescribeContact(PDA pda)
        {
            if (active == null || savedFrame < 0) return "pose unavailable";
            var mesh = pda == null ? null : pda.transform.Find("Mesh");
            var root = SNCameraRoot.main;
            var rootNow = root == null ? Vector3.zero : root.transform.position;
            var nowLocal = mesh == null ? Vector3.zero : mesh.InverseTransformPoint(savedTipWorld);
            return "ageFrames=" + (Time.frameCount - savedFrame) + ", rootDrift=" + (rootNow - savedRoot).ToString("F5") +
                ", tipWorld=" + savedTipWorld.ToString("F5") + ", tabletLocalSaved=" + savedTabletLocal.ToString("F5") +
                ", tabletLocalNow=" + nowLocal.ToString("F5") + ", " + active.visualContact.Describe(mesh, savedTipWorld);
        }
        private void Restore()
        {
            if (positioned && pointer != null) pointer.localPosition = savedLocalPosition;
            positioned = false;
            visualContact.Reset();
            pointer = null;
            laserPose.Clear();
            tipPose.Clear();
        }
    }
    [HarmonyPatch]
    internal static class PdaFingerPointerHandUpdate
    {
        private static MethodBase TargetMethod() { return AccessTools.Method(AccessTools.TypeByName("SubmersedVR.VRHands"), "LateUpdate"); }
        private static void Postfix(object __instance) { PdaFingerPointer.AfterHandUpdate(__instance); }
    }
    [HarmonyPatch(typeof(FPSInputModule), "GetCursorScreenPosition")]
    internal static class PdaFingerPointerRaycast
    {
        private static void Prefix() { PdaFingerPointer.BeforeRaycast(); }
    }
}
