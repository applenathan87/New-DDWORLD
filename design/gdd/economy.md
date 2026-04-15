# Economy System (경제시스템)

> **Status**: In Review
> **Author**: nathan (볼트 GDD 이전)
> **Last Updated**: 2026-04-15
> **Last Verified**: 2026-04-15
> **Implements Pillar**: 역전의 희열 — 지고 있어도 뒤집을 수 있는 희망

## Summary

전투에서 살아남은 병사가 곧 보상이 되는 '판돈(Pot)' 시스템. 라운드마다 잔존 병사의
가치가 중앙 판돈에 쌓이고, 최종 승자가 누적 판돈을 모두 가져간다. 초반에 지더라도
판돈이 크게 쌓여 후반 역전 시 막대한 보상을 얻는 구조를 만든다.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `Combat`

## Overview

DDworld의 경제시스템은 전투 결과와 보상을 직접 연결하는 '판돈(Pot)' 구조다. 매 라운드
종료 후 살아남은 병사의 수와 HP를 기반으로 가치를 계산하여 중앙 판돈에 적립한다.
5판 3선승 후 최종 승자가 누적 판돈을 전부 획득한다. 이 구조는 "질 때는 적게 잃고,
이길 때 크게 먹는" 전략적 판단을 가능하게 하며, 후반 역전의 희열을 만든다.

## Player Fantasy

- **역전의 희열** — 초반 2패로 판돈이 크게 쌓인 상태에서 후반 역전 시 막대한 보상
- **전략적 손절** — 이길 수 없는 판에 최소 병력만 투입하여 상대 보상을 줄이는 플레이
- **리스크/리워드 판단** — 강한 카드를 지금 쓸지, 나중을 위해 아낄지의 긴장감

## Detailed Design

### Core Rules

1. 매 라운드 종료 시, 살아남은 병사의 가치를 계산한다
2. 계산된 가치를 중앙 '판돈(Pot)'에 적립한다
3. 5판 3선승 후, 최종 승자가 누적된 판돈을 전부 획득한다
4. 패자는 판돈을 얻지 못한다
5. 획득한 판돈은 **덱 강화 통화**로 사용된다 (다음 매치 전 덱 구성 변경/업그레이드)

### States and Transitions

| State | Entry Condition | Exit Condition | Behavior |
|-------|----------------|----------------|----------|
| Round Settle | Combat 라운드 종료 | 적립 완료 | 잔존 병사 가치 계산 → 판돈 적립 |
| Accumulate | 적립 완료 | 다음 라운드 또는 매치 종료 | 누적 판돈 업데이트 |
| Final Payout | 3선승 달성 | 정산 완료 | 승자에게 누적 판돈 전액 지급 |

### Strategic Dynamics

- **기권(Fold) 전략**: 이길 수 없는 판에 최소 병력만 투입 → 상대가 가져갈 판돈을 줄임
- **올인(All-in) 전략**: 강한 카드를 집중 투입 → 많은 병사 생존 → 판돈 크게 적립
- **역전 구조**: 초반 2패 → 판돈 크게 축적 → 후반 역전 시 모든 판돈 획득

### Interactions with Other Systems

| System | Direction | Data Flow |
|--------|-----------|-----------|
| Combat | Economy ← Combat | 라운드 종료 시 잔존 병사 수/HP 수신 |
| Deck | Economy → Deck | 판돈 통화로 다음 매치 전 덱 구성 변경/업그레이드 |
| Meta Progression | Economy가 곧 메타 통화 | 판돈 = 덱 강화 통화 (MVP 수준 간단한 메타 성장) |

## Formulas

### Pot Contribution (라운드 판돈 적립)

