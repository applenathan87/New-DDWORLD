# Day 2 읽기 목록 — 미니어처 룩 (카메라 · DOF · 구도)

> 수집: 2026-07-03 · **한 주제 집중**: 평평한 3D를 "장난감 디오라마"로 만드는 법.
> 선정 근거: 1단계(보이는 공간) 요소 중 **잔디·조명·소품은 day_01이 이미 커버**, **카메라·미니어처/틸트시프트만 빈칸**이었음. 게다가 이건 우리 게임의 **시그니처 룩**이라 우선순위 최상.
> ⚠️ 검증: 웹 검색 기준(각 글 완독 X). 열어보고 싶은 건 Claude가 가져와 같이 뜯을 수 있음.
> 읽는 순서: ⭐ 3개 먼저.

---

## 🔬 왜 미니어처처럼 보이나 (원리)

- [ ] ⭐ **[Miniature faking (Wikipedia)](https://en.wikipedia.org/wiki/Miniature_faking)**
  - 틸트시프트/디오라마 효과의 **핵심 원리**: 얕은 심도(흐림)가 "이건 코앞의 작은 모형"이라고 뇌를 속임 → 장면 전체가 작게 인식됨.
  - 검색으로 확인된 **미니어처 공식 4요소**가 우리와 정확히 일치:
    1. **얕은 심도(강한 DOF)** — 가운데만 선명, 위아래 흐림
    2. **내려다보는 각도** — 쿼터뷰
    3. **높은 채도** — 따뜻하고 쨍한 색
    4. **작은 것이 빠르게 움직임** — 작은 유닛이 통통 움직이면 더 장난감 같음
- [ ] **[Tilt-Shift Shot (StudioBinder)](https://www.studiobinder.com/camera-shots/focus/tilt-shift-shot/)** — 사진/영상 쪽 실전 설명, 시각 예시.

## 📐 쿼터뷰 카메라 각도 & 구도

- [ ] ⭐ **[Mastering Camera Angles for Isometric Game Design (LensViewing)](https://lensviewing.com/camera-angle-for-isometric-game/)**
  - 아이소/쿼터뷰 각도 세팅(수평 45°, 수직 ~30°대). **우리 ADR의 쿼터뷰 X 29.2°와 같은 대역** — 방향 검증됨.
  - 핵심 원칙: **카메라 높이 일관성 유지**, 극단 각도 회피, **미묘한 동적 카메라 넛지로 주목 유도**(우리 프로토가 이미 '타일 선택 시 카메라 nudge' 넣은 것과 정확히 일치).
- [ ] **[How to Position the Camera for Isometric Assets (Game Developer)](https://www.gamedeveloper.com/design/how-to-position-the-camera-for-isometric-assets)** — 카메라 배치 실전.
- [ ] **[Isometric Camera Angle in Games (LensViewing)](https://lensviewing.com/isometric-camera-angle-in-games/)** — 2.5D 시각 가이드 심화.

## 🎛️ 유니티에서 틸트시프트 · DOF (1단계 후처리 실전)

- [ ] ⭐ **[Simple Tilt Shift Effect in Unity URP (Noveltech)](https://www.noveltech.dev/tilt-shift-unity)**
  - URP 커스텀 렌더러 피처로 틸트시프트 만들기. **1단계 후처리 실습에 바로 쓸 how-to.**
- [ ] **[Depth of Field (Unity Learn)](https://learn.unity.com/tutorial/post-processing-effects-depth-of-field-2019-3)** — DOF 후처리 기본. (버전 옛것이나 개념 동일)
- [ ] **[Miniature Camera — Tilt Shift, Unity 6 URP (YouTube)](https://www.youtube.com/watch?v=Enidtotl6sE)** — Unity 6 URP용 최신 틸트시프트/DOF 툴 소개(2026). 우리 버전대에 맞음.

## 🖼️ 디오라마 구도 원리 (초점 · 프레이밍 · 레이어)

> 미니어처 사진/모형 쪽 구도 원리 — 우리 전투 아레나 프레이밍에 그대로 적용됨.

- [ ] **[Building Immersive Miniature Dioramas (Last Hope Miniatures)](https://lasthopeminiatures.com/blogs/news/building-immersive-miniature-dioramas-a-step-by-step-guide)**
  - **내러티브 앵커**(단일 초점 하나로 장면 지배) + **3분할 법칙**(앵커를 중앙 아닌 곳에) + **리딩 라인**(길·울타리로 시선 유도) + **레이어링**(배경 먼저, 전경 나중 → 깊이감).
  - → 우리로 치면 **본성(성)을 내러티브 앵커**로, 헥사 길을 리딩 라인으로 쓰는 식.
- [ ] **[The Rise of 3D Digital Dioramas (RenderHub)](https://www.renderhub.com/blog/miniature-worlds-monumental-impact-the-rise-of-3d-digital-dioramas)** — 디지털 디오라마에서 조명이 시선을 이끄는 법.
- [ ] **[Creating a Mid-Century Diorama (The Rookies)](https://discover.therookies.co/2023/04/11/creating-a-mid-century-diorama-a-step-by-step-guide-to-3d-modeling-and-asset-creation/)** — 3D 디오라마 제작 단계별.

## 👀 게임 예시 모음 (눈 훈련)

- [ ] **[Games with tilt-shift / "miniature toy" aesthetic (ResetEra 스레드)](https://www.resetera.com/threads/what-are-some-video-games-with-a-tilt-shift-or-miniature-toy-aesthetic.964152/)**
  - 미니어처 룩 게임들을 모아놓은 목록. **레퍼런스 눈 훈련**용으로 최고. 마음에 드는 건 캡처해서 같이 분석 가능.
- [ ] **[Developing The Bad North Look — Oskar Stålberg (Konsoll 2018)](https://www.youtube.com/watch?v=6JcFbivo8dQ)** — 우리 #1 레퍼의 룩을 만든 사람 강연. (생성 얘기 나오면 룩 부분만)

---

## 🎯 우리 프로젝트에 바로 꽂히는 결론

- **미니어처 공식 = 쿼터뷰(29.2°) + 강한 DOF(틸트시프트) + 따뜻한 채도 + 작은 유닛의 통통 움직임.** 우리가 정한 방향이 이론적으로 정확했음.
- **내러티브 앵커**를 하나 정해라 — 우리는 **본성(성)** 이 유력. 그걸 3분할 위치에 두고, 헥사 길로 시선 유도.
- **카메라 높이 일관 + 미묘한 넛지** — 이미 프로토에 넛지 넣은 게 정석이었음.
- 1단계 후처리 실습 = **DOF/틸트시프트 볼륨 세팅**이 핵심. Noveltech 글이 그 실전 안내.

## 오늘 동선 추천 (⭐ 3개)

1. **Miniature faking** — 왜 우리 아트 전체가 성립하는지(공식 4요소).
2. **Isometric Camera Angles** — 우리 쿼터뷰 각도·구도의 근거와 원칙.
3. **Noveltech Tilt Shift (Unity URP)** — 1단계에서 실제로 만들 후처리의 how-to.

> 나머지 주제(절차적 애니·손맛·깊은 셰이더·복셀 캐릭터·절차적 생성)는 `backlog-reading.md`로 이동 — 해당 단계 오면 꺼내볼 것.
