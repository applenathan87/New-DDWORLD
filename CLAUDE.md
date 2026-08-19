# DDworld

> ⚠️ **2026-07-03 코어 전환: (PvE 헥사 오토배틀러) → 「마왕성 인사팀」(가제) — 다크판타지 코미디 HR 시뮬 + 다이어제틱 데스크.**
> **현행 기준 = [ideation/concept-demon-hr.md](ideation/concept-demon-hr.md) + [ideation/mvp-design.md](ideation/mvp-design.md).**
> 정식 GDD는 아직 미작성 — ideation에서 확정 후 `design/gdd/`로 승격 예정 ([design/gdd/README.md](design/gdd/README.md)).
> **폐기(참조 금지) — 컨셉이 두 번 죽었다**: ① 1세대 PvP 심리전 ② 2세대 PvE 헥사 영토 확장. 옛 문서 전부 `design/gdd/_archive/`(01-pvp-psychological·02-pve-hex) + `design/research/_archive/`로 이동. 인덱스 = [_archive/README.md](design/gdd/_archive/README.md).
> **다음 키워드가 나오면 폐기 맥락이다**: PvP·고스트·매칭·판돈·심리전 / 헥사·영토 확장·내 군대·400명 전투·소모전·permadeath·오토배틀러.

1인 인디 게임 "DDworld"(현 작업명 「마왕성 인사팀」) 개발 프로젝트.

## ⭐ 현재 작업 트랙 (2026-07-02~): Origin — 이해하며 직접 만들기

**당분간 메인 작업은 [`Origin/`](Origin/CLAUDE.md)에서 진행한다.**
유니티를 처음부터 하나하나 이해하며 「마왕성 인사팀」 MVP("간파가 재밌고 도장이 손맛 있는 면접 데스크")를 직접 만드는 학습·제작 트랙.

- 전체 지도: [Origin/roadmap.md](Origin/roadmap.md) (0~9단계) · 작업일지: `Origin/journal/`
- **작업 방식의 핵심 = 사용자의 이해도 축적**: 이해를 쌓아 확실한 디렉션을 주고, 직접 고칠 부분은 직접 고칠 수 있게 되는 것이 목표. 분담은 유연 — 사용자가 직접 하는 부분도, Claude에게 맡기는 부분도 있다. 단 Claude는 항상 "왜/어떻게"를 설명하며 진행. 상세는 [Origin/CLAUDE.md](Origin/CLAUDE.md).
- Origin 폴더에서의 작업은 학습 공간이므로 아래 프로덕션 기준(coding standards의 GDD 8섹션, 테스트 게이트 등)을 적용하지 않는다 (프로토타입 수준).
- 기획 문서 작업(GDD·아트바이블 등)은 기존대로 이 저장소 규칙을 따른다.

## Game Overview

- **게임명**: DDworld — 현 작업명 「마왕성 인사팀」(가제)
- **장르**: 다크판타지 코미디 HR 시뮬 (Papers, Please 계열 판단·심문 + 다이어제틱 데스크)
- **핵심 경험 / 후크**: 악당을 뽑는 인사팀이라 **판단축이 비틀린다** — 거짓말·잔인함이 (직무 JD에 따라) 장점, "알고 보니 착함"이 위험 신호. "세상을 한번 꼬아서 본다" + 가젯으로 업무 스코프가 늘어나는 성장.
- **코어 루프**: 지원 몬스터 등장 → 질문 카드로 심문 → 서류·진술·반응 대조로 거짓 간파 → 도장으로 판단(JD 적합 합·불).
- **핵심 원칙 = 물성**: UI 클릭이 아니라 책상 위 물건(도장·이력서·질문카드·서랍·촛불·돋보기)을 직접 잡고 만진다.
- **플랫폼**: PC (Steam) · **개발 규모**: 솔로 인디
- **멀티플레이어**: 없음 (싱글플레이). *비동기 PvP는 폐기 — [ADR-001](docs/architecture/ADR-001-async-pvp.md) Superseded.*

## Visual Style

**전체 = Low-poly 3D 모델링 — 캐릭터는 복셀풍(블로키) 룩** (스타일 근거 = [ADR-002](docs/architecture/ADR-002-visual-style-low-poly-3d.md), 단 아래대로 갱신. ⚠️ 2026-08-18: 캐릭터 제작 방식 복셀 → 복셀풍 로우폴리로 변경)

- **캐릭터(몬스터·플레이어 손)**: **복셀풍 로우폴리 메쉬** — 네모네모한 복셀 룩은 유지하되, 복셀 단위 제작이 아니라 블렌더 박스 모델링 (MagicaVoxel 폐기, 2026-08-18).
- **환경(책상·사무실·소품)**: Low-poly 3D 메쉬 — 캐릭터와 동일 파이프라인 (블렌더 단일).
- **톤**: 촛불 켜진 마왕성 사무실 — 따뜻+어두운 대비, "귀여운데 사악한" 대비. **코지 미니어처/틸트시프트 디오라마 톤은 미승계**(옛 헥사 전투 게임 기준).
- **애니메이션**: 면접 리액션 2종(긴장/안도) + 이펙트 수준. *전투용 부위분리 풀 리깅·400명 크라우드 렌더링([ADR-003](docs/architecture/ADR-003-rigid-instancing-crowd-rendering.md))은 폐기.*
- **카메라**: 고정 데스크 클로즈업.
- **레퍼런스**: Papers, Please · Strange Horticulture(다이어제틱 데스크). 화면 목업 = [ideation/refs/면접화면-목업.png](ideation/refs/면접화면-목업.png).

## Technology Stack

- **Engine**: Unity **6000.5.1f1** (Unity 6.5 — 2026-07-02 실측. 기존 프로토타입·Origin 새 프로젝트 동일 버전)
- **Language**: C#
- **Rendering**: URP (Universal Render Pipeline) — 고정 데스크 뷰 3D + Post-processing (Bloom/DOF/비네트)
- **3D 모델링**: Blender 로우폴리 단일 파이프라인 — 캐릭터(복셀풍 블로키 룩)·환경·소품 전부 (MagicaVoxel 폐기, 2026-08-18)
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
