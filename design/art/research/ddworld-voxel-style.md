# DDworld Voxel 스타일 — 포지션 & 디테일 전략 (논의 정리)

> **상태**: 논의 정리 (디테일 전략 일부 미확정) · 2026-06-29
> **성격**: 토론 결론 캡처 — 확정되면 art-bible/ADR로 승격
> **관련**: [voxel-styles.md](./voxel-styles.md) · [voxel-greebles.md](./voxel-greebles.md) · [ADR-002](../../../docs/architecture/ADR-002-visual-style-low-poly-3d.md) · [art-bible §1](../art-bible.md)

## 1. 스타일 포지션 (방향 확정)

4대 스타일(Vector / Pixelated / Blocky / Greeble) 중 DDworld =
- ✅ **정점색(vertex-color) voxel, 텍스처 없음** → "Blocky(텍스처드 큐브, Blockbench)"가 아니라 **Vector Voxel 계열**.
- 근거: blender-to-unity.md 파이프라인이 vertex color + GPU Instancing 기반. art-bible §1에 이미 명시됨.
- 회피: 스무스(Marching Cube, 고폴리·방향 반대) / Greeble 스타일(고폴리·노이즈).

## 2. 레퍼런스 (히어로 타깃)

사용자 제시 이미지 = 뿔투구 치비 전사, MagicaVoxel 히어로 렌더(림라이트+블룸+AO, 어두운 배경).
- **정점색 voxel 확인**(텍스처 X) → 방향 일치.
- 단 해상도 **~24~32 voxel + 체커 무늬** = art-bible 현 캐릭터 스펙(**8~16 voxel, ~500폴리, 2~3색**)보다 **높음/화려함**.
- ⚠️ 이 렌더의 화려함 절반은 MagicaVoxel 패스트레이서 → Unity URP는 재현 셋업 필요(에디터≠출시).
- 이미지 원본은 `design/art/references/`(이미지용 gitignore)에 보관 권장.

## 3. 디테일 전략 (leaning, 미확정)

질문: "전 유닛을 이 디테일로?" → **하드한 불가 사유 없음.** 단 비용(중요도순):
1. **ROI** — 전장 줌에선 잔디테일이 거의 안 보임 (close-up에서만 빛남).
2. **가독성** — 체커 등 잔무늬가 작아지면 노이즈 → 보드 읽기(심리전 핵심) 방해 + "대비로 시선 유도" 원칙 붕괴.
3. **성능** — 인스턴싱/LOD로 풀리나 공짜 아님. M5가 약한 PC 문제를 가림.

→ **방향(leaning)**: 품질 등급이 아니라 **"보는 거리에 맞춘 디테일"**.
- 스타일은 전 유닛 통일(정점색 voxel).
- **잡병 = art-bible 현 스펙(청키, 큰 색면, 잔무늬 절제)** → 거리에서 읽힘.
- **히어로/특수/클로즈업 = 레퍼런스 수준(고해상·체커 OK)**.
- LOD(멀면 단순 메시) + GPU Instancing 전제.
- 성능 검증은 **min-spec PC 기준**(M5 아님).

## 4. 미해결 (확정 필요)

- [ ] "거리 기반 디테일" 전략을 art-bible에 정식 반영? (현 8~16 voxel 스펙 = 잡병 티어로 두고, 히어로 티어 추가)
- [ ] 히어로 티어 voxel/폴리 예산 수치
- [ ] 티어 개수 (잡병/히어로 2티어? 그 이상?)
- [ ] 성능 한계선 = work-queue 13번(Track 성격) 실측 후 확정
