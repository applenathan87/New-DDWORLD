# Combat System (전투시스템)

> **Status**: Draft
> **Author**: nathan (볼트 GDD 이전)
> **Last Updated**: 2026-04-14
> **Last Verified**: 2026-04-14
> **Implements Pillar**: 심리전 — "읽었다!"의 쾌감

## Summary

5x5 그리드에 병종 카드를 비밀 배치한 뒤, 양측 배치가 동시에 공개되며 자동 전투가
진행되는 오토배틀 시스템. 플레이어의 판단(배치)이 전투 결과를 결정하며, 조작 실력이
아닌 두뇌 싸움이 핵심이다.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `Deck, Economy`

## Overview

DDworld의 전투는 고전 '배틀십'의 격자 배치와 '가위바위보' 상성을 결합한 심리전
오토배틀러다. 양 플레이어는 상대의 손패(보유 카드)를 볼 수 있지만, 5x5 그리드 위
어디에 배치했는지는 알 수 없다. 배치 완료 후 중앙 막이 해제되며 서로의 진형이
공개되고, 병사들이 병종별 행동 패턴에 따라 자동으로 전투한다. 한쪽 병사가 전멸하면
라운드가 종료된다.

## Player Fantasy

- **"읽었다!"의 쾌감** — 상대 카드를 보고 배치를 예측했는데 맞았을 때의 짜릿함
- **심리전의 긴장감** — 상대도 내 카드를 보고 있다는 사실에서 오는 읽기/역읽기
- **전투 관전의 몰입** — 내 판단이 맞았는지 지켜보는 오토배틀의 박진감
- **Anti-goal**: 조작 실력(APM)으로 승부가 갈리는 게임이 되어서는 안 됨

## Detailed Design

### Core Rules

1. 양 플레이어는 Deck 시스템에서 카드(병종)를 배분받는다
2. 상대의 손패가 공개된다 (어떤 카드를 가지고 있는지 알 수 있음)
3. 각자 5x5 그리드에 최대 5장을 비밀 배치한다
4. 배치 시간 초과 시 AI가 남은 카드를 랜덤 배치한다
5. 양측 배치 완료 → 중앙 막 해제 → 진형 공개
6. 자동 전투 개시 — 병종별 행동 패턴에 따라 진행
7. 한쪽 병사가 전멸하면 라운드 종료
8. 5판 3선승제 — 먼저 3라운드를 이긴 쪽이 매치 승리

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|-------|----------------|----------------|----------|
| Placement | 카드 배분 완료 | 양측 배치 완료 또는 시간 초과 | 5x5 그리드에 비밀 배치, 상대 손패 공개 |
| Reveal | 배치 완료 | 연출 종료 | 중앙 막 해제, 양측 진형 공개 |
| Battle | 진형 공개 완료 | 한쪽 전멸 | 병종별 행동 패턴에 따른 자동 전투 |
| Result | 전투 종료 | 정산 완료 | 승패 판정, 잔존 병사 → Economy 시스템으로 전달 |
| Match End | 3선승 달성 | 최종 정산 완료 | 누적 판돈 정산, 매치 종료 |

### Interactions with Other Systems

| System | Direction | Data Flow |
|--------|-----------|-----------|
| Deck | Combat ← Deck | 손패 카드 데이터 (병종, 수량) 수신 |
| Economy | Combat → Economy | 라운드 종료 시 잔존 병사 수/HP를 판돈 계산에 전달 |

## Formulas

### Type Advantage (상성 배수)

```
effective_damage = base_damage * type_advantage_multiplier
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| base_damage | float | 1-100 | UnitData SO | 병종 기본 공격력 |
| type_advantage_multiplier | float | 0.5-1.5 | data file | 상성 배수 |

**상성 매트릭스:**

| 공격 \ 방어 | 기병 | 창병 | 궁병 |
|-------------|------|------|------|
| **기병 (바위)** | 1.0 | 0.5 | 1.5 |
| **창병 (가위)** | 1.5 | 1.0 | 0.5 |
| **궁병 (보)** | 0.5 | 1.5 | 1.0 |

### Squad Damage (분대 데미지)

```
squad_damage = base_damage * type_advantage * (alive_count / max_count)
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| alive_count | int | 0-20 | runtime | 현재 생존 병사 수 |
| max_count | int | 5-20 | UnitData SO | 분대 최대 인원 |

