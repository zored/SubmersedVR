using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ZoredPdaScale
{
    // Unity's standard drag controls expect pixels, while SubmersedVR stores
    // world-space coordinates for the game's drag threshold. Change them only
    // for the duration of a touch-driven control callback.
    internal static class PdaTouchUiCoordinates
    {
        internal static void Before(PointerEventData eventData, out Vector2 previous)
        {
            previous = eventData.position;
            if (PdaTouchProbe.Dispatching && eventData.button == PointerEventData.InputButton.Left)
                eventData.position = PdaTouchProbe.TouchScreenPoint;
        }
        internal static void After(PointerEventData eventData, Vector2 previous) { eventData.position = previous; }
    }
    [HarmonyPatch(typeof(ScrollRect), "OnBeginDrag")]
    internal static class PdaTouchScrollBegin
    {
        private static void Prefix(PointerEventData eventData, out Vector2 __state) { PdaTouchUiCoordinates.Before(eventData, out __state); }
        private static void Postfix(PointerEventData eventData, Vector2 __state) { PdaTouchUiCoordinates.After(eventData, __state); }
    }
    [HarmonyPatch(typeof(ScrollRect), "OnDrag")]
    internal static class PdaTouchScrollDrag
    {
        private static void Prefix(PointerEventData eventData, out Vector2 __state) { PdaTouchUiCoordinates.Before(eventData, out __state); }
        private static void Postfix(PointerEventData eventData, Vector2 __state) { PdaTouchUiCoordinates.After(eventData, __state); }
    }
    [HarmonyPatch(typeof(Scrollbar), "OnDrag")]
    internal static class PdaTouchScrollbarDrag
    {
        private static void Prefix(PointerEventData eventData, out Vector2 __state) { PdaTouchUiCoordinates.Before(eventData, out __state); }
        private static void Postfix(PointerEventData eventData, Vector2 __state) { PdaTouchUiCoordinates.After(eventData, __state); }
    }
}
