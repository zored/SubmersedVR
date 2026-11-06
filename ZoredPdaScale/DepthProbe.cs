using BepInEx.Logging;
using UnityEngine;
using HarmonyLib;

namespace ZoredPdaScale
{
    // Diagnostic experiment: compare the GUI camera with and without a depth clear.
    public sealed class DepthProbe : MonoBehaviour
    {
        private readonly ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("Zored PDA Depth");
        private bool enabledExperiment;
        private Camera modifiedCamera;
        private CameraClearFlags previousFlags;

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F10))
            {
                enabledExperiment = !enabledExperiment;
                log.LogInfo("Depth experiment " + (enabledExperiment ? "enabled" : "disabled"));
            }

            var root = SNCameraRoot.main;
            var player = Player.main;
            var pda = player == null ? null : player.GetPDA();
            var camera = root == null ? null : AccessTools.Field(typeof(SNCameraRoot), "guiCamera").GetValue(root) as Camera;
            bool apply = enabledExperiment && pda != null && pda.isInUse && camera != null;

            if (modifiedCamera != null && (!apply || modifiedCamera != camera)) Restore();
            if (apply && modifiedCamera == null)
            {
                modifiedCamera = camera;
                previousFlags = camera.clearFlags;
                log.LogInfo("GUI camera=" + camera.name + ", clear=" + previousFlags + ", depth=" + camera.depth + ", mask=" + camera.cullingMask + "; testing Nothing");
                camera.clearFlags = CameraClearFlags.Nothing;
            }
        }

        private void Restore()
        {
            if (modifiedCamera != null)
            {
                modifiedCamera.clearFlags = previousFlags;
                log.LogInfo("Restored GUI camera clear mode to " + previousFlags);
            }
            modifiedCamera = null;
        }

        private void OnDestroy() { Restore(); }
    }
}