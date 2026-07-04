# Video Downloader (Android)

Android app that saves the video from a public Instagram / TikTok / X (Twitter)
post link to your device's `Movies/VideoDownloader` folder. Paste a link, or
share a link into the app from the Instagram/TikTok/X app's share sheet.

이 앱은 Instagram / TikTok / X(트위터)의 **공개** 게시물 링크를 붙여넣으면 동영상을
기기의 `동영상(Movies)/VideoDownloader` 폴더에 저장합니다. 각 앱의 공유 시트에서
이 앱으로 링크를 공유해도 동작합니다.

## Important limitations / 중요한 제한 사항

- **Public posts only.** There is no login flow, so private accounts, protected
  tweets, and Instagram Stories are not supported.
- These platforms don't offer an official "get me the video file" API, so this
  app relies on public techniques (an `og:video` tag on Instagram's embed page,
  TikTok's own page data, and the open-source `vxtwitter.com` mirror for X).
  All of these can break whenever the platform changes its page structure —
  if a download stops working, that's most likely why.
- Respect each platform's Terms of Service and the original creator's rights.
  Only download content you have the right to save (your own posts, or posts
  you have explicit permission to keep a copy of).
- This app was written and reviewed as source code but **not build-tested**
  in the environment that produced it (no Android SDK / no network access to
  Instagram, TikTok, or X from that sandbox). Build and test it locally before
  relying on it.

## Build

Requires [Android Studio](https://developer.android.com/studio) (Jellyfish or
newer) with an installed Android SDK (compileSdk 34), or the command line with
`ANDROID_HOME` set.

1. Open the `android/video-downloader` folder in Android Studio and let it
   sync Gradle (it will generate the Gradle wrapper for you on first sync).
2. Build > Build Bundle(s) / APK(s) > Build APK(s), or run:
   ```bash
   ./gradlew assembleDebug
   ```
   (Run `gradle wrapper` once first if `gradlew` isn't present yet — it wasn't
   possible to generate the wrapper in the offline sandbox this was built in.)
3. The APK is written to `app/build/outputs/apk/debug/app-debug.apk`.

## Install on your phone

1. Copy `app-debug.apk` to your Android phone (USB, cloud drive, etc.).
2. On the phone, enable "Install unknown apps" for the app you use to open the
   APK (Settings > Apps > Special access > Install unknown apps).
3. Tap the APK file and install.
4. Open the app, paste a public Instagram/TikTok/X link, and tap 다운로드
   (Download). Saved videos appear in your gallery app under
   `Movies/VideoDownloader`.

## Project layout

```
android/video-downloader/
  app/src/main/java/com/hanen/videodownloader/
    MainActivity.kt          Compose UI + share-intent handling
    DownloadViewModel.kt      UI state machine (idle/extracting/downloading/done/error)
    extractor/                One VideoExtractor per platform + registry
    network/                  Shared OkHttp client
    download/                 Streams the resolved URL into MediaStore
  app/src/test/              Offline unit tests for URL matching
```

Minimum supported Android version: **Android 10 (API 29)**, chosen so the app
can save files with `MediaStore` scoped storage and doesn't need the
`WRITE_EXTERNAL_STORAGE` permission at all.
