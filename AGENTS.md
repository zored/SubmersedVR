# Project notes

- Work in branch `zored` of `zored/SubmersedVR`; upstream is `Okabintaro/SubmersedVR`.
- Record user-facing feature changes in `ZORED_CHANGELOG.md`.
- Local game: `D:\SteamLibrary\steamapps\common\Subnautica`. The player uses PS VR2 and SteamVR.
- The original SubmersedVR plugin is installed with Tobey's BepInEx pack. Keep the upstream DLL and SteamVR bindings intact when developing the separate Zored PDA plugin.
- Build and install the PDA plugin with `powershell -ExecutionPolicy Bypass -File .\install-zored-pda.ps1`; `-BuildOnly` compiles without installation. The game must be closed to install.
- Check `BepInEx\LogOutput.log` for plugin loading and errors. Actual VR positioning and interaction require testing in the headset.
- `F7` and `F8` adjust the PDA multiplier in the running game; the value is persisted by BepInEx. Full code reload requires more lifecycle cleanup and is not implemented.

- After completing changes, commit and push them to branch `zored`; report whether the game needs restarting.
- The player's preferred PDA scale was saved as 2.0 in BepInEx configuration.

- F10 records a read-only snapshot of PDA canvases, materials and cameras in the Zored PDA Depth log. Do not change shared UI camera depth behavior to fix PDA-only occlusion.
