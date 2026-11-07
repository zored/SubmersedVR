# PDA and PS VR2: current investigation

## Pointer

The companion moves the UI ray origin to the right index fingertip while the PDA is in use, keeping the controller's existing aim direction. The user confirmed that the visible laser originates correctly, but the visible cursor is too low and both the cursor and selected button move when the head moves while the hand stays still. Direct finger presses without dragging are a future goal, not implemented.

Build `a29613f` adds F10 logs for pointer origin and direction, last UI raycast hit, camera root and PDA UI transform. It has been built and placed in `BepInEx\scripts`, but has not yet been loaded or tested in the headset. Next: with the game focused, close the PDA, press F6, open it, aim at a button, press F10, turn the head without moving the hand, and press F10 again. Compare `Pointer origin`, `Pointer hit` and `PDA UI transform` in the game log. F11 resets off after F6; if needed, enable it before both snapshots. Avoid switching Windows focus during the test.

## Pausing and controller binding

SteamVR's vrserver log for this Subnautica session confirms it loads the Oculus Touch default and auto-remaps it to PS VR2 Sense. Sony's local driver maps Oculus left `application_menu` to right `options`; right `a` and `b` map to Cross and Circle, already occupied. A separate `bindings_playstation_vr2_zored.json` was prepared with Options assigned to UIMenu, preserving original files, but has not been selected in SteamVR or tested. The user confirmed Options did not work before activation. Safely activate or create a user-specific binding and verify other actions; do not claim the pause button works without a headset check.

## Focus and PDA freeze

Alt+Tab reproduced a stuck PDA even without F11. F10 showed `isInUse=True`, `isOpen=False`, `Time.timeScale=0`, a normal-speed animator stuck in transition, focused game window and no active UWE.FreezeTime reasons; its cachedTimeScale was 0. The exact cause is unproven. F9 restored time and the user confirmed PDA and movement recovered without restarting. F9 only acts on a focused game, PDA in use, zero timeScale and no active FreezeTime reason. For a permanent fix, instrument focus and freeze transitions, reproduce the failure and preserve legitimate pause behavior.

F11 has intermittently failed to find an unused layer after previously working, and can later succeed again. It defaults off at each launch and ScriptEngine reload. Check log and current state rather than assuming it is enabled.

The user prefers interface Scale 2.0, TabletModelScale 1.9, TabletWidth 1.0 and HorizontalOffset approximately 0.04–0.05. These are persisted user settings, not values to overwrite blindly. F7/F8 adjust interface scale and force an update. F10 logging and F9 recovery are in the companion; the original SubmersedVR DLL is untouched.

## Latest snapshots and background setting

Two F10 snapshots showed nearly unchanged ray origin/direction but hit object changed from Text to InputBlocker as the player turned their head. The visual and selected targets both move. The cause remains unverified; inspect the actual raycast camera and coordinate mapping. Snapshots are in BepInEx LogOutput.log for the 12:06 session, around lines 652 and 712, if that log still exists.

The user's actual gameplay includes VR while the Windows window is unfocused. F10 confirmed gameRunInBackground=False while unityRunInBackground=True. The companion now enables the game setting on load via General/RunInBackground (default true). If disabled, the companion leaves the game setting unchanged; turn that setting off in the game if desired. The user confirmed VR gameplay works while another Windows window has focus after enabling this setting. Long-term absence of the intermittent freeze remains to be established; F9 is still available for the guarded stuck state.

## Keyboard shortcuts while the game is in the background

The user notes that F6, F9 and other keyboard shortcuts go to the foreground Windows application while the game is unfocused. Investigate suitable control methods for background VR play, including SteamVR controller actions and an explicitly scoped alternative for companion shortcuts. F6 belongs to ScriptEngine, not the companion, so do not assume changing the companion keyboard handler fixes F6. Preserve existing controller actions and avoid capturing keystrokes in other applications without an intentional design and user agreement. No background shortcut solution has been implemented.
