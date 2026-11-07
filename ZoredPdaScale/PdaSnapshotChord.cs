using HarmonyLib;
using UnityEngine;

namespace ZoredPdaScale
{
    // While the open-PDA button is held, Options requests one snapshot instead of pause.
    [HarmonyPatch(typeof(GameInput), "GetButtonDown")]
    internal static class PdaSnapshotChord
    {
        private static int capturedFrame = -1;
        [HarmonyPostfix]
        private static void Postfix(GameInput.Button action, ref bool __result)
        {
            if (!__result || action != GameInput.Button.UIMenu || !GameInput.GetButtonHeld(GameInput.Button.PDA)) return;
            var player = Player.main;
            var pda = player == null ? null : player.GetPDA();
            if (pda == null || !pda.isInUse || !pda.isOpen) return;
            __result = false;
            if (capturedFrame == Time.frameCount) return;
            capturedFrame = Time.frameCount;
            var probe = Object.FindObjectOfType<DepthProbe>();
            if (probe != null) { Debug.Log("Zored PDA: Options + triangle snapshot"); probe.Snapshot(); }
        }
    }
}

