# ADR-001: 비동기 PvP를 MVP 대전 모드로 채택

> **Status**: Accepted
> **Date**: 2026-05-25
> **Decider**: nathan
> **Tags**: networking, multiplayer, scope, mvp

---

## Context

DDworld의 핵심 가치 중 하나는 **PvP 심리전** (Pillar 1). 그러나 실시간 동기 PvP는 1인 인디 개발에 결정적 부담:

- **매칭 서버** 구축/운영 비용
- **네트워크 동기화** 복잡도 (lag compensation, 상태 일관성)
- **디스커넥션 처리** 로직
- **플레이어 풀 부족 시 매칭 대기 시간** (인디 출시 초반에 결정적 약점)
- **글로벌 매칭** 시 지연/시간대 문제

연구 결과 ([design/research/comparisons/](../../design/research/comparisons/)) 에서:

| 게임 | PvP 방식 | 개발 규모 | 참고점 |
|------|---------|---------|--------|
| **The Bazaar** | 비동기 (고스트) | 7년 + 팀 | 인디는 아니지만 비동기 PvP 성공 사례 |
| **Despot's Game** | 비동기 (King of the Hill) | 인디 스튜디오 + tinyBuild | **인디 비동기 PvP 검증 사례** |
| **TFT** | 동기 8인 | AAA (Riot Games) | 1인 인디로는 도달 불가 |

DDworld의 게임플레이는 본질적으로 **턴제에 가까움** (배치 → 자동 전투):
- 실시간 입력이 필요한 단계가 거의 없음
- 양측 배치만 결정되면 결과는 결정론적 시뮬레이션
- → **동기화의 필요성이 낮음**

---

## Decision

**MVP 대전 모드는 비동기 PvP로 한다.**

### 세부 구조

1. **초기 진입**: AI 대전 (튜토리얼 + 진입 장벽 낮춤)
2. **일정 진척 도달 시**: 비동기 PvP 잠금 해제
3. **대전 방식**: 다른 플레이어의 배치 데이터(고스트)와 대전
   - 매치 데이터 = 결정론적 시드 + 양측 배치 + 영웅 정보로 완전 직렬화
   - 클라이언트에서 결과 시뮬레이션 (서버는 데이터 전달만)
4. **동기 실시간 PvP**: **Post-1.0 확장 옵션 모드** (여력 있을 때)

### 진입 패턴 (Despot's Game 영감)

```
신규 플레이어
  ↓
튜토리얼 (AI 대전)
  ↓
AI 대전 N매치 클리어 (또는 첫 캠페인 클리어)
  ↓
비동기 PvP 잠금 해제
  ↓
다른 플레이어 고스트와 매칭 시작
  ↓
ELO/MMR 기반 매칭 점진 향상
```

---

## Consequences

### Positive (긍정적)

1. **1인 인디 운영 부담 폭감**
   - 실시간 서버 X, 매칭 큐 X, 동기화 X
   - BaaS (Firebase, PlayFab 등) 정도면 충분
2. **플레이어 풀 작아도 작동**
   - 100명 미만에서도 게임 성립 (저장된 고스트가 있으면)
3. **매칭 대기 시간 0**
   - 즉시 시작 가능
4. **글로벌 시간대 무관**
   - 한국 새벽에도 미국 플레이어 고스트와 대전 가능
5. **Multiplayer-Ready 원칙이 더 견고해짐**
   - 결정론 + 직렬화가 절대 요구사항 → 코드 품질 강제
6. **디스커넥션 개념 자체가 없음**
   - 매치 완결성 자동 보장
7. **장기적 데이터 가치**
   - 고스트 데이터 = 메타 변화 추적 가능

### Negative (부정적)

1. **실시간 PvP의 짜릿함 일부 손실**
   - "지금 이 순간 진짜 상대" 감각 약화
2. **즉각적 상호작용 불가**
   - 채팅, 감정 표현, 도발 등
3. **메타 진화 속도 느림**
   - 고스트가 옛 메타 시점이라 카운터 발견에 시간차
4. **"봇 같다" 인식 위험**
   - 비동기를 명확히 알리지 않으면 AI와 구분 안 됨

### Mitigations (완화책)

