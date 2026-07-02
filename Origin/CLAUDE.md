# Origin

> DDworld 전투 MVP를 **유니티 처음부터 하나하나 이해하며** 직접 만드는 학습·제작 워크스페이스.
> 위치: `c:\New-DDWORLD\Origin` (게임 저장소 안). 기획/문서는 상위 `c:\New-DDWORLD`·`C:\ProjectDDWORLD` 볼트에 있고, 여기는 **손으로 만드는 실전 공간**.

## 큰 목표

**"재밌고, 보기 좋은 전투"** = MVP. 그 MVP를 만드는 과정을, 유니티를 처음 켜는 것부터 하나하나 이해하며 진행한다.

## 작업 방식 (중요 — 매 세션 지킬 것)

- **한 번에 한 걸음.** "지금 할 것 하나 + 왜 그런지" 짧게 설명 → 사용자가 직접 유니티에서 함 → 막히면 질문 → 다음 걸음.
- 모르는 것은 물어보며 진행. 코드는 Claude가 베이스를 써주되 "이게 뭘 하는지" 설명과 함께.
- 이해가 붙으면 방향 전환 환영. 그게 목표.
- 유니티 화면 클릭은 사용자가 함(Claude는 직접 조작 불가). 막히면 화면 캡처/설명 → Claude가 어디를 누를지 짚어줌.

## 작업일지 (journal/)

- 매 작업일 = `DAY-N`. 사용자가 "오늘 배운 것/한 것" 초안을 주면 Claude가 정리해 `journal/DAY-NN.md`에 기록.
- `journal/000-log.md` = 전체 진행 로그. 한 날/안 한 날을 O/X로 체크.

## 로드맵

- 전체 지도: [roadmap.md](roadmap.md) — 단계별 상세 워크북은 진행하며 그때그때 `docs/`에 작성.

## 환경

- **엔진**: Unity 6.3 LTS + URP (새 프로젝트, `project/`에 생성 예정)
- **GPU**: RTX 4090 (24GB) — 로컬 렌더링/생성 최상급
- **모델링**: MagicaVoxel (캐릭터, 부위 분리) → **Blender(B방식: 본+리깅+키프레임)** → FBX → 유니티
- **이식 계획**: MVP가 "재밌다"고 검증되면 본 프로토타입(`prototypes/unity-prototype`)으로 이식. (버전·URP 동일하게 유지)

## 폴더 구조

```text
c:\New-DDWORLD\Origin\
├── CLAUDE.md        # 이 파일
├── roadmap.md       # 전체 지도
├── docs\            # 단계별 상세 워크북
├── journal\         # 작업일지 (000-log.md + DAY-NN.md)
├── refs\            # 레퍼런스 이미지
└── project\         # 유니티 새 프로젝트 (Unity Hub로 생성)
```
