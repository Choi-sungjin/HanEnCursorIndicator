# Custom Indicator Images

This folder sits next to the executable and contains the optional mascot pose images.

The app uses exactly three pose files:

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

Supported formats:

```text
idle.gif / idle.png / idle.jpg / idle.jpeg / idle.bmp
point.gif / point.png / point.jpg / point.jpeg / point.bmp
cheer.gif / cheer.png / cheer.jpg / cheer.jpeg / cheer.bmp
```

Tips:

- Transparent PNG files work best.
- Animated GIF files are supported.
- Leave a blank face area for the app to draw the input label.
- Right-click the tray icon and choose "이미지 폴더 열기" to open this folder.
- After changing files while the app is running, choose "커스텀 이미지 다시 불러오기".
- Use the tray menu "크기" for preset sizes or "드래그로 크기 조정" for mouse slider resizing.
- Use the tray menu "얼굴 중심 조정" to drag the label point separately for Idle, Point, and Cheer.
- Use the tray menu "미니미 색상" to choose base, Korean, and English clothing colors.
