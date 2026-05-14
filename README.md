# HanEn Cursor Indicator

Windows tray app that shows the current Korean/English input mode next to the mouse cursor.
마우스 커서 바로 옆에 현재 입력 상태를 `한` / `en`으로 표시합니다.

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

## Features

- Shows `한` next to the cursor when Korean input mode is active.
- Shows `en` next to the cursor when English input mode is active.
- Can be toggled from the Windows tray menu.
- Double-click the tray icon to quickly turn the cursor indicator on or off.
- Exit from the tray menu.

## Build

This project builds with the .NET Framework compiler included with Windows:

```bat
build.bat
```

The build output is `CursorImeIndicator.exe`. The distributable copy is stored as:

```text
dist/HanEnCursorIndicator.exe
```

## Notes

Because this is an unsigned personal executable, Windows SmartScreen may show a warning on first run.

## Demo GIF

The README animation is generated without external packages:

```bat
node tools/create-demo-gif.js
```
