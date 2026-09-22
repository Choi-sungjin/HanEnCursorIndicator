# HanEn Cursor Indicator

A Windows tray companion for Korean/English input indication, local screen reading, and reference-voice speech.
커서 옆의 `한` / `en` / `EN` 표시부터 **선택 영역 읽기, 답변 말풍선, 로컬 음성 복제**까지 지원하는 Windows 앱입니다.

![HanEn: 입력 상태, 영역 읽기, 자비스 복제 음성, 검증 결과 시연](assets/demo.gif)

> **26초 · 무음 GIF · 공개 예제로 재현한 기능 흐름**입니다. 실제 앱 설정 화면을 공개용 값으로 렌더링했고, 파형은 실제 합성 음성에서 가져왔습니다. 개인 바탕화면 녹화가 아닙니다.

**음성 들어보기:** [한국어 샘플](assets/jarvis-korean.wav) · [영어 샘플](assets/jarvis-english.wav)

영어판 자비스를 목표로 한 참조 샘플 기반 복제입니다. 공식 영화 원본 녹음 여부는 확인되지 않았으며, 원본 배우의 공식 음성 제품을 의미하지 않습니다.

## 최신 기능 / What's new

- **영역 읽기:** 지정한 화면 영역을 로컬 모델에 보내고 결과를 말풍선으로 표시합니다. 답변 상한은 **200자**입니다.
- **음성 엔진 3종:** Supertonic 기본 음성, 선택적인 Supertone API, **CosyVoice 참조 음성 복제**를 구분해 선택합니다.
- **자비스 복제:** 일반 남성 음성의 음높이를 낮추는 기존 프로필과 별개입니다. 복제 결과에는 그 음높이 필터를 적용하지 않습니다.
- **개별 제어:** 영역 지정, 읽기 켜기/끄기, 말풍선 닫기, 답변 음성을 각각 제어합니다. 단축키는 앱에서 설정합니다.
- **안정성:** 오래된 디스플레이 드라이버 이벤트로 읽기가 잘못 중단되는 경우를 줄였으며, 메모리 부족 시의 보호 동작은 유지합니다.

## 사용 시연 테스트 / Verified demo

| 단계 | 확인한 동작 | 확인 범위 |
| --- | --- | --- |
| 공개 예제 읽기 | `2 + 3 = ?`, 보기 A=5 / B=6 / C=7 → 실제 응답 `2 더하기 3 은 5 가 돼서 A 가 정답이야.` | 앱의 요청 생성·응답 추출 코드 + 로컬 `qwen3.5:4b`, 2026-09-23 |
| 음성 합성 | 저장된 참조 음색으로 한국어·영어 WAV 생성 | 위 샘플에서 청취 가능 |
| 실제 앱 재생 | 화면 읽기 → 답변 → CosyVoice 합성 → Windows 재생 완료 | 실행 중인 앱에서 확인, [검증 기록](docs/jarvis-clone-local-validation.md) |
| 형식 호환 | float32 WAV → PCM16 변환 | 샘플 수와 재생 길이 보존 |

테스트 PC의 CPU 복제 합성은 약 **20–34초**였으며, 문장 길이와 장비에 따라 달라집니다. 그래픽 메모리를 화면 읽기 모델에 남겨두도록 앱이 시작하는 복제 엔진은 CPU를 사용합니다.

**모델의 답변은 검토가 필요합니다.** 단순 덧셈 시연은 통과했지만, 별도의 혼합 산술 예제에서는 오답이 나왔습니다. 위 표는 전체 문제 풀이 정확도에 대한 보장이 아닙니다. [공개 예제와 결과](assets/demo-evidence.json)

**시연 개인정보 보호:** 예제 문장, 공개용 설정 화면, 생성 음성만 포함합니다. 계정, 사용자 폴더 경로, 개인 대화, 클립보드 내용, 원본 로그와 설정 파일은 포함하지 않습니다.


## Usage Example / 사용 예시

