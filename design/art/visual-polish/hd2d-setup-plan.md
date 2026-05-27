# HD-2D Setup Plan — 마스터 플랜 [DEPRECATED]

> ⚠️ **DEPRECATED (2026-05-25)**: 비주얼 방향이 **Low-poly Voxel 3D**로 전환됨. 결정 근거: [ADR-002](../../../docs/architecture/ADR-002-visual-style-low-poly-3d.md).
> 이 문서는 **참고용으로 보존**됩니다. 향후 가이드는 [README.md](README.md)와 새 문서들을 참조하세요.
>
> 폐기된 이유 요약:
> - HD-2D는 사실상 스퀘어에닉스 독점 기법, 인디 사례 부재
> - 1인 인디에 과한 작업 부담 (Blender + Aseprite + 8방향 픽셀 애니)
> - 카메라 자유도 제약 (픽셀 스프라이트는 카메라 정면 고정)
> - 검증된 대안 존재 (Tabletop Tavern, Bad North, A Short Hike 등이 Low-poly로 인디 성공)
>
> **유지되는 컨셉** (Low-poly 3D에도 적용 가능):
> - 쿼터뷰 카메라 각도 (X 29.2°)
> - 따뜻한 색온도
> - Tilt-shift + DOF + Bloom 적용 의도
> - 3D 환경 메쉬 우선 (단, 픽셀 텍스처 → 단색/Vertex color로 전환)

---

> **Status (원본)**: 계획 단계 (코드 변경 0)
> **Author**: nathan
> **Last Updated**: 2026-04-27
> **Target (원본)**: 옥토패스 트래블러 / Sea of Stars 수준의 HD-2D 룩
> **Engine**: Unity 6.3 LTS, URP (Universal Render Pipeline)

---

## 0. 요약

DDworld의 비주얼 정체성은 **HD-2D**입니다 (art-bible.md). 그러나 현재 게임은 단순한 픽셀 게임처럼 보이며, **가장 큰 원인은 "3D 환경 오브제의 부재"**입니다 — HD-2D = 3D 환경 + 2D 픽셀 스프라이트인데, DDworld는 평면 잔디 + 2D 빌보드 나무뿐입니다.

이 문서는 **3D 환경 구축을 최우선으로 한 5-Phase 마스터 플랜**을 제시합니다. 코드 리팩토링은 거의 필요 없습니다.

**예상 소요**: 총 8~14일 (Phase별 1~5일). 인디 1인 기준.

---

## 1. 목표 — DDworld의 HD-2D 정체성

### 1.1 art-bible 기반 핵심 비주얼 원칙

- **HD-2D + Tilt-shift** 비주얼 디렉션
- 픽셀 아트 스프라이트 (병종) + 3D 환경
- 따뜻하고 부드러운 톤
- 쿼터뷰 카메라 (X 29.2° 기울임)
- 옥토패스 트래블러 / Sea of Stars / Triangle Strategy 레퍼런스

### 1.2 HD-2D의 진짜 정의

```
HD-2D = 3D 환경/오브제 (픽셀 텍스처 매핑) + 2D 픽셀 스프라이트 캐릭터
              ↑                                    ↑
        옥토패스의 마을, 건물, 다리                        주인공, NPC, 적
```

옥토패스의 시그니처:
- 마을 집들 = 3D 박스에 픽셀 텍스처
- 다리, 계단, 폐허 = 3D 메쉬
- 큰 나무, 바위 = 3D
- 캐릭터, 풀(작은 것) = 2D 빌보드

**현재 DDworld 부족함**: 3D 환경 오브제 사실상 없음. 평면 잔디 + 2D 빌보드 나무뿐 → 픽셀 게임처럼 보임.

### 1.3 HD-2D 룩의 핵심 시각 요소

1. **3D 환경 메쉬** ⭐ 가장 중요 — 픽셀 텍스처 매핑된 3D 오브제
2. Bloom — 빛 번짐
3. Depth of Field / Tilt-shift — 미니어처 느낌
4. Color Grading — 따뜻한 색온도
5. Vignette — 시선 집중
6. Dynamic Lighting — 그림자 + 시간대 분위기
7. Layered Depth — 전경/중경/배경
8. Shader Polish — 잔디 흔들림, 거리 페이드
9. Particle Effects — 빛입자, 꽃잎

