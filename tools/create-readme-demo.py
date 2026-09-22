"""Build a privacy-safe README animation from public fixtures and reviewed assets.

Requires Pillow. No desktop capture, user settings, environment inspection or logs.
The flow is an illustrated reenactment; the settings panel and audio are real.
"""
from pathlib import Path
import argparse
import json
import math
import wave
from array import array
from PIL import Image, ImageDraw, ImageFont, ImageOps

ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / "assets"
W, H, FPS = 960, 540, 10
BG, PANEL, INK, MUTED, GREEN = "#10191e", "#1b2930", "#f0f5f2", "#a4b6bb", "#75e3b3"
FONT = Path("C:/Windows/Fonts/malgun.ttf")
BOLD = Path("C:/Windows/Fonts/malgunbd.ttf")

def font(size, bold=False):
    return ImageFont.truetype(str(BOLD if bold else FONT), size)

def text(d, xy, value, size=22, color=INK, bold=False):
    d.text(xy, value, font=font(size, bold), fill=color)

def card(d, box, fill=PANEL, outline=None):
    d.rounded_rectangle(box, radius=18, fill=fill, outline=outline, width=2)

def fixture():
    im = Image.new("RGB", (760, 380), "white")
    d = ImageDraw.Draw(im)
    text(d, (44, 35), "연습 문제", 28, "#263637", True)
    text(d, (44, 116), "2 + 3 = ?", 44, "#152925", True)
    text(d, (44, 240), "A. 5       B. 6       C. 7", 31, "#263637")
    im.save(ASSETS / "demo-problem.png")

