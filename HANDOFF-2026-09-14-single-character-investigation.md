# HanEnCursorIndicator 인수인계

작성일: 2026-09-14 (Asia/Seoul)
범위: 이 대화의 화면 읽기, 한 글자 답변, 호출 중단 분석 및 진단판 배포.
이 문서는 작업 규칙을 새로 만드는 AGENTS.md가 아니라 진행 기록이다.
문서 작성 때문에 앱을 재실행하거나 추가 검증하지 않았다. 아래 현재 상태는 마지막 관측 시점 기준이다.

## 1. 결론부터

**문제 해결 완료가 아니다.**

- 실제 모델 요청 두 건에서 전송 이미지 -> 모델 원문 -> 추출 문자열 -> 완료 이벤트 -> 말풍선 문자열/스크린샷을 연결했다. 그 두 건에서는 앱이 답변을 잘라내지 않았다.
- 가짜 서버가 한 글자 B를 반환하면 앱은 B를 유효한 답변으로 수락하고 표시한다. 이 경로는 직접 검증했다.
- 실제 요청 지침은 한국어 반말 두 문장인데, JSON 규격은 answer 문자열 길이 1~100을 허용한다. 설명 없는 답을 막거나 보완하는 처리는 아직 구현하지 않았다.
- 실제 모델에서 발생했던 한 글자 답변의 당시 원문과 이미지는 확보하지 못했다. 원인을 확정해서는 안 된다.
- 다음 증상을 기록하기 위한 기본 비활성 진단 래퍼를 소스에 추가하고 배포했다. **진단판이지 답변 문제 수정판이 아니다.**
- 9월 14일 로그에서 실제 한 글자 응답이 다시 확인됐다. 마지막 화면 읽기는 09:13:39 시작, 09:13:41 응답, 09:13:56 Idle 전환이다.
- 마지막 점검 당시 앱과 Ollama는 살아 있었지만 자동 화면 읽기는 Idle이었다. 누가/어떤 경로가 읽기를 중단했는지는 아직 확인하지 못했다.

## 2. 프로젝트와 변경 금지 사항

프로젝트: `C:\Users\chsjh\AIProjects\HanEnCursorIndicator`
소스: `CursorImeIndicator.cs` 단일 파일. csproj/MSBuild 없음.
실행 파일: 같은 폴더의 `CursorImeIndicator.exe`가 운영본이다.

- C# 5만 사용한다. 보간 문자열, ?. 및 nameof 금지.
- 소스의 한글은 TextResources 안의 대문자 Unicode escape로 유지한다.
- 소스 CRLF, BOM 없음, 기존 non-ASCII 바이트 수 보존. 이 한국어 Markdown 문서는 소스 인코딩 규칙의 대상이 아니다.
- 새 외부 라이브러리 금지, 새 abstraction 최대 1개, 수정 파일 최대 4개, 기존 아키텍처 유지.
- 이번 진단 구현의 운영 소스 변경은 CursorImeIndicator.cs 한 파일이다. 별도 TEMP 진단 프로그램의 보조 타입은 앱 아키텍처에 들어가지 않는다.
- images는 AppDomain.CurrentDomain.BaseDirectory 기준이다. 운영 exe만 다른 폴더로 옮기면 이미지가 사라질 수 있다.
- 사용자의 images/en-cheer.png, ko-cheer.png, upper-cheer.png 변경 및 images/extra 미추적 파일을 건드리지 않는다.
- 이 작업에서 Git 커밋/푸시/머지는 수행하지 않았다. 예전 브리핑의 main/원격 커밋 상태는 현재 상태로 간주하지 않는다.
- 터미널 드래그 복사 키 분류와 Windows Terminal의 ctrl+insert 설정은 이번 문제와 구분한다. 기존 해결을 되돌리지 않는다.

설정: `C:\Users\chsjh\AppData\Roaming\HanEnCursorIndicator\settings.ini`
로그: `%TEMP%\HanEnCursorIndicator\voice-debug.log`
로그 각 줄은 시각만 있고 날짜가 없다. 파일 수정 시각만으로 모든 줄의 날짜를 확정하지 말고 프로세스 시작 시각 등과 대조한다.

