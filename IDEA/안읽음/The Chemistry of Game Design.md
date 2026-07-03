---
중요도: ⭐⭐⭐⭐⭐
상태: 안읽음
분류: 시스템 설계 / 성장 루프
출처: 아티클 (Game Developer)
링크: https://www.gamedeveloper.com/design/the-chemistry-of-game-design
---

# The Chemistry of Game Design
**저자:** Daniel Cook (Lost Garden)
**출처:** [Game Developer](https://www.gamedeveloper.com/design/the-chemistry-of-game-design)
**분류:** 시스템 설계 / 성장 루프
**게시일:** 2007년 7월

---

## 핵심 메시지

> "The player is an entity that is driven, consciously or subconsciously, to learn new skills high in perceived value. They gain pleasure from successfully acquiring skills."

현재 게임 디자인은 중세 연금술처럼 비과학적·직관적이다. Cook은 관찰 가능한 플레이어 심리 패턴을 기반으로 한 테스트 가능한 모델, 즉 "게임 디자인의 과학화"를 주장한다.

---

## 주요 내용

### 1. 연금술 비유
- 현재 게임 디자인 = 중세 연금술: 표준화된 측정법, 명확한 용어, 검증 가능한 이론이 없음
- Tetris, Super Mario Bros. 같은 성공작들은 사실 "인간 심리의 기초 위에 세워진 매우 기계적이고 예측 가능한 핵심"을 가지고 있다

### 2. 플레이어 모델의 세 구성요소
- **Skills (스킬)**: 플레이어가 게임 세계를 조작하기 위해 사용하는 행동. 개념적(지도 탐색)부터 물리적 조작까지 포함
- **Driven to Learn (배움의 본능)**: 신경과학자 Edward A. Vessel 인용 — "이런 '아하!' 순간들은 뇌에서 화학물질의 홍수를 일으키고, 우리는 이를 즐거움으로 경험한다" → 학습 자체가 생물학적 보상
- **Perceived Value (지각된 가치)**: 플레이어는 객관적으로 가치 있는 스킬이 아니라, 자신이 유용하다고 인식하는 스킬을 우선 추구함

### 3. Skill Atom (스킬 원자) 프레임워크
플레이어가 스킬을 습득하는 4단계 피드백 루프:
- **Action**: 플레이어가 입력을 수행 (버튼 누르기 등)
- **Simulation**: 그 입력에 따라 게임 상태가 업데이트됨
- **Feedback**: 게임이 변화를 플레이어에게 전달 (시각, 청각, 촉각)
- **Modeling**: 플레이어가 자신의 멘탈 모델을 업데이트하고, 즐거움 또는 좌절을 경험

### 4. 구체적 예시: 마리오의 점프
버튼 누름 → 캐릭터가 포물선을 그리며 올라감 → 점프 애니메이션 표시 → 플레이어가 "이 버튼 = 점프"를 학습. 이것이 하나의 Skill Atom.

### 5. 방법론적 의의
직관적 추측 대신 테스트 가능하고 재현 가능한 메커닉 설계를 가능하게 함

---

## DDworld에 적용할 수 있는 것
- 성장·언락 시스템 설계 시 Skill Atom 단위로 쪼개서 생각하기
- "왜 이 스킬이 재미있는가?" → Perceived Value 관점에서 점검
- 성장 쾌감 = 학습 완료 순간의 생물학적 보상 → 그 순간을 명확히 연출하기
