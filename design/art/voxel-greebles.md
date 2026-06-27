# Voxel Art — Greebles 줄이기 (갈무리)

> **출처**: Zach Soares (Voxels), "Voxel Art: Reducing the Greebles" — https://medium.com/@Voxels/voxel-art-reducing-the-greebles-263cef16b12d
> **갈무리**: 2026-06-27
> **관련**: [ADR-002](../../docs/architecture/ADR-002-visual-style-low-poly-3d.md) (Low-poly Voxel 채택) · [blender-to-unity.md](../../docs/pipeline/blender-to-unity.md) · art-bible §1 (Rigid Voxel Animation)

## Greeble이란

voxel 표면의 불필요한 "돌기/요철". 작은 기하 변화가 누적돼 **멀리서 보면 지저분하게 울퉁불퉁**해 보이는 것.

## 왜 문제인가

- **성능**: voxel 1개 = 12 폴리곤. 비큐브 캐릭터는 쉽게 100+ 폴리곤 → 폴리 폭증, 하드웨어 부담.
- **가독성**: 인게임 조명과 만나면 greeble이 혼란스러운 그림자를 만들어 **의도한 디테일을 가림**.

## 줄이는 기법 4가지

1. **스테핑 최소화** — 45° 비스듬한 voxel 배열을 피한다 (불필요한 표면 변화 유발).
2. **팔레트 절제** — 요소당 베이스 1색 + 하이라이트 1색. 시각적 노이즈 감소.
3. **메쉬 전략적 분리** — 팔다리/부위를 나눠 리깅·애니가 깔끔해짐 (= DDworld 리지드 분절과 동일).
4. **평면성 수용** — 최대한 단순하게 모델링하고, **디테일은 애니메이션으로 전달**.

## 핵심 원칙 (인용)

> "그림자 색 그라데이션을 확실하게! 주저 말고, 색조(hue)와 밝기 **둘 다** 대비를 줘라."

## 기타 실전 팁

- 만들기 전에 머릿속으로 최종 형태부터 역설계.
- 표면 복잡함보다 **실루엣/형태 명료함 우선**.
- 부위 크기를 다양하게 → 애니메이션 중 클리핑(파고듦) 최소화.
- 에디터에서는 미완성처럼 보이는 게 정상 — **진짜 완성은 인엔진에서** (= "Unity가 룩 권위자").

## DDworld 적용

- "부위 분리"·"평면화"·"에디터≠최종"이 우리 Rigid Voxel 방향과 정확히 일치 → 검증 시 그대로 적용.
- 성능(12폴리/voxel)은 첫 모델 작업의 **크기/개수 한계선** 확인과 직결.
- 첫 모델 작업 시 [work-queue.md](../../production/work-queue.md) 아트 항목에서 이 원칙 적용.