## 3. 마지막으로 직접 확인한 운영 상태

관측: 2026-09-14 18:48~18:51경. 이후 변경 여부는 확인하지 않았다.

- PID: 31416
- 시작: 2026-09-14 08:59:29
- Path: `C:\Users\chsjh\AIProjects\HanEnCursorIndicator\CursorImeIndicator.exe`
- Responding: True
- 운영 exe SHA256: `31F191421CB126E92F1563E4D533FAF8AD450AF665A20CD7F305D41A20574AFE`
- 이 해시는 9월 11일 검증/배포한 진단판과 같다.
- 최신 기록 상태: 18:45:41.402 automatic screen read state=Idle reason=
- 현재 보이는 앱 소유 창을 열거했을 때 제목 없는 창 하나였다. 설정 대화상자가 현재 열려 있다는 증거는 없었다.
- 당시 사용 가능 RAM 약 8.44 GB, OS 보고 FreeVirtualMemory 약 9.95 GB. 이 값만으로 과거 자원 부족 원인을 설명하지 않는다.

Ollama `/api/ps` 관측:

- 서버: `http://127.0.0.1:11434`
- 상주 모델: `qwen3.5:4b` 한 개
- digest: `2a654d98e6fba55d452b7043684e9b57a947e393bbffa62485a7aac05ee4eefd`
- GPU residency: 3,799,420,762 bytes
- context_length: 32768
- expires_at이 2318년으로 보고됐다. 이를 정상 만료 정책으로 해석하지 않았다.
- 9월 11일에는 context_length 4096, residency 약 3.07 GB였다. 현재 차이가 생긴 원인은 조사하지 않았다.
- 서버 응답 및 모델 상주는 앱이 현재 추론 요청 중이라는 뜻이 아니다.

### 9월 14일 주요 로그

09:10:48 시작 -> 09:10:56 최종 56글자
09:11:27 시작 -> 09:11:29 최종 57글자
09:12:00 시작 -> 09:12:03 최종 80글자
09:12:33 시작 -> 09:12:36 최종 82글자
09:13:06 시작 -> 09:13:08 최종 29글자
09:13:39 시작 -> 09:13:41 contentchars=15 / finalchars=1 / doneReason=stop / failureCode=NONE
09:13:41.310 Resting
09:13:56.325 Idle
17:58:02.265 ResourceLow
17:59:34.263 Idle
17:59:35.681 ResourceLow
18:45:41.402 Idle

중요: 앱은 08:59에 시작한 뒤 09:10부터 읽었다. 따라서 09:13의 중단을 재시작 때문이라고 설명하면 안 된다. 17시 이후 자원 부족도 오전 중단의 원인이라는 증거가 아니다.

## 4. 앞선 단계에서 반영된 변경

아래는 앞선 작업 요약과 검증 산출물에 기록된 구현이다. 이 문서 작성 때 소스 전체를 다시 검사하지 않았다.

### 안전 및 통합 처리

1. 상시 읽기 ON의 최초 요청도 OnScreenReadTick 경로로 보내 periodic 모델과 안전 검사를 사용한다. 최초 요청만 수동 대형 모델로 가는 경로를 제거했다.
2. 오래된/불건전한 자원 스냅샷은 회복 대기 시간을 초기화한다. 오래된 스냅샷으로 안전하다고 판단하지 않는다.
3. WebException의 Timeout을 TIMEOUT, 나머지 전송 실패를 API_ERROR로 분류한다. 명시적 취소는 우선한다.
4. GPU 조회 행의 형식 오류, 음수, overflow를 무시하고 넘어가지 않도록 했다.
5. 실제 보이는 마스코트/말풍선만 캡처 마스크에 넣도록 처리했다.
6. 읽을 영역 변경/해제 시 이전 요청, 말풍선, 음성, 관련 상태를 무효화한다.
7. 영역 설정 저장 실패 시 기존 영역 사전과 선택 키를 복원하는 트랜잭션 처리를 넣었다.

