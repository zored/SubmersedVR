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
        private Harmony harmony;

        private void Awake()
        {
            Scale = Config.Bind("PDA", "Scale", 2.0f, "PDA size multiplier. F7/F8 adjust it while playing.");
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
        }
    }
}
