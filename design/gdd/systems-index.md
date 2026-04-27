# Systems Index — DDworld

> **Last Updated**: 2026-04-23
> **Purpose**: 전체 시스템의 authoritative 리스트. 계층, 의존성, 상태를 추적.
> **Authoritative source** for GDD 존재 여부, 우선순위, 설계 순서.

## 시스템 분류

### 설계 순서 (Design Order)

**Foundation** → **Core** → **Feature** → **Presentation** → **Polish**

## MVP Systems

| System | Layer | Priority | GDD | Status | Depends On | Depended By |
|--------|-------|----------|-----|--------|------------|-------------|
| Combat | Core | MVP | [combat.md](combat.md) | In Review (PASS) | Deck, UI | Economy |
| Deck | Core | MVP | [deck.md](deck.md) | In Review (PASS) | (none) | Combat |
| Economy | Core | MVP | [economy.md](economy.md) | In Review (PASS) | Combat | (none) |

## Pillars (from game-concept.md)

1. **심리전** — "읽었다!"의 쾌감 (구현: Combat, Deck)
2. **역전의 희열** — 지고 있어도 뒤집을 수 있는 희망 (구현: Economy)
3. **보는 맛** — 오토배틀의 관전 쾌감 (구현 담당: Combat Game Feel 섹션 — 미완성, D3-W2 참조)

## Anti-Pillars

- 조작 실력 게임이 아니다 (APM/반사신경 X)
- 랜덤 운 게임이 아니다 (드로우 운 X, 배치 판단)
- 복잡한 경영 게임이 아니다

## Post-MVP (확장 옵션)

| System | Layer | Priority | GDD | Status | Notes |
|--------|-------|----------|-----|--------|-------|
| Meta Progression | Feature | Post-MVP | — | Not Started | 덱 강화, 랭크, 시즌 등 |
| Multiplayer (PvP) | Feature | Post-MVP | — | Not Started | 현재 MVP는 AI 대전 |
| UI Systems | Presentation | MVP | — | Not Started | 배치 UI, 판돈 UI, 손패 UI |

## 리뷰 이력

| 일자 | 리뷰 유형 | Verdict | 링크 |
|------|----------|---------|------|
| 2026-04-16 | /review-all-gdds | FAIL (Blocking 3) | [review](reviews/gdd-cross-review-2026-04-16.md) |
| 2026-04-23 | /review-all-gdds (재리뷰) | PASS (CONCERNS) | [review](reviews/gdd-cross-review-2026-04-23.md) |

## 다음 단계

- [ ] /create-architecture 시작 가능 (Blocking 없음)
- [ ] Warning 해소: combat.md 배치 제한 시간 확정, Game Feel 섹션 보완
- [ ] 프로토타입 플레이테스트: 민병대 밸런스, 기권 전략 유효성 검증
- [ ] PvP 출시 전 필수: 패자 보상 결정

## Status 용어

| Status | 의미 |
|--------|------|
| Not Started | GDD 미작성 |
| In Draft | GDD 작성 중 |
| In Review (CONCERNS) | GDD 작성 완료, Warning 잔존 |
| In Review (PASS) | /review-all-gdds PASS, 아키텍처 진행 가능 |
| Needs Revision | 리뷰에서 Blocking 이슈 발견 |
| Approved | /design-review, /review-all-gdds 모두 통과 |
