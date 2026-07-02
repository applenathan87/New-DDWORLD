# 80 Level 읽기 목록 — 1단계 (보이는 공간)

> 수집: 2026-07-02 · 대상 단계: **1단계 — 잔디·쿼터뷰 카메라·조명·소품·색·후처리**
> ✔검증 = 내용을 직접 열어 확인함. 나머지는 제목/검색 요약 기준.
> 읽는 순서 추천: ⭐ 2개 먼저 → 1단계 작업과 병행.

---

## 🌿 잔디 + 바람 (아트 방향: "입체 잔디 + 바람 살랑임")

- [ ] ⭐ **[Stylized Nature: Vegetation, Animation, Shaders](https://80.lv/articles/stylized-nature-vegetation-animation-shaders)** ✔검증
  - Unity 기준. **전역 노이즈 텍스처를 월드 공간에서 패닝 → 버텍스 오프셋 + UV 애니메이션 2겹**으로 바람 구현.
  - 우리가 만들 잔디 살랑임의 교과서. 실무 상세 많음 (Amplify Shader, LOD, 색 관리 스크립트).
- [ ] **[Stylized Grass Shader for Unity](https://80.lv/articles/decorate-your-scenes-with-this-refreshing-stylized-grass-shader-for-unity)** ✔검증
  - URP 17+ · Unity 6 호환 유료 에셋 소개. 구매용이 아니라 **"완성형 잔디 셰이더의 기능 체크리스트"**로 참고 (바람 동적 속도/방향, 색 변화 등).
- [ ] **[Woodbound: Making a Living World with Wind in UE4](https://80.lv/articles/woodbound-making-a-living-world-with-wind-in-ue4)**
  - UE지만 개념 이식 가능 — 노이즈 패닝을 알파로 써서 잔디 기본색 ↔ "바람 줄기" 색을 lerp. **바람이 눈에 보이게 만드는 트릭.**
- [ ] **[Meadows: Creating Stylized Nature in UE4](https://80.lv/articles/meadows-creating-stylized-nature-in-ue4)**
  - 야숨 + 지브리 스타일 초원 브레이크다운. 따뜻한 톤 레퍼런스.

## 💡 조명 · 색 · 후처리 (미니어처 룩)

- [ ] ⭐ **[Stylized Environment Production in Unity](https://80.lv/articles/stylized-environment-production-in-unity)** ✔검증
  - **단일 디렉셔널 라이트 + 구름 라이트 쿠키**로 생동감 / **버텍스 컬러로 색 변화** / "후처리 중 컬러 그레이딩이 제일 효과 큼".
  - ADR-003(정점색 1머티리얼)·1단계 조명 계획과 같은 사상 — 방향 검증용으로도 좋음.
- [ ] **[Creating a Stylized Environment with Unity](https://80.lv/articles/creating-a-stylized-environment-with-unity)** — 미검증, 위 것 먼저.
- [ ] **[A Custom Lighting Solution Set Up in Unity](https://80.lv/articles/a-custom-lighting-solution-set-up-in-unity)** — 미검증, 심화용.

## 🧱 로우폴리 감각 (소품 — 돌담·울타리)

- [ ] [Lowpoly Environment Design Tricks](https://80.lv/articles/emek-can-ozben-low-poly-environment)
- [ ] [Tips on Low Poly Game Environments](https://80.lv/articles/tips-on-low-poly-game-environments)
- [ ] [Creating Visual Interest in Low Poly Artworks](https://80.lv/articles/creating-visual-interest-in-low-poly-artworks)

## 🚫 걸러낸 것 (다시 찾지 않기 위한 기록)

- [Bad North: Cure Minimalistic RTS with Vikings](https://80.lv/articles/bad-north-cure-minimalistic-rts-with-vikings) — 직접 확인: **소개 글 수준, 제작 기법 없음.**
- Townscaper 관련 — 뉴스성 소식만. Oskar Stålberg(Bad North 아티스트)의 기법 자료는 80.lv가 아니라 **GDC 강연·개인 트위터**가 본진.
