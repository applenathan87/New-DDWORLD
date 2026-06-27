# Voxel Art & Pipeline — Research

> **목적**: voxel 아트·툴·워크플로우 학습 자료를 주제별로 갈무리하고, 함께 논의하며 개념을 습득하는 공간.
> **방식**: 각 아티클 = 요약(완료) → 전문 번역(TODO) → 토론 내재화 → 합의 원칙은 정식 문서로 승격.
> **관련**: [ADR-002](../../../docs/architecture/ADR-002-visual-style-low-poly-3d.md) · [blender-to-unity.md](../../../docs/pipeline/blender-to-unity.md) · [art-bible](../art-bible.md)

---

## 소스 큐 — @Voxels (Zach Soares) 시리즈

> ⚠️ 살짝 outdated — 툴 버전·엔진 워크플로우는 현재 기준 교차검증 필요.

| 아티클 | 요약 | 전문번역 | 주제 |
|---|---|---|---|
| Reducing the Greebles | ✅ [voxel-greebles.md](./voxel-greebles.md) | ⬜ | greeble 최소화 (모델링 원칙) |
| Voxelart Styles in Video Games | ✅ [voxel-styles.md](./voxel-styles.md) | ⬜ | **4대 스타일 분류 (백본)** |
| Blocky Voxelart | ✅ [blocky-voxelart.md](./blocky-voxelart.md) | ⬜ | Minecraft/Hytale 계열 |
| Converting Marching Cube | ✅ [magicavoxel-marching-cubes.md](./magicavoxel-marching-cubes.md) | ⬜ | 스무스 voxel (DDworld 반대, 참고용) |

전문 번역·정독 = [work-queue.md](../../../production/work-queue.md) 14번 · 유사 아티클 검색 = 15번.

---

## 핵심 백본 — voxel 4대 스타일

| 스타일 | 특징 | 예시 | DDworld 적합성 |
|---|---|---|---|
| **Vector Voxel** | 곡선도 90°만 | Crossy Road | ⭕ 깔끔·저폴리 |
| **Pixelated (Flat Shaded)** | 3D 픽셀아트, 하드섀도우 금물 | Fugl | △ 조명 제약 |
| **Blocky** | 큐브 천지, 텍스처드 큐브 | Minecraft, Hytale | ⭕ 관대 |
| **Greeble** | 곡선·고해상 들쭉날쭉 | C&C Tiberian Sun | ❌ 고폴리·렌더 난이도 |

→ **DDworld는 Vector ~ Blocky 사이**(ADR-002 Low-poly + Rigid + greeble 최소화). **스무스(MC)·Greeble 회피.** (정확한 포지션은 토론 필요)

---

## 주제 맵 (확장 예정)

- **스타일/모델링**: voxel-styles · blocky-voxelart · voxel-greebles
- **툴 기법**: magicavoxel-marching-cubes (참고용)
- **예정(검색)**: 유사 아티클 — 디자인 방식, Blender↔Unity 워크플로우 (work-queue 15)

## 작업 방식

1. 요약 갈무리 (완료분)
2. 토론으로 DDworld 적용점 합의
3. 합의 원칙 → art-bible / blender-to-unity 로 승격
4. 전문 번역·정독 = work-queue 14
