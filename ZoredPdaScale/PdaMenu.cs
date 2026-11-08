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
        private readonly ReloadChordRules reloadChord = new ReloadChordRules();

        private void Update()
        {
            var player = Player.main;
            var pda = player == null ? null : player.GetPDA();
            bool leftHeld = GameInput.GetButtonHeld(GameInput.Button.MoveDown);
            bool rightHeld = GameInput.GetButtonHeld(GameInput.Button.MoveUp);
            bool open = pda != null && pda.isInUse && pda.isOpen;
            if (reloadChord.Step(open, leftHeld, rightHeld, Time.unscaledTime))
            {
                Debug.Log("Zored PDA: L1 + R1 reload requested");
                StartCoroutine(ReloadScripts());
            }
            if (parent == null) TryInstall();
            if (buttons.Count >= 2 && Time.unscaledTime >= nextLabelUpdate)
            {
                nextLabelUpdate = Time.unscaledTime + 0.2f;
                var percent = Mathf.RoundToInt(Plugin.Scale.Value * 100f);
                SetSizeLabel(0, "PDA size -", percent);
                SetSizeLabel(1, "PDA size +", percent);
                if (buttons.Count >= 4)
                {
                    var shift = Mathf.RoundToInt(Plugin.TabletShift.Value * 100f);
                    SetSizeLabel(2, "PDA shift -", shift);
                    SetSizeLabel(3, "PDA shift +", shift);
                }
            }
        }

        private void SetSizeLabel(int index, string text, int percent)
        {
            if (buttons[index] != null)
                buttons[index].GetComponentInChildren<TextMeshProUGUI>(true).SetText(text + "  " + percent + "%");
        }
        private void TryInstall()
        {
            var menu = IngameMenu.main;
            if (menu == null || menu.quitToMainMenuButton == null) return;
            parent = menu.quitToMainMenuButton.transform.parent;
            if (parent == null || parent.childCount == 0) { parent = null; return; }
            template = parent.GetChild(0).GetComponent<Button>();
            if (template == null || template.GetComponentInChildren<TextMeshProUGUI>(true) == null) { parent = null; return; }

            Add("PDA size -", () => Plugin.AdjustPdaSize(false));
            Add("PDA size +", () => Plugin.AdjustPdaSize(true));
            Add("PDA shift -", () => Plugin.AdjustTabletShift(-.02f));
            Add("PDA shift +", () => Plugin.AdjustTabletShift(.02f));
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
