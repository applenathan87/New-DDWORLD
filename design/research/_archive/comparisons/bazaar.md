# The Bazaar vs DDworld

> **Status**: Research / Comparative Analysis
> **Author**: nathan
> **Last Updated**: 2026-05-25
> **Trigger**: "이번 런에서 내 덱이 이런 요소들을 얻어서 이런 방향으로 가게 되었다"는 감각을 원함 (2026-05-25 세션)
> **Purpose**: Bazaar가 어떻게 "빌드 서사" 감각을 PvP 오토배틀러에 도입했는지 분석하여, DDworld가 차용 가능한 구조를 식별한다.

---

## 1. 게임 개요

| 항목 | 내용 |
|------|------|
| **개발/배급** | Tempo Storm (Reynad — 전 Hearthstone 프로) |
| **출시** | 2025-08-13 (Steam) |
| **장르** | **비동기 PvP 오토배틀러 + 덱빌딩 + 로그라이크** |
| **개발 기간** | 7년+ (2017~) |
| **가격** | $40 프리미엄 (F2P → 프리미엄 전환) |
| **본인 정의** | Reynad: **"Multiplayer Slay the Spire"** |
| **현재 영웅** | 3명 기본 (Vanessa, Pygmalien, Dooley) + 확장 (Mak, Stelle, Jules, Karnok) |
| **시장 위치** | "빌드 서사 PvP" 카테고리 정의 |

## 2. 핵심 게임플레이 루프

```
영웅 선택 (각 영웅마다 다른 아이템 풀)
  ↓
시작 아이템 3개 중 1개 선택 (무료)
  ↓
Day 1 진입 — 6개의 Hour로 구성
  ↓ 각 Hour마다:
  ┌─────────────────────────────────────┐
  │ 이벤트 선택:                          │
  │  - 상점 (아이템 구매/판매/강화)         │
  │  - NPC 전투 (PvE)                    │
  │  - 보물 발견                          │
  │  - 강화 이벤트                        │
  │  - 기타                              │
  └─────────────────────────────────────┘
  ↓ Day 마지막 Hour:
PvP 전투 (다른 플레이어의 "고스트 빌드"와 비동기 대전)
  ↓
승리 → 다음 Day로 / 패배 → HP 감소
  ↓
10승 = 런 클리어 / HP 0 = 런 종료
```

### 핵심 메카닉

#### A. 영웅 = 빌드 정체성 시작점
- 각 영웅은 **사용 가능 아이템 풀이 다름**
- 영웅별 스킬 (Vanessa: 무기 마스터, Dooley: 로봇 미니언 등)
- → 완전 랜덤이 아닌 **방향성 있는 랜덤**

#### B. 아이템 = 성장의 단위
- 슬롯(보드)에 배치 — **위치 기반 시너지** (인접 슬롯 영향)
- 자동 발동 (쿨다운 기반)
- 합성 (작은 아이템 → 큰 아이템)
- 강화 (Enchantment 부여)
- 데미지 타입: Normal / Burn / Poison / Shield (상성)

#### C. 비동기 PvP — 인디 친화 패턴
- 매칭 큐 없음
- 다른 플레이어의 빌드 **고스트(스냅샷)** 와 전투
- 내가 잘 때 내 빌드도 다른 사람과 싸움
- 100명만 있어도 작동

#### D. Day/Hour 시계 — 명확한 페이싱
- 한 눈에 진행 상황 파악
- Hour = 짧은 결정 단위
- Day = 큰 마일스톤
- 10승 = 최종 목표

---

## 3. DDworld 비교 매트릭스

| 항목 | The Bazaar | DDworld 현재 | 비고 |
|------|----------|------------|------|
| **장르 정체성** | 비동기 PvP + 로그라이크 빌더 | 1v1 심리전 오토배틀러 | 같은 "PvP 오토배틀러" 가족 |
| **매치 구조** | 1런 = 10승까지 (수시간) | 1매치 = 5판 3선승 (10분) | ❗ 매우 다름 |
| **세션 길이** | 1~3시간 | 10~30분 (3매치까지) | |
| **PvP 방식** | **비동기 (고스트 전투)** | 동기 (실시간) 예정 | ⭐ Bazaar가 인디에 유리 |
| **빌드 정체성** | ⭐⭐⭐ 영웅 + 아이템 조합 | 5병종 고정 | ❗ **DDworld 약점** |
| **"이번 런/매치" 서사** | ⭐⭐⭐ 시그니처 | ❌ 부재 | ❗ **DDworld 문제점** |
| **변화의 원천** | 영웅별 아이템 풀에서 랜덤 | 매 라운드 드로우 + **상대 가변성** | 다른 종류 |
| **포지셔닝** | 슬롯 인접 시너지 | 5x5 그리드 배치 | ✅ 유사한 결 |
| **심리전** | 약함 (빌드 vs 빌드 스냅샷) | ⭐⭐⭐ 핵심 (배치 비공개) | ✅ **DDworld 차별점** |
| **운 요소** | 큼 (아이템 풀 RNG) | 작음 (대칭 덱) | 다른 설계 철학 |
| **숨겨진 정보** | 약함 | ⭐⭐⭐ 강함 (배치 비공개) | DDworld 강점 |
| **장기 진척** | 약함 (스킨/도전과제만 — 리뷰 불만) | 계획 단계 (메타 진행) | 둘 다 약점 |
| **인디 적합성** | 7년 + 팀 | 1인 인디 | ⚠️ Bazaar 풀스코프는 무리 |
| **가격** | $40 프리미엄 | $15~20 안내 (BM 리서치 §B) | |

