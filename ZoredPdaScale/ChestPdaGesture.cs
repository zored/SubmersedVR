using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace ZoredPdaScale
{
    // Reuse the existing left-grip MoveDown binding and the game's normal PDA path.
    [HarmonyPatch(typeof(GameInput), "GetButtonDown")]
    internal static class ChestPdaGesture
    {
        private static readonly System.Type rigType = AccessTools.TypeByName("SubmersedVR.VRCameraRig");
        private static readonly FieldInfo rigInstance = rigType == null ? null : AccessTools.Field(rigType, "instance");
        private static readonly FieldInfo leftController = rigType == null ? null : AccessTools.Field(rigType, "leftController");
        private static readonly System.Type poseType = AccessTools.TypeByName("Valve.VR.SteamVR_Behaviour_Pose");
        private static readonly PropertyInfo validPose = poseType == null ? null : AccessTools.Property(poseType, "isValid");
        private static int loggedFrame = -1;

        [HarmonyPostfix]
        [HarmonyAfter("SubmersedVR")]
        private static void Postfix(GameInput.Button action, ref bool __result)
        {
            if (action != GameInput.Button.PDA || __result || Plugin.ChestGesture == null || !Plugin.ChestGesture.Value) return;
            var player = Player.main;
            var pda = player == null ? null : player.GetPDA();
            if (pda == null || (!pda.isInUse && Time.timeScale == 0f) || !ChestGestureRules.MayToggle(pda.isInUse, pda.isOpen, IngameMenu.main != null && IngameMenu.main.gameObject.activeInHierarchy, GameInput.GetButtonHeld(GameInput.Button.MoveUp)) || !GameInput.GetButtonDown(GameInput.Button.MoveDown)) return;
            var rig = rigInstance == null ? null : rigInstance.GetValue(null);
            var controller = rig == null || leftController == null ? null : leftController.GetValue(rig) as GameObject;
            var head = MainCamera.camera;
            if (controller == null || !controller.activeInHierarchy || head == null || poseType == null || validPose == null) return;
            var pose = controller.GetComponent(poseType);
            if (pose == null || !(bool)validPose.GetValue(pose, null)) return;
            var forward = Vector3.ProjectOnPlane(head.transform.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < .5f) return;
            var delta = controller.transform.position - head.transform.position;
            var right = Vector3.Cross(Vector3.up, forward);
            float x = Vector3.Dot(delta, right), y = delta.y, z = Vector3.Dot(delta, forward);
            if (ChestGestureRules.InZone(x, y, z)) __result = true;
            if (loggedFrame != Time.frameCount)
            {
                loggedFrame = Time.frameCount;
                Debug.Log("Zored PDA left grip: relative=" + x.ToString("F2") + "," + y.ToString("F2") + "," + z.ToString("F2") + "; gesture=" + __result);
            }
        }
    }
}


