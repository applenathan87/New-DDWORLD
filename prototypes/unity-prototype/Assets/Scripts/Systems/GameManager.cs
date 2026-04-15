using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 전체 흐름을 관리하는 매니저.
/// 라운드 진행, 카드 분배, 상태 전환.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("덱 구성 — 각 병종별 장수")]
    public UnitData cavalryData;
    public int cavalryCount = 10;

    public UnitData militiaData;
    public int militiaCount = 10;

    public UnitData spearmanData;
    public int spearmanCount = 10;

    public UnitData archerData;
    public int archerCount = 10;

    public UnitData trapData;
    public int trapCount = 5;

    // 현재 게임 상태
    public enum GamePhase { Setup, Draw, Placement, Battle, Result }
    public GamePhase CurrentPhase { get; private set; } = GamePhase.Setup;

    // 덱
    private Deck playerDeck;
    private Deck enemyDeck;

    // 라운드
    private int currentRound = 0;
    private int playerWins = 0;
    private int enemyWins = 0;
    private const int WINS_NEEDED = 3;
    private const int FIRST_DRAW = 8;
    private const int NORMAL_DRAW = 5;
    private const int CARDS_TO_PLACE = 5;

    // 배치 데이터: (row, col) -> CardData
    public Dictionary<Vector2Int, CardData> PlayerPlacements { get; private set; } = new();
    public Dictionary<Vector2Int, CardData> EnemyPlacements { get; private set; } = new();

    public Deck PlayerDeck => playerDeck;
    public Deck EnemyDeck => enemyDeck;
    public int CurrentRound => currentRound;
    public int PlayerWins => playerWins;
    public int EnemyWins => enemyWins;

    // 이벤트
    public System.Action<GamePhase> OnPhaseChanged;
    public System.Action<List<CardData>> OnCardsDrawn;
    public System.Action<int> OnPlacementNotReady; // 남은 배치 수


    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        StartNewGame();
    }

    /// <summary>
    /// 새 게임 시작: 덱 생성 → 1라운드 시작
    /// </summary>
    public void StartNewGame()
    {
        playerDeck = new Deck(BuildDeckCards());
        enemyDeck = new Deck(BuildDeckCards());

        currentRound = 0;
        playerWins = 0;
        enemyWins = 0;

        StartNewRound();
    }

    /// <summary>
    /// 새 라운드 시작: 카드 드로우 → 배치 페이즈
    /// </summary>
    public void StartNewRound()
    {
        currentRound++;
        PlayerPlacements.Clear();
        EnemyPlacements.Clear();

        // 드로우 페이즈 시작
        CurrentPhase = GamePhase.Draw;
        OnPhaseChanged?.Invoke(CurrentPhase);

        int drawCount = (currentRound == 1) ? FIRST_DRAW : NORMAL_DRAW;

        var playerDrawn = playerDeck.Draw(drawCount);
        var enemyDrawn = enemyDeck.Draw(drawCount);

        Debug.Log($"[라운드 {currentRound}] 플레이어 드로우 {playerDrawn.Count}장 (손패 {playerDeck.HandCount}장)");
        Debug.Log($"[라운드 {currentRound}] 상대 드로우 {enemyDrawn.Count}장 (손패 {enemyDeck.HandCount}장)");

        OnCardsDrawn?.Invoke(playerDrawn);
    }

    /// <summary>
    /// 드로우 애니메이션 완료 후 배치 페이즈로 전환 (PlacementUI에서 호출)
    /// </summary>
    public void EnterPlacementPhase()
    {
        CurrentPhase = GamePhase.Placement;
        OnPhaseChanged?.Invoke(CurrentPhase);
    }

    /// <summary>
    /// 플레이어가 카드를 격자에 배치
    /// </summary>
    public bool PlaceCard(CardData card, int row, int col)
    {
        var pos = new Vector2Int(row, col);

        if (PlayerPlacements.ContainsKey(pos)) return false;
        if (PlayerPlacements.Count >= CARDS_TO_PLACE) return false;
        if (!playerDeck.Hand.Contains(card)) return false;

        PlayerPlacements[pos] = card;
        playerDeck.PlayCard(card);

        Debug.Log($"배치: {card.unitData.unitName} → ({row}, {col}) | 남은 배치: {CARDS_TO_PLACE - PlayerPlacements.Count}장");

        return true;
    }

    /// <summary>
    /// 배치에서 카드 제거 (되돌리기)
    /// </summary>
    public void RemovePlacement(int row, int col)
    {
        var pos = new Vector2Int(row, col);
        if (PlayerPlacements.TryGetValue(pos, out CardData card))
        {
            PlayerPlacements.Remove(pos);
            playerDeck.Hand.Add(card);
            Debug.Log($"배치 취소: {card.unitData.unitName} ({row}, {col})");
        }
    }

    /// <summary>
    /// 배치 확정 → 전투 페이즈로 (전투 로직은 추후 구현)
    /// </summary>
    public void ConfirmPlacement()
    {
        if (PlayerPlacements.Count < CARDS_TO_PLACE)
        {
            OnPlacementNotReady?.Invoke(CARDS_TO_PLACE - PlayerPlacements.Count);
            return;
        }

        // 상대 랜덤 배치 (AI 대전 / PVP 연결 끊김 대비)
        AutoPlaceRandom(enemyDeck, EnemyPlacements);

        CurrentPhase = GamePhase.Battle;
        OnPhaseChanged?.Invoke(CurrentPhase);

        Debug.Log("=== 배치 확정! 전투 페이즈 시작 ===");

        // 전투 시뮬레이션 시작
        if (BattleSimulator.Instance != null)
            BattleSimulator.Instance.StartBattle();
    }

    /// <summary>
    /// 라운드 결과 보고 (BattleSimulator에서 호출)
    /// </summary>
    public void ReportRoundResult(bool playerWon)
    {
        if (playerWon) playerWins++;
        else enemyWins++;

        Debug.Log($"[스코어] 플레이어 {playerWins} - {enemyWins} 상대");

        if (playerWins >= WINS_NEEDED || enemyWins >= WINS_NEEDED)
        {
            CurrentPhase = GamePhase.Result;
            OnPhaseChanged?.Invoke(CurrentPhase);
            string winner = playerWins >= WINS_NEEDED ? "플레이어" : "상대";
            Debug.Log($"=== 매치 종료! {winner} 승리! ({playerWins}-{enemyWins}) ===");
        }
        else
        {
            // 다음 라운드 (2초 후)
            Invoke(nameof(StartNewRound), 2f);
        }
    }

    /// <summary>
    /// 랜덤 배치 — 양쪽 모두 사용 가능 (AI 대전, PVP 연결 끊김 대비)
    /// </summary>
    public void AutoPlaceRandom(Deck deck, Dictionary<Vector2Int, CardData> placements)
    {
        // 이미 배치된 수 계산
        int alreadyPlaced = placements.Count;
        int remaining = CARDS_TO_PLACE - alreadyPlaced;
        if (remaining <= 0) return;

        // 빈 칸 수집
        List<Vector2Int> available = new();
        for (int r = 0; r < 5; r++)
            for (int c = 0; c < 5; c++)
                if (!placements.ContainsKey(new Vector2Int(r, c)))
                    available.Add(new Vector2Int(r, c));

        // 셔플
        for (int i = available.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (available[i], available[j]) = (available[j], available[i]);
        }

        int placed = 0;
        var handCopy = new List<CardData>(deck.Hand);
        foreach (var card in handCopy)
        {
            if (placed >= remaining) break;
            if (placed >= available.Count) break;

            placements[available[placed]] = card;
            deck.PlayCard(card);
            placed++;
        }

        Debug.Log($"[랜덤 배치] {placed}장 배치 완료 (기존 {alreadyPlaced}장 + 신규 {placed}장)");
    }

    /// <summary>
    /// 50장 덱 카드 리스트 생성
    /// </summary>
    private List<CardData> BuildDeckCards()
    {
        var cards = new List<CardData>();
        int id = 0;

        for (int i = 0; i < cavalryCount; i++)
            cards.Add(new CardData(cavalryData, id++));
        for (int i = 0; i < militiaCount; i++)
            cards.Add(new CardData(militiaData, id++));
        for (int i = 0; i < spearmanCount; i++)
            cards.Add(new CardData(spearmanData, id++));
        for (int i = 0; i < archerCount; i++)
            cards.Add(new CardData(archerData, id++));
        for (int i = 0; i < trapCount; i++)
            cards.Add(new CardData(trapData, id++));

        return cards;
    }
}