**핵심 인사이트**: 1번이 없으면 나머지 다 적용해도 "예쁜 픽셀 게임"에 머무름. **3D 환경이 HD-2D의 정체성**.

---

## 2. 현재 상태 진단

### 2.1 잘 되어 있는 것 ✅

- 쿼터뷰 카메라 (X 29.2°) — HD-2D 정통 각도
- 잔디 텍스처 + 2D 나무 환경 (기본 레이어)
- 카메라 흐름 (페이즈별 위치/FOV 전환 + 휠 줌)
- 픽셀 아트 스프라이트 (병종, 카드, 나무)

### 2.2 부족한 것 ❌

| 항목 | 현재 | 부족한 이유 |
|------|------|------------|
| **3D 환경 오브제** ⭐ | **사실상 없음** | **HD-2D의 핵심 — 가장 시급** |
| Post-processing | 없음 | URP Volume 미적용 |
| 조명 | 모두 `URP/Unlit` | Lit 셰이더 + Directional Light 필요 |
| 색감/톤 | 원본 텍스처 색 | Color Adjustments 없음 |
| 환경 디테일 | 잔디 + 나무만 | 흔들림 셰이더, 파티클 부재 |
| 그림자 | 없음 | Lit 머티리얼 + Shadow 필요 |

---

## 3. 참고 게임 분석

### 3.1 옥토패스 트래블러 (Square Enix) — HD-2D의 정의

- 마을 집들이 모두 **3D 박스 + 픽셀 텍스처**
- 다리, 계단, 폐허, 신전 = 3D 메쉬
- 강한 Bloom + Tilt-shift Depth of Field
- 따뜻한 색온도

### 3.2 Sea of Stars (Sabotage Studio) — 인디 레퍼런스

- 옥토패스 대비 절제된 Bloom
- **3D 환경 + 2D 캐릭터 일관됨**
- 환경 파티클 (반딧불, 꽃잎)
- 인디 스코프이라 가장 현실적 레퍼런스

### 3.3 Triangle Strategy (Square Enix) — DDworld와 가장 유사

- 그리드 기반 전투 + HD-2D
- **각 전투 맵마다 다른 3D 환경** (마을 광장, 다리, 성벽 등)
- 카메라 줌/회전 적극적

---

## 4. Phase 1 — 3D 환경 구축 ⭐ 최우선

**목표**: 전장에 입체감 있는 3D 환경 오브제 배치 → HD-2D 정체성 핵심 확보

**도구**: **Blender** (모델링) + Unity (배치/머티리얼)

### 4.1 DDworld 전장에 어울리는 3D 오브제

DDworld는 **마을이 아닌 전장(battlefield)** 게임. 어울리는 오브제:

| 우선순위 | 오브제 | 위치 | 효과 |
|---------|------|------|------|
| ⭐⭐⭐ | **성벽 / 요새 잔해** | 양 진영 끝 | 양 진영 입체감, 무대화 |
| ⭐⭐⭐ | **깃발 / 기치** (3D 봉 + 천) | 양 진영 좌우 | 진영 정체성 |
| ⭐⭐ | **무기 거치대 / 방패벽** | 진영 뒤 | 전장 분위기 |
| ⭐⭐ | **돌무더기 / 폐허 기둥** | 측면 | 자연스러운 시야 차단 |
| ⭐⭐ | **큰 바위** | 외곽 | 깊이감, 가까운 디테일 |
| ⭐ | **다리 / 도랑** | 중앙선 | 양 진영 분리 |
| ⭐ | **천막 / 캠프 텐트** | 진영 뒤 | 야영지 느낌 |
| ⭐ | **지형 굴곡** (작은 언덕) | 외곽 | 평면 탈피 |

**Phase 1 MVP 목표**: ⭐⭐⭐ 우선순위 2개부터 시작 (성벽 + 깃발). 결과 보고 ⭐⭐ 추가.

### 4.2 Blender 모델링 워크플로우

#### Step 1: 단순 셰이프부터

