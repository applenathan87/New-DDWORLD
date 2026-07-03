# ADR-003: 대규모 캐릭터 렌더링 & 전투 시뮬레이션 — 리지드 부위 인스턴싱 + 데이터 기반 결정론 시뮬

## Status

**Superseded** (2026-07-03 — 「마왕성 인사팀」 전환. 대량 캐릭터 전투/크라우드 렌더링 자체가 폐기됨: 면접 게임엔 400명 전투가 없다. 현행 컨셉 = ideation/, 아카이브 인덱스 = design/gdd/_archive/README.md)

## Date

2026-06-29

## Last Verified

2026-06-29

## Decision Makers

nathan

## Summary

DDworld는 "귀여운 토탈워" 느낌을 위해 한 전투에 다수 병사(목표 상한 양측 합 400, 튜닝 노브)를 동시에 출연시켜야 한다. 본 ADR은 그 다수 캐릭터를 **리지드 복셀 부위의 GPU 인스턴싱(정점색 1머티리얼)** 으로 렌더하고, 병사를 **GameObject가 아닌 배열 데이터로 결정론 고정 틱 시뮬**하며, **시뮬과 렌더를 분리**하기로 결정한다. 스킨드 메쉬·VAT(애니 텍스처 베이크)는 채택하지 않는다.

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | Unity 6.3 LTS |
| **Domain** | Rendering / Animation / Core |
| **Knowledge Risk** | HIGH — post-cutoff, must verify |
| **References Consulted** | `docs/engine-reference/unity/VERSION.md`, [Total War 리서치 노트](#related), GPUOpen "Anatomy of the Total War Engine" |
| **Post-Cutoff APIs Used** | `Graphics.RenderMeshInstanced` / `Graphics.RenderMeshIndirect`, `BatchRendererGroup`, GPU Resident Drawer (URP), per-instance properties (MaterialPropertyBlock 대체 경로) |
| **Verification Required** | ① 400 인스턴스 × 5부위가 드로우콜 한 줌으로 묶이는지 Frame Debugger 확인 ② 정점색이 인스턴싱 경로에서 정상 출력되는지 ③ per-instance 팀컬러 틴트가 배칭 깨뜨리지 않는지 ④ Unity 6 GPU Resident Drawer 자동 인스턴싱 적용 여부 |

> **Note**: Knowledge Risk가 HIGH이므로, 엔진 버전 업그레이드 또는 위 API 동작 검증 실패 시 본 ADR을 재검증하고 필요 시 Superseded 처리한다.

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | ADR-001 (비동기 PvP — 결정론 시뮬 요구의 출처), ADR-002 (Low-poly Voxel 3D — 리지드 복셀 + 정점색의 출처) |
| **Enables** | 향후 ADR: 매치 데이터 직렬화 포맷(병사 데이터 배열 = 직렬화 단위) |
| **Blocks** | work-queue 13 (첫 Voxel 캐릭터 제작) — 본 ADR의 리깅/머티리얼 규칙 위에서 진행 |
| **Ordering Note** | 본 ADR은 ADR-002의 성능 가정(GPU Skinning + 200명, [ADR-002 §Consequences·Implementation Notes](ADR-002-visual-style-low-poly-3d.md))을 **갱신(refine)** 한다. 비주얼 스타일 결정 자체는 유효하며 superseded 아님. ADR-002의 "GPU Skinning 체크리스트"는 본 ADR의 리지드 인스턴싱 경로로 대체된다. |

## Context

### Problem Statement

오토배틀러로서 "전쟁 느낌"을 내려면 한 전투에 가능한 많은 병사가 동시에 보여야 한다(사용자 명시 목표: "최대한 많이"). 동시에 이는 비동기 PvP(ADR-001)의 **결정론·직렬화 절대 요구**와 충돌 없이 굴러가야 한다. 캐릭터 렌더 경로와 전투 시뮬 구조는 한 번 정하면 되돌리기 비싼 결정이므로(아트 파이프라인·전투 코드 양쪽을 묶음), 첫 Voxel 캐릭터 제작(work-queue 13) 전에 확정이 필요하다.

### Current State

- ADR-002에서 Low-poly Voxel 3D + Blender 본 애니(B 방식)를 채택하되, 성능 대응은 **"200명 + GPU Skinning + Animator Culling + LOD"** 로 가정해 두었다(스킨드 메쉬 전제).
- 그 가정은 토탈워 사례 리서치 이전 기준이며, "다수 병력 + 솔로 인디 + 결정론"의 조합에 최적이 아님이 드러났다.

### Constraints

- **엔진**: Unity 6.3 LTS, URP, MonoBehaviour 기반 (DOTS 사용 안 함 — technical-preferences).
- **결정론**: 같은 MatchData → 어떤 클라이언트에서도 같은 결과 (ADR-001, CLAUDE.md Multiplayer-Ready 원칙).
- **리소스**: 1인 인디. 무거운 베이킹 툴체인·복잡한 경로탐색 유지비를 감당하기 어렵다.
- **아트**: 리지드 복셀(부위 분리, 메쉬 변형 금지) + 정점색 (ADR-002, project_rigid_voxel_animation).
- **성능 예산**: 60 FPS / 16.6ms, Draw Calls < 200, Memory < 2GB (technical-preferences).

### Requirements

- 한 전투에서 동시 출연 캐릭터 **목표 상한 400(양측 합)** 을 60 FPS로 렌더. (튜닝 노브 — 더 상향 가능)
- 모든 출연 병사는 **개별 시뮬 엔티티**(위치·체력·전투 상태 보유). 시각적 가짜 채우기(visual padding) 없음.
- 시뮬은 결정론 고정 틱. 렌더는 그와 분리되어 60 FPS로 보간.
- 애니메이션 반복(iteration)이 자유로울 것 — 스쿼시·타이밍을 언제든 수정 가능(Rigid Voxel 원칙).

## Decision

다섯 개의 기둥으로 결정한다.

1. **렌더 = 리지드 복셀 부위의 GPU 인스턴싱.** 캐릭터를 Head/Body/Arms/Legs/Weapon 등 딱딱한 부위로 두고, 부위 메쉬별로 `RenderMeshInstanced`/`RenderMeshIndirect`로 다수 인스턴스를 한 번에 그린다. (400명 × ~5부위 = ~2,000 인스턴스 → 드로우콜 한 줌)
2. **정점색 1머티리얼.** 색은 텍스처가 아니라 정점에 박는다(MagicaVoxel 천연 출력). 모든 캐릭터가 정점색 출력용 URP 셰이더 **머티리얼 1개를 공유** → 인스턴싱(배칭)의 전제. 팀 구분 등 변형은 **부위 메쉬 정점색** 또는 **per-instance 컬러 틴트**로 처리(머티리얼은 여전히 1개).
3. **애니 = 부위 변환 데이터 (VAT 안 씀).** 리지드라 메쉬 변형 계산이 없으므로, 클립은 부위의 로컬 transform(회전/위치/`localScale` 스쿼시) 키프레임 데이터로 들고 런타임에 샘플링한다. 동작이 텍스처로 박제되지 않아 **반복 수정이 자유롭다.**
4. **병사 = 배열 데이터 엔티티 (GameObject 아님).** 병사 상태를 POCO 배열로 두고 중앙 매니저가 일괄 처리한다. 씬에 400 MonoBehaviour를 두지 않는다. 이 배열이 곧 직렬화/고스트 데이터 단위. **부대(분대)는 의사결정 층, 병사는 시뮬/렌더 대상**(토탈워 2층 구조 차용).
5. **시뮬/렌더 분리.** 결정론 고정 틱(예 20~30Hz)으로 시뮬이 "미래"를 만들고, 렌더는 틱 사이를 보간해 "현재"를 60 FPS로 그린다.

### Architecture

```
[입력/매치 데이터]
        │
        ▼
┌─────────────────────────────┐      고정 틱(20~30Hz), 시드 RNG
│  Simulation Layer (결정론)   │  ← 병사 = POCO 배열 (위치/체력/상태)
│  - 부대(분대) 의사결정 층     │     부대가 의도 → 병사가 로컬 해결
│  - 병사 일괄 틱 (매니저)      │     공유 플로우필드 + 가벼운 분리(separation)
└─────────────┬───────────────┘
              │ 상태 스냅샷 (직렬화 가능 = 고스트 데이터)
              ▼
┌─────────────────────────────┐      매 프레임 60 FPS
│  Render Layer (분리)         │  ← 틱 사이 보간(interpolate)
│  - 부위별 transform 계산      │     클립 = 부위 로컬 변환 데이터 샘플링
│  - 부위 메쉬별 인스턴싱 배치  │     RenderMeshInstanced/Indirect
│  - 정점색 1머티리얼 + 팀틴트  │     리지드 → 도장처럼 한 번에 N개
└─────────────────────────────┘
```

### Key Interfaces

```csharp
// 시뮬 = 순수 데이터 (직렬화/결정론 단위). MonoBehaviour 아님.
struct SoldierState {
    int   id;
    Fixed posX, posY;     // 결정론을 위해 고정소수점 권장(또는 엄격한 float 순서)
    int   hp;
    byte  team;
    byte  clipId;         // 현재 재생 클립(어택/런 등)
    Fixed clipTime;       // 클립 진행 시간
    int   squadId;        // 소속 부대(의사결정 층)
}

// 렌더 = 시뮬 상태를 읽어 부위 변환을 만들고 인스턴싱
//  prev/cur 틱 상태를 alpha로 보간 → 부드러운 60FPS
//  부위 메쉬당 Matrix4x4[] + per-instance color 배열을 채워 한 번에 draw
```

### Implementation Guidelines

- **Blender**: 부위를 본에 **리지드 바인드(웨이트 1.0, 단일 본, 페인팅 없음)**. 어택/런/idle/death 클립 키프레임. 낮은 프레임(12~24fps 스텝).
- **머티리얼**: 정점색 출력 URP 셰이더 **1개만** 프로젝트에 두고 전 캐릭터 공유. 캐릭터별 텍스처/머티리얼 생성 **금지**.
- **시뮬**: 병사 로직을 `Update()` 단위 MonoBehaviour로 흩지 말 것. 중앙 매니저가 고정 틱에서 배열을 일괄 처리. RNG는 시드 기반.
- **경로/이동**: 분대 단위로 1회 경로 산출 → 공유 플로우필드 + 가벼운 분리. HPA*/RVO/연속체 크라우드는 도입하지 않음(오버킬).
- **렌더**: 시뮬 틱과 프레임을 분리, 보간 alpha 적용. Frame Debugger로 인스턴싱 묶임 검증.
- **풀링**: 시각 오브젝트/버퍼는 재사용. 매 프레임 할당 금지.

## Alternatives Considered

### Alternative 1: 스킨드 메쉬 + Animator (캐릭터당 GameObject)

- **Description**: 표준 방식. 부위를 부드러운 스킨으로 본에 웨이트 페인팅, 캐릭터마다 GameObject + Animator로 클립 재생.
- **Pros**: 가장 직관적·표준, 부드러운 변형, 반복 자유.
- **Cons**: 캐릭터마다 본 행렬 매 프레임 GPU 업로드 + 배칭 잘 안 됨 → 드로우콜·스키닝 비용 폭증. 수십 명 넘어가면 성능 벽. 400 MonoBehaviour는 CPU 과부하.
- **Estimated Effort**: 낮음(초기) / 높음(스케일 대응 재작업).
- **Rejection Reason**: "다수 병력" 요구와 정면충돌. (단 우리는 리지드라 이 방식의 부드러운 변형 자체가 불필요.)

### Alternative 2: VAT (Vertex Animation Texture) 베이킹

- **Description**: 애니의 모든 프레임 정점 위치를 텍스처에 미리 구워, 런타임엔 텍스처 룩업으로 재생 + 인스턴싱.
- **Pros**: 스킨드 크라우드를 대량 렌더 가능.
- **Cons**: ① 베이킹 툴체인 유지비(솔로 인디 부담) ② **동작이 박제됨 — 스쿼시·타이밍 수정 시 재베이킹** → MVP 반복 단계와 상극 ③ 2019년경 기법, Unity 6 네이티브 인스턴싱과 중복 ④ **리지드인 우리에겐 스키닝이 없어 애초에 불필요.**
- **Estimated Effort**: 높음(툴·메모리·재베이킹 루프).
- **Rejection Reason**: 우리 규모(수백)에서 얻을 게 없고 유연성을 잃음. VAT는 "수천+ 도달 & CPU 변환 계산이 측정상 병목일 때"만 재검토하는 **비상구**로만 남긴다.

### Alternative 3: 리지드 부위 인스턴싱 + 데이터 시뮬 ⭐ (채택)

- **Description**: 본 ADR Decision.
- **Pros**: 대량(인스턴싱) + 유연(변환 데이터, 박제 안 됨) **동시 달성**. 리지드·정점색 아트 스타일과 정합. 결정론·직렬화 요구와 한 방향(병사 데이터 = 고스트 데이터). 토탈워의 무거운 기계장치(스키닝·HPA*·DX12 배리어) 회피.
- **Cons**: 부위 인스턴싱·보간·데이터 시뮬을 직접 구성하는 초기 설계 비용. Unity 6 인스턴싱 API 검증 필요(Knowledge Risk HIGH).
- **Estimated Effort**: 중간.
- **Rejection Reason**: 채택.

## Consequences

### Positive

- 400(이상)명을 드로우콜 한 줌으로 60 FPS 렌더 — DOTS·VAT 없이.
- 애니 반복이 끝까지 자유(스쿼시·타이밍 수정 자유) → Rigid Voxel "맛" 탐색에 유리.
- 병사 데이터 배열 = 결정론 시뮬 + 직렬화(고스트) + 성능이 **한 구조로 정렬**.
- Blender 리깅이 셋 중 가장 단순(웨이트 페인팅 불필요).

### Negative

- 부위 인스턴싱 + 보간 + 데이터 기반 시뮬을 처음부터 설계해야 함(평범한 GameObject+Animator보다 초기 손이 더 감).
- Unity 6 인스턴싱/정점색/per-instance 틴트 경로를 실제로 검증해야 함(미검증 시 리스크).
- 부드러운 메쉬 변형이 필요한 표현은 불가(리지드 한계) — 단 이는 ADR-002에서 이미 수용한 스타일.

### Neutral

- ADR-002의 성능 대응 항목(GPU Skinning 등)이 본 ADR 경로로 대체됨 → ADR-002 해당 절 갱신 필요(아래 Migration).

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| Unity 6 인스턴싱 경로에서 정점색/팀틴트가 배칭을 깸 | 중 | 높음 | 첫 캐릭터 단계에서 Frame Debugger 검증(스파이크) 후 본격 진행 |
| per-instance 색 다양성이 인스턴싱 효율 저하 | 중 | 중 | 색은 정점색 또는 단일 per-instance 프로퍼티로 한정, 머티리얼 분기 금지 |
| 결정론 깨짐(float 비결정성) | 중 | 높음(고스트 재현 실패) | 고정소수점 또는 엄격한 연산 순서 + 시드 RNG, 시뮬에서 Unity 물리/Time.deltaTime 배제 |
| 400 데이터 시뮬이 단일 스레드에서 무거움 | 낮 | 중 | 배열 일괄 처리, 부대 단위 경로 1회, 필요 시 잡(job) 분산 |
| 수천+로 상향 시 CPU 변환 계산 병목 | 낮 | 중 | 그 시점에 VAT/컴퓨트 셰이더 비상구 재검토(트리거: 프로파일링) |

## Performance Implications

| Metric | Before (ADR-002 가정) | Expected After | Budget |
|--------|--------|---------------|--------|
| 동시 캐릭터 | 200 (GPU Skinning) | 400 (튜닝 노브, 상향 여지) | — |
| Draw Calls | 유닛 수 비례 위험 | ~부위 수(약 5~수십) | < 200 |
| CPU (frame time) | 스키닝+Animator 부담 | 데이터 일괄 틱 + 보간 | 16.6ms |
| Memory | 캐릭터별 텍스처 가능성 | 정점색(텍스처 0) + 작은 클립 데이터 | < 2GB |

> 구체 수치는 첫 캐릭터 프로파일링으로 확정(Validation 참조). 위는 설계 목표.

## Migration Plan

기존 코드가 없으므로 코드 마이그레이션은 없다. 문서 정합만 맞춘다.

1. **ADR-002 갱신** — §Consequences "GPU Instancing 200명"·§Implementation Notes "GPU Skinning 체크리스트"를 본 ADR 경로(리지드 인스턴싱·VAT 배제·400)로 갱신하고 "Refined by ADR-003" 표기. *(별도 승인 후 진행)*
2. **technical-preferences / art-bible** — 정점색 1머티리얼·리지드 바인드·시뮬 분리 원칙을 규칙으로 반영.
3. **work-queue 13** — 첫 Voxel 캐릭터 제작을 본 ADR 규칙 위에서 시작.

**Rollback plan**: 인스턴싱 검증이 실패하면, 동일 아트(리지드·정점색)를 유지한 채 Alternative 1(스킨드/GameObject)로 후퇴해 소규모(수십)로 스코프 축소. 데이터 시뮬 층은 그대로 재사용 가능.

## Validation Criteria

- [ ] 400 인스턴스 × 5부위가 드로우콜 한 줌으로 묶임 (Frame Debugger)
- [ ] 정점색 + per-instance 팀틴트가 인스턴싱 경로에서 정상 출력
- [ ] 400명 전투가 60 FPS / 16.6ms 예산 내 (Profiler)
- [ ] 같은 MatchData(시드 포함) → 두 번 실행 시 병사 최종 상태 비트 동일(결정론)
- [ ] 병사 상태 배열이 직렬화/역직렬화로 라운드트립(고스트 데이터)
- [ ] 시뮬 틱과 렌더 프레임 분리 + 보간으로 시각적 부드러움 확인

## GDD Requirements Addressed

| GDD Document | System | Requirement | How This ADR Satisfies It |
|-------------|--------|-------------|--------------------------|
| `design/gdd/combat.md` | Combat | 양측 배치 장수가 동시에 전장에 출연하여 자동 전투("보는 맛" Pillar) | 리지드 부위 인스턴싱으로 다수(목표 400) 동시 렌더, 데이터 시뮬로 개별 전투 처리 |
| ADR-001 / CLAUDE.md | Networking | 같은 MatchData → 어떤 클라이언트에서도 같은 결과(비동기 고스트 재현) | 결정론 고정 틱 + 시드 RNG + 병사 데이터 배열 = 직렬화 가능한 시뮬 상태 |
| ADR-002 / project_rigid_voxel | Art/Animation | 리지드 복셀, 메쉬 변형 금지, Scale 스쿼시로 생동감 | 애니를 부위 변환 데이터로 두어 스쿼시/타이밍 유지 + 인스턴싱 |

> 추가로, 본 ADR은 "매치 데이터 직렬화 포맷" 향후 ADR의 입력(병사 데이터 구조)을 사실상 정의하는 **부분 기반(foundational)** 성격도 가진다.

## Related

- [ADR-001: 비동기 PvP](ADR-001-async-pvp.md) — 결정론·직렬화 요구의 출처 (Depends On)
- [ADR-002: Low-poly Voxel 3D](ADR-002-visual-style-low-poly-3d.md) — 리지드 복셀·정점색의 출처. **본 ADR이 성능 가정을 갱신함(Refined by ADR-003).**
- [project_rigid_voxel_animation] — 리지드 애니 원칙 (메모리)
- 리서치: 토탈워 엔진 구조 — GPUOpen "Anatomy of the Total War Engine" I~III, Game Developer "Designing Total War: Warhammer II", TWCenter "Basics of Battle AI and Pathfinding". (인스턴싱·sim/render 분리·2층 시뮬 구조의 근거)
- 코드: (구현 후 링크) `src/` 렌더·시뮬 레이어

## Revision History

| Date | Author | Change |
|------|--------|--------|
| 2026-06-29 | nathan | 초안 작성, Accepted |
