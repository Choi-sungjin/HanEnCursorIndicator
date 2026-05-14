# Custom Indicator Images

Put optional custom images in this folder when you want to replace the default text badges.
This folder must be next to the executable you are running.

For the repository build, the expected location is:

```text
dist/images/
```

Supported names:

```text
han.gif
han.png
han.jpg
han.jpeg
han.bmp

en.gif
en.png
en.jpg
en.jpeg
en.bmp
```

The app looks for files in this order: GIF, PNG, JPG, JPEG, BMP.

Tips:

- Use transparent PNG files for clean static badges.
- Use animated GIF files for moving badges.
- 32px to 64px square images work best.
- Right-click the tray icon and choose "이미지 폴더 열기" to open this folder.
- After changing files while the app is running, use the tray menu item "커스텀 이미지 다시 불러오기".
- The app shows a tray notification when reload finishes.
