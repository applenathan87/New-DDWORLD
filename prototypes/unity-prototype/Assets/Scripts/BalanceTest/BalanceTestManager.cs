using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// 밸런스 테스트 씬 전용 매니저.
/// 빈 씬에 이 스크립트 하나만 추가하면 동작.
/// B키로 전투 시작/재시작.
///
/// 사용법:
/// 1. Unity에서 새 씬 생성 (File > New Scene > Empty)
/// 2. 빈 GameObject 생성 → BalanceTestManager 컴포넌트 추가
/// 3. Inspector에서 5개 병종 UnitData 연결
/// 4. Play → B키로 전투 시작
/// </summary>
public class BalanceTestManager : MonoBehaviour
{
    [Header("병종 데이터 (Inspector에서 연결)")]
    public UnitData cavalryData;
    public UnitData militiaData;
    public UnitData spearmanData;
    public UnitData archerData;
    public UnitData trapData;

    [Header("덱 구성")]
    public int cavalryCount = 10;
    public int militiaCount = 10;
    public int spearmanCount = 10;
    public int archerCount = 10;
    public int trapCount = 5;

    [Header("전장 설정")]
    public int columns = 14;
    public int rows = 5;
    public float tileSize = 1f;
    public float tileGap = 0.08f;

    [Header("카메라")]
    public Vector3 camPos = new Vector3(7.2f, 5.4f, -7.11f);
    public Vector3 camRot = new Vector3(29.2f, 0f, 0f);
    public float camFOV = 45f;

    // 내부
    private Camera mainCamera;
    private BattleTile[,] tiles;
    private BattleSimulator simulator;
    private bool battleRunning;
    private int testCount;

    // 배치 데이터
    private Dictionary<Vector2Int, CardData> playerPlacements = new();
    private Dictionary<Vector2Int, CardData> enemyPlacements = new();

    private TextMeshPro infoText;

    private void Start()
    {
        SetupCamera();
        CreateGrid();
        CreateSimulator();
        CreateInfoText();

        Debug.Log("=== 밸런스 테스트 씬 ===");
        Debug.Log("B키: 전투 시작/재시작");
        ShowInfo("B키를 눌러 전투 시작");
    }

    private void Update()
    {
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            StartTest();
        }

        // 카메라 패닝 (우클릭 드래그)
        UpdateCamera();
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
        tiles = new BattleTile[columns, rows];

        Color playerColor = new Color(0.3f, 0.5f, 0.8f, 0.6f);
        Color neutralColor = new Color(0.5f, 0.5f, 0.5f, 0.4f);
        Color enemyColor = new Color(0.8f, 0.3f, 0.3f, 0.6f);

        // 바닥
        var ground = GameObject.CreatePrimitive(PrimitiveType.Quad);
        ground.name = "Ground";
        ground.transform.position = new Vector3(GetFieldCenter().x, -0.01f, GetFieldCenter().z);
        ground.transform.rotation = Quaternion.Euler(90, 0, 0);
        ground.transform.localScale = new Vector3(50f, 50f, 1);
        var groundMat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color"));
        groundMat.color = new Color(0.12f, 0.12f, 0.15f);
        ground.GetComponent<Renderer>().material = groundMat;
        Destroy(ground.GetComponent<Collider>());

        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                var tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tile.name = $"{(char)('A' + col)}{row + 1}";
                tile.transform.position = GetTileWorldPosition(col, row);
                tile.transform.rotation = Quaternion.Euler(90, 0, 0);
                tile.transform.localScale = new Vector3(tileSize * 0.95f, tileSize * 0.95f, 1);

                bool isPlayer = col < 5;
                bool isNeutral = col >= 5 && col < 9;
                bool isEnemy = col >= 9;
                Color color = isPlayer ? playerColor : isNeutral ? neutralColor : enemyColor;

                if ((col + row) % 2 == 1) color *= 0.8f;
                color.a = isNeutral ? 0.3f : 0.5f;

                var renderer = tile.GetComponent<Renderer>();
                var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color"));
                mat.color = color;
                mat.SetFloat("_Surface", 1);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 3000;
                renderer.material = mat;

                Destroy(tile.GetComponent<Collider>());

