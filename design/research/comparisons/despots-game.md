# Despot's Game vs DDworld

> **Status**: Research / Comparative Analysis
> **Author**: nathan
> **Last Updated**: 2026-05-25
> **Trigger**: 인디 규모의 로그라이크 오토배틀러 사례 + 비동기 PvP 패턴 학습 (2026-05-25 세션)
> **Purpose**: Despot's Game이 어떻게 인디 스코프로 로그라이크 오토배틀러를 만들고 비동기 PvP를 운영하는지 분석하여, DDworld의 현실적 벤치마크로 활용한다.

---

## 1. 게임 개요

| 항목 | 내용 |
|------|------|
| **개발** | Konfa Games (인디 스튜디오) |
| **배급** | tinyBuild |
| **출시** | 2022-09-29 (Steam) |
| **장르** | **전술 로그라이크 오토배틀러** (디스토피아 풍자) |
| **플랫폼** | PC, Mac, PS4/5, Xbox, iOS, Android (모든 주요 플랫폼) |
| **Steam 평가** | **82% 긍정 (1,289 리뷰) — Very Positive** |
| **컨셉** | "약한 인간들"을 무기로 무장시켜 던전 탐험 |
| **풀 제목** | Despot's Game: Dystopian Battle Simulator (구 Army Builder) |

## 2. 핵심 게임플레이 루프

```
파티 생성 (인간들 모집)
  ↓
던전 진입 — 절차적 생성 맵
  ↓
방 → 방 이동 (식량 소모)
  ↓ 각 방에서:
  ┌─────────────────────────────────────┐
  │ 이벤트 분기:                          │
  │  - 전투 (자동 전투)                   │
  │  - 상점 (무기/돌연변이/관 구매)         │
  │  - 보상 (인간 추가, 식량, 골드)         │
  │  - 이벤트 (선택지 분기)                │
  │  - 보스 룸 (층 종료)                  │
  └─────────────────────────────────────┘
  ↓
층 보스 격파 → 다음 층
  ↓
모든 층 클리어 = 메인 캠페인 종료
  ↓
[엔딩 후 잠금 해제] King of the Hill PvP 모드
```

### 핵심 메카닉

#### A. **간접 전투 (Indirect Combat)**
- 플레이어는 유닛 배치만 함
- 전투는 100% 자동
- 인간들이 가장 가까운 적을 공격
- → 반사신경 X, 전술적 사고만

#### B. **클래스 시스템**
- DPS / 탱크 / 디버퍼 / 버퍼 / 힐러 / 마법 캐스터
- 무기에 따라 클래스 결정 (장검 → 워리어, 활 → 궁수 등)
- **같은 클래스 다수 = 시너지 보너스**
- 최대 49명 파티

#### C. **식량 시스템 (Food System)** ⭐
- 방 이동 = 식량 소비
- 식량 = 파티 크기에 비례
- 식량 부족 → 인간들 약해짐
- → **자원 압박**으로 모험 페이싱 강화

#### D. **돌연변이 / 강화**
- 인간을 돌연변이로 변환 (영구 강화)
- 무기 슬롯 확장
- 패시브 효과 부여
- → **빌드 정체성** 형성

#### E. **King of the Hill — 비동기 PvP** ⭐ (시크릿 모드)
- 메인 캠페인 클리어 후 해금
- 본인 파티 vs 다른 플레이어 파티 (비동기)
- 점진적으로 더 강한 플레이어와 매칭
- HP는 매 전투마다 리셋
- 리더보드 경쟁

---

## 3. DDworld 비교 매트릭스

| 항목 | Despot's Game | DDworld 현재 | 비고 |
|------|--------------|------------|------|
| **장르 정체성** | 로그라이크 + 오토배틀러 + 던전 | 1v1 심리전 오토배틀러 | 같은 "오토배틀러" 가족, 다른 구조 |
| **개발 규모** | 인디 스튜디오 (소수) | 1인 인디 | ✅ 가까운 스코프 |
| **세션 길이** | 1~3시간 (런 단위) | 10~30분 (3매치까지) | 다른 길이 |
| **핵심 모드** | 싱글 PvE 캠페인 | PvP (목표) | 다른 방향 |
| **PvP** | 비동기 (시크릿 모드) | 동기 (실시간) 계획 | ⭐ Despot's 패턴 참고 가치 |
| **유닛 규모** | 최대 49명 | 분대 단위 (총 ~30~50명) | 비슷한 규모 |
| **유닛 분류** | 클래스 (6종) | 5병종 | DDworld가 더 좁음 |
| **시너지** | 클래스 누적 보너스 | (계획) 콤보 시스템 | 같은 방향성 |
| **빌드 정체성** | ⭐⭐ 무기/돌연변이로 형성 | ❌ 없음 | DDworld 약점 |
| **자원 압박** | 식량 시스템 | 판돈 (다른 결) | 다른 설계 |
| **변화의 원천** | 절차적 맵 + 무기 RNG | 손패 + 상대 가변성 | 다른 종류 |
| **숨겨진 정보** | 없음 (모두 공개) | ⭐⭐⭐ 강함 | DDworld 차별 |
| **심리전** | 없음 (PvE 위주) | ⭐⭐⭐ 핵심 | DDworld 차별 |
| **운 요소** | 큼 (절차 생성 + 무기 풀) | 작음 (대칭 덱) | 다른 철학 |
| **유머/톤** | ⭐⭐ 디스토피아 풍자 (시그니처) | 진지한 전장 | 다른 톤 |
| **Steam 평가** | 82% 긍정 (1,289) | (출시 전) | 인디 벤치마크 |
| **가격** | $14.99 | $15~20 안내 | 비슷한 가격대 |

