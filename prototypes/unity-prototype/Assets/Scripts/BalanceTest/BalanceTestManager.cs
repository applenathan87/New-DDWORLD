using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// 밸런스 테스트 씬 전용 매니저.
/// R: 랜덤 배치  B: 전투 시작  P: 일시정지
///
/// 사용법:
/// 1. Unity에서 새 씬 생성 (File > New Scene > Empty)
/// 2. 빈 GameObject 생성 → BalanceTestManager 컴포넌트 추가
/// 3. Inspector에서 병종 UnitData 5개 + Korean Font 연결
/// 4. Play → R키로 배치 → B키로 전투
/// </summary>
public class BalanceTestManager : MonoBehaviour
{
    [Header("병종 데이터")]
    public UnitData cavalryData;
    public UnitData militiaData;
    public UnitData spearmanData;
    public UnitData archerData;
    public UnitData trapData;

    [Header("폰트")]
    public TMP_FontAsset koreanFont;

    [Header("덱 구성")]
    public int cavalryCount = 10;
    public int militiaCount = 10;
    public int spearmanCount = 10;
    public int archerCount = 10;
    public int trapCount = 5;

    [Header("전장")]
    public int columns = 14;
    public int rows = 5;
    public float tileSize = 1f;
    public float tileGap = 0.08f;

    [Header("카메라")]
    public Vector3 camPos = new Vector3(7.2f, 5.4f, -7.11f);
    public Vector3 camRot = new Vector3(29.2f, 0f, 0f);
    public float camFOV = 45f;

    private Camera mainCamera;
    private BattleField battleField;
    private TextMeshPro infoText;
    private int testCount;
    private bool ready;       // BattleField 준비 완료
    private bool placed;      // 배치 완료 상태
    private bool fighting;    // 전투 중
    private bool paused;

    // 배치 데이터
    private Dictionary<Vector2Int, CardData> playerPlacements = new();
    private Dictionary<Vector2Int, CardData> enemyPlacements = new();

    private void Start()
    {
        EnsureGameManager();
        CreateGrid();
        EnsureSimulator();

        // BattleField.Start()가 카메라를 세팅하므로, 1프레임 뒤에 덮어쓰기
        StartCoroutine(LateSetup());
    }

    private System.Collections.IEnumerator LateSetup()
    {
        yield return null; // BattleField.Start() 완료 대기
        SetupCamera();
        CreateInfoText();
        ShowInfo("R: 랜덤 배치 | 1~6: 매치업 프리셋 | B: 전투 | P: 일시정지");
        ready = true;
    }

    private void Update()
    {
        if (!ready) return;
        var kb = Keyboard.current;

        // R: 랜덤 배치
        if (kb.rKey.wasPressedThisFrame) DoPlace(null);

        // 1~6: 프리셋 매치업
        if (kb.digit1Key.wasPressedThisFrame) DoPlace(Preset_CavalryVsSpearman);
        if (kb.digit2Key.wasPressedThisFrame) DoPlace(Preset_ArcherVsCavalry);
        if (kb.digit3Key.wasPressedThisFrame) DoPlace(Preset_SpearmanVsArcher);
        if (kb.digit4Key.wasPressedThisFrame) DoPlace(Preset_MilitiaVsCavalry);
        if (kb.digit5Key.wasPressedThisFrame) DoPlace(Preset_TrapMix);
        if (kb.digit6Key.wasPressedThisFrame) DoPlace(Preset_FullMirror);

        // B: 전투 시작
        if (kb.bKey.wasPressedThisFrame && placed && !fighting)
            DoFight();

        // P: 일시정지/재개
        if (kb.pKey.wasPressedThisFrame && fighting)
        {
            paused = !paused;
            Time.timeScale = paused ? 0f : 1f;
            ShowInfo(paused ? "일시정지 (P: 재개)" : "");
        }

        UpdateCamera();
    }

    // === 배치 ===

    // 프리셋 타입: (아군 병종 배열, 적 병종 배열) 반환
    private delegate (UnitData[] player, UnitData[] enemy) PresetFunc();
    private string lastPresetName;

