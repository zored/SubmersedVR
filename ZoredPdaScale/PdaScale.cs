using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace ZoredPdaScale
{
    [BepInPlugin("zored.submersedvr.pdascale", "Zored PDA Scale", "0.1.0")]
    [BepInDependency("SubmersedVR")]
    public sealed class Plugin : BaseUnityPlugin
    {
        internal static ConfigEntry<bool> ChestGesture;
        internal static ConfigEntry<float> Scale;
        internal static ConfigEntry<float> HorizontalOffset;
        internal static ConfigEntry<float> TabletShift;
        private Harmony harmony;
        private System.IO.FileSystemWatcher configWatcher;
        private int configChanged;
        private float configReloadAt = -1f;

        private void Awake()
        {
            StartCoroutine(InitializeWhenReady());
        }

        private System.Collections.IEnumerator InitializeWhenReady()
        {
            BepInEx.PluginInfo dependency;
            while (!BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("SubmersedVR", out dependency) ||
                   dependency == null || dependency.Instance == null ||
                   AccessTools.TypeByName("SubmersedVR.VRHands") == null)
                yield return null;
            Initialize();
        }

        private void Initialize()
        {
            ChestGesture = Config.Bind("PDA", "ChestGesture", true, "Open the PDA with left grip below and to the left of the head.");
            Scale = Config.Bind("PDA", "Scale", 2.205f, "PDA size multiplier. F7/F8 adjust it while playing.");
            HorizontalOffset = Config.Bind("PDA", "HorizontalOffset", 0.05f, "Horizontal UI offset in metres. J/L adjust it while the PDA is open.");
            TabletShift = Config.Bind("PDA", "TabletShift", 0.24f, "Shift tablet and UI right as a fraction of tablet width.");
            TabletModelScale.Width = Config.Bind("PDA", "TabletWidth", 1.0f, "Additional tablet width multiplier. U/O adjust it while the PDA is open.");
            TabletModelScale.Scale = Config.Bind("PDA", "TabletModelScale", 2.09475f, "Tablet model size multiplier. K/I adjust it while the PDA is open.");
            var runInBackground = Config.Bind("General", "RunInBackground", true, "When enabled, turn on the game setting for background play on load. When disabled, leave the game setting unchanged; turn it off in game settings if needed.");
            if (runInBackground.Value)
            {
                MiscSettings.runInBackground = true;
                ApplicationFocus.OnRunInBackgroundChanged();
                Logger.LogInfo("Background play enabled via companion.");
            }
            harmony = new Harmony("zored.submersedvr.pdascale");
            harmony.PatchAll();
            StartConfiguredScriptWatcher();
            StartConfigWatcher();
            gameObject.AddComponent<TabletModelScale>();
            gameObject.AddComponent<DepthProbe>();
            gameObject.AddComponent<PdaSpatialExperiment>();
            gameObject.AddComponent<PdaFingerPointer>();
            gameObject.AddComponent<ToolRecall>();
            gameObject.AddComponent<PdaTouchFeedback>();
            gameObject.AddComponent<PdaCloseVisibility>();
            gameObject.AddComponent<PdaOpeningPose>();
            gameObject.AddComponent<PdaRefraction>();
            gameObject.AddComponent<PdaMenu>();
            Logger.LogInfo("PDA scaling enabled.");
        }

        private void StartConfiguredScriptWatcher()
        {
            try
            {
                var type = AccessTools.TypeByName("ScriptEngine.ScriptEngine");
                BepInEx.PluginInfo info;
                var engine = BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("com.bepis.bepinex.scriptengine", out info) ? info.Instance : null;
                var config = type == null ? null : AccessTools.Property(type, "EnableFileSystemWatcher");
                var watcher = type == null ? null : AccessTools.Field(type, "fileSystemWatcher");
                var start = type == null ? null : AccessTools.Method(type, "StartFileSystemWatcher");
                if (engine == null || config == null || watcher == null || start == null)
                {
                    Logger.LogWarning("ScriptEngine watcher unavailable: engine=" + (engine != null) + ", type=" + (type != null) + ", property=" + (config != null) + ", field=" + (watcher != null) + ", method=" + (start != null));
                    return;
                }
                engine.Config.Reload();
                var enabled = config.GetValue(engine, null) as ConfigEntry<bool>;
                if (enabled != null && MenuControlRules.ShouldStartWatcher(enabled.Value, watcher.GetValue(engine) != null))
                {
                    start.Invoke(engine, null);
                    Logger.LogInfo("ScriptEngine automatic reload watcher started.");
                }
                else Logger.LogInfo("ScriptEngine watcher: enabled=" + (enabled != null && enabled.Value) + ", running=" + (watcher.GetValue(engine) != null));
            }
            catch (System.Exception e) { Logger.LogWarning("ScriptEngine watcher setup failed: " + e.Message); }
        }

        private void Update()
        {
            if (System.Threading.Interlocked.Exchange(ref configChanged, 0) != 0) configReloadAt = Time.unscaledTime + 1f;
            if (configReloadAt >= 0f && Time.unscaledTime >= configReloadAt)
            {
                configReloadAt = -1f;
                try { Config.Reload(); RefreshPda(); Logger.LogInfo("PDA configuration reloaded after file change."); }
                catch (System.Exception e) { Logger.LogWarning("PDA configuration reload failed: " + e.Message); }
            }
            if (GameInput.GetButtonDown(GameInput.Button.UIMenu))
                Logger.LogInfo("Pause diagnostic: UIMenu pressed; menu=" + (IngameMenu.main == null ? "none" : IngameMenu.main.gameObject.activeInHierarchy.ToString()));
            if (UnityEngine.Input.GetKeyDown(KeyCode.F9)) Recover(false);
            bool scaleChanged = false;
            if (UnityEngine.Input.GetKeyDown(KeyCode.F7)) { Scale.Value = MenuControlRules.Adjust(Scale.Value, -0.1f, 0.5f, 3f); scaleChanged = true; }
            if (UnityEngine.Input.GetKeyDown(KeyCode.F8)) { Scale.Value = MenuControlRules.Adjust(Scale.Value, 0.1f, 0.5f, 3f); scaleChanged = true; }
            bool offsetChanged = false;
            if (AdjustmentKey(KeyCode.J)) { HorizontalOffset.Value = Mathf.Clamp(HorizontalOffset.Value - 0.01f, -0.3f, 0.3f); offsetChanged = true; }
            if (AdjustmentKey(KeyCode.L)) { HorizontalOffset.Value = Mathf.Clamp(HorizontalOffset.Value + 0.01f, -0.3f, 0.3f); offsetChanged = true; }
            if (offsetChanged || scaleChanged)
            {
                var pda = FindObjectOfType<uGUI_PDA>();
                if (pda != null) { var scaler = pda.GetComponent<uGUI_CanvasScaler>(); if (scaler != null) scaler.SetDirty(); }
                if (offsetChanged) Logger.LogInfo("PDA horizontal offset: " + HorizontalOffset.Value.ToString("F2") + " m");
                if (scaleChanged) Logger.LogInfo("PDA interface scale: " + Scale.Value.ToString("F1"));
            }
        }

        private void StartConfigWatcher()
        {
            try
            {
                var path = Config.ConfigFilePath;
                configWatcher = new System.IO.FileSystemWatcher(System.IO.Path.GetDirectoryName(path), System.IO.Path.GetFileName(path));
                configWatcher.NotifyFilter = System.IO.NotifyFilters.LastWrite | System.IO.NotifyFilters.FileName | System.IO.NotifyFilters.Size;
                configWatcher.Changed += ConfigFileChanged;
                configWatcher.Created += ConfigFileChanged;
                configWatcher.Renamed += ConfigFileRenamed;
                configWatcher.EnableRaisingEvents = true;
                Logger.LogInfo("PDA configuration watcher started.");
            }
            catch (System.Exception e) { Logger.LogWarning("PDA configuration watcher failed: " + e.Message); }
        }
        private void ConfigFileChanged(object sender, System.IO.FileSystemEventArgs e) { System.Threading.Interlocked.Exchange(ref configChanged, 1); }
        private void ConfigFileRenamed(object sender, System.IO.RenamedEventArgs e) { System.Threading.Interlocked.Exchange(ref configChanged, 1); }

        internal void Recover(bool explicitMenu)
        {
            var player = Player.main;
            var pda = player == null ? null : player.GetPDA();
            if (MenuControlRules.MayRecover(pda != null && pda.isInUse, Application.isFocused, explicitMenu, Time.timeScale, UWE.FreezeTime.HasFreezers()))
            {
                Time.timeScale = 1f;
                Logger.LogWarning("Recovery restored game time after a PDA freeze with no active pause reasons.");
            }
            else Logger.LogInfo("Recovery skipped: PDA, focus, time scale or active pause conditions do not match.");
        }

        internal static void AdjustPdaSize(bool increase)
        {
            float ui, tablet;
            MenuControlRules.AdjustPdaSize(Scale.Value, TabletModelScale.Scale.Value, increase, out ui, out tablet);
            Scale.Value = ui;
            TabletModelScale.Scale.Value = tablet;
            RefreshPda();
        }
        internal static void AdjustInterface(float delta)
        {
            Scale.Value = MenuControlRules.Adjust(Scale.Value, delta, 0.5f, 3f);
            RefreshPda();
        }
        internal static void AdjustOffset(float delta)
        {
            HorizontalOffset.Value = MenuControlRules.Adjust(HorizontalOffset.Value, delta, -0.3f, 0.3f);
            RefreshPda();
        }
        internal static void AdjustTabletShift(float delta)
        {
            TabletShift.Value = MenuControlRules.Adjust(TabletShift.Value, delta, -.5f, .5f);
            RefreshPda();
        }
        private static void RefreshPda()
        {
            var pda = FindObjectOfType<uGUI_PDA>();
            if (pda != null) { var scaler = pda.GetComponent<uGUI_CanvasScaler>(); if (scaler != null) scaler.SetDirty(); }
        }

        internal static bool AdjustmentKey(KeyCode key)
        {
            var player = Player.main;
            var pda = player == null ? null : player.GetPDA();
            return pda != null && pda.isInUse && UnityEngine.Input.GetKeyDown(key);
        }
        private void OnDestroy()
        {
            if (configWatcher != null) { configWatcher.EnableRaisingEvents = false; configWatcher.Dispose(); configWatcher = null; }
            PdaHideLaser.Restore();
            PdaFingerTouch.Cancel();
            PdaTouchProbe.Dispose();
            if (harmony != null) harmony.UnpatchSelf();
            PdaScalePatch.Restore();
        }
    }

    [HarmonyPatch(typeof(uGUI_CanvasScaler), "UpdateTransform")]
    internal static class PdaScalePatch
    {
        private static uGUI_CanvasScaler current;
        private static Vector3 originalScale;

        internal static void Restore()
        {
            if (current != null)
            {
                current.transform.localScale = originalScale;
                current.SetDirty();
                current = null;
            }
        }

        [HarmonyPostfix]
        [HarmonyAfter("SubmersedVR")]
        private static void Postfix(uGUI_CanvasScaler __instance)
        {
            if (__instance.GetComponent<uGUI_PDA>() == null) return;
            if (current != __instance)
            {
                current = __instance;
                originalScale = __instance.transform.localScale;
            }
            __instance.transform.localScale = originalScale * Mathf.Clamp(Plugin.Scale.Value, 0.5f, 3f);
            // SubmersedVR positions the canvas in UI space before this postfix.
            __instance.transform.position += __instance.transform.right * (Mathf.Clamp(Plugin.HorizontalOffset.Value, -0.3f, 0.3f) + TabletModelScale.CurrentShift);
        }
    }
}


