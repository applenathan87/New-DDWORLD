using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 3D 전장의 개별 타일. 유닛 배치/표시를 담당.
/// </summary>
public class BattleTile : MonoBehaviour
{
    public int col;
    public int row;
    public bool isPlayerZone;

    private Renderer tileRenderer;
    private Color baseColor;
    private Color hoverColor;
    private CardUI placedCard;
    private Card3D placedCard3D;

    // 타일 위에 표시되는 유닛 정보 (3D 텍스트)
    private TextMeshPro unitLabel;

    // 3D 유닛 (캡슐)
    private List<GameObject> spawnedSoldiers = new();
    private List<Soldier> soldiers = new();

    /// <summary>
    /// 이 타일에 배치된 살아있는 병사 목록
    /// </summary>
    public List<Soldier> Soldiers => soldiers;

    public bool IsOccupied => placedCard != null || placedCard3D != null;
    public CardUI PlacedCard => placedCard;

    /// <summary>
    /// 적 방향 부호 (로컬 X 기준). 플레이어: +1(오른쪽), 적: -1(왼쪽)
    /// 모든 진형 메서드에서 이것만 사용할 것.
    /// </summary>
    private float EnemyDir => isPlayerZone ? 1f : -1f;

    public TMP_FontAsset koreanFont;
    private Color originalColor; // Setup 시 저장한 원래 색

    /// <summary>
    /// 전투 시작 시 타일을 원래 그리드 색으로 리셋 (병종 색/라벨 제거)
    /// </summary>
    public void ResetForBattle()
    {
        baseColor = originalColor;
        hoverColor = originalColor + new Color(0.15f, 0.15f, 0.15f, 0);
        tileRenderer.material.color = originalColor;
        if (unitLabel != null)
            unitLabel.gameObject.SetActive(false);
    }

    public void Setup(int col, int row, bool isPlayerZone, Color color, Renderer renderer)
    {
        this.col = col;
        this.row = row;
        this.isPlayerZone = isPlayerZone;
        this.tileRenderer = renderer;
        this.baseColor = color;
        this.originalColor = color;
        this.hoverColor = color + new Color(0.15f, 0.15f, 0.15f, 0);

        // 유닛 라벨 (사용하지 않지만 참조 유지)
        var labelObj = new GameObject("UnitLabel");
        labelObj.transform.SetParent(transform);
        labelObj.SetActive(false);

        unitLabel = labelObj.AddComponent<TextMeshPro>();
        unitLabel.text = "";
        unitLabel.raycastTarget = false;
        if (koreanFont != null) unitLabel.font = koreanFont;
    }

    /// <summary>
    /// 유닛 배치
    /// </summary>
    public void PlaceUnit(CardUI card)
    {
        placedCard = card;
        unitLabel.text = card.cardData.unitData.unitName + "\n" + card.cardData.unitData.soldierCount;

        // 타일 색상을 병종 색상으로 변경
        Color unitColor = GetUnitColor(card.cardData.unitData);
        baseColor = unitColor;
        hoverColor = unitColor + new Color(0.15f, 0.15f, 0.15f, 0);
        tileRenderer.material.color = unitColor;
    }

    /// <summary>
    /// 유닛 제거
    /// </summary>
    public CardUI RemoveUnit()
    {
        var card = placedCard;
        placedCard = null;
        unitLabel.text = "";

        // 원래 구역 색상으로 복원
        Color zoneColor = isPlayerZone
            ? new Color(0.3f, 0.5f, 0.8f, 0.6f)
            : new Color(0.5f, 0.5f, 0.5f, 0.4f);
        if ((col + row) % 2 == 1) zoneColor *= 0.8f;
        zoneColor.a = isPlayerZone ? 0.5f : 0.3f;

        baseColor = zoneColor;
        hoverColor = zoneColor + new Color(0.15f, 0.15f, 0.15f, 0);
        tileRenderer.material.color = zoneColor;

        return card;
    }

