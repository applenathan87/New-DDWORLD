# DDworld — Psychological Auto-Battler

1인 인디 오토배틀러 게임 "DDworld" 개발 프로젝트.

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

## Technology Stack

- **Engine**: Unity 6.3 LTS
- **Language**: C#
- **Rendering**: URP (Universal Render Pipeline) — 쿼터뷰 3D
- **Version Control**: Git with trunk-based development
- **Asset Pipeline**: Addressables
- **UI**: UI Toolkit

## Project Structure

@.claude/docs/directory-structure.md

## Engine Version Reference

@docs/engine-reference/unity/VERSION.md

## Technical Preferences

@.claude/docs/technical-preferences.md

## Coding Standards

@.claude/docs/coding-standards.md

## Collaboration Protocol

**User-driven collaboration, not autonomous execution.**
Every task follows: **Question → Options → Decision → Draft → Approval**

- Write/Edit 도구 사용 전 "May I write this to [filepath]?" 확인
- 변경 전 드래프트 또는 요약 제시 후 승인 요청
- 다중 파일 변경은 전체 changeset 명시적 승인 필요
- 사용자 지시 없이 commit 금지
- Session state는 `production/session-state/active.md`에 기록 (재시작 시 자동 복구)
