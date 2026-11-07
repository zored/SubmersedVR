using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace ZoredPdaScale
{
    public sealed class ToolRecall : MonoBehaviour
    {
        private Inventory owner;
        private InventoryItem lastItem;
        private static readonly System.Type rigType = AccessTools.TypeByName("SubmersedVR.VRCameraRig");
        private static readonly FieldInfo instance = rigType == null ? null : AccessTools.Field(rigType, "instance");
        private static readonly FieldInfo controllerField = rigType == null ? null : AccessTools.Field(rigType, "rightController");
        private static readonly System.Type poseType = AccessTools.TypeByName("Valve.VR.SteamVR_Behaviour_Pose");
        private static readonly PropertyInfo validPose = poseType == null ? null : AccessTools.Property(poseType, "isValid");

        private void Update()
        {
            var inventory = Inventory.main;
            var player = Player.main;
            if (inventory != owner) { owner = inventory; lastItem = null; }
            if (inventory == null || player == null || inventory.quickSlots == null) return;
            var slots = inventory.quickSlots;
            var held = inventory.GetHeld();
            var active = slots.GetActiveSlotID();
            if (held != null && active >= 0 && active < slots.GetSlotCount())
            {
                var item = slots.GetSlotItem(active);
                if (item != null) lastItem = item;
            }
            var pda = player.GetPDA();
            bool menu = IngameMenu.main != null && IngameMenu.main.gameObject.activeInHierarchy;
            bool inPda = pda != null && pda.isInUse;
            if (!GameInput.GetButtonDown(GameInput.Button.MoveUp)) return;
            if (Time.timeScale == 0f || inPda || menu || player.currentMountedVehicle != null) return;
            var rig = instance == null ? null : instance.GetValue(null);
            var controller = rig == null || controllerField == null ? null : controllerField.GetValue(rig) as GameObject;
            var head = MainCamera.camera;
            if (controller == null || !controller.activeInHierarchy || head == null || poseType == null || validPose == null) return;
            var pose = controller.GetComponent(poseType);
            if (pose == null || !(bool)validPose.GetValue(pose, null)) return;
            var forward = Vector3.ProjectOnPlane(head.transform.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < .5f) return;
            var delta = controller.transform.position - head.transform.position;
            var right = Vector3.Cross(Vector3.up, forward);
            float x = Vector3.Dot(delta, right), y = delta.y, z = Vector3.Dot(delta, forward);
            if (ToolRecallRules.ShoulderZone(x, y, z))
            {
                for (int i = 0; i < slots.GetSlotCount(); i++)
                {
                    TechType type = slots.GetSlotBinding(i);
                    if (type != TechType.Knife && type != TechType.HeatBlade) continue;
                    if (i != active) slots.SlotKeyDown(i);
                    Debug.Log("Zored tool recall: shoulder knife slot " + i);
                    return;
                }
                Debug.Log("Zored tool recall: knife not in quick slots");
                return;
            }
            if (!ToolRecallRules.InZone(x, y, z)) return;
            if (held != null && active >= 0) { slots.DeselectSlots(); Debug.Log("Zored tool recall: holster"); return; }
            if (held != null || lastItem == null) return;
            int slot = slots.GetSlotByItem(lastItem);
            if (slot < 0 || slot >= slots.GetSlotCount() || slots.GetSlotItem(slot) != lastItem) { Debug.Log("Zored tool recall: last item is no longer bound"); return; }
            slots.SlotKeyDown(slot);
            Debug.Log("Zored tool recall: requested slot " + slot);
        }
    }
}