1. 웹에서 받은 실행 파일 또는 로컬 개발 빌드 `CursorImeIndicator.exe`를 실행합니다.
2. 설치 과정 없이 바로 실행되고 Windows 트레이 아이콘이 추가됩니다.
3. 한글 입력 상태에서는 미니미 얼굴에 `한`이 표시됩니다.
4. 영어 소문자 입력 상태에서는 `en`, 대문자 입력 상태에서는 `EN`이 표시됩니다.
5. 입력 상태가 바뀌면 미니미가 1초 동안 마우스를 가리킨 뒤 정자세로 돌아옵니다.
6. 미니미 이미지·크기·표시 모드를 취향에 맞게 바꿉니다.
7. 트레이 메뉴의 말풍선 항목에서 읽을 영역을 지정하고 화면 읽기를 켭니다. 영역 지정 단축키도 별도로 설정할 수 있습니다.
8. 답변을 소리로 들으려면 `답변 음성`을 켭니다. 드래그한 글 읽기는 별도의 `드래그 텍스트 읽기` 옵션입니다.
9. `보이스 설정` → `TTS 엔진`에서 원하는 엔진을 선택합니다. 복제 목소리는 아래 CosyVoice 연결 설정이 먼저 필요합니다.

입력 표시만 사용하면 추가 AI 엔진은 필요 없습니다. 화면 읽기와 음성 기능은 각각 로컬 모델·엔진 또는 선택한 API 설정이 필요합니다. 상시 화면 읽기는 앱을 재시작하면 다시 켜야 합니다.

## Download

The commercial build is downloaded from the official website after license verification. GitHub should not publish the paid executable.

For local development builds, run:

```bat
build.bat
```

The local build output is `CursorImeIndicator.exe`. Package that file privately for the website/Microsoft Store flow.

## Windows Support

- Windows 10 / Windows 11
- No separate installer required
- Built with the .NET Framework compiler included on Windows

## Included Mascot Images

The default image pack uses three shared pose files:

| Pose | File | Behavior |
| --- | --- | --- |
| Idle | `dist/images/idle.png` | Shown when the mouse cursor is still |
| Point | `dist/images/point.png` | Shown for 1 second after input mode changes |
| Cheer | `dist/images/cheer.png` | Shown while the mouse cursor is moving |

The app draws `한`, `en`, or `EN` on the mascot face at runtime, so the basic pack only needs three pose images.

## Character Concepts

See [`list/`](list/) for 13 original mascot concept images and an animated preview GIF.
Ready-to-use 3-pose packs are in [`list/packs/`](list/packs/).

## Custom Images / 이미지 추가

To replace the mascot, put images next to the exe. You can use either a 3-image shared pack or a 9-image state pack.

Fastest path: right-click the tray icon, choose the image picker, select 3-image shared mode or 9-image state mode, choose the state/pose slot, then pick a file. The app copies it into `dist/images/` with the correct slot filename and reloads it immediately.

### 3-image shared pack

Use three pose images. The app draws `한`, `en`, or `EN` on top of the same image set:

```text
dist/
  HanEnCursorIndicator.exe
  images/
    idle.png
    point.png
    cheer.png
```

### 9-image state pack

Use separate images for each input state and pose. The app picks the image by current state + current pose:

```text
dist/
  HanEnCursorIndicator.exe
  images/
    ko-idle.png
    ko-point.png
    ko-cheer.png
    en-idle.png
    en-point.png
    en-cheer.png
    upper-idle.png
    upper-point.png
    upper-cheer.png
```

State names inside the app are `ko`, `en`, and `EN`. `EN-idle.png`, `EN-point.png`, and `EN-cheer.png` are also supported, but Windows folders are usually case-insensitive, so `upper-*` is the safer filename set when you also have `en-*` files in the same folder.

Supported image formats:

```text
images/idle.gif
images/idle.png
images/idle.jpg
images/idle.jpeg
images/idle.jfif
images/idle.bmp

images/point.gif
images/point.png
images/point.jpg
images/point.jpeg
images/point.jfif
images/point.bmp

images/cheer.gif
images/cheer.png
images/cheer.jpg
images/cheer.jpeg
images/cheer.jfif
images/cheer.bmp

images/ko-idle.png
images/en-idle.png
images/upper-idle.png
```

The app searches in this order: GIF, PNG, JPG, JPEG, JFIF, BMP.

Tips:

- Use transparent PNG files for clean static mascot poses.
- Use animated GIF files if you want a moving pose.
- With a 3-image pack, leave a blank face area; the app draws `한`, `en`, or `EN` automatically.
- With a 9-image pack, you can include the text directly in each image and turn off `글자 표시` from the tray menu.
- Right-click the tray icon and choose `이미지 폴더 열기` to open the correct folder.
- After changing files, choose `커스텀 이미지 다시 불러오기`.
- Choose `이미지 누끼 처리` from the tray menu to select one or more images and save transparent `*-cutout.png` copies. It samples the outer edge color to remove connected backgrounds, falls back to a centered subject mask for complex photos, and the default option shrinks large images to a lightweight 160px app-ready PNG.
- Turn on `라인으로 누끼 보정` in the cutout options to draw correction lines before saving.
- Use `윤곽 안쪽만 남기기` and drag around the outside contour of the subject; the app closes the outline and removes everything outside it. Use `배경 라인 제거` only when drawing on a background area that should be removed.

## Size Control / 크기 조정

Right-click the tray icon and open `크기`.

- Choose a preset: `50%`, `75%`, `100%`, `125%`, `150%`, `200%`, `250%`.
- Choose `드래그로 크기 조정` to open a slider.
- Drag the slider with the mouse to tune the size gain by percentage.
- The selected percentage is saved and reused next time.

## Display Mode / 표시 모드

Right-click the tray icon and open `표시 모드`.

- `항상 따라다니기`: the mascot follows the cursor whenever the app is enabled.
- `멈췄을 때만 표시`: the mascot hides while the mouse is moving, then appears next to the cursor after the mouse stays still for a short moment.
- The selected mode is saved and reused next time.

## Label Position Control / 글자 위치 조정

Right-click the tray icon and choose `글자 위치 조정`.

- Choose a state: `ko`, `en`, or `EN`.
- Choose a pose: `Idle`, `Point`, or `Cheer`.
- Drag the blue point anywhere on the image preview to place the label center.
- The app saves label positions separately for each state + pose combination.
- Use `기본값` to reset the selected state + pose.

## Label Toggle / 글자 표시

Right-click the tray icon and toggle `글자 표시`.

- On: the app draws `한`, `en`, or `EN` over the mascot.
- Off: the mascot image follows the cursor without drawing extra text.
- This is useful when a 9-image pack already has the face text inside each image.

## Mascot Color / 미니미 색상

Right-click the tray icon and open `미니미 색상`.

- `기본 색상 선택`: choose the normal mascot clothing color.
- `상태별 색상 사용`: turn on different clothing colors for Korean and English states.
- `한글 색상 선택`: clothing color used when the label is `한`.
- `영어 소문자 색상 선택`: clothing color used when the label is `en`.
- `영어 대문자 색상 선택`: clothing color used when the label is `EN`.
- `글씨 색상`: choose separate face-label colors for `한`, `en`, and `EN`.
- The face label stays readable while the body/clothing area is recolored.

## Voice / TTS (Supertonic · CosyVoice · optional Supertone API)

Right-click the tray icon and open `보이스`.