### 모니터와 영역 고정

- selectedReadRegionKey로 특정 모니터와 물리 ROI를 고정한다. 커서를 다른 모니터로 옮겨도 캡처 대상은 따라가지 않는다.
- 설정된 영역이 사라지거나 모니터 배치/DPI가 달라지면 정지한다. 다른 모니터 전체로 조용히 fallback하지 않는다.
- 설정을 한 번도 하지 않은 경우에만 기존 커서 모니터 전체 읽기 fallback을 허용한다.
- 예전 단일 영역 설정의 호환 처리, 다중 영역의 모호성, 명시적 해제 상태를 구분한다.
- 활성 영역 해제 시 나머지 모니터의 저장 영역을 자동 선택하지 않는다.

물리 모니터 관측:

- DISPLAY1: (0,0)-(2560,1600), DPI144
- DISPLAY5: (-1920,126)-(0,1206), DPI96
- DISPLAY6: (2560,0)-(4480,1080), DPI96

혼합 배율에서는 전역 1.5배 계산이나 Screen.Bounds의 무조건 사용이 위험하다. 배치/캡처 증거는 물리 좌표와 PMv2 기준으로 구분한다.

### 주기 읽기

- 자동 경로의 변화 감지 게이트를 제거했다. 화면이 안 바뀌었다는 이유로 수분간 아무 요청도 안 나가는 기존 동작을 바꿨다.
- 자원, 영역, 오류 정지, 진행 중 요청 검사는 유지한다.
- 30초 설정은 직전 요청 종료 후 다음 요청 시작까지의 최소 간격이다. 추론 시간까지 포함한 답변 간격이 정확히 30초라는 뜻이 아니다.
- 상태 변화 및 자동 요청 시작 대상 키를 로그에 남기도록 했다.
- **상시 읽기 ON 상태의 재시작 복원은 구현되지 않았다.**

## 5. 실제 모델 요청을 추적한 결과

실제 모델 추론은 기존 운영 앱을 잠시 멈춘 상태에서 한 건씩 실행했다. 추가 모델을 올리거나 앱과 검사 요청을 겹치지 않았다. 검사가 끝나면 기존 exe를 재시작했다.

### A. 현재 저장 영역, 실제 모델 1회

산출물:
`C:\Users\chsjh\AppData\Local\Temp\HanEn-real-trace-534e76202c9e4f22983674e0b3a9d0c3`

- 관측 시각: 9월 11일 17:29
- 저장 영역: DISPLAY1, X770 Y382 W1494 H986 (LTRB 770,382,2264,1368)
- 실제 전송 이미지: 1280x844 JPEG
- 커서는 오른쪽 모니터에 있었지만 지정된 주 모니터 영역을 전송했다.
- raw message.content 71글자 -> 추출 답변 56글자
- 추출 결과 = replyBox.Text = ScreenReadCompleted 값 = responseBubble.bubbleText
- visible-bubble.png를 직접 봤고 두 줄이 모두 표시됐다.
- 실제 답변은 A와 한국어 설명이었으나 존댓말을 사용해 반말 지침을 어겼다. 정답 정확성 통과를 의미하지 않는다.

### B. 과거 영역 좌표로 현재 화면 읽기, 실제 모델 1회

산출물:
`C:\Users\chsjh\AppData\Local\Temp\HanEn-old-roi-181fd947d473452993944944d3cb770f`

