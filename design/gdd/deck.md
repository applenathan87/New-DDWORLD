# Deck System (덱/카드 시스템)

> **Status**: In Review
> **Author**: nathan (볼트 GDD 이전)
> **Last Updated**: 2026-04-15
> **Last Verified**: 2026-04-15
> **Implements Pillar**: 심리전 — 읽기/역읽기의 기반

## Summary

매치 중 카드(병종)를 배분하고, 라운드 간 이월하며, 손패를 관리하는 시스템.
상대에게 손패가 공개되기 때문에 "어떤 카드를 가지고 있는지"가 심리전의 출발점이 된다.
드로우와 이월 규칙이 라운드 간 전략적 연속성을 만든다.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `Combat`

## Overview

DDworld에서 플레이어는 매 라운드 덱에서 카드를 드로우하고, 그중 5장을 그리드에 배치하며,
나머지 3장을 다음 라운드로 이월한다. 상대의 손패는 항상 공개되므로, 어떤 카드를 이월할지
자체가 심리전의 재료가 된다. 덱 시스템은 코어 루프의 시작점이자, 라운드 간 전략적
연속성을 제공하는 핵심 시스템이다.

## Player Fantasy

- **전략적 자원 관리** — 지금 배치할 카드와 다음을 위해 아끼는 카드를 고르는 판단
- **읽기의 재료** — 상대가 어떤 카드를 이월했는지 보고 다음 배치를 예측하는 재미
- **역전의 준비** — 이번 라운드를 포기하더라도 다음을 위해 강한 카드를 이월하는 전략

## Detailed Design

### Core Rules

1. 매치 시작 시 각 플레이어에게 공유 덱 풀에서 카드가 배분된다
2. 1라운드: 8장 드로우 (보유 0 + 드로우 8)
3. 2라운드 이후: 이월 3장 + 드로우 5장 = 손패 8장
4. 매 라운드 손패 8장 중 5장을 그리드에 배치한다
5. 배치하지 않은 나머지 3장은 자동으로 다음 라운드에 이월된다
6. 상대의 손패(8장)는 항상 전체 공개된다 (MVP 확정)

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|-------|----------------|----------------|----------|
| Draw | 라운드 시작 | 드로우 완료 | 덱에서 카드 배분 (1R: 8장, 2R+: 5장) |
| Hand | 드로우 완료 | 배치 단계 진입 | 손패 8장 확정, 상대에게 공개 |
| Placement | 배치 단계 | 배치 완료 | 5장 선택하여 그리드에 배치 |
| Carryover | 배치 완료 | 라운드 종료 | 배치하지 않은 3장이 자동으로 다음 라운드에 이월 |

### Card Flow Table

| 라운드 | 보유 | 드로우 | 손패 합계 | 배치 | 이월 |
|--------|------|--------|-----------|------|------|
| 1R | 0장 | 8장 | 8장 | 5장 | 3장 |
| 2R | 3장 | 5장 | 8장 | 5장 | 3장 |
| 3R~ | 3장 | 5장 | 8장 | 5장 | 3장 |

### Interactions with Other Systems

| System | Direction | Data Flow |
|--------|-----------|-----------|
| Combat | Deck → Combat | 배치된 5장의 카드 데이터 (병종, 스탯) 전달 |
| Economy | Economy → Deck (간접) | [미정 — 메타 성장으로 덱 구성 변화 가능성] |

## Formulas

### Draw Count

```
draw_count = (round == 1) ? initial_hand_size : refill_count
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| initial_hand_size | int | 8 | config | 1라운드 초기 드로우 수 |
| refill_count | int | 5 | config | 2라운드 이후 드로우 수 |
| carryover_count | int | 3 | config | 이월 카드 수 |

**Constraint**: `carryover_count + refill_count == initial_hand_size` (항상 손패 8장)

### Deck Composition (덱 구성)

양 플레이어는 동일한 구성의 45장 덱으로 시작한다. 카드는 중복 허용 (같은 병종 여러 장).

```
total_deck_size = cavalry + militia + spearman + archer + trap
               = 10 + 10 + 10 + 10 + 5 = 45
