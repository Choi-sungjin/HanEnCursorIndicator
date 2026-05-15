# Custom Indicator Images

This folder sits next to the executable and contains the optional mascot pose images.

The app supports a 3-image shared pack:

```text
idle.png
point.png
cheer.png
```

Behavior:

- `idle.png`: normal standing pose.
- `point.png`: shown for 1 second after the input mode changes.
- `cheer.png`: shown periodically.

The app draws the current input label on the face automatically:

- Korean mode: `한`
- English lowercase mode: `en`
- English uppercase mode: `EN`

You can also use a 9-image state pack:

```text
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

The app state names are `ko`, `en`, and `EN`. `EN-idle.png`, `EN-point.png`, and `EN-cheer.png` are accepted too, but Windows folders are usually case-insensitive, so `upper-*` is safer when `en-*` files are also present.

Supported formats:

```text
idle.gif / idle.png / idle.jpg / idle.jpeg / idle.bmp
point.gif / point.png / point.jpg / point.jpeg / point.bmp
cheer.gif / cheer.png / cheer.jpg / cheer.jpeg / cheer.bmp
ko-idle.gif / ko-idle.png / ko-idle.jpg / ko-idle.jpeg / ko-idle.bmp
en-idle.gif / en-idle.png / en-idle.jpg / en-idle.jpeg / en-idle.bmp
upper-idle.gif / upper-idle.png / upper-idle.jpg / upper-idle.jpeg / upper-idle.bmp
```

Tips:

- Transparent PNG files work best.
- Animated GIF files are supported.
- Leave a blank face area for the app to draw the input label when using a 3-image pack.
- Turn off "글자 표시" if your 9-image pack already includes the label in each image.
- Right-click the tray icon and choose "이미지 폴더 열기" to open this folder.
- After changing files while the app is running, choose "커스텀 이미지 다시 불러오기".
- Use the tray menu "크기" for preset sizes or "드래그로 크기 조정" for mouse slider resizing.
- Use the tray menu "글자 위치 조정" to drag the label point separately for `ko` / `en` / `EN` and Idle / Point / Cheer.
- Use the tray menu "미니미 색상" to choose base, Korean, English lowercase, and English uppercase clothing colors.
