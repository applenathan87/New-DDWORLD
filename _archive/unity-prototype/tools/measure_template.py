# resume_template.png 구조 요소 픽셀 측정 — 점선/상자/배너/액자 좌표 자동 추출
from PIL import Image, ImageDraw
import sys

SRC = "/Users/nathan/New_DDWORLD/prototypes/unity-prototype/Assets/StreamingAssets/MawangHR/resume_template.png"
OUT = "/private/tmp/claude-501/-Users-nathan-New-DDWORLD/9033a137-b611-466d-9c8c-44bcc5b48db8/scratchpad/template_annotated.png"

img = Image.open(SRC).convert("RGBA")
W, H = img.size
px = img.load()
print(f"이미지: {W}x{H}")

def lum(p):
    return 0.299 * p[0] + 0.587 * p[1] + 0.114 * p[2]

# ── 1) 가로 어두운 선 감지: 특정 x창에서 행별 어두운 픽셀 수 ──
def dark_rows(x0, x1, thr=110, min_count=40):
    rows = []
    for y in range(120, H - 60):
        c = 0
        for x in range(x0, x1, 2):  # 2px 스텝 (속도)
            p = px[x, y]
            if p[3] > 200 and lum(p) < thr:
                c += 1
        rows.append((y, c))
    # 국소 피크 클러스터링
    peaks = []
    cluster = []
    for y, c in rows:
        if c >= min_count:
            cluster.append((y, c))
        else:
            if cluster:
                best = max(cluster, key=lambda t: t[1])
                peaks.append(best)
                cluster = []
    if cluster:
        peaks.append(max(cluster, key=lambda t: t[1]))
    return peaks

# 정보란 점선 (오른쪽 컬럼): x 470~940
info_lines = dark_rows(470, 940, thr=120, min_count=60)
print("\n[우측 컬럼 어두운 가로선 후보] (y, 강도):")
for y, c in info_lines:
    print(f"  y={y}  count={c}")

# 본문 폭 전체의 상자 테두리: x 140~920
box_lines = dark_rows(140, 920, thr=110, min_count=250)
print("\n[본문 폭 가로선 후보 = 상자 테두리/구분선] (y, 강도):")
for y, c in box_lines:
    print(f"  y={y}  count={c}")

# ── 2) 보라 배너 감지 ──
def is_purple(p):
    r, g, b, a = p
    return a > 200 and 30 <= r <= 100 and 20 <= g <= 75 and 45 <= b <= 120 and b > g + 8 and r < b + 30

purple_rows = []
for y in range(200, H - 100):
    c = sum(1 for x in range(80, 450, 2) if is_purple(px[x, y]))
    purple_rows.append((y, c))

bands = []
cur = None
for y, c in purple_rows:
    if c > 30:
        if cur is None:
            cur = [y, y]
        else:
            cur[1] = y
    else:
        if cur and cur[1] - cur[0] > 15:
            bands.append(tuple(cur))
        cur = None
if cur and cur[1] - cur[0] > 15:
    bands.append(tuple(cur))
print("\n[보라 배너 세로 구간]:")
banner_rects = []
for y0, y1 in bands:
    xs = [x for x in range(60, 500) for yy in (y0 + (y1 - y0) // 2,) if is_purple(px[x, yy])]
    if xs:
        r = (min(xs), y0, max(xs), y1)
        banner_rects.append(r)
        print(f"  x={r[0]}..{r[2]}, y={r[1]}..{r[3]}")

# ── 3) 초상화 금색 액자 감지 (좌상단 영역) ──
def is_gold(p):
    r, g, b, a = p
    return a > 200 and r > 115 and 70 < g < 175 and b < 115 and r > b + 35

gxs, gys = [], []
for y in range(230, 750, 2):
    for x in range(70, 500, 2):
        if is_gold(px[x, y]):
            gxs.append(x)
            gys.append(y)
if gxs:
    print(f"\n[초상화 금색 액자 bbox]: x={min(gxs)}..{max(gxs)}, y={min(gys)}..{max(gys)}")

# ── 4) 주석 이미지 생성 (검출 결과를 그려서 눈으로 검증) ──
ann = img.convert("RGB").copy()
d = ImageDraw.Draw(ann)
for y, c in info_lines:
    d.line([(470, y), (940, y)], fill=(255, 0, 0), width=3)
for y, c in box_lines:
    d.line([(140, y), (920, y)], fill=(0, 120, 255), width=3)
for r in banner_rects:
    d.rectangle(r, outline=(0, 200, 0), width=4)
if gxs:
    d.rectangle((min(gxs), min(gys), max(gxs), max(gys)), outline=(255, 0, 255), width=4)
ann.save(OUT)
print(f"\n주석 이미지 저장: {OUT}")
