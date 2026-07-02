using System.Collections.Generic;
using UnityEngine;

namespace DDworld.CombatTest
{
    /// <summary>
    /// PvE 흐름 매니저 (combat_test) — 원본 GameManager(PvP 3판2선승·덱 드로우)의 대체.
    ///
    /// 원본과 다른 점:
    /// - 적 = EncounterLibrary 몬스터 진형. 배치 "전"에 100% 공개 (보고 카운터 배치 = 코어)
    /// - 카드 = 보유 로스터 전체를 들고 입장. 배치해도 소모되지 않음 (B안 — pve-pivot Q5 자유 편성)
    ///   유일한 손실은 나중에 permadeath로 얹는다 — 현재 v0는 손실 없음.
    /// - 판 구조 = 단판: 승리 → 다음 인카운터 / 패배 → 재도전 / 전부 클리어 → 처음부터
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("보유 로스터 — 병종별 장수 (CombatTestBootstrap이 설정)")]
        public int militiaCount = 3;
        public int spearmanCount = 2;
        public int archerCount = 2;
        public int cavalryCount = 2;
        public int trapCount = 1;

        public enum GamePhase { Setup, Draw, Placement, Battle, Result }
        public GamePhase CurrentPhase { get; private set; } = GamePhase.Setup;

        // 보유 로스터 (영구) — 매 전투마다 손패로 전량 리필
        private readonly List<CardData> owned = new();
        private Roster rosterHand;

        private int currentRound = 0;
        private int playerWins = 0;
        private int enemyWins = 0;
        private const int CARDS_TO_PLACE = 5;   // 최대 배치 수 (최소 1)

        /// <summary>현재 인카운터 (E0~). 승리 시 다음으로 진행.</summary>
        public int EncounterIndex { get; private set; } = 0;

        // 배치 데이터: (row, col) -> CardData
        public Dictionary<Vector2Int, CardData> PlayerPlacements { get; private set; } = new();
        public Dictionary<Vector2Int, CardData> EnemyPlacements { get; private set; } = new();

        /// <summary>손패. 원본 Deck의 Hand/HandCount/PlayCard 인터페이스만 유지한 로스터.</summary>
        public Roster PlayerDeck => rosterHand;
        public int CurrentRound => currentRound;
        public int PlayerWins => playerWins;
        public int EnemyWins => enemyWins;

        // 이벤트 (원본과 동일 시그니처)
        public System.Action<GamePhase> OnPhaseChanged;
        public System.Action<List<CardData>> OnCardsDrawn;
        public System.Action<int> OnPlacementNotReady;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            StartNewGame();
        }

        /// <summary>새 캠페인: 보유 로스터 구성 → 첫 인카운터</summary>
        public void StartNewGame()
        {
            owned.Clear();
            owned.AddRange(EncounterLibrary.BuildStartingRoster(
                militiaCount, spearmanCount, archerCount, cavalryCount, trapCount));
            rosterHand = new Roster();

            currentRound = 0;
            playerWins = 0;
            enemyWins = 0;

            StartNewRound();
        }

        /// <summary>새 전투: 적 진형 선공개 → 로스터 손패 → 배치 페이즈</summary>
        public void StartNewRound()
        {
            currentRound++;
            PlayerPlacements.Clear();
            EnemyPlacements.Clear();

            // PvE 핵심 ①: 몬스터 진형을 먼저 깔아둔다 (100% 공개 → 카운터 배치)
            foreach (var kvp in EncounterLibrary.BuildFormation(EncounterIndex))
                EnemyPlacements[kvp.Key] = kvp.Value;

            CurrentPhase = GamePhase.Draw;
            OnPhaseChanged?.Invoke(CurrentPhase); // (BattleSimulator가 이전 전투 잔재 정리)

            // 잔재 정리 후 적 진형을 전장에 표시 (배치 중에도 보임)
            BattleField.Instance?.ShowEnemyPlacements();

            // PvE 핵심 ②: 손패 = 보유 로스터 전체 (드로우/소모 없음 — B안)
            rosterHand.Hand.Clear();
            rosterHand.Hand.AddRange(owned);

            Debug.Log($"[{EncounterLibrary.Encounters[EncounterIndex].name}] 보유 {owned.Count}장으로 입장");

            OnCardsDrawn?.Invoke(rosterHand.Hand);
        }

        /// <summary>드로우 애니메이션 완료 후 배치 페이즈로 전환 (PlacementUI에서 호출)</summary>
        public void EnterPlacementPhase()
        {
            CurrentPhase = GamePhase.Placement;
            OnPhaseChanged?.Invoke(CurrentPhase);
        }

        /// <summary>플레이어가 카드를 격자에 배치</summary>
        public bool PlaceCard(CardData card, int row, int col)
        {
            var pos = new Vector2Int(row, col);

            if (PlayerPlacements.ContainsKey(pos)) return false;
            if (PlayerPlacements.Count >= CARDS_TO_PLACE) return false;
            if (!rosterHand.Hand.Contains(card)) return false;

            PlayerPlacements[pos] = card;
            rosterHand.PlayCard(card); // 손에서만 빠짐 — 전투 후 로스터로 복귀 (소모 아님)

            Debug.Log($"배치: {card.unitData.unitName} → ({row}, {col}) | {PlayerPlacements.Count}/{CARDS_TO_PLACE}");
            return true;
        }

        /// <summary>배치에서 카드 제거 (되돌리기)</summary>
        public void RemovePlacement(int row, int col)
        {
            var pos = new Vector2Int(row, col);
            if (PlayerPlacements.TryGetValue(pos, out CardData card))
            {
                PlayerPlacements.Remove(pos);
                rosterHand.Hand.Add(card);
                Debug.Log($"배치 취소: {card.unitData.unitName} ({row}, {col})");
            }
        }

        /// <summary>배치 확정 → 전투 (최소 1부대 — 약하게 가는 건 플레이어 선택, Q5)</summary>
        public void ConfirmPlacement()
        {
            if (PlayerPlacements.Count == 0)
            {
                OnPlacementNotReady?.Invoke(1);
                return;
            }

            CurrentPhase = GamePhase.Battle;
            OnPhaseChanged?.Invoke(CurrentPhase);

            Debug.Log("=== 배치 확정! 전투 시작 ===");

            if (BattleSimulator.Instance != null)
                BattleSimulator.Instance.StartBattle();
        }

        /// <summary>전투 결과 보고 (BattleSimulator에서 호출)</summary>
        public void ReportRoundResult(bool playerWon)
        {
            if (playerWon) playerWins++;
            else enemyWins++;

            Debug.Log($"[전적] 승 {playerWins} · 패 {enemyWins}");

            if (playerWon)
            {
                if (EncounterIndex >= EncounterLibrary.Count - 1)
                {
                    // 전체 인카운터 클리어 → 캠페인 재시작
                    CurrentPhase = GamePhase.Result;
                    OnPhaseChanged?.Invoke(CurrentPhase);
                    Debug.Log($"=== 전 인카운터 클리어! (승 {playerWins} · 패 {enemyWins}) — 처음부터 다시 ===");
                    EncounterIndex = 0;
                    Invoke(nameof(StartNewGame), 6f);
                    return;
                }
                EncounterIndex++;   // 다음 인카운터
            }
            // 패배 시 같은 인카운터 재도전 (EncounterIndex 유지)

            Invoke(nameof(StartNewRound), 4f);
        }
    }

    /// <summary>
    /// 원본 Deck의 최소 인터페이스(Hand/HandCount/PlayCard)만 유지한 로스터 손패.
    /// 드로우 더미/버림 더미 없음 — 배치는 손에서만 빠지고, 매 전투 로스터 전체로 리필된다.
    /// </summary>
    public class Roster
    {
        public List<CardData> Hand = new();
        public int HandCount => Hand.Count;
        public void PlayCard(CardData card) => Hand.Remove(card);
    }
}
