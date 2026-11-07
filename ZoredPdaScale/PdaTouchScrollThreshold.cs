using HarmonyLib;
using UnityEngine;

namespace ZoredPdaScale
{
    // Keep the original drag threshold for lasers and inventory items.
    [HarmonyPatch(typeof(FPSInputModule), "ShouldStartDrag")]
    internal static class PdaTouchScrollThreshold
    {
        private const float Distance = .005f;
        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        private static void Postfix(ref bool __result, Vector2 pressPos, Vector2 currentPos, bool useDragThreshold)
        {
            if (!PdaTouchProbe.Dispatching || !PdaFingerTouch.IsScrolling) return;
            __result = !useDragThreshold || (pressPos - currentPos).sqrMagnitude >= Distance * Distance;

        }
    }
}