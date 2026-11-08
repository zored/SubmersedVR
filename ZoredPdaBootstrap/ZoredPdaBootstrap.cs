using System;
using System.Collections;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[BepInPlugin("zored.submersedvr.bootstrap", "Zored PDA Bootstrap", "1.0.0")]
[BepInDependency("com.bepis.bepinex.scriptengine")]
[BepInDependency("SubmersedVR")]
public sealed class ZoredPdaBootstrap : BaseUnityPlugin
{
    private Button button;
    private bool attempted;
    private void Update()
    {
        if (button == null) InstallButton();
        if (!attempted && Player.main != null && IngameMenu.main != null)
        {
            attempted = true;
            StartCoroutine(Reload());
        }
    }
    private void InstallButton()
    {
        var menu = IngameMenu.main;
        if (menu == null || menu.quitToMainMenuButton == null) return;
        var parent = menu.quitToMainMenuButton.transform.parent;
        if (parent == null || parent.childCount == 0) return;
        var template = parent.GetChild(0).GetComponent<Button>();
        if (template == null || template.GetComponentInChildren<TextMeshProUGUI>(true) == null) return;
        button = Instantiate(template, parent);
        button.name = "ZoredPdaActivate";
        button.gameObject.SetActive(true);
        button.interactable = true;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => StartCoroutine(Reload()));
        var label = button.GetComponentInChildren<TextMeshProUGUI>(true);
        label.SetText("Activate PDA mod");
        label.enableAutoSizing = true; label.fontSizeMin = 10f; label.fontSizeMax = 24f;
        var layout = button.GetComponent<LayoutElement>();
        if (layout == null) layout = button.gameObject.AddComponent<LayoutElement>();
        layout.ignoreLayout = true;
        var rect = button.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f); rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(170f, 42f); rect.anchoredPosition = new Vector2(20f, -184f);
        Logger.LogInfo("PDA activation button installed.");
    }
    private IEnumerator Reload()
    {
        yield return null;
        var type = AccessTools.TypeByName("ScriptEngine.ScriptEngine");
        PluginInfo info;
        var engine = BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("com.bepis.bepinex.scriptengine", out info) ? info.Instance : null;
        var reload = type == null ? null : AccessTools.Method(type, "ReloadPlugins");
        if (engine == null || reload == null) { Logger.LogWarning("PDA activation unavailable: ScriptEngine not ready."); yield break; }
        // ScriptEngine can outlive its manager across scene changes and leave
        // destroyed script instances registered in Chainloader. Never remove a live instance.
        foreach (var guid in new[] { "zored.submersedvr.pdascale", "zored.pda.reflectiondiag" })
        {
            PluginInfo registered;
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue(guid, out registered) &&
                (registered == null || registered.Instance == null))
            {
                BepInEx.Bootstrap.Chainloader.PluginInfos.Remove(guid);
                Logger.LogInfo("Removed destroyed script registration: " + guid);
            }
        }
        try { reload.Invoke(engine, null); Logger.LogInfo("PDA scripts activation requested."); }
        catch (Exception e) { Logger.LogWarning("PDA activation failed: " + e); }
    }
    private void OnDestroy() { if (button != null) Destroy(button.gameObject); }
}