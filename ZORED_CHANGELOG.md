- Reduced the finger-scroll drag threshold from 4 cm to 5 mm for ScrollRect and Scrollbar; headset validation pending.

# Zored changelog

## Unreleased

- Add experimental right-hand and head reflections on PDA glass; VR alignment, stereo and performance need checking.

- Add subtle edge-faded touch ripples and static edge chips to PDA glass; mapping, stereo and VR performance need headset checks.

- Hide the UI laser while using the PDA; retain interaction and restore it in the pause menu.
- Soften and darken PDA glass with smooth refraction waves and a mild blur.
- Add a separate delayed script bootstrap and pause-menu activation button; fresh-start verification is pending.

- Prepare an experimental screen-only refractive glass material, with fallback to the original if the shader cannot be loaded. Unity 2019.4.41-to-2019.4.36 bundle compatibility and stereo rendering need headset testing.

- Replace the separate PDA size and alignment menu controls with proportional PDA size decrease/increase, retaining the saved width and offset; headset validation is pending.

- Support finger press, hold, release and dragging for PDA scrolling while preserving controller input; the player confirmed scrolling and the adjusted touch response in VR.

- Skip the observed PDA opening pose transition and hide the model and canvas together on close while preserving PDA timing; the player confirmed the result in VR.

- Wait for SubmersedVR before initializing the companion, allowing ScriptEngine startup loading; the next fresh game start still needs verification.
- Avoid mistaking the game's cursor and tooltip for foreign occupants of the PDA camera layer.

- Prepare experimental direct fingertip presses with proximity/press colors and right-controller feedback; extended probing, controller interaction, and both-eye rendering still need headset verification.
- Reserve an unused PDA layer even when other cameras use broad culling masks, restoring those masks afterward; verify camera isolation and hand visibility in VR.
- Prepare Options + triangle diagnostic snapshots using existing actions, and hide the tablet mesh during the existing closing sequence; verify both in VR.

- Add direct pause menu controls for PDA dimensions, positioning, diagnostics, camera mode and ScriptEngine reload; adjustment labels show steps from the preferred baseline.
- Automatically reload companion settings after config file changes and start ScriptEngine’s enabled file watcher in the current session.
- Keep the PDA camera requested and retry when no unused layer is temporarily available.

- Add a PS VR2 binding for a short Options press using the Sony driver’s single-click mode; the user confirmed that it opens the pause menu. A long press still recenters VR.

- Keep the PDA pointer aligned with the tracked fingertip by capturing its pose after the VR hand update; the user confirmed the pointer works in the headset.
- Request the PDA-only camera automatically after load or F6; F11 can still toggle it for the session, and the unused-layer safety check remains in place.

- Add a separate SubmersedVR companion plugin that enlarges the PDA interface to 200% by default. F7/F8 adjust the scale in 10% increments while the game is running.
- Add a PowerShell build and install script for the companion plugin.

- F10 records a read-only snapshot of PDA canvases, materials and cameras to diagnose depth without changing other UI rendering.

- Add an opt-in F11 experiment that renders the PDA on a dedicated camera and layer while keeping the shared UI camera unchanged. Closing the PDA or switching the experiment off restores its original layers.

- Avoid nesting the PDA rendering camera under another VR-tracked camera by default. F12 switches between sibling and original nested camera modes for live comparison; F10 records the physical PDA model hierarchy.

- Enlarge the tablet mesh in its local X/Y plane while preserving thickness and the hand/screen anchor. Numpad +/- adjusts model size in-game.

- Add Numpad 4/6 to shift the PDA interface horizontally and Numpad 1/3 to adjust tablet width separately, with persistent settings. The new default shifts the interface slightly and widens the tablet; verify local directions and fit in VR.

- Replace keypad controls with letter keys while the PDA is open: J/L move the interface, U/O adjust tablet width, and K/I adjust tablet size.

- Scale the physical tablet around the mesh bounds center and choose the width axis from the screen orientation; verify the axis and alignment in VR.

- Prepare an opt-in ScriptEngine build mode for F6 reloads and restore PDA UI state when unloading; in-game compatibility remains to be verified.

- Correct tablet width adjustment after in-game testing identified X as the height axis; select width from the remaining face dimensions and preserve thickness.

- Set the tested PDA defaults to interface scale 2.0, horizontal offset 0.05, tablet scale 1.9 and width 1.0; preserve existing saved settings.

- Prepare an experimental PDA-only UI ray origin at the right index fingertip, retaining controller aim and restoring the previous origin when the PDA closes; headset verification is required.
- Add a separate PS VR2 Sense binding based on the Oculus layout, with Options assigned to the pause action; it must be selected in SteamVR and tested.
- Extend the F10 diagnostic snapshot with PDA open/use state, model transform and player animation state for investigating intermittent PDA failures.

- Recalculate the PDA interface immediately when F7/F8 changes its saved scale.

- Include game time, focus and animator transition details in F10 snapshots to investigate PDA hangs after switching Windows applications.

- Add F9 as a manual recovery for a stuck PDA when the game is focused, time is stopped and no game pause reason is active; record freeze reasons in F10 snapshots.

- Extend F10 snapshots with UI pointer, raycast result and PDA canvas positions to investigate cursor drift.

- Enable the game's run-in-background setting when the companion loads, configurable in General/RunInBackground; F10 also records both background settings. Background VR testing is ongoing.

- Added separate optional BepInEx auto-load plugin for the most recent save after startup initialization. Built and installed locally; fresh game startup and VR behavior remain unverified. Disable through BepInEx config or remove the separate plugin DLL.

- Added visual fingertip constraint against the tablet face and a separate pause-menu tablet/UI horizontal shift control. Deep held touches continue to use the tracked finger. Requires headset validation; mesh-bounds fallback is approximate.

- Set PDA defaults to the player's current interface scale 2.205, tablet scale 2.09475, and tablet shift 24%; existing saved settings remain authoritative.

- Expanded F10 diagnostic snapshots with tracked fingertip, tablet geometry, and the last touch probe result for alignment investigation.

- Log contact positions and the full UI target path at finger press for alignment diagnostics.

- Hide the tablet and PDA interface during the brief initial opening pose and reveal them after the pose settles, avoiding a visible positional jump.
