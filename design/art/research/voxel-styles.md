# Voxel Art Styles in Games (갈무리)

> **출처**: Zach Soares (Voxels), "Voxelart Styles in Video Games" — https://medium.com/@Voxels/voxelart-styles-in-video-games-310566d3b83d
> **갈무리**: 2026-06-27 · 요약 (전문 번역 TODO) · 살짝 outdated
> **관련**: [voxel-greebles.md](./voxel-greebles.md) · [blocky-voxelart.md](./blocky-voxelart.md) · [ADR-002](../../../docs/architecture/ADR-002-visual-style-low-poly-3d.md)

## 4대 voxel 스타일

| 스타일 | 정의 | 예시 | 트레이드오프 |
|---|---|---|---|
| **Vector Voxel** | 곡선 표현도 90° 각도만 사용 | Crossy Road | 깔끔·다양한 스케일에서 가독, 저사양(모바일)용. 단 평면적·단순 |
| **Pixelated (Flat Shaded)** | 3D 공간에 픽셀아트 재현. 플랫 셰이딩 필수 | Fugl, Voxelnauts | "가장 순수한 voxel". 선명한 색, 단 **하드 섀도우가 룩을 망침** |
| **Blocky** | 세계·캐릭터가 다 큐브. 보통 텍스처드 폴리곤 큐브 | Minecraft, Hytale | 기술적으로 관대, 단 여전히 아트 실력 요구 |
| **Greeble** | 곡선 수용·vector 규칙 무시. 고해상 들쭉날쭉 | C&C: Tiberian Sun, Critical Annihilation | 렌더 최난·고폴리. PBR/SSAO 덕 보지만 엔진 부담 |

## 애니메이션

3가지 기법(**리지드 / 소프트바디 / 프레임**)이 있고, 각각 파이프라인 난이도·비주얼 트레이드오프가 달라 최종 스타일 선택에 영향. (DDworld = 리지드 확정)

## DDworld 적용

- ADR-002 = Low-poly Voxel + Rigid 애니 + greeble 최소화 → **Vector ~ Blocky 사이**가 우리 자리.
- **Greeble 스타일·스무스(MC) 회피** (고폴리 = 군대 규모 성능과 충돌).
- ⚠️ Pixelated 스타일의 "**하드섀도우 금물**"은 art-bible의 소프트섀도우+DOF 방향과 맞물림 — 우리가 그쪽 색감이면 조명 절제 주의.
- 정확한 포지션(Vector vs Blocky vs 혼합)은 **토론 주제**.
