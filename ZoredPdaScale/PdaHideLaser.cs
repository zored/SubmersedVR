using HarmonyLib;
using UnityEngine;
using System.Reflection;
namespace ZoredPdaScale
{
    [HarmonyPatch]
    internal static class PdaHideLaser
    {
        private static Renderer hiddenDot;
        private static bool wasDotEnabled;
        private static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("SubmersedVR.LaserPointer");
            return type == null ? null : AccessTools.Method(type, "Update");
        }
        private static void Postfix(Component __instance)
        {
            var player = Player.main;
            var pda = player == null ? null : player.GetPDA();
            var menu = IngameMenu.main;
            var hide = __instance.name == "laserPointerUI" && pda != null && pda.isInUse && (menu == null || !menu.gameObject.activeInHierarchy);
            if (!hide) { if (__instance.name == "laserPointerUI") Restore(); return; }
            var line = __instance.GetComponent<LineRenderer>();
            if (line != null) line.enabled = false;
            var field = AccessTools.Field(__instance.GetType(), "pointerDot");
            var dotObject = field == null ? null : field.GetValue(__instance) as GameObject;
            var dot = dotObject == null ? null : dotObject.GetComponent<Renderer>();
            if (dot != hiddenDot) { Restore(); hiddenDot = dot; if (dot != null) wasDotEnabled = dot.enabled; }
            if (dot != null) dot.enabled = false;
        }
        internal static void Restore() { if (hiddenDot != null) hiddenDot.enabled = wasDotEnabled; hiddenDot = null; }
    }
}