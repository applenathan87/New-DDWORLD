# Visual Polish — Low-poly Voxel 3D 구현 가이드

> **Status**: HD-2D 방향성 폐기 → Low-poly Voxel 3D 전환 (2026-05-25, [ADR-002](../../../docs/architecture/ADR-002-visual-style-low-poly-3d.md))
> **Last Updated**: 2026-05-25
> **Owner**: nathan
>
> ⚠️ **방향 전환 안내**: 이 폴더는 원래 HD-2D 구현 가이드였으나, 2026-05-25 비주얼 스타일 결정으로 **Low-poly Voxel 3D 방향으로 재편**됩니다. [hd2d-setup-plan.md](hd2d-setup-plan.md)는 deprecated 참고용으로 보존됩니다.

이 폴더는 DDworld의 **Low-poly Voxel 3D 비주얼**을 실제 Unity 프로젝트에서 구현하기 위한 가이드 문서를 담습니다.

`design/art/art-bible.md`가 **"무엇을(What)"** — 비주얼 방향성 — 을 다룬다면, 이 폴더는 **"어떻게(How)"** — Unity URP에서 어떤 설정/효과/셰이더로 구현할지 — 를 다룹니다.

## 도구 체인

```
MagicaVoxel (캐릭터 부위별 모델링)
    ↓ .fbx
Blender (본 + 리깅 + 애니메이션)
    ↓ .fbx
Unity 6.3 LTS URP
    + Bloom / DOF (Gaussian) / Tilt-Shift / Color Grading / Vignette
```

## 주요 효과 (Unity URP)

