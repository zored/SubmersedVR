using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZoredPdaScale
{
    // Compact controls alongside the pause menu; the existing menu stays intact.
    public sealed class PdaMenu : MonoBehaviour
    {
        private readonly List<Button> buttons = new List<Button>();
        private Transform parent;
        private Button template;
        private float nextLabelUpdate;

        private void Update()
        {
            if (parent == null) TryInstall();
            if (buttons.Count >= 8 && Time.unscaledTime >= nextLabelUpdate)
            {
                nextLabelUpdate = Time.unscaledTime + 0.2f;
                int ui = MenuControlRules.Steps(Plugin.Scale.Value, 2f, .1f);
                int offset = MenuControlRules.Steps(Plugin.HorizontalOffset.Value, .05f, .01f);
                int size = MenuControlRules.Steps(TabletModelScale.Scale.Value, 1.9f, .1f);
                int width = MenuControlRules.Steps(TabletModelScale.Width.Value, 1f, .1f);
                SetLabel(0, "UI size -", ui); SetLabel(1, "UI size +", ui);
                SetLabel(2, "UI left", offset); SetLabel(3, "UI right", offset);
                SetLabel(4, "Tablet size -", size); SetLabel(5, "Tablet size +", size);
                SetLabel(6, "Tablet width -", width); SetLabel(7, "Tablet width +", width);
            }
        }

        private void SetLabel(int index, string text, int value)
        {
            if (buttons[index] != null)
                buttons[index].GetComponentInChildren<TextMeshProUGUI>(true).SetText(text + "  " + (value > 0 ? "+" : "") + value);
        }

        private void TryInstall()
        {
            var menu = IngameMenu.main;
            if (menu == null || menu.quitToMainMenuButton == null) return;
            parent = menu.quitToMainMenuButton.transform.parent;
            if (parent == null || parent.childCount == 0) { parent = null; return; }
            template = parent.GetChild(0).GetComponent<Button>();
            if (template == null || template.GetComponentInChildren<TextMeshProUGUI>(true) == null) { parent = null; return; }

            Add("UI size -", () => Plugin.AdjustInterface(-0.1f));
            Add("UI size +", () => Plugin.AdjustInterface(0.1f));
            Add("UI left", () => Plugin.AdjustOffset(-0.01f));
            Add("UI right", () => Plugin.AdjustOffset(0.01f));
            Add("Tablet size -", () => TabletModelScale.Scale.Value = MenuControlRules.Adjust(TabletModelScale.Scale.Value, -0.1f, 0.5f, 3f));
            Add("Tablet size +", () => TabletModelScale.Scale.Value = MenuControlRules.Adjust(TabletModelScale.Scale.Value, 0.1f, 0.5f, 3f));
            Add("Tablet width -", () => TabletModelScale.Width.Value = MenuControlRules.Adjust(TabletModelScale.Width.Value, -0.1f, 0.5f, 3f));
            Add("Tablet width +", () => TabletModelScale.Width.Value = MenuControlRules.Adjust(TabletModelScale.Width.Value, 0.1f, 0.5f, 3f));
            Add("Reload (F6)", () => StartCoroutine(ReloadScripts()));
            Add("Recover (F9)", () => GetComponent<Plugin>().Recover(true));
            Add("Snapshot (F10)", () => GetComponent<DepthProbe>().Snapshot());
            Add("PDA camera (F11)", () => GetComponent<PdaSpatialExperiment>().Toggle());
            Add("Camera mode (F12)", () => GetComponent<PdaSpatialExperiment>().ToggleMode());
            Debug.Log("Zored PDA: added " + buttons.Count + " pause menu controls.");
        }

        private void Add(string text, UnityEngine.Events.UnityAction action)
        {
            int index = buttons.Count;
            var button = Instantiate(template, parent);
            button.name = "ZoredPdaButton" + index;
            button.gameObject.SetActive(true);
            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { Debug.Log("Zored PDA menu: " + text); action(); });
            var label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            label.SetText(text);
            label.enableAutoSizing = true;
            label.fontSizeMin = 10f;
            label.fontSizeMax = 24f;
            var layout = button.GetComponent<LayoutElement>();
            if (layout == null) layout = button.gameObject.AddComponent<LayoutElement>();
            layout.ignoreLayout = true;
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(170f, 42f);
            rect.anchoredPosition = new Vector2(20f + (index % 2) * 178f, -(index / 2) * 46f);
            buttons.Add(button);
        }

        private IEnumerator ReloadScripts()
        {
            yield return null;
            var type = AccessTools.TypeByName("ScriptEngine.ScriptEngine");
            BepInEx.PluginInfo info;
            var engine = BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("com.bepis.bepinex.scriptengine", out info) ? info.Instance : null;
            var reload = type == null ? null : AccessTools.Method(type, "ReloadPlugins");
            if (engine == null || reload == null) { Debug.LogWarning("Zored PDA: ScriptEngine reload unavailable."); yield break; }
            try { reload.Invoke(engine, null); }
            catch (Exception e) { Debug.LogWarning("Zored PDA: ScriptEngine reload failed: " + e.Message); }
        }

        private void OnDestroy()
        {
            foreach (var button in buttons) if (button != null) Destroy(button.gameObject);
            buttons.Clear();
        }
    }
}
