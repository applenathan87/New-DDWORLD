# Docs Directory

When authoring or editing files in this directory, follow these standards.

## Architecture Decision Records (`docs/architecture/`)

Use the ADR template: `.claude/docs/templates/architecture-decision-record.md`

**Required sections:** Title, Status, Context, Decision, Consequences,
ADR Dependencies, Engine Compatibility, GDD Requirements Addressed

**Status lifecycle:** `Proposed` → `Accepted` → `Superseded`
- Never skip `Accepted` — stories referencing a `Proposed` ADR are auto-blocked
- Use `/architecture-decision` to create ADRs through the guided flow

**TR Registry / Control Manifest / `docs/registry/`:** 템플릿 기능. 이 저장소에서는 2026-09-12 재정비 때
`tr-registry.yaml`·`docs/registry/`를 삭제했다 (옛 헥사 GDD 기준이라 전부 STALE). 정식 GDD·ADR이 생긴 뒤
`/architecture-review`가 필요로 하면 그때 다시 만든다.

**현재 ADR 상태:** ADR-001(비동기 PvP)·ADR-003(400명 크라우드) = Superseded. ADR-002(비주얼 스타일)만 유효하되
2026-08-18 갱신(캐릭터 = 복셀풍 로우폴리) 헤더 주석을 따른다.

**Validation:** Run `/architecture-review` after completing a set of ADRs.

## Engine Reference (`docs/engine-reference/`)

Version-pinned engine API snapshots. **Always check here before using any
engine API** — the LLM's training data predates the pinned engine version.

Current engine: see `docs/engine-reference/unity/VERSION.md`
