---
Day: 22
날짜: 2026-04-14
작업시간: 3
상태: 완료
---
[[데브로그/데브로그]]
# Day 22 — 배틀 그리드 컨셉 & Unity 프로토타입 시작

## 한 일
- DDworld 게임 컨셉 확정: 심리전 기반 전략 오토배틀러 (PvP)
  - 배틀쉽 격자 배치 + 가위바위보 상성 + 오토배틀러 자동 전투
  - 상대 손패 공개, 배치 비공개 → 불완전 정보 심리전
- 핵심 시스템 설계
  - 카드 흐름: 8장 드로우 → 5장 배치 → 3장 이월 (5판 3선승)
  - 병종 5종 기본 스탯: 기병/민병/창병/궁병/함정
  - 틱 기반 전투 (1틱=0.5초), 전장 14칸 (5+4+5)
  - 판돈(Loot) 시스템: 잔존 병사 = 보상, 역전 구조
  - 50장 덱 빌딩: 상위호환이 아닌 개성 있는 덱 구성
- GDD 파일에 분배: 게임개요, 핵심경험, 핵심루프, 전투시스템, 경제시스템
- HTML 틱 시뮬레이터 제작 (밸런스 확인용)
- Unity 프로토타입 시작 (Universal 3D, 쿼터뷰)
  - 프로젝트 세팅 & 폴더 구조
  - ScriptableObject 기반 병종 데이터 5종 생성
  - 덱 시스템 (셔플, 드로우, 이월)
  - GameManager (라운드 흐름 관리)
  - 배치 UI (카드 표시, 5x5 격자, 드래그 앤 드롭 구현 중)
- 안읽음 아티클 10개 WebFetch로 요약 완료

## 배운 것
- S.M.A.R.T 목표로 성공 정의를 먼저 세우는 것의 중요성 (Bass Monkey 포스트모템)
- Chris Zukowski의 계단식 마케팅: 첫 게임부터 대박 노리지 말고 단계적으로
- Unity TMP 한글 폰트: Custom Range 32-126,44032-55203 로 한글 전체 포함
- Unity UI 드래그 앤 드롭: IBeginDragHandler, IDragHandler, IEndDragHandler + IDropHandler

## 막힌 것
- 드래그 앤 드롭이 아직 작동하지 않음 (EventSystem/레이캐스트 이슈 디버깅 필요)

## 메모
- 게임 컨셉의 핵심: "정보는 있는데 확신은 없는" 상태에서 결정 → 읽기의 쾌감
- 병종 행동 패턴(기병 돌격 전환, 옆칸 감지 등)은 시각적 프로토타입 후 구체화
- 맥에서도 개발 환경 확인 필요 → 크로스 플랫폼 폰트 준비 완료

## 다음 목표
- 드래그 앤 드롭 디버깅 완료
- 배치 확정 → 전투 페이즈 연결
- 창병 행동 패턴부터 시각적으로 구현
- 맥 개발 환경 확인

## Unity 프로젝트 세팅 가이드 (새 환경에서 열 때)

프로젝트 경로: `DDWORLD/My project/`

### 1. TMP 폰트 아틀라스 재생성 (git에서 제외됨, 133MB)
- Window > TextMeshPro > Font Asset Creator
- Source Font File: `Assets/Fonts/MonaS12`
- Sampling Point Size: `Auto Sizing`
- Atlas Resolution: `8192 x 8192`
- Character Set: `Custom Range`
- Custom Character Range: `32-126,44032-55203`
- Generate Font Atlas → `Assets/Fonts/MonaS12 SDF` 로 저장

### 2. 씬 오브젝트 확인 (SampleScene)
- **GameManager** (빈 오브젝트)
  - GameManager.cs 컴포넌트
  - Inspector에서 5개 병종 데이터 연결:
    - Cavalry Data ← `Assets/ScriptableObjects/Units/Cavalry`
    - Militia Data ← `Assets/ScriptableObjects/Units/Militia`
    - Spearman Data ← `Assets/ScriptableObjects/Units/Spearman`
    - Archer Data ← `Assets/ScriptableObjects/Units/Archer`
    - Trap Data ← `Assets/ScriptableObjects/Units/Trap`
- **PlacementUI** (빈 오브젝트)
  - PlacementUI.cs 컴포넌트
  - Korean Font ← `Assets/Fonts/MonaS12 SDF`

### 3. 확인
- Play → Console에 `[라운드 1] 플레이어 드로우 8장` 로그 확인
- 카드 UI + 5x5 격자가 화면에 표시되면 정상
