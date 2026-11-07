# PDA and PS VR2: current investigation

## Pointer

The user confirmed the pointer works after caching the fingertip pose after VRHands.LateUpdate and applying it for input. This keeps the input ray aligned with the rendered finger while moving the head. Direct finger-touch presses are not implemented and are reserved for a later chat.

## Pausing and controller binding

The user confirmed a short press of right PS VR2 Sense Options opens the pause menu; the log records UIMenu presses. SteamVR is configured for `bindings_playstation_vr2_zored_complex.json` in the repository using a user-specific CurrentURL setting. It uses the Sony driver's complex_button single mapping. A long press is reserved for VR recentering. Original SteamVR bindings remain intact. The live config points to the repository file; moving or deleting it breaks the user binding. Verify other controller actions if not yet confirmed.

## Focus and PDA freeze

Alt+Tab reproduced a stuck PDA even without F11. F10 showed `isInUse=True`, `isOpen=False`, `Time.timeScale=0`, a normal-speed animator stuck in transition, focused game window and no active UWE.FreezeTime reasons; its cachedTimeScale was 0. The exact cause is unproven. F9 restored time and the user confirmed PDA and movement recovered without restarting. F9 only acts on a focused game, PDA in use, zero timeScale and no active FreezeTime reason. For a permanent fix, instrument focus and freeze transitions, reproduce the failure and preserve legitimate pause behavior.

F11 has intermittently failed to find an unused layer after previously working, and can later succeed again. It is now requested at each launch and ScriptEngine reload, subject to the unused-layer check. Check log and current state rather than assuming it is enabled.

The user prefers interface Scale 2.0, TabletModelScale 1.9, TabletWidth 1.0 and HorizontalOffset approximately 0.04–0.05. These are persisted user settings, not values to overwrite blindly. F7/F8 adjust interface scale and force an update. F10 logging and F9 recovery are in the companion; the original SubmersedVR DLL is untouched.

## Latest snapshots and background setting

The user requested the PDA-only camera start automatically. The companion now requests it on load and F6; F11 still toggles it during the session. Startup remains subject to the unused-layer check, which has failed intermittently in this session. The user confirmed automatic startup after F6; the log later also recorded an unused-layer failure, so repeated openings remain to be verified.

Two F10 snapshots showed nearly unchanged ray origin/direction but hit object changed from Text to InputBlocker as the player turned their head. The visual and selected targets both move. The cause remains unverified; inspect the actual raycast camera and coordinate mapping. Snapshots are in BepInEx LogOutput.log for the 12:06 session, around lines 652 and 712, if that log still exists.

The user's actual gameplay includes VR while the Windows window is unfocused. F10 confirmed gameRunInBackground=False while unityRunInBackground=True. The companion now enables the game setting on load via General/RunInBackground (default true). If disabled, the companion leaves the game setting unchanged; turn that setting off in the game if desired. The user confirmed VR gameplay works while another Windows window has focus after enabling this setting. Long-term absence of the intermittent freeze remains to be established; F9 is still available for the guarded stuck state.

## Keyboard shortcuts while the game is in the background

ScriptEngine AutoReload/EnableFileSystemWatcher is enabled with a 3-second delay. DLL file changes triggered a successful automatic reload in the running game. The companion separately watches its own config and logged a successful reload after a file write. The pause menu now displays 13 direct buttons for settings and actions; the user confirmed they appear and the log recorded UI size +. Labels display adjustment steps relative to Scale 2.0, HorizontalOffset 0.05, TabletModelScale 1.9 and TabletWidth 1.0; appearance was confirmed but step changes have not been independently verified in the headset. F9 from the menu bypasses only the foreground requirement; it still requires an in-use PDA, zero timeScale and no FreezeTime reasons. Test other menu actions and layout as needed. Do not globally capture keyboard input.

## User interaction preferences

Give the next concrete steps without asking whether the user is ready. Do not suggest saving the game; never close it without the user directing the session. The user plans direct finger-touch activation in a later chat.

## Current handoff

The user wants the next chat to implement direct finger-touch activation of PDA buttons. Existing fingertip ray pointing works; physical touch activation does not exist yet. Keep existing SteamVR bindings and settings, build the separate companion, use automatic ScriptEngine reload, and validate both eyes and drag behavior. The F11 camera may temporarily find no unused layer and now retries while requested; do not claim it always activates. The new menu actions beyond UI size + still need headset checks.
