---
title: M01 GDD v1 확정 — 코어 시스템 7개
start: 2026-10-01
due: 2026-10-31
status: planned
done_when: 시스템 인덱스 1에서 7번 GDD가 design-review(lean)에서 APPROVED이고 review-all-gdds를 통과했다
---

# M01 — GDD v1 확정 (코어 시스템 7개)

> 기간 2026-10-01 → 10-31 (새 시즌 Day 1부터) · 상태 **예정** · 이전 = [M00 준비](M00-prep.md) · 다음 = M02 GDD v1.1 (스케줄링·밤 허브·미니게임 + 프로토)
> **완료 조건**: [systems-index.md](../../design/gdd/systems-index.md) 1에서 7번 GDD가 `/design-review --depth lean`에서 APPROVED이고, `/review-all-gdds`를 통과했다.
> 상위 문서: [game-concept.md](../../design/gdd/game-concept.md) · 순서·범위 = systems-index.md 권장 설계 순서

## 백로그 (순서 = 설계 순서)

- [ ] 01 케이스 데이터·콘텐츠 파이프라인 GDD (M) — 02와 같이 설계(스키마 ↔ 정답 모델). 완료 = 8섹션 채움
- [ ] 02 채용 지침 누적 GDD (M) — 날짜별 지침 풀, 유지·갱신·소멸, 케이스 × 지침 정답 모델(열린 질문 #11 결정)
- [ ] 03 판정(마킹·도장·가젯) GDD (L) — 보류 도장·스탯 패널·마킹 노브·돋보기 제한 결정 포함
- [ ] 04 데스크 물성·인터랙션 GDD (M) — Game Feel 섹션 필수(도장·카드 손맛)
- [ ] 05 하루 구조·결산 GDD (M) — 근무 시간·추가 점수·재도전 없음·게임오버 패턴·지연 사고
- [ ] 06 승진·경제 GDD (M) — 공적 공식·직급·일급·골드 소비처
- [ ] 07 1차 면접(카드·코스트) GDD (L) — 손패·턴당 코스트·진술 마킹·결정타 분산·함정 질문 비용
- [ ] 08 GDD 검토 7건 (각 S) — 완료된 GDD마다 **새 세션**에서 `/design-review [파일] --depth lean` → systems-index 상태 열 갱신
- [ ] 09 `/review-all-gdds` (S) — 7개 완료 후 교차 검토
- [ ] 10 mvp-design·night-part 갱신 (S) — GDD로 승격된 내용은 원문에 "→ GDD" 표시, 남는 것만 유지

## 운용 규칙

- GDD 하나 = 세션 하나 이상. **각 GDD는 새 세션에서 시작**하고, 컨텍스트 70% 이상이면 섹션 경계에서 세션을 바꾼다(승인된 섹션은 파일에 있으므로 이어진다).
- 질문은 텍스트로(버튼 UI 금지). 전문 에이전트는 공식·엣지 케이스·인수 기준처럼 값이 필요한 섹션에만.
- 8섹션 표준: Overview · Player Fantasy · Detailed Rules · Formulas · Edge Cases · Dependencies · Tuning Knobs · Acceptance Criteria (+ Game Feel은 물성·판정·면접에 필수).
- 하루 조합 = 이어가기 1 + 마일스톤 항목 1 + 여유 1. L 항목은 하루에 하나.

## 비고

- 2차(M02) = 08 스케줄링 퍼즐 · 09 밤 파트 허브 · 10 리듬·QTE 미니게임 GDD + 미니게임·미니맵 프로토. 데모 범위에서 빠지는 게 아니라 순서만 뒤.
- 진행: 0 / 10
