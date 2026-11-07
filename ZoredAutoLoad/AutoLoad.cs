using System;
using System.Collections;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

[BepInPlugin("zored.subnautica.autoload", "Zored Auto Load", "1.0.0")]
public class AutoLoad : BaseUnityPlugin {
    private static AutoLoad instance;
    private ConfigEntry<bool> enabledSetting;
    private bool attempted;
    private Harmony harmony;

    private void Awake() {
        instance = this;
        enabledSetting = Config.Bind("General", "Enabled", true, "Load the most recent save once on a fresh game launch.");
        harmony = new Harmony("zored.subnautica.autoload");
        var iterator = AccessTools.Inner(typeof(StartScreen), "<LoadMainMenu>d__36");
        var target = iterator == null ? null : AccessTools.Method(iterator, "MoveNext");
        if (target == null) { Logger.LogError("Startup hook unavailable; automatic loading disabled."); return; }
        harmony.Patch(target, postfix: new HarmonyMethod(typeof(AutoLoad), "AfterMenu"));
        Logger.LogInfo("Automatic loading initialized; enabled=" + enabledSetting.Value);
    }

    private static void AfterMenu(bool __result) {
        if (!__result && instance != null && !instance.attempted) {
            instance.attempted = true;
            if (instance.enabledSetting.Value) instance.StartCoroutine(instance.LoadAfterMenu());
        }
    }

    private IEnumerator LoadAfterMenu() {
        yield return null;
        var manager = SaveLoadManager.main;
        var menu = uGUI_MainMenu.main;
        if (manager == null || menu == null) { Logger.LogWarning("Menu or save manager unavailable; leaving menu open."); yield break; }
        var slots = manager.GetActiveSlotNames();
        var info = new SaveLoadManager.GameInfo[slots.Length];
        var dates = new long[slots.Length];
        var corrupt = new bool[slots.Length];
        for (int i = 0; i < slots.Length; i++) {
            info[i] = manager.GetGameInfo(slots[i]);
            if (info[i] != null) { dates[i] = info[i].dateTicks; corrupt[i] = info[i].corrupted; }
        }
        int selected = Selection.Choose(dates, corrupt);
        if (selected < 0) { Logger.LogWarning("No usable most recent save; leaving menu open."); yield break; }
        var save = info[selected];
        Logger.LogInfo("Loading most recent save: " + slots[selected]);
        UWE.CoroutineHost.StartCoroutine(menu.LoadGameAsync(slots[selected], save.session, save.changeSet, save.gameMode));
    }

    private void OnDestroy() { if (harmony != null) harmony.UnpatchSelf(); if (instance == this) instance = null; }
}
