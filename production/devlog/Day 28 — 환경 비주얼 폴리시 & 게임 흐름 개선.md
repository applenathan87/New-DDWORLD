---
Day: 28
날짜: 2026-04-27
작업시간: 8
상태: 완료
---
[[데브로그/데브로그]]
# Day 28 — 환경 비주얼 폴리시 & 게임 흐름 개선

## 한 일

### 환경 비주얼 (잔디 + 나무)
- **basic_tile_v2.png 도입** — 픽셀 아트 잔디 텍스처 적용 (Filter Mode = Point)
- 이전 `grass.jpg` 삭제, 폴백 시스템(v2 → v1 → Tilemap_color1) 코드에 내장
- **타일 간 갭 제거** — `tileGap = 0`, `tileSize * 0.95f → 1.0f` 변경으로 타일들이 빈틈 없이 붙음
- BattleField 씬 파일들도 함께 수정 (BalanceTest.unity, SampleScene.unity)
- **잔디 바닥 50x50 확장** — BattleField의 단색 진한 초록 ground를 완전히 덮도록. 나무 사이/뒤 영역에도 잔디 텍스처 표시

### 나무 배치 시스템 대폭 강화
- **상단(카메라 먼쪽)**: 8줄 빽빽한 깊이감 있는 숲 (이전 2줄)
- **좌/우 측면**: 6줄 → 10줄로 확장, 줄 간격 1.0으로 단단한 벽처럼 빽빽
- **하단**: PlaceTreeRowEdgesOnly로 가운데 비움 / `bottomClearMargin`으로 조절
- **상단 좌/우 코너 채움** — `topCornerExtension = 4f`로 빈 코너 해결
- **HD-2D 스타일 정착** — 나무는 항상 수직 고정(Quaternion.Euler(0,0,0)). 카메라 각도 정렬 시도했다가 누워버려 원복
- 인스펙터 변수 다수 추가 (sideTreeRows, topCornerExtension, bottomTreeRows 등)

### 카메라 흐름 단순화 (3번 → 2번 이동)
- **이전**: Placement → 전체뷰(2) → 1.5초 대기 → 줌인(3) — 라운드당 4.4초
- **이후**: Placement → 줌인 전투뷰 직행 — 라운드당 1초
- `ZoomToBattleView()` 신규 메서드: 줌인 단계 1로 직접 이동
- AutoZoomToBattle / 1.5초 대기 제거

### 휠 줌 — 진짜 다가가는 줌인
- **이전**: FOV만 변경 (시야각 좁아짐, 평면적 망원렌즈 효과)
- **이후**: FOV + Position 동시 트윈 — 카메라가 forward 방향으로 dolly 이동
- `AnimateZoomStep(oldLevel, newLevel)` 신규 메서드 (현재 위치 기준 dolly)
- 인스펙터 변수: `zoomDollyPerStep = 1.5f` (조정 가능)
- 패닝 후 줌도 정상 작동 (현재 위치 기준 이동)

### 카드 UI 위치 조정
- 플레이어 핸드 Y 비율 0.7 → 0.55 (위로 올림)
- 인스펙터에서 즉석 조정 가능: `playerHandYRatio`, `enemyHandYRatio` (Range 0~1 슬라이더)

### 이월 카드 시각적 차별화 (3장 직접 이동 애니메이션)
- 1라운드: 8장 모두 덱에서 드로우 (기존)
- 2라운드+: **이월 3장이 손패 영역 중앙에 모여 시작 → 손패 위치로 펼쳐지는 애니메이션** (0.6초)
- 그 후 신규 5장이 덱에서 차례로 드로우 — 위치 충돌 없음
- `AnimateDrawSequence(playerCarryover, enemyCarryover)` 매개변수 추가

### 시각 잔재 정리 시스템 구축
**문제**: 라운드 종료 후 시체, 라인, 결과 화면이 다음 라운드로 이월됨

해결 — BattleSimulator가 OnPhaseChanged 구독, Draw 진입 시 일괄 정리:
- FieldBounds (초록 사각 경계선)
- LaneLine_Player / LaneLine_Enemy (자유 추적 하얀 세로선)
- ResultScreen (전투 결과 화면)
- **모든 BattleTile의 병사** (시체 + 생존자) — `BattleTile.ClearSoldiers()` public 변경
- **DirArrow / RangeIndicator** — `Soldier.OnDestroy()`에서 자체 정리
- **Arrow** (비행 중 화살) — `FindObjectsByType<Arrow>()`로 일괄 정리

### 게임 흐름 버그 수정
- **무승부 즉시 종료 버그**: StartBattle의 ClearVisualOverlays 호출이 막 배치된 병사를 지워버림 → 호출 제거
- **드로우 페이즈 타이틀 중복** ("드로우 페이즈" → "Round X" → "드로우 페이즈"): OnPhaseChanged의 Draw 케이스 제거
- **1라운드 드로우 애니메이션 생략**: PlacementUI.Start() 폴백 경로가 carryover=HandCount로 처리하던 문제. round==1일 때 carryover=0으로 수정

