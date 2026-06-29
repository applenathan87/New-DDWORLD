# Cross-GDD Review Report

**Date**: 2026-04-23
**GDDs Reviewed**: 3 (combat, deck, economy)
**Systems Covered**: Combat, Deck, Economy
**Verdict**: PASS (CONCERNS) — 이전(2026-04-16) FAIL → PASS로 승격
**Previous Review**: [gdd-cross-review-2026-04-16.md](gdd-cross-review-2026-04-16.md)

---

## 요약

이전 리뷰(2026-04-16)의 Blocking 3개가 모두 해결되었습니다. 본 재리뷰에서 Phase 4 시나리오 워크스루로 새 Blocker 1개(배치 장수 하한선 미정의)가 발견되었으나, **리뷰 중 인라인 수정으로 해결**되어 최종 Verdict는 PASS입니다. 잔존 Warning은 프로토타입 플레이테스트 또는 UX 스펙에서 자연스럽게 해소 가능한 수준입니다.

## 이전 리뷰 Blocking 이슈 상태 (2026-04-16 → 2026-04-23)

| ID | 이전 상태 | 현재 상태 | 근거 |
|----|-----------|-----------|------|
| C-01 | 🔴 Blocking — 궁병/함정 데이터 미정 | ✅ 해결 | economy.md:85 — 궁병 10명/base_value=1.0, 함정은 "판돈 계산 제외" 명시 |
| C-02 | 🔴 Blocking — Deck↔Economy 의존성 모순 | ✅ 해결 | 양쪽 GDD 모두 "MVP 범위 밖"으로 통일 (economy.md:38, 59, 128 / deck.md:104, 119) |
| D-01 | 🔴 Blocking — 자원 싱크 미정의 | ✅ 조건부 해결 | economy.md:38 Core Rule 5 — "판돈은 매치 승리의 점수". MVP 스코프 내 싱크 정의됨 |
| C-03 | ⚠️ Warning — combat.md [미정] 4개 | ⚠️ 일부 해결 | 배치 제한 시간은 여전히 [미정]. 나머지는 Alpha 보류 |
| D-02 | ⚠️ Warning — 민병대 raw stats 최강 | ⚠️ 유지 | 프로토타입 플레이테스트 검증 대기 |
| D-03 | ⚠️ Warning — 패자 보상 부재 | ⚠️ 유지 | PvP 출시 전 결정. MVP에서는 비차단 |

---

## Consistency Issues (Phase 2)

### Blocking
없음.

### Warnings

#### C2-W1: deck.md Cross-References의 stale/misleading reference
- **위치**: deck.md:172 Cross-References 두 번째 행
- **내용**: "잔존 병사 → 이월 가치 판단 | economy.md | 판돈 적립 규칙 | Rule dependency"
- **문제**: "이월 가치 판단"은 economy.md에 정의된 실제 규칙이 아닌 플레이어의 전략적 판단. 데이터 의존성이 아닌 개념적 참조를 의존성처럼 기술.
- **권장**: "Rule dependency" → "Conceptual reference"로 표기하거나 행 삭제

#### C2-W2: 분대 인원(squad_size) 이중 정의
- **위치**: combat.md Unit Data 테이블 + economy.md 분대 가치 테이블
- **내용**: 두 GDD가 동일 값(기병 5, 창병/궁병 10, 민병대 20, 함정 1)을 독립 정의
- **문제**: 현재 일치하지만 한쪽 수정 시 자동 동기화 없음. Single source of truth 부재
- **권장**: combat.md를 authoritative source로 지정하고 economy.md는 "combat.md Unit Data 참조" 명시. entity registry(entities.yaml)에 등록도 고려

### Info

#### C2-I1: combat.md Dependencies 방향 표기 혼용
- **위치**: combat.md:187 — "Economy | Economy depends on Combat"
- **내용**: 다른 행은 "Combat depends on X" 형식인데 Economy 행만 주체 역전

#### C2-I2: economy.md alive_soldiers 범위 표기 모호
- **위치**: economy.md:73 — "alive_soldiers | array | 0-25"
- **내용**: 이론적 최대가 분대 개수(5) 기준인지 병사 수(최대 100) 기준인지 모호
- **권장**: "0-25 (분대 단위 최대 5분대×민병대 20명 기준이면 개별 병사 100명까지 가능)" 등 명확화

---

## Game Design Issues (Phase 3)

### Blocking
없음.

### Warnings

#### D3-W1: 배치 제한 시간 미정의 — Anti-pillar 위반 가능성
- **위치**: combat.md:196, 303 — Tuning Knob "배치 제한 시간 [미정]"
- **내용**: 값에 따라 Anti-pillar "조작 실력 게임이 아니다" 위반 가능. 너무 짧으면(예: 10초) 드래그 속도가 승패에 영향
- **권장**: MVP 결정 필요. 최소 30초 이상 권장. 프로토타입 플레이테스트로 검증

