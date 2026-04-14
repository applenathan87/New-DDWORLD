# DDworld — Psychological Auto-Battler

1인 인디 오토배틀러 게임 "DDworld" 개발 프로젝트.
Claude Code 서브에이전트가 도메인별로 역할을 분담하여 개발을 지원합니다.

## Game Overview

- **게임명**: DDworld
- **장르**: 심리전 오토배틀러 (배틀십 + 가위바위보 + 자동전투)
- **핵심 경험**: 상대 패를 보고 배치를 읽는 "두뇌 싸움"
- **플랫폼**: PC (Steam)
- **개발 규모**: 솔로 인디

## Design Reference

- **DDworld 볼트**: `~/ProjectDDWORLD/` (Obsidian vault — GDD, 데브로그, 게임분석)
- **GDD 핵심**: `~/ProjectDDWORLD/02_GDD/`
- **데브로그**: `~/ProjectDDWORLD/데브로그/`

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
