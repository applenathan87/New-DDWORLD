# 12 Voxel 캐릭터 — 모델링 치트시트

> **목적**: ~12 voxel 키 캐릭터(MagicaVoxel)를 만들 때 비율·요령을 빠르게 보는 참고.
> **근거**: 리서치 2026-06-30 + [art-bible §8 G](design/art/art-bible.md) (스케일 규약 정본).
> **한 줄**: 12 voxel에선 디테일이 거의 안 보임 → **실루엣 + 색**이 전부. 참고 = **Crossy Road**.

---

## 시작 비율 (바로 써먹기)

대략 **6(폭) × 5(깊이) × 12(높이) voxel** 로 시작:

- **머리 4~5 / 몸통 4 / 다리 4** → 약 **2~3등신**(귀엽게). 머리 키울수록 더 치비.
- **팔다리 1~2 voxel 두께**, **관절(팔꿈치·무릎) 없음** — 해상도가 없어서 못 넣음.
- **머리 크게** + 눈은 **1 voxel 점** 하나 + **실루엣 후크**(투구/무기/귀/망토) + **강조색 1~2개**.

## 핵심 교훈 (12 voxel = 거의 다 실루엣+색)

- **정체성 = 실루엣 + 색.** 얼굴 디테일은 거의 무의미. 병종마다 **다른 실루엣 + 시그니처 색** → 전장에서 한눈에 구분 (Bad North 방식).
- **회색으로 형태부터 잡고 색은 나중.** (형태 약한 걸 색이 숨김)
- **미러-X로 반쪽만** 만들기. **쿼터뷰**라야 큐브가 입체로 읽힘 (= 우리 29.2° 카메라가 정답).
- **버리는 것**: 손가락·관절·옷주름·2 voxel 미만 소품 → 블룸/DOF에 다 묻힘. 복셀은 **윤곽을 바꾸는 곳**(투구 볏·무기·망토)에 투자.

## 워크플로우 (MagicaVoxel)

1. 볼륨 크기 고정 (대략 6×5×12)
2. 회색 블록으로 실루엣 먼저 → OK면 색
3. 미러-X 켜고 반쪽만, `T`(추가)/`R`(지우기)
4. 부위 분리(Head/Body/Arms/Legs/Weapon) 유지 → 리깅·리지드 애니용
5. 게임 줌(쿼터뷰)에서 가독성 확인 — 모델러 안에서가 아니라

## 참고 링크

**보면서 비율 잡기 (돌려보기)**
- Crossy Road 3D 모델: https://sketchfab.com/3d-models/crossy-road-in-voxel-art-2754d84449bd4411a9498b3064d78fb1
- Crossy Road 캐릭터 시트: https://dribbble.com/shots/3626156-Crossy-Road-Characters
- Sketchfab `voxel-character` 태그: https://sketchfab.com/tags/voxel-character
- "Little People" 보셀 캐릭터 팩 45종: https://sketchfab.com/3d-models/little-people-voxel-character-pack-low-poly-381390656fe04623996f59980e66377f

**실제 치수 / 튜토리얼**
- Tiago — 첫 3D 모델 (12×10×30 voxel, 우리 12에 맞게 축소): https://tiagojdf.medium.com/how-i-did-my-first-3d-model-magicavoxel-tutorial-6273319486e6
- 1분 캐릭터 만들기: https://www.youtube.com/watch?v=gND2_m4Kx3I
- Thinker-Talk (MagicaVoxel→Mixamo→Unity, T-포즈·부위 분리): https://www.thinker-talk.com/how-to-create-a-voxel-character-in-magicavoxel-animate-it-in-mixamo-and-use-it-in-unity-3d

## 우리 프로젝트 연결

- **비율·치수** → [art-bible §8 G](design/art/art-bible.md) (병사 ~12 voxel = 스케일 기준 자)
- **실루엣+색으로 구분** → [ADR-003](docs/architecture/ADR-003-rigid-instancing-crowd-rendering.md) (400명 군중 가독성)
- **부위 분리 · 리지드 애니** → art-bible §8 A/C, [project_rigid_voxel]
- **쿼터뷰 29.2°** → 큐브 입체감 (Crossy Road 교훈) — art-bible §1
