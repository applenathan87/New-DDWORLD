# DDworld Monetization — Research & Direction

> **Status**: Draft — Not Decided
> **Author**: nathan
> **Last Updated**: 2026-04-23
> **Target**: 프로토타입 검증 이후 방향 확정. 지금은 리서치 + 옵션 정리 단계.
> **Promotion Path**: 확정 시 `design/gdd/monetization.md`로 정식 GDD 승격

## 0. 요약

DDworld의 BM 선택은 **게임 설계 철학(3대 Anti-Pillar)** 과 **솔로 인디의 제작 여력** 두 제약에 크게 구속됩니다. 병력 카드 판매는 설계 훼손 위험이 커서 배제하고, Premium 본체 + 소규모 코스메틱 DLC 방향이 가장 현실적입니다. 단, 아직 확정이 아닙니다.

---

## 1. 경쟁사 분석: 클래시 로얄 BM (2025~2026)

### 1.1 수익 구조

| 요소 | 설명 | 성격 |
|------|------|------|
| Pass Royale | 시즌제 배틀패스. 2024년 2.0 개편으로 무료/골드/다이아 3티어 | 정기 수익의 중심 |
| 젬(Gems) | 게임 내 프리미엄 통화. 현금 → 젬 → 상자 가속 | 전통적 F2P |
| 상자(Chests) | 랜덤 카드 획득. 재화로도 가능하나 젬 가속 유도 | 시간 단축형 (2025년 3월 축소) |
| Lucky Drops | 2025년 3월 업데이트 신설. 하루 4회 무료 | 상자 대체 |
| 스킨/배너 | 2024년부터 비중 증가. 이스포츠 한정판 $15~20 | 코스메틱 |
| Mastery 시스템 | 카드 레벨링 + 코스메틱 + 업그레이드 결합 | 복합 과금 레인 |

### 1.2 규모

- **2025년 3월 월 매출 $105M** (출시 8년 후)
- 1월 대비 600% 증가
- 연 $2B+ 규모

### 1.3 2024 → 2025 전략 전환

| 이전 (Pay to Progress) | 이후 (Pay to Feel Unique) |
|------------------------|---------------------------|
| 돈 쓰면 빨리 강해짐 | 돈 쓰면 특별해 보임 |
| 무과금은 진행 느림 | 무과금도 진행 가능. 스킨만 유료 |
| 과금이 승률에 직결 | 코스메틱/한정판이 핵심 유료 가치 |

이 전환으로 매출 600% 상승. **"F2P는 진행 가능 + 과금은 유니크함"** 이 현재의 공식.

### 1.4 그럼에도 남은 P2W 논란

2024~2025년에 전 세계 **글로벌 보이콧** 발생. 핵심 이슈:

- Pass Royale 2.0으로 Gold Pass 가치 하락
- **Evolutions**(카드 진화) 시스템이 Pass 구매자에게만 실질적으로 열림
- 카드 레벨 격차가 매칭에 영향 — "Max 레벨 상대를 못 이김"
- Banner Boxes 삭제 등 시즌 보상 축소

### 1.5 핵심 교훈

클래시 로얄은 **"카드 강화를 팔았다가 P2W 소리 듣는 게임의 전형"**. $2B 매출로 논란을 흡수하지만, 인디는 같은 구조 가지면 리뷰 폭탄으로 게임이 죽음.

---

## 2. DDworld BM 제약 조건

### 2.1 Anti-Pillar와의 충돌

| Anti-Pillar | 충돌 가능 BM |
|-------------|-------------|
| 조작 실력 게임이 아니다 | (해당 없음) |
| **랜덤 운 게임이 아니다** | 🔴 카드 상자 / 가챠 / 랜덤 드롭 |
| **복잡한 경영 게임이 아니다** | 🔴 카드 레벨링 / 강화 / 업그레이드 시스템 |

### 2.2 핵심 게임 설계와의 충돌

DDworld의 Pillar "심리전"은 **대칭적 손패 공개**와 **동등한 카드 파워**를 전제로 합니다.

- 플레이어 A의 기병(레벨 5) vs 플레이어 B의 기병(레벨 10) 비대칭 발생 시:
  - 배치 읽기가 의미 없음 (파워 차이로 그냥 결정남)
  - "읽었다!"의 쾌감 붕괴
  - 클래시 로얄이 받은 비판을 **작은 규모로 그대로** 받게 됨

**결론**: 병력 카드 판매는 DDworld 설계 자체를 무너뜨림. 배제.

### 2.3 솔로 인디 제작 여력 제약

- 스킨: 5병종 × 3~5 종 = 20~25개 유닛 비주얼 필요 → **인디에게 부담**
- 시즌 콘텐츠 생산: 지속적 업데이트는 풀타임 인력 필요 → **1인에게 불가**
- 서버 비용: F2P 모델은 DAU에 따라 비용 증가 → **리스크 큼**

---

## 3. BM 후보 옵션

### 3.1 Option A — Premium (일회 구매)

- **구조**: Steam에서 $15~25 일회 구매. 모든 콘텐츠 포함.
- **장점**: 설계 철학 훼손 0. 디자인 작업 최소. 발매 후 운영 부담 작음.
- **단점**: 장기 수익 약함. DAU 감소 시 수익 종료.
- **참고 게임**: Into the Breach, FTL, Slay the Spire, Slay the Princess

### 3.2 Option B — Premium + Cosmetic DLC (유력 후보)