                if (isPlayer || isEnemy)
                {
                    var battleTile = tile.AddComponent<BattleTile>();
                    battleTile.Setup(col, row, isPlayer, color, renderer);
                    tiles[col, row] = battleTile;
                }
            }
        }
    }

    private void CreateSimulator()
    {
        if (BattleSimulator.Instance == null)
        {
            var simObj = new GameObject("BattleSimulator");
            simulator = simObj.AddComponent<BattleSimulator>();
        }
        else
        {
            simulator = BattleSimulator.Instance;
        }
    }

    private void CreateInfoText()
    {
        var obj = new GameObject("InfoText");
        obj.transform.SetParent(mainCamera.transform);
        obj.transform.localPosition = new Vector3(0, 0, 3f);
        obj.transform.localRotation = Quaternion.identity;

        infoText = obj.AddComponent<TextMeshPro>();
        infoText.fontSize = 4;
        infoText.alignment = TextAlignmentOptions.Center;
        infoText.color = Color.white;
        infoText.rectTransform.sizeDelta = new Vector2(8f, 2f);
        infoText.raycastTarget = false;
    }

    private void ShowInfo(string msg)
    {
        if (infoText != null) infoText.text = msg;
    }

    // === 테스트 실행 ===

    private void StartTest()
    {
        testCount++;
        Debug.Log($"\n=== 밸런스 테스트 #{testCount} ===");
        ShowInfo($"테스트 #{testCount} 진행 중...");

        // 정리
        CleanupBattle();

        // 덱 생성 + 드로우
        var playerDeck = new Deck(BuildDeckCards());
        var enemyDeck = new Deck(BuildDeckCards());
        playerDeck.Draw(8);
        enemyDeck.Draw(8);

        // 양쪽 랜덤 배치
        playerPlacements.Clear();
        enemyPlacements.Clear();
        RandomPlace(playerDeck, playerPlacements);
        RandomPlace(enemyDeck, enemyPlacements);

        // 타일에 유닛 표시
        ResetAllTiles();

        foreach (var kvp in playerPlacements)
        {
            var tile = tiles[kvp.Key.y, kvp.Key.x];
            if (tile != null) tile.PlaceUnitFromData(kvp.Value);
        }

        foreach (var kvp in enemyPlacements)
        {
            int gridCol = 9 + (4 - kvp.Key.y);
            var tile = tiles[gridCol, kvp.Key.x];
            if (tile != null) tile.PlaceUnitFromData(kvp.Value);
        }

        // GameManager에 배치 데이터 전달 (BattleSimulator가 참조)
        SetupGameManagerForTest(playerDeck, enemyDeck);

        // 전투 시작
        simulator.StopAllCoroutines();
        simulator.StartBattle();
    }

    private void SetupGameManagerForTest(Deck playerDeck, Deck enemyDeck)
    {
        // GameManager가 없으면 임시 생성
        if (GameManager.Instance == null)
        {
            var gmObj = new GameObject("GameManager_Test");
            var gm = gmObj.AddComponent<GameManager>();
            // Inspector 연결 대신 코드로 설정
            gm.cavalryData = cavalryData;
            gm.militiaData = militiaData;
            gm.spearmanData = spearmanData;
            gm.archerData = archerData;
            gm.trapData = trapData;
        }

        // 배치 데이터 복사
        GameManager.Instance.PlayerPlacements.Clear();
        GameManager.Instance.EnemyPlacements.Clear();
        foreach (var kvp in playerPlacements)
            GameManager.Instance.PlayerPlacements[kvp.Key] = kvp.Value;
        foreach (var kvp in enemyPlacements)
            GameManager.Instance.EnemyPlacements[kvp.Key] = kvp.Value;
    }

    private void CleanupBattle()
    {
        foreach (var soldier in FindObjectsByType<Soldier>(FindObjectsSortMode.None))
            Destroy(soldier.gameObject);
        foreach (var arrow in FindObjectsByType<Arrow>(FindObjectsSortMode.None))
            Destroy(arrow.gameObject);

        // 경계선 제거
        var bounds = GameObject.Find("FieldBounds");
        if (bounds != null) Destroy(bounds);
    }

    private void ResetAllTiles()
    {
        for (int col = 0; col < columns; col++)
            for (int row = 0; row < rows; row++)
            {
                var tile = tiles[col, row];
                if (tile != null) tile.ResetForBattle();
            }
    }

    private void RandomPlace(Deck deck, Dictionary<Vector2Int, CardData> placements)
    {
        List<Vector2Int> available = new();
        for (int r = 0; r < 5; r++)
            for (int c = 0; c < 5; c++)
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

    // === 좌표 ===

    private Vector3 GetFieldCenter()
    {
        float x = (columns - 1) * (tileSize + tileGap) / 2f;
        float z = (rows - 1) * (tileSize + tileGap) / 2f;
        return new Vector3(x, 0, z);
    }

    private Vector3 GetTileWorldPosition(int col, int row)
    {
        return new Vector3(col * (tileSize + tileGap), 0, row * (tileSize + tileGap));
    }

    // === 카메라 조작 ===

    private bool camDragging;
    private Vector2 camDragOrigin;

    private void UpdateCamera()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.rightButton.wasPressedThisFrame)
        {
            camDragging = true;
            camDragOrigin = mouse.position.ReadValue();
        }
        if (mouse.rightButton.wasReleasedThisFrame)
            camDragging = false;

        if (camDragging)
        {
            Vector2 delta = (Vector2)mouse.position.ReadValue() - camDragOrigin;
            camDragOrigin = mouse.position.ReadValue();

            Vector3 right = mainCamera.transform.right;
            Vector3 forward = Vector3.Cross(right, Vector3.up).normalized;
            float panScale = 0.012f;
            mainCamera.transform.position -= right * delta.x * panScale;
            mainCamera.transform.position -= forward * delta.y * panScale;
        }

        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float fov = mainCamera.fieldOfView - scroll * 2f;
            mainCamera.fieldOfView = Mathf.Clamp(fov, 15f, 70f);
        }
    }
}
