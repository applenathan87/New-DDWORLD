# HD-2D Setup Plan — 마스터 플랜

> **Status**: 계획 단계 (코드 변경 0)
> **Author**: nathan
> **Last Updated**: 2026-04-27
> **Target**: 옥토패스 트래블러 / Sea of Stars 수준의 HD-2D 룩
> **Engine**: Unity 6.3 LTS, URP (Universal Render Pipeline)

---

## 0. 요약

DDworld의 비주얼 정체성은 **HD-2D**입니다 (art-bible.md). 그러나 현재 게임은 단순한 픽셀 게임처럼 보이며, 그 원인은 **렌더링 파이프라인 셋업의 부재**입니다 (코드 문제 아님).

이 문서는 **현재 씬을 점진적으로 폴리시하여 HD-2D 룩을 구현하는 4-Phase 마스터 플랜**을 제시합니다. 코드 리팩토링은 거의 필요 없습니다.

**예상 소요**: 총 5~10일 (Phase별 1~3일). 인디 1인 기준.

---

## 1. 목표 — DDworld의 HD-2D 정체성

### 1.1 art-bible 기반 핵심 비주얼 원칙

(art-bible.md에서 정의된 내용 요약)
- **HD-2D + Tilt-shift** 비주얼 디렉션
- 픽셀 아트 스프라이트 (병종) + 3D 환경
- 따뜻하고 부드러운 톤
- 쿼터뷰 카메라 (X 29.2° 기울임)
- 옥토패스 트래블러 / TABS / Into the Breach 레퍼런스

### 1.2 HD-2D 룩의 핵심 시각 요소 (분석)

#### 옥토패스 트래블러의 비주얼을 분해하면:

1. **Bloom** (빛 번짐) — 가장 큰 시각 정체성. 빛이 나는 영역이 부드럽게 번짐
2. **Depth of Field / Tilt-shift** — 가까운/먼 영역 흐림 → 미니어처 느낌
3. **Color Grading** — 따뜻한 색온도, 부드러운 채도
4. **Vignette** — 화면 가장자리 어두움 → 시선 집중
5. **Film Grain / LUT** — 영화적 질감
6. **Dynamic Lighting** — 그림자 + 시간대 분위기
7. **Particle Effects** — 빛입자, 꽃잎, 먼지 (분위기 연출)
8. **Layered Depth** — 전경/중경/배경 레이어 (DDworld는 이미 일부 구현)
9. **Shader Polish** — 잔디 흔들림, 거리 페이드 등

**핵심 인사이트**: 옥토패스의 코드 자체는 평범함. 차이의 90%는 **Volume + 셰이더 + 조명**에서 옴.

---

## 2. 현재 상태 진단

### 2.1 잘 되어 있는 것 ✅

- **쿼터뷰 카메라** (X 29.2°) — HD-2D 정통 각도
- **잔디 + 나무 환경** — 기본 레이어 구성
- **카메라 흐름** — 페이즈별 단순한 위치/FOV 전환 + 휠 줌
- **픽셀 아트 스프라이트** — 병종, 카드, 나무 (basic_tile_v2 등)
- **자동 환경 생성** — 모든 씬에서 동일 환경 보장

### 2.2 부족한 것 ❌

| 항목 | 현재 | 부족한 이유 |
|------|------|------------|
| **Post-processing** | 없음 | URP Volume 미적용 |
| **조명** | 모두 `URP/Unlit` (조명 무시) | Lit 셰이더 + Directional Light 필요 |
| **색감/톤** | 원본 텍스처 색 그대로 | Color Adjustments 없음 |
| **거리감** | 평면 빌보드 | Depth of Field, 거리 페이드 없음 |
| **환경 디테일** | 잔디 + 나무만 | 흔들림 셰이더, 파티클 부재 |
| **그림자** | 없음 | Lit 머티리얼 + Shadow Caster 필요 |
| **카메라 효과** | 단순 트윈 | 카메라 셰이크, 영화적 효과 없음 |

