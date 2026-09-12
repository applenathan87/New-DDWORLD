# Directory Structure

```text
/
├── CLAUDE.md                    # Master configuration
├── .claude/                     # Agent definitions, skills, hooks, rules, docs
├── src/                         # Game source code (core, gameplay, ai, networking, ui, tools)
├── assets/                      # Game assets (art, audio, vfx, shaders, data)
├── design/                      # Game design documents (gdd, art, research)
│   └── research/notes/          # 프로젝트에 근거로 쓰는 기획 이론 노트 + takeaways.md (읽기 볼트에서 가져온 결론)
├── docs/                        # Technical documentation (architecture, api, postmortems)
│   └── engine-reference/        # Curated engine API snapshots (version-pinned)
├── tests/                       # Test suites (unit, integration, performance, playtest)
├── tools/                       # Build and pipeline tools (ci, build, asset-pipeline)
│   └── desk/                    # 출근부 앱 — 출퇴근·할 일·데브로그 로컬 웹앱 (의존성 0, 출근.bat 더블클릭)
├── prototypes/                  # Throwaway prototypes (isolated from src/)
├── Origin/                      # ⭐ 현재 작업 트랙 — 유니티 학습 + 전투 MVP (roadmap, journal, unity project)
├── ideation/                    # 아이디에이션 작업 공간 (로그라인·후크 등 — Claude 능동 참여)
├── REF_GAME/                    # 직접 레퍼런스 게임 분석 (Papers, Please 등)
├── image/                       # 아트 무드보드 (gitignored)
├── CONTEST/                     # 공모전 캘린더·목표 (참고 전용 — 자동 참조 X)
│                                # ※ 아티클·일반 공부 자료는 저장소 밖 읽기 볼트 C:\Reading (2026-09-12 이관)
└── production/                  # Production management (sprints, milestones, releases)
    ├── desk/                    # 출근부 데이터 (todo.md + devlog/YYYY-MM-DD.md — 하루 한 파일, 원본은 md)
    ├── session-state/           # Ephemeral session state (active.md — gitignored)
    └── session-logs/            # Session audit trail (gitignored)
```
