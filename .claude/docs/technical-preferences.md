# Technical Preferences

<!-- DDworld 프로젝트 설정. 에이전트들이 이 파일을 참조합니다. -->

## Engine & Language

- **Engine**: Unity **6000.5.1f1** (Unity 6.5 — 2026-07-02 실측, 프로토타입·Origin 동일)
- **Language**: C#
- **Rendering**: URP (Universal Render Pipeline) — 쿼터뷰 3D
- **Physics**: Unity Physics (기본)

## Input & Platform

- **Target Platforms**: PC (Steam)
- **Input Methods**: Keyboard/Mouse
- **Primary Input**: Mouse (카드 드래그 & 그리드 배치)
- **Gamepad Support**: Partial (추후 고려)
- **Touch Support**: None
- **Platform Notes**: Steam 출시 대상. 16:9 / 16:10 가로 모드. 최소 해상도 1280x720.

## Naming Conventions

- **Classes**: PascalCase (`BattleManager`, `UnitData`)
- **Variables**: camelCase (`soldierCount`, `currentRound`)
- **Signals/Events**: PascalCase with On prefix (`OnBattleStarted`, `OnRoundEnded`)
- **Files**: PascalCase matching class name (`GameManager.cs`, `Deck.cs`)
- **Scenes/Prefabs**: PascalCase (`BattleGrid.prefab`, `CardSlot.prefab`)
- **Constants**: UPPER_SNAKE_CASE (`MAX_HAND_SIZE`, `GRID_SIZE`)

## Performance Budgets

- **Target Framerate**: 60 FPS
- **Frame Budget**: 16.6ms
- **Draw Calls**: < 200 (쿼터뷰 2.5D, 유닛 수 제한적)
- **Memory Ceiling**: 2GB

## Testing

- **Framework**: Unity Test Framework (NUnit)
- **Minimum Coverage**: 밸런스 공식, 전투 로직, 덱 시스템은 반드시 테스트
- **Required Tests**: RPS 상성 공식, 틱 기반 전투, 루트 계산, 덱 셔플/드로우

## Forbidden Patterns

- 싱글턴 남용 — DI(Dependency Injection) 우선
- 하드코딩된 밸런스 값 — ScriptableObject 또는 외부 데이터로 관리
- Update()에서 매 프레임 할당 — 오브젝트 풀링 사용

## Allowed Libraries / Addons

- DOTween (애니메이션)
- TextMeshPro (텍스트 렌더링)
- Addressables (에셋 관리)
- UI Toolkit (런타임 UI)

## Architecture Decisions Log

> ⚠️ **2026-07-03 「마왕성 인사팀」 전환**: ADR-001·003 Superseded. ADR-002는 캐릭터 복셀 결정만 유효(환경=로우폴리 3D). 인덱스 = design/gdd/_archive/README.md

- [ADR-001](../../docs/architecture/ADR-001-async-pvp.md) — 비동기 PvP (2026-05-25) → **Superseded** (PvP 자체 폐기)
- [ADR-002](../../docs/architecture/ADR-002-visual-style-low-poly-3d.md) — 비주얼 스타일 Low-poly Voxel (2026-05-25, Accepted) · ⚠️ **캐릭터 복셀만 유효** — 환경=로우폴리 3D, 미니어처 톤·400명 크라우드 전제는 폐기
- [ADR-003](../../docs/architecture/ADR-003-rigid-instancing-crowd-rendering.md) — 400명 크라우드 렌더링/전투 시뮬 (2026-06-29) → **Superseded** (전투 자체 폐기)

## Engine Specialists

- **Primary**: `unity-specialist`
- **Language/Code Specialist**: (C# — Unity 내장)
- **Shader Specialist**: `unity-shader-specialist`
- **UI Specialist**: `unity-ui-specialist`
- **Additional Specialists**: `unity-addressables-specialist`
- **Routing Notes**: DOTS는 사용하지 않음 (MonoBehaviour 기반)

### File Extension Routing

| File Extension / Type | Specialist to Spawn |
|-----------------------|---------------------|
| `.cs` (게임 코드) | `unity-specialist` |
| `.shader`, `.shadergraph` | `unity-shader-specialist` |
| `.uxml`, `.uss` (UI Toolkit) | `unity-ui-specialist` |
| `.prefab`, `.unity` (씬/프리팹) | `unity-specialist` |
| `.asmdef` (어셈블리 정의) | `unity-specialist` |
| General architecture review | Primary |