**Expected output range**: 0 (전멸) ~ base_damage * 1.5 (풀 분대 + 상성 유리)

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|----------|------------------|-----------|
| 양쪽 유닛이 동시에 전멸 | [미정 — 공격자 우선 또는 무승부 처리] | 결정 필요 |
| 배치 시간 초과 | AI가 남은 카드를 랜덤 배치 | 게임 진행 보장 |
| 배치를 1장도 하지 않음 | AI가 전체 손패에서 5장 랜덤 배치 | 게임 진행 보장 |
| 상대 이탈 (disconnect) | AI가 남은 라운드 대행 | 매치 완결 보장 |
| 3선승 전에 카드가 부족 | [미정 — Deck 시스템과 연계 필요] | 결정 필요 |

## Dependencies

| System | Direction | Nature of Dependency |
|--------|-----------|---------------------|
| Deck | Combat depends on Deck | 손패 카드 데이터, 배치 가능 수량 |
| Economy | Economy depends on Combat | 잔존 병사 수/HP로 판돈 계산 |
| UI (PlacementUI) | Combat depends on UI | 그리드 배치 입력 수신 |

## Tuning Knobs

| Parameter | Current Value | Safe Range | Effect of Increase | Effect of Decrease |
|-----------|--------------|------------|-------------------|-------------------|
| 그리드 크기 | 5x5 | 3x3 ~ 7x7 | 전략 복잡도 증가, 배치 시간 증가 | 단순해짐, 빠른 게임 |
| 배치 카드 수 | 5장 | 3 ~ 8 | 전략 폭 넓어짐 | 핵심 선택에 집중 |
| 배치 제한 시간 | [미정] | 30s ~ 120s | 신중한 플레이 | 직관적 판단 유도 |
| 상성 유리 배수 | 1.5x | 1.2 ~ 2.0 | 상성 읽기 중요도 증가 | 순수 배치 전략 비중 증가 |
| 상성 불리 배수 | 0.5x | 0.3 ~ 0.8 | 역상성 시 큰 패널티 | 상성 무시 전략 가능 |
| 선승 조건 | 3선승 (5판) | 2선승 ~ 4선승 | 긴 매치, 역전 기회 증가 | 빠른 매치 |

### Unit Data (병종별)

| 병종 | 등급 | 분대 인원 | 특성 |
|------|------|----------|------|
| 민병대 (Militia) | 물량형 | ~20명 | 낮은 스탯, 다수 공격 |
| 보병 (Spearman) | 표준형 | ~10명 | 균형 |
| 기병 (Cavalry) | 정예형 | ~5명 | 높은 스탯, 소수 |
| 궁병 (Archer) | 원거리 | [미정] | 원거리 지원, 거리 조절 |
| 함정 (Trap) | 특수 | [미정] | [미정] |

## Visual/Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|-------|----------------|---------------|----------|
| 배치 완료 | 카드가 그리드에 안착하는 연출 | 배치 확인음 | MVP |
| 진형 공개 | 중앙 막 해제 연출 | 긴장감 있는 공개 사운드 | MVP |
| 상성 유리 타격 | 강조 이펙트 (크리티컬 느낌) | 강한 타격음 | MVP |
| 유닛 전멸 | 병사 쓰러지는 애니메이션 | [미정] | Alpha |

## Game Feel

### Feel Reference

배틀십의 "Hit/Miss" 공개 순간 — 상대 배치를 예측하고 맞았을 때의 짜릿함.
오토체스(TFT, Dota Underlords)의 전투 관전 — 내 배치가 맞는지 지켜보는 긴장감.
NOT 리얼타임 전략(스타크래프트) — APM이 필요하면 안 됨.

