using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace ZoredPdaScale
{
    // An opt-in experiment: isolate the PDA from the shared UI camera.
    public sealed class PdaSpatialExperiment : MonoBehaviour
    {
        private readonly ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("Zored PDA Spatial");
        private readonly Dictionary<Transform, int> originalLayers = new Dictionary<Transform, int>();
        private bool requested;
        private bool useSiblingCamera = true;
        private int layer = -1;
        private Camera dedicated;
        private uGUI_PDA target;

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F11))
            {
                requested = !requested;
                log.LogInfo("Spatial experiment " + (requested ? "enabled" : "disabled"));
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.F12))
            {
                useSiblingCamera = !useSiblingCamera;
                log.LogInfo("Spatial camera mode: " + (useSiblingCamera ? "sibling" : "child"));
                Restore();
            }
        }

        private void LateUpdate()
        {
            var player = Player.main;
            var gamePda = player == null ? null : player.GetPDA();
            if (!requested || gamePda == null || !gamePda.isInUse)
            {
                Restore();
                return;
            }
            var pda = FindObjectOfType<uGUI_PDA>();
            var root = SNCameraRoot.main;
            if (pda == null || root == null) { Restore(); return; }
            var cameraField = AccessTools.Field(typeof(SNCameraRoot), "guiCamera");
            var worldField = AccessTools.Field(typeof(SNCameraRoot), "mainCamera");
            var uiCamera = cameraField == null ? null : cameraField.GetValue(root) as Camera;
            var worldCamera = worldField == null ? null : worldField.GetValue(root) as Camera;
            if (uiCamera == null || worldCamera == null || uiCamera.depth <= worldCamera.depth)
            {
                Restore();
                return;
            }
            if (dedicated != null && (target != pda || dedicated.transform.parent != (useSiblingCamera ? uiCamera.transform.parent : uiCamera.transform))) Restore();
            if (dedicated == null && !StartExperiment(pda, uiCamera, worldCamera))
            {
                requested = false;
                return;
            }
            foreach (var child in pda.GetComponentsInChildren<Transform>(true))
            {
                if (child == null || child.gameObject.layer != 5) continue;
                if (!originalLayers.ContainsKey(child)) originalLayers.Add(child, child.gameObject.layer);
                child.gameObject.layer = layer;
            }
        }

        private bool StartExperiment(uGUI_PDA pda, Camera uiCamera, Camera worldCamera)
        {
            int used = 0;
            foreach (var camera in Camera.allCameras) used |= camera.cullingMask;
            foreach (var obj in FindObjectsOfType<Transform>()) used |= 1 << obj.gameObject.layer;
            for (int i = 8; i < 31; i++) if ((used & (1 << i)) == 0) { layer = i; break; }
            if (layer < 0) { log.LogWarning("No unused layer; spatial experiment not started."); return false; }
            var holder = new GameObject("Zored PDA Spatial Camera");
            holder.transform.SetParent(useSiblingCamera ? uiCamera.transform.parent : uiCamera.transform, false);
            dedicated = holder.AddComponent<Camera>();
            dedicated.CopyFrom(uiCamera);
            dedicated.cullingMask = 1 << layer;
            dedicated.clearFlags = CameraClearFlags.Nothing;
            dedicated.depth = (worldCamera.depth + uiCamera.depth) / 2f;
            dedicated.enabled = true;
            target = pda;
            log.LogInfo("PDA-only " + (useSiblingCamera ? "sibling" : "child") + " camera started on layer " + layer + "; depth=" + dedicated.depth);
            return true;
        }

        private void Restore()
        {
            if (dedicated != null)
            {
                dedicated.enabled = false;
                Destroy(dedicated.gameObject);
            }
            foreach (var entry in originalLayers)
                if (entry.Key != null && entry.Key.gameObject.layer == layer) entry.Key.gameObject.layer = entry.Value;
            if (target != null) log.LogInfo("PDA-only camera stopped and original layers restored.");
            originalLayers.Clear();
            dedicated = null;
            target = null;
            layer = -1;
        }

        private void OnDestroy() { Restore(); }
    }
}