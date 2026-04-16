# Cross-GDD Review Report

**Date**: 2026-04-16
**GDDs Reviewed**: 3 (combat, deck, economy)
**Systems Covered**: Combat, Deck, Economy
**Verdict**: FAIL — Blocking 이슈 3개 해결 필요

---

## Consistency Issues

### Blocking (아키텍처 진행 전 반드시 해결)

#### C-01: Economy GDD — 궁병/함정 데이터 불일치

- `economy.md` 분대 가치 테이블에서 **궁병 분대 인원: [미정]**, **함정 squad_value/base_value: [미정]**
- `combat.md`에는 궁병 10명, 함정 1개로 **확정**되어 있음
- 판돈 계산 공식이 이 값 없이는 정확히 동작할 수 없음
- **수정**: economy.md의 분대 가치 테이블에 궁병 10명(base_value=1.0), 함정 값을 combat.md 기준으로 채워야 함

#### C-02: Deck↔Economy 의존성 기술 불일치

- `deck.md` Dependencies: Economy를 "MVP 범위 밖"으로 기술
- `economy.md` Dependencies: Deck을 "[미정]"으로 기술
- `economy.md` Interactions: "Economy → Deck: 판돈 통화로 덱 구성 변경/업그레이드"를 Core Rule 5에서 확정 서술
- 같은 관계를 한쪽은 "확정", 한쪽은 "범위 밖"으로 모순
- **수정**: 둘 중 하나를 정해야 함 — MVP에 포함? 범위 밖?

### Warnings (해결 권장, blocking은 아님)

#### C-03: Combat [미정] 항목 4개 잔존

- 배치 제한 시간, 그리드 배치 제한 규칙, 돌격 보너스(charge_multiplier/threshold), 크리티컬 시스템
- 이 중 "배치 제한 시간"과 "배치 제한 규칙"은 MVP에 필요할 수 있음

#### C-04: 기초 문서 부재

- `game-concept.md`, `game-pillars.md`, `systems-index.md` 없음
- 필라가 각 GDD 헤더에 흩어져 있지만 공식 정의 문서가 없음
- 지금까지 2개 필라 확인: "심리전" (combat, deck), "역전의 희열" (economy)

---

## Game Design Issues

### Blocking

#### D-01: 유일한 자원 싱크(sink)가 미정의

- 판돈(Pot)이 유일한 자원인데, **이걸 뭐에 쓰는지 정의되지 않음**
- `economy.md` Core Rule 5: "덱 강화 통화로 사용" → Open Question: "덱 강화로 뭘 살 수 있는가? [미정]"
- 자원의 **소스(전투 승리)는 명확**하지만 **싱크(사용처)가 없으면** 경제 시스템이 의미를 잃음
- **권장**: 최소 MVP 수준의 싱크를 정의해야 경제 루프가 완성됨

### Warnings

#### D-02: 민병대가 raw stats 최강 — 의도된 것인가?

병종별 총 전투력 비교:

| 병종 | 총 DPS (ATK×인원) | 총 HP (HP×인원) | 특이사항 |
|------|-------------------|-----------------|----------|
| 기병 | 75 (15×5) | 150 (30×5) | 돌격 보너스 (미구현) |
| 창병 | 80 (8×10) | 150 (15×10) | 대돌격 방어 |
| 궁병 | 30 effective (4×10×0.75) | 100 (10×10) | 사거리 3칸 |
| **민병대** | **80 (4×20)** | **160 (8×20)** | **패닉 메카닉** |
| 함정 | 범위 데미지 (1회) | 1 | 특수 |

- 민병대가 **총 DPS 공동 1위 + 총 HP 단독 1위**
- Economy의 기권 전략("약한 카드로 채워 판돈 최소화")과 모순 가능성
- 패닉 메카닉이 충분히 페널티가 되는지 프로토타입 검증 필요

#### D-03: 패자 보상 부재 — 연패 이탈 리스크

- Economy: "패자에게도 소량 보상? [미정]"
- 3:0으로 연패하면 판돈을 하나도 얻지 못함
- PvP 게임에서 연패자의 이탈을 방지하는 캐치업 메카닉이 없음

---

## Cross-System Scenario Issues

Scenarios walked: 3

### Warnings

#### 기권(Fold) 전략 — Combat × Economy × Deck

- 기권 전략은 "약한 카드를 배치"하는 것이 전제인데, 어떤 카드가 "약한 카드"인지 기준이 불명확
- 민병대가 raw stats 최강이면 기권 전략의 카드 선택 근거가 무너짐
- D-02와 직결됨

### Info

#### 양측 동시 전멸 — Combat × Economy

- Combat: "양쪽 모두 1승" / Economy: "판돈 적립 0"
- 두 GDD가 일관성 있게 처리. 문제 없음

#### 5라운드 풀 매치 덱 소진 — Deck

- 45장 덱 / 최대 28장 소비 = 17장 잔여. 소진 없음 확인

---

## GDDs Flagged for Revision

| GDD | Reason | Type | Priority |
|-----|--------|------|----------|
| economy.md | 궁병/함정 데이터 [미정] (C-01) | Consistency | **Blocking** |
| economy.md | 자원 싱크 미정의 (D-01) | Design Theory | **Blocking** |
| economy.md | Deck 의존성 기술 모순 (C-02) | Consistency | **Blocking** |
| combat.md | [미정] 항목 4개 잔존 (C-03) | Completeness | Warning |
| economy.md | 패자 보상 미정 (D-03) | Design Theory | Warning |

---

## Verdict: FAIL

Blocking 이슈 3개(C-01, C-02, D-01)가 해결되어야 아키텍처 단계로 진행할 수 있습니다.

### Required Actions Before Re-running

1. **economy.md**: 궁병 분대 인원(10명, base_value=1.0)과 함정의 squad_value/base_value 확정 (C-01)
2. **economy.md + deck.md**: Deck↔Economy 의존성을 MVP 포함/제외로 통일 (C-02)
3. **economy.md**: 판돈 사용처(덱 강화 내용) 최소 MVP 수준으로 정의 (D-01)
