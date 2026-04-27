using UnityEngine;

/// <summary>
/// 전장 환경 비주얼: 잔디 바닥 + 나무 배치
/// BattleField과 함께 사용. 게임 로직 없음, 순수 비주얼.
/// </summary>
public class EnvironmentSetup : MonoBehaviour
{
    [Header("타일 설정")]
    public float tileSize = 1f;
    public float tileGap = 0f;
    public int gridColumns = 14;
    public int gridRows = 5;
    public int borderSize = 1; // 전투 그리드 바깥 여백 (칸)

    [Header("나무 설정")]
    public float treeScale = 3f;
    public int treesPerSide = 12; // 한 변당 나무 수
    public float treeRandomOffset = 0.3f; // 위치 랜덤 오프셋

    [Header("나무 가림 방지 (하단)")]
    [Tooltip("카메라 가까운 쪽(하단) 가운데 비울 영역 너비. 필드 가림 방지용")]
    public float bottomClearMargin = -4f;
    [Tooltip("하단 좌/우 가장자리에 배치할 나무 수 (각 변, 각 줄)")]
    public int bottomEdgeTreeCount = 30;
    [Tooltip("하단 나무 줄 수 (카메라 시야 내에 보이는 줄)")]
    public int bottomTreeRows = 5;
    [Tooltip("하단 줄 간격")]
    public float bottomTreeRowSpacing = 1.5f;

    [Header("배치 밀도 (상단)")]
    [Tooltip("카메라 먼쪽(상단) 나무 줄 수. 클수록 깊이감 + 가득 차는 느낌")]
    public int topTreeRows = 8;
    [Tooltip("상단 줄 간격 (Z축)")]
    public float topTreeRowSpacing = 1.5f;
    [Tooltip("상단 좌/우 코너 추가 확장 폭 (코너가 비어 보일 때 늘림)")]
    public float topCornerExtension = 4f;

    [Header("배치 밀도 (좌우)")]
    [Tooltip("좌/우 측면 나무 줄 수. 클수록 측면이 빽빽한 숲처럼 보임")]
    public int sideTreeRows = 10;
    [Tooltip("좌/우 줄 간격 (X축)")]
    public float sideTreeRowSpacing = 1f;

    private float tileStep;
    private Texture2D tilemapTex;
    private Texture2D[] treeTex;

    private void Start()
    {
        tileStep = tileSize + tileGap;

        // 바닥 텍스처: basic_tile_v2를 기본으로 사용. 누락 시 폴백 순차 시도.
        string[] groundTextureCandidates = { "Sprites/basic_tile_v2", "Sprites/basic_tile", "Sprites/Tilemap_color1" };
        foreach (var path in groundTextureCandidates)
        {
            tilemapTex = Resources.Load<Texture2D>(path);
            if (tilemapTex != null) break;
        }
        treeTex = new Texture2D[4];
        for (int i = 0; i < 4; i++)
            treeTex[i] = Resources.Load<Texture2D>($"Sprites/Tree{i + 1}");

        if (tilemapTex == null)
        {
            Debug.LogError("Tilemap_color1 텍스처를 찾을 수 없습니다! Assets/Resources/Sprites/에 있는지 확인하세요.");
            return;
        }

        CreateGrassGround();
        CreateTrees();
    }

