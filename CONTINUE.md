# Continue on Windows

## Player checklist

Run after `git pull`, then a **fresh** game start (no F6) — this is the only way to test the bootstrap.

1. Autoload: the companion works without F6; if not, the pause menu shows `Activate PDA mod` and it works.
2. Laser: anchored to the palm, stable when the index finger bends; hidden while the PDA is open, back in the pause menu.
3. Touch: press, hold, release, finger scroll (starts after ~5 mm), laser drag, inventory drag.
4. Touch accuracy: press the toolbar icons left to right, then screen corners and centre; note any icon that is missed or offset.
5. Right hand stops at the glass while pressing and does not stick when moving the tablet.
6. Left hand is no longer inside the tablet; `PDA shift ±` and `PDA size ±` in the pause menu work and persist.
7. Opening: the tablet appears directly in place; closing hides the tablet and UI together.
8. Glass: darker, smooth waves, static chips, ripple on press and while sliding; check both eyes and frame rate.
9. Reflection of the right hand and blurred head: currently reported invisible.
10. Chest gesture: taking the PDA from the chest.

Send `D:\SteamLibrary\steamapps\common\Subnautica\BepInEx\LogOutput.log` after steps 1, 4 and 9.

## Prompt for the agent

```
Continue SubmersedVR development in C:\Users\BAZA\git\github.com\zored\SubmersedVR, branch zored.
Read AGENTS.md, ZORED_PDA_STATUS.md and CONTINUE.md first; run `git status`.
Build: `powershell -ExecutionPolicy Bypass -File .\install-zored-pda.ps1 -BuildOnly`; load into a running game with `-HotReload`, then F6.
The last commit added untested press-time diagnostics (not compiled on the author's machine — build first and fix compile errors).
Never close the game, never overwrite saved settings, never change SteamVR bindings; mention a restart only when actually required.
Commit to origin/zored after verification and record user-facing changes in ZORED_CHANGELOG.md.

Tasks in order:
1. Touch accuracy. Each finger press logs "Zored PDA contact measurement" and "Zored PDA contact precise".
   Compare glassUv (fingertip projected onto the tablet screen face) with uiUv (fingertip in the PDA canvas, both 0..1).
   - ageFrames > 0 or tabletLocalNow != tabletLocalSaved: the tablet moves between pose capture (VRHands.LateUpdate) and input; fix the timing.
   - glassMode=bounds: the mesh is unreadable and the model bounds approximate the glass; uv is unreliable.
   - stable offset/scale between glassUv and uiUv: implement the glass->UI mapping (finger projected onto the glass face, mapped by uv onto the canvas) behind a config flag, default off; keep press/hold/release thresholds and scrolling.
   The earlier "finger far from tablet model" readings are probably a diagnostic artefact; the new fields confirm or refute it.
2. Hand/head reflection is invisible. Check the one-time active/stereo log in PdaHandReflection; suspect the UI camera drawing over the glass.
3. Verify the bootstrap autoload result from the log of a fresh start.
4. Remove or flag the touch diagnostics in PdaFingerTouch.cs and DepthProbe.cs once accuracy is confirmed; delete the "DEBUG(ChatGPT)" comments with them.
Report what was verified in the headset versus only built or unit-tested.
```
