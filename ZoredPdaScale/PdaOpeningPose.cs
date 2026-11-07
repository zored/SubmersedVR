using System.Collections;
using HarmonyLib;
using UnityEngine;

namespace ZoredPdaScale
{
    // Keep the temporary opening pose out of view, then expose the settled pose.
    public sealed class PdaOpeningPose : MonoBehaviour
    {
        private static PdaOpeningPose active;
        private float until, started;
        private Renderer hidden;
        private bool wasEnabled, revealPending;
        private CanvasGroup canvas;
        private float canvasAlpha;
        private static readonly System.Reflection.FieldInfo canvasField = AccessTools.Field(typeof(uGUI_PDA), "canvasGroup");
        private void Awake() { active = this; }
        internal static void Begin(PDA pda)
        {
            if (active == null || pda == null) return;
            active.StopAllCoroutines();
            active.RestoreVisibility();
            active.revealPending = false;
            active.started = Time.unscaledTime;
            active.until = active.started + 1f;
            var mesh = pda.transform.Find("Mesh");
            var renderer = mesh == null ? null : mesh.GetComponent<Renderer>();
            var ui = Object.FindObjectOfType<uGUI_PDA>();
            var group = ui == null || canvasField == null ? null : canvasField.GetValue(ui) as CanvasGroup;
            if (renderer == null || group == null) return;
            active.hidden = renderer;
            active.wasEnabled = renderer.enabled;
            active.canvas = group;
            active.canvasAlpha = group.alpha;
            renderer.enabled = false;
            group.alpha = 0f;
        }
        internal static void BeforeClose()
        {
            if (active == null) return;
            active.StopAllCoroutines();
            active.until = 0f;
            active.RestoreVisibility();
        }
        private void Update()
        {
            if (until <= 0f) return;
            var player = Player.main;
            var pda = player == null ? null : player.GetPDA();
            var animator = player == null ? null : player.playerAnimator;
            bool opening = pda != null && pda.isInUse && pda.state == PDA.State.Opening;
            bool skipped = false;
            if (opening && animator != null && animator.layerCount > 0 && animator.IsInTransition(0))
            {
                var clips = animator.GetNextAnimatorClipInfo(0);
                foreach (var info in clips)
                {
                    if (info.clip == null || info.clip.name != "player_pda_start") continue;
                    var state = animator.GetNextAnimatorStateInfo(0);
                    if (!animator.HasState(0, state.fullPathHash)) continue;
                    animator.Play(state.fullPathHash, 0, .98f);
                    skipped = true;
                    until = 0f;
                    Debug.Log("Zored PDA: skipped PDA opening pose transition");
                    break;
                }
            }
            if (!revealPending && PdaOpeningVisibilityRules.ShouldReveal(skipped, opening, Time.unscaledTime - started))
            {
                revealPending = true;
                StartCoroutine(RevealAfterFrame());
            }
            if (!opening || Time.unscaledTime > until) until = 0f;
        }
        private IEnumerator RevealAfterFrame()
        {
            // Animator, hand tracking, mesh and canvas must all finish this frame.
            yield return new WaitForEndOfFrame();
            RestoreVisibility();
        }
        private void RestoreVisibility()
        {
            if (hidden != null) hidden.enabled = wasEnabled;
            if (canvas != null) canvas.alpha = canvasAlpha;
            hidden = null; canvas = null;
        }
        private void OnDestroy() { RestoreVisibility(); if (active == this) active = null; }
    }
    [HarmonyPatch(typeof(PDA), "Close")]
    internal static class PdaOpeningVisibilityClose
    {
        private static void Prefix() { PdaOpeningPose.BeforeClose(); }
    }
}