HD-2D는 **저폴리(low-poly) + 픽셀 텍스처**가 정답. 모델 자체는 단순:
- 박스 → 성벽, 텐트 기둥, 무기 거치대
- 실린더 → 깃발 봉, 폐허 기둥
- 디포메이션된 박스 → 큰 바위, 돌무더기

복잡한 메쉬는 오히려 픽셀 텍스처와 안 어울림.

**모델당 폴리곤 수**: 100~500개면 충분.

#### Step 2: UV 언랩핑

- **수직 투영(Box Unwrap)** 사용 — 박스 면마다 픽셀 또렷
- Smart UV는 픽셀 아트와 안 어울림
- 면 크기에 비례한 UV 영역 (Pixel/Unit 비율 일관)

#### Step 3: 픽셀 텍스처 제작

- 해상도: **32x32 ~ 128x128** (모델 크기에 비례)
- 도구: Aseprite, Photoshop, Krita
- Filter 안 쓰기 (안티앨리어싱 X)
- 색 팔레트 제한 (HD-2D 톤에 맞춰 따뜻한 색)

#### Step 4: 익스포트

- 포맷: `.fbx` (Unity 표준) 또는 `.blend` 직접 import
- Scale: 1 unit = 1m (Unity와 일치)

### 4.3 Unity Import 설정

#### 텍스처 Import 설정 (HD-2D 핵심)
```
Filter Mode    : Point (no filter)        ← 픽셀 또렷
Compression    : None or High Quality
Wrap Mode      : Clamp 또는 Repeat
Max Size       : 256 (작게 유지)
sRGB           : on (color texture)
```

#### Material 설정
```
Shader: Universal Render Pipeline/Unlit (Phase 1 시작 - 단순)
       → Phase 3에서 Lit으로 전환 (조명 받게)

Surface Inputs:
  Base Map    : 픽셀 텍스처
  Smoothness  : 0 (반사 없음)
```

### 4.4 배치 가이드

전장 좌표계 기준 (현재 BattleField 14x5 그리드):

```
       z (적 진영 뒤, 화면 위)
       ↑
  성벽/요새 잔해 (3D)              ← 적 진영 끝
  ─────────────────
  깃발 (3D 봉 + 천)
  
  [필드 14x5 그리드]              ← 전투 영역 (변경 없음)
  
  깃발 (3D 봉 + 천)
  ─────────────────
  성벽/요새 잔해 (3D)              ← 아군 진영 끝
       ↓
       (z = 카메라 가까운 쪽)
```

좌/우 외곽:
- 가까운 측면: 큰 바위 / 폐허 기둥 (카메라에 크게)
- 먼 측면: 작은 돌무더기 / 자연 디테일

### 4.5 EnvironmentSetup.cs 확장

기존 `CreateGrassGround()` + `CreateTrees()` 와 동일한 패턴:

```csharp
private void Start()
{
    CreateGrassGround();
    CreateTrees();              // 2D 빌보드 (기존)
    CreateBattleStructures();   // 3D 환경 오브제 (신규) ⭐
}

private void CreateBattleStructures()
{
    PlaceFortressWalls();       // 양 진영 끝 성벽
    PlaceFlags();               // 양 진영 깃발
    PlaceBoulders();            // 외곽 큰 바위
    // ... 등
}
```

각 메서드는 `Resources.Load<GameObject>()` + `Instantiate`로 Prefab 배치.

### 4.6 예상 소요

| 작업 | 시간 |
|------|------|
| Blender 모델링 (8~10 오브제) | 2~3일 |
| 픽셀 텍스처 제작 | 1~2일 |
| Unity Import + Prefab 만들기 | 0.5일 |
| EnvironmentSetup.cs 확장 + 배치 | 1일 |
| 배치 튜닝 | 0.5일 |

**총 5~7일**

### 4.7 Phase 1 후 예상 결과

- 전장에 입체감 (양 진영 성벽 + 깃발이 무대 형성)
- 외곽 큰 바위/폐허로 깊이감
- "픽셀 게임" → "HD-2D 게임"으로 정체성 확보
- 후속 Phase의 기반 마련

### 4.8 Phase 1 체크리스트

