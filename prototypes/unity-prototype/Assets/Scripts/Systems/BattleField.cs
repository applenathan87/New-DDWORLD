using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using TMPro;

/// <summary>
/// 전장 카메라 컨트롤러.
/// 쿼터뷰 카메라를 설정하고, 페이즈에 따라 줌인/줌아웃.
/// 전장 구조: 플레이어(A~E) | 중립(F~I) | 적(J~N) × 5행
/// </summary>
public class BattleField : MonoBehaviour
{
    public static BattleField Instance { get; private set; }

    [Header("전장 크기")]
    public int columns = 14;  // A~N
    public int rows = 5;      // 1~5
    public float tileSize = 1f;
    public float tileGap = 0.08f;

    [Header("폰트 (밸런스 테스트 씬용)")]
    public TMP_FontAsset koreanFont;

    [Header("카메라 설정")]
    public float cameraMoveDuration = 1f;

    [Header("전투 페이즈 카메라 (전체 맵)")]
    public Vector3 battleCamPos = new Vector3(7.2f, 5.4f, -5.48f);
    public Vector3 battleCamRot = new Vector3(29.2f, 0f, 0f);
    public float battleFOV = 60f;

    [Header("배치 페이즈 카메라 (내 진영 줌인)")]
    public Vector3 placementCamPos = new Vector3(2.32f, 3.8f, -4.11f);
    public Vector3 placementCamRot = new Vector3(35f, 0f, 0f);
    public float placementFOV = 50f;

    private Camera mainCamera;
    private GameObject debugGrid;
    private BattleTile[,] battleTiles;

    // 전투 중 카메라 자유 이동
    private bool freeCamEnabled;
    private Vector3 dragOrigin;
    private bool isDragging;
    private const float PAN_SPEED = 0.015f;

    // 줌 단계 (0=전체, 1~3=줌인)
    private int zoomLevel = 0;
    private float scrollAccum = 0f;
    private const float SCROLL_THRESHOLD = 1.5f;
    private static readonly float[] ZOOM_FOVS = { 60f, 45f, 32f, 20f }; // 0~3단계

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        mainCamera = Camera.main;
        SetupCamera();
        CreateGroundPlane();
        CreateDebugGrid();

