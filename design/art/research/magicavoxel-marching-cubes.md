# MagicaVoxel Marching Cube 변환 (갈무리)

> **출처**: Zach Soares (Voxels), "Converting MagicaVoxel's Marching Cube for your game" — https://medium.com/@Voxels/converting-magicavoxels-marching-cube-for-your-game-799ce993721a
> **갈무리**: 2026-06-27 · 요약 (전문 번역 TODO) · 살짝 outdated
> **관련**: [voxel-styles.md](./voxel-styles.md) · [blender-to-unity.md](../../../docs/pipeline/blender-to-unity.md)

## Marching Cube(MC)란

MagicaVoxel의 MC export = **매끈·둥근** voxel 메시 (블로키와 정반대). 게임에 넣으려면 변환 필요.

## 왜 변환?

- 정점색 직접 사용은 게임에 비효율 → **정점색을 텍스처로 베이크**.
- 원본 MC는 "폴리곤 수가 극도로 높아 사실상 쓸모없음" → 최적화 필수.

## 파이프라인 (툴 4개)

1. **MagicaVoxel** → `.mc` export
2. **Meshlab** → `.mc` → `.DAE` (정점색 보존)
3. **Maya** 등 → `.DAE` import, 색 확인(Toggle Display Colors), `.FBX` export
4. **Simplygon** → 최적화 + 색을 텍스처로 베이크

## 핵심 설정 / 함정

- Simplygon: "Physically Based" 모드 + "256×256" 텍스처
- Reduction + Material Baking 둘 다 ON
- Reduction 공격적 (저자 30%)
- ⚠️ **"Bake Vertex Colors" 켜기** — 안 켜면 정점색이 텍스처로 안 나옴

## DDworld 적용 — ⚠️ 우리 방향과 반대

- MC = **스무스 voxel** = ADR-002의 **Low-poly 블로키 방향과 정반대** + 고폴리(군대 규모 성능과 충돌).
- 파이프라인도 무거움 (Meshlab/Maya/Simplygon, 유료 툴 포함).
- → **DDworld 채택 안 함.** "이런 길도 있다 + 왜 안 가는지"의 참고용으로만 보관.