#### D3-W2: "보는 맛" 필라 담당 시스템 부재
- **위치**: game-concept.md Pillar 3 vs 세 GDD
- **내용**: 3개 필라 중 "보는 맛" (오토배틀 관전 쾌감)을 직접 담당하는 GDD 또는 섹션이 없음. combat.md Game Feel 섹션의 "Animation Feel Targets", "Impact Moments"가 "[미정]" 상태
- **권장**: combat.md Game Feel 섹션에 "관전 쾌감을 만드는 연출 이벤트" 초안 작성 (배치 공개 순간, 크리티컬, 역전 모멘트 등)

#### D3-W3 (D-02 유지): 민병대 raw stats 지배 가능성
- **위치**: combat.md Unit Data
- **내용**: 민병대 총 DPS 80 (공동 1위) + 총 HP 160 (단독 1위). 패닉 메카닉(50% 이하 시 1초 공격 중단)이 충분한 페널티인지 미검증
- **권장**: 프로토타입 플레이테스트로 민병대 채용률과 승률 데이터 확보. 필요 시 패닉 페널티 강화 (예: 2초, 또는 패닉 중 받는 데미지 1.5배)

#### D3-W4 (D-03 유지): 패자 보상 부재 — 연패 이탈 리스크
- **위치**: economy.md:194 Open Questions — "패자에게도 소량 보상? [미정]"
- **내용**: PvP 게임에서 3:0 스윕 시 연패자 심리적 보상 부재
- **권장**: PvP 출시 전 필수 결정. MVP(AI 대전)에서는 비차단. Open Questions의 Deadline을 "PvP 출시 전"으로 명시

### Info

#### D3-I1: D-01 조건부 해결 — 점수 표시 UX 연출 필요
- **내용**: 판돈=점수가 MVP 싱크로 기능하려면 플레이어가 점수를 "의미 있게" 느껴야 함. 매치 종료 시 "이번 경기 총 판돈 XXX점" 연출, 최고 기록 표시 등
- **권장**: UX 스펙에서 다룰 것. GDD 수정 불필요

#### D3-I2: 배치 단계 cognitive load 6항목
- **내용**: 플레이어가 동시 관리하는 active 시스템 6개(내 손패, 상대 손패, 상대 이월, 배치 선택, 기권 판단, 판돈 현황). Miller's Law(7±2) 한계선
- **권장**: UX 레이아웃에서 구역 분리하여 시각적으로 정리

#### D3-I3: Player Fantasy 일관성 양호
- **내용**: "전략가-전술가-도박사"가 하나의 플레이어 정체성으로 통합 가능. deck.md "자원 관리자" 프레이밍만 주의 (Anti-pillar "복잡한 경영 게임이 아니다"와 긴장)

---

## Cross-System Scenario Issues (Phase 4)

**Scenarios walked**: 5

1. 3:0 스윕 승리 — Combat × Economy
2. 기권 라운드 실행 — Combat × Economy × Deck
3. 양측 동시 전멸로 3:3 동률 → 연장 라운드 — Combat × Economy × Deck
4. 함정 미발동 상태로 라운드 종료 — Combat × Economy
5. 최종 라운드 배치 시 덱 소진 가능성 — Deck × Combat

### Blockers (리뷰 중 인라인 해결됨)

#### S2-a (해결됨): 배치 장수 하한선 미정의
- **이전 상태**: combat.md Core Rule 3 "최대 5장"만 있고 하한선 미정의. economy.md AC "기권(최소 배치)"가 테스트 불가
- **해결**: 리뷰 중 인라인 수정 — 배치는 **5장 고정**, 0~4장 시 AI 자동 채움으로 변경
- **수정된 파일**: combat.md (Core Rule 3, 4; Edge Cases), economy.md (Strategic Dynamics, Edge Cases, Acceptance Criteria)
- **새로운 기권 정의**: "약한 카드 5장으로 채워 배치" (vs 이전 "최소 병력 투입")

### Warnings

#### S1-W1: 3:0 스윕 시 패자 경험 공백
- **시나리오**: 플레이어 A 3:0 스윕 → B는 5R 중 3R만 플레이, 판돈 0, 2R 미진행
- **문제**: D3-W4(D-03 패자 보상)와 직결. MVP에서는 비차단, PvP 전 해결 필요

#### S2-W1: "약한 카드" 기준이 D3-W3(민병대)와 모순 가능성
- **시나리오**: 기권 전략 실행 시 어떤 카드가 "약한 카드"인지 모호. 민병대가 raw stats 최강이면 "약한 카드" 선택 근거가 무너짐
- **문제**: 프로토타입에서 병종별 총 전투력 비교 후 "기권용 카드"의 실질적 후보를 식별 필요 (예: 궁병 총 DPS 30이 가장 낮으므로 궁병 5장이 기권 배치 후보)

### Info