        // 페이즈 변경 이벤트 구독
        GameManager.Instance.OnPhaseChanged += OnPhaseChanged;
    }

    private void SetupCamera()
    {
        if (mainCamera == null) return;

        mainCamera.orthographic = false;
        mainCamera.fieldOfView = battleFOV;
        mainCamera.transform.position = battleCamPos;
        mainCamera.transform.rotation = Quaternion.Euler(battleCamRot);

        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = new Color(0.08f, 0.08f, 0.12f);
    }

    /// <summary>
    /// 그리드 바깥을 채우는 바닥면
    /// </summary>
    private void CreateGroundPlane()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Quad);
        ground.name = "Ground";
        ground.transform.SetParent(transform);
        ground.transform.position = new Vector3(GetFieldCenter().x, -0.01f, GetFieldCenter().z);
        ground.transform.rotation = Quaternion.Euler(90, 0, 0);
        ground.transform.localScale = new Vector3(50f, 50f, 1); // 충분히 크게

        var renderer = ground.GetComponent<Renderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.color = new Color(0.12f, 0.12f, 0.15f);
        renderer.material = mat;

        Destroy(ground.GetComponent<Collider>());
    }

    /// <summary>
    /// 전장 타일 그리드 생성
    /// </summary>
    private void CreateDebugGrid()
    {
        debugGrid = new GameObject("DebugGrid");
        debugGrid.transform.SetParent(transform);
        battleTiles = new BattleTile[columns, rows];

        Color playerColor = new Color(0.3f, 0.5f, 0.8f, 0.6f);
        Color neutralColor = new Color(0.5f, 0.5f, 0.5f, 0.4f);
        Color enemyColor = new Color(0.8f, 0.3f, 0.3f, 0.6f);

        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                var tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tile.name = $"{(char)('A' + col)}{row + 1}";
                tile.transform.SetParent(debugGrid.transform);
                tile.transform.position = GetTileWorldPosition(col, row);
                tile.transform.rotation = Quaternion.Euler(90, 0, 0);
                tile.transform.localScale = new Vector3(tileSize * 0.95f, tileSize * 0.95f, 1);

                // 구역별 색상
                bool isPlayer = col < 5;
                bool isNeutral = col >= 5 && col < 9;
                Color color = isPlayer ? playerColor : isNeutral ? neutralColor : enemyColor;

                if ((col + row) % 2 == 1) color *= 0.8f;
                color.a = isNeutral ? 0.3f : 0.5f;

                var renderer = tile.GetComponent<Renderer>();
                var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                mat.color = color;
                mat.SetFloat("_Surface", 1);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 3000;
                renderer.material = mat;

                // 플레이어 구역: BattleTile + 콜라이더 (드래그 앤 드롭용)
                // 적 구역: BattleTile (유닛 표시용, 콜라이더 없음)
                bool isEnemyZone = col >= 9;
                if (isPlayer || isEnemyZone)
                {
                    Destroy(tile.GetComponent<Collider>());

                    if (isPlayer)
                    {
                        var boxCol = tile.AddComponent<BoxCollider>();
                        boxCol.size = new Vector3(1, 1, 0.1f);
                    }

                    var battleTile = tile.AddComponent<BattleTile>();
                    if (koreanFont != null)
                        battleTile.koreanFont = koreanFont;
                    else if (Hand3D.Instance != null)
                        battleTile.koreanFont = Hand3D.Instance.koreanFont;
                    else if (PlacementUI.Instance != null)
                        battleTile.koreanFont = PlacementUI.Instance.koreanFont;
                    battleTile.Setup(col, row, isPlayer, color, renderer);
                    battleTiles[col, row] = battleTile;
                }
                else
                {
                    Destroy(tile.GetComponent<Collider>());
                }
            }
        }
    }

    /// <summary>
    /// 마우스 위치에서 레이캐스트하여 BattleTile 반환
    /// </summary>
    public BattleTile GetTileUnderMouse()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            return hit.collider.GetComponent<BattleTile>();
        }
        return null;
    }

    /// <summary>
    /// 특정 위치의 BattleTile 반환
    /// </summary>
    public BattleTile GetBattleTile(int col, int row)
    {
        if (col < 0 || col >= columns || row < 0 || row >= rows) return null;
        return battleTiles[col, row];
    }

    /// <summary>
    /// 카메라를 부드럽게 이동 (위치, 회전, FOV)
    /// </summary>
    public void MoveCameraTo(Vector3 pos, Vector3 rot, float fov, float duration = -1f)
    {
        if (mainCamera == null) return;
        if (duration < 0) duration = cameraMoveDuration;

        mainCamera.transform.DOMove(pos, duration).SetEase(Ease.InOutCubic);
        mainCamera.transform.DORotateQuaternion(Quaternion.Euler(rot), duration).SetEase(Ease.InOutCubic);
        DOTween.To(() => mainCamera.fieldOfView,
            x => mainCamera.fieldOfView = x,
            fov, duration).SetEase(Ease.InOutCubic);
    }

    /// <summary>
    /// 배치 페이즈: 내 진영 줌인
    /// </summary>
    public void ZoomToPlayerField()
    {
        MoveCameraTo(placementCamPos, placementCamRot, placementFOV);
    }

    /// <summary>
    /// 전투 페이즈: 전체 맵 줌아웃
    /// </summary>
    public void ZoomToFullField()
    {
        MoveCameraTo(battleCamPos, battleCamRot, battleFOV);
    }

    /// <summary>
    /// 적 배치 데이터를 적 구역 타일에 시각적으로 표시
    /// EnemyPlacements의 (row, col) 0~4를 적 구역 col 9~13으로 매핑
    /// </summary>
    public void ShowEnemyPlacements()
    {
        var placements = GameManager.Instance.EnemyPlacements;
        foreach (var kvp in placements)
        {
            int placementRow = kvp.Key.x;
            int placementCol = kvp.Key.y;
            CardData card = kvp.Value;

            // 적 배치 (0~4) → 전장 적 구역 (col 9~13), 행은 반전 (적 시점)
            int gridCol = 9 + (4 - placementCol);
            int gridRow = placementRow;

            var tile = GetBattleTile(gridCol, gridRow);
            if (tile != null)
            {
                tile.PlaceUnitFromData(card);
                Debug.Log($"[적 배치 표시] {card.unitData.unitName} → {(char)('A' + gridCol)}{gridRow + 1}");
            }
        }
    }

    private void OnPhaseChanged(GameManager.GamePhase phase)
    {
        switch (phase)
        {
            case GameManager.GamePhase.Draw:
            case GameManager.GamePhase.Placement:
                freeCamEnabled = false;
                ZoomToPlayerField();
                break;
            case GameManager.GamePhase.Battle:
                ZoomToFullField();
                ShowEnemyPlacements();
                // 줌아웃 완료 후 자유 카메라 활성화
                Invoke(nameof(EnableFreeCam), cameraMoveDuration + 0.1f);
                break;
        }
    }

    private void EnableFreeCam()
    {
        freeCamEnabled = true;
        zoomLevel = 0;
        scrollAccum = 0f;

        // 전장 전체를 보여준 뒤 2단계로 줌인
        Invoke(nameof(AutoZoomToBattle), 1.5f);
    }

    private void AutoZoomToBattle()
    {
        zoomLevel = 1;
        Vector3 center = GetFieldCenter();
        Vector3 targetPos = mainCamera.transform.position;
        targetPos.x = center.x;
        targetPos.z = -7.11f;
        mainCamera.transform.DOMove(targetPos, 0.8f).SetEase(Ease.InOutCubic);
        DOTween.To(() => mainCamera.fieldOfView,
            x => mainCamera.fieldOfView = x,
            ZOOM_FOVS[1], 0.8f).SetEase(Ease.InOutCubic);
    }

    private void Update()
    {
        if (!freeCamEnabled || mainCamera == null) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        // 마우스 우클릭 드래그 → 패닝
        if (mouse.rightButton.wasPressedThisFrame)
        {
            isDragging = true;
            dragOrigin = mouse.position.ReadValue();
        }
        if (mouse.rightButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }
        if (isDragging)
        {
            Vector2 delta = (Vector2)mouse.position.ReadValue() - (Vector2)dragOrigin;
            dragOrigin = mouse.position.ReadValue();

            Vector3 right = mainCamera.transform.right;
            Vector3 forward = Vector3.Cross(right, Vector3.up).normalized;

            float panScale = PAN_SPEED * (ZOOM_FOVS[zoomLevel] / 60f);
            mainCamera.transform.position -= right * delta.x * panScale;
            mainCamera.transform.position -= forward * delta.y * panScale;
        }

        // 스크롤 → 줌 단계 전환
        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            scrollAccum += scroll;

            if (scrollAccum >= SCROLL_THRESHOLD && zoomLevel < ZOOM_FOVS.Length - 1)
            {
                zoomLevel++;
                scrollAccum = 0f;
                DOTween.To(() => mainCamera.fieldOfView,
                    x => mainCamera.fieldOfView = x,
                    ZOOM_FOVS[zoomLevel], 0.3f).SetEase(Ease.OutCubic);
            }
            else if (scrollAccum <= -SCROLL_THRESHOLD && zoomLevel > 0)
            {
                zoomLevel--;
                scrollAccum = 0f;
                DOTween.To(() => mainCamera.fieldOfView,
                    x => mainCamera.fieldOfView = x,
                    ZOOM_FOVS[zoomLevel], 0.3f).SetEase(Ease.OutCubic);
            }
        }
    }

    // === 좌표 계산 ===

    public Vector3 GetFieldCenter()
    {
        float centerX = (columns - 1) * (tileSize + tileGap) / 2f;
        float centerZ = (rows - 1) * (tileSize + tileGap) / 2f;
        return new Vector3(centerX, 0, centerZ);
    }

    public Vector3 GetPlayerZoneCenter()
    {
        float centerX = 2 * (tileSize + tileGap); // A~E 중심 (C열)
        float centerZ = (rows - 1) * (tileSize + tileGap) / 2f;
        return new Vector3(centerX, 0, centerZ);
    }

    public Vector3 GetTileWorldPosition(int col, int row)
    {
        float x = col * (tileSize + tileGap);
        float z = row * (tileSize + tileGap);
        return new Vector3(x, 0, z);
    }
}
