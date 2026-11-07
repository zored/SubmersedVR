using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace ZoredPdaScale
{
    [BepInPlugin("zored.submersedvr.pdascale", "Zored PDA Scale", "0.1.0")]
    [BepInDependency("SubmersedVR")]
    public sealed class Plugin : BaseUnityPlugin
    {
        internal static ConfigEntry<float> Scale;
        internal static ConfigEntry<float> HorizontalOffset;
        private Harmony harmony;

        private void Awake()
        {
            Scale = Config.Bind("PDA", "Scale", 2.0f, "PDA size multiplier. F7/F8 adjust it while playing.");
            HorizontalOffset = Config.Bind("PDA", "HorizontalOffset", 0.05f, "Horizontal UI offset in metres. J/L adjust it while the PDA is open.");
            TabletModelScale.Width = Config.Bind("PDA", "TabletWidth", 1.0f, "Additional tablet width multiplier. U/O adjust it while the PDA is open.");
            TabletModelScale.Scale = Config.Bind("PDA", "TabletModelScale", 1.9f, "Tablet model size multiplier. K/I adjust it while the PDA is open.");
            var runInBackground = Config.Bind("General", "RunInBackground", true, "When enabled, turn on the game setting for background play on load. When disabled, leave the game setting unchanged; turn it off in game settings if needed.");
            if (runInBackground.Value)
            {
                MiscSettings.runInBackground = true;
                ApplicationFocus.OnRunInBackgroundChanged();
                Logger.LogInfo("Background play enabled via companion.");
            }
            harmony = new Harmony("zored.submersedvr.pdascale");
            harmony.PatchAll();
            gameObject.AddComponent<TabletModelScale>();
            gameObject.AddComponent<DepthProbe>();
            gameObject.AddComponent<PdaSpatialExperiment>();
            gameObject.AddComponent<PdaFingerPointer>();
            Logger.LogInfo("PDA scaling enabled.");
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F9))
            {
                var player = Player.main;
                var pda = player == null ? null : player.GetPDA();
                if (pda != null && pda.isInUse && Application.isFocused && Time.timeScale == 0f && !UWE.FreezeTime.HasFreezers())
                {
                    Time.timeScale = 1f;
                    Logger.LogWarning("F9 restored game time after a PDA freeze with no active pause reasons.");
                }
                else Logger.LogInfo("F9 recovery skipped: PDA, focus, time scale or active pause conditions do not match.");
            }
            bool scaleChanged = false;
            if (UnityEngine.Input.GetKeyDown(KeyCode.F7)) { Scale.Value = Mathf.Clamp(Scale.Value - 0.1f, 0.5f, 2f); scaleChanged = true; }
            if (UnityEngine.Input.GetKeyDown(KeyCode.F8)) { Scale.Value = Mathf.Clamp(Scale.Value + 0.1f, 0.5f, 2f); scaleChanged = true; }
            bool offsetChanged = false;
            if (AdjustmentKey(KeyCode.J)) { HorizontalOffset.Value = Mathf.Clamp(HorizontalOffset.Value - 0.01f, -0.3f, 0.3f); offsetChanged = true; }
            if (AdjustmentKey(KeyCode.L)) { HorizontalOffset.Value = Mathf.Clamp(HorizontalOffset.Value + 0.01f, -0.3f, 0.3f); offsetChanged = true; }
            if (offsetChanged || scaleChanged)
            {
                var pda = FindObjectOfType<uGUI_PDA>();
                if (pda != null) { var scaler = pda.GetComponent<uGUI_CanvasScaler>(); if (scaler != null) scaler.SetDirty(); }
                if (offsetChanged) Logger.LogInfo("PDA horizontal offset: " + HorizontalOffset.Value.ToString("F2") + " m");
                if (scaleChanged) Logger.LogInfo("PDA interface scale: " + Scale.Value.ToString("F1"));
            }
        }

        internal static bool AdjustmentKey(KeyCode key)
        {
            var player = Player.main;
            var pda = player == null ? null : player.GetPDA();
            return pda != null && pda.isInUse && UnityEngine.Input.GetKeyDown(key);
        }
        private void OnDestroy()
        {
            harmony.UnpatchSelf();
            PdaScalePatch.Restore();
        }
    }

    [HarmonyPatch(typeof(uGUI_CanvasScaler), "UpdateTransform")]
    internal static class PdaScalePatch
    {
        private static uGUI_CanvasScaler current;
        private static Vector3 originalScale;

        internal static void Restore()
        {
            if (current != null)
            {
                current.transform.localScale = originalScale;
                current.SetDirty();
                current = null;
            }
        }

        [HarmonyPostfix]
        [HarmonyAfter("SubmersedVR")]
        private static void Postfix(uGUI_CanvasScaler __instance)
        {
            if (__instance.GetComponent<uGUI_PDA>() == null) return;
            if (current != __instance)
            {
                current = __instance;
                originalScale = __instance.transform.localScale;
            }
            __instance.transform.localScale = originalScale * Mathf.Clamp(Plugin.Scale.Value, 0.5f, 2f);
            // SubmersedVR positions the canvas in UI space before this postfix.
            __instance.transform.position += __instance.transform.right * Mathf.Clamp(Plugin.HorizontalOffset.Value, -0.3f, 0.3f);
        }
    }
}
