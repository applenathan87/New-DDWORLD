using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 5x5 배치 격자의 한 칸.
/// - 핸드에서 카드를 드롭하면 병사로 배치
/// - 배치된 병사를 드래그하면 다른 칸으로 이동
/// - 빈 칸 클릭 시 아무 동작 없음, 병사 칸 클릭 시 핸드로 회수
/// </summary>
public class GridCellUI : MonoBehaviour,
    IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int row;
    public int col;
    public TMP_FontAsset koreanFont;

    private Image background;
    private TextMeshProUGUI unitText;
    private CardUI placedCard;

    // 드래그 중 마우스를 따라다니는 병사 고스트
    private static GameObject dragGhost;
    private static GridCellUI dragSourceCell;

    private static readonly Color COLOR_EMPTY = new Color(0.15f, 0.15f, 0.25f, 0.8f);
    private static readonly Color COLOR_HOVER = new Color(0.3f, 0.3f, 0.5f, 0.9f);
    private static readonly Color COLOR_DRAG_SOURCE = new Color(0.15f, 0.15f, 0.25f, 0.5f);

    public void Setup(int r, int c)
    {
        row = r;
        col = c;

        background = GetComponent<Image>();
        if (background == null) background = gameObject.AddComponent<Image>();
        background.color = COLOR_EMPTY;

        // 배치된 유닛 표시용 텍스트
        var textObj = new GameObject("UnitText");
        textObj.transform.SetParent(transform, false);
        unitText = textObj.AddComponent<TextMeshProUGUI>();
        unitText.text = "";
        unitText.fontSize = 12;
        unitText.alignment = TextAlignmentOptions.Center;
        unitText.color = Color.white;
        unitText.raycastTarget = false;
        if (koreanFont != null) unitText.font = koreanFont;
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // 좌표 표시
        var coordObj = new GameObject("Coord");
        coordObj.transform.SetParent(transform, false);
        var coordText = coordObj.AddComponent<TextMeshProUGUI>();
        string rowLabel = ((char)('A' + r)).ToString();
        coordText.text = rowLabel + (col + 1);
        coordText.fontSize = 8;
        coordText.alignment = TextAlignmentOptions.TopLeft;
        coordText.color = new Color(1, 1, 1, 0.3f);
        coordText.raycastTarget = false;
        if (koreanFont != null) coordText.font = koreanFont;
        var coordRect = coordObj.GetComponent<RectTransform>();
        coordRect.anchorMin = Vector2.zero;
        coordRect.anchorMax = Vector2.one;
        coordRect.offsetMin = new Vector2(2, 0);
        coordRect.offsetMax = new Vector2(0, -2);
    }

    // ===========================================
    // 드롭: 핸드 카드 또는 다른 칸의 병사를 받는다
    // ===========================================

    public void OnDrop(PointerEventData eventData)
    {
        // 1) 필드 → 필드 (다른 칸에서 병사가 드래그되어 옴)
        if (dragSourceCell != null && dragSourceCell != this)
        {
            if (IsOccupied)
            {
                // 양쪽 다 유닛이 있으면 서로 교환
                // PlacementUI.Instance?.SwapFieldUnits(dragSourceCell, this);
            }
            else
            {
                // 빈 칸으로 이동
                // PlacementUI.Instance?.MoveUnit(dragSourceCell, this);
            }
            return;
        }

        // 2) 핸드 → 필드 (카드 드래그)
        var cardUI = eventData.pointerDrag?.GetComponent<CardUI>();
        if (cardUI == null) return;

        // 2a) 점유된 칸에 드롭 → 스왑
        if (IsOccupied)
        {
            // PlacementUI.Instance?.SwapUnit(this, cardUI);
            return;
        }

        // 2b) 빈 칸에 드롭 → 필드가 가득 찼는지 확인
        if (GameManager.Instance.PlayerPlacements.Count >= 5)
        {
            PlacementUI.Instance?.ShowFloatingMessage("필드가 가득 찼습니다!");
            return;
        }

        // PlacementUI.Instance?.PlaceOnCell(this, cardUI);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsOccupied && eventData.dragging)
            background.color = COLOR_HOVER;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsOccupied)
            background.color = COLOR_EMPTY;
        else if (dragSourceCell != this)
            background.color = GetUnitColor(placedCard.cardData.unitData);
    }

    // ===========================================
    // 클릭: 배치된 병사를 핸드로 회수
    // ===========================================

    public void OnPointerClick(PointerEventData eventData)
    {
        // 드래그 후 마우스를 놓은 것은 클릭으로 처리하지 않음
        if (eventData.dragging) return;
        // if (placedCard != null)
        //     PlacementUI.Instance?.RemovePlacement(this);
    }

    // ===========================================
    // 드래그: 배치된 병사를 다른 칸으로 이동
    // ===========================================

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsOccupied) return;

        dragSourceCell = this;

        // 고스트 생성: 병사 정보를 보여주는 작은 UI
        CreateDragGhost(eventData);

        // 원래 칸을 반투명하게
        background.color = COLOR_DRAG_SOURCE;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragGhost == null) return;

        // 고스트가 마우스를 따라다님
        Canvas rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
        RectTransform ghostRect = dragGhost.GetComponent<RectTransform>();
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            eventData.position,
            rootCanvas.worldCamera,
            out localPoint);
        ghostRect.anchoredPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 고스트 제거
        if (dragGhost != null)
        {
            Object.Destroy(dragGhost);
            dragGhost = null;
        }

        // 원래 칸 색상 복원
        if (dragSourceCell != null && dragSourceCell.IsOccupied)
        {
            dragSourceCell.background.color = GetUnitColor(dragSourceCell.placedCard.cardData.unitData);
        }

        dragSourceCell = null;
    }

    /// <summary>
    /// 드래그 중 마우스를 따라다니는 병사 고스트 UI 생성
    /// </summary>
    private void CreateDragGhost(PointerEventData eventData)
    {
        Canvas rootCanvas = GetComponentInParent<Canvas>().rootCanvas;

        dragGhost = new GameObject("DragGhost");
        dragGhost.transform.SetParent(rootCanvas.transform, false);
        dragGhost.transform.SetAsLastSibling();

        var ghostRect = dragGhost.AddComponent<RectTransform>();
        ghostRect.sizeDelta = new Vector2(48, 48);

        // 배경 (병종 색상, 원형 느낌)
        var ghostBg = dragGhost.AddComponent<Image>();
        ghostBg.color = GetUnitColor(placedCard.cardData.unitData);
        ghostBg.raycastTarget = false;

        // 병사 이름 + 수
        var textObj = new GameObject("GhostText");
        textObj.transform.SetParent(dragGhost.transform, false);
        var text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = placedCard.cardData.unitData.unitName + "\n" + placedCard.cardData.unitData.soldierCount;
        text.fontSize = 10;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;
        if (koreanFont != null) text.font = koreanFont;
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // CanvasGroup으로 레이캐스트 통과 (아래 칸이 드롭을 받을 수 있도록)
        var cg = dragGhost.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.alpha = 0.85f;
    }

    // ===========================================
    // 배치/제거
    // ===========================================

    /// <summary>
    /// 카드를 병사로 배치 (카드 → 병사 변환)
    /// </summary>
    public void PlaceUnit(CardUI card)
    {
        placedCard = card;
        unitText.text = card.cardData.unitData.unitName + "\n" + card.cardData.unitData.soldierCount + "명";
        background.color = GetUnitColor(card.cardData.unitData);
    }

    /// <summary>
    /// 배치된 병사를 제거하고 카드로 돌려줌
    /// </summary>
    public CardUI RemoveUnit()
    {
        var card = placedCard;
        placedCard = null;
        unitText.text = "";
        background.color = COLOR_EMPTY;
        return card;
    }

    public bool IsOccupied => placedCard != null;

    /// <summary>
    /// 현재 드래그 중인 출발 칸인지 확인 (외부에서 사용)
    /// </summary>
    public static bool IsDraggingFromCell => dragSourceCell != null;

    private Color GetUnitColor(UnitData data)
    {
        return data.category switch
        {
            UnitCategory.Cavalry => new Color(0.91f, 0.30f, 0.24f, 0.7f),
            UnitCategory.Melee => data.unitName.Contains("민")
                ? new Color(0.15f, 0.68f, 0.38f, 0.7f)
                : new Color(0.16f, 0.50f, 0.73f, 0.7f),
            UnitCategory.Ranged => new Color(0.95f, 0.61f, 0.07f, 0.7f),
            UnitCategory.Special => new Color(0.56f, 0.27f, 0.68f, 0.7f),
            _ => COLOR_EMPTY
        };
    }
}