- Turn on `드래그 텍스트 읽기` to read selected text after a mouse drag.
- `단축키 설정` lets you bind a global hotkey (e.g. `Ctrl+Alt+V`) that toggles `드래그 텍스트 읽기` on/off from anywhere. Click the input box, press the desired key combo, and save. The combo must include `Ctrl` or `Alt`; use `지우기` to remove the hotkey.
- Two TTS engines are available under `TTS 엔진`:
  - `Supertonic 로컬 (무료)` — **default**. Uses the open-source on-device [Supertonic](https://github.com/supertone-inc/supertonic) engine. No API key needed.
  - `Supertone API (클라우드)` — the original cloud engine. Requires an API Key and Voice ID.

### Supertonic local engine (default)

Nothing has to be installed by hand. Open `보이스 > 로컬 음성 설치/점검` (the same window is reachable
from the `로컬 음성 설치/점검` button inside `보이스 설정`) and press `설치`. The window logs what it
is doing line by line and can be stopped at any point.

The setup does, in order:

1. Looks for a Python 3.9+ already on the PC — the `py` launcher, `%LOCALAPPDATA%\Programs\Python`,
   uv's interpreters, Anaconda/Miniconda, and `PATH`. The Microsoft Store `python.exe` alias is
   skipped: it is a stub that opens the Store instead of running Python.
2. Creates a **private** virtual environment at `%LOCALAPPDATA%\HanEnCursorIndicator\supertonic\runtime`.
   An existing Python install is used as the base but is never modified.
3. If no usable Python exists, downloads the official embeddable Python from python.org into
   `%LOCALAPPDATA%\HanEnCursorIndicator\supertonic\python` and bootstraps pip into it. No admin
   rights, no `PATH` changes.
4. Runs `pip install "supertonic[serve]"` in that environment.
5. Pre-downloads the `supertonic-3` model, so the first spoken line is not a 400 MB stall.
6. Verifies that `supertonic`, `fastapi`, and `uvicorn` all import, then records the interpreter
   in `voice.ini` as `localPython`.

About 600 MB is downloaded the first time; roughly 165 MB of that stays as the private runtime.
Everything after that runs offline, with no API key.

If Supertonic already lives somewhere the scan does not reach — a project venv, a named conda
environment — use `Python 직접 지정` to point at that `python.exe`. It is checked and reused as-is,
with no second install.

Notes:

- The server is started as `python.exe -c "…supertonic.cli…" serve --host 127.0.0.1 --port 7788`
  rather than through `supertonic.exe`. pip's generated `.exe` launchers are blocked outright on
  machines with an Application Control (WDAC) policy, while `python.exe` itself keeps running.
  A pre-existing `supertonic.exe` is still accepted as a fallback.
- The server starts in the background when voice is enabled, and is stopped on exit if the app
  started it.
- Trying to speak before setup has run offers the installer instead of just naming a pip command.
- Pick the local voice in `보이스 설정`: `성별` checkboxes (남성/여성) plus the `톤/목소리` slider (1–5) select among the 10 built-in styles (`F1`–`F5`, `M1`–`M5`).
- `속도` slider (50–200%) controls speech speed (Supertonic clamps to 70–200%); `품질(스텝)` slider (1–32, default 8) trades synthesis quality against speed. Supertonic-3 has no separate pitch parameter — tone variation comes from the voice styles.

### CosyVoice reference voice / 자비스 복제

이미 설치한 Windows 로컬 CosyVoice Studio와 저장된 `prompt.gguf` 참조 음성을 연결합니다. 모델이나 참조 음성은 이 저장소에 포함되지 않으며, 현재 앱의 Supertonic 설치 버튼이 CosyVoice까지 설치하지는 않습니다.

1. CosyVoice Studio에서 사용할 참조 음성을 준비합니다.
2. 앱을 종료하고 `%APPDATA%\HanEnCursorIndicator\voice.ini`의 아래 항목을 자신의 설치 위치와 음성 ID로 설정합니다. 기존의 다른 항목은 유지합니다.
3. 앱을 다시 열고 `보이스 설정` → `CosyVoice (자비스 복제)`를 선택합니다. `답변 음성` 또는 `드래그 텍스트 읽기`를 켜서 사용합니다.

```ini
engine=cosyvoice
cosyVoiceStudioPath=D:\VoiceClone\voice-clone-studio
cosyVoiceSpeaker=your_reference_voice_id
speedPercent=100
```

위 경로와 음성 ID는 설명용 예시입니다. 참조 파일은 설치 폴더의 `data/voices/<음성 ID>/prompt.gguf`에 있어야 합니다. 복제는 입력 문장을 번역하지 않습니다. 한국어 문장은 한국어로, 영어 문장은 영어로 합성합니다.

앱은 필요할 때 로컬 엔진을 시작하고 참조 음성을 등록합니다. 누락·합성 오류가 발생해도 다른 목소리로 몰래 대체하지 않습니다. Supertonic의 성별·톤·품질 선택은 복제 음색에 적용되지 않으며, 복제 모드에서 비활성화됩니다.

### Supertone API engine (optional)

- Open `보이스 설정`, switch `TTS 엔진` to `Supertone API`, and enter your own API Key, Voice ID, language, model, style, speed, and max text length.
- The API Key is saved per Windows user with Windows DPAPI encryption at `%APPDATA%\HanEnCursorIndicator\supertone.key`.
- The API Key is not saved in `settings.ini`, not included in Git, and is never shown again in the settings window.
- Drag selection uses a brief `Ctrl+C` copy, sanitizes the copied text, restores the previous clipboard data, and then calls Supertone.
- Symbols and separators such as `-`, `ㅡ`, `_`, `@`, `#`, and emoji are removed so they are not spoken as symbol names.
- Text is capped at 300 characters to match Supertone's Text to Speech API limit.
- `클립보드 텍스트 테스트` reads the current clipboard text so you can test without dragging.

Supported model names in the settings menu follow the Supertone API docs: `sona_speech_1`, `sona_speech_2`, `sona_speech_2_flash`, `sona_speech_2t`, and `supertonic_api_1`.

## License / 라이선스

Right-click the tray icon and open `라이선스`.

- `라이선스 등록`: enter the web license server URL and the purchased license key.
- `라이선스 상태`: validates online when possible and falls back to the encrypted offline token.
- `이 PC 비활성화`: frees the current PC activation.
- License keys and offline tokens are saved per Windows user with Windows DPAPI encryption under `%APPDATA%\HanEnCursorIndicator`.
- The Windows app contains only the public API URL and never includes Paddle, Supabase, Toss, or email service secret keys.
- A paid license supports 2 PC activations and 14 days of offline use by default.

## Animation Effects

- Input-mode changes use a subtle pop animation.
- `point.png` appears for 1 second after the language state changes.
- `idle.png` appears while the mouse cursor is still.
- `cheer.png` appears while the mouse cursor is moving.
- State-specific files such as `ko-point.png` or `upper-cheer.png` override the shared pose image.
- Custom animated GIF poses keep their GIF animation.
- If no custom image is found, the app falls back to the default text badge.

## Features

- Shows `한` for Korean input mode.
- Shows `en` for English lowercase mode.
- Shows `EN` for English uppercase mode, including Caps Lock / Shift state.
- Humanoid minimi mascot with 3-image or 9-image packs.
- Optional custom PNG/JPG/JPEG/JFIF/BMP/GIF images.
- Tray menu on/off toggle.
- Tray menu image slot picker.
- Tray menu image reload.
- Tray menu label visibility toggle.
- Tray menu display mode: always follow or show only when the mouse is idle.
- Tray menu size presets and drag slider.
- Tray menu state + pose label-position drag editor.
- Tray menu mascot color picker.
- Tray menu label color picker and background-line cutout refinement.
- Selected-region local screen reading, answer bubbles up to 200 characters, and separate reading/voice hotkeys.
- CosyVoice reference-voice cloning with native Windows WAV playback.
- Tray menu Supertone voice settings with encrypted per-PC API key storage.
- Tray menu one-click Supertonic local voice setup: private Python runtime, `supertonic[serve]`, and the model, with no terminal.

## Monetization Direction

The first commercial flow is:

1. Customer buys a `Personal Lifetime` license through Paddle.
2. The web app receives Paddle `transaction.completed`.
3. The server creates a license key in Supabase.
4. The customer uses the key to download and activate the Windows app.
5. Microsoft Store registration is prepared after the web checkout flow is stable.

## Build

This project builds with the .NET Framework compiler included with Windows:

```bat
build.bat
```

The build output is `CursorImeIndicator.exe`. Commercial distributables are packaged outside GitHub.

## Demo GIF

The README GIF is a privacy-safe illustrated flow with a separately rendered **real settings panel** and an actual generated-audio waveform. It is not a desktop screen recording and does not imply instantaneous synthesis. Reviewed public inputs are under `assets/`; no personal desktop or settings are loaded by the GIF generator.

```bat
python -m pip install Pillow
python tools/create-readme-demo.py
```

The Node entry point `node tools/create-demo-gif.js` invokes the same Python generator; set `HANEN_DEMO_PYTHON` if Python is not on PATH. The output is 960×540, 26 seconds, looping, with no sound. [All-frame review sheet](assets/demo-review-contact-sheet.jpg)

## Notes

Because this is an unsigned personal executable, Windows SmartScreen may show a warning on first run.
