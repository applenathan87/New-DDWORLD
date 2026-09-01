# Work Queue — DDworld

> **Purpose**: 세션 간 영구 추적 작업 큐. (TodoWrite는 세션 한정 → 영구 추적은 이 파일.)
> **Last Updated**: 2026-09-02

---

## ⚠️ 이 큐 전체가 STALE (2026-07-03) — 재작성 대기

> 아래 "옛 큐" 항목은 **폐기된 옛 컨셉(1세대 PvP 심리전 / 2세대 PvE 헥사 오토배틀러) 기준**이다. 실행 금지.
> **현행 코어 = 「마왕성 인사팀」** (악당 면접 HR 시뮬). 옛 컨셉 인덱스 = [design/gdd/_archive/README.md](../design/gdd/_archive/README.md)

## 🎯 지금 할 일 (TODO) — 여기만 읽으면 됨

> "todo 찾아봐" = **이 블록만** 읽기.

- **[최우선] 문서 정합성 복구 — 본격 개발 전 필수 (2026-09-02 결정)**
  진단: 폐기 문서(art-bible·복셀 research·technical-preferences·game-designer 에이전트/메모리)가 현행 트리·자동 로드 경로에 남아 복셀/전투 맥락이 재유입됨. 결정이 본문 수정 없이 헤더 주석으로 누적되어 본문이 옛 결정을 따름. 면접 뎁스는 문서 4개(mvp-design §7 / interview_idea v0.3 / interview-depth / interview-catch)가 미조정 상태.
  1. **격리** — "현행 트리엔 현행만". **자동 로드되는 하네스(CCGS) 설정 파일부터** 현행 기준으로 수정 — 하네스가 아니라 컨셉 전환 때 안 고친 설정이 복셀·전투 맥락 재유입 통로. art-bible·복셀 research·옛 큐는 `_archive/`로 이동
     - `.claude/docs/technical-preferences.md` (CLAUDE.md에 `@` 임포트 → 매 세션 로드) — L9 "쿼터뷰 3D", L16 "카드 드래그 & 그리드 배치", L34 "쿼터뷰 2.5D", L41 "RPS 상성·틱 기반 전투·루트 계산·덱 셔플" 테스트 요건 → 데스크 클로즈업·물성 드래그·면접 판정 기준으로 교체
     - Claude 전역 메모리 `~/.claude/projects/c--New-DDWORLD/memory/MEMORY.md` — "⭐ 코어 전환: PvE 헥사"·async_pvp·ADR-003 항목이 현행처럼 남음 → 폐기 표시 + 「마왕성 인사팀」 항목 추가
     - `.claude/docs/templates/game-concept.md` — 헥사 예시 잔존 (경미)
     - ✅ 이미 정상: `agents/game-designer.md`·`agent-memory/game-designer/`(현행 반영됨), `.claude/rules/`(전투 전제 없음) — 2026-09-02 점검
  2. **결정 원장** `design/decisions.md` 신설 — 각 문서 헤더에 흩어진 확정 사항을 "날짜·결정·폐기한 것·근거" 한 줄씩 집약 (유일한 정본). 이후 규칙: 결정 변경 = 원장 1줄 + 해당 문서 **본문** 수정, 옛 내용은 주석 대신 아카이브
  3. **면접 뎁스 결정** — 덱빌딩(v0.3) vs 캐치(v0.1) 관계 확정. 이게 안 되면 면접 GDD 작성 불가
  4. **GDD 새로 쓰기** — 원장 + ideation 재료로 `design/gdd/`에 확정 시스템만 8섹션 작성(코어 루프·판정/결산·진행/가젯·밤 파트·면접). 승격된 ideation 문서는 "참고용, 정본 아님"으로 동결
  5. **아트 바이블 재작성(짧게)** — 복셀풍 로우폴리 + 핸드페인팅 텍스처 여부 결정 후 20~30줄
  → 1·2는 Claude가 changeset 초안 → 사용자 승인 후 실행. 3은 사용자 결정. 이 work-queue 파일 자체도 1단계에서 재작성 대상.