### 2.3 진단 결론

**원인은 코드가 아닌 "렌더링 셋업 부재"**. 따라서 점진적 셋업 추가만으로 큰 비주얼 변화 가능.

---

## 3. 참고 게임 분석

### 3.1 옥토패스 트래블러 (Octopath Traveler) — Square Enix
**HD-2D의 정의를 만든 게임.** 가장 가까운 레퍼런스.

핵심 요소:
- **강한 Bloom** — 빛 영역이 매우 부드럽게 번짐
- **Tilt-shift Depth of Field** — 화면 상단/하단 흐림 → 미니어처
- **따뜻한 색온도** — 모든 씬에 황금빛 톤
- **유닛 픽셀 아트 + 3D 환경의 조합**
- **시간대 변화** (낮/석양/밤) → 분위기

DDworld와의 공통점: 픽셀 아트 캐릭터 + 3D 환경
DDworld와의 차이점: RPG vs 오토배틀러 (UI/카드 시스템 다름)

### 3.2 Sea of Stars — Sabotage Studio
**옥토패스의 영향을 받은 인디 게임.** 인디 스코프 레퍼런스로 적합.

핵심 요소:
- 옥토패스 대비 절제된 Bloom (눈 피로 적음)
- 더 강한 색감 대비
- 환경 파티클 (반딧불, 꽃잎) 활용

DDworld 적용 시: Sea of Stars 수준의 **절제된 Bloom**이 인디에게 현실적

### 3.3 Triangle Strategy — Square Enix / Artdink
**HD-2D 전략 게임.** DDworld와 가장 비슷한 장르감.

핵심 요소:
- 그리드 기반 전투 + HD-2D
- 카메라 줌/회전이 적극적
- 그리드 셀 시각화 + HD-2D 환경 융합

DDworld와의 시각적 유사성: 그리드 + 환경. **카메라/UI 융합 방법** 참고할 가치 있음

### 3.4 Into the Breach — Subset Games
**HD-2D는 아니지만 그리드 전략 게임의 비주얼 명료성 레퍼런스.**

핵심 요소:
- 그리드 + 유닛이 매우 명료하게 보임
- 시야 가림 없음 (DDworld의 나무 가림 방지 원칙과 일치)

---

## 4. Phase 1 — URP Volume + Post-processing ⭐ 최우선

**목표**: 가장 큰 시각 변화를 가장 적은 작업으로 달성. 80% 효과를 1~2일에.

### 4.1 적용할 효과 6가지

#### 1. Bloom (필수 — HD-2D의 정수)
- **용도**: 빛이 나는 영역 부드럽게 번짐
- **HD-2D 영향도**: ★★★★★
- **권장 파라미터**:
  - Threshold: 0.9~1.1 (밝은 부분만 영향)
  - Intensity: 0.4~0.8 (절제된 정도)
  - Scatter: 0.7
- **참고**: 옥토패스는 강함, Sea of Stars는 절제. **Sea of Stars 수준 추천**

#### 2. Color Adjustments (필수 — 따뜻한 톤)
- **용도**: 전체 색감 조정
- **HD-2D 영향도**: ★★★★☆
- **권장 파라미터**:
  - Post Exposure: 0
  - Contrast: 5~10
  - Color Filter: 살짝 따뜻한 색 (RGB 약 1.0, 0.97, 0.92)
  - Saturation: 5~15 (살짝 올림)

#### 3. Depth of Field / Tilt-shift (선택 — 미니어처 느낌)
- **용도**: 가까운/먼 영역 흐림
- **HD-2D 영향도**: ★★★★☆
- **모드**: Bokeh (영화적) 또는 Gaussian (저비용)
- **권장 파라미터**:
  - Focus Distance: 6~8 (필드 중앙)
  - Aperture: 5~8
- **주의**: 너무 강하면 가독성 떨어짐. 절제

