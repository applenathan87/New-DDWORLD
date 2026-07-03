# DDworld

> ⚠️ **2026-06-29 코어 전환: PvP 심리전 → PvE 헥사 영토 확장 오토배틀러.**
> **현행 기준 = [game-concept.md](design/gdd/game-concept.md) + [pve-pivot.md](design/research/pve-pivot.md)(결정 로그).**
> 아래 "Game Overview"·"Multiplayer-Ready 원칙"은 **옛 PvP 기준이라 폐기 대기** — game-concept.md가 우선한다.
> 옛 시스템 GDD(combat/deck/economy/deck-building/systems-index)는 `design/gdd/_archive/`로 이동, [ADR-001](docs/architecture/ADR-001-async-pvp.md)(비동기 PvP)도 폐기 예정. **PvP·고스트·매칭·판돈·심리전 내용 참조 금지.**

1인 인디 게임 "DDworld" 개발 프로젝트.

## ⭐ 현재 작업 트랙 (2026-07-02~): Origin — 이해하며 직접 만들기

**당분간 메인 작업은 [`Origin/`](Origin/CLAUDE.md)에서 진행한다.**
유니티를 처음부터 하나하나 이해하며 전투 MVP("재밌고 보기 좋은 전투")를 직접 만드는 학습·제작 트랙.

- 전체 지도: [Origin/roadmap.md](Origin/roadmap.md) (0~9단계) · 작업일지: `Origin/journal/`
- **작업 방식의 핵심 = 사용자의 이해도 축적**: 이해를 쌓아 확실한 디렉션을 주고, 직접 고칠 부분은 직접 고칠 수 있게 되는 것이 목표. 분담은 유연 — 사용자가 직접 하는 부분도, Claude에게 맡기는 부분도 있다. 단 Claude는 항상 "왜/어떻게"를 설명하며 진행. 상세는 [Origin/CLAUDE.md](Origin/CLAUDE.md).
- Origin 폴더에서의 작업은 학습 공간이므로 아래 프로덕션 기준(coding standards의 GDD 8섹션, 테스트 게이트 등)을 적용하지 않는다 (프로토타입 수준).
- 기획 문서 작업(GDD·아트바이블 등)은 기존대로 이 저장소 규칙을 따른다.

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

- **Engine**: Unity **6000.5.1f1** (Unity 6.5 — 2026-07-02 실측. 기존 프로토타입·Origin 새 프로젝트 동일 버전)
- **Language**: C#
- **Rendering**: URP (Universal Render Pipeline) — 쿼터뷰 3D + Post-processing
- **3D 모델링**: MagicaVoxel (캐릭터) + Blender (애니메이션, 환경)
- **Version Control**: Git with trunk-based development
- **Asset Pipeline**: Addressables
- **UI**: UI Toolkit

## Project Structure

@.claude/docs/directory-structure.md

### 참고 전용 폴더 (자동 참조 금지 — 토큰 절약)

- `article/` — 인디 개발 아티클 모음 (사용자 개인 읽기용)
- `CONTEST/` — 공모전 캘린더·목표

이 두 폴더는 **아이디에이션·일반 작업 시 자동으로 읽지 않는다.** 사용자가 명시적으로 요청할 때만 참조.

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
