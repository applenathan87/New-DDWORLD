# Combat System — Review Log

## Review — 2026-04-15 — Verdict: NEEDS REVISION
Scope signal: L
Specialists: 없음 (lean 모드)
Blocking items: 5 | Recommended: 4
Summary: 전투 진행 방식(틱 시스템, 행동 패턴, 데미지 적용)이 미정이었으나, 리뷰 세션에서 대부분 해결됨. 틱 기반 동시 행동 + 연속 이동 모델, 개인 데미지, 5병종 행동 패턴 확정, 동시 전멸 처리 확정. 민병대/함정 상성과 궁병 사거리, 돌격 보너스 수치는 프로토타입에서 조율 예정.
Prior verdict resolved: 첫 리뷰

### Blocking items resolved in-session:
1. 전투 진행 방식 → 틱 기반 동시 행동, 연속 이동 확정
2. 데미지 적용 방식 → 개인 데미지 모델 확정
3. 5병종 행동 패턴 → 전부 확정 (Battle Simulation 섹션)
4. 동시 전멸 처리 → 양쪽 모두 1승 획득
5. 민병대/함정 상성 → 보류 (프로토타입 후 결정)

### Remaining open items:
- 배치 제한 시간
- 그리드 배치 제한 규칙
- 궁병 사거리
- 돌격 보너스 수치 (charge_multiplier, charge_threshold)
