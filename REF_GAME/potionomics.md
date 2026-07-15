# Potionomics (포셔노믹스)

> 조사일: 2026-07-15 · 용도: **면접 덱빌딩 설계 레퍼런스** ([ideation/interview_idea.md](../ideation/interview_idea.md)) — 다른 4종(밤 파트용)과 조사 목적이 다름.
> 12+ 출처 교차 검증 (공식 위키 API 원문·개발자 인터뷰·가이드 4종·리뷰 4종·스팀 커뮤니티 스레드 3종·공식 발표). 단일 출처 정보는 ⚠️ 표시.

## 개요

- **한 줄 소개**: 빚더미에 앉은 마녀 실비아가 포션을 양조해 상점에서 팔며, 판매가를 **카드 배틀 "흥정"**으로 끌어올리는 덱빌딩 × 상점 경영 × 소셜 시뮬.
- **장르**: Deckbuilder / Shop Management / Visual Novel(소셜·로맨스) 하이브리드
- **개발사·출시**: Voracious Games / XSEED Games — **2022-10-17** (PC). **Masterwork Edition** 2024-10-22 (콘솔 발매 + PC 무료 업데이트)
- **Steam**: https://store.steampowered.com/app/1874490/Potionomics/ — 매우 긍정적 (영어 리뷰 5,086건 중 90%) · 메타크리틱 80 · 2023 IGF 관객상
- **면접 덱빌딩 참고 포인트**: "카드 덱빌딩 + 상대 상태 게이지 + 설득 대화"의 대표 사례. 우리 면접(질문 카드 + 긴장 온도계 + 질문 포인트 3)과 구조가 1:1로 비교됨 — 특히 **드로우 RNG의 실제 체감**과 **"덱이 정답기가 되는" 붕괴의 실증 사례**.

## 흥정 미니게임 구조

