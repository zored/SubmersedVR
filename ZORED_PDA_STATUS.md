## ScriptEngine recovery after changing saves

- After returning to the menu and loading a save, ScriptEngine retained registrations for destroyed companion and reflection-diagnostic instances, causing duplicate GUID errors. A one-time helper verified that removing only those destroyed registrations restored both scripts in the running game; a subsequent normal reload also succeeded. No game restart or save modification was required for this session.
- The bootstrap now applies the same guarded cleanup before activation. Its updated DLL is built but must be installed while the game is closed and verified after a fresh launch and a save transition. It never removes live instances or unrelated registrations.

## Reflection investigation

- After a fresh game launch, the player confirmed the mirror-like hand reflection is visible only in the left eye, while an additional hand-shaped image inside the tablet is visible in both eyes. GrabPass refraction may account for the latter, but this has not been isolated experimentally. The game launches after a computer restart.
- Retested Unity bundle build after the restart: Unity 2019.4.41 still reports no valid license. No revised shader bundle was produced.
- Forward capture produced nonzero alpha in both eye textures. The player saw the pale silhouette in the left eye; the installed build now uses a warmer flat hand tint. Headset verification of that tint and both eyes remains open.
- The game reports multi-pass stereo; the existing glass shader chooses the left texture for that mode. A shader correction was investigated but the local Unity editor reported no valid license, so no new bundle was built or installed. The right-eye appearance remains unresolved.

## Controller reload chord

- Holding the existing MoveDown and MoveUp actions (L1 + R1 in the current PS VR2 binding) for three seconds with the PDA open requests ScriptEngine reload. The chord is disarmed on startup or reload until both buttons are released; no bindings are changed. Automated rule tests and compilation passed; the player and game log confirmed reload after holding R1, then L1. The chest gesture is suppressed while R1 is already held and the PDA is open.

## Fresh-start and touch check, 8 November 2026

- The fresh-start log shows bootstrap activation, glass shader loading and stereo reflection capture active. The player confirmed finger presses and scrolling worked without noticeable errors; the detailed temporary touch measurements were removed. The unreadable mesh made glass UV measurements approximate, and the one-frame pose age and differing tablet coordinates do not alone establish a touch fault.
- The player reported no visible reflection. Earlier capture diagnostics reported zero alpha in both reflection textures; the current active/stereo log does not prove they contain visible pixels. The cause remains open.
- F9 restored time after a zero-time PDA pause with no active FreezeTime reasons. The player attributed it to the paused game; the underlying transition remains unproven. The guarded recovery is unchanged.
- The diagnostic cleanup is build-verified; it has not yet been retested in the headset.

## Experimental right-hand and head reflections

- Prepared an experimental reflection of the right-hand skinned renderer, restricted to a 0.16 m region around the right hand; an indistinct ellipsoid around the view camera represents the character's head. The low-opacity reflection is softened when composited into the existing screen material. Two 256x256 off-screen textures are refreshed at most around 30 Hz when the PDA is open; no world or UI layers are changed. The effect falls back off if capture throws or shaders are unavailable. Accurate hand isolation, reflection visibility, anatomical position, both eyes, UV mapping and actual VR GPU cost require headset validation. Current UI camera may draw in front of the glass and obscure the effect.
- Isolated Unity tests verified reflection-plane math, a skinned mesh following a bone in the isolated capture, mask clipping, separate textures and blending into the glass. The v6 bundle's PDA material reported supported under isolated Unity 2019.4.36 D3D11; stereo and the actual game mesh were not verified by those tests. A build based on the installed touch-diagnostic source was installed; the running game reloaded and reported shader loaded and screen material applied. The repository capture source includes an additional one-time active/stereo log that was compiled but not installed to avoid overwriting a parallel touch task.
## Wider, less frequent ripples

- Increased ripple refraction offset from 0.0014 to 0.0035 and broadened the ring, preserving edge fade. Motion-triggered waves are throttled to 0.24 s instead of 0.12 s; initial press remains immediate. The rules regression test passes, and the v5 shader loaded in Unity 2019.4.36 D3D11. The running game reloaded a separately built companion and applied the shader. Visual strength and alignment remain to be checked in the headset. Other parallel hand-position changes were left uninstalled, as in their originating task.

## Stronger PDA ripples

- Player confirmed the first ripple works and requested greater visibility. Refraction and highlight strength were increased and the fade slowed slightly; no texture samples were added. The v4 bundle was verified in a Unity 2019.4.36 D3D11 test, installed alongside the in-use v3 bundle, and the live log confirms script reload and glass application. Visual strength, both eyes and GPU impact require headset confirmation.

## Finger-contact ripple and static glass chips

