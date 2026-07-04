#!/usr/bin/env python3
# 케이스 시트 왕복 변환기 — gamedata.json <-> cases.csv (구글 시트 저작용)
#
# 사용법 (저장소 루트 또는 아무 데서나):
#   python3 prototypes/unity-prototype/tools/cases_sheet.py export
#       -> tools/cases.csv 생성 (구글 시트에 "파일 > 가져오기"로 올릴 파일)
#   python3 prototypes/unity-prototype/tools/cases_sheet.py import <받은파일.csv>
#       -> 시트에서 다운로드한 CSV를 검증 후 gamedata.json에 반영
#          (applicants 전체 교체 + day1 풀 명단 재생성. 반영 전 .bak 백업 자동 생성)
#
# 규칙 (게임 쪽 GameDataValidator와 동일):
#   - 정답: PASS 또는 FAIL
#   - 근거 칸: 빈칸(단서 아님) 또는 정답과 같은 값(결정적 근거)만 허용
#   - 결정적 근거(정답 방향)가 최소 1개 있어야 함 (이력 근거 또는 특이근거)
#   - 이력은 1~3줄 (4줄부터 레이아웃 한계로 특이사항과 겹침)
#   - id 중복 금지
#
# ⚠️ 시트 작업을 시작하면 지원자 데이터의 진실 원본 = 시트.
#    gamedata.json의 applicants를 손으로 고치지 말 것 (다음 import 때 덮어써짐).

import csv
import json
import os
import shutil
import sys

ROOT = os.path.dirname(os.path.abspath(__file__))
JSON_PATH = os.path.normpath(os.path.join(ROOT, "..", "Assets", "StreamingAssets", "MawangHR", "gamedata.json"))
CSV_PATH = os.path.join(ROOT, "cases.csv")

# 시트 열 순서 (한글 헤더 = 저작 편의, 순서 바꿔도 됨 — 이름으로 읽음)
HEADERS = ["id", "이름", "종족", "직무", "연봉", "한마디",
           "이력1", "근거1", "이력2", "근거2", "이력3", "근거3",
           "특이사항", "특이근거", "정답", "해설", "풀"]

VALID_JD = ("adm", "guard")          # JD 추가 시 여기와 gamedata.json의 jds에 함께 추가
VALID_POOL = ("day1", "day2", "day3")


def load_json():
    with open(JSON_PATH, encoding="utf-8") as f:
        return json.load(f)


def export_csv():
    d = load_json()
    # utf-8-sig(BOM): Numbers/엑셀에서 열어도 한글이 안 깨지게. 구글 시트는 둘 다 OK.
    with open(CSV_PATH, "w", newline="", encoding="utf-8-sig") as f:
        w = csv.writer(f)
        w.writerow(HEADERS)
        for a in d["applicants"]:
            row = [a["id"], a["name"], a["species"], a["jdId"], a["salary"], a["quote"]]
            lines = list(a["resumeLines"]) + [{"text": "", "evidence": ""}] * 3
            for line in lines[:3]:
                row += [line["text"], line["evidence"]]
            row += [a["special"], a["specialEvidence"], a["correct"], a["reveal"], "day1"]
            w.writerow(row)
    print(f"내보내기 완료: {CSV_PATH} ({len(d['applicants'])}건)")
    print("→ 구글 시트에서 '파일 > 가져오기 > 업로드'로 올리세요.")


