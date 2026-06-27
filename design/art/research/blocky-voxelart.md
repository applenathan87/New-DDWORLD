# Blocky Voxelart (갈무리)

> **출처**: Zach Soares (Voxels), "Blocky Voxelart: a Brief" — https://medium.com/@Voxels/blocky-voxelart-a-brief-11d3ef9f9724
> **갈무리**: 2026-06-27 · 요약 (전문 번역 TODO) · 살짝 outdated
> **관련**: [voxel-styles.md](./voxel-styles.md) · [voxel-greebles.md](./voxel-greebles.md)

## 블로키 스타일이란

큐브 형태 + 단순화된 실루엣. **형태 복잡성 대신 텍스처 디테일**로 승부. 전통 voxel의 볼류메트릭 제약과 달리 단순성·접근성(플레이어·모더) 우선.

## 핵심 원칙

1. **큐브로 추상화** — 형태를 본질로 환원 (고양이 = 삼각 귀 + 꼬리).
2. **텍스처 주도 복잡성** — 기하가 아니라 회화적 표면으로 디자인을 전달.
3. **레이어링** — 평면 텍스처 너머 폴리곤 기법으로 깊이 (Hytale 머리카락).
4. **단순성 = 기능** — 큐브가 메인 도구. 메카닉 제공 + 접근성 유지.

## 주의

- 평평하게 "붙인 듯한" 디테일 회피 → 레이어로 입체감.
- 정교한 형태보다 **명료한 실루엣** 우선.

## 도구

- **Blockbench** — 블로키/텍스처드 큐브 제작 도구 (MagicaVoxel과 다른 계열).

## DDworld 적용

- ⚠️ **중요한 갈림길**: 블로키는 **텍스처드 폴리곤 큐브**(Blockbench)인데, 우리 ADR-002/파이프라인은 **MagicaVoxel 정점색(vertex color) voxel**. → "블로키 = 텍스처 주도"를 그대로 따르면 텍스처 워크플로우가 추가됨.
- 우리 방향(정점색 + greeble 최소화)은 오히려 **Vector Voxel에 더 가까울 수** 있음 → [voxel-styles.md](./voxel-styles.md) 포지션 토론과 직결.