```
round_pot = sum(alive_soldiers[i].value) for each surviving unit
soldier_value = base_value * (current_hp / max_hp)
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| alive_soldiers | array | 0-25 | Combat result | 라운드 종료 시 생존 병사 목록 (개별 병사 단위) |
| base_value | float | 계산값 | UnitData SO | 병종별 병사 1명 가치 = squad_value / squad_size |
| squad_value | float | 10 | config | 분대당 균등 가치 (모든 병종 동일) |
| current_hp | float | 0-max | Combat result | 현재 HP |
| max_hp | float | > 0 | UnitData SO | 최대 HP |

**분대당 균등 가치 모델:**

| 병종 | 분대 인원 | 분대 가치 | 병사 1명 가치 (base_value) |
|------|----------|-----------|---------------------------|
| 기병 (Cavalry) | 5명 | 10 | 2.0 |
| 창병 (Spearman) | 10명 | 10 | 1.0 |
| 궁병 (Archer) | [미정] | 10 | [미정] |
| 민병대 (Militia) | 20명 | 10 | 0.5 |
| 함정 (Trap) | [미정] | [미정] | [미정 — 특수 유닛] |

어떤 병종으로 이기든 풀 분대 생존 시 판돈 기여값이 동일하다.
기권 전략의 핵심: 약한 카드로 채워 적의 판돈 적립을 최소화.

**Expected output range**: 0 (전멸) ~ 50 (배치 5분대 × 10 = 풀 생존 최대치)

### Total Pot (누적 판돈)

```
total_pot = sum(round_pot[r]) for r = 1 to current_round
```

### Winner Payout (최종 정산)

```
winner_reward = total_pot * winner_multiplier
```

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| winner_multiplier | float | 1.0 | config | 최종 보상 배수 (기본 1.0) |

## Edge Cases

| Scenario | Expected Behavior | Rationale |
|----------|------------------|-----------|
| 양쪽 모두 병사 전멸 (무승부) | 판돈 적립 0 (양쪽 생존 병사 없음). 양쪽 모두 1승 획득 (Combat 규칙) | 생존 병사 없으면 가치 0, 판돈에 기여 없음 |
| 3:0 스윕 승리 | 정상 정산 — 3라운드 판돈만 누적 | 판돈이 적어 스윕은 보상이 낮음 |
| 3:2 접전 승리 | 정상 정산 — 5라운드 판돈 누적 | 접전일수록 판돈 커짐 |
| 기권(최소 배치) 라운드 | 적은 병사 → 적은 판돈 적립 | 기권 전략의 보상 |

## Dependencies

| System | Direction | Nature of Dependency |
|--------|-----------|---------------------|
| Combat | Economy depends on Combat | 잔존 병사 수/HP 데이터 |
| Meta Progression | [미정] | 판돈을 메타 자원으로 변환 |
| Deck | [미정] | 메타 성장으로 덱 구성 변경 가능성 |

## Tuning Knobs

| Parameter | Current Value | Safe Range | Effect of Increase | Effect of Decrease |
|-----------|--------------|------------|-------------------|-------------------|
| squad_value (분대 가치) | 10 | 5 ~ 20 | 판돈 스케일 증가, 매치 보상 증가 | 판돈 작아짐 |
| winner_multiplier | 1.0 | 0.5 ~ 2.0 | 승리 보상 증가 | 승리 보상 감소 |
| HP 비율 반영 여부 | 반영 (current/max) | on/off | 정밀한 가치 산정 | 생존 여부만 판단 |

## Visual/Audio Requirements

| Event | Visual Feedback | Audio Feedback | Priority |
|-------|----------------|---------------|----------|
| 판돈 적립 | 병사 → 코인/가치 변환 연출 | 적립 사운드 | MVP |
| 누적 판돈 증가 | 중앙 판돈 UI 업데이트 | [미정] | MVP |
| 최종 정산 | 승자에게 판돈 쏟아지는 연출 | 보상 획득 사운드 | Alpha |

## Game Feel

### Feel Reference

포커의 팟(pot) — 판돈이 쌓일수록 긴장감 상승. 테이크 더 웨스트의 역전 보상 —
밀리다가 뒤집었을 때의 쾌감.

### Weight and Responsiveness Profile

- **Weight**: 판돈 적립은 시각적으로 "쌓이는 느낌", 최종 정산은 "폭발적 보상"
- **Failure texture**: 패배 시에도 "판돈을 적게 줬다"는 전략적 만족감이 남아야 함

### Feel Acceptance Criteria

- [ ] 판돈이 쌓이는 것이 시각적으로 느껴진다
- [ ] 역전 승리 시 보상이 클 때 "대박" 느낌이 든다
- [ ] 기권 전략이 유효하다고 느껴진다 (무의미한 패배가 아님)

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
|-------------|-----------------|-----------------|-----------|
| 현재 라운드 판돈 | [미정] | 라운드 종료 시 | 전투 결과 화면 |
| 누적 판돈 | 화면 중앙 상단 | 라운드 종료 시 | 항상 |
| 최종 보상 | 매치 종료 화면 | 매치 종료 시 | 승리 시 |

## Cross-References

| This Document References | Target GDD | Specific Element Referenced | Nature |
|--------------------------|-----------|----------------------------|--------|
| 잔존 병사 수/HP | `design/gdd/combat.md` | Battle result (alive_count, HP) | Data dependency |
| 카드 배치 수 → 투입 병력 | `design/gdd/deck.md` | 배치 카드 수 (5장) | Rule dependency |

## Acceptance Criteria

- [ ] 라운드 종료 시 잔존 병사의 가치가 정확히 계산된다
- [ ] 판돈이 라운드마다 누적된다
- [ ] 3선승 달성 시 승자가 누적 판돈을 획득한다
- [ ] 기권(최소 배치) 시 적립 판돈이 감소한다
- [ ] 3:0 승리보다 3:2 승리의 누적 판돈이 크다
- [ ] 밸런스 값에 하드코딩 없음 (ScriptableObject/config 사용)

## Open Questions

| Question | Owner | Deadline | Resolution |
|----------|-------|----------|-----------|
| 병종별 base_value 기준은? | nathan | 해결됨 | 분대당 균등 가치 (squad_value=10, 병사당 = 10/인원) |
| 판돈 외 메타 자원이 있는가? | nathan | 해결됨 | MVP에서는 판돈이 유일한 통화 (덱 강화용) |
| 패자에게도 소량 보상? | nathan | [미정] | 연패 시 이탈 방지 고려. 프로토타입 후 결정 |
| 시즌/랭크 시스템과의 연결? | nathan | MVP 범위 밖 | 향후 확장 |
| 덱 강화로 뭘 살 수 있는가? | nathan | [미정] | 새 병종? 스탯 업? 특수 카드? 프로토타입 후 결정 |
