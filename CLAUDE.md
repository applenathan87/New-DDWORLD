# DDworld — Psychological Auto-Battler

1인 인디 오토배틀러 게임 "DDworld" 개발 프로젝트.

## Game Overview

- **게임명**: DDworld
- **장르**: 심리전 오토배틀러 (배틀십 + 가위바위보 + 자동전투)
- **핵심 경험**: 상대 패를 보고 배치를 읽는 "두뇌 싸움"
- **플랫폼**: PC (Steam)
- **멀티플레이어**: **비동기 PvP** (MVP — 다른 플레이어 배치 고스트와 대전). 동기 실시간 PvP는 Post-1.0 옵션 모드. 프로토타입은 AI 대전. (→ [ADR-001](docs/architecture/ADR-001-async-pvp.md))
- **개발 규모**: 솔로 인디

### Multiplayer-Ready 원칙 (src/ 프로덕션 코드에 적용)

**비동기 PvP가 MVP 모드이므로 결정론과 직렬화는 절대 요구사항**입니다.
프로토타입(`prototypes/`)에는 적용하지 않고, `src/`에 프로덕션 코드를 작성할 때 적용:

1. **결정론적 시뮬레이션** ⭐ 필수: 랜덤은 반드시 시드 기반. **같은 MatchData → 어떤 클라이언트에서도 같은 결과 보장** (비동기 고스트 재현의 핵심)
2. **입력/로직 분리**: 플레이어 입력 → Command 객체 → 게임 상태 변경 (직접 변경 금지)
3. **게임 상태 직렬화** ⭐ 필수: 핵심 게임 상태는 한 곳에 모아서 직렬화 가능하게 관리 (고스트 데이터 = 직렬화된 MatchData)
4. **고정 시뮬레이션 스텝**: 전투 시뮬레이션은 `Time.deltaTime` 대신 고정 틱 사용 (프레임레이트 무관)
5. **매치 데이터 구조** ⭐ 필수: 양측 배치 + 시드 + 영웅 정보를 하나의 MatchData로 묶어서 저장/전달 (고스트 풀의 단위)

## Visual Style

**Low-poly Voxel 3D** ([ADR-002](docs/architecture/ADR-002-visual-style-low-poly-3d.md))

- **캐릭터**: MagicaVoxel로 부위별 분리 모델링 (Head/Body/Arms/Legs/Weapon)
- **애니메이션**: Blender에서 본 + 리깅 + 키프레임 (B 방식)
- **환경**: Low-poly 메쉬 (Blender 또는 MagicaVoxel)
- **카메라**: 쿼터뷰 (X 29.2°) + Zoom 시 캐릭터 디테일
- **톤**: 따뜻한 색감 + Tilt-shift 미니어처 효과 + Bloom + DOF
- **레퍼런스**: Bad North, Tabletop Tavern, A Short Hike, Crossy Road

## Technology Stack

- **Engine**: Unity 6.3 LTS
- **Language**: C#
- **Rendering**: URP (Universal Render Pipeline) — 쿼터뷰 3D + Post-processing
- **3D 모델링**: MagicaVoxel (캐릭터) + Blender (애니메이션, 환경)
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
