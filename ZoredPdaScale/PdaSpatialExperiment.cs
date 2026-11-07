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
        private readonly Dictionary<Camera, bool> cameraMasks = new Dictionary<Camera, bool>();
        private bool requested = true;
        private bool useSiblingCamera = true;
        private int layer = -1;
        private Camera dedicated;
        private uGUI_PDA target;
        private float nextLayerAttempt;
        private bool warnedNoLayer;
        private float nextCollisionCheck;

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F11))
            {
                Toggle();
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.F12))
            {
                ToggleMode();
            }
        }

        internal void Toggle()
        {
            requested = !requested;
            nextLayerAttempt = 0f;
            warnedNoLayer = false;
            log.LogInfo("Spatial experiment " + (requested ? "enabled" : "disabled"));
        }
        internal void ToggleMode()
        {
            useSiblingCamera = !useSiblingCamera;
            log.LogInfo("Spatial camera mode: " + (useSiblingCamera ? "sibling" : "child"));
            Restore();
        }
        internal string CameraStatus { get { return requested ? "On" : "Off"; } }
        internal string CameraMode { get { return useSiblingCamera ? "Sibling" : "Child"; } }

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
            if (dedicated == null && MenuControlRules.ShouldRetryCamera(Time.unscaledTime, nextLayerAttempt) && !StartExperiment(pda, uiCamera, worldCamera))
            {
                nextLayerAttempt = Time.unscaledTime + 2f;
                return;
            }
            if (dedicated == null) return;
            ReserveCameraLayers();
            if (Time.unscaledTime >= nextCollisionCheck)
            {
                nextCollisionCheck = Time.unscaledTime + 2f;
                if (LayerHasForeignObjects()) { Restore(); return; }
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
            foreach (var obj in Resources.FindObjectsOfTypeAll<Transform>())
                if (obj != null && obj.gameObject.scene.IsValid()) used |= 1 << obj.gameObject.layer;
            for (int i = 8; i < 31; i++) if ((used & (1 << i)) == 0) { layer = i; break; }
            if (layer < 0)
            {
                if (!warnedNoLayer) log.LogWarning("No unused layer; retrying PDA camera while requested.");
                warnedNoLayer = true;
                return false;
            }
            warnedNoLayer = false;
            var holder = new GameObject("Zored PDA Spatial Camera");
            holder.transform.SetParent(useSiblingCamera ? uiCamera.transform.parent : uiCamera.transform, false);
            dedicated = holder.AddComponent<Camera>();
            dedicated.CopyFrom(uiCamera);
            dedicated.cullingMask = 1 << layer;
            dedicated.clearFlags = CameraClearFlags.Nothing;
            dedicated.depth = (worldCamera.depth + uiCamera.depth) / 2f;
            dedicated.enabled = true;
            ReserveCameraLayers();
            target = pda;
            log.LogInfo("PDA-only " + (useSiblingCamera ? "sibling" : "child") + " camera started on layer " + layer + "; depth=" + dedicated.depth);
            return true;
        }

        private bool LayerHasForeignObjects()
        {
            // FPSInputModule moves its own cursor to the current UI target's layer.
            // It is part of this rendering path, not a layer collision.
            var input = FindObjectOfType<FPSInputModule>();
            var cursorField = AccessTools.Field(typeof(FPSInputModule), "_cursorCanvasGO");
            var cursor = input == null || cursorField == null ? null : cursorField.GetValue(input) as GameObject;
            var tooltip = FindObjectOfType<uGUI_Tooltip>();
            foreach (var obj in Resources.FindObjectsOfTypeAll<Transform>())
            {
                if (obj == null || !obj.gameObject.scene.IsValid() || obj.gameObject.layer != layer || originalLayers.ContainsKey(obj)) continue;
                if (target != null && obj.IsChildOf(target.transform))
                {
                    // Newly instantiated PDA elements can inherit the reserved layer.
                    // Return them to the UI layer when the dedicated camera stops.
                    originalLayers[obj] = 5;
                    continue;
                }
                if (cursor != null && obj.IsChildOf(cursor.transform)) continue;
                if (tooltip != null && obj.IsChildOf(tooltip.transform)) continue;
                log.LogWarning("PDA camera layer occupied by " + obj.name + ", parent=" + (obj.parent == null ? "none" : obj.parent.name) + ", PDA child=" + (target != null && obj.IsChildOf(target.transform)) + "; retrying.");
                return true;
            }
            return false;
        }
        // The selected layer has no scene objects. Cameras with broad masks must
        // temporarily exclude it so only the dedicated camera draws the PDA.
        private void ReserveCameraLayers()
        {
            if (layer < 0) return;
            int bit = 1 << layer;
            foreach (var camera in Camera.allCameras)
            {
                if (camera == null || camera == dedicated) continue;
                if (!cameraMasks.ContainsKey(camera)) cameraMasks.Add(camera, (camera.cullingMask & bit) != 0);
                else if ((camera.cullingMask & bit) != 0) cameraMasks[camera] = true;
                if ((camera.cullingMask & bit) != 0) camera.cullingMask &= ~bit;
            }
        }
        private void Restore()
        {
            if (dedicated != null)
            {
                dedicated.enabled = false;
                Destroy(dedicated.gameObject);
            }
            if (target != null && layer >= 0)
                foreach (var child in target.GetComponentsInChildren<Transform>(true))
                    if (child != null && child.gameObject.layer == layer && !originalLayers.ContainsKey(child))
                        originalLayers[child] = 5;
            foreach (var entry in originalLayers)
                if (entry.Key != null && entry.Key.gameObject.layer == layer) entry.Key.gameObject.layer = entry.Value;
            if (target != null) log.LogInfo("PDA-only camera stopped and original layers restored.");
            originalLayers.Clear();
            if (layer >= 0) foreach (var entry in cameraMasks)
                if (entry.Key != null && entry.Value) entry.Key.cullingMask |= 1 << layer;
            cameraMasks.Clear();
            dedicated = null;
            target = null;
            layer = -1;
        }

        private void OnDestroy() { Restore(); }
    }
}