- 관측 시각: 9월 11일 17:31
- 적용 좌표: X812 Y521 W1414 H762 (LTRB 812,521,2226,1283)
- 실제 전송 이미지: 1280x689
- 저장된 사용자 영역은 바꾸지 않고 검사 요청의 로컬 bounds만 바꿨다.
- raw message.content 37글자 -> 추출 답변 23글자
- 답변: A, (나) Vt.T는 원래 U.T와 다름
- 추출/완료/말풍선 문자열과 화면 표시가 일치했다.
- 두 문장 지침을 충족하지 않았지만 앱은 수락했다.
- 과거 좌표에서 현재 화면을 읽었을 뿐이다. 당시 픽셀이 없으므로 과거 현상의 재현이라고 부르면 안 된다.
- 이미지 위쪽의 함수명/본문 일부와 아래쪽 보기가 잘려 있었다. 임의로 사용자의 영역을 확장하지 않았다.

각 폴더의 핵심 파일:

- request.json: 실제 전송 payload. 저장 지침과 이미지 base64가 들어 있어 민감할 수 있다.
- image.jpg: 그 payload에서 추출한 실제 이미지. 별도 재촬영본이 아니다.
- raw-response.json: 실제 로컬 서버 응답.
- trace.json: 요청/응답 해시, ROI, 문자열, 이벤트, 표시 상태.
- visible-bubble.png: 같은 검사에서 실제 보이는 말풍선.

**trace.json의 PASS는 전달 경로와 이미지 크기/문자열 일치 검사만 뜻한다. 답변의 수학적 정확성, 모든 지침 준수, 역사적 원인 해결을 의미하지 않는다.**

## 6. 실제 전송 규격에서 확인한 점

- model: qwen3.5:4b
- think: false
- temperature: 0
- num_predict: 384
- num_ctx: 4096 (9월 11일 캡처 요청 기준)
- answer 키 하나만 있는 JSON 객체를 요구한다.
- answer의 schema는 minLength=1, maxLength=100이다.
- 시스템 지침은 정답/이유를 한국어 반말 두 문장으로 요구한다.

한 글자도 규격을 만족한다는 점과 모델의 지침 미준수는 확인됐다. 그러나 이를 근거로 과거 모든 한 글자 응답이 모델 때문이었다고 확정할 수는 없다.
최소 글자 수만 올리거나 유효한 답을 숨기면 사용자는 여전히 답변을 못 받는다. 설명 보완 요청을 검토한다면 같은 이미지/기존 요청 수명/동일 deadline/기존 취소를 유지하고 최대 한 번만 직렬로 처리해야 한다. **이 보완 요청은 아직 설계 후보일 뿐 구현하지 않았다.**

## 7. 배포한 수동 활성화 진단 코드

소스 변경: CursorImeIndicator.cs 안의 CompanionChatForm.

- 원래 RequestJson 본문을 RequestJsonCoreForTrace로 이름만 변경하고 얇은 래퍼를 추가했다.
- 원래 FinishChat 본문을 FinishChatCoreForTrace로 이름만 변경하고 얇은 래퍼를 추가했다.
- 실제 서명은 RequestJson(string route, string payload, int requestId)이다. 중간에 서브에이전트가 요청 ID가 없다고 보고했으나 잘못이었고 정정했다.
- 기본 비활성. `%TEMP%\HanEnCursorIndicator\screen-trace.enable` 파일이 있을 때만 기록한다.
- 프로세스 전체에서 이미지 포함 요청 최대 2건을 기록한다. 카운터는 프로세스 재시작 때 초기화된다.
- 활성화 파일이 계속 남으면 이후 앱 재시작에서도 다시 두 건이 기록될 수 있다. 관찰 완료 후 정확한 marker 파일만 제거해 비활성화해야 한다.
- 외부 전송은 추가하지 않는다. 파일 기록 실패는 원래 앱 요청을 실패시키지 않도록 예외를 삼킨다.
- 요청 ID와 관측 generation을 혼동하지 않도록 관측값/불명 상태를 남긴다. 충돌 또는 세대 변경은 정상 연결이라고 주장하지 않는다.

출력 폴더:
`%TEMP%\HanEnCursorIndicator\screen-trace-<GUID>`

파일 계약:

| 파일 | 내용 |
| --- | --- |
| capture.txt | observedGeneration, monitorKey, captureBounds, association |
| request.json | 원본 메서드에 전달한 payload 그대로, 최대 4MB |
| raw-response.json | 원본 메서드가 반환한 문자열 그대로, 최대 1MB |
| transport-error.txt | 전송 예외 타입 이름 |
| transport-state.txt | observedBefore, observedAfter, ambiguousOrStale |
| finish-input.txt | requestId, textArgument=prompt, otherStringArgument=result, boolArgument=success |
| ui-observed.txt | requestId, uniqueCurrentGeneration, completedEventDuringCall, completedText, generation, replyBox, bubbleText, bubbleIsWaiting, visible, bounds |

크기 초과 시 일부를 잘라 쓰지 않고 해당 이름의 .omitted 파일을 쓴다.
문자열 값은 일반 텍스트이며 내부 줄바꿈을 escape하지 않는다. 단순 줄별 key=value 파싱은 모호할 수 있으므로 알려진 다음 키 경계로 읽거나 형식을 개선해야 한다.
ui-observed의 bounds는 관리 Bounds이며 물리 좌표라고 가정하면 안 된다.

정상 연결 증거의 조건:

- observedBefore == observedAfter == requestId
- ambiguousOrStale=False, uniqueCurrentGeneration=True
- completedEventDuringCall=True
- 모델에서 추출한 문자열, result, completedText, replyBox, bubbleText 일치
- visible=True만으로 성공 판정하지 않는다. 이전 말풍선일 수 있다.
- UI 파일 부재는 거절 증거가 아니다. 기록 실패 또는 연결 불명일 수 있다.

9월 11일에는 marker를 생성해 배포했으나 사용자의 ON 조작을 기다리는 동안 기록 폴더가 0건이었다. **9월 14일에는 요청 로그만 확인했고 생산 진단 폴더의 새 내용은 아직 열어보지 않았다. 다음 담당자가 우선 확인할 대상이다.**

## 8. 검사 기록과 범위

### 앞선 작업 요약에 기록된 검사

- 기본 통합 80 assertions.
- core 보강 89 checks.
- 영역/저장/마스킹 37 assertions.
- 고정 대상 109 또는 110 checks: 현재 커서 위치에 따라 native 추가 사례 수가 달랐다.
- 주기 읽기 20 checks.
- 마지막 주기 후보의 배치: 20 + 89 + 110 = 219 checks.
- 위 숫자를 모두 합쳐 고유 테스트 총수로 보고하지 않는다. 중복 실행/중복 커버리지가 있다.
- 일부 검사는 IL의 특정 호출 존재 여부만 확인하므로 실제 UI 통합 증거와 구분한다.

### 실제 전송 이미지의 영역 검증

`%TEMP%\HanEn-verify-20260911\wire-fixed-target-run1\primary`
`%TEMP%\HanEn-verify-20260911\wire-fixed-target-run1\left`

- 소유 패턴 창과 가짜 서버를 사용해 앱이 실제 보낸 이미지의 좌표/마스크를 검사했다.
- 주 모니터와 음수 좌표 왼쪽 모니터에서 1440x800 영역 -> 1280x711 전송을 확인했다.
- 주 모니터 검사 때 커서가 다른 모니터에 있었으므로 고정 대상 선택의 증거가 된다.
- 검사 이미지 일부에서 별도 운영 프로세스의 마스코트가 보였다. 검사 폼이 소유하지 않은 다른 프로세스의 창까지 마스킹됐다고 주장하지 않는다.

### 진단판 집중 검사: 이번 대화에서 실행/결과 확인

보고서:
`C:\Users\chsjh\AppData\Local\Temp\HanEn-trace-focused-1ad978394a0e4d5e85fb3d96d974489c\report.json`

- 가짜 서버 응답 B, 한국어 두 문장: 두 케이스 모두 PASS.
- B도 policyCode=NONE, 완료 이벤트, replyBox, bubbleText, Visible=True가 확인됐다.
- 실제 가짜 서버에 전달된 payload/응답과 진단 파일의 byte hash가 일치했다.
- 안정된 generation과 해당 완료 이벤트를 확인했다.
- 세 번째 기록 시도는 HTTP 없이 private gate 호출로 차단을 확인했다.
- marker 제거 후 gate가 아무 파일도 만들지 않는 것을 확인했다.
- 이 배치에 실제 모델/GPU 추론은 없었다. 빌드 1회, 집중 검사 1회, 수정 후 재검사 없음.