- [ ] DDworld 전장 오브제 우선순위 결정 (⭐⭐⭐ 2개부터)
- [ ] Blender에서 첫 모델 (성벽 또는 깃발)
- [ ] UV 언랩 (Box Unwrap)
- [ ] 픽셀 텍스처 제작 (Aseprite, 32~128px)
- [ ] FBX 익스포트 → Unity Import
- [ ] Texture Filter Mode = Point 설정
- [ ] Material 셋업 (Unlit으로 시작)
- [ ] Prefab 만들기
- [ ] EnvironmentSetup.cs에 CreateBattleStructures 추가
- [ ] 양 진영 / 외곽 배치
- [ ] 위치/스케일 튜닝
- [ ] BalanceTest, SampleScene 둘 다 적용 확인
- [ ] ⭐⭐⭐ 2개 완료 → ⭐⭐ 추가 결정
- [ ] 결과 검토 → Phase 2 진입 결정

---

## 5. Phase 2 — URP Volume + Post-processing

**목표**: 3D 환경에 영화적 색감 + 빛 번짐 + 미니어처 효과

### 5.1 적용 효과 6가지

#### 1. Bloom (필수)
- Threshold: 0.9~1.1 / Intensity: 0.4~0.8 / Scatter: 0.7
- Sea of Stars 수준 (절제)

#### 2. Color Adjustments (필수)
- Contrast: 5~10
- Color Filter: 따뜻한 색 (RGB 1.0, 0.97, 0.92)
- Saturation: 5~15

#### 3. Depth of Field / Tilt-shift
- Mode: Bokeh 또는 Gaussian
- Focus Distance: 6~8

#### 4. Vignette
- Intensity: 0.25~0.4

#### 5. Color LUT (선택)

#### 6. Film Grain (선택)
- Type Thin 1, Intensity 0.2~0.3

### 5.2 Unity 셋업

1. Hierarchy > Create > Volume > Global Volume
2. Profile → New Profile: `Assets/Settings/HD2D_VolumeProfile.asset`
3. 효과들 Add Override
4. Camera Component → Post Processing: ✅
5. Anti-aliasing: SMAA (TAA는 픽셀 아트와 안 맞음)

### 5.3 예상 소요

1~2일.

---

## 6. Phase 3 — 조명 시스템

**목표**: 그림자 + 시간대 분위기로 입체감 추가

### 6.1 적용 항목

- Directional Light: 50°, -30°, 0° (석양 방향), 황금빛
- Ambient Lighting: Color, 푸른 그림자 톤
- 머티리얼 Lit 전환 (잔디부터 점진적, 그 다음 3D 환경 오브제)
- Shadow Type: Soft Shadows

### 6.2 시간대 분위기 (선택, MVP 후)

여러 Volume Profile 만들어 페이즈별: HD2D_Day / Sunset / Night.

### 6.3 예상 소요

1~2일.

---

## 7. Phase 4 — 셰이더 폴리시

**목표**: 환경에 생동감 추가 (정적 → 동적)

### 7.1 우선순위 셰이더

- 잔디 흔들림 셰이더 (Vertex 시간 기반)
- **깃발 흔들림** (3D 깃발 천 부분 — Phase 1에서 만든 모델)
- 거리 페이드 (선택)
- Pixel-perfect Outline (선택)

### 7.2 예상 소요

2~3일.

---

## 8. Phase 5 — 작은 환경 디테일

**목표**: 분위기 + 디테일 추가

### 8.1 추가 요소

- 파티클: 꽃잎 / 빗방울 / 먼지 / 빛입자 / 반딧불
- 작은 오브젝트 (풀숲 다발, 작은 돌)
- 카메라 셰이크 (전투 임팩트)
- 동적 변화 (시간대 전환, 충격 플래시)

### 8.2 예상 소요

2~3일 (선택사항 많음).

---

## 9. 성능 예산 종합

기술 표준 ([CLAUDE.md](../../../CLAUDE.md)):
- Target FPS: 60 / Frame Budget: 16.6ms
- Draw Calls: < 200 / Memory: 2GB

