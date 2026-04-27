---
Day: 25
날짜: 2026-04-16
작업시간: 6
상태: 완료
---
[[데브로그/데브로그]]
# Day 25 — 프로젝트 리뷰 & 아트 바이블 코어 작성

## 한 일

### GDD 교차 리뷰 1차 (`/review-all-gdds`)
- combat.md, deck.md, economy.md 3개 GDD 동시 리뷰
- **Verdict: FAIL** — Blocking 3건 식별
  - C-01: economy.md 궁병/함정 데이터 미정
  - C-02: Deck↔Economy 의존성 기술 모순
  - D-01: 자원 싱크 미정의
- 리뷰 리포트: `design/gdd/reviews/gdd-cross-review-2026-04-16.md`

### Blocking 이슈 즉시 해소
- **C-01 해결**: economy.md 분대 가치 테이블에 궁병 10명/base_value=1.0, 함정 "판돈 계산 제외" 명시
- **C-02 해결**: 양쪽 GDD 모두 "Deck↔Economy = MVP 범위 밖"으로 통일
- **D-01 조건부 해결**: economy.md Core Rule 5 — "판돈 = 매치 승리의 점수" (덱 강화 통화 전환은 MVP 범위 밖)

### game-concept.md 신규 작성
- Elevator Pitch + Core Fantasy 정리
- 3대 Pillars 명시 (심리전 / 역전의 희열 / 보는 맛)
- 3대 Anti-Pillars 명시 (조작 실력 X / 랜덤 운 X / 복잡한 경영 X)
- Core Loop 다이어그램 + 매치 단위 경험 + 플레이어 감정 곡선
- 컨셉 진화 이력 (Day 1~13 스킬젬 액션 RPG → Day 22 심리전 오토배틀러까지 3번의 피봇)

### deck.md 용어 정리
- "이월 메카닉" 용어 수정: 선택 이월 → 자동 이월
- 손패 8장 = 5장 배치 + 3장 자동 이월 명시

### economy.md MVP 범위 정리
- 판돈 = 점수 역할만 (덱 강화 통화 전환은 확장 옵션으로 분리)
- Open Questions에 메타 자원 결정 상태 정리

### 아트 바이블 코어 4섹션 작성 (`design/art/art-bible.md`)
- HD-2D + Tilt-shift 비주얼 디렉션
- 카메라 시스템 (쿼터뷰 3D + 단계별 줌)
- 팔레트 스왑 전략 (인디 1인 에셋 효율)
- 컬러 시스템 (병종별 + UI)

### CLAUDE.md 멀티플레이어 원칙 추가
- src/ 프로덕션 코드 5대 원칙 명문화:
  1. 결정론적 시뮬레이션 (시드 기반 랜덤)
  2. 입력/로직 분리 (Command 패턴)
  3. 게임 상태 직렬화 가능
  4. 고정 시뮬레이션 스텝
  5. 매치 데이터 구조 통합
- 프로토타입(`prototypes/`)은 이 원칙에서 제외 명시

### 옛 볼트 아카이브 처리
- `~/ProjectDDWORLD/` 옵시디언 볼트는 더 이상 참조하지 않음
- 모든 작업은 `New_DDWORLD/`에 집중

### 인프라
- `.gitignore`: 아트 레퍼런스 폴더(`design/art/references/`) 제외 규칙 추가

## 배운 것

- **/review-all-gdds 리뷰는 GDD 작성 직후 바로 돌릴 가치가 있음** — Blocking 즉시 식별
- **"덱 강화 통화"같은 미래 기능을 GDD 본문에 확정 서술하면 모순 발생** — MVP 범위 밖이면 명확히 분리해야 함
- **game-concept.md는 Pillar/Anti-Pillar의 단일 출처(SSOT)** — 흩어져 있던 필라를 한 곳에 모음으로써 후속 GDD 검증 기준 확보
- **HD-2D + Tilt-shift 조합**이 인디 솔로 규모에서 비주얼 임팩트를 내는 좋은 선택지

## 막힌 것

- D-01 자원 싱크 문제는 "MVP 범위 밖"으로 우회했지만, 장기 메타 시스템 설계 시 다시 돌아와야 할 부채

## 다음 목표

- [ ] 비주얼 프로토타입 — 전장 환경(잔디/나무) 배치
- [ ] 아트 바이블 픽셀 세계 원칙 보강
- [ ] 프로토타입 README 작성
