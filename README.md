# HanEn Cursor Indicator

Windows-only tray app that shows the current Korean/English input mode next to the mouse cursor.
마우스 커서 바로 옆에 현재 입력 상태를 `한` / `en`으로 표시하는 Windows 전용 앱입니다.

![HanEn Cursor Indicator demo](assets/demo.gif)

## Usage Example / 사용 예시

1. `dist/HanEnCursorIndicator.exe`를 실행합니다.
2. 설치 과정 없이 바로 실행되고 Windows 트레이 아이콘이 추가됩니다.
3. 한글 입력 상태에서는 커서 옆에 `한`이 표시됩니다.
4. 영어 입력 상태에서는 커서 옆에 `en`이 표시됩니다.
5. 트레이 아이콘 메뉴에서 커서 표시를 켜거나 끌 수 있습니다.

## Download

Run the single executable:

```text
dist/HanEnCursorIndicator.exe
```

No installer is required. The app starts immediately and adds a tray icon.
기본 사용은 exe 파일 하나만 실행하면 됩니다.

## Windows Support

- Windows 10 / Windows 11
- No separate installer required
- Built with the .NET Framework compiler included on Windows

## Custom Images / 이미지 추가

The app still works as a single exe by default. If you want custom badges, create an optional `images` folder next to the exe:
기본은 exe 하나만 실행하면 됩니다. 커스텀 이미지를 쓰고 싶을 때만 exe 옆에 `images` 폴더를 만들면 됩니다.

```text
dist/
  HanEnCursorIndicator.exe
  images/
    han.png
    en.png
```

Supported file names:

```text
images/han.gif
images/han.png
images/han.jpg
images/han.jpeg
images/han.bmp

images/en.gif
images/en.png
images/en.jpg
images/en.jpeg
images/en.bmp
```

The app searches in this order: GIF, PNG, JPG, JPEG, BMP.
중요: 이미지는 실행 중인 exe 바로 옆 `images` 폴더에서 찾습니다. 이 저장소 기준으로는 `dist/images/`입니다.

Tips:

- Right-click the tray icon and choose `이미지 폴더 열기` to open the correct folder.
- Put your files there as `han.png` / `en.png` or `han.gif` / `en.gif`.
- Right-click the tray icon and choose `커스텀 이미지 다시 불러오기`.
- The app shows a tray notification with the number of loaded custom images.
- Use transparent PNG files for clean static badges.
- Use animated GIF files for moving badges.
- 32px to 64px square images work best.
- After changing images while the app is running, use the tray menu item `커스텀 이미지 다시 불러오기`.

If nothing changes:

- Check that the app you are running is `dist/HanEnCursorIndicator.exe`.
- Check that images are inside `dist/images/`.
- Check the exact file names: `han` for Korean, `en` for English.
- Windows notifications may hide the reload balloon, but the image is still reloaded.

## Animation Effects

- Input-mode changes use a subtle pop animation.
- Custom animated GIF badges keep their GIF animation.
- If no custom image is found, the app falls back to the default `한` / `en` text badge.

## Features

- Shows `한` next to the cursor when Korean input mode is active.
- Shows `en` next to the cursor when English input mode is active.
- Optional custom PNG/JPG/BMP/GIF badge images.
- Animated GIF badge support.
- Can be toggled from the Windows tray menu.
- Double-click the tray icon to quickly turn the cursor indicator on or off.
- Exit from the tray menu.

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

The README animation is generated without external packages:

```bat
node tools/create-demo-gif.js
```

## Notes

Because this is an unsigned personal executable, Windows SmartScreen may show a warning on first run.
