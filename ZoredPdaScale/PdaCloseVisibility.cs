using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace ZoredPdaScale
{
    // Hide only the tablet mesh while the game's normal closing sequence completes.
    public sealed class PdaCloseVisibility : MonoBehaviour
    {
        private static PdaCloseVisibility active;
        private Renderer hidden;
        private bool originalEnabled;
        private void Awake() { active = this; }
        internal static void OnClose(PDA pda)
        {
            if (active == null || pda == null || pda.state != PDA.State.Closing) return;
            var ui = Object.FindObjectOfType<uGUI_PDA>();
            var setVisible = AccessTools.Method(typeof(uGUI_PDA), "SetCanvasVisible");
            if (ui != null && setVisible != null) setVisible.Invoke(ui, new object[] { false });
            var mesh = pda.transform.Find("Mesh");
            var renderer = mesh == null ? null : mesh.GetComponent<Renderer>();
            if (renderer == null) return;
            active.Restore();
            active.hidden = renderer;
            active.originalEnabled = renderer.enabled;
            renderer.enabled = false;
        }
        internal static void OnOpen(bool opened) { if (opened && active != null) active.Restore(); }
        private void Restore()
        {
            if (hidden != null) hidden.enabled = originalEnabled;
            hidden = null;
        }
        private void OnDestroy() { Restore(); if (active == this) active = null; }
    }
    [HarmonyPatch(typeof(PDA), "Close")]
    internal static class PdaCloseVisibilityPatch
    {
        private static void Postfix(PDA __instance) { PdaCloseVisibility.OnClose(__instance); }
    }
    [HarmonyPatch(typeof(PDA), "Open")]
    internal static class PdaOpenVisibilityPatch
    {
        private static void Postfix(PDA __instance, bool __result) { PdaCloseVisibility.OnOpen(__result); if (__result) PdaOpeningPose.Begin(__instance); }
    }
}
