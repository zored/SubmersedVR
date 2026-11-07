using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ZoredPdaScale
{
    public sealed class PdaTouchFeedback : MonoBehaviour
    {
        private static PdaTouchFeedback active;
        private readonly TouchFeedbackRules rules = new TouchFeedbackRules();
        private static readonly Type rigType = AccessTools.TypeByName("SubmersedVR.VRCameraRig");
        private static readonly FieldInfo rigInstance = rigType == null ? null : AccessTools.Field(rigType, "instance");
        private static readonly FieldInfo rigPointer = rigType == null ? null : AccessTools.Field(rigType, "laserPointerUI");
        private static readonly FieldInfo cursorField = AccessTools.Field(typeof(FPSInputModule), "_cursorGraphic");
        private LineRenderer line;
        private Graphic cursor;
        private Renderer dot;
        private MaterialPropertyBlock oldLineBlock, oldDotBlock;
        private MaterialPropertyBlock lineBlock;
        private MaterialPropertyBlock dotBlock;
        private Color lineStart, lineEnd, cursorColor;
        private bool tinted;
        private int state, frame = -1;
        private FPSInputModule input;

        private void Awake() { lineBlock = new MaterialPropertyBlock(); dotBlock = new MaterialPropertyBlock(); active = this; }
        internal static void Observe(FPSInputModule inputModule, bool valid, float distance, bool pressed, bool held)
        {
            if (active == null) return;
            active.input = inputModule;
            active.frame = Time.frameCount;
            active.state = active.rules.State(valid, distance, pressed, Time.unscaledTime);
            if (valid && held) active.state = 2;
            if (pressed) Vibrate();
        }
        private static void Vibrate()
        {
            try
            {
                var type = AccessTools.TypeByName("SubmersedVR.HapticsVR");
                var method = type == null ? null : AccessTools.Method(type, "PlayHapticsNew");
                var controller = type == null ? null : type.GetNestedType("Controller", BindingFlags.Public | BindingFlags.NonPublic);
                if (method != null && controller != null)
                    method.Invoke(null, new object[] { Enum.Parse(controller, "Right"), 0f, .05f, 10f, .3f });
            }
            catch (Exception e) { Debug.LogWarning("Zored PDA touch vibration: " + e.Message); }
        }
        private void LateUpdate()
        {
            var player = Player.main;
            var pda = player == null ? null : player.GetPDA();
            if (frame < Time.frameCount - 1 || pda == null || !pda.isInUse || state == 0) { Restore(); return; }
            var rig = rigInstance == null ? null : rigInstance.GetValue(null);
            var pointer = rig == null || rigPointer == null ? null : rigPointer.GetValue(rig) as Component;
            var nextLine = pointer == null ? null : pointer.GetComponent<LineRenderer>();
            var dotField = pointer == null ? null : AccessTools.Field(pointer.GetType(), "pointerDot");
            var dotObject = dotField == null ? null : dotField.GetValue(pointer) as GameObject;
            var nextDot = dotObject == null ? null : dotObject.GetComponent<Renderer>();
            var nextCursor = input == null || cursorField == null ? null : cursorField.GetValue(input) as Graphic;
            if (tinted && (line != nextLine || cursor != nextCursor || dot != nextDot)) Restore();
            if (!tinted)
            {
                line = nextLine;
                cursor = nextCursor;
                dot = nextDot;
                if (line != null) { oldLineBlock = new MaterialPropertyBlock(); line.GetPropertyBlock(oldLineBlock); }
                if (dot != null) { oldDotBlock = new MaterialPropertyBlock(); dot.GetPropertyBlock(oldDotBlock); }
                if (line != null) { lineStart = line.startColor; lineEnd = line.endColor; }
                if (cursor != null) cursorColor = cursor.color;
                tinted = true;
            }
            var color = state == 2 ? new Color(.1f, 1f, .2f, 1f) : new Color(1f, .8f, .05f, 1f);
            if (line != null) { line.startColor = color; line.endColor = color; line.GetPropertyBlock(lineBlock); lineBlock.SetColor("_Color", Color.white); line.SetPropertyBlock(lineBlock); }
            if (dot != null) { dot.GetPropertyBlock(dotBlock); dotBlock.SetColor("_Color", color); dot.SetPropertyBlock(dotBlock); }
            if (cursor != null) cursor.color = new Color(color.r, color.g, color.b, cursorColor.a);
        }
        private void Restore()
        {
            if (!tinted) return;
            if (line != null) { line.startColor = lineStart; line.endColor = lineEnd; line.SetPropertyBlock(oldLineBlock); }
            if (dot != null) dot.SetPropertyBlock(oldDotBlock);
            if (cursor != null) cursor.color = cursorColor;
            line = null; cursor = null; dot = null; oldLineBlock = null; oldDotBlock = null; tinted = false;
        }
        private void OnDestroy() { Restore(); if (active == this) active = null; }
    }
}