---

## 4. DDworld가 배울 점 (Take-aways)

### 🎯 직접 차용 가치 (높은 우선순위)

#### A. **영웅 시스템 — 빌드 정체성 출발점** ⭐⭐⭐

**Bazaar 모델**: 영웅별 아이템 풀 차별화

**DDworld 적용 안**:
- 매치 시작 시 3명 영웅 중 1명 선택
- 각 영웅마다 **덱 비율 또는 특수 능력** 차별화:

| 영웅 (예시) | 덱 차이 | 특수 능력 |
|-----------|--------|---------|
| **기병대장** | 기병 +5장 (총 15장), 민병대 -5장 | 기병 돌격 데미지 +10% |
| **수렵단장** | 궁병 +5장, 함정 +3장 | 궁병 명중률 +5% |
| **민병대 지휘관** | 민병대 +10장, 기병 -3장 | 민병대 패닉 시간 -0.5초 |
| **기본 사령관** | 균형 (현재 45장 구성) | 없음 |

**효과**:
- 매치 시작 즉시 "이번엔 기병 빌드" 감각 발생
- **상대 영웅 정보도 보임** → 심리전 강화 ("기병대장이면 기병 위주 배치 예상")
- 운 요소 없이 빌드 다양성

#### B. **매치 사이 "런" 구조** ⭐⭐⭐

**Bazaar 모델**: Day → Day 진행, 사이에 보상

**DDworld 적용 안**:
- 5판 3선승 = 1매치 (현재 유지)
- **3 매치 연승 = 1런 클리어** (Slay the Spire식)
- 매치 사이 **보상 선택 화면**:
  - "다음 매치 손패 +1장" vs "함정 데미지 +20%" vs "기병 +3장 임시 추가"
  - 3개 중 1개 선택
- **모드 분리**:
  - **퀵 매치** (현재 시스템) — 10분 단발
  - **런 모드** (신규) — 30~40분 캠페인
- 런 클리어 시 영구 보상 (영웅/스킨 언락)

#### C. **비동기 PvP** ⭐⭐⭐

**Bazaar 모델**: 다른 플레이어 빌드 고스트와 전투

**DDworld 적용 안 — "배치 고스트" 시스템**:
- 다른 플레이어의 배치 데이터를 서버에 저장
- 매칭 시 비슷한 실력대 고스트 가져옴
- 양쪽 비동기 플레이
- **심리전 보존**: 상대 배치 여전히 비공개. "당시 그 상대가 한 배치"를 봄
- 1인 인디 운영 부담 폭감 (실시간 동기화 X)

⚠️ 단점: 진짜 실시간 PvP의 짜릿함은 약함. **MVP는 비동기, Post-1.0에서 실시간 추가** 고려

#### D. **Day/Hour 같은 명확한 페이싱 UI** ⭐⭐

**Bazaar 모델**: 시계 UI로 진행감 시각화

**DDworld 적용 안**:
- "런 진행 바" (현재 매치 / 3 매치 중)
- "이번 런에서 누적 판돈" 시각화
- "획득한 보너스/카드" 리스트 표시 (영구 아이콘처럼)
- → **진행감 즉시 강화**

### ⚠️ 부분적 차용

#### E. **아이템 시스템 — 신중히**
- Bazaar의 아이템 풀 = 수백 개. DDworld는 무리
- 대안: **매치 사이 보상으로만 "보너스 카드" 20~30개**
- 라운드 내에는 추가하지 말 것 (심리전 약화 위험)

---

## 5. DDworld가 피할 점 (Anti-patterns)

### 🚫 Bazaar를 따라가면 위험한 것들

#### A. **수백 개 아이템 풀**
- Bazaar는 7년 + 팀이 만든 양
- 1인 인디 = 20~30개 보너스 카드부터
- 양보다 **조합 가능성**으로 깊이 확보

#### B. **7명 영웅**
- 1인 인디는 3명도 큰 작업
- DDworld 시작: 3명 (각각 명확한 정체성)
- Post-1.0: 4~5명 확장

