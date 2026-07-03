---
중요도: ⭐⭐⭐⭐⭐
상태: 읽음
분류: 1인 개발 / 포스트모템
출처: 아티클 (Game Developer)
링크: https://www.gamedeveloper.com/game-platforms/bass-monkey-postmortem-from-zero-experience-to-solo-game-dev-in-18-months-without-quitting-your-day-job-
---

태그: [[이론]] [[포스트모템]]

# Bass Monkey Postmortem — From Zero Experience to Solo Game Dev in 18 Months
**출처:** [Game Developer](https://www.gamedeveloper.com/game-platforms/bass-monkey-postmortem-from-zero-experience-to-solo-game-dev-in-18-months-without-quitting-your-day-job-)
**분류:** 1인 개발 / 포스트모템
**게시일:** 2023년 12월

---

## 핵심 메시지

> "Get over yourself and finish something." — David Wehle

게임 개발 경험 제로인 저자(Jacob Weersing)가 직장을 유지하면서 18개월 만에 첫 게임 Bass Monkey를 Steam에 출시한 과정을 정리한 포스트모템. 꾸준함과 완성이 핵심이며, 무료 도구와 학습 자료만으로도 충분히 게임을 만들 수 있다는 것을 증명한다.

---

## 주요 내용

### 1. 시작 전 준비 — 성공의 정의와 습관
- 개발 시작 전에 **성공의 정의를 S.M.A.R.T 목표로 설정**할 것. 저자의 정의: "비게이머 친구들도 즐길 수 있는 차분하고 재미있는 멀티플레이어 게임을 1~2년 내 Steam에 출시"
- **매일 10분이라도 게임 관련 작업**을 할 것. 버그 테스트만 해도 된다. 꾸준함이 완성으로 이어진다 (James Clear의 Atomic Habits 참고)
- **자신의 강점을 파악**하고 그것을 기반으로 게임 컨셉을 설계할 것. 저자는 음악 녹음, 만화 그리기, 프로그래밍 로직 경험을 조합해 "만화 스타일 음악 중심 액션 게임"을 만들었다
- 작은 것부터 시작하고, 다른 프로젝트에 한눈팔지 말 것

### 2. 프로젝트 관리
- **프로젝트 목적 문서**: 대규모 GDD 대신 "누구를 위한 게임인가, 왜 만드는가, 어떤 메시지를 전달하는가"를 간단히 정리하면 충분
- **타임라인 공식**: 예상 시간 x 3배. 스코프 확대("판다곰 캐릭터도 추가하면 좋겠다" 같은 유혹)를 고려해야 한다
- **작업 추적 도구**: 저자는 스마트폰 기본 노트 앱 사용. Trello도 추천. 주간/월간 진행 상황의 가시화가 사기 진작에 도움

### 3. 아트와 음악
- **아트 도구**: Aseprite(픽셀 아트), PureRef(참고 이미지 관리)
- 프로 아티스트도 참고 자료를 사용한다. 무드보드 구축을 권장
- **에셋 활용**: itch.io 등에서 Creative Commons Zero 또는 상용 허가 에셋 사용 가능. David Wehle의 The First Tree는 기존 에셋을 영리하게 변형해 성공한 사례
- **색상/구성**: Blender Guru의 색상 이론 및 구성(Composition) 강좌 추천
- **음악 도구**: Reaper(무료 DAW), Splice(샘플 라이브러리). 음악 경험이 없으면 AudioJungle, Freesound.org, Zapsplat.com 활용

### 4. 프로그래밍
- 저자의 기초: 대학 프로그래밍 1과목. Coursera 기초 강좌로 보완 가능
- **도구 선택**: GameMaker Studio 2 (드래그앤드롭 방식으로 낮은 학습곡선)
- **학습 채널**: Heartbeast(YouTube) — 핵앤슬래시 튜토리얼로 Bass Monkey 개발
- 프로그래밍 문제의 99.99%는 이미 누군가 겪었다. Google 검색으로 대부분 해결
- 자주 휴식을 취하고, 막힌 문제는 수면 후 해결되기도 한다

### 5. 게임 디자인
- **핵심 철학**: "게임은 플레이어에게 어떤 감정을 느끼게 해야 한다" (Game Dev Field Guide 팟캐스트)
- **Hook & Kicker**: 초기 관심을 끄는 Hook과 인상을 남기는 Kicker 설계 필수 (Mike Rose의 게임 판매 프레젠테이션 참고)
- **리워드 시스템**: 플레이어가 계속 돌아오게 하는 보상 스케줄 설계 필수 (chaoticstupid.com 참고)
- **플레이테스트**: 자존심을 내려놓고, 플레이 가능한 초기 버전부터 정기적으로 진행. 플레이어의 실제 반응을 관찰
- **Juice**: "Juice it or lose it" 강연(Martin Jonasson & Petri Purho) 참고. 게임을 더 재미있게 만드는 요소들
- **추천 채널**: Game Maker's Toolkit(Mark Brown), Design Doc

### 6. 마케팅
- 저자도 마케팅은 "여전히 미스터리"라고 고백하지만, 개발 중 마케팅 시작이 필수
- **참고 자료**: Chris Zukowski(howtomarketagame.com), Tim Ruswick(Game Dev Underground)
- **소셜 미디어**: Twitter는 "게임 개발자의 LinkedIn". 1~2개 플랫폼에만 집중할 것
- **현실적 한계**: 마케팅만으로 "보통" 게임을 대박으로 만들 수 없다. 게임 자체에 대중적 매력이 있어야 한다
- **Chris Zukowski의 계단식 접근법**: 초기에는 확장 불가능한 활동(블로그, 이메일 응답)부터 시작. 한 게임씩, 한 단계씩 진행
- 출시 게임 수가 많을수록 팬층 확대 + 스킬 향상의 선순환 구조

---

## DDworld에 적용할 수 있는 것
- **성공의 정의를 먼저 세울 것**: DDworld도 "어떤 상태가 되면 성공인가"를 S.M.A.R.T 목표로 구체화해두면, 스코프 확대 유혹을 억제하고 완성까지 갈 수 있다
- **프로젝트 목적 문서 간소화**: 방대한 GDD보다 "누구를 위한 게임인가, 왜 만드는가, 어떤 감정을 줄 것인가"를 한 페이지로 정리하는 것이 1인 개발에 적합
- **예상 일정 x 3배 법칙**: 현실적 타임라인 설정에 활용
- **Hook & Kicker + 리워드 스케줄**: DDworld의 성장/언락/빌드업 핵심 루프 설계 시, 초기 관심(Hook)과 지속적 복귀 동기(보상 스케줄)를 명시적으로 설계할 것
- **기존 에셋의 영리한 활용**: 1인 개발 + 컨셉/경험 중심 인디게임이라면, 아트 에셋을 직접 만들기보다 기존 에셋을 변형해 핵심 경험에 집중하는 전략이 유효
- **매일 10분 습관**: 기획 단계에서도 매일 조금씩 작업하는 습관이 완성까지의 핵심 동력

## 관련 노트
- [[1인 개발]]
- [[포스트모템]]
