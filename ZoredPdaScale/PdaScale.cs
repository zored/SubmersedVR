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
            HorizontalOffset = Config.Bind("PDA", "HorizontalOffset", 0.02f, "Horizontal UI offset in metres. Numpad 4/6 adjust it while playing.");
            TabletModelScale.Width = Config.Bind("PDA", "TabletWidth", 1.2f, "Additional tablet width multiplier. Numpad 1/3 adjust it while playing.");
            TabletModelScale.Scale = Config.Bind("PDA", "TabletModelScale", 2.0f, "Tablet model size multiplier. Numpad +/- adjust it while playing.");
            harmony = new Harmony("zored.submersedvr.pdascale");
            harmony.PatchAll();
            gameObject.AddComponent<TabletModelScale>();
            gameObject.AddComponent<DepthProbe>();
            gameObject.AddComponent<PdaSpatialExperiment>();
            Logger.LogInfo("PDA scaling enabled.");
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F7)) Scale.Value = Mathf.Clamp(Scale.Value - 0.1f, 0.5f, 2f);
            if (UnityEngine.Input.GetKeyDown(KeyCode.F8)) Scale.Value = Mathf.Clamp(Scale.Value + 0.1f, 0.5f, 2f);
            bool offsetChanged = false;
            if (UnityEngine.Input.GetKeyDown(KeyCode.Keypad4)) { HorizontalOffset.Value = Mathf.Clamp(HorizontalOffset.Value - 0.01f, -0.3f, 0.3f); offsetChanged = true; }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Keypad6)) { HorizontalOffset.Value = Mathf.Clamp(HorizontalOffset.Value + 0.01f, -0.3f, 0.3f); offsetChanged = true; }
            if (offsetChanged)
            {
                var pda = FindObjectOfType<uGUI_PDA>();
                if (pda != null) { var scaler = pda.GetComponent<uGUI_CanvasScaler>(); if (scaler != null) scaler.SetDirty(); }
                Logger.LogInfo("PDA horizontal offset: " + HorizontalOffset.Value.ToString("F2") + " m");
            }
        }

        private void OnDestroy()
        {
            harmony.UnpatchSelf();
        }
    }

    [HarmonyPatch(typeof(uGUI_CanvasScaler), "UpdateTransform")]
    internal static class PdaScalePatch
    {
        private static uGUI_CanvasScaler current;
        private static Vector3 originalScale;

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