def import_csv(path):
    d = load_json()
    with open(path, encoding="utf-8-sig") as f:
        rows = list(csv.DictReader(f))

    apps, pools, errors, warns = [], {}, [], []
    seen_ids = set()

    for i, r in enumerate(rows, start=2):  # 2행부터 = 시트에서 보이는 행 번호
        def cell(key):
            return (r.get(key) or "").strip()

        rid = cell("id")
        if not rid and not cell("이름"):
            continue  # 완전 빈 행은 조용히 스킵
        if not rid:
            errors.append(f"{i}행: id가 비었습니다")
            continue
        if rid in seen_ids:
            errors.append(f"{i}행: id 중복 '{rid}'")
            continue
        seen_ids.add(rid)

        correct = cell("정답")
        if correct not in ("PASS", "FAIL"):
            errors.append(f"{i}행 '{rid}': 정답은 PASS/FAIL이어야 합니다 (현재 '{correct}')")
            continue
        if cell("직무") not in VALID_JD:
            errors.append(f"{i}행 '{rid}': 직무는 {VALID_JD} 중 하나여야 합니다 (현재 '{cell('직무')}')")

        lines = []
        for n in (1, 2, 3):
            text, ev = cell(f"이력{n}"), cell(f"근거{n}")
            if not text:
                if ev:
                    errors.append(f"{i}행 '{rid}': 이력{n}이 비었는데 근거{n}만 있습니다")
                continue
            if ev not in ("", correct):
                errors.append(f"{i}행 '{rid}': 근거{n}은 빈칸 또는 정답({correct})과 같아야 합니다 (현재 '{ev}')")
            lines.append({"text": text, "evidence": ev})
        if not lines:
            errors.append(f"{i}행 '{rid}': 이력 줄이 하나도 없습니다")
            continue

        sp_ev = cell("특이근거")
        if sp_ev not in ("", correct):
            errors.append(f"{i}행 '{rid}': 특이근거는 빈칸 또는 정답({correct})과 같아야 합니다 (현재 '{sp_ev}')")
        if correct not in [l["evidence"] for l in lines] + [sp_ev]:
            errors.append(f"{i}행 '{rid}': 결정적 근거(정답 방향 표시)가 하나도 없습니다")

        pool = cell("풀") or "day1"
        if pool not in VALID_POOL:
            warns.append(f"{i}행 '{rid}': 풀 '{pool}'은 알 수 없는 값 (지금은 day1만 사용)")
        pools[rid] = pool

        apps.append({
            "id": rid, "name": cell("이름"), "species": cell("종족"),
            "jdId": cell("직무"), "salary": cell("연봉"), "quote": cell("한마디"),
            "resumeLines": lines,
            "special": cell("특이사항"), "specialEvidence": sp_ev,
            "correct": correct, "reveal": cell("해설"),
        })

    if errors:
        print(f"❌ 반영 중단 — 오류 {len(errors)}건 (시트에서 고친 뒤 다시 실행):")
        for e in errors:
            print("  ·", e)
        sys.exit(1)
    for w_ in warns:
        print("⚠️ ", w_)

    day1_ids = [a["id"] for a in apps if pools[a["id"]] == "day1"]
    day = d["days"][0]
    if day.get("firstId") and day["firstId"] not in day1_ids:
        print(f"⚠️  firstId '{day['firstId']}'가 day1 풀에 없습니다 — 첫 슬롯 고정이 무시됩니다")
    if day.get("promoteMin", 0) > min(day.get("drawCount") or len(day1_ids), len(day1_ids)):
        print("⚠️  promoteMin이 등장 서류 수보다 큽니다 — 승진 불가능 (게임 시작 시 에러)")

    shutil.copy(JSON_PATH, JSON_PATH + ".bak")  # 실수 대비 직전본 백업
    d["applicants"] = apps
    day["applicantIds"] = day1_ids
    with open(JSON_PATH, "w", encoding="utf-8") as f:
        json.dump(d, f, ensure_ascii=False, indent=2)
        f.write("\n")
    n_pass = sum(1 for a in apps if a["correct"] == "PASS")
    print(f"✅ 반영 완료: 지원자 {len(apps)}건 (PASS {n_pass} / FAIL {len(apps) - n_pass}) · day1 풀 {len(day1_ids)}건")
    print(f"   백업: {JSON_PATH}.bak · 유니티에서 Play로 확인하세요")


if __name__ == "__main__":
    if len(sys.argv) >= 2 and sys.argv[1] == "export":
        export_csv()
    elif len(sys.argv) >= 3 and sys.argv[1] == "import":
        import_csv(sys.argv[2])
    else:
        print(__doc__ or "사용법: cases_sheet.py export | import <파일.csv>")
        sys.exit(1)
