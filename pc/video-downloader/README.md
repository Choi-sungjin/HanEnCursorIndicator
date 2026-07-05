# Video Downloader (Windows PC)

Windows desktop app that saves the video from a public Instagram / TikTok /
X (Twitter) post link to `내 비디오(Videos)\VideoDownloader`.

Instagram / TikTok / X(트위터)의 **공개** 게시물 링크를 붙여넣으면 동영상을
`내 비디오(Videos)\VideoDownloader` 폴더에 저장하는 Windows 프로그램입니다.
Android 버전은 [`android/video-downloader/`](../../android/video-downloader/)에 있습니다.

## Build / 빌드

Windows 10/11에 기본 포함된 .NET Framework 컴파일러만 사용하므로 아무것도
설치할 필요가 없습니다. 이 폴더에서 실행:

```bat
build.bat
```

빌드 결과물은 `VideoDownloader.exe`입니다. 더블클릭으로 바로 실행됩니다
(설치 과정 없음).

## Usage / 사용법

1. `VideoDownloader.exe` 실행
2. Instagram / TikTok / X 앱이나 웹에서 게시물 링크 복사
3. `붙여넣기` 버튼 클릭 (또는 직접 입력)
4. `다운로드` 클릭
5. 완료되면 `저장 폴더 열기`로 저장된 mp4 확인

지원 링크 형식:

```text
https://www.instagram.com/reel/XXXX/        (릴스)
https://www.instagram.com/p/XXXX/           (게시물 동영상)
https://www.tiktok.com/@user/video/1234     (틱톡)
https://vm.tiktok.com/XXXX/                 (틱톡 공유 단축 링크)
https://x.com/user/status/1234              (X)
https://twitter.com/user/status/1234        (구 트위터 주소)
```

## Important limitations / 중요한 제한 사항

- **공개 게시물만 지원합니다.** 로그인 기능이 없으므로 비공개 계정, 보호된
  트윗, 인스타그램 스토리는 다운로드할 수 없습니다.
- 이 플랫폼들은 "동영상 파일을 주는" 공식 API를 제공하지 않으므로, 이 앱은
  공개적으로 알려진 방법(인스타그램 임베드 페이지의 `og:video` 태그, 틱톡
  페이지에 포함된 JSON + tikwm.com 미러 API 폴백, X는 오픈소스
  vxtwitter.com 미러)을 사용합니다. 플랫폼이 페이지 구조를 바꾸면 언제든
  동작이 멈출 수 있습니다.
- 각 플랫폼의 이용약관과 원작자의 저작권을 지켜 주세요. 본인 게시물이거나
  저장 권한이 있는 콘텐츠만 다운로드하세요.
- 이 코드는 오프라인 샌드박스에서 작성되어 Mono C# 컴파일러(C# 5 모드)로
  컴파일 검증까지는 마쳤지만, 실제 Instagram/TikTok/X 접속 테스트는 하지
  못했습니다. 문제가 생기면 위의 "페이지 구조 변경" 가능성부터 의심하세요.
- 서명되지 않은 개인 실행 파일이므로 처음 실행할 때 Windows SmartScreen
  경고가 표시될 수 있습니다.
