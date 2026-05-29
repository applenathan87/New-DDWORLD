# Blender → Unity 파이프라인

> **Status**: Draft (실제 모델 작업하며 검증 예정)
> **Last Updated**: 2026-05-28
> **Stack**: MagicaVoxel → Blender → Unity 6.3 LTS + URP
> **관련 문서**: [art-bible.md](../../design/art/art-bible.md) §1 (Rigid Voxel Animation), [ADR-002](../architecture/ADR-002-visual-style-low-poly-3d.md)

DDworld의 캐릭터/환경 에셋이 MagicaVoxel → Blender → Unity로 흘러가는 전체 파이프라인을 정리한 문서. 첫 모델 작업 시 이 문서를 따라가면 누락 없이 셋업할 수 있도록 작성됨.

---

## 1. 전체 파이프라인 개요

```
MagicaVoxel        Blender               Unity
─────────         ─────────             ─────────
부위별 .obj   →   본 리깅 +         →   .fbx 임포트
  export          애니메이션              + URP 머티리얼
  (Vertex Color)  (12~24fps)              + Animator
                  .fbx export             + GPU Instancing
                                          ↓
                                       카메라 + URP Volume
                                       (DOF, Bloom, Tilt-shift)
                                          ↓
                                       화면 출력
```

---

## 2. 모델 임포트: Export 형식 선택

세 가지 옵션 중 **`.fbx` 권장**.

| 형식 | 장점 | 단점 | 추천 |
|------|------|------|------|
| **.fbx** | 본/애니메이션/메쉬/UV 모두 포함, 안정적, 표준 | 파일 크기 큼 | ✅ DDworld 기본 |
| **.blend 직접** | Blender 수정 시 Unity 자동 갱신 | Blender 설치 필수, 버전 차이 문제 | ❌ |
| **.glb / .gltf** | 작고 모던 표준 | Unity 기본 지원 약함 (플러그인 필요) | ❌ |

**Blender export 설정 (.fbx):**
- `File → Export → FBX (.fbx)`
- Scale: 1.0 (Blender 1m = Unity 1m)
- Apply Scalings: FBX All
- Forward: -Z Forward, Up: Y Up (Unity 기본)
- Armature → Add Leaf Bones: Off
- Bake Animation: On (애니메이션 포함 시)

---

## 3. 머티리얼: 다시 만들어야 한다 ⚠️

**Blender 머티리얼(Principled BSDF 등)은 Unity로 안 넘어온다.** 색상 정보만 일부 따라오고, 셰이더 자체는 Unity URP 셰이더로 재지정 필요.

### Voxel 캐릭터의 머티리얼 구성

```
셰이더: URP/Lit (또는 URP/Simple Lit, URP/Unlit)
  ├ Base Map: 없음 (텍스처 안 씀)
  ├ Base Color: Vertex Color 활성화 → MagicaVoxel 색상이 그대로 표시
  ├ Metallic: 0
  ├ Smoothness: 0 ~ 0.1 (Voxel은 무광)
  └ Enable GPU Instancing: ✅
```

### 핵심 트릭: Vertex Color

MagicaVoxel은 각 voxel에 색을 직접 칠하고, 이게 export 시 **Vertex Color**로 변환되어 .fbx에 들어간다. URP 셰이더가 Vertex Color를 읽도록 설정만 하면 텍스처 없이 색이 표시된다.

**URP/Lit이 기본적으로 Vertex Color를 안 읽으므로 두 가지 방법 중 택일:**
- Shader Graph로 커스텀 셰이더 생성 (Vertex Color 노드 → Base Color)
- 무료 에셋 "URP Vertex Color Shader" 사용

---

## 4. Unity Import 설정 (.fbx Inspector)

fbx를 Unity에 넣으면 Inspector에 4개 탭이 보인다.

### Model 탭

| 설정 | 값 | 이유 |
|------|------|------|
| Scale Factor | 1 | Blender 1m = Unity 1m |
| Mesh Compression | Off | Voxel은 폴리곤 적음, 압축 불필요 |
| Read/Write | Off | 런타임 메쉬 수정 안 함 |
| Optimize Game Objects | ✅ | 본 계층 최적화 |
| Generate Colliders | Off | 별도로 캡슐 콜라이더 부착 예정 |
| Import BlendShapes | Off | Voxel은 BlendShape 안 씀 |

### Rig 탭

| 설정 | 값 | 이유 |
|------|------|------|
| Animation Type | **Generic** | Voxel은 표준 휴머노이드 본 구조와 다름 |
| Avatar Definition | Create from this Model | |
| Optimize Game Objects | ✅ | 본 GameObject를 최적화 데이터로 압축 |

> ⚠️ **Humanoid는 사용 금지**. Humanoid는 사람형 본 매핑 가정 — Voxel 캐릭터의 분리된 부위 구조와 충돌.

### Animation 탭

| 설정 | 값 | 이유 |
|------|------|------|
| Import Animation | ✅ | |
| Resample Curves | Off | 12~24fps 키프레임 그대로 유지 |
| Anim. Compression | Keyframe Reduction | 용량 절감 |
| Rotation Error | 0.5 | |