- **전체 흐름**: 포션 진열 → 상점 오픈(하루 6타임슬롯 중 2슬롯) → 손님 순차 등장, 1명당 1회 카드 흥정. 포션 **기본가는 흥정 전에 결정**(품질·트레잇·이벤트)되어 있고, 흥정은 기본가에 **배율을 얹는** 행위. ([Fandom: Potion Selling](https://potionomics.fandom.com/wiki/Potion_Selling))
- **턴 구조**: 플레이어 ↔ 고객 턴 교대. 시작 손패 3장. ⚠️ 매 턴 드로우 수와 턴 종료 인내 비용이 각 1씩 증가 — 턴이 길어질수록 손은 커지고 시간이 비싸짐 (Sirus Gaming 단독 기재).
- **자원 = 인내(Patience) 단일화** ⭐: 별도 에너지 없음. **카드 코스트(0~3)도, 턴 종료 비용도, 흥정의 남은 시간도 전부 인내** — 셋이 같은 자원. 고객마다 시작 인내가 다름 (11~12 수준).
- **관심(Interest) 게이지**: 하트 4~5개, 고객마다 하트당 필요 포인트("마일스톤")와 기본 관심이 다름. 하트를 채울 때마다 가격 배율 임계 상승. ⚠️ 하트별 정확한 배율표는 어느 출처에도 완전 정리 없음.
- **종료 3경로** ⭐: ① **Closer 카드** = 보너스(+5%↑)와 함께 마감 ② **Close Deal 버튼** = 아무 때나 현재 배율로 안전 확정 (보너스 없음) ③ **하트 만렙 = 자동 성공·최대 가격** (인내가 음수여도 무관 — 판정은 카드 해소 후). **실패** = 만렙 전 인내 0 → 고객 이탈, ⚠️ 위키는 "포션 폐기"까지 명시 (단독).
- **Opener/Closer 위치 문법** ⭐: 턴의 첫 카드로 낼 때만 보너스 발동(Opener) / 마감 전용(Closer). 개발자: "실제 세일즈 대화의 구조(후킹→본론→클로징)를 카드 위치 문법으로 반영". ([Game Developer 인터뷰](https://www.gamedeveloper.com/business/how-potionomics-turned-price-haggling-into-a-card-game))

## 스트레스 시스템 (플레이어측 리스크 게이지)

- 흥정 중 고객의 공격(디버프)으로 증가. **자동 감소 없음, 흥정·날짜를 넘어 지속** (대회장까지 따라감).
- **작동 = 드로우 오염 확률**: 스트레스 % = "카드 1장 뽑을 때마다 그 카드가 스트레스 카드(회색·플레이 불가·잔류 페널티)로 변할 확률". 높은 구간일수록 더 악질 카드 (40%+에서 장당 +6~8%짜리).
- **100% = 블랙아웃**: 그날 강제 종료 (다음날 0 리셋). 대회 중이면 라운드 패배.
- 해소 = 수면(-10%~)·친구와 놀기(슬롯당 -5~-50)·특정 카드 — **스트레스 관리가 흥정 밖 시간 경제와 직결**.
- **구조 문제** ⭐: 확률이 확률을 낳는 지수 나선 — 스트레스 카드가 스트레스를 올리고, 오른 스트레스가 더 많은 오염을 뽑게 함. 커뮤니티: "의도된 죽음의 나선", "30~40%면 사실상 게임 오버", "5%인데 2장이 동시에 — XCOM보다 나쁨". ([스팀 토론](https://steamcommunity.com/app/1874490/discussions/0/3488628582211206265/))

## 덱빌딩 층

- **획득 = 캐릭터 호감도 랭크업이 유일한 공급원** (상점 구매 없음, Owl만 대회 보상 예외). 캐릭터당 8종을 랭크 1·2·3·5·6·7·9·10에서 지급. 개발자가 **Persona 소셜링크 차용**을 명시 — "흥정으로 돈 → 선물 → 관계 → 새 카드"의 순환. ([TechRaptor 전체 카드 목록](https://techraptor.net/gaming/guides/potionomics-card-unlock-guide))
- **덱 규칙**: **정확히 20장** 고정. 중복 = 보유 수량만큼 (Chorus는 4장 일괄 지급 → 4장 투입 가능). Stance는 동시 1개 활성 (⚠️ 덱 투입 제한인지 활성 제한인지 출처 엇갈림). **카드 업그레이드 없음** — 고랭크에서 상위 카드를 받는 구조.
- **드로우 RNG 체감** ⭐ (우리 "드로우 기각" 판단의 검증 대상):
  - 불만측: 스트레스 오염 뭉침·"Close Deal 카드가 첫턴에 몰림" 등 드로우 불운 불만 다수.
  - 옹호측: "통계 오해일 뿐" 정도 — **드로우 RNG 자체가 재미라는 옹호는 조사 범위에서 발견 못 함**.
  - **핵심 관찰**: 상급 가이드가 전부 **드로우 엔진(Improv·Scheme)으로 RNG를 소거하는 방향**으로 수렴 — 숙련 = "게임이 준 무작위를 없애는 것"이었다는 방증. ([xanthir 최적 덱 분석](https://www.xanthir.com/b5N40))

## 시너지·콤보·고객 변주

- **카드 문법 분류** (약 80종 전체가 시스템 효과, 카드 고유 대사 없음): 관심 즉발 / 지속턴 / 증폭 버프 / 가격 직접 조작(Flattery +15%) / 인내 생성 / 코스트 조작 / 드로우 / 쉴드 / 스트레스 해소 / **자해 리스크 전환**(Wing It: 관심 +12·스트레스 +5) / 양자택일 / 스탠스 / Opener / Closer.
- **대표 콤보**: 버프 스택 → 대형 카드 (Pump Up +50% → Enthusiasm 16) / 가격 % 겹치기 (Flattery + Closer = +20%↑) / 다타 스케일링 (카드 칠 때마다 +2~3 × 0코스트 스팸).
- **파훼 덱 (사실상의 최적해)** ⭐: **Chorus 엔진** — Chorus(칠 때마다 +5 누적 성장) 4장 + 드로우 + Rhythm(코스트 -1) → "다음 턴이 사실상 무한" → 90% 만렙. **Jingle 음수 코스트 버그** — 코스트 -2가 0코스트 카드에서 음수로 새어 **인내가 역생성** → "고객이 영원히 턴을 못 가짐". ([Infinity Deck 가이드](https://steamcommunity.com/sharedfiles/filedetails/?id=2879007152), [KosGames](https://kosgames.com/potionomics-roflstomp-haggling-deck-33059/))
- **고객 유형 변주**: 존재 — 고객 = 인내·하트·기본 관심 + 고유 능력 2~3개 재조합 (Stubborn: 관심 획득 -25%, 쉴드 시작, 매턴 스트레스 +2, 가격 하락 등). "쉴드엔 Disarm"식 읽기가 성립. **그러나 저항이 약해서 범용 굿스터프 덱 하나가 전 고객을 관통** — 덱을 갈아끼울 이유를 만드는 데 실패.

## 루프 통합

- 하루 6슬롯 × 약 50일, **10일마다 대회(보스 흥정)** 5회. 오픈당 흥정 5~7건 연속.
- **대회 = 보스 흥정**: 3판 2선승, 내 포션 가치(기본가+흥정)가 상대를 넘으면 승리. 보스는 전용 디버프 + **전용 스트레스 카드를 내 덱에 주입** (Roxanne의 Sabotage, Anubia의 Thought Spiral) ⭐. **패배 = 하드 게임오버** (타이틀 복귀 — 세이브 로드 사실상 강제). ([Gamer Journalist](https://gamerjournalist.com/how-to-prepare-for-and-beat-each-competition-boss-in-potionomics/))
- **흥정 스킵 밸브**: Day 14 해금 자판기 **Vendi** — 8스택 넣으면 흥정 없이 기본가 +15% 고정 자동판매. 흥정(+50%↑)보다 싸지만 시간 무소모 → 후반 잡판매 아웃소싱.
- **설계 유래**: 원안은 Recettear식 가격 직접 제시 → 카드게임으로 전환 ("생각을 고르고 순서대로 잇는 대화 구조가 카드 플레이와 닮았다" — 개발자).

## 평가·비판

- **호평**: PC Gamer 87 "가볍고 경쾌한데 매력이 꽉 참" / 관계 랭크업 → 새 카드가 "항상 설렌다" (진행 연결 호평).
- **비판**: ① 중반부터 그라인드화 (RPGFan 74 — "매일 가마 4×3회 양조, 수학이 재미없어짐") ② 스트레스 나선의 체감 불공정 ③ 대회 스파이크 + 하드 게임오버 + 재료 RNG 게이트의 결합 (2차 대회 진행 막힘 보고 다수) ④ 파훼 덱으로 후반 흥정이 자동승리 기계화 ⑤ 코지 톤 vs 10일 데드라인 압박의 부조화.
- **Masterwork Edition의 대응**: 흥정 코어 규칙 개편 없이 **난이도 모드 레이어로 흡수** — Cozy 모드(교제 시간 무소모 + 보스 리밸런스), Endless 모드, 보스 Finn 확장(신규 카드). 대회 스트레스 카드 일부 수정. ([Marvelous 공식](https://marvelousgames.com/news/potionomics-masterwork-edition-out-now))

## 「마왕성 인사팀」 면접 덱빌딩에 주는 시사점

1. **게이지 구조 — 우리 온도계가 원리적으로 더 풍부하나, "확정 임계"가 필요**: 포셔노믹스는 역할 분리된 게이지 3개(Interest=점수·단조 증가 / Patience=타이머 겸 코스트 / Stress=리스크). 우리 긴장 온도계는 **양방향 조준 게이지**(올려도·내려도 다른 정보 채널)로 포셔노믹스에 없는 의사결정 축을 가짐. 단 포셔노믹스 손맛의 원천 두 가지 — "만렙 = 자동 성공"(확정 마감 보상)과 "인내 0 = 명확한 실패 비용" — 에 대응하는 **확률 아닌 확정 임계 이벤트**(온도 극단 도달 시 실수 대사/본색 대사가 확정 노출)가 온도계에도 있어야 "달군다 → 함정"이 콤보 손맛이 된다.
2. **드로우 RNG 기각 판단을 지지**: ① 최대 불만이 드로우 오염 RNG ② 숙련 플레이 = RNG 소거 덱으로 수렴 (무작위가 재미의 원천이 아니었다는 방증) ③ 20장 고정 덱이라 로그라이크식 리플레이 기여도 애초 없음. **단 경고 하나**: 드로우 순서가 하루 5~7회 반복 흥정의 유일한 미세 변주였음 — 로드아웃형인 우리는 그 변주를 **지원자 콘텐츠(케이스 풀·거짓 패턴·온도 반응)가 전담**해야 하며, 부족하면 중반 단조화가 포셔노믹스보다 이르게 온다.
3. **카드 = 전부 시스템 효과, 대사 0으로 대화극 성립**: 카드 약 80종에 고유 대사 없음, 고객 리액션도 제너릭 — **네이밍·플레이버만으로**("Flattery", "Good Cop, Bad Cop", "Wing It") 협상극의 서사감 획득. 우리 2계열 분리(질문 카드 소수 정예 + 화법·기술 카드 무제한)가 검증된 경로임을 실증: 깊이는 "단일 수용체(관심↔긴장) × 그걸 움직이는 수십 가지 방법"에서 나온다.
4. **"덱 = 정답기" 붕괴는 실존한다 (반면교사)**: Chorus 단일 최적 덱 90% 만렙 + Jingle 음수 코스트 무한 인내 + 고객 저항이 약해 범용 덱이 전부 관통. 포셔노믹스는 출력이 가격(스칼라)이라 붕괴해도 "돈을 더 벌 뿐"이지만, **우리 출력은 합불(이진 판단)이라 덱이 정답을 출력하는 순간 코어(간파)가 죽는다** — "카드는 정보 채널만" 가드레일이 옳음을 지지. 구체 클램프 3규칙: ① 코스트 감소·환급은 하한 0 클램프 (음수 누출 금지) ② 세션 내 무한 성장 스케일링 금지 (상한/리셋) ③ 지원자 저항은 수치 감소가 아니라 **채널 락** ("압박 채널 면역, 정중만 열림") — 그래야 로드아웃을 갈아끼울 이유가 매일 생긴다.
5. **훔칠 것**: ① **관계 → 카드 획득 파이프라인** (Persona식 — 화법 카드를 "동료 악마에게 배운 기술"로: 밤 파트·까마귀 상점과 연결, 카드 한 장이 서사 사건이 됨) ② **Opener/Closer 위치 문법 + 푸시-유어-럭 마감** ("언제 내는가"가 효과를 바꿈 + "조기 판단으로 포인트 절약 vs 끝까지 심문해 확신" — 대사 없이 시퀀스 퍼즐 생성) ③ **지원자 = 스탯+능력 재조합 저비용 변주 & 보스의 덱 오염** (특급 지원자가 역으로 면접관 손패에 "동요 카드"를 꽂는 역심문 연출).
6. **피할 것**: ① **확률이 확률을 낳는 플레이어측 리스크 게이지** (스트레스 나선 — 긴장을 지원자 쪽에 둔 우리 설계가 이미 회피 중. 면접관측 압박을 만든다면 확률 오염이 아니라 결정적 페널티로) ② **동일 미니게임 고빈도 반복 무대응** (스킵 밸브 Vendi가 너무 늦게 옴 — 우리는 "숙련 후 위임/약식 처리" 밸브를 진행 커브에 처음부터, 단 "자동 처리 = 정확도·수익 열화" 트레이드오프로) ③ **하드 게임오버 + RNG 게이트 결합** (세이브 로드 강제 = 몰입 붕괴 — 실패는 서사 분기(질책·감봉·오채용 후일담)로 흡수, Papers, Please 계보 톤 정합).

## 출처

- 공식: [Steam](https://store.steampowered.com/app/1874490/Potionomics/) · [Marvelous — Masterwork 발표](https://marvelousgames.com/news/potionomics-masterwork-edition-out-now) · [콘솔 패치노트](https://marvelousgames.com/news/potionomics-masterwork-edition-console-patch-notes)
- 위키: [Fandom — Potion Selling](https://potionomics.fandom.com/wiki/Potion_Selling) · [Fandom — Stress](https://potionomics.fandom.com/wiki/Stress)
- 인터뷰: [Game Developer — How Potionomics turned price haggling into a card game](https://www.gamedeveloper.com/business/how-potionomics-turned-price-haggling-into-a-card-game)
- 가이드·분석: [TechRaptor 카드 목록](https://techraptor.net/gaming/guides/potionomics-card-unlock-guide) · [Gamepur 흥정 해설](https://www.gamepur.com/guides/how-haggling-works-in-potionomics) · [xanthir 최적 덱 분석](https://www.xanthir.com/b5N40) · [스팀 Infinity Deck](https://steamcommunity.com/sharedfiles/filedetails/?id=2879007152) · [Gamer Journalist 대회 공략](https://gamerjournalist.com/how-to-prepare-for-and-beat-each-competition-boss-in-potionomics/) · [Bonus Action 워크스루](https://bonus-action.com/guides/potionomics-walkthrough-all-potion-competitions-tips-and-faq/)
- 리뷰: [RPGFan (74)](https://www.rpgfan.com/review/potionomics/) · [Tech-Gaming (80)](https://www.tech-gaming.com/potionomics/) · [Siliconera — Masterwork](https://www.siliconera.com/review-potionomics-masterwork-edition-feels-like-a-better-deal/) · [Wikipedia 리셉션 종합](https://en.wikipedia.org/wiki/Potionomics)
- 커뮤니티: 스팀 토론 — [스트레스 확률 불만](https://steamcommunity.com/app/1874490/discussions/0/3488628582211206265/) · [인내 0/-1 성공 판정](https://steamcommunity.com/app/1874490/discussions/0/5254037276324376318/) · [덱 장수](https://steamcommunity.com/app/1874490/discussions/0/601892644808828583/) · [만렙 상시화](https://steamcommunity.com/app/1874490/discussions/0/3732952742351682244/) · [2차 대회 난이도](https://steamcommunity.com/app/1874490/discussions/0/3493130356501734949/)
- ⚠️ 단일 출처 주의: 턴별 드로우/인내 에스컬레이션(Sirus) · 실패 시 포션 파기(Fandom) · 패치 전 고객 20명(유저 증언) · 대회 4회 기재(Bonus Action — 5회가 다수설). Reddit은 크롤러 차단으로 스팀 토론판 대체.