#### C. **매치 길이 (1~3시간)**
- Bazaar의 약점 — "한 번 시작하면 못 멈춤"
- DDworld의 10분 매치는 차별점 — 유지
- 런 모드는 **선택지로** 추가 (강제 X)

#### D. **장기 진척 부재** ❗
- Bazaar 최대 불만: "10승 해도 스킨뿐"
- 리뷰 평가 깎임
- **DDworld는 처음부터 메타 진척 설계 필요** (replayability-and-synergy.md §11~§13)

#### E. **무거운 운 요소**
- Bazaar는 "좋은 아이템 못 뽑으면 답 없음" 불만
- DDworld의 대칭 + 손패 공개 유지
- 영웅 시스템 도입 시도 **양쪽 동일한 영웅 풀에서 선택** (대칭 보존)

#### F. **$40 가격대**
- 인디치고 비쌈 — 진입 장벽
- DDworld: $15~20 권장 (BM 리서치 §B)

---

## 6. 결정적 통찰 (Key Insight)

> **The Bazaar는 "오토배틀러 + Slay the Spire"라면, DDworld는 "Battleship + Slay the Spire"가 될 수 있다.**

같은 Slay the Spire식 런 구조를 차용하되:
- **Bazaar = 아이템 빌드 위주**, 심리전 약함
- **DDworld = 배치 심리전 위주**, 빌드는 영웅으로 결정

이 차별점이 DDworld의 새로운 시장 포지션:

> **"매치 단위로 끝나는 PvP 오토배틀러는 많다. 매치 안에서 심리전이 있는 게임은 없다. 매치 사이에 빌드 서사가 있는 PvP 심리전 오토배틀러는 더더욱 없다."**

---

## 7. DDworld 재설계 안 — "Bazaar 영감 버전"

### 단계적 도입 로드맵

| 단계 | 도입 요소 | 기간 |
|------|---------|------|
| **MVP** | 현재 시스템 유지 (5병종, 단발 매치) — 스코프 보호 | 현재 |
| **Beta** | **영웅 시스템 도입** (3명) | 6개월 |
| **1.0** | **런 모드** (3매치 연승) + 보상 선택 | 1년 |
| **Post-1.0** | 비동기 PvP, 영웅 확장 (4~5명), 영구 진척 | 1.5년+ |

### 모드 구조 (1.0 시점)

```
== 퀵 매치 모드 == (단발, 10분)
영웅 선택 → 1매치 (5판 3선승)
짧게 즐기는 사람용

== 런 모드 == (Bazaar 영감, 30~40분)
영웅 선택 → Match 1 → 보상 선택 → Match 2 → 보상 → Match 3
3매치 연승 = 런 클리어 (영구 보상)
빌드 서사 즐기는 사람용
```

---

## 8. Sources / 출처

- [The Bazaar — Official Site](https://playthebazaar.com/)
- [The Bazaar — Steam](https://store.steampowered.com/app/1617400/The_Bazaar/)
- [The Bazaar Review — Mobalytics](https://mobalytics.gg/news/guides/the-bazaar-review)
- [What is The Bazaar? — Mobalytics](https://mobalytics.gg/the-bazaar/guides/what-is-the-bazaar)
- [The Bazaar Gameplay — Game Rant](https://gamerant.com/bazaar-gameplay-reveal/)
- [The Bazaar — Game8](https://game8.co/articles/games/the-bazaar)
- [The Bazaar Reviews — Metacritic](https://www.metacritic.com/game/the-bazaar/)
- [The Bazaar Interview With Reynad — Noisy Pixel](https://noisypixel.net/the-bazaar-interview-reynad-asynchronous-pvp-deckbuilder/)
- [The Bazaar Steam Release & Monetization — Mobalytics](https://mobalytics.gg/the-bazaar/guides/steam-release-monetization-updates)
- [The Bazaar Beginner's Guide — Gaming.news](https://gaming.news/codex/the-bazaar-complete-beginners-guide-to-heroes-economy-combat/)
- [No game has consumed my 2025 like The Bazaar — Rogue.site](https://www.rogue.site/editorials/the-bazaar-explained-preview/)

---

## 9. Cross-References

- [design/gdd/game-concept.md](../../gdd/game-concept.md) — DDworld Pillars / Anti-Pillars
- [design/research/replayability-and-synergy.md](../replayability-and-synergy.md) — 콤보/덱빌딩 도입
- [design/research/monetization.md](../monetization.md) — BM 검토 (Bazaar의 $40 프리미엄 모델 참고)
- [design/research/comparisons/tft.md](tft.md) — TFT 비교 (성장 vs 심리전)
- [design/research/comparisons/despots-game.md](despots-game.md) — Despot's Game 비교 (인디 비동기 PvP 사례)