- **[다음] 도구 연동 — 문서 정합성 복구 뒤 (2026-09-02 결정)**
  1. **codex-plugin-cc 연동** (OpenAI 공식, [github.com/openai/codex-plugin-cc](https://github.com/openai/codex-plugin-cc)) — 용도 = **교차 리뷰**(작성은 Claude, 검토는 Codex). 첫 시험 = GDD 초안에 `/codex:adversarial-review`. 전제: ChatGPT Plus 구독 + Node 18.18+. 설치: `/plugin marketplace add openai/codex-plugin-cc` → `/plugin install codex@openai-codex` → `/reload-plugins` → `/codex:setup`. ⚠️ `--enable-review-gate`는 끄고 시작(Claude↔Codex 루프로 양쪽 한도 소진 경고). Codex엔 프로젝트 맥락이 없으므로 결정 원장(2번)을 같이 넘길 것.
  2. **Unity MCP 검토** — Unity 6.5 호환 확인 후 Origin 트랙에서 시험. 용도 한정 = 컴파일 에러·콘솔·테스트·플레이 모드만 (씬 조작 X). 도입 시 `/context`로 세션 시작 점유량 측정 후 유지 여부 결정.
  3. **CCGS review-mode** — `production/review-mode.txt`에 `solo` 생성 (현재 파일 없음 → 기본 lean).
- **[진행중] MVP 프로토 스프린트 (7/4~7/7)**: [docs/mawang-hr-proto-brief.md](../docs/mawang-hr-proto-brief.md) — S1 완료, S2(도장 물성+면접 루프)부터 계속
- **현행 학습**: [Origin/roadmap.md](../Origin/roadmap.md) (사용자 개인 진행 — 유니티 친해지기)
- **현행 기획**: [ideation/mvp-design.md](../ideation/mvp-design.md) — 프로토 검증 결과 반영 → 확정 시 `design/gdd/` 승격
- 아래 옛 큐(헥사·PvP)는 **전부 폐기** — 새 컨셉 기준으로 이 파일을 언젠가 재작성할 것.

---

## 🗄️ 옛 큐 (폐기 — 히스토리 참고용, 실행 금지)

> 이하 원문 보존. 헥사·PvP·판돈·400명 전투 = 죽은 맥락. 링크된 pve-pivot.md 등은 `_archive/`로 이동됨.

### (구) 최우선 (2026-06-29) — 코어 전환: PvE 헥사 영토 확장

0. **PvE 전환 Q 리스트 해결** — (폐기) 옛 시작점은 pve-pivot.md(현 `design/gdd/_archive/02-pve-hex/pve-pivot.md`)였음. 헥사 컨셉 자체가 폐기되어 무효.

---

## ⏸️ Phase 2 — 신규 GDD 작성 (보류 — PvE 전환 후 재정의)

> 아래는 옛 PvP/트리맵 구조 기반. 전환 Q 해결 시 헥사/영토/건물/출정 구조로 재작성됨.

### Q1~Q6 결정 (2026-05-25) 으로 발생한 신규 시스템 GDD 작성 필요

1. **`design/gdd/run-mode.md` 작성** ⭐
   - 런 구조: 시작 → 트리맵 → 노드 진행 → 종료
   - 노드 종류 5개: PvP 매치 / 크립(PvE 단일) / 상점 / 이벤트 / 보스(?)
   - 트리맵 구조 (Slay 스타일, 위로 진행, 분기 선택)
   - HP 시스템 (런 도중 패배 시 HP 감소, 0 시 런 종료)
   - 종료 조건: PvP X승 도달 = 클리어 / HP 0 = 패배
   - 8섹션 GDD 표준 준수
   - 권장 도구: `/design-system` 스킬

2. **`design/gdd/shop.md` 작성** ⭐
   - 카드 추가: 덱에 새 카드 1장 추가
   - 카드 강화: 기존 덱 카드의 능력치 ↑
   - 패시브 특성: 영구 효과 (예: "창병 모두 HP +5")
   - 통화: 판돈 → 런 통화 환산 정책 (1:1? 배수?)
   - 가격 책정
   - 상점 UI 흐름 (등장 옵션 수, 거절 옵션 등)
   - 8섹션 GDD 표준 준수

### Q7 — 영웅 시스템 결정 ⭐

3. **영웅 시스템 도입 여부/시점 결정**
   - 사용자가 "개념부터 파악 필요"라고 함
   - 이전 세션에서 개념 설명 완료 (Slay/Bazaar/Hearthstone/TT 비교)
   - 도입 옵션:
     - A. MVP부터 3명 (빌드 서사 완성도 ↑, 스코프 폭증)
     - B. MVP는 단일 영웅, Beta에 3명 (균형)
     - C. MVP에 영웅 없음, Beta에 본격 도입 (가장 단순)
   - 결정 후: 결정 시 `design/gdd/hero-system.md` 작성, systems-index 업데이트

---

## 🟡 추가 누락 GDD (MVP 완전성)

4. **`design/gdd/ui-systems.md` 작성**
   - 배치 UI, 판돈 UI, 손패 UI, 트리맵 UI
   - 가장 시급한 누락 시스템

5. **`design/gdd/ai-opponent.md` 작성**
   - 비동기 PvP 진입 게이트
   - 튜토리얼 + 신규 플레이어 보호 매칭

6. **`design/gdd/async-pvp.md` 작성**
   - 고스트 데이터 구조
   - 매칭 로직 (ELO/MMR)
   - 신규 플레이어 보호 정책

---

## 🟢 검증 / 보강 작업

7. **3판 2선승 변경 후 밸런스 검증**
   - 5판 대비 매치 깊이 변화
   - 1패 시 압박감이 적절한지
   - 프로토타입 플레이테스트로 확인

8. **민병대 밸런스 검증** (D3-W3, [04-23 리뷰](../design/gdd/reviews/gdd-cross-review-2026-04-23.md))
   - 총 DPS 80 + HP 160 = 단독 1위
   - 패닉 메카닉이 충분한 페널티인지

9. **기권 전략 유효성 검증** (S2-W1)
   - "약한 카드" 후보 식별 (궁병 5장 등)

10. **Warning 해소** (04-23 리뷰)
    - combat.md 배치 제한 시간 확정 (D3-W1)
    - combat.md Game Feel 섹션 보완 (D3-W2 — "보는 맛" Pillar)

---

## 🎨 아트 / Voxel 에셋 파이프라인

13. **첫 Voxel 캐릭터 — MagicaVoxel→Blender 리지드 애니 작업 시 유의점 적용·검증**
    - ⭐ **[ADR-003](../docs/architecture/ADR-003-rigid-instancing-crowd-rendering.md) 규칙 위에서 진행**: 리지드 바인드(웨이트 페인팅 X) + 정점색 1머티리얼 + VAT 안 씀. 인스턴싱/정점색/팀틴트를 Frame Debugger·Profiler로 검증(ADR-003 §Validation)
    - 기술 파이프라인: [blender-to-unity.md](../docs/pipeline/blender-to-unity.md) (Draft) 따라가며 §8에 발생 이슈 기록
    - 모델링 원칙(greeble 최소화): [voxel-greebles.md](../design/art/research/voxel-greebles.md)
      (45° 스테핑 자제 / 팔레트 1+1 / 부위 분리 / 평면화+애니로 디테일 / 그림자 hue·밝기 대비)
    - 리지드 분절 런·어택 애니(12~24fps 스텝) 실제 제작
    - 확인: 크기/개수 성능 한계, Vertex Color 색 표시, GPU Instancing draw call

14. ✅ **Voxel 학습 아티클 4편 정독** (개념 습득) — 2026-06-29 완료
    - @Voxels(Zach Soares) 시리즈: greebles / styles / blocky / marching-cube
    - 전문 번역 3편 완료: [greebles-full](../design/art/research/voxel-greebles-full.md) · [styles-full](../design/art/research/voxel-styles-full.md) · [blocky-full](../design/art/research/blocky-voxelart-full.md)
    - marching-cube는 브리핑만(우리 방향 아님, 기존 요약본 유지)
    - 결과: 스타일 포지션(Vector~Blocky) + 모델링 원칙 내재화 → ADR-003 결정의 기반

15. **유사 아티클 검색·습득** (지속) — 2026-06-29 두 파트로 분리
    - **파트1: 복셀 캐릭터 디자인 방법론** (← 현재 집중)
      - 비율/실루엣/해상도/팔레트/가독성 + 부위 분리 전제 설계
      - ⭐ **성능 방법론**: 귀여움 유지하며 캐릭터를 최대한 많이 띄워도 무거워지지 않는 설계
        (저 voxel 수, 공유 머티리얼/팔레트, GPU Instancing, LOD, draw call 예산, 메쉬 최적화, 리지드 본 = 저비용 애니)
    - **파트2: MagicaVoxel 파트 분리 export** (보류, 나중에)
      - Head/Body/Arms/Legs/Weapon 별 오브젝트/그룹 분리, 피벗 유지, 정점색 보존, Blender 재분리
      - work-queue 13번(첫 리지드 애니)의 직접 전제 조건
    - 발견 시 design/art/research/ 에 갈무리 → 토론

---

## 🔵 아키텍처 / 인프라

11. **`/create-architecture` 시작 가능** — Blocking 없음, ADR-001·002·003 채택됨

**채택된 ADR**: [ADR-001](../docs/architecture/ADR-001-async-pvp.md) 비동기 PvP · [ADR-002](../docs/architecture/ADR-002-visual-style-low-poly-3d.md) Low-poly Voxel · [ADR-003](../docs/architecture/ADR-003-rigid-instancing-crowd-rendering.md) 리지드 인스턴싱 대량 렌더/시뮬

12. **향후 ADR 후보** (번호 미정 — 작성 시 다음 번호 부여):
    - 매치 데이터 직렬화 포맷 (Protobuf vs JSON vs MessagePack) — ADR-003이 병사 데이터 구조 기반 제공
    - 인프라/BaaS 선택 (Firebase 등)
    - 신규 플레이어 보호 정책 (AI 매칭 비율, ELO 보정)
    - 고스트 데이터 갱신 정책

---

## ⚫ Open Questions (결정 필요)

- **매치 종료 시 카드 복귀 정책** (deck.md) — run-mode.md 작성 시 확정
- **판돈 → 런 통화 환산 정책** (economy.md) — run-mode/shop GDD에서 확정
- **상점 카드 추가/강화 가격 책정** — shop.md
- **PvP 매치 X승 = 클리어**의 X값 — run-mode.md
- **트리맵 깊이** (몇 층, 몇 노드?) — run-mode.md
- **HP 시스템 상세** (시작 HP, 매치 패배 시 -? HP) — run-mode.md

---

## 📌 영구 진행 사항 (참고)

- ADR-001: 비동기 PvP MVP 채택
- Pillar 2 교체: "역전의 희열" → "내 빌드의 서사 (선택/성장/발견)"
- Q1~Q6 결정 (2026-05-25): 매치 3판 2선승, 트리맵 + 노드, 런 단위 덱, 크립 단일 전투, HP+X승 종료

---

## 운영 방법

- 작업 완료 시 해당 항목 체크 또는 삭제
- 신규 작업 발생 시 적절한 섹션에 추가
- 우선순위는 🔴(긴급) → 🟡(필요) → 🟢(검증) → 🔵(인프라) → ⚫(질문)
- 세션 시작 시 이 파일을 확인하여 다음 작업 결정