#### 4. Vignette (선택 — 시선 집중)
- **용도**: 화면 가장자리 어둡게
- **HD-2D 영향도**: ★★★☆☆
- **권장 파라미터**:
  - Intensity: 0.25~0.4
  - Smoothness: 0.4
  - Color: 검정

#### 5. Color LUT (고급 — 영화적 톤)
- **용도**: 미리 정의된 색감 LUT 적용
- **HD-2D 영향도**: ★★★☆☆ (효과적이지만 학습 필요)
- **선택사항**: Phase 1 후반에 시도

#### 6. Film Grain (선택 — 영화적 질감)
- **용도**: 미세한 그레인 노이즈
- **HD-2D 영향도**: ★★☆☆☆
- **권장**: Type Thin 1, Intensity 0.2~0.3

### 4.2 Unity URP에서 셋업 방법

#### 단계 1: Volume GameObject 생성
1. Hierarchy > Create > Volume > Global Volume
2. 인스펙터: Profile → New 클릭하여 새 VolumeProfile 생성
3. Profile 저장 위치: `prototypes/unity-prototype/Assets/Settings/HD2D_VolumeProfile.asset`

#### 단계 2: 효과 추가
Volume 컴포넌트의 Add Override 버튼 → Post-processing → 위 6개 효과 차례로 추가

#### 단계 3: Camera 설정
Main Camera의 Camera 컴포넌트 인스펙터:
- Rendering > Post Processing: ✅ 체크
- Rendering > Anti-aliasing: SMAA 또는 FXAA (TAA는 픽셀 아트와 안 맞음)

#### 단계 4: URP Renderer 확인
`Assets/Settings/URP_Renderer.asset` 또는 유사 파일에서:
- Post Processing: Enabled

### 4.3 예상 소요 시간

- **셋업**: 30분 (Volume 추가 + 효과 활성화)
- **파라미터 튜닝**: 2~4시간 (Sea of Stars 같은 절제된 룩 찾기)
- **검토 + 다음 단계 결정**: 1~2시간

**총 1일 미만**.

### 4.4 Phase 1 후 예상 결과

게임 화면이 다음과 같이 변화:
- 빛이 나는 부분 (UI 텍스트, 카드 강조 등)이 부드럽게 번짐
- 전체 색감이 따뜻해지고 픽셀 아트가 영화적으로 보임
- 화면 가장자리 어두워져 필드 영역에 시선 집중
- 멀리 있는 나무가 흐려져 미니어처 느낌

**예상 효과**: HD-2D 룩의 70~80% 달성. Phase 2~4는 폴리시 단계.

### 4.5 Phase 1 성능 영향

| 효과 | GPU 비용 (1080p) |
|------|----------------|
| Bloom (Medium) | +1.0~1.5ms |
| Color Adjustments | +0.1ms |
| DOF (Bokeh) | +0.8~1.5ms |
| DOF (Gaussian) | +0.3~0.5ms |
| Vignette | +0.05ms |
| Film Grain | +0.1ms |

**총 +2~3ms** (60FPS 예산 16.6ms 대비 충분히 여유).

---

## 5. Phase 2 — 조명 시스템

**목표**: 그림자와 시간대 분위기로 입체감 + 드라마 추가

### 5.1 적용할 변경

#### 1. Directional Light 설정
- 위치: 씬 내 `Directional Light` GameObject
- 회전: 50°, -30°, 0° (석양 방향)
- 색: 따뜻한 황금빛 (FF E0 B0 정도)
- Intensity: 1.0~1.3

#### 2. Ambient Lighting
- Window > Rendering > Lighting Settings
- Environment Lighting: Source = Color
- Ambient Color: 약간 푸른 하늘 (B0 C0 D0) — 그림자 영역에 색감 부여

#### 3. 머티리얼 전환 (선택적)
현재 모두 `Universal Render Pipeline/Unlit`을 사용. 조명을 받게 하려면:
- 잔디 바닥 → `Universal Render Pipeline/Lit`
- 나무 빌보드 → `Universal Render Pipeline/Sprite-Lit-Default`
- 병종 스프라이트 → `Sprite-Lit-Default`

