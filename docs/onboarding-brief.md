# DDworld — 신규 모델/협업자 온보딩 브리핑

> **용도**: 새 AI 모델(예: Fable)이나 협업자에게 프로젝트를 "다 읽지 않고" 빠르게 파악시키기 위한 큐레이션 읽기 가이드.
> **핵심 주의**: 2026-06-29 코어 전환 때문에 문서가 "현행 정본 / 참고 / 폐기"로 갈린다. 아래 순서·구분을 반드시 지킬 것.
> **Last Updated**: 2026-07-02

---

## 한 줄 요약

솔로 인디 게임(Unity 6.3 / C#). **2026-06-29에 코어를 전환**했다: `비동기 PvP 심리전 오토배틀러` → **`PvE 헥사 영토 확장 오토배틀러`**.

---

## ① 먼저 읽어라 (정본 — 이게 전부의 기준)

1. **[design/research/pve-pivot.md](../design/research/pve-pivot.md)** — **결정 로그 = 정본.** 확정된 것(A섹션 LOCKED)과 미결 질문(Q리스트)이 여기 다 있다. 뭐가 정해졌고 뭐가 안 정해졌는지 판단의 최종 근거.
2. **[design/gdd/game-concept.md](../design/gdd/game-concept.md)** — 위 결정들을 "읽히는 서사"로 합본한 게임 개요. 용어집(캠페인/진격/티어/레벨)·엘리베이터 피치·Open Items.

## ② 시스템 상세가 필요하면

3. **[design/gdd/combat.md](../design/gdd/combat.md)** — 현행 전투(PvE 레인 오토배틀, 5×5 카운터 배치). *2026-06-30 재작성본.*
4. **[design/art/art-bible.md](../design/art/art-bible.md)** — 아트 정본. Low-poly Voxel 3D + Tilt-shift. (렌더 전제는 ADR-002/003)

## ③ 코드(프로토타입)를 볼 거면 진입점

- **[prototypes/unity-prototype/Assets/Scripts/Hex/HexGamePrototype.cs](../prototypes/unity-prototype/Assets/Scripts/Hex/HexGamePrototype.cs)** — 현재 활발히 작업 중인 헥사 프로토타입 (최근 커밋 전부 여기).
- **[Scripts/Systems/](../prototypes/unity-prototype/Assets/Scripts/Systems/)** — BattleSimulator·Soldier·GameManager 등 전투 시뮬. ⚠️ 일부는 옛 PvP 프로토타입 잔재일 수 있으니 헥사 쪽과 대조.

## 🚫 읽지 마라 (폐기 — 옛 PvP 기준이라 오정보를 준다)

- `design/gdd/_archive/` 전체 (옛 combat/deck/economy/systems-index)
- `design/art/_archive/` 전체 (HD-2D 시절 자료)
- `docs/architecture/ADR-001` (비동기 PvP — 폐기 예정)
- CLAUDE.md의 "Game Overview"·"Multiplayer-Ready 원칙" 섹션 → **옛 PvP 기준, game-concept.md가 우선.**
- 키워드로 **PvP·고스트·매칭·판돈·심리전**이 나오면 폐기 맥락이다.

---

## 사용 팁

- 목적이 명확하면(리뷰 / 특정 시스템 검증 / 밸런스) ①의 2개만으로도 충분할 때가 많다. 상세(②)·코드(③)는 필요할 때만.
- 이 문서 자체도 전환이 더 진행되면 갱신 대상이다. 정본(pve-pivot.md)과 어긋나면 정본이 우선.
