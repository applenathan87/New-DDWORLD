# design/gdd/ — 현재 상태

> **2026-09-12: 게임 컨셉 문서 v1 완성** — [game-concept.md](game-concept.md). `/design-system`이 읽는 상위 정본.
> 시스템별 GDD(8섹션 표준)는 아직 없다. **10월 새 시즌 첫 마일스톤 = GDD v1 확정** — 이 폴더가 채워지는 시점.

## 지금 어디를 봐야 하나

- **게임 컨셉 정본** → [game-concept.md](game-concept.md) — 피치·정체성·핵심 판타지·후크·MDA·코어 루프·필라와 안티필라·레퍼런스·타깃·기술·리스크·MVP 정의·다음 단계
- **상세 기획(컨셉 단계 문서)** → [design/concept/](../concept/_index.md) — mvp-design(3일 구조·판정·단서·가젯·카드·콘텐츠), interview_idea(덱빌딩), night-part(밤 파트), build-roadmap(구현 순서 초안). 컨셉 문서와 어긋나면 game-concept.md가 우선.
- **옛 컨셉(폐기)** → [_archive/README.md](_archive/README.md) — 1세대 PvP · 2세대 헥사, 둘 다 죽음.

## 다음 작업

1. `systems-index.md` — MVP를 시스템으로 분해하고 의존·우선순위·설계 순서를 정한다 (템플릿 = `.claude/docs/templates/systems-index.md`. `/map-systems` 스킬은 이 저장소에 없으므로 손으로 작성).
2. 시스템별 GDD — `/design-system [시스템]`으로 8섹션 표준(Overview · Player Fantasy · Detailed Rules · Formulas · Edge Cases · Dependencies · Tuning Knobs · Acceptance Criteria) 작성. 후보(game-concept.md 열린 질문 표 기준): 판정(도장·마킹·JD) · 지침 누적 · 면접(카드·코스트) · 가젯 · 하루 구조(근무 시간·결산·지연 사고) · 경제(공적·직급·골드) · 밤 파트(미니맵 허브·인물·카드) · 미니게임 · 콘텐츠 파이프라인.
3. 각 GDD는 새 세션에서 `/design-review [파일] --depth lean`, 전부 쓰면 `/review-all-gdds`.
