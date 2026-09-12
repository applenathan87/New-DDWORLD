# Session State — active

> 재시작 시 자동 복구용(SessionStart 훅이 읽음). 최신 상태만 유지.
> **Last Updated**: 2026-09-12 (저장소 폴더 재정비 완료)

## ⭐ 다음 세션 시작점 (2026-09-12 기록)

**2026-09-12 저장소 폴더 재정비 완료** — 내역 = [production/reorg-2026-09.md](../reorg-2026-09.md).
요약: ideation → `design/concept`, REF_GAME → `design/research/ref-games`, 프로토 → `_archive/unity-prototype`(캐시 삭제), 옛 데브로그·Origin/journal·src·.github·registry 삭제, 유니티 프로젝트 자리 = 최상위 `game/`(아직 없음), 작업 기록은 출근부 한 곳. 같은 날 저녁: 루트 CLAUDE.md를 "일하는 법" 중심 36줄로 개편, **Origin 트랙 해체**(로드맵만 `design/concept/build-roadmap.md`로), 순서 = **GDD v1 → game/** (병행 안 함).

다음에 할 일 (우선순위):

1. **출근부 ver02 첫 묶음** — [tools/desk/ROADMAP.md](../../tools/desk/ROADMAP.md) "첫 묶음" 1번부터 (퇴근 보고 3칸 → 커밋 요약 → 다음 할 일 이어가기 → …).
2. **10월 1일 새 시즌** (`tools/desk/desk.config.json` seasonStart → 2026-10-01) — 첫 마일스톤 = **GDD v1 확정**: `design/concept/`의 mvp-design·interview_idea를 `/design-system`으로 `design/gdd/`에 승격. 마일스톤·주간 계획 파일 형식은 그때 확정 (ROADMAP 두 번째 묶음).
3. **game/ 착수 (GDD v1 뒤)** — Unity Hub로 `game/` 생성 (Location `C:\DungeonHR`, Project name `game`) + `game/CLAUDE.md` → 구현 순서 초안 [design/concept/build-roadmap.md](../../design/concept/build-roadmap.md).

창 여러 개 사용 규칙: 창마다 담당 폴더, 커밋은 자기 파일만(뺀 것은 알림).

## 현재 코어 = 「마왕성 인사팀」(가제)

DDworld 코어를 **2026-07-03에 전환**했다: (2세대 PvE 헥사 오토배틀러) → **「마왕성 인사팀」** (Papers, Please식 악당 면접 + 다이어제틱 데스크). 후크 = **판단축 반전**(악당을 뽑기에 거짓말·잔인함이 장점).

- 현행 기준: [design/concept/concept-demon-hr.md](../../design/concept/concept-demon-hr.md) + [design/concept/mvp-design.md](../../design/concept/mvp-design.md) · 인덱스 [_index.md](../../design/concept/_index.md)
- 구현 순서 초안: [design/concept/build-roadmap.md](../../design/concept/build-roadmap.md) (GDD v1 뒤 마일스톤으로 재작성) · 옛 프로토(참고 전용): [_archive/unity-prototype/README.md](../../_archive/unity-prototype/README.md)
- 옛 컨셉(폐기) 인덱스: [design/gdd/_archive/README.md](../../design/gdd/_archive/README.md)

## 프로토 상태 (2026-07-14 기준 — 2026-09-12 아카이브됨)

- 프로토 S1~S2e + **S3a(밤 파트: 내 방 기숙사·석간 신문·월급 정산·까마귀 상점)** 완료. 밤 파트 레퍼런스 조사 5종 = `design/research/ref-games/`.
- **S3b(면접 뎁스: 던지기 톤 + 긴장 온도계)는 구현 롤백** (구현 8c2c10e → 롤백 e9e542f) — "코드보다 기획 확정이 먼저" 방침. 그 재기획이 아래 트랙으로 이어짐.
- **2026-08-18 아트 방향 변경**: 캐릭터 제작 = **복셀(MagicaVoxel) → 복셀풍 로우폴리(블렌더)** — 네모네모 룩은 유지, 복셀 단위 제작만 폐기. ADR-002·현행 문서 일괄 반영 완료. 에셋 전수 체크리스트 = [design/concept/asset-checklist.md](../../design/concept/asset-checklist.md) 신설 (사운드 = Artlist 사용자 선정 방침).

## 기획 트랙: 면접 덱빌딩 (2026-07-15~16 대화, 이후 변동 없음)

**기준 문서 = [design/concept/interview_idea.md](../../design/concept/interview_idea.md) (Draft v0.3)** — S3b 재기획(게이지)을 흡수. 형제 = [interview-depth.md](../../design/concept/interview-depth.md)(아이디어 카탈로그), 레퍼런스 = [ref-games/potionomics.md](../../design/research/ref-games/potionomics.md)(포셔노믹스 흥정 조사, 7/15).

**확정된 것 (상세 = interview_idea.md):**
- 목적 = 준비 전략(덱세팅) + 면접 내 콤보 / 스코프 = 씨앗 프로토 선적용
- 카드 2계열: 질문 카드(대사 필요, 소수 정예) + 화법·기술 카드(시스템 효과, 확장 가능 — 씨앗 3종: 차 대접·책상 내려치기·침묵)
- **아침 로드아웃 폐기 → 뽑기형 덱빌딩**: 상점 매매(구매+반값 되팔기) · 덱 정원제(중복 = 확률 조형) · 면접 중 뽑기(시작 4장+사용당 1장). 안전장치 = 기본 질문 상시 비품 + 결정타 2경로 저작 규칙 + 덱 오염 금지
- **붕괴**: 게이지 한계 초과 = 면접 강제 종료 + 그 건 실패 판정 (붕괴 = 그냥 실패, 단서 아님)
- **카드 효과 = 범위 랜덤** (카드에 범위 표기, 문턱은 확정 — 블랙잭 구조). 발끈 규칙은 붕괴+경고 연출로 대체
- 용어 1차 정리 완료 (은어 → 우리말. "덱 오염"은 유지 결정)

**미결 게이트 (interview_idea.md §6):**
1. 게이지의 의미·방향 (차오르는 긴장 vs 깎이는 멘탈/HP) — **사용자 고민 중**
2. 아래쪽 극단 처리 (안전 vs 양쪽 붕괴) — **사용자 고민 중**
3. 위 확정 시 → **용어 일가 일괄 개명** (tell·hot/cold·온도계 — 후보 논의됨: 꼬리/낌새/기색/꼬투리, 고긴장/저긴장 등)
4. 시나리오 시연(슬라임 '말랑' 1판, 대화 기록)에서 발견한 3건 = §6-8~10 **보류** (수확 방식 A/B · 기본 질문 재사용 · 게이지 개인차)

**기획 트랙 다음 할 것 (우선순위순) — GDD v1 마일스톤에서 소화:**
1. **§6-1·2 결정** (게이지 그림) → 용어 개명 + 보류 3건 처리 → 씨앗 스펙 확정
2. **열린 질문 #11 (공문/JD 로테이션)** — 덱빌딩(#11이 덱 가치를 날마다 재정의)과 맞물리므로 동시 결정 권장
3. 이월 항목 (7/4 기록분, 소화 여부 미확인): 플레이 검증 리스트·S4 튜닝+REPORT·사이드 옵션(OrbFx·텍스트 존 규약·블렌더 머그·종족색) — 아카이브된 프로토 README와 대조 필요

## 미결 디자인 메모

- mvp-design §15: #11(공문 로테이션) · #6 보류 도장 · #7 스탯 패널 · #9 드래그 재배치 · #10 마킹 강제 · #4 승진 미달 처리
- interview_idea.md §6 = 면접 덱빌딩 미결 전체 목록 (1~10)
- 곡면 텍스트 정책 (7/5 논의, 문서 미반영) — 수정구·블렌더 가이드에 "텍스트 존 규칙" 섹션 추가할 것