**주의**: 모두 전환 X. 잔디만 전환해도 큰 차이. 점진적.

#### 4. Shadow 활성화
- Light 컴포넌트의 Shadow Type: Soft Shadows
- URP Renderer Asset의 Shadows 설정 확인

### 5.2 시간대 분위기 옵션

여러 Volume Profile 만들어 페이즈별 적용 가능:
- HD2D_Day.asset (일반 전투)
- HD2D_Sunset.asset (3:0 매치 종료 등 드라마틱한 순간)
- HD2D_Night.asset (특수 시나리오)

**MVP에서는 HD2D_Day 하나만**.

### 5.3 예상 소요

1~2일.

---

## 6. Phase 3 — 셰이더 폴리시

**목표**: 환경에 생동감 추가 (정적 → 동적 느낌)

### 6.1 우선순위 셰이더

#### 1. 잔디 흔들림 셰이더 (필수)
- Vertex 셰이더로 시간 기반 흔들림
- art-bible에 명시된 "환경 디테일 애니메이션"
- 셰이더 그래프 또는 HLSL

#### 2. 거리 페이드 (선택)
- 멀리 있는 나무를 살짝 페이드아웃
- DOF와 결합하면 시너지

#### 3. Pixel-perfect Outline (선택)
- 병종 외곽선 강조
- 픽셀 아트의 선명함 유지

### 6.2 예상 소요

2~3일 (셰이더 학습 곡선 포함).

---

## 7. Phase 4 — 환경 디테일

**목표**: 분위기 + 디테일 추가

### 7.1 추가 요소

#### 1. 파티클 시스템
- 꽃잎 / 빗방울 / 먼지 / 빛입자
- 분위기 연출
- 성능 주의 (max particle count 제한)

#### 2. 환경 오브젝트
- 돌, 풀숲, 작은 디테일
- 변형(variation)으로 단조로움 깨기

#### 3. 동적 변화
- 카메라 셰이크 (전투 임팩트)
- 충격 시 화면 페이드/플래시

### 7.2 예상 소요

2~3일 (선택사항 많음).

---

## 8. 성능 예산 종합

기술 표준 ([CLAUDE.md](../../../CLAUDE.md)):
- Target FPS: 60
- Frame Budget: 16.6ms
- Draw Calls: < 200
- Memory: 2GB

| Phase | 추가 GPU 비용 | 누적 |
|-------|------------|------|
| 현재 (Phase 0) | - | ~5ms |
| Phase 1 (Post-processing) | +2~3ms | 7~8ms |
| Phase 2 (조명 + 그림자) | +1~2ms | 8~10ms |
| Phase 3 (셰이더) | +0.5~1ms | 9~11ms |
| Phase 4 (파티클) | +1~2ms | 10~13ms |

**Phase 4까지 가도 16.6ms 예산 안에 안전**. 단, 저사양 PC에서는 Phase 4 시점에서 옵션 제공 필요.

---

## 9. 의사결정 / Open Questions

### 결정된 것 ✅

| 항목 | 결정 | 근거 |
|------|------|------|
| 접근 방식 | 옵션 C (현재 씬 점진적 폴리시) | 인디 1인 스코프 |
| 우선순위 | Phase 1 → 2 → 3 → 4 | 효과 대비 비용 |
| 레퍼런스 | Sea of Stars 수준 (절제된 옥토패스) | 인디 현실성 |
| 카메라 | 현재 쿼터뷰 X 29.2° 유지 | 검증된 값 |
| 머티리얼 전환 | 점진적, 잔디부터 | 안전한 변경 |

### 미결정 ❌

