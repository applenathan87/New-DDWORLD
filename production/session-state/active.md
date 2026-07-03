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
- **S1 (7/4) 구현 완료**: Day 1 서류 심사 루프 (사용자 플레이 검증 대기)
- 다음: S2 (7/5) = 도장 드래그 물성 + 면접 루프. 상세 플랜·컷 우선순위는 브리프 참조.

## 다음 할 것

- S2~S4 스프린트 계속 (브리프의 세션 플랜)
- 스프린트 후: 검증 결과를 ideation/mvp-design.md에 반영 → 확정되면 `design/gdd/` 정식 GDD 승격