- PDA_screen's existing shader now adds up to four short, edge-faded touch ripples to the existing refraction and three subtle static edge chips. It uses the existing GrabPass and texture samples; no extra screen grab or texture sample was added. GPU cost is not yet measured in VR.
- Finger press starts a ring; held movement produces weaker rings with minimum movement and time limits. Animated fingertip is transformed from UI space back into model space using the camera-root translation and the screen renderer transform. Face bounds are derived from the screen submesh when readable, otherwise the whole mesh bounds are used. Accurate alignment, corner fading, visibility under the UI, appearance of chips, both eyes and performance require live headset testing. UI and model are independently scalable/offset, so mapping remains provisional.
- Standalone ripple event test passed. Shader bundle loaded and was reported supported in the isolated Direct3D11 player using game Unity 2019.4.36. The running game automatically reloaded the companion and logged the shader loaded; actual effect and frame time remain unverified. Installed as v3 alongside older bundles to avoid files held by the running game. Existing game settings, other plugins and bootstrap were not changed.
## Startup fallback, laser visibility and revised glass

- The live startup log showed ScriptEngine loading ZoredPdaScale during Chainloader startup and failing with NullReferenceException in GameObject.AddComponent. LoadOnStart was true; the later reload succeeded. The script now waits for the player and pause menu via a separate BepInEx bootstrap with dependencies on ScriptEngine and SubmersedVR, and the bootstrap installs an independent "Activate PDA mod" pause button. ScriptEngine LoadOnStart is set to false so its early load no longer races the chainloader. Only the PDA script is currently present in the scripts directory. A fresh game start and the fallback button remain unverified; bootstrap itself cannot load into a running BepInEx session.
- The companion hides only the UI laser line and dot renderer while the PDA is in use, except when the pause menu is open. Raycast camera and cursor are retained; dot renderer state is restored on exit/unload. Verify cursor feedback, laser interaction, pause-menu laser and both eyes in the headset.
- Revised glass has a subtle smooth spatial wave, five-sample blur and darker tint. Shader bundle loaded with isSupported=true in an isolated Direct3D11 player using game Unity 2019.4.36. A new v2 bundle filename avoids the old bundle locked by the running game. The current ScriptEngine auto-reloaded and logged shader loaded; game appearance, stereo, and performance remain unverified.
## Finger scrolling start threshold

- SubmersedVR's ShouldStartDrag patch uses a 0.04 world-XY threshold, so finger-driven scrolling can require approximately 4 cm of projected movement before it starts. A Unity Harmony test using the original patch reproduced failure to start at 5 mm; this is a confirmed code-path delay, though the player's full in-game symptom still needs headset validation.
- The companion now applies a 0.005 world-XY threshold only while dispatching a finger gesture to a ScrollRect or Scrollbar. The original threshold remains for laser gestures and other drags. A Unity test applying both Harmony patches verified 3 mm does not trigger, 6 mm triggers for finger scrolling, and the old threshold remains for laser and item cases. The test used a stub for the held target; verify actual target selection, scrolling, small clicks and drag behavior in game. World-XY projection and natural hand jitter may affect perceived threshold.
## Palm-anchored laser and independent finger collision

- The laser origin follows the right hand and the index finger's straight rest pose; animation of individual finger joints no longer moves it. Its direction remains the controller UI direction. The animated fingertip is tracked separately for touch.
- Touch uses a separate camera projected onto the PDA surface and signed fingertip distance. A crossing toward the surface presses, withdrawal releases, and a large tracking jump or an already-near finger on opening cannot press. Physical controller input takes priority. ScrollRect and Scrollbar callbacks receive coordinates from the touch camera during finger gestures.
- Plugin compilation and standalone gesture rules pass. In an isolated Unity 2019.4.41 play-mode test, the actual touch probe with Harmony selected a UI target underneath the finger while the laser remained elsewhere, and the camera/coordinate override was restored. Separate Unity tests verified rest-pose geometry and ScrollRect behavior. These tests do not establish behavior in Subnautica or VR stereo. Verify laser stability while moving the finger, clicking, holding and scrolling, laser dragging, haptics and both eyes in the headset. The thresholds and moving PDA need particular attention.
## Experimental PDA screen refraction

- PDARefraction applies a custom screen-space refraction material only to the PDA_screen material slot and restores the original on close/unload. Missing, unloadable or unsupported shaders leave the original material intact; other tablet materials and UI are not replaced.
- The D3D11 shader was compiled with Unity 2019.4.41f2. The unchanged installed bundle loaded and shader.isSupported returned true under the game UnityPlayer 2019.4.36f1 in an isolated test application. The test shell needed its own version headers adjusted; this does not verify game rendering, VR stereo or performance. Check appearance and both eyes in the headset.
- Unity Personal was activated. Unity Support states that 2019.4.36 does not support the current Personal activation; 2019.4.41f2 is the earliest supported version. The project files and compiled bundle are in PdaGlassShader.
## Combined PDA size controls

- Pause-menu layout now has two PDA size buttons. They scale the current interface and model together in 5% steps, preserving their ratio; existing width and horizontal offset are not altered. The effective interface scale range is extended to 0.5–3.0 to permit increasing from the previous 2.0 setting.
- Proportional scaling and boundary tests pass. Verify both controls, fit and both eyes in the headset. The individual keyboard shortcuts and other menu actions remain available.
## Finger hold and scrolling

