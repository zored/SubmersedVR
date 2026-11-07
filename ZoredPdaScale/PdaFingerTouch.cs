using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ZoredPdaScale
{
    // Feed press and release into the same input pipeline used by the laser.
    [HarmonyPatch(typeof(FPSInputModule), "UpdateMouseState")]
    [HarmonyAfter("SubmersedVR")]
    internal static class PdaFingerTouch
    {
        private static readonly FingerTouchRules touch = new FingerTouchRules();
        private static readonly System.Reflection.FieldInfo mouseState = AccessTools.Field(typeof(FPSInputModule), "m_MouseState");
        private static Vector2 previousPosition;
        private static PointerEventData heldData;
        internal static bool IsScrolling
        {
            get
            {
                var target = heldData == null ? null : heldData.pointerDrag;
                return target != null && (target.GetComponent<ScrollRect>() != null || target.GetComponent<Scrollbar>() != null);
            }
        }
        private static void Postfix(FPSInputModule __instance, PointerEventData leftData)
        {
            var pda = Player.main == null ? null : Player.main.GetPDA();
            RaycastResult hit = default(RaycastResult);
            float distance = 0f;
            var valid = pda != null && pda.isInUse && PdaTouchProbe.TryGet(touch.Held, out hit, out distance);
            if (!valid) { hit = default(RaycastResult); distance = 0f; }
            if (mouseState == null) return;
            var mouse = mouseState.GetValue(__instance);
            if (mouse == null) return;
            var getButton = AccessTools.Method(mouse.GetType(), "GetButtonState");
            var setButton = AccessTools.Method(mouse.GetType(), "SetButtonState");
            if (getButton == null || setButton == null) return;
            var button = getButton.Invoke(mouse, new object[] { PointerEventData.InputButton.Left });
            var eventData = AccessTools.Property(button.GetType(), "eventData").GetValue(button, null);
            var state = (PointerEventData.FramePressState)AccessTools.Field(eventData.GetType(), "buttonState").GetValue(eventData);
            var physical = state != PointerEventData.FramePressState.NotChanged ||
                GameInput.GetButtonHeld(GameInput.button0) || (!touch.Held && (leftData.pointerPress != null || leftData.dragging));
            var wasHeld = touch.Held;
            var action = touch.Step(valid, distance, physical);
            PdaTouchFeedback.Observe(__instance, valid, distance, action == TouchEvent.Press, touch.Held);
            PdaRefraction.ObserveTouch(action == TouchEvent.Press, touch.Held && valid);
            if (touch.Held) heldData = leftData;
            else if (action == TouchEvent.Release || physical) heldData = null;
            if (physical) return;
            if (action == TouchEvent.Press || wasHeld) PdaTouchProbe.BeginDispatch();
            if (action == TouchEvent.Press || wasHeld)
            {
                // Use the probe hit while touching, including its world coordinates,
                // so the existing FPSInputModule drag threshold sees finger movement.
                leftData.pointerCurrentRaycast = hit;
                if (valid)
                {
                    Vector2 position = hit.worldPosition;
                    leftData.delta = action == TouchEvent.Press ? Vector2.zero : position - previousPosition;
                    leftData.position = position;
                    previousPosition = position;
                }
            }
            if (action == TouchEvent.Press || action == TouchEvent.Release)
            {
                Debug.Log("Zored PDA touch " + (action == TouchEvent.Press ? "down" : "up") + ": " + (hit.gameObject == null ? "none" : hit.gameObject.name) + ", distance=" + distance.ToString("F4"));
                if (action == TouchEvent.Press && hit.gameObject != null)
                {
                    var targetTransform = hit.gameObject.transform;
                    string path = targetTransform.name + "[" + targetTransform.GetSiblingIndex() + "]";
                    for (var parent = targetTransform.parent; parent != null; parent = parent.parent)
                        path = parent.name + "[" + parent.GetSiblingIndex() + "]/" + path;
                    var rect = targetTransform as RectTransform;
                    var ui = hit.gameObject.GetComponentInParent<uGUI_PDA>();
                    var root = SNCameraRoot.main;
                    Vector3 tip;
                    if (PdaFingerPointer.TryGetTip(out tip))
                    {
                        var mesh = pda == null ? null : pda.transform.Find("Mesh");
                        var worldTip = root == null ? tip : tip + root.transform.position;
                        var localTip = mesh == null ? Vector3.zero : mesh.InverseTransformPoint(worldTip);
                        var uiTip = ui == null ? Vector3.zero : ui.transform.InverseTransformPoint(tip);
                        var hitLocal = rect == null ? Vector3.zero : rect.InverseTransformPoint(hit.worldPosition);
                        Debug.Log("Zored PDA contact measurement: time=" + Time.unscaledTime.ToString("F4") + ", path=" + path + ", tipUI=" + tip.ToString("F5") + ", hitUI=" + hit.worldPosition.ToString("F5") + ", uiTip=" + uiTip.ToString("F5") + ", tabletTip=" + localTip.ToString("F5") + ", targetLocal=" + hitLocal.ToString("F5") + ", targetRect=" + (rect == null ? "none" : rect.rect.ToString()) + ", distance=" + distance.ToString("F5"));
                    }
                }
                setButton.Invoke(mouse, new object[] { PointerEventData.InputButton.Left,
                    action == TouchEvent.Press ? PointerEventData.FramePressState.Pressed : PointerEventData.FramePressState.Released, leftData });
            }
        }
        internal static void Cancel()
        {
            if (!touch.Held) return;
            touch.Cancel();
            var data = heldData;
            heldData = null;
            if (data == null) return;
            if (data.pointerPress != null) ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerUpHandler);
            if (data.dragging && data.pointerDrag != null) ExecuteEvents.Execute(data.pointerDrag, data, ExecuteEvents.endDragHandler);
            data.eligibleForClick = false;
            data.pointerPress = null;
            data.rawPointerPress = null;
            data.dragging = false;
            data.pointerDrag = null;
        }
    }

    [HarmonyPatch(typeof(PDA), "Close")]
    internal static class PdaFingerTouchClose
    {
        private static void Postfix(PDA __instance) { if (__instance.state == PDA.State.Closing) PdaFingerTouch.Cancel(); }
    }
}