### 실패한 검사 하나

PowerShell의 ConvertTo-Json 결과를 reflection Invoke에 그대로 넘겨 PSObject를 System.String으로 바꿀 수 없다는 오류가 난 별도 보조 검사:
`%TEMP%\HanEn-verify-20260911\single-character-extraction-contract.json`

이 파일의 null/false 결과는 앱 실패가 아니다. 검사 인수 변환 실패이므로 판정에서 제외했다. 해당 보조 코드는 수정/재실행하지 않았다. 이후 별도 C# 가짜 서버 검사에서 B 전달 경로를 실제로 검증했다.

## 9. 도구, 산출물, 배포와 롤백

작업 폴더:
`C:\Users\chsjh\AppData\Local\Temp\HanEn-verify-20260911`

주요 스크립트:

- observe-live-bubble.ps1: 지정 PID의 보이는 앱 창을 PMv2 물리 좌표로 수동 입력 없이 캡처. 최대 40초.
- trace-real-screen-request.ps1: 실제 앱 어셈블리의 숨은 컨트롤러를 실행하고 loopback proxy로 실제 Ollama 요청/응답을 보관. 운영 앱 정지 확인, 이미 상주한 4B만 허용. 요청 1회, 90초/전체 110초 제한. HistoricalRoi 옵션은 저장 설정을 바꾸지 않는다.
- run-real-trace-safely.ps1: 정확한 PID/path 확인, exe/settings 백업, 검사, finally에서 기존 exe 재시작. ON 상태 복원을 보장하지 않는다.
- trace-focused.ps1: 격리 TEMP, 가짜 서버, 실제 앱 캡처 경로 및 추적 계약 검증. 2요청, 전체 75초 제한.
- verify-fixes.ps1, verify-region-fixes.ps1, verify-fixed-target.ps1, verify-periodic-read.ps1: 앞선 단계 검사.

보고서:

- single-character-investigation-status.md: 단계별 관측과 미완료 상태 누적 기록.
- verification-report.md, fixed-region-release-report.md: 이전 단계 기록. 최신 배포/현재 상태와 구분한다.
- periodic-live-observation.json: 종료 후 다음 요청까지 약 30초인 세 요청의 로그 증거. 답변 품질/표시 증거는 아니다.

주요 백업:

- 최신 진단판 직전 운영 exe/settings: `%TEMP%\HanEn-verify-20260911\rollback-trace-20260911-174516`
- 진단 수정 전 소스/실행 파일: `%TEMP%\HanEn-verify-20260911\before-trace-instrumentation-20260911-173658`
- 첫 실제 추적 전: `%TEMP%\HanEn-verify-20260911\before-real-trace-20260911-172925`
- 두 번째 실제 추적 전: `%TEMP%\HanEn-verify-20260911\before-real-trace-20260911-173147`
- 주기 읽기 수정 전: `%TEMP%\HanEn-verify-20260911\rollback-periodic-20260911-165510`
- 고정 영역 변경 전: `%TEMP%\HanEn-verify-20260911\rollback-fixed-target-20260911-164024`

진단판 SHA256:
`31F191421CB126E92F1563E4D533FAF8AD450AF665A20CD7F305D41A20574AFE`

진단판 직전 주기 읽기 운영본 SHA256:
`E7CD42C0C1370566FCDA254CC57493CB1A866A383E05E7F15F3C38233630C590`

안전 교체 절차:

