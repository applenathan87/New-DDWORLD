# Systems Index — DDworld

> **Last Updated**: 2026-05-25 (오후, 런 구조 도입 반영)
> **Purpose**: 전체 시스템의 authoritative 리스트. 계층, 의존성, 상태를 추적.
> **Authoritative source** for GDD 존재 여부, 우선순위, 설계 순서.

## 시스템 분류

### 설계 순서 (Design Order)

**Foundation** → **Core** → **Feature** → **Presentation** → **Polish**

## MVP Systems

| System | Layer | Priority | GDD | Status | Depends On | Depended By |
|--------|-------|----------|-----|--------|------------|-------------|
| Combat | Core | MVP | [combat.md](combat.md) | In Review (3판 2선승 반영) | Deck, UI | Economy |
| Deck | Core | MVP | [deck.md](deck.md) | In Review (런 단위 덱 반영) | Run Mode | Combat |
| Economy | Core | MVP | [economy.md](economy.md) | In Review (라운드 수 반영) | Combat | Shop |
| **Run Mode** | Core | MVP | run-mode.md (예정) | Not Started | Combat, Shop, Deck | 모든 노드 컨테이너 |
| **Shop System** | Core | MVP | shop.md (예정) | Not Started | Card System, Economy | Deck |
| **Tree Map UI** | Presentation | MVP | — | Not Started | Run Mode | UX 핵심 |
| **HP System** | Core | MVP | — | Not Started | Run Mode | 런 종료 조건 |
| **Crypt (PvE Combat)** | Core | MVP | — | Not Started | Combat | Run Mode (크립 노드) |
| **Card Progression** | Core | MVP | — | Not Started | Deck, Shop | 카드 강화/패시브 추적 |
| AI Opponent | Core | MVP | — | Not Started | Combat | Async PvP (진입 게이트) |
| Async PvP (Ghost) | Core | MVP | — | Not Started | Combat, Ghost Data Store | — |
| Ghost Data Store | Infrastructure | MVP | — | Not Started | (BaaS — Firebase 등) | Async PvP |

## Pillars (from game-concept.md)

1. **심리전** — "읽었다!"의 쾌감 (구현: Combat, Deck)
2. **내 빌드의 서사** — 선택, 성장, 발견의 즐거움 (구현: Hero System, Combo/Synergy, Run Mode — 모두 Post-MVP)
3. **보는 맛** — 오토배틀의 관전 쾌감 (구현 담당: Combat Game Feel 섹션 — 미완성, D3-W2 참조)

## Anti-Pillars

- 조작 실력 게임이 아니다 (APM/반사신경 X)
- 랜덤 운 게임이 아니다 (드로우 운 X, 배치 판단)
- 복잡한 경영 게임이 아니다

## Post-MVP / 보류 (확장 옵션)

| System | Layer | Priority | GDD | Status | Notes |
|--------|-------|----------|-----|--------|-------|
| Match/Hand UI | Presentation | MVP | — | Not Started | 배치 UI, 판돈 UI, 손패 UI (MVP인데 GDD 미작성) |
| **Hero System** | Feature | **TBD (Q7 보류)** | — | Not Started | 영웅 선택으로 빌드 정체성. 개념 학습 후 결정 ([bazaar.md](../research/comparisons/bazaar.md) 영감) |
| Meta Progression | Feature | Post-MVP | — | Not Started | 랭크, 시즌, 영구 진척 등 |
| Combo / Synergy System | Feature | Post-MVP | — | Not Started | 위치 기반 시너지 ([research](../research/replayability-and-synergy.md)) |
| Deck Building (Light, Run 전) | Feature | Post-MVP | — | Not Started | 런 시작 전 병종 비율 사전 조정 |
| Sync PvP (Realtime) | Feature | **Post-1.0 옵션 모드** | — | Not Started | MVP는 비동기. 동기는 여력 시 추가 ([ADR-001](../../docs/architecture/ADR-001-async-pvp.md)) |

## 리뷰 이력

| 일자 | 리뷰 유형 | Verdict | 링크 |
|------|----------|---------|------|
| 2026-04-16 | /review-all-gdds | FAIL (Blocking 3) | [review](reviews/gdd-cross-review-2026-04-16.md) |
| 2026-04-23 | /review-all-gdds (재리뷰) | PASS (CONCERNS) | [review](reviews/gdd-cross-review-2026-04-23.md) |

## 다음 단계

작업 큐는 [production/work-queue.md](../../production/work-queue.md)에서 영구 추적.

**즉시 (Phase 2 — GDD 신규 작성)**:
- [ ] **`run-mode.md` 작성** — 런 구조, 노드 종류, 트리맵, HP 시스템, 종료 조건
- [ ] **`shop.md` 작성** — 카드 추가/강화/패시브, 가격, 통화

**그 다음**:
- [ ] **UI Systems GDD 작성** (배치 UI, 판돈 UI, 손패 UI, 트리맵 UI)
- [ ] **AI Opponent GDD 작성** (비동기 PvP 진입 게이트)
- [ ] **Async PvP GDD 작성** (고스트 데이터 구조, 매칭 로직)
- [ ] **Hero System 결정 (Q7)** — 개념 학습 후 도입 여부/시점 결정

**검증 / 보강**:
- [ ] 3판 2선승 변경 후 밸런스 검증 (5판 대비 매치 깊이 변화)
- [ ] 프로토타입 플레이테스트: 민병대 밸런스, 기권 전략 유효성
- [ ] Warning 해소: combat.md 배치 제한 시간 확정, Game Feel 섹션 보완

**아키텍처**:
- [ ] /create-architecture 시작 가능 (Blocking 없음, ADR-001 채택됨)
- [ ] 향후 ADR 후보: 매치 데이터 직렬화 포맷, BaaS 선택, 신규 플레이어 보호 정책

## 아키텍처 결정 기록

| ADR | 제목 | 일자 | Status |
|-----|------|------|--------|
| [ADR-001](../../docs/architecture/ADR-001-async-pvp.md) | 비동기 PvP를 MVP 대전 모드로 채택 | 2026-05-25 | Accepted |

## Status 용어

| Status | 의미 |
|--------|------|
| Not Started | GDD 미작성 |
| In Draft | GDD 작성 중 |
| In Review (CONCERNS) | GDD 작성 완료, Warning 잔존 |
| In Review (PASS) | /review-all-gdds PASS, 아키텍처 진행 가능 |
| Needs Revision | 리뷰에서 Blocking 이슈 발견 |
| Approved | /design-review, /review-all-gdds 모두 통과 |