| Phase | 추가 비용 | 누적 |
|-------|---------|------|
| 현재 (Phase 0) | - | ~5ms |
| **Phase 1 (3D 환경)** | +0.5~1ms (low-poly) | 5.5~6ms |
| Phase 2 (Post-processing) | +2~3ms | 7.5~9ms |
| Phase 3 (조명 + 그림자) | +1~2ms | 8.5~11ms |
| Phase 4 (셰이더) | +0.5~1ms | 9~12ms |
| Phase 5 (파티클) | +1~2ms | 10~14ms |

**Phase 5까지 가도 16.6ms 예산 안에 안전**.

3D 환경의 Draw Call 영향: 모델당 1~2 draw call. 8~10 모델이면 +10~20 draw call. 200 한도 대비 충분히 여유.

---

## 10. 의사결정 / Open Questions

### 결정된 것 ✅

| 항목 | 결정 | 근거 |
|------|------|------|
| 접근 방식 | 옵션 C (현재 씬 점진적 폴리시) | 인디 1인 스코프 |
| **우선순위** | **Phase 1 (3D 환경) → 2 (Post-processing) → 3 (조명) → 4 (셰이더) → 5 (디테일)** | **3D 환경이 HD-2D 정체성 핵심** |
| 레퍼런스 | Sea of Stars 수준 (절제된 옥토패스) | 인디 현실성 |
| 카메라 | 현재 쿼터뷰 X 29.2° 유지 | 검증된 값 |
| **3D 모델 소스** | **Blender (직접 모델링)** | Nathan이 진행 가능 |

### 미결정 ❌

| 질문 | 결정 시점 |
|------|---------|
| 어떤 3D 오브제 우선 만들까? (8~10개 후보 중) | Phase 1 시작 시 |
| 픽셀 텍스처 도구 (Aseprite vs Photoshop)? | Phase 1 시작 시 |
| 텍스처 해상도 표준 (32 / 64 / 128px)? | Phase 1 시작 시 |
| Material 시작 — Unlit vs Lit? | Phase 1: Unlit, Phase 3에서 Lit 전환 |
| Phase 2 파라미터 정확한 값? | Phase 2 진입 시 실험 |
| 시간대 변화 (Day/Sunset/Night) MVP 포함? | Phase 3 진입 시 |
| 잔디 흔들림 셰이더 그래프 vs HLSL? | Phase 4 진입 시 |
| 파티클 종류와 밀도? | Phase 5 진입 시 |

---

## 11. 다음 단계 (Phase 1 진입 시)

1. **3d-environment.md** 작성 (Phase 1 상세 — 모델 목록 확정, Blender 가이드, Import 설정)
2. Blender에서 첫 모델 (가장 우선순위 높은 — 성벽 또는 깃발) 시작
3. Unity에 Import + Material 셋업 + Prefab 생성
4. EnvironmentSetup.cs에 CreateBattleStructures 메서드 추가
5. 첫 모델 배치 후 결과 확인
6. 만족스러우면 → 나머지 모델 진행
7. 모든 ⭐⭐⭐ 오브제 완료 후 → Phase 2 진입 결정

---

## 12. 참고 자료

### Unity URP 공식 (6.3 LTS)
- [URP Post-processing](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.0/manual/integration-with-post-processing.html)
- [URP Lit Shader](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.0/manual/lit-shader.html)
- [Unity 픽셀 아트 권장 사항](https://docs.unity3d.com/6000.0/Documentation/Manual/2d-pixel-art.html)

### Blender + 픽셀 아트 (검색 키워드)
- "Blender low-poly tutorial"
- "Pixel art texture mapping Blender"
- "HD-2D 3D model tutorial"
- "Blender FBX export Unity"

### 픽셀 텍스처 도구
- Aseprite ($19.99, 픽셀 아트 표준)
- Photoshop, Krita (대안)
- LibreSprite (Aseprite 무료 대안)

### 참고 게임 (직접 분석)
- 옥토패스 트래블러 1, 2 (Square Enix)
- Sea of Stars (Sabotage Studio) — 인디 레퍼런스
- Triangle Strategy (Square Enix) — 그리드 전략 + HD-2D
- Live A Live (Square Enix HD-2D 리메이크)

---

> **이 문서는 Phase 진행에 따라 업데이트됩니다.** 실제 적용한 값/모델/파라미터는 각 Phase의 상세 문서(`3d-environment.md`, `post-processing.md` 등)에 기록.
