# ADR-002: 비주얼 스타일 — Low-poly Voxel 3D 채택

> **Status**: Accepted — ⚠️ 2026-07-03 「마왕성 인사팀」 전환: **캐릭터 복셀 결정은 유효**하나 (1) 환경 = **로우폴리 3D**로 명확화, (2) 미니어처·틸트시프트 톤 / 부위분리(전투 애니) / 400명 크라우드([ADR-003](ADR-003-rigid-instancing-crowd-rendering.md), Superseded) 전제는 폐기. 상세 = design/gdd/_archive/README.md
> **Date**: 2026-05-25
> **Decider**: nathan
> **Tags**: visual, art-direction, scope, tooling
> **Refined by**: [ADR-003](ADR-003-rigid-instancing-crowd-rendering.md) (2026-06-29) — 성능/렌더 경로를 "GPU Skinning + 200명" 가정에서 **리지드 부위 인스턴싱 + 정점색 1머티리얼 + 데이터 시뮬 + 400명**으로 갱신. 비주얼 스타일 결정 자체는 유효(superseded 아님). 아래 성능 관련 항목은 ADR-003 경로로 대체됨.

---

## Context

DDworld의 기존 비주얼 방향은 **HD-2D** (3D 환경 + 2D 픽셀 스프라이트, [art-bible.md](../../design/art/art-bible.md) 초안 + [hd2d-setup-plan.md v2](../../design/art/visual-polish/hd2d-setup-plan.md)).

그러나 다음 문제점이 발견됨:

1. **HD-2D는 사실상 스퀘어에닉스 독점 기법** — 인디 성공 사례 거의 없음 ([comparisons](../../design/research/comparisons/) 분석)
2. **1인 인디에 과한 작업 부담**:
   - Blender 3D 환경 + Aseprite 픽셀 스프라이트 + 8방향 애니메이션
   - 도구 2개, 워크플로우 2개
3. **카메라 자유도 제한** — 픽셀 스프라이트는 항상 카메라 정면이라야 함. DDworld의 다양한 카메라 워크(배치/공개/전투/트리맵)에 제약
4. **검증된 인디 사례 부재** — 옥토패스 스타일은 AAA 영역

연구 결과:
- **Tabletop Tavern** (Low-poly 3D, 92% 긍정, 인디) = DDworld와 가장 유사한 게임 → 검증
- **Bad North** = Low-poly + 자동 전투 + 미니어처 톤
- **Townscaper, A Short Hike** = Low-poly + DOF/Bloom으로 따뜻한 톤
- **Crossy Road, Cube World** = Voxel 캐릭터 검증

캐릭터 작업 비교 (5병종 × 5애니메이션 기준):
- HD-2D: 픽셀 스프라이트 × 8방향 × 5애니 = 약 200장 그리기
- Voxel + Blender: 모델 5개 + 본 리깅 + .anim 5개 = 30~50시간

---

## Decision

**DDworld는 Low-poly Voxel 3D 비주얼로 가며, 다음 도구 체인을 사용한다:**

```
[캐릭터 디자인]    MagicaVoxel
       ↓ .fbx export (부위별 분리)
[리깅 + 애니메이션] Blender (본 기반, B 방식)
       ↓ .fbx export
[적용 + 렌더링]    Unity 6.3 LTS URP
       + Bloom / DOF (Gaussian) / Tilt-Shift / Color Grading
```

### 비주얼 방향성

- **캐릭터**: Voxel 큐브 분리 부위 (Head/Body/Arms/Legs/Weapon)
- **환경**: Low-poly 메쉬 (집/나무/돌/깃발) — Blender 또는 MagicaVoxel
- **카메라**: 쿼터뷰 + 확대 시 캐릭터 디테일 보임 (zoom 기능 활용)
- **톤**: 따뜻한 색감, Tilt-shift 미니어처 효과, 부드러운 조명
- **레퍼런스**: Bad North, Tabletop Tavern, A Short Hike, Crossy Road

### 폐기되는 것

