# HanEn Cursor Indicator

Windows-only tray app that shows the current Korean/English input mode next to the mouse cursor.
마우스 커서 바로 옆에 현재 입력 상태를 `한` / `en` / `EN`으로 표시하는 Windows 전용 앱입니다.

![HanEn Cursor Indicator demo](assets/demo.gif)

## Usage Example / 사용 예시

1. `dist/HanEnCursorIndicator.exe`를 실행합니다.
2. 설치 과정 없이 바로 실행되고 Windows 트레이 아이콘이 추가됩니다.
3. 한글 입력 상태에서는 미니미 얼굴에 `한`이 표시됩니다.
4. 영어 소문자 입력 상태에서는 `en`, 대문자 입력 상태에서는 `EN`이 표시됩니다.
5. 입력 상태가 바뀌면 미니미가 1초 동안 마우스를 가리킨 뒤 정자세로 돌아옵니다.
6. 일정 주기마다 만세 포즈가 표시됩니다.

## Download

Run the executable:

```text
dist/HanEnCursorIndicator.exe
```

The app still works with only the exe. If `dist/images/` is included, the humanoid minimi mascot images are used automatically.

## Windows Support

- Windows 10 / Windows 11
- No separate installer required
- Built with the .NET Framework compiler included on Windows

## Included Mascot Images

The default image pack uses three shared pose files:

| Pose | File | Behavior |
| --- | --- | --- |
| Idle | `dist/images/idle.png` | Normal standing pose |
| Point | `dist/images/point.png` | Shown for 1 second after input mode changes |
| Cheer | `dist/images/cheer.png` | Shown periodically |

The app draws `한`, `en`, or `EN` on the mascot face at runtime, so the basic pack only needs three pose images.

## Character Concepts

See [`list/`](list/) for 13 original mascot concept images and an animated preview GIF.
Ready-to-use 3-pose packs are in [`list/packs/`](list/packs/).

## Custom Images / 이미지 추가

To replace the mascot, put images next to the exe. You can use either a 3-image shared pack or a 9-image state pack.

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
images/idle.bmp

images/point.gif
images/point.png
images/point.jpg
images/point.jpeg
images/point.bmp

images/cheer.gif
images/cheer.png
images/cheer.jpg
images/cheer.jpeg
images/cheer.bmp

images/ko-idle.png
images/en-idle.png
images/upper-idle.png
```

The app searches in this order: GIF, PNG, JPG, JPEG, BMP.

Tips:

- Use transparent PNG files for clean static mascot poses.
- Use animated GIF files if you want a moving pose.
- With a 3-image pack, leave a blank face area; the app draws `한`, `en`, or `EN` automatically.
- With a 9-image pack, you can include the text directly in each image and turn off `글자 표시` from the tray menu.
- Right-click the tray icon and choose `이미지 폴더 열기` to open the correct folder.
- After changing files, choose `커스텀 이미지 다시 불러오기`.
- Choose `이미지 누끼 처리` from the tray menu to select one or more images and save transparent `*-cutout.png` copies. It samples the outer edge color to remove connected backgrounds, and the default option shrinks large images to a lightweight 160px app-ready PNG.

## Size Control / 크기 조정

Right-click the tray icon and open `크기`.

- Choose a preset: `50%`, `75%`, `100%`, `125%`, `150%`, `200%`, `250%`.
- Choose `드래그로 크기 조정` to open a slider.
- Drag the slider with the mouse to tune the size gain by percentage.
- The selected percentage is saved and reused next time.

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
- The face label stays readable while the body/clothing area is recolored.

## Animation Effects

- Input-mode changes use a subtle pop animation.
- `point.png` appears for 1 second after the language state changes.
- `idle.png` returns after the point animation.
- `cheer.png` appears periodically.
- State-specific files such as `ko-point.png` or `upper-cheer.png` override the shared pose image.
- Custom animated GIF poses keep their GIF animation.
- If no custom image is found, the app falls back to the default text badge.

## Features

- Shows `한` for Korean input mode.
- Shows `en` for English lowercase mode.
- Shows `EN` for English uppercase mode, including Caps Lock / Shift state.
- Humanoid minimi mascot with 3-image or 9-image packs.
- Optional custom PNG/JPG/BMP/GIF images.
- Tray menu on/off toggle.
- Tray menu image reload.
- Tray menu label visibility toggle.
- Tray menu size presets and drag slider.
- Tray menu state + pose label-position drag editor.
- Tray menu mascot color picker.

## MVP Direction

This can grow into a small Windows utility MVP with:

- Custom image packs
- Startup-on-boot option
- Simple settings UI
- GitHub Releases download page
- Signed executable for fewer SmartScreen warnings

## Build

This project builds with the .NET Framework compiler included with Windows:

```bat
build.bat
```

The build output is `CursorImeIndicator.exe`. The distributable copy is stored as:

```text
dist/HanEnCursorIndicator.exe
```

## Demo GIF

The README animation uses the current minimi mascot pose images from `dist/images/` and is generated without external packages:

```bat
node tools/create-demo-gif.js
```

## Notes

Because this is an unsigned personal executable, Windows SmartScreen may show a warning on first run.
