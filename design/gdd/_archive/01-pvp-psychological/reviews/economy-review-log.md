# Economy System — Review Log

## Review — 2026-04-15 — Verdict: NEEDS REVISION
Scope signal: S
Specialists: 없음 (lean 모드)
Blocking items: 3 | Recommended: 3
Summary: base_value 체계, 판돈 용도, 동시 전멸 판돈 처리가 미정이었으나 리뷰 세션에서 해결됨. 분대당 균등 가치 모델(squad_value=10) 확정, 판돈을 덱 강화 통화로 결정, 동시 전멸 시 판돈 0 적립으로 확정. 패자 보상과 덱 강화 상세 내용은 프로토타입 후 결정.
Prior verdict resolved: 첫 리뷰

### Blocking items resolved in-session:
1. base_value → 분대당 균등 가치 모델 (squad_value=10)
2. Expected output range → 0~50 (최대 5분대 풀 생존)
3. 판돈 용도 → 덱 강화 통화 (MVP 메타 성장)

### Design decisions made:
- 분대당 균등 가치: 어떤 병종이든 풀 분대 = 10 판돈 기여
- 판돈 = 덱 강화 통화 (매치 간 메타 성장)
- 동시 전멸 시 판돈 적립 0