- HD-2D 방향성 ([hd2d-setup-plan.md v2](../../design/art/visual-polish/hd2d-setup-plan.md) deprecated)
- 픽셀 스프라이트 캐릭터
- 옥토패스 트래블러를 1순위 레퍼런스로 인용

### 유지되는 것 (HD-2D 플랜 v2에서)

- 쿼터뷰 카메라 각도 (X 29.2°)
- 따뜻한 색온도 (Color Grading 방향성)
- Tilt-shift + DOF + Bloom 적용 의도
- 3D 환경 메쉬 (단, 픽셀 텍스처 매핑 → 단색/Vertex color로 전환)

---

## Consequences

### Positive (긍정적)

1. **작업 시간 대폭 절감**
   - MagicaVoxel = Blender 모델링의 1/5 시간
   - 본 애니메이션 1세트 = 8방향 픽셀 애니 × 5보다 빠름
   - 1인 인디 스코프에 현실적
2. **카메라 자유** — Zoom, 회전, 동적 카메라 모두 가능
3. **확대 시 디테일** — DDworld의 zoom 기능과 시너지 (캐릭터 매력 보임)
4. **검증된 인디 트랙** — TT, Bad North, A Short Hike 등
5. **도구 단순화** — MagicaVoxel + Blender + Unity (3개 명확)
6. **AI 도구 활용 가능** — Voxel AI 생성기, Mixamo 등
7. **GPU Instancing 효율** — 정점색 1머티리얼로 대량 렌더 유리 (구체 경로·상한은 [ADR-003](ADR-003-rigid-instancing-crowd-rendering.md): 리지드 부위 인스턴싱 + 400명)

### Negative (부정적)

1. **"픽셀 아트" 정체성 손실** — 픽셀 미감 팬 일부 이탈 가능성 (단, 핵심 타겟은 비주얼보다 메카닉)
2. **art-bible / visual-polish 일부 작업 폐기** — 단, 코드 변경 0이라 매몰비용 적음
3. **본 리깅 학습 필요** — Blender 새 영역 (단, Mixamo로 우회 가능)
4. ~~**200명 + 본 애니메이션 성능 주의** — 풀배치 시 15ms 근접. 최적화 옵션 필수 (Animator Culling, GPU Skinning, LOD)~~ → **[ADR-003] 갱신**: 스킨드/GPU Skinning 가정 폐기. 리지드 부위 인스턴싱 + 정점색 1머티리얼 + 데이터 시뮬로 400명 대응 (스키닝 비용 자체가 없음)

### Mitigations (완화책)

| 위험 | 완화 방안 |
|------|---------|
| 픽셀 팬 이탈 | Voxel은 픽셀의 친척. 일부 호환 가능 |
| 본 리깅 학습 부담 | Mixamo 자동 리깅 (캐릭터 업로드 → 자동) |
| 성능 (대량 캐릭터) | **[ADR-003] 갱신**: 리지드 부위 인스턴싱 + 정점색 1머티리얼 + 데이터 시뮬 (스키닝 없음) → 400명 |
| 분리 부위 정렬 | MagicaVoxel World Mode pivot 정확히 설정 |

---

## Alternatives Considered

### Option A: HD-2D 유지 (기존 방향)
- ❌ 거부: 인디 작업 부담 큼, 인디 사례 부재

### Option B: 2D 픽셀 아트 (Stacklands식)
- ❌ 거부: 분대 단위 시각화 어려움, 카메라 워크 제약

### Option C: Voxel + A2 (본 없음)
- ⚠️ 일부 채택 가능: 200명 풀배치 시 fallback 옵션. 단 zoom 시 어색

### Option D: Low-poly Voxel 3D + B (본 사용) ⭐
- ✅ 채택: 위 장단점 분석 참조

### Option E: 풀 리얼리스틱 3D
- ❌ 거부: 1인 인디 스코프 폭증

---

## Implementation Notes

### 권장 워크플로우 (5병종 기준)