### Input Responsiveness

| Action | Max Input-to-Response Latency (ms) | Frame Budget (at 60fps) | Notes |
|--------|-----------------------------------|------------------------|-------|
| 카드 드래그 시작 | 50ms | 3 frames | 즉각 반응 필수 |
| 그리드 셀에 드롭 | 80ms | 5 frames | 스냅 피드백 |
| 배치 확정 버튼 | 100ms | 6 frames | |

### Animation Feel Targets

[미정 — 프로토타입 후 결정]

### Impact Moments

[미정 — 프로토타입 후 결정]

### Weight and Responsiveness Profile

- **Weight**: 배치는 가볍고 빠르게, 전투 관전은 무겁고 임팩트 있게
- **Player control**: 배치 단계에서 높은 컨트롤, 전투 단계에서 관전자 (개입 불가)
- **Snap quality**: 그리드 배치는 크리스프한 스냅, 전투는 스무스한 애니메이션
- **Failure texture**: 읽기 실패 시 "아, 거기였구나" — 공정하게 느껴야 함

### Feel Acceptance Criteria

- [ ] 카드 드래그가 즉각 반응하고 그리드 셀에 명확히 스냅된다
- [ ] 진형 공개 순간에 긴장감이 느껴진다
- [ ] 상성 유리 타격이 시각적으로 구분된다
- [ ] 전투 관전 중 "내 배치가 맞았다/틀렸다"가 바로 읽힌다

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|-------------|-----------------|-----------------|-----------|
| 상대 손패 | 화면 상단 | 라운드 시작 시 | 항상 |
| 내 손패 | 화면 하단 | 드로우/배치 시 | 항상 |
| 5x5 그리드 | 화면 중앙 | 실시간 | 항상 |
| 라운드 스코어 (X-X) | 화면 상단 | 라운드 종료 시 | 항상 |
| 배치 제한 시간 | 그리드 근처 | 1초마다 | 배치 단계 |
| 병사 HP | 유닛 위 | 실시간 | 전투 단계 |

## Cross-References

| This Document References | Target GDD | Specific Element Referenced | Nature |
|--------------------------|-----------|----------------------------|--------|
| 손패 카드 배분 | `design/gdd/deck.md` | 드로우/이월 규칙 | Data dependency |
| 잔존 병사 → 판돈 계산 | `design/gdd/economy.md` | 판돈 적립 공식 | Data dependency |

## Acceptance Criteria

- [ ] 5x5 그리드에 카드를 드래그하여 배치할 수 있다
- [ ] 상대 손패가 배치 단계에서 공개된다
- [ ] 상성 배수가 정확히 적용된다 (기병→궁병 1.5x, 기병→창병 0.5x 등)
- [ ] 병사 개별 HP가 독립적으로 연산된다
- [ ] 한쪽 전멸 시 라운드가 종료된다
- [ ] 5판 3선승 후 매치가 종료된다
- [ ] 배치 시간 초과 시 AI가 랜덤 배치한다
- [ ] 전투 연산이 16.6ms 이내에 완료된다
- [ ] 밸런스 값에 하드코딩 없음 (ScriptableObject 사용)

## Open Questions

| Question | Owner | Deadline | Resolution |
|----------|-------|----------|-----------|
| 양쪽 동시 전멸 시 처리 방법? | nathan | [미정] | |
| 병종별 구체적 행동 패턴 (창병, 궁병)? | nathan | [미정] | 기병: 일직선 돌진 확정. 나머지 미정 |
| 배치 제한 시간은 몇 초? | nathan | [미정] | |
| 민병대/함정의 상성 위치? | nathan | [미정] | 기본 3병과(기병/창병/궁병) 외 특수 유닛 |
| 그리드 배치에 제한 규칙이 있는가? (예: 궁병은 뒷줄만) | nathan | [미정] | |
