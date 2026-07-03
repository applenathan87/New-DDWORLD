# DDworld — 신규 모델/협업자 온보딩 브리핑

> **용도**: 새 AI 모델이나 협업자에게 프로젝트를 "다 읽지 않고" 빠르게 파악시키는 큐레이션 읽기 가이드.
> **Last Updated**: 2026-07-03 (「마왕성 인사팀」 전환 반영)

---

## 한 줄 요약

솔로 인디 게임(Unity 6.5 / C#). 현 컨셉 = **「마왕성 인사팀」(가제)** — 마왕성 인사팀 신입이 되어, 사악한 직무에 지원한 몬스터를 면접 보는 **다크판타지 코미디 HR 시뮬**. Papers, Please식 판단·심문 + 책상 위 물건을 직접 만지는 다이어제틱 데스크.

**후크**: 악당을 뽑기에 **판단축이 비틀린다** — 거짓말·잔인함이 (직무 JD에 따라) 장점, "알고 보니 착함"이 위험 신호.

---

## ⚠️ 컨셉을 두 번 갈아엎었다 — 옛 문서 대량 폐기

코어가 **두 번** 전환됐다. 아래 키워드가 나오면 **폐기 맥락**이다:

- **1세대(폐기)**: PvP · 고스트 · 매칭 · 판돈 · 심리전
- **2세대(폐기)**: 헥사 · 영토 확장 · 내 군대 · 400명 전투 · 소모전 · permadeath · 오토배틀러
- 옛 문서 전부 = `design/gdd/_archive/` + `design/research/_archive/` — 인덱스: [design/gdd/_archive/README.md](../design/gdd/_archive/README.md)

## ① 먼저 읽어라 (현행 정본)

1. **[ideation/concept-demon-hr.md](../ideation/concept-demon-hr.md)** — 컨셉·후크·물성 원칙·업무 스코프 확장(가젯 진행)·커리어 아크.
2. **[ideation/mvp-design.md](../ideation/mvp-design.md)** — MVP 전체 기획: 3일+승진(성과), JD 적합 판정, 가젯 진행(촛불→질문카드→돋보기), 하루 결산+지연 사고, 질문 횟수 제한.
3. **[ideation/refs/면접화면-목업.png](../ideation/refs/면접화면-목업.png)** — 사용자 제작 화면 목업 (비주얼·UX 타깃).

## ② 지금 실제 제작 (작업 트랙)

- **[Origin/](../Origin/CLAUDE.md)** — 유니티를 처음부터 이해하며 만드는 학습·제작 트랙. 지도 = [Origin/roadmap.md](../Origin/roadmap.md) (마왕성 인사팀 MVP로 개편됨).

## ③ 아트

- **[design/art/art-bible.md](../design/art/art-bible.md)** — ⚠️ 옛 헥사 전투 기준 문서. **승계 = 캐릭터 복셀 + 정점색 기술만.** 「마왕성 인사팀」은 **환경 = 로우폴리 3D**, 코지 미니어처 톤 미승계. 재작성 대기.

## 🚫 읽지 마라 (폐기 — 오정보를 준다)

- `design/gdd/_archive/` 전체 (1·2세대 GDD)
- `design/research/_archive/` 전체
- `docs/architecture/ADR-001`(비동기 PvP) · `ADR-003`(400명 크라우드 렌더링) = **Superseded**
- 정식 `design/gdd/` GDD는 아직 없음 — [design/gdd/README.md](../design/gdd/README.md) 참고.

---

## 사용 팁

- 목적이 명확하면 ①의 2개(concept + mvp-design)만으로 충분할 때가 많다.
- 이 문서도 전환이 진행되면 갱신 대상. 현행 정본(ideation/)과 어긋나면 정본이 우선.
