---
Day: 23
날짜: 2026-04-15
작업시간: 8
상태: 완료
---
[[데브로그/데브로그]]
# Day 23 — CCGS 이전 & 프로토타입 구조 정비

## 한 일
- DDworld 프로젝트를 CCGS(Claude Code Game Studios) 템플릿 구조로 전면 재구성
  - 디렉토리 구조 정립: `src/`, `design/`, `docs/`, `prototypes/`, `production/` 등
  - CLAUDE.md 마스터 설정 파일 작성 (기술 스택, 코딩 표준, 협업 프로토콜)
  - 기술 프리퍼런스 정리: Unity 6.3 LTS, URP 쿼터뷰, C#, UI Toolkit
  - 엔진 버전 레퍼런스 문서 작성 (Unity 6.3 LTS, Knowledge Gap Warning 포함)
- GDD 3개를 CCGS 8섹션 포맷으로 이전
  - `design/gdd/combat.md` — 5x5 그리드 배치, 틱 기반 자동전투, RPS 상성
  - `design/gdd/deck.md` — 50장 덱, 8장 드로우/5장 배치/3장 이월, 손패 공개
  - `design/gdd/economy.md` — 판돈(Pot) 시스템, 잔존 병사 = 보상, 역전 구조
- Unity 프로토타입을 정식 프로젝트 구조로 재편성
  - 루트에 흩어져 있던 스크립트를 `Assets/Scripts/` 하위로 이동
  - Unity 프로젝트 파일 전체 추가: Scenes, Settings, ProjectSettings, Packages
  - DOTween 플러그인, Mona 한글 폰트, InputSystem 설정 포함
  - ScriptableObject 에셋 5종 (기병/민병/창병/궁병/함정) 포함
- 딤 오버레이 화면 하단 미커버 버그 수정
  - 원인: 딤 Quad가 카메라 정면 기준으로만 배치되어 쿼터뷰 각도에서 하단이 비었음
  - 수정: localPosition을 `(0, 0, 4.7f)`로, localRotation을 `Euler(21.65f, 0, 0)`으로 카메라 기울기에 맞춤
- FloatingMessage(배치 알림 텍스트) Z값 & 스케일 조정
  - Z 위치를 카메라 기준 6으로 고정 (`6f - uiDepth`)하여 카드/그리드 앞에 표시
  - 스케일 0.7배 축소로 텍스트가 화면을 과도하게 차지하던 문제 해결

## 배운 것
- CCGS 템플릿은 1인 인디에도 유용 — GDD 포맷이 강제하는 8섹션 구조가 빠진 부분을 잡아줌
- 쿼터뷰 3D에서 카메라 자식 UI(딤, 메시지 등)는 카메라 rotation을 고려해야 함
  - 카메라가 X축으로 기울어져 있으면 Quad도 같은 각도로 기울여야 화면 전체를 덮음
- Unity 프로젝트를 Git으로 관리할 때 `.gitignore`가 중요 — Library/, Temp/, Logs/ 등 제외 필수

## 막힌 것
- 없음 — 구조 정비 & 이전 작업이라 기술적 블로커 없이 진행

## 메모
- 기존 옵시디언 볼트의 데브로그 22일치를 CCGS `production/devlog/`로 이전 완료
- GDD는 아직 Draft 상태 — Formulas, Edge Cases 섹션이 상세화 필요
- 프로토타입은 `prototypes/unity-prototype/`에 격리됨 — 프로덕션 코드와 분리 원칙 유지
- BattleTile이 기존 153줄에서 269줄로 확장됨 (3D 캡슐 유닛 스폰 기능 추가)

## 다음 목표
- 전투 페이즈 시각화: 배치 확정 후 양측 진형 공개 연출
- 틱 기반 전투 로직 프로토타입 (1틱=0.5초, 병종별 행동 패턴)
- 프로토타입 README.md 작성 (가설, 실행 방법, 현재 상태)
- GDD Formulas 섹션 상세화 (전투 공식, 데미지 계산, 상성 배율)
