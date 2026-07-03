# Directory Structure

```text
/
├── CLAUDE.md                    # Master configuration
├── .claude/                     # Agent definitions, skills, hooks, rules, docs
├── src/                         # Game source code (core, gameplay, ai, networking, ui, tools)
├── assets/                      # Game assets (art, audio, vfx, shaders, data)
├── design/                      # Game design documents (gdd, narrative, levels, balance)
├── docs/                        # Technical documentation (architecture, api, postmortems)
│   └── engine-reference/        # Curated engine API snapshots (version-pinned)
├── tests/                       # Test suites (unit, integration, performance, playtest)
├── tools/                       # Build and pipeline tools (ci, build, asset-pipeline)
├── prototypes/                  # Throwaway prototypes (isolated from src/)
├── Origin/                      # ⭐ 현재 작업 트랙 — 유니티 학습 + 전투 MVP (roadmap, journal, unity project)
├── ideation/                    # 아이디에이션 작업 공간 (로그라인·후크 등 — Claude 능동 참여)
├── article/                     # 인디 개발 아티클 모음 (개인 읽기용, 참고 전용 — 자동 참조 X)
├── CONTEST/                     # 공모전 캘린더·목표 (참고 전용 — 자동 참조 X)
└── production/                  # Production management (sprints, milestones, releases)
    ├── session-state/           # Ephemeral session state (active.md — gitignored)
    └── session-logs/            # Session audit trail (gitignored)
```