- Finger contact now produces separate press, hold, and release events through the normal FPSInputModule. A close or plugin unload cancels a held gesture; controller input takes priority.
- The player confirmed finger scrollbar dragging, release, normal clicking and laser dragging. The press threshold was adjusted from 0.015 m to 0.025 m while the release threshold remained 0.05 m; the player confirmed the response and hold feel comfortable.
- Automatic loading on the next fresh game start remains unverified. Existing focus-freeze and layer robustness observations below still apply.
## Latest live checks

- Options + triangle was observed to capture a snapshot with the PDA open and pause menu closed.
- ScriptEngine LoadOnStart is now enabled in the local config. The companion waits until SubmersedVR is loaded before patching. Automatic load on the next fresh game start still needs verification; do not close the current game for that check.
- Layer-conflict diagnostics identified the game's own WorldCursor and Tooltip following the PDA layer. The collision check now excludes those actual game components. The cursor and tooltip were followed by a dynamically created PDA child being mistaken for a collision; PDA descendants on the temporary layer are now tracked and restored. Repeated openings, other scenes and hand visibility still require headset verification.
- The user confirmed reliable touch, proximity and press colors, haptics, laser dragging, and seeing hands in front of the interface in the current session. Verify both eyes and repeat after a fresh game start.
# Current work in progress (not headset-verified)

- Direct touch was observed but reported unreliable. A rear-offset UI ray probe now covers crossing the screen plane; fast-crossing rule test passes. Verify activation, no click-through, laser input, inventory dragging and both eyes in the headset.
- Proximity colors and short right-hand haptics are prepared. The original colors are restored after leaving the target. Their appearance and haptic strength have not been confirmed in the headset.
- PDA camera now reserves an unoccupied scene layer despite broad camera masks and restores modified masks; F11 remains requested by default. Verify repeated opening, layer collisions, other UI and whether hands appear in front of the interface. Hand overlay has not been implemented.
- A diagnostic chord uses the existing PDA and UIMenu actions: start with the PDA closed, hold triangle until the PDA opens, then briefly press Options. It suppresses the pause action for the chord; no binding was modified. Verify this and ordinary Options/recenter behavior.
- The guarded skip of the observed transition into player_pda_start was accepted by the player. The native canvas-hide method now runs at the start of closing; the player confirmed the tablet and interface disappear together and reopen normally.
- Existing focus/PDA freeze root cause and other unverified menu actions remain open as recorded below. No changes are committed or pushed until the requested headset checks.
# PDA and PS VR2: current investigation

## Pointer

The user confirmed the pointer works after caching the fingertip pose after VRHands.LateUpdate and applying it for input. This keeps the input ray aligned with the rendered finger while moving the head. A direct finger-touch prototype exists; the user reported inconsistent activation and the current revision awaits headset verification.

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

Give the next concrete steps without asking whether the user is ready. Do not suggest saving the game; never close it without the user directing the session. The user requested direct finger-touch activation and will test the current revision in a later game session.

## Current handoff

The user wants the next chat to implement direct finger-touch activation of PDA buttons. Existing fingertip ray pointing works; physical touch activation does not exist yet. Keep existing SteamVR bindings and settings, build the separate companion, use automatic ScriptEngine reload, and validate both eyes and drag behavior. The F11 camera may temporarily find no unused layer and now retries while requested; do not claim it always activates. The new menu actions beyond UI size + still need headset checks.

## Tablet shift and visual finger constraint (headset validation pending)

- Added TabletShift, initially 10% of the scaled tablet width, with pause-menu +/- controls in 2% steps. Existing UI-only offset and scale settings are retained. Model and UI move by the same world-space amount.
- After caching the tracked fingertip, the visual hand is constrained to the PDA screen face after a front-to-back crossing. It follows motion along the face and releases on withdrawal or leaving the face. The screen submesh is used when readable; otherwise the front side of the full mesh bounds is an approximation. IK-enabled mode is left unchanged.
- A held UI probe starts in front of the interface even for a deeply tracked fingertip, so deep movement does not itself invalidate an existing hold. Finger input uses the original tracked pose.
- Companion and bootstrap compiled in a separate copy; standalone touch, surface, menu, ripple and pose tests passed. Actual glass alignment, fallback geometry, moving tablet, hold/scroll/release, laser, ripple, menu layout, both eyes and performance remain unverified in the headset.

## Saved preferred PDA defaults

- As of 2026-11-07 the player's selected values are Scale 2.205, TabletModelScale 2.09475, TabletWidth 1, HorizontalOffset 0.05 m, TabletShift 0.24, ChestGesture true and RunInBackground true. These are the defaults for a new config; saved values continue to take precedence.

## Opening pose visibility

- Frame diagnostics showed the tablet screen anchor moving substantially between the opening frame and the next frame, coinciding with the existing player_pda_start transition skip; the UI settled a frame later. The model and canvas are hidden until the skipped pose has finished the frame, with a 0.25 s fallback and restoration on close or unload. Standalone visibility rules and companion build passed. Verify initial appearance, rapid open/close, menu/touch and both eyes in the headset.