| 질문 | Owner | 결정 시점 |
|------|-------|---------|
| Phase 1 파라미터 정확한 값은? (Bloom Intensity 등) | nathan | Phase 1 진입 시 실험 |
| DOF는 Bokeh vs Gaussian? | nathan | Phase 1 셋업 시 |
| Color LUT 사용? | nathan | Phase 1 후반 |
| 모든 머티리얼을 Lit으로 전환? | nathan | Phase 2 진입 시 |
| 시간대 변화 (Day/Sunset/Night) MVP에 포함? | nathan | Phase 2 진입 시 |
| 잔디 흔들림 셰이더 그래프 vs HLSL? | nathan | Phase 3 진입 시 |
| 파티클 종류와 밀도? | nathan | Phase 4 진입 시 |

---

## 10. 다음 단계 (실제 진행 시)

이 문서 검토 완료 → Phase 1 실제 구현 진입 시:

1. **post-processing.md** 작성 (Phase 1 상세 — Volume Profile 셋업)
2. Unity Editor에서 Global Volume + Profile 생성
3. 6개 효과 추가 + 파라미터 튜닝
4. BalanceTest 씬에서 결과 확인
5. SampleScene에도 동일 적용
6. 만족스러우면 → Phase 2 진입 결정
7. 만족스럽지 않으면 → 파라미터 추가 조정 또는 방향 재검토

---

## 11. 참고 자료

### Unity URP 공식 (6.3 LTS)
- [URP Post-processing 공식 문서](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.0/manual/integration-with-post-processing.html)
- [Volume Framework 공식 문서](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.0/manual/Volumes.html)
- [URP Lit Shader 공식 문서](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.0/manual/lit-shader.html)

### HD-2D 분석/튜토리얼 (찾아볼 키워드)
- "HD-2D Unity URP tutorial"
- "Octopath Traveler HD-2D analysis"
- "Tilt-shift effect Unity"
- "Bloom intensity Sea of Stars"

### 참고 게임 (직접 플레이 / 영상 분석)
- 옥토패스 트래블러 1, 2 (Square Enix)
- Sea of Stars (Sabotage Studio) — 인디 레퍼런스
- Triangle Strategy (Square Enix)
- Live A Live (Square Enix HD-2D 리메이크)

---

## 부록 — Phase 별 체크리스트 (실제 진행 시 사용)

### Phase 1 체크리스트
- [ ] Global Volume GameObject 생성
- [ ] HD2D_VolumeProfile.asset 생성
- [ ] Bloom 추가 + 파라미터 조정
- [ ] Color Adjustments 추가 + 따뜻한 톤
- [ ] Depth of Field 추가 (Gaussian or Bokeh)
- [ ] Vignette 추가
- [ ] Film Grain 추가 (선택)
- [ ] Camera Post Processing 활성화
- [ ] BalanceTest 씬에서 결과 확인
- [ ] SampleScene에도 적용 확인
- [ ] post-processing.md 작성 (사용한 파라미터 기록)
- [ ] Phase 1 결과 검토 + Phase 2 진입 결정

### Phase 2 체크리스트
- [ ] Directional Light 설정 (석양 방향, 황금빛)
- [ ] Ambient Lighting 설정 (푸른 그림자)
- [ ] 잔디 머티리얼 → Lit 전환 (선택)
- [ ] Shadow Type: Soft Shadows
- [ ] lighting.md 작성

### Phase 3 체크리스트
- [ ] 잔디 흔들림 셰이더 (Wind Shader)
- [ ] 거리 페이드 (선택)
- [ ] Pixel Outline (선택)
- [ ] shaders.md 작성

### Phase 4 체크리스트
- [ ] 파티클 시스템 (꽃잎/먼지)
- [ ] 환경 오브젝트 (돌, 풀숲)
- [ ] 카메라 셰이크 (전투 임팩트)

---

> **이 문서는 Phase 진행에 따라 업데이트됩니다.** 실제 적용한 값, 발견한 문제, 미세 조정 사항은 각 Phase의 상세 문서(post-processing.md 등)에 기록.
