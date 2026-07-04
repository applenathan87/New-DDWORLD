# Session State — active

> 재시작 시 자동 복구용(SessionStart 훅이 읽음). 최신 상태만 유지.
> **Last Updated**: 2026-07-04 (밤)

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
- **S2a (7/4) 구현 완료**: 전화 스케줄링 퍼즐 (Day 1 후반 업무) — **사진을 수정구에 갖다 대면 통화**(말풍선이 수정구 위에, 사진이 붙음), 끌어서 캘린더 배치(슬롯 점유 표시+정확 안착), 전원 배치 시 확정 버튼 활성화, 위반=다음날 아침 사고 보고서. **단계 비연결 원칙** 확정 (각 단계 = 독립 지원자 세트, 랜덤 풀 대비 — mvp-design §3).
- **S2a+ (7/4 밤, 커밋 2a09c2b)**: **깃펜 방향 마킹**(좌클릭 V=합격 신호/우클릭 X=탈락 신호, 방향 오독 채점 "여긴 마왕성이다") + **물리 놀이터**(소품 5종·도장 던지기 — Rigidbody 튕김/구름, 멈추면 보라 마법 버스트로 소멸→제자리 재생성, PropPhysics 공용 루틴) + 확정 잉크 최상단 정렬.
- **다음: S2b = 면접 루프** (몬스터 등장 + 질문 카드 + 답변 마킹(V/X 확장)) → S3 = 3일 구조·승진·가젯·Day 3 이중반전. 상세는 브리프 참조. 몬스터 등장/퇴장 연출 = MagicBurst 재활용, 피격 튕김 = 몬스터에 콜라이더만 추가.
- 미결 디자인 메모: mvp-design §15 열린 질문 10개 (보류 도장·예상 평가 스탯·드래그 재배치 유지 여부·마킹 노브 등). 평가 체크리스트 = 면접 단계 승진 보상 가젯으로 결정됨.

## 다음 할 것

- S2~S4 스프린트 계속 (브리프의 세션 플랜)
- 스프린트 후: 검증 결과를 ideation/mvp-design.md에 반영 → 확정되면 `design/gdd/` 정식 GDD 승격