1. 현재 PID와 정확한 exe 경로, 테스트 후보 해시를 확인한다. 위 PID는 재사용하지 않는다.
2. 기존 exe/settings를 새 롤백 폴더에 보관한다.
3. 후보를 repo 안의 고유 preflight 이름으로 복사해 기존 앱이 살아 있을 때 실행한다. 동일 인스턴스 mutex로 정상 종료되는지 확인한다.
4. 확인한 PID만 종료하고 후보를 repo의 원래 exe에 복사한다.
5. WorkingDirectory=repo, 숨김 창으로 시작한다. 초기 종료/해시/실행 경로를 확인한다.
6. 실패 시 보관한 exe로 복원한다. 사용자가 바꾼 최신 설정을 오래된 백업으로 무작정 덮지 않는다.

Smart App Control을 비활성화하거나 검증을 우회하지 않는다. exe와 함께 필요한 images/fonts 위치를 유지한다.

빌드 필수 참조:

.NET Framework v4.0.30319의 csc.exe, /langversion:5, /target:winexe, app.manifest, assets/app-icon.ico.
System.dll, System.Drawing.dll, System.IO.Compression.FileSystem.dll, System.Security.dll, System.Windows.Forms.dll.
/lib:<Framework>\WPF, UIAutomationClient.dll, UIAutomationTypes.dll, WindowsBase.dll.
후보는 먼저 TEMP에 별도 이름으로 빌드한다. 운영 exe를 바로 덮어 빌드하지 않는다.

## 10. 호출 중단 분석에서 새로 확인한 경로

9월 14일 운영 바이너리 해시가 진단판과 같음을 확인했고, 진단 추가 직전 소스 백업에서 다음 경로를 확인했다. 진단 래퍼는 이 영역을 수정하지 않았다.

- SetContinuousScreenRead(bool active)는 체크 상태와 스케줄러 SetEnabled(active)를 연결한다.
- 지침 편집 OnEditCompanionPrompt는 편집 전 IsEnabled를 저장하고 SetEnabled(false)를 호출한다. finally에서 `resume && continuousReadItem.Checked`로 복원한다.
- 말풍선 색상 선택도 같은 일시 정지/복원 패턴을 사용한다.
- 따라서 Idle 로그 하나만으로 사용자가 체크를 직접 껐다고 단정하면 안 된다.
- SetEnabled는 내부 enabled 값을 바꾸지만 호출자/이유를 기록하지 않는다.
- 확인한 복원 식 자체만으로 복원 버그가 증명된 것은 아니다. 어떤 경로가 실제 실행됐는지 증거가 필요하다.

참조한 백업 파일의 위치:

- 색상 선택 일시 정지/복원: 757, 772행 부근
- 지침 편집 일시 정지/복원: 2385~2405행 부근
- SetContinuousScreenRead: 2409행 부근
- ScreenReadScheduler.SetEnabled: 14932행 부근

위 행 번호는 백업 파일 기준이다. 진단 헬퍼가 추가된 현재 소스의 행 번호와 혼용하지 않는다.

## 11. 잘못 말한 부분과 정정

- '재시작 때문에 이번 읽기가 꺼졌다': 9월 14일 오전 중단에는 맞지 않는다. 프로세스가 재시작되지 않았다.
- '사용자가 상시 읽기를 껐다': 확인되지 않았다. 일시 정지/복원 경로도 있으므로 단정하지 않는다.
- '자원 부족 때문에 오전 요청이 멈췄다': 오후 자원 부족 기록으로 오전 원인을 설명할 수 없다.
- '진단 검사 PASS = 한 글자 문제 해결': 아니다. 전달/기록 기능만 통과했다.
- 'RequestJson에는 요청 ID 인수가 없다': 서브에이전트의 잘못된 보고였다. 실제 세 번째 인수는 requestId이다.
- '한 글자 응답은 앱이 잘라낸 것이다': 실제 두 요청에서 반박됐고, 과거 사례 자체는 아직 미확정이다.

## 12. 하위 에이전트와 진행 방식

- Copernicus: 안전/주기 처리 맥락, 진단 래퍼 작성.
  `01a08f4f-d058-7680-a446-5e65297fad90`
