# Art Archive — HD-2D (폐기)

> **격리일**: 2026-06-29
> **이유**: 비주얼 스타일이 HD-2D → Low-poly Voxel 3D로 전환됨 ([ADR-002](../../../docs/architecture/ADR-002-visual-style-low-poly-3d.md)).

이 폴더는 **HD-2D 시절 자료의 보존고**입니다. 현재 방향(Voxel)과 무관하니 **작업 시 참조하지 마세요.** 역사·참고 목적으로만 보존.

## 내용물

- `hd2d/` — HD-2D 레퍼런스 (옥토패스 트래블러 분석, HD-2D 위키, Blender 자료)
- `visual-polish-hd2d-setup-plan.md` — 구 HD-2D 구현 마스터 플랜 (Phase 1~5, DEPRECATED)

## 현재 정본은 어디?

- **아트 방향(What)**: [art-bible.md](../art-bible.md) ← 정본
- **구현(How)**: [visual-polish/README.md](../visual-polish/README.md)
- **렌더/대량 규칙**: [ADR-003](../../../docs/architecture/ADR-003-rigid-instancing-crowd-rendering.md)

## 유지된 것 (아카이브 안 됨)

HD-2D에서 유래했지만 복셀에도 유효한 연출 — **DOF·Tilt-shift·Bloom·Color Grading·Vignette·쿼터뷰 카메라·따뜻한 톤** — 은 폐기되지 않고 art-bible §1·§2 + visual-polish/README 효과 테이블로 정식 편입되어 살아있습니다.
