# Zored changelog

## Unreleased

- Add a separate SubmersedVR companion plugin that enlarges the PDA interface to 200% by default. F7/F8 adjust the scale in 10% increments while the game is running.
- Add a PowerShell build and install script for the companion plugin.

- F10 records a read-only snapshot of PDA canvases, materials and cameras to diagnose depth without changing other UI rendering.

- Add an opt-in F11 experiment that renders the PDA on a dedicated camera and layer while keeping the shared UI camera unchanged. Closing the PDA or switching the experiment off restores its original layers.