| 효과 | URP 기본 | 비고 |
|------|--------|------|
| Bloom | ✅ | 따뜻한 빛 번짐 |
| DOF (Gaussian) | ✅ | 가벼움, DDworld에 적합 |
| Color Grading | ✅ | 따뜻한 톤 (R 1.0, G 0.97, B 0.92) |
| Vignette | ✅ | 시선 집중 |
| **Tilt-Shift** | ❌ Custom Renderer Feature | 미니어처 핵심. [Noveltech 튜토리얼](https://www.noveltech.dev/tilt-shift-unity) 또는 [iMemento GitHub](https://github.com/iMemento/Tilt-Shift-URP) 참조 |
| Anti-aliasing | SMAA 권장 | Voxel 엣지 부드럽게 (TAA는 Low-poly와 안 맞음) |

---

## 🎯 현재 작업 컨텍스트 (다음 세션 진입용)

### 왜 이 폴더가 만들어졌나?

Day 28 (2026-04-27) 작업 마무리 후, **게임의 룩이 "단순한 픽셀 게임"처럼 보인다는 인식** 때문. art-bible에서 정의한 HD-2D 정체성과 현재 실제 화면이 괴리됨.

**진단**: 코드/씬 구조의 문제가 아닌 **렌더링 파이프라인 셋업의 문제**.
- Post-processing 미적용 (Bloom, DOF, Color grading, Tilt-shift 등)
- 모든 머티리얼이 `URP/Unlit` (조명 무시)
- Volume Profile 없음
- 환경 디테일 부족 (잔디 흔들림 셰이더, 파티클 등)

### 작업 방향 결정

3가지 옵션 검토:
- **A. 새 Unity 프로젝트** — 마이그레이션 부담 큼, 인디 1인에게 비현실적
- **B. 새 씬에서 HD-2D 셋업** — Renderer Asset 충돌 가능성
- **C. 현재 씬에서 점진적 폴리시** ⭐ **선택**

**선택된 방향: 옵션 C**
- 현재 코드/시스템은 잘 작동하므로 깨고 다시 짤 필요 없음
- HD-2D는 셋업 + 셰이더의 문제이지 코드의 문제가 아님
- 점진적 검증 가능 (한 단계씩 결과 보고 다음 결정)

### 진행 방식

**문서 먼저 → 검토 → 실제 작업**:
1. 이 폴더에 마스터 플랜 작성 (Phase 1~4)
2. Nathan이 문서 검토 (코드 손대기 전)
3. 방향 OK면 Phase 1 (Volume + Post-processing) 실제 구현 진입
4. Phase 1 결과 보고 다음 Phase 결정

코드 손대기 전 단계라 **잘못 판단해도 비용 0**. 신중하게 검토 가능.

---

## 📁 폴더 인덱스

| 파일 | 내용 | 상태 |
|------|------|------|
| **README.md** | 이 파일. 인덱스 + 현재 작업 컨텍스트 | ✅ 작성됨 |
| **hd2d-setup-plan.md** ⭐ | **마스터 플랜**. Phase 1~5 단계별 계획 (v2: 3D 환경 우선) | ✅ 작성됨 |
| **3d-environment.md** ⭐ | **Phase 1 상세** (Blender 모델 목록, Import 설정, 배치) | 📋 Phase 1 진입 시 작성 |
| post-processing.md | Phase 2 상세 (Volume Profile 파라미터) | 📋 Phase 2 진입 시 작성 |
| lighting.md | Phase 3 상세 (조명 시스템) | 📋 Phase 3 진입 시 작성 |
| shaders.md | Phase 4 상세 (커스텀 셰이더) | 📋 Phase 4 진입 시 작성 |
| decisions/ | 의사결정 기록 (채택/배제 이유) | 📋 필요 시 작성 |

**점진적 확장 원칙**: 사용 안 하는 파일을 미리 만들지 않습니다. Phase 진입할 때 해당 문서 작성.

---

## 🚀 다음 세션 빠른 시작 가이드

새 세션에서 이 작업을 이어가실 때:

### Step 1: 컨텍스트 복구
```
1. 이 README.md 읽기 (현재 컨텍스트 파악)
2. hd2d-setup-plan.md 읽기 (마스터 플랜 확인)
3. design/art/art-bible.md 읽기 (비주얼 정체성 재확인)
```

### Step 2: 검토 결과에 따라 분기

#### 시나리오 A: 마스터 플랜이 마음에 듬 → Phase 1 진입
- "Phase 1 시작하자" 한 마디면 OK
- **3d-environment.md 작성** + Blender 모델링 가이드 + Unity Import 워크플로우
- Phase 1 = **3D 환경 구축** (성벽, 깃발, 큰 바위 등)
- 예상 소요: 5~7일 (Blender 작업 포함)

#### 시나리오 B: 방향 수정 필요
- hd2d-setup-plan.md 수정
- 어떤 부분 수정 원하는지 의견 → 문서 업데이트
- 코드 손대기 전이라 비용 0

#### 시나리오 C: 다른 작업 우선
- Day 28 메모에 적힌 **함정 배치 룰 상세 설계**가 다른 우선순위 후보
- 그쪽 진행하다가 나중에 visual polish로 복귀

---

## 📝 관련 문서

### 이 프로젝트 내
- [design/art/art-bible.md](../art-bible.md) — 비주얼 정체성 (전체 방향)
- [design/gdd/game-concept.md](../../gdd/game-concept.md) — 게임 컨셉 (필라 / 안티-필라)
- [docs/engine-reference/unity/VERSION.md](../../../docs/engine-reference/unity/VERSION.md) — Unity 6.3 LTS

### 외부 참고
- Unity URP Post-processing 공식 문서 (Unity 6.3 LTS)
- 옥토패스 트래블러 비주얼 분석
- Sea of Stars 비주얼 분석
- Triangle Strategy 비주얼 분석

---

## ⚠️ 작업 시 주의사항

1. **현재 작동하는 시스템 깨지 말 것** — 코드 거의 그대로, 셋업/셰이더만 추가
2. **BalanceTest 씬과 SampleScene 둘 다 영향 받음** — 둘 다 비주얼 동기화되도록 신경 써야
3. **성능 영향 추적** — Phase별로 GPU/CPU 비용 측정 (Stats 창)
4. **Graphics Quality 옵션은 출시 단계까지 보류** — MVP는 단일 품질로
5. **점진적 진행** — Phase 1 결과 보고 Phase 2 결정. 한 번에 다 하지 않음