**클립 분리:**
- 같은 fbx에 여러 애니메이션(Idle, Walk, Attack)이 있으면 이 탭에서 start/end 프레임 지정해 클립 분리
- 또는 Blender에서 NLA Track으로 묶어서 export하면 자동 분리됨

### Materials 탭

| 설정 | 값 | 이유 |
|------|------|------|
| Material Creation Mode | **None** | 자동 생성 머티리얼 무시 |
| | | 직접 URP 머티리얼 만들어 할당 |

---

## 5. 씬에 배치 후 렌더링

1. fbx 파일을 Hierarchy에 드래그 → 씬에 배치
2. 자식으로 `SkinnedMeshRenderer` 또는 `MeshRenderer` 컴포넌트 자동 생성
3. 직접 만든 URP 머티리얼을 메쉬에 할당 → 색이 보임
4. `Animator` 컴포넌트 + `Animator Controller` 연결 → 애니메이션 재생

---

## 6. Voxel 캐릭터 특화 최적화

### A. 머티리얼은 1개로 통일 → GPU Instancing

- 모든 voxel 캐릭터가 같은 URP 셰이더 + Vertex Color 사용
- 머티리얼 1개를 200 유닛이 공유하면 **GPU Instancing**으로 draw call이 1~몇 개로 축소
- 머티리얼 Inspector에서 `Enable GPU Instancing` 체크

### B. 팀 컬러는 MaterialPropertyBlock

- 같은 머티리얼 인스턴스를 공유하되 색만 다르게
- `MaterialPropertyBlock`으로 캐릭터별 Tint 색 주입
- 블루팀 / 레드팀이 같은 머티리얼 인스턴스를 쓰면서도 색 분리 가능 → GPU Instancing 유지

```csharp
// 예시 코드 (실제 코드 작업 시 src/에 구현)
var props = new MaterialPropertyBlock();
props.SetColor("_TeamColor", teamColor);
renderer.SetPropertyBlock(props);
```

### C. 조명: Lightmap이 아닌 Light Probe

- Voxel 캐릭터는 동적 오브젝트 → Lightmap 사용 안 함
- 씬에 `Light Probe Group`을 배치하면 동적 캐릭터가 자연스럽게 조명 받음

### D. 그림자

- MeshRenderer: `Cast Shadows: On`, `Receive Shadows: On`
- URP Asset에서 **Soft Shadows 활성화** → art-bible §2의 "부드러운 소프트 섀도우" 구현

### E. Anti-Aliasing

- URP Camera 컴포넌트: `Anti-aliasing: Subpixel Morphological (SMAA)`
- Voxel의 각진 엣지를 부드럽게 처리 (art-bible §1에 명시됨)

---

## 7. 애니메이션 임포트: Rigid Voxel 원칙 적용

art-bible §1 "Rigid Voxel Animation"에 따라:

- **Animation Type**: Generic (위에서 설정)
- **fps**: 12 ~ 24fps (Blender 키프레임 그대로 유지)
- **Resample Curves**: Off (Unity가 24fps 등으로 재샘플링하지 않도록)
- **Anim. Compression**: Keyframe Reduction (용량 절감하되 키프레임은 유지)

> 메쉬 변형 없이 본 회전 + Scale만 사용하므로 압축 효과가 매우 큼. 일반 캐릭터 대비 애니메이션 용량 1/3 이하로 떨어진다.

---

## 8. 자주 만나는 문제 (To be filled)

> 실제 모델 작업하면서 발생하는 이슈를 여기에 추가.

- [ ] 첫 모델 임포트 시 색이 안 보임 → Vertex Color 셰이더 설정 누락 가능성
- [ ] 본 회전이 이상함 → Blender → Unity 축 변환 (-Z Forward / Y Up) 확인
- [ ] 애니메이션이 너무 부드러움 → Resample Curves 끄고 키프레임 보존 확인
- [ ] Draw call이 줄지 않음 → 머티리얼 GPU Instancing 체크 + MaterialPropertyBlock 사용 확인

---

## 9. 체크리스트: 첫 모델 셋업

- [ ] MagicaVoxel에서 부위별 분리 모델링 (Head/Body/Arm_L/Arm_R/Leg_L/Leg_R/Weapon/Shield)
- [ ] Blender에서 본 1개당 부위 메쉬 1개 100% 바인딩 (Rigid Voxel Animation)
- [ ] 12~24fps 애니메이션 클립 작성
- [ ] .fbx export (-Z Forward / Y Up, Scale 1.0)
- [ ] Unity 임포트 후 Rig: Generic, Optimize Game Objects On
- [ ] URP 머티리얼 생성 (Vertex Color 셰이더, GPU Instancing On)
- [ ] 씬에 배치 후 색/애니메이션 정상 동작 확인
- [ ] Light Probe Group으로 조명 받는지 확인
- [ ] SMAA 적용된 카메라로 렌더링 확인

---

## 10. 다음 단계

- 실제 첫 캐릭터 모델 작업 시 이 문서대로 진행
- 발생한 이슈는 §8에 추가
- 안정화되면 art-bible §8 "Asset Standards"에 핵심 규칙만 요약 반영
