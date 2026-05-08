## Session Extract — /review-all-gdds 2026-04-23
- Verdict: PASS (CONCERNS) — 이전(2026-04-16) FAIL → PASS로 승격
- GDDs reviewed: 3 (combat, deck, economy)
- Previous blocking resolved: C-01, C-02, D-01 모두 해결 확인
- Inline fix applied: Phase 4에서 S2-a (배치 장수 하한선 미정의) 발견 → combat.md (Core Rules 3/4, Edge Cases) + economy.md (Strategic Dynamics, Edge Cases, AC) 7개 섹션 수정으로 해결
- New design decision: 배치는 5장 고정. 0~4장 시 AI 자동 채움. 기권 전략 = "약한 카드 5장 배치"
- Flagged for revision: combat.md (D3-W1 배치 시간, D3-W2 Game Feel), economy.md (D3-W4 패자 보상), deck.md (C2-W1 stale ref)
- Systems index: 신규 생성 — design/gdd/systems-index.md
- Recommended next: /create-architecture 시작 가능 (Blocking 없음)
- Report: design/gdd/reviews/gdd-cross-review-2026-04-23.md

## Session Extract — BM Research Note 2026-04-23
- Created: design/research/monetization.md (Draft, Not Decided)
- Context: 클래시 로얄 BM 분석 → DDworld BM 방향 고민
- Excluded: 병력 카드 판매 / 상자 / 가챠 (Anti-Pillar 충돌로 배제)
- Leading option: Option B (Premium $15~20 + Cosmetic DLC) + Option D (연출 판매)
- Decision deferred until: 프로토타입 리플레이성 / 매치 길이 검증 후
- Promotion path: 확정 시 design/gdd/monetization.md로 승격

## Session Extract — Replayability & Synergy Research 2026-04-23
- Created: design/research/replayability-and-synergy.md (Draft, 방향 결정)
- Context: 반복 피로도 걱정 → 카드 강화 배제 → 위치 시너지 + 포커 핸드 + 이월 시스템 결합
- Decision: 방향 B 채택 (MVP 4개 콤보 → Alpha 8~9개 확장)
- Combo draft: 병종별 2개 + 복합 1개 = 9개 (팔랑크스, 쐐기, 집중사격, 군중사기, 방진, 일렬돌격, 일제사격, 인해전술, 풀하우스)
- Key insight: 이월 3장이 "찌꺼기"에서 "콤보 시드"로 격상 → 심리전 레이어 확장
- Anti-Pillar 해석 명문화: Input vs Output Randomness 구분 (§10 부록)
- Promotion path: Phase 1 검증 후 combat.md (시너지 규칙) + deck.md (이월 전략) 업데이트
- Prototype priority: H1 (콤보 없이도 재미있는가) 먼저 검증

## Session Extract — Deck Building Addition 2026-04-23
- Added to: design/research/replayability-and-synergy.md §11, §12, §13
- Decision: Option B (매치 전 덱 공개) + Option C (병종 5~15장 제약) 조합
- Structural advantage: 제약상 최대 2병종까지만 특화 가능 (3병종 올인 수학적 차단)
- 7 deck scenarios analyzed: 균형/기병/창병/궁병/민병/함정/2병종올인
- 매치업 매트릭스: 지배 전략 부재 확인, 단 궁병덱 상대적 강세 (검증 필요)
- Psychological layers: 4레이어로 확장 (덱 메타 → 손패 → 이월 → 그리드)
- Anti-Pillar 수정 방향: "복잡한 경영" → "무거운 경영" (§12)
- 허용 (가벼운 메타): 덱 비율 조정. 배제 유지 (무거운 메타): 카드 수집/강화/레벨링/뽑기
- 후속 논의 예정: §13 카드 획득 방식 (상자 vs 직접 구매 vs 드래프트)
- Next conversation: 질문 B (카드 획득 방식) 재개

## Session Extract — Card Acquisition: Box Lottery 2026-04-27
- Updated: design/research/replayability-and-synergy.md §13 (확정)
- Decision: 상자 뽑기 채택 (인게임 재화 한정, 외부 결제 없음)
- Key design: 플레이어가 우선순위 선택 → 확률 가중치 부여 (순수 랜덤 아님)
- Reasoning: PvP 장기 운영 + 종족 확장 대비 + 메타 진행 루프 필요
- Anti-Pillar 호환: 매치 내 운(Input Randomness)이 아닌 "메타 진행 운"으로 분류
- D-01 해결: 판돈 = 점수 + 상자 구매 통화로 사용처 확정
- Pending spec items: 상자 티어/가격, 확률 가중치 수치, 중복 처리, pity 시스템, 신규 온보딩
- Anti-pattern 회피: 시간잠금/외부결제/카드레벨업/FOMO 한정판 모두 배제

## Session Extract — Visual Polish Plan 2026-04-27 (Day 28 후속)
- Created: design/art/visual-polish/ 폴더
  - README.md (인덱스 + 작업 컨텍스트)
  - hd2d-setup-plan.md (마스터 플랜, Phase 1~4)
- Context: 게임 룩이 "단순한 픽셀 게임" 같음 → HD-2D 정체성 부재 진단
- Diagnosis: 렌더링 파이프라인 셋업 부재 (코드 문제 아님)
- Approach: 옵션 C (현재 씬 점진적 폴리시) 선택 — 옵션 A(새 프로젝트) / B(새 씬) 검토 후 배제
- 4-Phase plan:
  - Phase 1 (1~2일): URP Volume + Post-processing (Bloom/DOF/Color/Vignette/Tilt-shift)
  - Phase 2 (1~2일): 조명 시스템 (Directional Light, 그림자, 시간대)
  - Phase 3 (2~3일): 셰이더 폴리시 (잔디 흔들림, 거리 페이드)
  - Phase 4 (2~3일): 환경 디테일 (파티클, 오브젝트, 카메라 셰이크)
- Reference target: Sea of Stars 수준 (절제된 옥토패스)
- Code refactoring: 거의 불필요 (셋업 + 셰이더 추가만)
- Status: 계획 단계 — 실제 코드/Unity 변경 0
- Next session: hd2d-setup-plan.md 검토 후 Phase 1 진입 결정 OR 다른 작업 우선순위 (함정 배치 룰)