def build():
    evidence = json.loads((ASSETS / "demo-evidence.json").read_text(encoding="utf-8"))
    assert evidence["passed"] is True
    # Each input is explicitly reviewed and committed; never sweep a user's folder.
    settings = Image.open(ASSETS / "voice-settings-public.png").convert("RGB")
    settings = settings.crop((0, 145, settings.width, 518))
    settings.thumbnail((438, 295), Image.Resampling.LANCZOS)
    with wave.open(str(ASSETS / "jarvis-korean.wav")) as audio:
        assert audio.getsampwidth() == 2 and audio.getnchannels() == 1
        samples = array("h", audio.readframes(audio.getnframes()))
    bars = [max(abs(v) for v in samples[i * len(samples)//160:(i+1)*len(samples)//160]) / 32768 for i in range(160)]
    mascots = {p: Image.open(ROOT / "dist" / "images" / (p + ".png")).convert("RGBA") for p in ("idle", "point", "cheer")}
    frames = []
    headings = ["입력 상태가 커서 옆에", "선택한 영역을 읽고 답하기", "샘플의 음색으로 말하기", "실제 사용 경로 검증"]
    labels = ["01  한 / en / EN", "02  화면 읽기 · 최대 200자", "03  CosyVoice · 자비스 복제", "04  검증 결과 + 실제 음성 파형"]
    for n in range(260):
        t = n / FPS
        scene = 0 if t < 5 else 1 if t < 12 else 2 if t < 18 else 3
        local = t - [0, 5, 12, 18][scene]
        im = Image.new("RGB", (W, H), BG)
        d = ImageDraw.Draw(im)
        text(d, (36, 22), "HanEn Cursor Indicator", 21, GREEN, True)
        text(d, (36, 64), headings[scene], 33, INK, True)
        text(d, (36, 112), labels[scene], 17, MUTED)
        text(d, (36, 498), "공개 예제 · 기능 흐름 재현 / 실제 설정 화면·합성 음성 사용", 15, MUTED)
        d.rounded_rectangle((36, 530, 36+int(888*(n+1)/260), 533), radius=1, fill=GREEN)
        if scene == 0:
            card(d, (36, 160, 924, 473))
            mode = min(2, int(local / 1.65))
            state = ["한", "en", "EN"][mode]
            text(d, (73, 200), ["한글을 입력하는 중", "English input", "CAPS LOCK"][mode], 32, INK, True)
            text(d, (74, 275), "입력 상태 확인을 위해 시선을 옮길 필요 없이", 20, MUTED)
            text(d, (74, 310), "커서 옆 미니미에서 바로 확인하세요.", 20, MUTED)
            pose = mascots["point" if mode == 1 else "idle"].copy()
            pose.thumbnail((190, 190), Image.Resampling.LANCZOS)
            x, y = 666+int(10*math.sin(local*2)), 205
            im.paste(pose, (x, y), pose)
            text(d, (x+78, y+54), state, 25, "#234934", True)
            d.polygon([(x-14,y+95),(x-14,y+137),(x-3,y+126),(x+7,y+144),(x+15,y+140),(x+5,y+121),(x+20,y+121)], fill="white", outline=BG)
            text(d, (74, 408), "크기 · 이미지 팩 · 표시 위치도 자유롭게", 18, GREEN)
        elif scene == 1:
            card(d, (36, 160, 570, 471), "#f6faf7")
            text(d, (66, 188), "공개 예제 · 연습 문제", 21, "#3d6455")
            text(d, (66, 253), "2 + 3 = ?", 36, "#142e25", True)
            text(d, (66, 323), "A. 5     B. 6     C. 7", 25, "#142e25")
            d.rounded_rectangle((52, 178, 552, 382), radius=12, outline="#299970", width=3)
            if local < 2.5:
                yy = 182+int(194*local/2.5)
                d.line((56, yy, 548, yy), fill="#299970", width=2)
            card(d, (594, 160, 924, 471))
            text(d, (617, 186), "영역 지정 → 화면 읽기", 19, GREEN, True)
            if local < 2.5:
                text(d, (617, 253), "선택한 영역 분석 중…", 20)
            else:
                text(d, (617, 244), "2 + 3 = 5이므로", 23, INK, True)
                text(d, (617, 286), "정답은 A, 5야.", 23, INK, True)
                text(d, (617, 367), "실제 예제 테스트: A 확인", 17, GREEN)
            text(d, (66, 419), "최대 200자 · 근거와 답을 함께 표시", 19, "#3d6455")
        elif scene == 2:
            card(d, (36, 160, 505, 475), "#e9edef")
            im.paste(settings, (51, 169))
            text(d, (539, 178), "실제 앱의 음성 설정", 23, GREEN, True)
            text(d, (539, 239), "참조 샘플의 음색을 복제", 23, INK, True)
            text(d, (539, 281), "한국어 · 영어 문장 합성", 21)
            text(d, (539, 325), "단순 음높이 가공과 별도 모드", 18, MUTED)
            text(d, (539, 378), "CPU 사용으로 화면 읽기와 공존", 18, MUTED)
            text(d, (539, 431), "공개용 설정값 · 개인 정보 없음", 16, GREEN)
        else:
            card(d, (36, 160, 924, 473))
            for i, line in enumerate(["화면 읽기 → 답변 생성 확인", "복제 음성 합성 → Windows 재생 완료", "한국어·영어 샘플 제공"]):
                text(d, (67, 187+i*41), "✓  "+line, 22, GREEN if i==1 else INK, True)
            text(d, (67, 328), "실제 한국어 음성 파형  ·  GIF는 무음 / 아래 샘플에서 듣기", 17, MUTED)
            pos = int(min(1,local/7.76)*len(bars))
            for k, peak in enumerate(bars):
                x = 70+k*5.05
                amp = max(2, peak*40)
                d.line((x, 405-amp, x, 405+amp), fill=GREEN if k<=pos else "#38535b", width=3)
            text(d, (67, 445), "측정 예: 합성 약 20–34초 · 환경·문장 길이에 따라 달라짐", 15, MUTED)
        frames.append(im)
    # Shared palette avoids flicker, real GIF compression avoids the legacy 14 MB output.
    palette = frames[190].quantize(colors=224)
    encoded = [f.quantize(palette=palette, dither=Image.Dither.NONE) for f in frames]
    encoded[0].save(ASSETS / "demo.gif", save_all=True, append_images=encoded[1:], duration=100, loop=0, optimize=True, disposal=1)
    # Every frame on a contact sheet, not just representative screenshots.
    review = Image.new("RGB", (8*240, math.ceil(len(frames)/8)*150), "#ffffff")
    for i, f in enumerate(frames):
        thumb=f.resize((240,135)); x=(i%8)*240; y=(i//8)*150
        review.paste(thumb,(x,y)); ImageDraw.Draw(review).text((x+3,y+135),str(i),fill="black")
    review.save(ASSETS / "demo-review-contact-sheet.jpg", quality=85)
    print(json.dumps({"frames":len(frames),"seconds":26,"size":[W,H],"bytes":(ASSETS/'demo.gif').stat().st_size}))

if __name__ == "__main__":
    parser=argparse.ArgumentParser(); parser.add_argument("--fixture-only", action="store_true")
    args=parser.parse_args(); ASSETS.mkdir(exist_ok=True); fixture()
    if not args.fixture_only: build()