| 위험 | 완화 방안 |
|------|---------|
| "봇 같다" 인식 | 고스트의 **원래 플레이어 정보 명시** (닉네임, 시점, 사용 영웅, 그 플레이어의 통계) |
| 즉각적 상호작용 부재 | **매치 후 비동기 챌린지** ("이 플레이어에게 도전 보내기"), 댓글/리액션 |
| 메타 진화 속도 | 고스트 풀 갱신 빈도 조절 (최근 고스트 가중치) |
| 실시간 욕구 | Post-1.0 동기 PvP 옵션 모드로 보완 |

---

## Alternatives Considered

### Option A: 처음부터 동기 실시간 PvP (TFT 패턴)
**❌ 거부**
- 1인 인디 스코프 폭증 (서버 + 매칭 + 동기화 + lag compensation)
- AAA 스튜디오 영역
- 출시 초기 플레이어 풀 부족 시 매칭 불가능

### Option B: AI 전용 (멀티 없음)
**❌ 거부**
- Pillar "심리전"의 깊이가 AI로는 불충분
- 시장 차별점 약함 (싱글 오토배틀러는 포화)
- 장기 동기 부여 약함

### Option C: 비동기 PvP (Bazaar/Despot's 패턴)
**✅ 채택**
- 위 장단점 분석 참조

### Option D: 로컬 멀티 (같은 PC 핫시트)
**❌ 거부**
- 시장 규모 너무 작음
- "심리전 = 배치 비공개"인데 같은 PC는 비공개 보장 어려움

### Option E: 하이브리드 (동기 + 비동기 둘 다 MVP)
**❌ 거부**
- MVP에 두 모드는 스코프 폭발
- 동기 모드 부담은 옵션 A와 동일

---

## Implementation Notes

### 매치 데이터 직렬화 구조 (개요)

```
MatchData {
  matchId: string
  randomSeed: int                     // 결정론적 시뮬레이션 시드
  rules: { rounds: 5, winsRequired: 3, ... }
  playerA: {
    playerId, nickname, hero,
    rounds: [
      { handDealt: [card], placement: [grid], carryover: [card] },
      ...
    ]
  }
  playerB: {
    playerId, nickname, hero, capturedAt: timestamp,  // 고스트 메타
    rounds: [...]
  }
  result: { winner, finalScore, finalPot }
}
```

### 인프라 선택지 (1인 인디 친화 BaaS)

| 옵션 | 장점 | 단점 |
|------|------|------|
| **Firebase** | Unity 통합 우수, 무료 티어 큼 | Google 의존 |
| **PlayFab** | 게임 특화 | MS 의존, 복잡 |
| **Supabase** | 오픈소스, PostgreSQL 기반 | Unity 통합 약함 |
| **자체 서버** | 완전 제어 | 1인 인디에 비현실적 |

→ **권장**: Firebase (MVP 시점). 향후 자체 인프라로 마이그레이션 가능 구조로 설계.

### 매칭 알고리즘 (개요)
- ELO/MMR 기반 실력대 매칭
- 신규 플레이어 보호: 데뷔 5매치는 가벼운 풀 또는 봇
- 고스트 풀 갱신: 최근 7일 우선, 그 외 가중치 감소

### 영구 저장 vs 임시 저장
- 본인 매치 기록: 영구 (리플레이/통계)
- 고스트 풀: 갱신 정책 별도 (예: 30일 후 자연 도태)

---

## Related Documents

### 내부
- [CLAUDE.md](../../CLAUDE.md) — Multiplayer-Ready 원칙
- [design/gdd/game-concept.md](../../design/gdd/game-concept.md) — Platform & Scope
- [design/gdd/combat.md](../../design/gdd/combat.md) — Edge Cases (이탈 처리)
- [design/gdd/systems-index.md](../../design/gdd/systems-index.md) — MVP/Post-MVP 분류

### 비교 분석
- [design/research/comparisons/bazaar.md](../../design/research/comparisons/bazaar.md)
- [design/research/comparisons/despots-game.md](../../design/research/comparisons/despots-game.md)
- [design/research/comparisons/tft.md](../../design/research/comparisons/tft.md)

### 향후 ADR 후보
- ADR-002: 매치 데이터 직렬화 포맷 (Protobuf vs JSON vs MessagePack)
- ADR-003: 인프라 선택 (Firebase vs 대안)
- ADR-004: 신규 플레이어 보호 정책 (AI 매칭 비율, ELO 보정)
- ADR-005: 고스트 데이터 갱신 정책

---

## Revision History

| Date | Author | Change |
|------|--------|--------|
| 2026-05-25 | nathan | 초안 작성, Accepted |