---

## 4. DDworld가 배울 점 (Take-aways)

### 🎯 직접 차용 가치

#### A. **비동기 PvP 패턴** ⭐⭐⭐

**Despot's 모델**: 메인 캠페인 클리어 → King of the Hill PvP 잠금 해제

**DDworld 적용 안**:
- 처음에는 **튜토리얼/AI 대전**으로 진입 장벽 낮춤
- 일정 진척 도달 시 **비동기 PvP 잠금 해제**
  - "다른 플레이어의 배치 데이터 (고스트)와 대전"
- 1인 인디 친화 — 실시간 매칭/서버 부담 폭감
- 점진적 매칭으로 실력대 자연스럽게 분리

**Bazaar와 차이**: Bazaar는 처음부터 비동기 PvP, Despot's는 PvE 클리어 후 해금 → DDworld는 **둘의 중간** 가능 (튜토리얼 후 즉시 해금)

#### B. **클래스 시너지 시스템** ⭐⭐

**Despot's 모델**: 같은 클래스 N명 = 보너스 (3명 → +10%, 5명 → +25% 식)

**DDworld 적용 안**:
- `replayability-and-synergy.md` 콤보 시스템과 결합
- 5x5 그리드에서 **같은 병종 N장 배치 시 시너지 발동**
  - 예: 기병 3장 인접 → "돌격 진형" (돌격 데미지 +15%)
  - 예: 궁병 4장 한 열 → "일제 사격" (1초간 명중률 100%)
- 영웅 시스템과 결합하면 강력 (영웅별로 특정 시너지 강화)

#### C. **인디 스코프 벤치마크** ⭐⭐⭐

**Despot's 통계**:
- 1,289 리뷰 / 82% 긍정
- 모든 주요 플랫폼 출시
- 인디 스튜디오 (소수) 작업
- 출시 후 지속 업데이트 (PvP 모드 추가, 신규 콘텐츠)