```

| 병종 | 장수 | 비율 |
|------|------|------|
| 기병 (Cavalry) | 10장 | 22% |
| 민병대 (Militia) | 10장 | 22% |
| 창병 (Spearman) | 10장 | 22% |
| 궁병 (Archer) | 10장 | 22% |
| 함정 (Trap) | 5장 | 11% |

**5판 소비 검증:**
- 매 라운드 배치 5장 소모, 이월 3장 재사용
- 최대 5라운드: 5장 × 5 = 25장 소모 + 이월 풀 3장 = 덱 잔여 17장
- **결론: 45장이면 5판 완주 가능. 카드 부족 없음**

**Note**: 프로토타입에서는 50장(함정 5 포함)이었으나, 검증 결과 45장이 정확한 수치.
향후 메타 성장 시스템에서 덱 구성 변경은 MVP 범위 밖 (확장 옵션).

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|----------|------------------|-----------|
| 덱에 드로우할 카드가 부족 | 발생하지 않음 (45장 덱 / 최대 5라운드 검증 완료) | Deck Composition 검증 참조 |
| 이월 카드가 3장 미만 (손패 부족) | 발생하지 않음 (매 라운드 항상 8장 보장) | 45장 덱 구성으로 5판 완주 가능 |
| 1라운드에서 8장 미만 드로우 | 발생하지 않음 (덱 크기 보장) | 덱 풀 설계로 방지 |

## Dependencies

| System | Direction | Nature of Dependency |
|--------|-----------|---------------------|
| Combat | Combat depends on Deck | 배치 카드 데이터 제공 |
| Economy | MVP 범위 밖 | 메타 성장 → 덱 구성 변경은 향후 확장 옵션 |

## Tuning Knobs

| Parameter | Current Value | Safe Range | Effect of Increase | Effect of Decrease |
|-----------|--------------|------------|-------------------|-------------------|
| 초기 드로우 (1R) | 8장 | 5 ~ 12 | 선택지 증가, 첫 라운드 복잡도 상승 | 단순한 오프닝 |
| 매 라운드 리필 | 5장 | 3 ~ 8 | 손패 변화폭 증가 | 이월 카드 비중 증가 |
| 이월 수 | 3장 | 1 ~ 5 | 라운드 간 전략 연속성 강화 | 매 라운드 독립적 |
| 배치 수 | 5장 | 3 ~ 8 | 그리드 활용도 증가 | 핵심 유닛에 집중 |
| 손패 공개 범위 | 전체 공개 (MVP 확정) | 전체 / 부분 / 비공개 | 심리전 강화 | 불확실성 증가 |

## Visual/Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|-------|----------------|---------------|----------|
| 카드 드로우 | 카드가 손패 영역으로 이동하는 애니메이션 | 드로우 사운드 | MVP |
| 상대 손패 공개 | 상대 카드가 뒤집히며 공개 | 공개 사운드 | MVP |
| 이월 카드 선택 | 선택된 카드 하이라이트 | 선택 확인음 | MVP |

## Game Feel

### Feel Reference

포커의 핸드 공개 — 상대 패를 볼 때의 긴장감. 슬레이 더 스파이어의 카드 선택 —
한정된 손패에서 최선을 고르는 판단.

### Weight and Responsiveness Profile

- **Weight**: 가볍고 빠른 카드 조작
- **Snap quality**: 카드 선택/배치가 크리스프하게 반응
- **Player control**: 높음 — 어떤 카드를 배치하고 이월할지 완전한 통제

### Feel Acceptance Criteria

- [ ] 카드 드로우 시 각 카드가 구분 가능하게 표시된다
- [ ] 상대 손패 공개가 자연스럽고 읽기 쉽다
- [ ] 이월 카드 선택이 직관적이다

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|-------------|-----------------|-----------------|-----------|
| 내 손패 (8장) | 화면 하단 | 드로우/배치 시 | 항상 |
| 상대 손패 (8장) | 화면 상단 | 라운드 시작 시 | 항상 |
| 남은 덱 수 | [미정] | 드로우 시 | [미정] |
| 이월 카드 선택 UI | 손패 영역 | 배치 후 | 이월 단계 |

## Cross-References

| This Document References | Target GDD | Specific Element Referenced | Nature |
|--------------------------|-----------|----------------------------|--------|
| 배치된 카드 → 전투 | `design/gdd/combat.md` | Placement state | Data dependency |
| 잔존 병사 → 이월 가치 판단 | `design/gdd/economy.md` | 판돈 적립 규칙 | Rule dependency |

## Acceptance Criteria

- [ ] 1라운드에 8장이 드로우된다
- [ ] 2라운드 이후 이월 3장 + 드로우 5장 = 손패 8장이 유지된다
- [ ] 손패에서 5장을 선택하여 그리드에 배치할 수 있다
- [ ] 배치 후 나머지 3장이 자동으로 이월된다
- [ ] 상대의 손패 8장이 공개된다
- [ ] 덱 셔플이 결정론적(deterministic)이다 (시드 기반)
- [ ] 카드 수량에 하드코딩 없음 (config 파일로 관리)

## Open Questions

| Question | Owner | Deadline | Resolution |
|----------|-------|----------|-----------|
| 덱 풀 구성은? | nathan | 해결됨 | 45장 (기병10/민병10/창병10/궁병10/함정5), 중복 허용 |
| 이월 방식은? | nathan | 해결됨 | 배치 후 나머지 3장 자동 이월 |
| 덱이 소진되면? | nathan | 해결됨 | 45장으로 5판 완주 가능, 소진 없음 |
| 메타 성장으로 덱 구성을 바꿀 수 있는가? | nathan | MVP 범위 밖 | 향후 확장 옵션으로 분리 |