#### S3-I1: 5판 표현과 연장 라운드의 모호성
- **시나리오**: 3:3 동률 → 추가 라운드 진행. 덱은 6R까지도 충분(15장 잔여)
- **문제**: combat.md Tuning Knob "선승 조건 3선승(5판)"과 추가 라운드가 모순적으로 읽힐 수 있음
- **권장**: combat.md에 "선승 조건 우선, 판수는 유연" 명시 (1줄 추가)

#### S4-I1: 미발동 함정이 "전멸 판정"에 포함되는지 GDD 미명시
- **시나리오**: 함정 HP 1 + 미발동 시 A측 "잔존 병사"에 함정이 포함되면 "한쪽 전멸" 판정 오작동 가능
- **권장**: combat.md에 "함정은 전멸 판정에서 제외"를 명시하거나, Battle Simulation 섹션에서 별도 서술

#### S5-I1: deck.md Formulas의 드로우/배치 총합 표현 혼동
- **시나리오**: "최대 5라운드: 5장 × 5 = 25장 소모"는 배치 기준. 드로우 총합은 28장(1R 8 + 2~5R 각 5)
- **권장**: deck.md Formulas에 "배치 소모 기준" 명시

---

## GDDs Flagged for Revision

| GDD | Reason | Type | Priority |
|-----|--------|------|----------|
| combat.md | 배치 제한 시간 [미정] (D3-W1) | Completeness | Warning |
| combat.md | Game Feel 섹션 "[미정]" 2건 — 보는 맛 필라 담당 (D3-W2) | Design Theory | Warning |
| economy.md | 패자 보상 미정 (D3-W4) | Design Theory | Warning (PvP 전 해결) |
| deck.md | Cross-References stale reference (C2-W1) | Consistency | Warning |
| combat.md / economy.md | squad_size single source of truth (C2-W2) | Consistency | Warning |

---

## Verdict: PASS (CONCERNS)

**이전 Blocking 3개(C-01, C-02, D-01) 모두 해결 확인.**
**Phase 4에서 발견된 Blocker 1개(S2-a)는 리뷰 중 인라인 수정으로 해결.**

잔존 Warning 9건(Phase 2: 2건, Phase 3: 4건, Phase 4: 2건, 기타: 1건)은 아키텍처 진행을 차단하지 않습니다. 대부분 프로토타입 플레이테스트(D3-W3 민병대, S2-W1 약한 카드), UX 스펙 작성(D3-I1, D3-I2), 사소한 문구 보완(C2-W1, C2-W2, S3-I1, S4-I1, S5-I1)으로 해결 가능합니다.

### 다음 단계 권장

1. **/create-architecture 시작 가능** — Blocking 없음
2. **동시 진행 가능한 개선 작업** (Warning 해소):
   - combat.md 배치 제한 시간 확정 (최소 30s 권장)
   - combat.md Game Feel 섹션 "보는 맛" 연출 초안 작성
   - deck.md Cross-References 정리
3. **프로토타입 플레이테스트로 검증**:
   - 민병대 밸런스 (D3-W3)
   - 기권 전략의 "약한 카드" 후보 (S2-W1)
4. **PvP 출시 전 필수**: 패자 보상 결정 (D3-W4)

---

## 인라인 수정 내역 (2026-04-23)

Phase 4 Blocker 해결을 위한 수정:

### combat.md

**Core Rule 3 (line 38) 변경**
```
이전: 각자 5x5 그리드에 최대 5장을 비밀 배치한다
이후: 각자 5x5 그리드에 정확히 5장을 비밀 배치한다 (배치는 항상 5장 고정)
```

**Core Rule 4 (line 39) 변경**
```
이전: 배치 시간 초과 시 AI가 남은 카드를 랜덤 배치한다
이후: 배치 장수가 5장 미만인 상태로 확정 또는 시간 초과 시, AI가 손패에서 랜덤 카드를 랜덤 위치에 채워 5장을 완성한다
```

**Edge Cases 통합 (line 177-178)**
```
이전: "배치 시간 초과" + "배치를 1장도 하지 않음" 두 행
이후: "배치 장수 0~4장 (시간 초과 포함)" 단일 행 — AI 자동 채움, 5장 고정 명시
```

### economy.md

**Strategic Dynamics (line 50) 변경**
```
이전: 최소 병력만 투입 → 상대가 가져갈 판돈을 줄임
이후: 약한 카드 5장으로 채워 배치 → 전멸 시 상대가 가져갈 판돈을 줄임 (배치는 항상 5장 고정)
```

**Edge Cases (line 120) 변경**
```
이전: 기권(최소 배치) 라운드 | 적은 병사 → 적은 판돈 적립
이후: 기권(약한 카드 5장) 라운드 | 약한 카드의 낮은 squad value → 적은 판돈 적립
```

**Acceptance Criteria (line 184) 변경**
```
이전: 기권(최소 배치) 시 적립 판돈이 감소한다
이후: 기권(약한 카드 5장 배치) 시 적립 판돈이 감소한다 (예: 궁병 5장 배치 vs 기병 5장 배치 비교)
```
