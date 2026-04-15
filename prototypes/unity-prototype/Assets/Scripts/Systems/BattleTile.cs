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

    public bool IsOccupied => placedCard != null || placedCard3D != null;
    public CardUI PlacedCard => placedCard;

    public TMP_FontAsset koreanFont;

    public void Setup(int col, int row, bool isPlayerZone, Color color, Renderer renderer)
    {
        this.col = col;
        this.row = row;
        this.isPlayerZone = isPlayerZone;
        this.tileRenderer = renderer;
        this.baseColor = color;
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

        // 그리드 배치: cols x rows 계산
        int cols, rows;
        if (count <= 1) { cols = 1; rows = 1; }
        else if (count <= 4) { cols = 2; rows = 2; }
        else if (count <= 6) { cols = 3; rows = 2; }
        else if (count <= 9) { cols = 3; rows = 3; }
        else if (count <= 12) { cols = 4; rows = 3; }
        else if (count <= 16) { cols = 4; rows = 4; }
        else { cols = 5; rows = 4; }

        Color soldierColor = GetSoldierColor(data);

        // 머티리얼 생성
        var shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        var mat = new Material(shader);
        mat.color = soldierColor;
        var outlineMat = new Material(shader);
        outlineMat.color = Color.black;

        float spacingX = count > 1 ? usable / cols : 0;
        float spacingZ = count > 1 ? usable / rows : 0;

        int spawned = 0;
        for (int r = 0; r < rows && spawned < count; r++)
        {
            for (int c = 0; c < cols && spawned < count; c++)
            {
                float xOff = (c - (cols - 1) * 0.5f) * spacingX;
                float zOff = (r - (rows - 1) * 0.5f) * spacingZ;

                // 타일은 Euler(90,0,0) 회전 → 로컬 좌표계:
                //   local X = world X (좌우)
                //   local Y = world Z (앞뒤)
                //   local -Z = world Y (위)
                float heightOffset = -SOLDIER_HEIGHT / 2f; // 바닥에 서있도록

                // 아웃라인 (검은 캡슐, 살짝 크게)
                var outline = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                outline.name = $"Outline_{spawned}";
                outline.transform.SetParent(transform);
                outline.transform.localPosition = new Vector3(xOff, zOff, heightOffset);
                outline.transform.localScale = new Vector3(
                    SOLDIER_RADIUS * 2f * OUTLINE_THICKNESS,
                    SOLDIER_HEIGHT * 0.5f * OUTLINE_THICKNESS,
                    SOLDIER_RADIUS * 2f * OUTLINE_THICKNESS);
                outline.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                outline.GetComponent<Renderer>().material = outlineMat;
                outline.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                Destroy(outline.GetComponent<Collider>());
                spawnedSoldiers.Add(outline);

                // 캡슐 본체
                var soldier = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                soldier.name = $"Soldier_{spawned}";
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

                spawned++;
            }
        }
    }

    private void ClearSoldiers()
    {
        foreach (var s in spawnedSoldiers)
            if (s != null) Destroy(s);
        spawnedSoldiers.Clear();
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
