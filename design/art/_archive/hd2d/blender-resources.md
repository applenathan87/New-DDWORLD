# HD-2D × Blender 자료 모음

> **Last Updated**: 2026-04-27
> **Author**: nathan
> **Purpose**: HD-2D 비주얼을 만들기 위한 Blender 학습/참고 자료 정리
> **Related**: [`../visual-polish/hd2d-setup-plan.md`](../visual-polish/hd2d-setup-plan.md) — 마스터 플랜

---

## 0. 개요

HD-2D 게임을 Blender로 만드는 워크플로우의 핵심:

```
1. 단순 low-poly 메쉬 (Box, Cylinder 조합)
   ↓
2. 박스 투영(Box Projection) UV 언랩
   ↓
3. 픽셀 텍스처 페인팅 (Aseprite 또는 Blender 내장)
   ↓
4. FBX 익스포트 → Unity Import
   ↓
5. Unity Texture 설정: Filter Mode = Point (픽셀 또렷)
   ↓
6. Material: Unlit 또는 Lit (조명 받게)
```

이 파일은 위 워크플로우의 각 단계에 도움될 외부 자료를 모은 것입니다.

---

## 1. HD-2D 기본 이해 (먼저 읽을 것)

### 1.1 HD-2D 정의

**원문**: [HD-2D — Wikipedia](https://en.wikipedia.org/wiki/HD-2D)
**한국어 번역**: [`references/hd2d-wikipedia.md`](references/hd2d-wikipedia.md) ⭐ 전문 번역 완료

- HD-2D 용어의 정의, 역사, 사용 게임 목록
- Square Enix가 옥토패스 트래블러(2018)로 정립
- "2D 픽셀 캐릭터 + 3D 환경" 핵심 정의
- 기원, 개발 과정, 특징, 사용 게임, 평가 등 완전 정리

### 1.2 옥토패스 트래블러 분석

**원문**: [Octopath Traveler's HD-2D Style — Unreal Engine Spotlight](https://www.unrealengine.com/en-US/spotlights/octopath-traveler-s-hd-2d-art-style-and-story-make-for-a-jrpg-dream-come-true)
**한국어 정리**: [`references/octopath-unreal-spotlight.md`](references/octopath-unreal-spotlight.md) ⚠️ 부분 정리 (사이트 인증으로 본문 직접 접근 불가)

- HD-2D의 시각적 특징 분석
- 픽셀 캐릭터 + 3D 환경 결합 방법
- 토모야 아사노, 시즈카 모리모토, 마사아키 하야사카 개발자 인용
- UE4 사용 + 점광원 + 그림자 시스템 기술 설명

---

**원문**: [How to make 3D environment in Octopath Traveler style — Quora](https://www.quora.com/How-can-I-go-about-making-a-3D-environment-in-the-style-of-Octopath-Traveler-1)
**한국어 정리**: [`references/octopath-quora.md`](references/octopath-quora.md) ⚠️ 부분 정리 (Quora 로그인 필요로 본문 직접 접근 불가)

- 일반인 대상 가이드 (Blender, Unity, 텍스처 등)
- 8단계 워크플로우 (컨셉 → 레퍼런스 → 3D 모델링 → 픽셀 텍스처 → 2D 스프라이트 → 조명 → 후처리 → 카메라)
- 일반적 함정 정리

---

**원문**: [Why Octopath Traveler's HD-2D Style Changed RPG Gaming Forever — Samppy](https://samppy.com/octopath-travelers-hd-2d/)
**한국어 번역**: [`references/octopath-samppy.md`](references/octopath-samppy.md) ⭐ 전문 번역 완료

- HD-2D가 왜 효과적인지 분석 (영감 / 디자인 철학)
- 500만 장 판매, 6명 프로그래머의 성취
- PBR 텍스처, 커스텀 셰이더, 틸트 시프트 등 기술 분석
- HD-2D의 4가지 기술 기반 명시 (DDworld 현재 부족함과 매핑)

---

## 2. Blender 픽셀 아트 모델링 (핵심 워크플로우)

### 2.1 공식 자료

**[Exploring 3D Pixel Art in Blender 4.2 — Blender Studio](https://studio.blender.org/blog/3d-pixel-art-in-blender/)** ⭐
- Blender 공식 블로그
- Blender 4.2 기준 최신 워크플로우
- 셰이더 셋업, 텍스처 처리 핵심 단계

### 2.2 YouTube 튜토리얼 (영상)

**[3D Pixel Art Texture Painting Blender Tutorial](https://www.youtube.com/watch?v=PLHQGJbziSU)** ⭐⭐⭐
- 픽셀 텍스처를 직접 Blender에서 페인팅하는 방법
- 가장 직접적인 워크플로우 학습용

**[Blender Beginners Tutorial on Low Poly Modelling and Pixel Shaders](https://www.youtube.com/watch?v=-5VclQ0dqnY)** ⭐⭐⭐
- 초보자용. PS1 스타일 (HD-2D 인접 미학)
- 픽셀 셰이더 셋업 포함

**[Creating a Low Poly Pixel Texture 3D Asset in Blender 2.8+](https://www.youtube.com/watch?v=yh6785-ff8k)** ⭐⭐⭐
- 자동차 모델링 + UV + 텍스처 페인팅 풀 워크플로우
- 2.8+ (현재 4.2와 거의 동일)

**[Pixel Perfect Texture & Easy Unwrapping in Blender 2.8](https://www.youtube.com/watch?v=RQVAUaSUP-k)** ⭐⭐
- UV 언랩 + 픽셀 정렬 워크플로우
- DDworld의 박스 형태 모델(성벽, 무기 거치대 등)에 적합

**[Blender Tutorial: How to Texture Pixel Art in Blender](https://www.youtube.com/watch?v=x76Vt9m_q0w)** ⭐⭐
- 픽셀 텍스처 적용 기초

**[PigArt — Low Poly Pixel Models](https://www.youtube.com/watch?v=PCx-kBndTqQ)** ⭐⭐
- PigArt 채널 (저폴리/픽셀 아트 타임랩스)
- 시각적 영감 + 작업 흐름 관찰

**[PigArt — Low Poly Forest Assets](https://www.youtube.com/watch?v=rtO9maU709k)** ⭐⭐
- 나무, 바위, 풀 같은 자연 요소
- DDworld의 외곽 환경 (큰 바위, 폐허 등)에 직접 적용 가능

**[PigArt 채널 전체](https://www.youtube.com/channel/UCBigB2UktH5XE1hY1vqghOA)**
- 저폴리 픽셀 모델링 시리즈

### 2.3 커뮤니티 Q&A / 블로그

**[How to make 3D models with pixel textures? — Blender Artists Community](https://blenderartists.org/t/how-to-make-3d-models-with-pixel-textures-low-poly-pixel-art/1179588)** ⭐⭐
- 실용적 팁 / 실패 케이스 / 권장사항
- 포럼 형식이라 자주 막히는 부분 확인 가능

**[Master Low Poly Modeling — How To Make Low Poly Models In Blender](https://cookwithrome.com/blog/how-to-make-low-poly-models-in-blender/)** ⭐⭐
- 블로그 정리 (UV 언랩, 텍스처 적용 가이드)

---

## 3. 도구 / 애드온

### 3.1 Sprytile (강력 추천) ⭐⭐⭐

**[Sprytile by Jeiel Aranal — itch.io](https://jeiel.itch.io/sprytile)**
**[Sprytile GitHub](https://github.com/Sprytile/Sprytile)**
**[ReSprytile (Blender 4.x 호환 포크)](https://github.com/ionthedev/ReSprytile)**

#### 무엇인가?
**Blender 안에서 타일맵 편집기처럼 작업하는 픽셀 아트 모델링 애드온.** 무료, 오픈소스.

#### 핵심 기능
- 2D 타일셋 → 3D 모델 변환
- UV 페인팅 (모델링하면서 동시에 UV 매핑)
- 픽셀 그리드 정렬 도구 (vertex가 픽셀 단위로 스냅)

#### DDworld 활용
- 성벽, 무기 거치대 같은 **박스 기반 모델**을 빠르게 제작
- 기존 픽셀 텍스처를 타일셋으로 사용하여 즉시 적용
- 일반 Blender 모델링보다 시간 50%+ 단축 가능

#### Unity Export
**[Sprytile Unity Settings 공식 문서](https://github.com/Sprytile/SprytileDocs/blob/master/docs/unity-settings.md)** ⭐
- Material: **Unlit Transparent** (Blender 결과와 동일)
- 또는 **Standard Cutout** (조명 받게 하려면)

**[Sprytile Unity 익스포트 Q&A](https://itch.io/t/168614/noob-here-how-do-i-export-this-to-unity-with-textures-attached)**
- 텍스처 함께 익스포트하는 방법

### 3.2 Blender Spritesheets

**[blender-spritesheets — GitHub](https://github.com/theloneplant/blender-spritesheets)**
- 3D 모델/애니메이션을 스프라이트시트로 익스포트
- DDworld는 주로 정적 환경이므로 우선순위 낮지만, 향후 애니메이션 환경(흔들리는 깃발 등)에 유용

### 3.3 Pixel Art Shader (Lucas Roedel)

**[This free Blender addon is a shortcut to cool retro pixel art — Creative Bloq](https://www.creativebloq.com/3d/this-free-blender-addon-is-a-shortcut-to-cool-retro-pixel-art)**
- 무료 픽셀 아트 셰이더
- Blender 뷰포트에서 픽셀화 효과 미리보기

---

## 4. Unity 연동 (Blender → Unity)

### 4.1 Unity URP 픽셀 아트 셋업

**[Unity Pixelated Art Style In URP — KYRIOTA](https://kyriota.com/2022/08/02/UnityPixelatedArtStyleInURP/)** ⭐⭐⭐
- URP에서 픽셀 아트 효과 (가장 직접적 가이드)
- Render Pipeline Asset 셋업
- 픽셀 보존 카메라 설정

**[How to setup URP to support a project that is both 2D & 3D? — Unity Discussions](https://discussions.unity.com/t/how-to-setup-urp-to-support-a-project-that-is-both-2d-3d/849133)**
- HD-2D처럼 2D + 3D 혼합 프로젝트의 URP 셋업
- DDworld가 정확히 이 케이스

**[Unity Manual — 2D Renderer asset in URP](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/Setup.html)**
- Unity 6 공식 문서

### 4.2 Blender → Unity 익스포트

**[Importing from Blender — Unity Discussions](https://discussions.unity.com/t/importing-from-blender/841932)**
- FBX vs .blend 직접 import 비교
- 머티리얼 처리

---

## 5. 유료 강좌 (체계적 학습 원할 때)

### 5.1 Blender → Unity 파이프라인

**[From Blender to Unity: Game-Ready Assets — GameDev.tv](https://gamedev.tv/courses/blender-to-unity)** ⭐
- 가장 신뢰성 있는 강좌 (GameDev.tv)
- 모델링 → 리깅 → 텍스처 → Unity 임포트 전체 흐름

**[Learn Blender 3D Modeling for Unity Video Game Development — Udemy](https://www.udemy.com/course/learn-blender-3d-modeling-for-unity-video-game-development/)**

**[Blender 3D Modeling for Games — COMPLETE WORKFLOW — Udemy](https://www.udemy.com/course/blender-3d-modeling-for-games-complete-workflow/)**

**[Unity Game Asset Creation in Blender — Udemy](https://www.udemy.com/course/unity-game-asset-creation-in-blender-textured-3d-models/)**

### 5.2 환경 디자인 (HD-2D 인접)

**[Game Environment Creation — Blender & Unity (LMHPOLY) — 무료](https://www.classcentral.com/course/youtube-how-i-made-this-game-environment-blender-unity-128164)** ⭐
- 무료 강좌
- 환경 만들기 풀 워크플로우 (Blender + Unity)

**[Modular Environment Building for Games with Blender — Udemy](https://www.udemy.com/course/3dmotive-learn-modular-environment-building-for-games-with-blender/)**
- 모듈러 환경 (DDworld의 성벽/벽 같은 반복 요소에 적합)

### 5.3 HD-2D 특화 (제한적)

**[Learn how to create games with the new HD-2D graphical style — GameDev.tv Community](https://community.gamedev.tv/t/learn-how-to-create-games-with-the-new-hd-2d-graphical-style/196891)**
- GameDev.tv의 HD-2D 강좌 요청 토픽 (정식 출시 시 우선 후보)

**[SMILE GAME BUILDER — Octopath Traveler Tutorial Series — YouTube](https://www.youtube.com/watch?v=dI3F9h2f2ys)**
- Smile Game Builder 기준이라 직접 적용은 어렵지만, **HD-2D 디자인 사고 과정 참고**용

---

## 6. 추천 학습 순서 (DDworld 진입용)

### 단계 1: 이해 (1~2시간)
1. [HD-2D Wikipedia](https://en.wikipedia.org/wiki/HD-2D) 읽기
2. [Octopath Quora 가이드](https://www.quora.com/How-can-I-go-about-making-a-3D-environment-in-the-style-of-Octopath-Traveler-1) 읽기
3. PigArt 채널 영상 1~2개 시청 (시각적 감각 잡기)

### 단계 2: Blender 기초 (1일)
1. [Blender Beginners Tutorial on Low Poly Modelling and Pixel Shaders](https://www.youtube.com/watch?v=-5VclQ0dqnY) — 풀 시청
2. 직접 따라하면서 단순 Box 모델링 + UV 언랩 + 픽셀 텍스처

### 단계 3: 도구 셋업 (반나절)
1. [ReSprytile](https://github.com/ionthedev/ReSprytile) 설치 (Blender 4.x용)
2. [Sprytile 공식 문서](https://github.com/Sprytile/SprytileDocs) 읽기
3. 간단한 박스 모델 1개 만들어보기

### 단계 4: 첫 모델 (1일)
**DDworld Phase 1 ⭐⭐⭐ 우선순위**: **성벽 모델 1개**
- Box 5~10개 조합으로 성벽 형태
- Sprytile로 픽셀 텍스처 즉석 적용
- FBX 익스포트 → Unity Import

### 단계 5: Unity URP 셋업 (반나절)
1. [Unity Pixelated Art Style In URP — KYRIOTA](https://kyriota.com/2022/08/02/UnityPixelatedArtStyleInURP/) 따라하기
2. Texture Filter Mode = Point 등 픽셀 보존 설정
3. Material 셋업 (Unlit으로 시작)

### 단계 6: 첫 배치 (1일)
- Prefab 만들기
- `EnvironmentSetup.cs`에 `CreateBattleStructures()` 메서드 추가
- 양 진영 배치 + 위치 튜닝

**총 예상**: 약 4~5일 (학습 + 첫 모델 1개)

이후 같은 파이프라인으로 깃발, 큰 바위 등 추가 모델 제작 시 모델당 0.5~1일.

---

## 7. DDworld Phase 1 적용 우선순위

| 우선순위 | 모델 | 적합한 자료 |
|---------|------|-----------|
| ⭐⭐⭐ | 성벽 / 요새 잔해 | Sprytile (박스 기반), Modular Environment Udemy |
| ⭐⭐⭐ | 깃발 (3D 봉 + 천) | YouTube 튜토리얼 (Cylinder + Plane), 셰이더 (Phase 4 흔들림) |
| ⭐⭐ | 무기 거치대 | Sprytile (박스 조합) |
| ⭐⭐ | 폐허 기둥 | PigArt 영상 영감 |
| ⭐⭐ | 큰 바위 | [PigArt — Low Poly Forest Assets](https://www.youtube.com/watch?v=rtO9maU709k) |

---

## 8. Open Questions / 결정 사항

| 질문 | 결정 / 참고 |
|------|-----------|
| Blender 버전? | 4.2+ (LTS) — 최신 기능 + ReSprytile 호환 |
| 픽셀 텍스처 도구? | Aseprite ($19.99, 표준) 또는 LibreSprite (무료) |
| Sprytile 사용? | **사용 권장** — 박스 형태 모델 빠른 제작 |
| 텍스처 해상도 표준? | 모델 크기에 비례 (32~128px). Phase 1 시작 시 결정 |
| FBX vs .blend? | **FBX 권장** — Unity 표준, 호환성 안전 |
| Unity Material? | Phase 1: **Unlit** (단순) → Phase 3: **Lit** (조명) |

---

## 9. 향후 추가될 자료

이 파일은 학습 중 발견하는 새 자료를 추가하면서 업데이트합니다:
- 직접 시도해서 좋았던 영상 → ⭐ 추가
- 막히는 부분 발견 → "Common Issues" 섹션 추가
- DDworld 특화 노하우 → 별도 섹션
