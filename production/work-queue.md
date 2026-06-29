# Work Queue — DDworld

> **Purpose**: 세션 간 영구 추적 작업 큐. TodoWrite는 현재 세션 한정이라, 다음 세션에도 이어질 작업은 여기에 기록.
> **Last Updated**: 2026-06-27

---

## 🔴 Phase 2 — 신규 GDD 작성 (다음 우선순위)

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
