# DDworld — Psychological Auto-Battler

1인 인디 오토배틀러 게임 "DDworld" 개발 프로젝트.
Claude Code 서브에이전트가 도메인별로 역할을 분담하여 개발을 지원합니다.

## Game Overview

- **게임명**: DDworld
- **장르**: 심리전 오토배틀러 (배틀십 + 가위바위보 + 자동전투)
- **핵심 경험**: 상대 패를 보고 배치를 읽는 "두뇌 싸움"
- **플랫폼**: PC (Steam)
- **멀티플레이어**: 온라인 PvP 대전 (프로덕션 목표). 프로토타입은 AI 대전
- **개발 규모**: 솔로 인디

### Multiplayer-Ready 원칙 (src/ 프로덕션 코드에 적용)

프로토타입(`prototypes/`)에는 적용하지 않는다. `src/`에 프로덕션 코드를 작성할 때 적용:

1. **결정론적 시뮬레이션**: 랜덤은 반드시 시드 기반. 같은 입력 → 같은 결과 보장
2. **입력/로직 분리**: 플레이어 입력 → Command 객체 → 게임 상태 변경 (직접 변경 금지)
3. **게임 상태 직렬화**: 핵심 게임 상태는 한 곳에 모아서 직렬화 가능하게 관리
4. **고정 시뮬레이션 스텝**: 전투 시뮬레이션은 `Time.deltaTime` 대신 고정 틱 사용
5. **매치 데이터 구조**: 양측 배치를 하나의 MatchData로 묶어서 전달

## Design Reference

- **모든 작업은 이 프로젝트(`New_DDWORLD/`)에 집중한다**
- `~/ProjectDDWORLD/`는 아카이브됨 — 참조하지 않는다
- 아트 레퍼런스: `design/art/references/` (git 미추적)

## Technology Stack

- **Engine**: Unity 6.3 LTS
- **Language**: C#
- **Rendering**: URP (Universal Render Pipeline) — 쿼터뷰 3D
- **Version Control**: Git with trunk-based development
- **Build System**: Unity Build Pipeline
- **Asset Pipeline**: Addressables
- **UI**: UI Toolkit (신규 프로젝트 권장)

> **Note**: Unity 전용 에이전트를 사용합니다: `unity-specialist`,
> `unity-ui-specialist`, `unity-shader-specialist`, `unity-addressables-specialist`.

## Project Structure

@.claude/docs/directory-structure.md

## Engine Version Reference

@docs/engine-reference/unity/VERSION.md

## Technical Preferences

@.claude/docs/technical-preferences.md

## Coordination Rules

@.claude/docs/coordination-rules.md

## Collaboration Protocol

**User-driven collaboration, not autonomous execution.**
Every task follows: **Question -> Options -> Decision -> Draft -> Approval**

- Agents MUST ask "May I write this to [filepath]?" before using Write/Edit tools
- Agents MUST show drafts or summaries before requesting approval
- Multi-file changes require explicit approval for the full changeset
- No commits without user instruction

See `docs/COLLABORATIVE-DESIGN-PRINCIPLE.md` for full protocol and examples.

> **첫 세션?** `/help`로 사용 가능한 명령어를 확인하세요.

## Coding Standards

@.claude/docs/coding-standards.md

## Context Management

@.claude/docs/context-management.md