- **구조**: 본체 $15~20 + 이후 스킨/배너 팩 $3~5 단위로 출시.
- **장점**: 장기 수익 가능. P2W 논란 없음. 게임 설계 훼손 없음.
- **단점**: 스킨 제작 부담 (완화 방법 아래 참조).
- **참고 게임**: Slay the Spire (캐릭터 DLC), Balatro (향후 계획)

#### 스킨 제작 부담 완화 방법

1. **팔레트 스왑**: 같은 모델/애니메이션 + 색상 변경만. 제작 비용 1/5.
2. **AI 생성 + 수정**: Midjourney 등으로 베이스 에셋 생성 후 통일화.
3. **배너/아이콘/이펙트**: 유닛이 아닌 주변 요소만 판매.

### 3.3 Option C — F2P + Cosmetic + Pass

- **구조**: 본체 무료. 배틀패스 + 코스메틱만 유료.
- **장점**: 수익 잠재력 최대. 진입 장벽 없음.
- **단점**: 시즌 콘텐츠 생산 부담 풀타임급. 서버/운영 비용 발생.
- **참고 게임**: 클래시 로얄(현재 방향), Fortnite, Apex Legends
- **평가**: 인디 1인에게는 운영 부담 과대. **현재 단계에서 추천 X**.

### 3.4 Option D — DDworld 특화 "연출 판매"

Pillar "보는 맛"을 BM으로 전환:

| 판매 대상 | 설명 | 제작 부담 |
|-----------|------|----------|
| 승리 연출 스킨 | 3선승 달성 시 폭죽/배경 변경 | 낮음 |
| 배치 트레일 | 카드를 그리드에 놓을 때 광 이펙트 | 낮음 |
| 함성/대사 팩 | 병종 소환 시 대사 변경 (한국어 성우, 밈) | 중간 |
| 배틀 로그 스킨 | 판돈 UI, 스코어 표시 디자인 변경 | 낮음 |
| 배너/칭호 | 프로필 장식 | 낮음 |

- **장점**: 유닛 애니메이션보다 제작 부담 훨씬 작음. 결과에 영향 0. 심리전 플레이어의 "나만의 스타일" 자존심 자극.
- **단점**: 지속적으로 새 연출을 만들어야 함.
- **평가**: Option B와 조합 가능한 **보완적 방향**.

---

## 4. 현재의 임시 방향성 (미확정)

**유력한 조합**: Option B (Premium + Cosmetic DLC) + Option D (연출 판매)

- 본체 $15~20 Steam 판매
- 이후 3~6개월 간격으로 $3 내외 코스메틱 팩 출시
  - 병종 스킨 팩 (팔레트 스왑 기반)
  - 연출/UI 팩 (Option D 요소)
- 카드 강화/상자/가챠 **일체 없음**
- F2P 전환은 **프로덕션 단계 이후 수치 검증 후 재검토**

### 4.1 결정 보류 이유

- 프로토타입에서 **리플레이성 검증** 아직 안 됨 → 일회 구매 BM의 전제 조건 미확인
- **매치 길이** 측정 필요 → 10분 이상이면 F2P 모델과 상성 나쁨
- **시장 반응** 불확실 → Steam 데모 피드백 필요

### 4.2 결정 시점

- 프로토타입 플레이테스트 완료 후 (현재 진행 중)
- Steam Next Fest 등 데모 피드백 수집 후
- 정식 GDD 승격 (`design/gdd/monetization.md`) 시점에 확정

---

## 5. 참고 자료

### 클래시 로얄 BM 관련
- [Clash Royale's Revenue Empire: How Supercell Generates $2B+ Annually in 2026 — Spawnrift](https://spawnrift.com/clash-royales-revenue-empire-how-supercell-generates-2b-annually-in-2026/)
- [Clash Royale made $105M in 30 Days — Gamigion](https://www.gamigion.com/clash-royale-made-105m-in-30-days-top-game-after-8-years/)
- [Monetization in Clash Royale: A Case Study — Bencin Studios](https://www.bencinstudios.com/blog/monetization-clash-royale-case-study)
- [Clash Royale's New Pass: Is It Pay-to-Win? — zLeague](https://www.zleague.gg/theportal/clash-royale-pass/)
- [Clash Royale Faces Global Boycott — EGW](https://egw.news/gaming/news/27322/clash-royale-faces-global-boycott-as-players-prote-HO51_UNPv)
- [Players Say 'Clash Royale' Is Becoming Pay-to-Win — LemonWire](https://lemonwire.com/2026/03/24/clash-royale-pay-to-win-concerns/)

### 인디 Premium BM 레퍼런스 (후속 조사 필요)
- Slay the Spire 판매 수치 및 DLC 전략
- Into the Breach 판매 수치
- Balatro 판매 수치 및 DLC 계획

---

## 6. Open Questions

| 질문 | Owner | Deadline | Resolution |
|------|-------|----------|-----------|
| 프로토타입에서 리플레이성이 충분한가? | nathan | 프로토타입 완료 시 | [미정] |
| 매치 평균 길이는? (5분 미만? 10분 이상?) | nathan | 플레이테스트 후 | [미정] |
| 한국/아시아 시장 vs 북미 시장 중 어느 쪽을 먼저? | nathan | [미정] | [미정] |
| 데모 공개 시점 (Steam Next Fest 참가 여부) | nathan | [미정] | [미정] |
| Option D 연출 판매가 실제로 구매 동기를 만들 수 있는가? | nathan | 검증 필요 | [미정] |
| 로컬 CPU AI 대전만으로 BM이 성립하는가? (온라인 PvP 필수?) | nathan | [미정] | [미정] |
