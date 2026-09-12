# design/gdd/ — 현재 상태

> **2026-09-12: 게임 컨셉 문서 v1 + 시스템 인덱스 v1 완성** — [game-concept.md](game-concept.md) · [systems-index.md](systems-index.md). `/design-system`이 읽는 상위 정본 두 개.
> 시스템별 GDD(8섹션 표준)는 아직 없다. **10월 새 시즌 첫 마일스톤 = GDD v1 확정(코어 7개)**, 이어서 GDD v1.1(변주·밤 3개) — 이 폴더가 채워지는 시점.

## 지금 어디를 봐야 하나

- **게임 컨셉 정본** → [game-concept.md](game-concept.md) — 피치·정체성·핵심 판타지·후크·MDA·코어 루프·필라와 안티필라·레퍼런스·타깃·기술·리스크·MVP 정의·다음 단계
- **상세 기획(컨셉 단계 문서)** → [design/concept/](../concept/_index.md) — mvp-design(3일 구조·판정·단서·가젯·카드·콘텐츠), interview_idea(덱빌딩), night-part(밤 파트), build-roadmap(구현 순서 초안). 컨셉 문서와 어긋나면 game-concept.md가 우선.
- **옛 컨셉(폐기)** → [_archive/README.md](_archive/README.md) — 1세대 PvP · 2세대 헥사, 둘 다 죽음.

## 다음 작업

1. ~~`systems-index.md` 작성~~ → 완료 (2026-09-12). 시스템 12개, MVP 10개, 설계 순서 = 케이스 데이터 → 지침 → 판정 → 물성 → 하루 구조·결산 → 승진·경제 → 면접 (1차) → 스케줄링 → 밤 허브 → 미니게임 (2차).
2. 시스템별 GDD — `/design-system [시스템]`으로 8섹션 표준(Overview · Player Fantasy · Detailed Rules · Formulas · Edge Cases · Dependencies · Tuning Knobs · Acceptance Criteria) 작성. 순서와 범위는 [systems-index.md](systems-index.md). 질문은 텍스트로, 에이전트는 값이 필요한 섹션에만.
3. 각 GDD는 새 세션에서 `/design-review [파일] --depth lean`, 1차 7개 끝나면 `/review-all-gdds`.