    private void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (IsOccupied && placedCard3D != null && Hand3D.Instance != null)
        {
            // 배치된 카드를 픽업하여 드래그 시작 (타일→타일 이동 가능)
            Hand3D.Instance.PickUpFromTile(this);
        }
    }

    // === Card3D 지원 ===

    public void PlaceUnit3D(Card3D card)
    {
        placedCard3D = card;
        var data = card.cardData.unitData;
        string unit = data.category == UnitCategory.Special ? "개" : "명";
        unitLabel.text = data.unitName + "\n" + data.soldierCount + unit;

        Color unitColor = GetUnitColor(data);
        baseColor = unitColor;
        hoverColor = unitColor + new Color(0.15f, 0.15f, 0.15f, 0);
        tileRenderer.material.color = unitColor;

        // 3D 유닛(캡슐) 생성
        SpawnSoldiers(data);
    }

    public Card3D RemoveUnit3D()
    {
        var card = placedCard3D;
        placedCard3D = null;
        unitLabel.text = "";

        // 캡슐 제거
        ClearSoldiers();

        Color zoneColor = isPlayerZone
            ? new Color(0.3f, 0.5f, 0.8f, 0.6f)
            : new Color(0.5f, 0.5f, 0.5f, 0.4f);
        if ((col + row) % 2 == 1) zoneColor *= 0.8f;
        zoneColor.a = isPlayerZone ? 0.5f : 0.3f;

        baseColor = zoneColor;
        hoverColor = zoneColor + new Color(0.15f, 0.15f, 0.15f, 0);
        tileRenderer.material.color = zoneColor;

        return card;
    }

    /// <summary>
    /// CardData만으로 유닛 표시 (Card3D 없이, 적 배치용)
    /// </summary>
    public void PlaceUnitFromData(CardData data)
    {
        var unit = data.unitData;
        string suffix = unit.category == UnitCategory.Special ? "개" : "명";
        unitLabel.text = unit.unitName + "\n" + unit.soldierCount + suffix;
        unitLabel.gameObject.SetActive(true);

        Color unitColor = GetUnitColor(unit);
        baseColor = unitColor;
        hoverColor = unitColor + new Color(0.15f, 0.15f, 0.15f, 0);
        tileRenderer.material.color = unitColor;

        SpawnSoldiers(unit);
    }

    public void SetHover(bool hover)
    {
        if (tileRenderer != null)
            tileRenderer.material.color = hover ? hoverColor : baseColor;
    }

    // === 3D 유닛 생성 ===

    // 고정 캡슐 크기
    private const float SOLDIER_RADIUS = 0.06f;
    private const float SOLDIER_HEIGHT = 0.15f;
    private const float OUTLINE_THICKNESS = 1.3f; // 아웃라인 배율

    private void SpawnSoldiers(UnitData data)
    {
        ClearSoldiers();

        int count = data.soldierCount;
        float tileSize = BattleField.Instance != null ? BattleField.Instance.tileSize : 1f;
        float margin = tileSize * 0.1f;
        float usable = tileSize - margin * 2f;

        // 병종별 배치 좌표 생성
        List<Vector2> positions;
        if (data.category == UnitCategory.Cavalry)
            positions = GetWedgePositions(count, usable);
        else if (data.category == UnitCategory.Ranged)
            positions = GetStaggeredPositions(count, usable);
        else if (data.category == UnitCategory.Melee && data.rpsType == RpsType.Scissors)
            positions = GetPhalanxPositions(count, usable);
        else
            positions = GetGridPositions(count, usable);

        Color soldierColor = GetSoldierColor(data);

        var shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        var mat = new Material(shader);
        mat.color = soldierColor;
        var outlineMat = new Material(shader);
        outlineMat.color = Color.black;

        float heightOffset = -SOLDIER_HEIGHT / 2f;

        for (int i = 0; i < positions.Count; i++)
        {
            float xOff = positions[i].x;
            float zOff = positions[i].y;

            // 캡슐 본체 (먼저 생성)
            var soldier = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            soldier.name = $"Soldier_{i}";
            soldier.transform.SetParent(transform);
            soldier.transform.localPosition = new Vector3(xOff, zOff, heightOffset);
            soldier.transform.localScale = new Vector3(
                SOLDIER_RADIUS * 2f,
                SOLDIER_HEIGHT * 0.5f,
                SOLDIER_RADIUS * 2f);
            soldier.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            soldier.GetComponent<Renderer>().material = mat;
            soldier.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Destroy(soldier.GetComponent<Collider>());
            spawnedSoldiers.Add(soldier);

            // 아웃라인 (병사의 자식 → 전투 시 같이 이동)
            var outline = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            outline.name = $"Outline_{i}";
            outline.transform.SetParent(soldier.transform);
            outline.transform.localPosition = Vector3.zero;
            outline.transform.localScale = Vector3.one * OUTLINE_THICKNESS;
            outline.transform.localRotation = Quaternion.identity;
            outline.GetComponent<Renderer>().material = outlineMat;
            outline.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Destroy(outline.GetComponent<Collider>());

            var soldierComp = soldier.AddComponent<Soldier>();
            soldierComp.Setup(data.soldierHP, soldierColor);
            soldiers.Add(soldierComp);
        }
    }

    /// <summary>
    /// 딱 맞는 직사각형 그리드 좌표 생성
    /// </summary>
    private List<Vector2> GetGridPositions(int count, float usable)
    {
        // 딱 떨어지는 그리드 선택 (세로 대형: rows > cols)
        int cols, rows;
        if (count <= 1)       { cols = 1; rows = 1; }
        else if (count <= 4)  { cols = 2; rows = 2; }
        else if (count <= 6)  { cols = 2; rows = 3; }
        else if (count <= 9)  { cols = 3; rows = 3; }
        else if (count == 10) { cols = 2; rows = 5; }
        else if (count <= 12) { cols = 3; rows = 4; }
        else if (count <= 16) { cols = 4; rows = 4; }
        else                  { cols = 4; rows = 5; }

        float spacingX = count > 1 ? usable / cols : 0;
        float spacingZ = count > 1 ? usable / rows : 0;

        var positions = new List<Vector2>();
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                if (positions.Count >= count) break;
                float x = (c - (cols - 1) * 0.5f) * spacingX;
                float z = (r - (rows - 1) * 0.5f) * spacingZ;
                positions.Add(new Vector2(x, z));
            }
        return positions;
    }

    /// <summary>
    /// 팔랑크스(방패벽) 대형 — 창병용
    /// 전열: 촘촘하게 밀집 (적 방향) / 후열: 살짝 넓은 간격 (내 진영쪽)
    /// </summary>
    private List<Vector2> GetPhalanxPositions(int count, float usable)
    {
        float dir = EnemyDir;
        int frontCount = (count + 1) / 2; // 전열 (반올림)
        int backCount = count - frontCount;

        float rowSpacing = usable * 0.3f;
        float frontSpacing = usable / (frontCount + 0.5f); // 전열: 넓게 (방패벽)
        float backSpacing = usable / (backCount + 2f);     // 후열: 촘촘 (밀집 지원)

        var positions = new List<Vector2>();

        // 전열 (적 방향)
        for (int i = 0; i < frontCount; i++)
        {
            float y = (i - (frontCount - 1) * 0.5f) * frontSpacing;
            positions.Add(new Vector2(rowSpacing * dir, y));
        }

        // 후열 (내 진영쪽)
        for (int i = 0; i < backCount; i++)
        {
            float y = (i - (backCount - 1) * 0.5f) * backSpacing;
            positions.Add(new Vector2(-rowSpacing * dir, y));
        }

        return positions;
    }

    /// <summary>
    /// 지그재그(staggered) 대형 — 궁병용 (4-3-3 등 줄마다 엇갈림)
    /// 뒷줄이 넓고, 앞줄로 갈수록 좁아지는 역삼각형
    /// </summary>
    private List<Vector2> GetStaggeredPositions(int count, float usable)
    {
        // 줄별 인원 배분: 뒤(내 진영쪽, 넓음) → 앞(적쪽, 좁음)
        // rowCounts[0] = 뒷줄, rowCounts[last] = 앞줄
        var rowCounts = new List<int>();
        int remaining = count;
        int maxPerRow = 4;
        while (remaining > 0)
        {
            int inRow = Mathf.Min(remaining, maxPerRow);
            rowCounts.Add(inRow);
            remaining -= inRow;
            if (maxPerRow > 2) maxPerRow = 3;
        }

        float dir = EnemyDir;
        float rowSpacing = usable / (rowCounts.Count + 1);
        float colSpacing = usable / 5f;

        var positions = new List<Vector2>();
        for (int r = 0; r < rowCounts.Count; r++)
        {
            int n = rowCounts[r];
            // r=0 뒷줄(내 진영쪽 = -dir), r=last 앞줄(적쪽 = +dir)
            float xOff = (r - (rowCounts.Count - 1) * 0.5f) * rowSpacing * dir;

            for (int i = 0; i < n; i++)
            {
                float y = (i - (n - 1) * 0.5f) * colSpacing;
                positions.Add(new Vector2(xOff, y));
            }
        }
        return positions;
    }

    /// <summary>
    /// 쐐기(V자) 진형 좌표 생성 — 상대쪽(+X)에 뾰족한 형태
    /// 타일 로컬 X = 열 방향 = 적 방향
    /// 플레이어 구역: +X가 상대 방향 / 적 구역: -X가 상대 방향
    /// </summary>
    private List<Vector2> GetWedgePositions(int count, float usable)
    {
        var positions = new List<Vector2>();
        float dir = EnemyDir;
        float rowSpacing = usable / 3f;

        int placed = 0;
        int rowIdx = 0;
        while (placed < count)
        {
            int inThisRow = (rowIdx == 0) ? 1 : 2;
            if (placed + inThisRow > count) inThisRow = count - placed;

            // 2열이 타일 가운데(0)에 오도록, 1열(선두)은 앞으로, 3열은 뒤로
            float xOff = (1 - rowIdx) * rowSpacing * dir;

            if (inThisRow == 1)
            {
                positions.Add(new Vector2(xOff, 0));
            }
            else
            {
                float spread = usable * 0.2f * rowIdx;
                for (int i = 0; i < inThisRow; i++)
                {
                    float y = (i - (inThisRow - 1) * 0.5f) * spread * 2f;
                    positions.Add(new Vector2(xOff, y));
                }
            }

            placed += inThisRow;
            rowIdx++;
        }
        return positions;
    }

    private void ClearSoldiers()
    {
        foreach (var s in spawnedSoldiers)
            if (s != null) Destroy(s);
        spawnedSoldiers.Clear();
        soldiers.Clear();
    }

    private Color GetSoldierColor(UnitData data)
    {
        return data.category switch
        {
            UnitCategory.Cavalry => new Color(0.85f, 0.25f, 0.20f),
            UnitCategory.Melee => data.unitName.Contains("민")
                ? new Color(0.12f, 0.60f, 0.35f)
                : new Color(0.13f, 0.45f, 0.68f),
            UnitCategory.Ranged => new Color(0.90f, 0.55f, 0.05f),
            UnitCategory.Special => new Color(0.50f, 0.22f, 0.60f),
            _ => Color.gray
        };
    }

    private Color GetUnitColor(UnitData data)
    {
        return data.category switch
        {
            UnitCategory.Cavalry => new Color(0.91f, 0.30f, 0.24f, 0.8f),
            UnitCategory.Melee => data.unitName.Contains("민")
                ? new Color(0.15f, 0.68f, 0.38f, 0.8f)
                : new Color(0.16f, 0.50f, 0.73f, 0.8f),
            UnitCategory.Ranged => new Color(0.95f, 0.61f, 0.07f, 0.8f),
            UnitCategory.Special => new Color(0.56f, 0.27f, 0.68f, 0.8f),
            _ => baseColor
        };
    }
}