```
1주차: 캐릭터 모델링 (MagicaVoxel)
  - 5병종 부위별 분리 모델 (Head/Body/Arms/Legs/Weapon)
  - 약 5~8시간

2주차: 리깅 + 애니메이션 (Blender)
  - Mixamo 자동 리깅 (휴머노이드)
  - 또는 수동 본 추가 (voxel은 단순)
  - Idle/Walk/Attack/Death 각 1세트
  - 약 20~30시간 (Mixamo 활용 시 10시간)

3주차: Unity 통합
  - Animator Controller 셋업
  - URP Post-processing (Bloom/DOF/Tilt-Shift)
  - 머티리얼 + Vertex Color
  - 약 1주
```

### 성능 최적화 체크리스트 (대량 캐릭터)

> ⚠️ **[ADR-003]로 대체됨.** 아래 GPU Skinning 기반 체크리스트는 스킨드 메쉬 가정용이었음.
> 현재 경로(리지드 부위 인스턴싱 + 정점색 1머티리얼 + 데이터 시뮬)의 검증 항목은
> [ADR-003 §Validation Criteria](ADR-003-rigid-instancing-crowd-rendering.md)를 따른다.

(아래는 폐기된 스킨드 가정 — 참고용 보존)

- [ ] ~~**Animator.cullingMode = CullCompletely**~~
- [ ] ~~**GPU Skinning 활성화**~~
- [x] **Material에 Enable GPU Instancing** (여전히 유효 — 정점색 1머티리얼)
- [ ] ~~**본 수 최소화 (휴머노이드 30개 → 8~12개)**~~ (리지드는 부위당 단일 본)
- [x] **LOD 시스템** (먼/작은 유닛 디테일·애니 갱신 빈도 ↓ — 여전히 유효)
- [x] **Object Pool** (여전히 유효)
- [x] **Profiler로 16.6ms 예산 검증** (여전히 유효)

### 도구 학습 자료

- **MagicaVoxel**: 공식 사이트 + YouTube "MagicaVoxel basics" (1~2시간)
- **Blender 본 리깅**: Mixamo 또는 YouTube "Blender bone rigging tutorial" (3~5시간)
- **Unity Animator**: Unity Learn "Animator Controller basics" (1시간)

---

## Related Documents

### 내부
- [CLAUDE.md](../../CLAUDE.md) — Visual Style
- [design/gdd/game-concept.md](../../design/gdd/game-concept.md) — Visual Direction
- [design/art/art-bible.md](../../design/art/art-bible.md) — 비주얼 원칙
- [design/art/visual-polish/README.md](../../design/art/visual-polish/README.md) — 폴리시 가이드
- [docs/architecture/ADR-001-async-pvp.md](ADR-001-async-pvp.md) — 동일 세션 결정

### 폐기/Deprecated
- [design/art/visual-polish/hd2d-setup-plan.md](../../design/art/visual-polish/hd2d-setup-plan.md) — HD-2D 마스터 플랜 (참고용 보존)
- [design/art/hd2d/](../../design/art/hd2d/) — HD-2D 레퍼런스 (참고용 보존)

### 비교 분석
- [design/research/comparisons/tabletop-tavern...](../../design/research/comparisons/) — Tabletop Tavern (Low-poly 3D 인디 검증)
- [design/research/comparisons/despots-game.md](../../design/research/comparisons/despots-game.md) — 인디 벤치마크
- [design/research/comparisons/bazaar.md](../../design/research/comparisons/bazaar.md) — 빌드 서사 영감

### 관련 / 향후 ADR
- [ADR-003: 리지드 인스턴싱 & 대량 캐릭터 렌더/시뮬](ADR-003-rigid-instancing-crowd-rendering.md) (Accepted, 본 ADR 성능 가정 갱신)
- (향후) 매치 데이터 직렬화 포맷
- (향후) BaaS 선택 (Firebase 등)

---

## Revision History

| Date | Author | Change |
|------|--------|--------|
| 2026-05-25 | nathan | 초안 작성, Accepted |
