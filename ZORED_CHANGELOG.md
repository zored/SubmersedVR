# Zored changelog

## Unreleased

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
