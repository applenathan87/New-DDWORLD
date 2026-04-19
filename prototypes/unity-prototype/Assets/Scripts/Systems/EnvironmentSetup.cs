using UnityEngine;

/// <summary>
/// 전장 환경 비주얼: 잔디 바닥 + 나무 배치
/// BattleField과 함께 사용. 게임 로직 없음, 순수 비주얼.
/// </summary>
public class EnvironmentSetup : MonoBehaviour
{
    [Header("타일 설정")]
    public float tileSize = 1f;
    public float tileGap = 0.08f;
    public int gridColumns = 14;
    public int gridRows = 5;
    public int borderSize = 1; // 전투 그리드 바깥 여백 (칸)

    [Header("나무 설정")]
    public float treeScale = 1.5f;
    public int treesPerSide = 12; // 한 변당 나무 수
    public float treeRandomOffset = 0.3f; // 위치 랜덤 오프셋

    private float tileStep;
    private Texture2D tilemapTex;
    private Texture2D[] treeTex;

    private void Start()
    {
        tileStep = tileSize + tileGap;

        // Resources에서 텍스처 로드
        tilemapTex = Resources.Load<Texture2D>("Sprites/grass");
        if (tilemapTex == null)
            tilemapTex = Resources.Load<Texture2D>("Sprites/Tilemap_color1"); // 폴백
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
    /// </summary>
    private void CreateGrassGround()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Quad);
        ground.name = "GrassGround";
        ground.transform.SetParent(transform);

        // 전장 중심 계산
        float centerX = (gridColumns - 1) * tileStep / 2f;
        float centerZ = (gridRows - 1) * tileStep / 2f;

        // 잔디밭 크기: 전투 그리드 + 여백 + 약간의 여유
        float groundWidth = (gridColumns + borderSize * 2 + 2) * tileStep;
        float groundHeight = (gridRows + borderSize * 2 + 2) * tileStep;

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

        // 상단 (적 진영 뒤)
        PlaceTreeRow(treeParent, fieldLeft, fieldRight, fieldTop + 0.5f, 1, treesPerSide, ref treeId);
        PlaceTreeRow(treeParent, fieldLeft, fieldRight, fieldTop + 2.0f, 1, treesPerSide + 3, ref treeId);

        // 하단 (플레이어 진영 뒤)
        PlaceTreeRow(treeParent, fieldLeft, fieldRight, fieldBottom - 0.5f, -1, treesPerSide, ref treeId);
        PlaceTreeRow(treeParent, fieldLeft, fieldRight, fieldBottom - 2.0f, -1, treesPerSide + 3, ref treeId);

        // 좌측
        PlaceTreeColumn(treeParent, fieldLeft - 0.5f, fieldBottom - 1f, fieldTop + 1f, -1, 8, ref treeId);
        PlaceTreeColumn(treeParent, fieldLeft - 2.0f, fieldBottom - 2f, fieldTop + 2f, -1, 10, ref treeId);

        // 우측
        PlaceTreeColumn(treeParent, fieldRight + 0.5f, fieldBottom - 1f, fieldTop + 1f, 1, 8, ref treeId);
        PlaceTreeColumn(treeParent, fieldRight + 2.0f, fieldBottom - 2f, fieldTop + 2f, 1, 10, ref treeId);
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

        // 카메라를 향하도록 빌보드 (쿼터뷰에 맞게 X축만 회전)
        // 쿼터뷰 카메라 각도에 맞춰 약간 기울임
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
