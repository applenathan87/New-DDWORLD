# Session State — active

> 재시작 시 자동 복구용(SessionStart 훅이 읽음). 최신 상태만 유지.
> **Last Updated**: 2026-07-04 (저녁, Day 29 세션 종료)

## 현재 코어 = 「마왕성 인사팀」(가제)

DDworld 코어를 **2026-07-03에 전환**했다: (2세대 PvE 헥사 오토배틀러) → **「마왕성 인사팀」** (Papers, Please식 악당 면접 + 다이어제틱 데스크). 후크 = **판단축 반전**(악당을 뽑기에 거짓말·잔인함이 장점).

- 현행 기준: [ideation/concept-demon-hr.md](../../ideation/concept-demon-hr.md) + [ideation/mvp-design.md](../../ideation/mvp-design.md)
- 제작 트랙: [Origin/roadmap.md](../../Origin/roadmap.md) · 스프린트 브리프: [docs/mawang-hr-proto-brief.md](../../docs/mawang-hr-proto-brief.md)
- 옛 컨셉(폐기) 인덱스: [design/gdd/_archive/README.md](../../design/gdd/_archive/README.md)

## 진행 중: MVP 프로토 스프린트 (7/4~7/7, Fable) — S2b까지 완료 (일정 하루 선행)

- 위치: `prototypes/unity-prototype` — 씬 `Assets/mawanghr.unity`, 코드 `Assets/Scripts/MawangHR/`, 콘텐츠 `Assets/StreamingAssets/MawangHR/gamedata.json`
- 빌드 이력 상세 = 프로토 README. **S1~S2b 완료**: 서류 심사(마킹·도장·물리 소품) + 스케줄링 퍼즐 + **1차 면접 루프**(몬스터 그레이박스·3D 질문 카드·답변 마킹·승진 연동).

## 직전 세션에 한 일 (2026-07-04, Day 29 — 상세 = production/devlog/Day 29)

1. **코드 리뷰 7건 반영**: 빌드 지뢰 2(셰이더 스트리핑·Input 액션) + 데이터 안전화(GameDataValidator) + 16:10 잘림. 우려점 8건 = 프로토 README.
2. **케이스 풀 확정·구현**: 지원자 7→17건, 매 판 7건 뽑기(미출현 우선 셔플백, 물컹이 첫 슬롯), **쿼터 폐지**. mvp-design §10a.
3. **시트 파이프라인**: tools/cases_sheet.py(왕복 변환) + case-sheet-guide.md(작업법 + 비주얼 3층 분류). **아트: 종족 통폐합 안 함**(모델 최대 제작 + 폴백).
4. **보상 경제 3축**(#13: 공적/직급/골드, XP 없음) + **공적 게이지**(0→100 바+카운터, 승진 = meritGoal 도달).
5. **S2b 면접 루프**: 3D 카드 물성(포인트 3)·HUD 자막·진술 기록 V/X·승진 연동 Day 2·면접자 4명(끄르렁/밀크티/골드바/하품). 레이스 버그 수정 + 몬스터 피격 튕김·움찔.

## 다음 할 것

1. **S2b 플레이 검증** — 카드 던지는 손맛(물성 2차)·자막 가독성·피격 기믹·코미디 톤 피드백
2. **열린 질문 #11 결정** (공문/JD 로테이션 — 사용자 고민 중) ← **S3 스키마 설계 전 필수**
3. **S3** (브리프 7/6): 3일 구조·승진 가젯(촛불/돋보기)·Day 3 사기부서+스파이 이중반전·지연 사고/신문·엔딩 등급
4. S4 (7/7): 튜닝 + REPORT (가설 판정 + Origin 해부 가이드 + roadmap 현재 위치 동기화)

## 미결 디자인 메모

- mvp-design §15 열린 질문: #11(공문 로테이션, 보류) · #6 보류 도장 · #7 스탯 패널 · #9 드래그 재배치 유지 · #10 마킹 최소 1개 강제(적중 반영은 결정됨) · #4 승진 미달 처리
- 프로토 README "알려진 우려점" 8건 (도장 확정 시점 = 디자인 결정 대기) + 첫 빌드 체크리스트
- 시트 저작 전환 시점 = 케이스 20건+ (가이드 = prototypes/unity-prototype/tools/case-sheet-guide.md)
