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

        // 유닛 이름 표시용 3D 텍스트
        var labelObj = new GameObject("UnitLabel");
        labelObj.transform.SetParent(transform);
        labelObj.transform.localPosition = new Vector3(0, 0.01f, 0);
        labelObj.transform.rotation = Quaternion.Euler(90, 0, 0);

        unitLabel = labelObj.AddComponent<TextMeshPro>();
        unitLabel.text = "";
        unitLabel.fontSize = 3;
        unitLabel.alignment = TextAlignmentOptions.Center;
        unitLabel.color = Color.white;
        unitLabel.rectTransform.sizeDelta = new Vector2(1f, 0.6f);
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
        // UI 위를 클릭한 경우 무시
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (IsOccupied && Hand3D.Instance != null)
        {
            Hand3D.Instance.RemoveAndReturnToHand(this);
        }
    }

    // === Card3D 지원 ===

    public void PlaceUnit3D(Card3D card)
    {
        placedCard3D = card;
        unitLabel.text = card.cardData.unitData.unitName + "\n" + card.cardData.unitData.soldierCount;

        Color unitColor = GetUnitColor(card.cardData.unitData);
        baseColor = unitColor;
        hoverColor = unitColor + new Color(0.15f, 0.15f, 0.15f, 0);
        tileRenderer.material.color = unitColor;
    }

    public Card3D RemoveUnit3D()
    {
        var card = placedCard3D;
        placedCard3D = null;
        unitLabel.text = "";

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

    public void SetHover(bool hover)
    {
        if (tileRenderer != null)
            tileRenderer.material.color = hover ? hoverColor : baseColor;
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
