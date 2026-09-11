# HanEnCursorIndicator 사용 가이드

마우스 옆 이미지로 한글/영문 입력 상태를 표시하고, 현재 모니터의 화면을 로컬 AI로 읽어 말풍선과 음성으로 전달하는 Windows 트레이 앱입니다. 선택한 텍스트를 읽는 드래그 음성 기능도 제공합니다.

## 준비와 실행

### 첫 실행에서 로컬 AI 선택 설치

EXE를 처음 실행하면 설치 안내가 나타납니다. 두 항목은 기본 선택 해제 상태이며 각각 동의할 수 있습니다.

| 선택 항목 | 설치 대상 | 사용하는 기능 |
| --- | --- | --- |
| 로컬 음성 | Supertonic 3와 전용 Python 환경·음성 모델 | 드래그 텍스트와 말풍선 답변 읽기 |
| 로컬 LLM | 필요한 경우 Ollama, 그리고 `qwen3.5:4b` | 커서가 있는 모니터의 화면 분석과 말풍선 답변 |

1. 필요한 항목만 선택하고 **선택한 항목 설치 / 점검**을 누릅니다.
2. 처음 다운로드할 때는 인터넷과 충분한 디스크 공간이 필요합니다. Ollama와 모델은 수 GB 이상의 공간을 사용할 수 있습니다. 다운로드 중에는 진행 상태를 확인합니다.
3. Ollama가 없다면 공식 설치 파일을 내려받아 서명과 게시자를 확인한 뒤 실행합니다. 별도의 공식 설치 창이 나타나면 그 창에서 설치를 마칩니다. 보안 경고나 설치 확인을 강제로 우회하지 않습니다.
4. 기존 설치와 모델은 재사용합니다. 모델 다운로드 후에는 로컬 모델의 준비 상태와 이미지 지원을 확인합니다. 설치 창이 닫혔다는 이유만으로 완료 처리하지 않습니다.
5. 하나가 실패해도 다른 항목의 결과는 유지됩니다. 실패한 항목은 **서랍 → 로컬 AI 설치 / 점검**에서 다시 선택할 수 있습니다.

**지금은 건너뛰기**를 눌러도 이미지 기능은 사용할 수 있습니다. 최초 안내를 이미 표시한 상태는 저장되므로 다음 실행마다 반복해서 묻지 않습니다. 설치 후에도 화면 상시 읽기와 말풍선 음성은 자동으로 켜지지 않으며, 사용자가 해당 메뉴에서 켭니다.

취소는 앱이 진행하는 다운로드·모델 요청을 대상으로 합니다. 이미 열린 공식 Ollama 설치 창은 그 창에서 직접 취소하거나 닫아야 합니다. 기존 Ollama 서버, 다른 모델과 사용자 설정은 삭제하거나 종료하지 않습니다.