### 결과 화면 대폭 리뉴얼
- **PhaseTitle ("승리!" 큰 텍스트) 제거** — ResultScreen이 이미 result 텍스트 포함이라 중복
- **검은 반투명 배경** 추가 (alpha 0.85, 2.8x1.5)
- **레이아웃 변경** — 세로 일렬 → 좌(아군) / 우(적군) 분할
  - 헤더: 상단 중앙
  - 좌측 X=-0.7: 아군 통계 (TopLeft 정렬)
  - 우측 X=0.7: 적군 통계 (TopLeft 정렬)
- **Z-fighting 해결**: 배경 z=0.2, 텍스트 z=-0.1 (차이 0.3)
- **등장 애니메이션**: 0 → 1 스케일 OutBack 0.4초 (살짝 튀는 등장)
- **표시 시간**: 2초 → 4초 (`Invoke(StartNewRound, 4f)`)
- **피해 항목 제거** (요청 반영) — `딜 / 처치`만 표시
- 헤더와 통계 사이 마진 추가 (Y=0.05 → -0.2)
- "다음 라운드로..." 푸터 제거 (요청 반영)

### 함정 통계 누락 수정
- **버그**: TriggerTrapExplosion이 데미지만 가하고 함정 자신의 totalDamageDealt / killCount에 카운트 안 함
- **수정**: 함정도 일반 공격처럼 자기 통계에 누적
- 결과 화면에서 `함정: 0/1 | 딜 30 | 처치 3` 정상 표시

### 함정 폭발 범위 확장
- **이전**: 반경 1.0 (타일 1개) — 인접 대각 셀(거리 √2≈1.41) 범위 밖
- **이후**: 반경 1.5 — 인접 8방향 모두 포함
- BattleSimulator 인스펙터 변수 `trapExplosionRadius = 1.5f` 추가 (조정 가능)
- BattleField.tileSize 의존성 제거 (BattleSimulator에서 독립 관리)

### 자동 환경 생성
- BattleField가 EnvironmentSetup이 씬에 없으면 자동 생성
- SampleScene 등 다른 씬에서도 동일한 환경 비주얼 보장

### CCGS 템플릿 잔재 정리 (private repo 정리)
- README.md, LICENSE, UPGRADING.md 제거 (template 마케팅 문서)
- "CCGS Skill Testing Framework/" 폴더 제거 (127 파일, ~19,000줄 삭제)

## 배운 것

- **URP Transparent의 Z-fighting**: ZWrite OFF + 거리 정렬 의존이라 z 차이 0.01 정도면 매 프레임 정렬 흔들림. 0.3 unit 이상 떨어뜨려야 안정.
- **HD-2D 정통 스타일**: 나무를 카메라 각도에 맞춰 회전시키면 누워버림. 수직 고정이 옥토패스/스타듀밸리식 정답.
- **인스펙터 값 vs 코드 기본값**: 씬에 직렬화된 값이 코드 기본값을 오버라이드. 코드 기본값은 "처음 컴포넌트 추가 시"만 사용.
- **카메라 페이즈 전환에서 정적 빌보드는 한계**: 카메라 회전이 변하면 정렬 어긋남. HD-2D처럼 회전 0이 가장 견고.
- **상태 정리는 Draw 페이즈 진입 시점이 안전**: StartBattle 시점은 이미 병사 스폰된 상태라 정리하면 게임 깨짐.
- **이월 카드 시각화는 "직접 이동 애니메이션"이 핵심**: 즉시 박혀있는 게 아니라, 손패 중앙에서 펼쳐지는 동작이 "내가 가지고 있던 카드" 정체성을 만듦.
- **결과 화면의 좌우 분할**: 한 TextMeshPro에 띄어쓰기로 분리는 가독성 한계. 별도 GameObject + TopLeft 정렬이 깔끔.
- **함정의 통계 카운트 누락**: 일반 공격은 명시적으로 dmgDealt/killCount 증가시키는데 폭발 로직에는 누락. 항상 데미지 가한 주체의 통계도 동시에 갱신해야.

## 막힌 것

- 나무 카메라 정렬을 시도했다가 부호 잘못 적용해서 나무가 누워버림 → HD-2D 스타일(회전 0)로 복귀
- 결과 화면 z 차이를 0.011로 두었다가 z-fighting으로 깜빡임 → 0.3 unit으로 분리
- StartBattle에 방어적으로 ClearVisualOverlays 추가했다가 막 배치된 병사를 지워서 즉시 무승부 발생 → 호출 제거
- 1라운드에서 PlacementUI.Start() 폴백 경로가 모든 카드를 이월로 처리하여 드로우 애니메이션 생략됨 → round==1 분기 추가

## 다음 목표

- [ ] **함정 배치 룰 상세 설계** ⭐ 다음 세션 시작점
  - 현재: 함정이 중간선(탐지 라인) 바깥에 있으면 작동 안 함
  - 정교한 룰 정의 필요: 어느 영역에 배치 가능? 발동 조건? 사거리 vs 직접 접촉? 범위 폭발 vs 단일 타격?
  - design/gdd/combat.md 함정 섹션 보강 + 코드 반영
- [ ] 함정 배치 시각적 가이드 (배치 가능 영역 하이라이트)
- [ ] 결과 화면 표시 시간 미세 조정 (현재 4초)
- [ ] Soldier OnDestroy의 다른 부속물 (FloatingText 등) 정리 검토
- [ ] 이월 카드 시각 강조 (배지/색상 차별화)
- [ ] 카드 호버 시 "이월" vs "신규" 구분 UI
