# Directory Structure

> 2026-09-12 재정비 기준. 이동·삭제 내역과 이유 = [production/reorg-2026-09.md](../../production/reorg-2026-09.md)

```text
/
├── CLAUDE.md                    # Master configuration
├── .claude/                     # Agent definitions, skills, hooks, rules, docs
├── game/                        # ⭐ 유니티 프로젝트 (Unity 6000.5.1f1 + URP) — Origin 0단계에서 Unity Hub로 생성 (아직 없음)
│                                #   Hub: Location=C:\DungeonHR, Project name=game. 미리 만들어 두지 않는다 (Hub가 거부)
├── Origin/                      # ⭐ 현재 작업 트랙 문서 — 유니티 학습·제작 (CLAUDE.md · roadmap · docs 워크북 · refs)
├── design/                      # 기획 = 볼트의 심장
│   ├── concept/                 # 현행 기획 정본 (concept-demon-hr · mvp-design · interview_idea … + refs/ 목업) ← 옛 ideation/
│   ├── gdd/                     # 정식 GDD (8섹션 표준) — 10월 첫 마일스톤 "GDD v1"에서 작성. _archive/README = 죽은 컨셉 연혁
│   ├── art/                     # 아트 바이블(STALE, 재작성 대기) · visual-polish
│   └── research/                # notes/(기획 이론 노트) · takeaways.md(읽기 볼트 결론) · ref-games/(레퍼런스 게임 조사 ← 옛 REF_GAME/)
├── docs/                        # 기술 문서 — architecture/(ADR) · pipeline/ · onboarding-brief · WORKFLOW-GUIDE
│   └── engine-reference/        # Curated engine API snapshots (version-pinned)
├── production/                  # 기록·PM
│   ├── desk/                    # 출근부 데이터 (todo.md · devlog/YYYY-MM-DD.md) — 유일한 작업 기록
│   ├── session-state/           # Session state (active.md)
│   ├── session-logs/            # Session audit trail (gitignored)
│   └── reorg-2026-09.md         # 2026-09 폴더 재정비 기록
├── tools/desk/                  # 출근부 앱 (의존성 0, 출근.bat 더블클릭)
├── _archive/                    # 참고 전용 보관소 (README = 인덱스)
│   ├── unity-prototype/         # 2026-07 그레이박스 프로토 (MawangHR S1~S3a) — 캐시 삭제됨, 열면 Unity가 재생성
│   └── mawang-hr-proto-brief.md # 그 프로토의 작업 명령서
├── CONTEST/                     # 공모전 캘린더·목표 (참고 전용 — 자동 참조 X)
└── image/                       # 아트 무드보드 (gitignored)
```

- **삭제된 것** (2026-09-12, git 히스토리에 보존 — 삭제 직전 트리 = 커밋 86ac21f): `production/devlog/`(옛 Day 01~29), `Origin/journal/`, `src/`, `.github/`, `docs/registry/`, `docs/architecture/tr-registry.yaml`, `prototypes/`(→ _archive), `ideation/`(→ design/concept), `REF_GAME/`(→ design/research/ref-games).
- **`.claude/` 템플릿 문서·스킬이 말하는 `src/`·`tests/`·`assets/`는 이 저장소에 없다** — 코드는 전부 `game/`(유니티 프로젝트) 안. 그 스킬들은 GDD·ADR이 생긴 뒤 필요할 때만 쓴다.
- 아티클·일반 공부 자료는 저장소 밖 읽기 볼트 `C:\Reading`.