    private void DoPlace(PresetFunc preset)
    {
        testCount++;
        fighting = false;
        placed = false;
        paused = false;
        Time.timeScale = 1f;

        CleanupBattle();
        if (BattleSimulator.Instance != null)
            BattleSimulator.Instance.ClearResultScreen();
        ResetAllTiles();

        playerPlacements.Clear();
        enemyPlacements.Clear();

        if (preset != null)
        {
            // 프리셋 배치
            var (playerUnits, enemyUnits) = preset();
            PresetPlace(playerUnits, playerPlacements);
            PresetPlace(enemyUnits, enemyPlacements);
        }
        else
        {
            // 랜덤 배치
            lastPresetName = "랜덤";
            var playerDeck = new Deck(BuildDeckCards());
            var enemyDeck = new Deck(BuildDeckCards());
            playerDeck.Draw(8);
            enemyDeck.Draw(8);
            RandomPlace(playerDeck, playerPlacements);
            RandomPlace(enemyDeck, enemyPlacements);
        }

        // 타일에 표시
        var bf = BattleField.Instance;
        foreach (var kvp in playerPlacements)
        {
            var tile = bf.GetBattleTile(kvp.Key.y, kvp.Key.x);
            if (tile != null) tile.PlaceUnitFromData(kvp.Value);
        }
        foreach (var kvp in enemyPlacements)
        {
            int gridCol = 9 + (4 - kvp.Key.y);
            var tile = bf.GetBattleTile(gridCol, kvp.Key.x);
            if (tile != null) tile.PlaceUnitFromData(kvp.Value);
        }

        // GameManager에 배치 데이터 전달
        var gm = GameManager.Instance;
        gm.PlayerPlacements.Clear();
        gm.EnemyPlacements.Clear();
        foreach (var kvp in playerPlacements) gm.PlayerPlacements[kvp.Key] = kvp.Value;
        foreach (var kvp in enemyPlacements) gm.EnemyPlacements[kvp.Key] = kvp.Value;

        placed = true;
        Debug.Log($"=== 배치 #{testCount} [{lastPresetName}] ===");
        LogPlacements();
        ShowInfo($"[{lastPresetName}] 배치 완료 — B: 전투 시작");
    }

    /// <summary>
    /// 프리셋 배치: 병종 배열을 그리드 중앙부터 배치
    /// 1개면 중앙(row2), 2개면 row1,row3, 3개면 row1,2,3...
    /// </summary>
    private void PresetPlace(UnitData[] units, Dictionary<Vector2Int, CardData> placements)
    {
        int count = Mathf.Min(units.Length, 5);
        int startRow = (5 - count) / 2; // 중앙 정렬

        for (int i = 0; i < count; i++)
        {
            var card = new CardData(units[i], i);
            placements[new Vector2Int(startRow + i, 2)] = card;
        }
    }

    // === 프리셋 정의 ===

    // 1: 기병 vs 창병
    private (UnitData[], UnitData[]) Preset_CavalryVsSpearman()
    {
        lastPresetName = "기병 vs 창병";
        return (new[] { cavalryData }, new[] { spearmanData });
    }

    // 2: 궁병 vs 기병
    private (UnitData[], UnitData[]) Preset_ArcherVsCavalry()
    {
        lastPresetName = "궁병 vs 기병";
        return (new[] { archerData }, new[] { cavalryData });
    }

    // 3: 창병 vs 궁병
    private (UnitData[], UnitData[]) Preset_SpearmanVsArcher()
    {
        lastPresetName = "창병 vs 궁병";
        return (new[] { spearmanData }, new[] { archerData });
    }

    // 4: 민병대 vs 기병
    private (UnitData[], UnitData[]) Preset_MilitiaVsCavalry()
    {
        lastPresetName = "민병대 vs 기병";
        return (new[] { militiaData }, new[] { cavalryData });
    }

    // 5: 함정 + 궁병 vs 기병
    private (UnitData[], UnitData[]) Preset_TrapMix()
    {
        lastPresetName = "함정+궁병 vs 기병";
        return (new[] { trapData, archerData }, new[] { cavalryData, cavalryData });
    }

    // 6: 풀 미러
    private (UnitData[], UnitData[]) Preset_FullMirror()
    {
        lastPresetName = "풀 미러";
        return (
            new[] { cavalryData, spearmanData, archerData, militiaData, trapData },
            new[] { cavalryData, spearmanData, archerData, militiaData, trapData }
        );
    }

    // === B: 전투 ===

    private void DoFight()
    {
        fighting = true;
        paused = false;
        Time.timeScale = 1f;

        // 타일 색 리셋
        ResetAllTiles();

        ShowInfo("");
        Debug.Log($"=== 전투 #{testCount} 시작 ===");

        BattleSimulator.Instance.StopAllCoroutines();
        BattleSimulator.Instance.StartBattle();
    }

    // === 초기화 ===

