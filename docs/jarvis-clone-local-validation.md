# Local Jarvis reference voice integration — 2026-09-23

The running HanEnCursorIndicator now selects `cosyvoice`, using the installed reference `jarvis_english_20260921`. This replaces pitch-shifted Supertonic for the selected engine. The reference is community-uploaded Jarvis-labelled audio; official film provenance and perceptual similarity are not verified.

## Implementation

- Added a third engine in the tray menu and voice settings. Unrelated Supertonic voice controls are disabled when clone mode is selected.
- Persisted `cosyVoiceStudioPath` and `cosyVoiceSpeaker` in the existing voice settings.
- Reuses the local CosyVoice studio installation, starts its launcher without opening a browser when required, waits for readiness and registers the saved prompt after engine restart.
- Uses local HTTP only. Missing installation/reference or synthesis errors are reported instead of substituting a different voice.
- Converts the engine's float32 mono WAV to PCM16 for the existing Windows playback pipeline. No Jarvis pitch filter is applied to the cloned voice.
- Starts the voice engine on CPU to preserve GPU headroom for the screen-reading model. Existing studio backend preferences are not overwritten.

## Current validation

- C# compiler and git diff whitespace checks passed.
- Existing Supertonic picker regression script passed.
- Saved engine/reference mapping passed. English and Korean float-to-PCM conversion preserves sample count and duration (6.00 and 7.76 seconds).
- Actual running app: hotkey activated screen reading at 00:48:54; model answer completed at 00:48:55; the app automatically started the CPU clone server and registered its prompt. Clone synthesis took 20.403 seconds. Native playback opened at 00:49:21, played at 00:49:21.900 and closed at 00:49:26.008 without playback error.
- A subsequent screen read completed at 00:49:26 while the voice engine remained loaded. GPU free memory was about 2849 MiB after switching voice inference to CPU.
- Earlier Vulkan trials took 32.53 seconds for English and 44.83 seconds for Korean, and left insufficient free GPU memory for the existing screen-reader guard. The guard was not relaxed.

## Boundaries

Speech stays Korean unless the supplied text is English; cloning does not translate. The current voice speed is 100%. Synthesis and native playback have been verified; film-voice resemblance requires user listening. Publication is tracked by Git history; the checks above were completed before publication.

Local samples and verification scripts: `%LOCALAPPDATA%\HanEnCursorIndicator\tests\jarvis-20260923`.
Original executable and settings backup: `%LOCALAPPDATA%\HanEnCursorIndicator\backups\jarvis-20260923-003738`.