설치 방식과 요구 사항의 공식 안내: [Ollama Windows](https://docs.ollama.com/windows), [Windows 다운로드](https://ollama.com/download/windows), [qwen3.5:4b](https://ollama.com/library/qwen3.5%3A4b).

### 파일 배치와 수동 준비

1. 기본 설치와 음성 엔진 준비는 [README](README.md)를 참고합니다.
2. 소스에서 빌드할 때는 Windows에서 `build.bat`을 실행합니다. 실행 중인 EXE를 바로 덮어쓰지 말고 기존 파일을 백업한 뒤 교체합니다.
3. `CursorImeIndicator.exe` 옆에 `images`와 `fonts` 폴더를 함께 둡니다. EXE만 옮기면 이미지나 번들 폰트가 표시되지 않을 수 있습니다.
4. 화면 읽기는 로컬 Ollama와 `qwen3.5:4b` 모델이 필요합니다. `ollama list`로 모델을 확인하고, Ollama가 `http://127.0.0.1:11434`에서 실행 중인 상태로 둡니다.
5. EXE를 실행하고 알림 영역의 앱 아이콘을 우클릭하면 설정 메뉴인 **서랍**이 열립니다.

화면 읽기 모델과 음성 엔진은 서로 다릅니다. Ollama는 화면 답변을 만들고, 음성 메뉴에서 선택한 엔진은 그 답변을 소리로 읽습니다. 화면 읽기에는 클라우드 모델을 사용하지 않습니다. 음성의 처리 위치는 선택한 음성 엔진에 따릅니다.

## 서랍 구성

| 묶음 | 설정과 기능 |
| --- | --- |
| 이미지 | 표시 켜기/끄기, 입력 상태, 이미지 선택·폴더·다시 불러오기, 배경 제거, 크기, 표시 방식, 색상, 라벨, 얼굴 중심, 이미지 단축키 |
| 말풍선 | 화면 한 번 읽기, 상시 읽기, 정지·숨기기, 읽을 영역 지정·해제, 현재 말풍선 닫기, 읽기 간격, 답변 표시 시간, 상시 읽기 모델, 답변 지침, 글꼴·글자 크기·색상, 화면 읽기 단축키, 말풍선 음성 읽기·전용 단축키, 영역 읽기 단축키 |
| 음성 | 드래그 읽기, 엔진·목소리·속도 설정, 음성 정지, 드래그 음성 단축키 |

라이선스와 앱 종료는 기능 설정과 별도로 배치되어 있습니다.

## 단축키 등록

각 기능의 단축키 설정에서 입력 칸을 선택하고 원하는 조합을 누른 뒤 **저장**합니다. Ctrl 또는 Alt가 포함된 조합을 사용합니다. 지우기를 누른 뒤 저장하면 해당 키를 해제합니다.

| 대상 | 켜기/끄기 키 | 정지 키 |
| --- | --- | --- |
| 이미지 | 이미지 표시 전환 | 이미지 숨김 |
| 말풍선 화면 읽기 | 상시 화면 읽기 전환 | 화면 읽기 취소, 말풍선 숨김, 해당 음성 취소 |
| 드래그 음성 | 드래그 읽기 활성화 전환 | 드래그 음성 재생·대기 작업 정지 |
| 말풍선 음성 | 답변 음성 읽기 전환 | 말풍선 음성만 정지; 켜짐 설정은 유지 |
| 영역 읽기 | 지금 한 번 읽기 | 현재 말풍선만 닫기; 읽기와 음성 설정은 그대로 |

추가된 이미지·말풍선·말풍선 음성 단축키의 기본값은 미지정입니다. 기존 드래그 음성 설정은 유지됩니다. 기능 간 같은 조합을 중복 등록할 수 없으며, 다른 앱이 사용하는 키는 등록에 실패할 수 있습니다. 실패 안내가 나오면 다른 조합을 지정합니다.

## 말풍선 음성 사용

1. **서랍 → 말풍선 → 음성 읽기**를 켭니다. 기본값은 OFF이고 선택한 상태는 저장됩니다.
2. 화면을 한 번 읽거나 상시 읽기를 켭니다. 성공한 최종 답변이 말풍선에 표시되면 해당 답변을 읽습니다.
3. 목소리·속도·엔진은 기존 **음성** 설정을 공유합니다. 드래그 읽기가 꺼져 있어도 말풍선 음성은 사용할 수 있습니다.
4. 말풍선 음성을 끄면 해당 재생과 대기 작업만 취소합니다. 드래그 읽기와 말풍선 표시는 그대로 유지됩니다.

처리 중 안내나 오류 문구는 읽지 않습니다. 합성 중 정지하면 결과를 폐기하지만, 합성 호출 자체가 즉시 끝나는 것은 아니므로 다음 음성이 시작될 때까지 잠시 기다릴 수 있습니다. 재생은 Windows 내장 MCI를 사용하며, 재생 완료까지 기다리는 동안에도 해당 출처의 음성을 정지할 수 있도록 구성했습니다. 정지에는 장치의 오디오 버퍼에 따른 짧은 지연이 있을 수 있습니다.

### 답변에 지침이 반복되는 경우

화면 읽기 요청에서는 저장한 답변 지침과 실제 화면 분석 요청을 분리합니다. 모델이 지침을 긴 문장으로 복사한 것으로 감지되면, 그 응답은 말풍선에 표시하거나 음성으로 읽지 않고 트레이 알림으로 안내합니다. 이전 정상 말풍선은 유지하며 자동 재시도하지 않습니다.

짧은 인용이나 일반 답변은 허용하지만 반복 검사는 문자열 기반 보조 장치이므로 모든 오답을 판별하지는 못합니다. 정상 응답이 차단되거나 같은 문제가 반복되면 지침을 간결하게 조정하고 화면을 한 번 다시 읽습니다. 저장한 지침을 앱이 임의로 고치지는 않습니다.

## 사용

1. Ollama에서 기본 모델 `qwen3.5:4b`를 사용할 수 있는 상태로 둔다.
2. **서랍 → 말풍선 → 화면 읽고 말풍선으로 보기**를 누른다.
3. 클릭 당시 커서가 있는 모니터를 한 번 읽고, 답변만 마스코트 근처 말풍선으로 보여준다.
4. **말풍선 글자 크기**에서 8~32pt로 조절한다. 설정은 저장되며 말풍선 크기도 바뀐다.

입력창, 대화 기록창, 처리 중 문구는 표시하지 않는다. 실패할 때만 트레이 알림으로 알려준다.
트레이의 상시 화면 읽기 토글을 켜면 즉시 한 번 읽고, 읽기가 끝난 뒤 설정한 간격만큼 쉬었다가 다시 읽는다. 간격은 20~120초에서 5초 단위로 고르며 기본값은 60초다. 고정 주기가 아니라 **직전 읽기가 끝난 뒤의 최소 대기 시간**이다. 처리 중에는 중복 요청하지 않는다. 끄면 진행 요청을 취소하며, 앱 재시작 시에는 꺼짐으로 시작한다.
간격이 지나도 화면이 그대로면 읽지 않는다. 2초마다 영역을 가로 64픽셀로 줄여 비교하고, **연속 두 번** 의미 있는 차이가 확인될 때만 읽는다. 창 하나가 스쳐 지나가는 정도는 한 프레임이라 모델을 부르지 않는다.
요청이 도는 동안에는 말풍선에 `기다려줘`를 표시한다. 그 문구는 음성으로 읽지 않고 답변 기록에도 들어가지 않는다. 정상 답변은 표시된 순간부터 세어 10~60초(기본 20초) 뒤에 사라진다. **현재 말풍선 닫기**는 표시만 숨기고 추론·상시 읽기·음성 설정은 건드리지 않는다.
말풍선은 클릭과 포커스를 가로채지 않으며 새 답변이 올 때까지 유지된다. 읽기를 끄면 진행 중 요청을 취소하고 말풍선도 숨긴다. 다음 캡처에서는 기존 말풍선 영역을 가려 자기 답변을 다시 읽지 않게 한다.
이전 대화 내용은 화면 읽기 요청 사이에 전달하지 않는다.

## 화면과 모델

- 트레이 화면 읽기는 클릭 당시 커서가 있는 모니터를 한 번 캡처한다. 그 모니터에 지정한 영역이 있으면 그 사각형만 캡처한다.
- 캡처에서는 말풍선과 마스코트를 모두 가린다. 자기 답변과 자기 그림을 다시 읽지 않고, 마스코트의 움직임이 화면 변화로 잡히지도 않는다.
- 화면은 긴 변 1280픽셀 이내의 JPEG로 줄여 메모리에서 로컬 모델에 전달한다.
- 화면 파일과 대화 기록을 앱이 디스크에 저장하지 않는다. 화면 읽기마다 이전 문답을 비운다.
- 요청 주소는 `http://127.0.0.1:11434`로 고정한다. 프록시와 HTTP 리다이렉트는 사용하지 않는다.
- 모델의 이미지 지원을 확인한 뒤 화면을 전달한다.
- 모델은 도구를 실행하거나 컴퓨터를 조작하지 않는다. 화면 문장은 관찰 자료로 취급한다.
- 요청은 한 번에 하나이며 자동 재시도하지 않는다. 사용자가 요청한 화면 읽기의 전체 제한은 180초, 상시 읽기는 60초, 일반 대화는 60초다.
- 모델을 용도별로 나눈다. 사용자가 요청한 읽기는 `companionModel`에 지정한 모델을, 상시 읽기는 더 작은 `periodicModel`(기본 `qwen3.5:4b`)을 쓴다.
- `OLLAMA_MAX_LOADED_MODELS=1` 환경에서는 두 모델이 동시에 상주할 수 없다. 그래서 상시 모델은 `keep_alive`를 길게, 사용자 요청 모델은 짧게 잡는다. 큰 모델을 붙잡아 두면 다음 상시 읽기가 그 축출을 먼저 기다려야 한다.

## 읽을 영역 지정

**서랍 → 말풍선 → 읽을 영역 지정**을 누르면 모니터마다 어둡게 덮인 화면이 뜨고, 드래그해서 읽을 사각형을 그린다. 마우스를 떼면 확정되고, `Esc`나 오른쪽 클릭은 취소다. 64×64픽셀보다 작은 드래그는 저장하지 않는다.

영역은 모니터별로 따로 저장한다. 저장 단위는 장치 이름·배치 위치·크기·배율을 모두 포함하므로, 해상도나 배율을 바꾸거나 모니터 배치를 옮기면 저장된 영역이 더 이상 맞지 않는 것으로 처리된다. 이때는 엉뚱한 곳을 읽지 않고 서랍에 `영역 설정 필요`로 멈춘다. **영역 지정 해제**를 누르고 다시 지정하면 풀린다.

영역을 한 번도 지정하지 않은 모니터는 예전처럼 전체를 읽는다. 모니터 8개까지 기억한다.

> 같은 기종 모니터 두 대를 포트만 바꿔 끼우면 저장 단위가 같아져 영역이 서로 바뀔 수 있다. 이 경우만 직접 다시 지정한다.

## 자원 보호와 서랍 상태

읽기를 시작하기 전에 여유 RAM·여유 Commit·여유 GPU 메모리를 확인한다. 모자라는 것을 확인하지 못했을 때는 안전으로 간주하지 않고 차단한다. 사용자가 직접 요청한 읽기도 이 검사를 건너뛰지 않는다. NVIDIA 어댑터가 아예 없는 PC에서는 GPU 검사만 건너뛰고 RAM·Commit 검사는 그대로 둔다.

서랍의 화면 읽기 상태는 다음 중 하나다.

| 상태 | 뜻 |
| --- | --- |
| 영역 설정 필요 | 저장한 영역이 지금 화면 구성과 맞지 않는다 |
| 변화 대기 | 읽을 때가 됐지만 화면이 그대로다 |
| 분석 중 | 모델이 응답을 만들고 있다 |
| 휴식 중 | 직전 읽기 뒤 간격을 기다리는 중이다 |
| 자원 부족 | 메모리나 GPU 여유가 모자란다. 조건이 60초 연속 회복되면 풀린다 |
| 오류로 정지 | 시간 초과나 서버 오류가 났다. 사용자가 직접 한 번 읽혀야 재개한다 |

진행 중인 요청도 자원이 부족한 상태가 10초 이상 이어지면 중단한다. 기준을 낮추거나 시간 제한을 늘려 우회하지 않고, 다른 앱이나 모델 프로세스를 종료하지도 않는다.

## 꾸미기와 답변 지침

- 이미지 크기와 말풍선 글자 크기는 별도로 설정합니다.
- 글자 크기는 8~32pt, 기본 10pt입니다. 글자의 크기와 분량에 따라 말풍선 크기가 달라지며 화면 경계 안에서 줄바꿈합니다. 아주 긴 답변은 화면에 모두 표시되지 않을 수 있으므로 짧게 답하도록 지침을 설정합니다.
- 글꼴은 번들 나눔고딕·나눔손글씨와 맑은 고딕을 선택할 수 있습니다. 배경·글자·테두리 색상도 말풍선 메뉴에서 설정합니다.
- 말풍선은 커서를 따라 이동하며 위치에 따라 꼬리가 좌우로 바뀝니다.
- 답변 지침은 최대 2,000자입니다. 저장한 지침은 다음 요청부터 적용되고 재시작 후에도 유지됩니다. 기본값 불러오기도 저장을 눌러야 반영됩니다.
- 지침 편집 중에는 상시 읽기가 잠시 멈추며, 편집 전 켜져 있었다면 이후 재개됩니다.

지침 예시:

> 현재 화면에서 가장 중요한 내용을 한국어 두 문장으로 설명해 줘. 화면에서 확인되지 않는 사실은 추측하지 말고, 필요한 경우 다음 행동을 한 가지만 제안해 줘.

## 문제가 생기면

- 연결 실패: Ollama 실행 상태와 `ollama list`의 모델 이름을 확인한다.
- 시간 초과: Ollama와 로컬 모델의 부하를 확인한 뒤 화면 읽기를 다시 요청한다. 자동 재시도하지 않는다.
- 이미지 미지원: 설치된 `qwen3.5:4b` 모델의 이미지 지원 상태를 확인한다.
- 소리가 안 남: 말풍선 음성 ON/OFF, 선택한 음성 엔진의 준비 상태, Windows 볼륨과 출력 장치를 확인한다. 드래그 음성 토글과 말풍선 음성 토글은 별개다.
- 말풍선을 없애려면 **정지·숨기기**를 사용한다. 음성만 OFF로 바꾸는 것으로는 말풍선이 숨겨지지 않는다.
- 단축키가 안 됨: 저장 시 등록 실패 안내가 있었는지 확인하고 다른 조합을 지정한다.
- 상시 읽기가 안 돌아감: 서랍의 화면 읽기 상태를 먼저 본다. `영역 설정 필요`면 영역을 다시 지정하고, `자원 부족`이면 메모리를 쓰는 다른 작업을 정리한다. `변화 대기`는 고장이 아니라 화면이 그대로라는 뜻이다.
- 드래그 음성 진단 로그는 `%TEMP%\HanEnCursorIndicator\voice-debug.log`에 있다. 선택 텍스트가 포함될 수 있으므로 공개 저장소에 올리지 않는다.

## 변경과 배포

기존 단일 소스에 `CompanionChatForm` 한 클래스를 추가한다. 외부 패키지, 빌드 참조,
터미널 복사 판정, 사용자 이미지와 기존 설정 구조는 유지한다.

운영 앱과 분리해 살펴보려면 검증용 실행 파일에 `/companion-preview`를 전달한다.
이 경로는 말풍선 미리보기용이며 기존 트레이와 전역 훅을 추가하지 않는다.
운영 실행 파일을 교체하거나 재시작하기 전에는 최종 승인을 받는다.
이미지는 실행 파일 옆 `images` 폴더에 함께 둔다.

검증 산출물과 롤백용 실행 파일은 `%TEMP%\HanEnCompanion-work`에 둔다.

モニターの番号は固定せず、要求時のカーソルがある画面を対象にします。

## 실행 검증 기록 (2026-09-09)

설치된 EXE를 대상으로 다음 검사를 수행했습니다. 아래 결과는 전체 UI 수동 검증이나 CI 통과를 의미하지 않습니다.

| 검사 | 결과와 범위 |
| --- | --- |
| 추가 기능 15개 | 통과. 기본 OFF, 미지정 키, 8개 단축키 ID와 중복 검사, 출처별 대기열 보존, 실제 Windows 단축키 등록·충돌·해제·재등록 |
| 실제 화면 읽기 → 답변 → 음성 | 통과. 실제 화면 읽기 요청을 실행하고 최종 답변 이후 말풍선 소유 음성 플레이어 실행과 작업 완료 확인. 약 19.96초 |
| 음성 파일 | WAV 294,956바이트 확보. 파일은 검증용 임시 폴더에만 보관 |
| 드래그와 독립성 | 테스트의 드래그 읽기 OFF 상태에서 말풍선 음성 동작 확인 |

실행 테스트는 설치된 어셈블리의 화면 읽기 경로와 제한된 테스트 컨텍스트를 사용했습니다. 사용자 설정을 저장하거나 운영 프로세스를 종료하지 않았습니다. 실제 스피커 청취·오디오 루프백, 트레이 메뉴 클릭 전체 흐름, 물리 단축키 입력, 활성 재생의 취소 경쟁, 설정 저장 후 재시작은 이번 검사 범위가 아닙니다. 소스 변경이 없어 이번 문서 작업에서 다시 빌드하거나 CI를 실행하지 않았습니다.

## 공개 저장소에 올리기 전

- 이 문서와 폰트를 배포할 때 아래 저작권·라이선스를 함께 유지합니다.
- 개인 설정, 인증 정보, 화면 캡처, 음성 파일, 진단 로그, 임시 테스트 산출물과 롤백 EXE는 올리지 않습니다.
- 사용자 이미지의 공개·커밋 여부는 별도로 결정합니다. 실행 파일이 무시 목록에 있다면 소스 푸시만으로 바이너리가 배포되는 것은 아닙니다.
- 이 작업에서는 커밋·푸시를 수행하지 않았습니다. 공개할 변경을 선택하고 최종 확인한 뒤 별도로 진행합니다.

## Bundled fonts

NanumGothic-Regular.ttf and NanumPenScript-Regular.ttf are unmodified Google Fonts distributions. Both use the following license. Keep this notice with the fonts when distributing the app.

Copyright (c) 2010, NHN Corporation (http://www.nhncorp.com),
with Reserved Font Name Nanum, Naver Nanum, NanumGothic, Naver 
NanumGothic, NanumMyeongjo, Naver NanumMyeongjo, NanumBrush, Naver
NanumBrush, NanumPen, Naver NanumPen.

This Font Software is licensed under the SIL Open Font License, Version 1.1.
This license is copied below, and is also available with a FAQ at:
http://scripts.sil.org/OFL


-----------------------------------------------------------
SIL OPEN FONT LICENSE Version 1.1 - 26 February 2007
-----------------------------------------------------------

PREAMBLE
The goals of the Open Font License (OFL) are to stimulate worldwide
development of collaborative font projects, to support the font creation
efforts of academic and linguistic communities, and to provide a free and
open framework in which fonts may be shared and improved in partnership
with others.

The OFL allows the licensed fonts to be used, studied, modified and
redistributed freely as long as they are not sold by themselves. The
fonts, including any derivative works, can be bundled, embedded, 
redistributed and/or sold with any software provided that any reserved
names are not used by derivative works. The fonts and derivatives,
however, cannot be released under any other type of license. The
requirement for fonts to remain under this license does not apply
to any document created using the fonts or their derivatives.

DEFINITIONS
"Font Software" refers to the set of files released by the Copyright
Holder(s) under this license and clearly marked as such. This may
include source files, build scripts and documentation.

"Reserved Font Name" refers to any names specified as such after the
copyright statement(s).

"Original Version" refers to the collection of Font Software components as
distributed by the Copyright Holder(s).

"Modified Version" refers to any derivative made by adding to, deleting,
or substituting -- in part or in whole -- any of the components of the
Original Version, by changing formats or by porting the Font Software to a
new environment.

"Author" refers to any designer, engineer, programmer, technical
writer or other person who contributed to the Font Software.

PERMISSION & CONDITIONS
Permission is hereby granted, free of charge, to any person obtaining
a copy of the Font Software, to use, study, copy, merge, embed, modify,
redistribute, and sell modified and unmodified copies of the Font
Software, subject to the following conditions:

1) Neither the Font Software nor any of its individual components,
in Original or Modified Versions, may be sold by itself.

2) Original or Modified Versions of the Font Software may be bundled,
redistributed and/or sold with any software, provided that each copy
contains the above copyright notice and this license. These can be
included either as stand-alone text files, human-readable headers or
in the appropriate machine-readable metadata fields within text or
binary files as long as those fields can be easily viewed by the user.

3) No Modified Version of the Font Software may use the Reserved Font
Name(s) unless explicit written permission is granted by the corresponding
Copyright Holder. This restriction only applies to the primary font name as
presented to the users.

4) The name(s) of the Copyright Holder(s) or the Author(s) of the Font
Software shall not be used to promote, endorse or advertise any
Modified Version, except to acknowledge the contribution(s) of the
Copyright Holder(s) and the Author(s) or with their explicit written
permission.

5) The Font Software, modified or unmodified, in part or in whole,
must be distributed entirely under this license, and must not be
distributed under any other license. The requirement for fonts to
remain under this license does not apply to any document created
using the Font Software.

TERMINATION
This license becomes null and void if any of the above conditions are
not met.

DISCLAIMER
THE FONT SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO ANY WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT
OF COPYRIGHT, PATENT, TRADEMARK, OR OTHER RIGHT. IN NO EVENT SHALL THE
COPYRIGHT HOLDER BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
INCLUDING ANY GENERAL, SPECIAL, INDIRECT, INCIDENTAL, OR CONSEQUENTIAL
DAMAGES, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF THE USE OR INABILITY TO USE THE FONT SOFTWARE OR FROM
OTHER DEALINGS IN THE FONT SOFTWARE.