- Mendel: 실제 wire 추적과 집중 검사 도구 작성.
  `01a08f5e-e841-73e1-8a65-a430ee3eb80d`
- Hooke: 고정 모니터 선택/영역 관련 앞선 변경.
  `01a08f51-5b61-7df2-b386-714c57848b33`

9월 14일 Copernicus 재연결은 agent not found로 실패했다. 이전 agent가 아직 살아 있다고 가정하지 않는다. 새 세션에서는 이 인수인계와 실제 파일이 근거다.
메인이 후보 빌드, 실제 검사 실행, 결과 해석, 이미지 확인, 교체/롤백을 맡았다. 에이전트의 완료 보고만으로 기능 완료를 선언하지 않았다.

이전 goal은 9월 11일 상시 읽기 ON 조작 대기가 세 차례 이어져 blocked로 처리했다. 이후 9월 14일 사용자가 새로 호출 상태/원인을 물었고 그 범위의 읽기 전용 조사를 했다. 목표를 완료 처리한 적은 없다.

## 13. 다음 담당자가 할 일

1. 현재 PID/path/hash와 최신 로그를 한 번 확인한다. 날짜가 바뀌었으므로 위 PID나 모델 context를 현재값으로 가정하지 않는다.
2. **9월 14일 생성됐을 수 있는 운영 screen-trace 폴더를 우선 확인한다.** 이 대화에서는 9월 14일 해당 폴더 내용을 아직 읽지 않았다. 첫 두 요청만 기록되므로 requestId=6의 한 글자 원문이 들어 있다고 가정하지 않는다.
3. request.json에서 실제 이미지를 추출하고 raw-response -> 최종 문자열 -> UI 기록을 요청별로 비교한다. 서로 다른 요청이나 오래된 말풍선을 연결하지 않는다.
4. 호출 중단은 응답 길이 문제와 분리해서 조사한다. SetEnabled(false), 체크 변경, 지침/색상 설정 일시 정지/복원에 호출 이유와 사용자 의도/실제 실행 허용 상태를 기록할 지점을 정한다.
5. 중단 원인을 찾기 전에 또 재시작해 현재 상태를 지우거나 사용자에게 ON만 반복 요구하지 않는다. 정말 필요한 UI 조작만 한 번 요청한다.
6. 한 글자의 모델 원문이 확보되면 첫 실패 단계만 수정한다. 단순 길이 하한/무조건 숨김/설명 날조로 정상처럼 보이게 만들지 않는다.
7. 실제 수정 후 관련 검사 1회, 실패 시 승인된 범위의 수정 후 재확인 1회, 마지막 빌드/검증 1회라는 사용자 예산을 지킨다. 같은 통과 검사를 반복하지 않는다.
8. 실제 사용 및 재시작 후 동작까지 확인한다. 상시 읽기 ON 복원은 별도 미완료이므로 새로 구현하지 않고 완료라고 주장하지 않는다.
9. 관찰 완료 후 진단 marker를 제거해 민감한 캡처가 다음 재시작에도 남지 않도록 한다. 수집 파일은 사용자 승인 없이 외부 전송/커밋하지 않는다.

## 14. 이전 사용자 브리핑에만 있는 별도 범위

무료 폰트, 말풍선 색상/크기/꼬리, 서랍 통합 UI/단축키/상태 아이콘, 말풍선 음성, 드래그 음성, 설치 마법사와 모델 다운로드 등은 이 대화 이전부터 이어진 요구다. 이 문서는 그 항목 모두를 새로 검증한 완료 목록이 아니다.
현재 목표와 관계없는 기존 기능을 재구현하거나 사용자 이미지/설치를 정리하지 않는다. 설치/배포/Git에 관한 예전 승인도 현재 범위와 구분한다.

---
완료 기준: 검사 숫자가 아니라 사용자가 요구한 실제 상태와 동일 요청의 증거로 판단한다. 현재 남은 것은 한 글자 답변의 실제 원인/수정, 읽기 중단 경로의 원인, 실제 운영 재검증이다.