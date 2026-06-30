# Hex Prototype v0 — "왕국이 자라는 손맛"

> 던져버릴 프로토타입 (relaxed standards). 목적 = **헥사 영토 확장 루프**가 재밌는지 검증.
> 근거 설계: `design/research/pve-pivot.md`, `design/gdd/game-concept.md`.

## 실행법 (에디터 세팅 거의 없음)

1. Unity로 `prototypes/unity-prototype` 프로젝트 열기
2. **새 빈 씬** 생성 (File → New Scene → Empty 또는 기본)
3. 빈 GameObject 하나 만들고 (`GameObject → Create Empty`)
4. 거기에 **`HexGamePrototype`** 스크립트 부착
5. **Play** ▶ — 카메라·조명·헥사 그리드가 코드로 자동 생성됨

> 씬/프리팹/카메라 세팅 불필요. 스크립트 하나가 전부 런타임 생성.

## 조작

- **타일 클릭(좌)** = 선택 (선택 타일은 살짝 떠오르고 파랗게). 선택 시 카메라가 그쪽으로 살짝 따라옴(딜레이 후)
- **우클릭 드래그** = 카메라 이동(팬) — grab 스타일
- 내 영토에 **인접한** 빈 땅 → 패널의 **"정착"** / 적 타일 → **"전투로 정복"**
  - 본성에서 마커가 이동하는 연출 후 점령 (= 하루 소비)
- **정복한 빈 타일** 선택 → **농장**(카드 생산) / **금광**(골드) 건설
- **End Turn** = 전투 없이 하루 보내기 (수입·생산 틱)
- **맵 리셋** 버튼

## 루프

타일 정착/정복(+1 day) → 정복 타일당 골드 수입 → 농장/금광 건설 → 본성 레벨업(정복 N개마다 → 건설 한도·생산 슬롯↑) → 더 확장.

## 스코프 (v0)

**들어감**: 헥사 그리드 / 본성 중심 / 빈·적 타일(멀수록 적↑) / 인접 확장 / 이동 연출 / Day·골드·본성레벨 / 건물 2종 / 카드 추상 생산 / OnGUI HUD.

**플레이스홀더·제외(의도)**:
- **전투 = 즉시 승리**(진짜 combat.md 규칙 아님)
- **아트 = 단색 헥사**(보셀 모델 아님)
- 멀티자원·퀘스트·인구·미니맵·세이브·안개 — 전부 나중
- 8병종/패턴/티어/머지 — 카드는 *숫자*만

## 만지면서 조정할 만한 값 (스크립트 Inspector)

`mapRadius`, `hexSize`, `mapSeed`(적 배치), `startGold`, `incomePerClaimedTile`, `mineBonus`, `costFarm/Mine`, `farmIntervalDays`, `tilesPerCastleLevel`.

**카메라 (Play 중 실시간 조정됨)**: `cameraPitch`(내려보는 각도°, 작을수록 낮은 쿼터뷰), `cameraYaw`(좌우 회전°), `cameraDistance`(0=자동), `cameraFov`(시야각), `cameraPivot`(중심점). Play 상태에서 GameObject 선택 → Inspector 값 바꾸면 즉시 반영.

## 알려진 한계 / 검증 포인트

- 카메라 프레이밍은 `mapRadius` 기준 자동 — 너무 크/작으면 `HexGamePrototype.SetupCameraAndLight()` 조정.
- URP/Built-in 둘 다 대응(색은 `_BaseColor`+`_Color` 동시 세팅). 색이 안 보이면 셰이더 fallback 확인.
- 입력은 OnGUI 이벤트 기반이라 Input System 백엔드와 무관.
- **검증 질문**: 이 루프가 "한 번 더" 누르고 싶은가? 확장-건설-성장의 리듬이 손맛 나는가?