**DDworld 시사점**:
- 1인 인디 → Steam 단일 플랫폼 출시 권장 (Despot's는 스튜디오라 다 플랫폼 가능)
- 출시 시점: 80%+ 긍정 + 500~1,000 리뷰 = 성공 가능 영역
- 출시 후 컨텐츠 추가 (PvP 모드 같은 큰 업데이트)로 장수

#### D. **자원 압박 메카닉 (식량 시스템)** ⭐⭐

**Despot's 모델**: 식량으로 모험 페이싱 + 의사결정 압박

**DDworld 적용 안 — "전쟁 자원" 메카닉 (Post-MVP)**:
- 매 라운드 사용 가능한 "동원력" 점수
- 강한 카드는 동원력 높게 소모 (기병 3, 민병대 1 등)
- 동원력 한도 내에서 5장 선택
- → 무조건 강한 카드만 5장 못 둠 (밸런스 강제)
- **현재 시스템과 충돌 가능** — 신중 검토 필요

⚠️ 단점: 결정 복잡도 ↑, 직관성 ↓

#### E. **유머/톤의 정체성** ⭐

**Despot's 모델**: 디스토피아 풍자가 시그니처 — "약한 인간을 소모품처럼 다룸"

**DDworld 시사점**:
- 게임의 **분위기/세계관**이 강한 정체성 만듦
- 진지한 전장 톤은 좋지만, **약간의 위트나 시그니처 요소** 고려 가치
- 예: 패배 시 병사들의 코믹한 반응? 영웅의 독특한 대사?

---

## 5. DDworld가 피할 점 (Anti-patterns)

### 🚫 Despot's 약점 / 위험 요소

#### A. **"오토 정리 버튼" 같은 자동화 함정**
- Despot's 리뷰 불만 #1: "배치를 자동으로 해주는 버튼이 결정의 재미를 깎음"
- DDworld 교훈: **배치를 자동화하지 말 것**. 시간 초과 시 자동 배치는 OK지만, 플레이어가 자발적으로 자동을 누르게 만들지 말 것

#### B. **49명 유닛의 시각적 혼란**
- Despot's는 유닛 수가 많아 누가 누군지 구분 어려움
- DDworld는 분대 단위 (5병종 × 5분대 = 25명 정도)가 정답
- → 가독성 우선

#### C. **싱글 PvE 위주의 한계**
- Despot's PvP는 시크릿 모드 = 메인 콘텐츠 아님
- 장기 동기 부여가 약함 (런 클리어 후 할 게 적음)
- DDworld는 **PvP가 메인**이라 이 문제 자동 회피

#### D. **무거운 운 요소**
- 절차 생성 맵 + 무기 RNG로 "운으로 졌다" 발생
- DDworld의 대칭 디자인이 이 문제 회피
- 영웅/보너스 도입 시도 **양쪽 동일한 풀** 유지

#### E. **풍자 톤의 양날의 칼**
- 매력적이지만 호불호 명확
- DDworld는 **글로벌 시장** 목표라면 톤은 중립적/보편적 권장
- 시그니처 요소는 미니멀하게

---

## 6. 결정적 통찰 (Key Insight)

> **Despot's Game은 "PvE 로그라이크가 메인, PvP는 보너스"인 인디 사례. DDworld는 "PvP 심리전이 메인, 로그라이크 요소는 보너스"의 거울상이 될 수 있다.**

### 두 게임의 거울 관계

| 측면 | Despot's | DDworld |
|------|---------|---------|
| 메인 모드 | PvE 로그라이크 | PvP 심리전 |
| 부가 모드 | 비동기 PvP (시크릿) | 로그라이크 런 (Bazaar 영감) |
| 핵심 가치 | 빌드 발견 + 풍자 | 심리전 + 역전 |
| 운 요소 | 큼 | 작음 |
| 인디 스코프 | 인디 스튜디오 | 1인 인디 |

**핵심**: Despot's는 DDworld가 **실제로 도달 가능한 인디 성공 사례**. Bazaar는 너무 크고(7년+팀), TFT는 너무 다른 시장(AAA).

### 직접 적용 가능한 3가지

1. **비동기 PvP** — 1인 인디에 결정적 (서버 부담 ↓)
2. **클래스 시너지 시스템** — DDworld 콤보 시스템의 검증된 패턴
3. **인디 벤치마크 수치** — 80%+ 긍정 / 1,000+ 리뷰 = 성공 영역

---

## 7. DDworld 적용 우선순위

| 우선순위 | 차용 요소 | 작업량 | 효과 |
|---------|---------|-------|------|
| ⭐⭐⭐ | 비동기 PvP 패턴 (MVP에 통합) | 중 | 1인 인디 운영 부담 ↓ |
| ⭐⭐⭐ | 클래스/병종 시너지 | 중 | 매치 내 깊이 ↑ |
| ⭐⭐ | 자동화 함정 회피 (UI 디자인 원칙) | 소 | 결정의 재미 보존 |
| ⭐⭐ | 인디 벤치마크 수치 추적 | 소 | 목표 명확화 |
| ⭐ | 자원 압박 (전쟁 자원) — Post-MVP | 대 | 검증 후 결정 |
| ⭐ | 시그니처 톤/위트 요소 | 소~중 | 정체성 강화 |

---

## 8. 인디 사례 데이터 (참고용)

DDworld 출시 목표 수치 (Despot's 벤치마크 기반):

| 지표 | Despot's (3년 운영) | DDworld 목표 (출시 6개월) |
|------|------------------|----------------------|
| Steam 리뷰 수 | 1,289 | 300~500 |
| Steam 긍정 비율 | 82% | 80%+ |
| 플랫폼 | 모든 주요 플랫폼 | Steam 단일 |
| 가격 | $14.99 | $15~20 |
| 출시 후 업데이트 | 다수 (PvP 모드 추가 등) | 정기 업데이트 + 큰 모드 1~2개 |

---

## 9. Sources / 출처

- [Despot's Game on Steam](https://store.steampowered.com/app/1227280/Despots_Game_Dystopian_Battle_Simulator/)
- [Despot's Game — Wikipedia](https://en.wikipedia.org/wiki/Despot%27s_Game)
- [Despot's Game Reviews — Metacritic](https://www.metacritic.com/game/despots-game/)
- [Despot's Game Review — Minireview.io](https://minireview.io/auto-battler/despot-s-game)
- [Despot's Game Review — Superjump Magazine](https://www.superjumpmagazine.com/despots-game-review/)
- [Despot's Game Review — Gamecritics](https://gamecritics.com/eugene-sax/despots-game-dystopian-army-builder-review/)
- [Despot's Game Review — But Why Tho?](https://butwhytho.net/2022/10/review-despots-game-dystopian-army-builder-brings-rogue-lite-to-auto-battlers-pc/)
- [Despot's Game Review — Cogconnected](https://cogconnected.com/review/despots-game-dystopian-army-builder-review/)
- [PvP Discussion — Steam Community](https://steamcommunity.com/app/1227280/discussions/0/5221353200090794163/)
- [Christmas Brawl Update: New PvP Mode — Steam Events](https://steamcommunity.com/app/1227280/eventcomments/4328520278457945168/)

---

## 10. Cross-References

- [design/gdd/game-concept.md](../../gdd/game-concept.md) — DDworld Pillars / Anti-Pillars
- [design/research/replayability-and-synergy.md](../replayability-and-synergy.md) — 콤보/시너지 도입 (Despot's 클래스 시너지와 동일 결)
- [design/research/comparisons/tft.md](tft.md) — TFT 비교 (성장 vs 심리전, 인디 vs AAA)
- [design/research/comparisons/bazaar.md](bazaar.md) — The Bazaar 비교 (런 구조, 비동기 PvP, 인디 한계)