    private void SetupCamera()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            var camObj = new GameObject("Main Camera");
            mainCamera = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
        }
        mainCamera.transform.position = camPos;
        mainCamera.transform.rotation = Quaternion.Euler(camRot);
        mainCamera.fieldOfView = camFOV;
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = new Color(0.08f, 0.08f, 0.12f);
    }

    private void CreateGrid()
    {
        // BattleField 컴포넌트를 실제로 생성 (BattleSimulator가 참조)
        var bfObj = new GameObject("BattleField");
        battleField = bfObj.AddComponent<BattleField>();
        battleField.koreanFont = koreanFont;
        battleField.columns = columns;
        battleField.rows = rows;
        battleField.tileSize = tileSize;
        battleField.tileGap = tileGap;
        // BattleField.Start()가 자동으로 그리드/카메라를 세팅
    }

    private void EnsureGameManager()
    {
        if (GameManager.Instance != null) return;
        var obj = new GameObject("GameManager_Test");
        var gm = obj.AddComponent<GameManager>();
        gm.skipAutoStart = true;
        gm.cavalryData = cavalryData;
        gm.militiaData = militiaData;
        gm.spearmanData = spearmanData;
        gm.archerData = archerData;
        gm.trapData = trapData;
    }

    private void EnsureSimulator()
    {
        if (BattleSimulator.Instance != null)
        {
            BattleSimulator.Instance.koreanFont = koreanFont;
            return;
        }
        var obj = new GameObject("BattleSimulator");
        var sim = obj.AddComponent<BattleSimulator>();
        sim.koreanFont = koreanFont;
    }

    private void CreateInfoText()
    {
        var obj = new GameObject("InfoText");
        obj.transform.SetParent(mainCamera.transform);
        obj.transform.localPosition = new Vector3(0, -0.8f, 3f);
        obj.transform.localRotation = Quaternion.identity;

        infoText = obj.AddComponent<TextMeshPro>();
        if (koreanFont != null) infoText.font = koreanFont;
        infoText.fontSize = 3;
        infoText.alignment = TextAlignmentOptions.Center;
        infoText.color = Color.white;
        infoText.rectTransform.sizeDelta = new Vector2(8f, 2f);
        infoText.raycastTarget = false;
    }

    private void ShowInfo(string msg) { if (infoText != null) infoText.text = msg; }

    // === 유틸 ===

    private void CleanupBattle()
    {
        if (BattleSimulator.Instance != null)
            BattleSimulator.Instance.StopAllCoroutines();

        foreach (var s in FindObjectsByType<Soldier>(FindObjectsSortMode.None)) Destroy(s.gameObject);
        foreach (var a in FindObjectsByType<Arrow>(FindObjectsSortMode.None)) Destroy(a.gameObject);
        var bounds = GameObject.Find("FieldBounds");
        if (bounds != null) Destroy(bounds);
    }

    private void ResetAllTiles()
    {
        var bf = BattleField.Instance;
        if (bf == null) return;
        for (int col = 0; col < columns; col++)
            for (int row = 0; row < rows; row++)
            {
                var tile = bf.GetBattleTile(col, row);
                if (tile != null) tile.ResetForBattle();
            }
    }

    private void RandomPlace(Deck deck, Dictionary<Vector2Int, CardData> placements)
    {
        List<Vector2Int> available = new();
        for (int r = 0; r < 5; r++)
            for (int c = 0; c < 5; c++)
                available.Add(new Vector2Int(r, c));

        for (int i = available.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (available[i], available[j]) = (available[j], available[i]);
        }

        int placed = 0;
        var handCopy = new List<CardData>(deck.Hand);
        foreach (var card in handCopy)
        {
            if (placed >= 5) break;
            placements[available[placed]] = card;
            deck.PlayCard(card);
            placed++;
        }
    }

    private List<CardData> BuildDeckCards()
    {
        var cards = new List<CardData>();
        int id = 0;
        for (int i = 0; i < cavalryCount; i++) cards.Add(new CardData(cavalryData, id++));
        for (int i = 0; i < militiaCount; i++) cards.Add(new CardData(militiaData, id++));
        for (int i = 0; i < spearmanCount; i++) cards.Add(new CardData(spearmanData, id++));
        for (int i = 0; i < archerCount; i++) cards.Add(new CardData(archerData, id++));
        for (int i = 0; i < trapCount; i++) cards.Add(new CardData(trapData, id++));
        return cards;
    }

    private void LogPlacements()
    {
        Debug.Log("--- 아군 배치 ---");
        foreach (var kvp in playerPlacements)
            Debug.Log($"  ({kvp.Key.x},{kvp.Key.y}): {kvp.Value.unitData.unitName}");
        Debug.Log("--- 적군 배치 ---");
        foreach (var kvp in enemyPlacements)
            Debug.Log($"  ({kvp.Key.x},{kvp.Key.y}): {kvp.Value.unitData.unitName}");
    }

    // === 카메라 ===

    private bool camDragging;
    private Vector2 camDragOrigin;

    private void UpdateCamera()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.rightButton.wasPressedThisFrame) { camDragging = true; camDragOrigin = mouse.position.ReadValue(); }
        if (mouse.rightButton.wasReleasedThisFrame) camDragging = false;

        if (camDragging)
        {
            Vector2 delta = (Vector2)mouse.position.ReadValue() - camDragOrigin;
            camDragOrigin = mouse.position.ReadValue();
            Vector3 right = mainCamera.transform.right;
            Vector3 forward = Vector3.Cross(right, Vector3.up).normalized;
            mainCamera.transform.position -= (right * delta.x + forward * delta.y) * 0.012f;
        }

        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
            mainCamera.fieldOfView = Mathf.Clamp(mainCamera.fieldOfView - scroll * 2f, 15f, 70f);
    }
}
