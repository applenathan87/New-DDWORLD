# Session State — active

> 재시작 시 자동 복구용(SessionStart 훅이 읽음). 최신 상태만 유지.
> **Last Updated**: 2026-07-03

## 현재 코어 = 「마왕성 인사팀」(가제)

DDworld 코어를 **2026-07-03에 전환**했다: (2세대 PvE 헥사 오토배틀러) → **「마왕성 인사팀」** (Papers, Please식 악당 면접 + 다이어제틱 데스크). 후크 = **판단축 반전**(악당을 뽑기에 거짓말·잔인함이 장점).

- 현행 기준: [ideation/concept-demon-hr.md](../../ideation/concept-demon-hr.md) + [ideation/mvp-design.md](../../ideation/mvp-design.md)
- 제작 트랙: [Origin/roadmap.md](../../Origin/roadmap.md) (면접 데스크 MVP로 개편 완료)
- 옛 컨셉(폐기) 인덱스: [design/gdd/_archive/README.md](../../design/gdd/_archive/README.md)

## 직전 세션에 한 일 (2026-07-03)

1. ideation 정독 → 가제 **"마왕성 인사팀"** 확정.
2. **MVP 기획 v0.2** 작성 (mvp-design.md): 3일+승진(성과 트리거), JD 적합 판정, 가젯 진행(촛불→질문카드→돋보기), 하루 결산+지연 사고, 질문 횟수 제한.
3. **Origin 로드맵** 개편: 전투 MVP → 면접 데스크 MVP.
4. **저장소 대청소**: 옛 컨셉(1세대 PvP·2세대 헥사) 문서 전부 아카이브(`_archive/`), 진입점(루트 CLAUDE.md·onboarding·이 파일) 현행 기준으로 갱신. ADR-001/003 Superseded. 아트 방향 확정 = **캐릭터 복셀 / 환경 로우폴리 3D / 코지 미니어처 톤 미승계.**

## 진행 중: MVP 프로토 스프린트 (7/4~7/7, Fable)

**[docs/mawang-hr-proto-brief.md](../../docs/mawang-hr-proto-brief.md)가 스프린트 기준 문서.**
- 위치: `prototypes/unity-prototype` — 씬 `Assets/mawanghr.unity`, 코드 `Assets/Scripts/MawangHR/`, 콘텐츠 `Assets/StreamingAssets/MawangHR/gamedata.json`
- **S1~S2 물성 구현 완료 (7/4, 커밋 a2affcf)**: Day 1 서류 심사 루프 + 단서 마킹(근거 이중 채점) + **3D 데스크 그레이박스** + pick-up-to-read(함께 들기·드래그 재배치·빈 곳 클릭 내려놓기) + 3D 도장(커서 지점 낙하 = 잉크 일치) + 절차생성 사운드. 사용자 플레이 검증 중 — 손맛·리듬 피드백 반영하며 진행.
- **다음: S2 나머지 = 면접 루프** (몬스터 등장 + 질문 카드 + 답변 마킹) → S3 = 3일 구조·승진·가젯·Day 3 이중반전. 상세는 브리프 참조.
- 미결 디자인 메모: 목업 2차의 "보류 도장 3종·예상 평가 스탯·체크리스트"는 열린 질문 (mvp-design §15에 반영 예정).

## 다음 할 것

- S2~S4 스프린트 계속 (브리프의 세션 플랜)
- 스프린트 후: 검증 결과를 ideation/mvp-design.md에 반영 → 확정되면 `design/gdd/` 정식 GDD 승격