    /// <summary>
    /// 잔디 바닥 생성: 경계 없는 하나의 넓은 잔디밭
    /// 타일맵 텍스처를 타일링하여 자연스러운 잔디 표면
    /// 나무 영역까지 모두 덮도록 충분히 크게 (BattleField의 단색 ground 50x50을 완전히 덮음)
    /// </summary>
    private void CreateGrassGround()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Quad);
        ground.name = "GrassGround";
        ground.transform.SetParent(transform);

        // 전장 중심 계산
        float centerX = (gridColumns - 1) * tileStep / 2f;
        float centerZ = (gridRows - 1) * tileStep / 2f;

        // 잔디밭 크기: BattleField의 단색 Ground(50x50)와 동일하게 덮어서
        // 나무 사이/뒤편 영역에도 잔디 텍스처가 보이도록 함
        float groundWidth = 50f;
        float groundHeight = 50f;

        ground.transform.position = new Vector3(centerX, -0.02f, centerZ);
        ground.transform.rotation = Quaternion.Euler(90, 0, 0);
        ground.transform.localScale = new Vector3(groundWidth, groundHeight, 1);

        Destroy(ground.GetComponent<Collider>());

        // 심리스 잔디 텍스처를 타일링 (Unlit: 조명 영향 없이 원본 색 유지)
        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.mainTexture = tilemapTex;
        mat.SetFloat("_Smoothness", 0f);
        mat.color = Color.white;

        // 심리스 텍스처: 전체를 타일링 (타일 1칸 = 텍스처 1반복)
        float tilesX = groundWidth / tileStep;
        float tilesZ = groundHeight / tileStep;
        mat.mainTextureScale = new Vector2(tilesX, tilesZ);
        mat.mainTextureOffset = Vector2.zero;

        ground.GetComponent<Renderer>().material = mat;
    }

    /// <summary>
    /// 나무 배치: 전장 둘레에 빌보드 스프라이트로 배치
    /// </summary>
    private void CreateTrees()
    {
        if (treeTex[0] == null)
        {
            Debug.LogWarning("Tree 텍스처를 찾을 수 없습니다!");
            return;
        }

        var treeParent = new GameObject("Trees");
        treeParent.transform.SetParent(transform);

        // 전장 경계 계산
        float fieldLeft = -borderSize * tileStep - tileStep;
        float fieldRight = (gridColumns + borderSize) * tileStep;
        float fieldBottom = -borderSize * tileStep - tileStep;
        float fieldTop = (gridRows + borderSize) * tileStep;

        int treeId = 0;

        // 상단 (적 진영 뒤) — 카메라 먼쪽, 깊이감 있게 가득
        // 줄이 멀어질수록 더 많이 + 더 넓게 배치하여 자연스러운 숲 느낌
        // 코너 비주얼: 모든 상단 줄이 좌/우 코너까지 확장되어 좌상단/우상단 빈 공간 채움
        for (int i = 0; i < topTreeRows; i++)
        {
            float zOffset = 0.5f + i * topTreeRowSpacing;
            // 줄이 멀어질수록 좌우로도 점진적 확장 + 기본 코너 확장
            float widen = topCornerExtension + i * 0.6f;
            // 줄이 멀수록 더 많이 (원근감)
            int count = treesPerSide + 4 + i * 2;
            PlaceTreeRow(treeParent, fieldLeft - widen, fieldRight + widen, fieldTop + zOffset, 1, count, ref treeId);
        }

        // 하단 (플레이어 진영 뒤) — bottomClearMargin으로 가운데 비움 폭 조절
        // - 양수: 필드 좌우 경계보다 더 넓게 비움 (필드 잘 보임)
        // - 0: 필드 좌우 경계까지 비움
        // - 음수: 가운데까지 채움 (음수가 커질수록 더 빽빽)
        float clearLeft = fieldLeft - bottomClearMargin;
        float clearRight = fieldRight + bottomClearMargin;
        for (int i = 0; i < bottomTreeRows; i++)
        {
            float zOffset = 1.5f + i * bottomTreeRowSpacing;
            float widen = i * 0.6f;
            // 줄이 멀어질수록 가운데 비우는 영역도 약간 좁힘 (멀리 있는 나무는 시야 가림 적음)
            float clearShrink = i * 0.3f;
            PlaceTreeRowEdgesOnly(
                treeParent,
                fieldLeft - 2f - widen,
                fieldRight + 2f + widen,
                clearLeft + clearShrink,
                clearRight - clearShrink,
                fieldBottom - zOffset,
                bottomEdgeTreeCount + i,
                ref treeId);
        }

        // 좌/우 컬럼은 상단 끝까지 충분히 올려서 코너가 끊겨 보이지 않게 함
        float topReach = fieldTop + 0.5f + (topTreeRows - 1) * topTreeRowSpacing;
        float bottomReach = fieldBottom - 1.5f - (bottomTreeRows - 1) * bottomTreeRowSpacing;

        // 좌측 — 여러 줄로 빽빽하게
        for (int i = 0; i < sideTreeRows; i++)
        {
            float xOffset = 0.5f + i * sideTreeRowSpacing;
            int count = 8 + topTreeRows + i * 2;
            PlaceTreeColumn(treeParent, fieldLeft - xOffset, bottomReach, topReach + i * 0.5f, -1, count, ref treeId);
        }

        // 우측 — 여러 줄로 빽빽하게
        for (int i = 0; i < sideTreeRows; i++)
        {
            float xOffset = 0.5f + i * sideTreeRowSpacing;
            int count = 8 + topTreeRows + i * 2;
            PlaceTreeColumn(treeParent, fieldRight + xOffset, bottomReach, topReach + i * 0.5f, 1, count, ref treeId);
        }
    }

    /// <summary>
    /// 가로줄 배치 — 가운데 영역(clearMin~clearMax)은 비우고 좌/우 가장자리에만 배치.
    /// 카메라 가까운 쪽에서 필드 시야를 가리지 않도록 사용.
    /// </summary>
    private void PlaceTreeRowEdgesOnly(GameObject parent, float xMin, float xMax, float clearMin, float clearMax, float z, int countPerSide, ref int id)
    {
        // 좌측 영역 (xMin → clearMin)
        for (int i = 0; i < countPerSide; i++)
        {
            float t = (float)i / Mathf.Max(countPerSide - 1, 1);
            float x = Mathf.Lerp(xMin, clearMin, t) + Random.Range(-treeRandomOffset, treeRandomOffset);
            float zPos = z + Random.Range(-treeRandomOffset, treeRandomOffset);
            CreateTreeBillboard(parent, new Vector3(x, 0, zPos), ref id);
        }
        // 우측 영역 (clearMax → xMax)
        for (int i = 0; i < countPerSide; i++)
        {
            float t = (float)i / Mathf.Max(countPerSide - 1, 1);
            float x = Mathf.Lerp(clearMax, xMax, t) + Random.Range(-treeRandomOffset, treeRandomOffset);
            float zPos = z + Random.Range(-treeRandomOffset, treeRandomOffset);
            CreateTreeBillboard(parent, new Vector3(x, 0, zPos), ref id);
        }
    }

    private void PlaceTreeRow(GameObject parent, float xMin, float xMax, float z, int facing, int count, ref int id)
    {
        float spacing = (xMax - xMin) / Mathf.Max(count - 1, 1);
        for (int i = 0; i < count; i++)
        {
            float x = xMin + i * spacing + Random.Range(-treeRandomOffset, treeRandomOffset);
            float zPos = z + Random.Range(-treeRandomOffset, treeRandomOffset);
            CreateTreeBillboard(parent, new Vector3(x, 0, zPos), ref id);
        }
    }

    private void PlaceTreeColumn(GameObject parent, float x, float zMin, float zMax, int facing, int count, ref int id)
    {
        float spacing = (zMax - zMin) / Mathf.Max(count - 1, 1);
        for (int i = 0; i < count; i++)
        {
            float xPos = x + Random.Range(-treeRandomOffset, treeRandomOffset);
            float z = zMin + i * spacing + Random.Range(-treeRandomOffset, treeRandomOffset);
            CreateTreeBillboard(parent, new Vector3(xPos, 0, z), ref id);
        }
    }

    private void CreateTreeBillboard(GameObject parent, Vector3 position, ref int id)
    {
        // 랜덤 나무 종류 선택 (4종)
        int treeType = Random.Range(0, 4);
        var tex = treeTex[treeType];
        if (tex == null) return;

        // 스프라이트 시트에서 첫 번째 프레임만 사용
        // Tree1,2: 1536x256, 8프레임 → 1프레임 = 192x256
        // Tree3,4: 1536x192, 8프레임 → 1프레임 = 192x192
        float frameWidth = 1f / 8f; // UV 기준 1/8

        var treeObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        treeObj.name = $"Tree_{id++}";
        treeObj.transform.SetParent(parent.transform);
        Destroy(treeObj.GetComponent<Collider>());

        // 나무 크기 (종류에 따라)
        float aspect = (float)tex.height / (tex.width / 8f); // 프레임 비율
        float width = treeScale * (0.8f + Random.Range(0f, 0.4f));
        float height = width * aspect;

        treeObj.transform.position = position + Vector3.up * (height * 0.5f);
        treeObj.transform.localScale = new Vector3(width, height, 1);

        // HD-2D 스타일: 나무는 항상 수직으로 서있고, 쿼터뷰 카메라가 알아서 비스듬히 보여줌
        // (옥토패스 트래블러, Stardew Valley 등 정통 픽셀 아트 스프라이트 처리 방식)
        treeObj.transform.rotation = Quaternion.Euler(0, 0, 0);

        // 투명 머티리얼 + 스프라이트 시트 첫 프레임 UV
        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.mainTexture = tex;

        // 투명도 활성화
        mat.SetFloat("_Surface", 1); // Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3000;
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

        // 첫 번째 프레임만 표시 (UV: 0~1/8)
        mat.mainTextureScale = new Vector2(frameWidth, 1f);
        mat.mainTextureOffset = new Vector2(0, 0);

        treeObj.GetComponent<Renderer>().material = mat;
    }
}
