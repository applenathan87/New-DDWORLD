using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

/// <summary>
/// 손패에 표시되는 카드 한 장의 UI.
/// 드래그하여 격자에 배치.
/// </summary>
public class CardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    public CardData cardData;
    public TMP_FontAsset koreanFont;

    private Image background;
    private Image border;
    private TextMeshProUGUI nameText;
    private TextMeshProUGUI countText;

    private Transform originalParent;
    private int originalSiblingIndex;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    // 병종별 색상
    private static readonly Color COLOR_CAVALRY  = new Color(0.91f, 0.30f, 0.24f);
    private static readonly Color COLOR_MILITIA   = new Color(0.15f, 0.68f, 0.38f);
    private static readonly Color COLOR_SPEARMAN  = new Color(0.16f, 0.50f, 0.73f);
    private static readonly Color COLOR_ARCHER    = new Color(0.95f, 0.61f, 0.07f);
    private static readonly Color COLOR_TRAP      = new Color(0.56f, 0.27f, 0.68f);
    private static readonly Color COLOR_DEFAULT   = new Color(0.4f, 0.4f, 0.4f);

    public void Setup(CardData data)
    {
        cardData = data;

        rectTransform = GetComponent<RectTransform>();

        // CanvasGroup (드래그 중 레이캐스트 통과용)
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // 배경
        background = GetComponent<Image>();
        if (background == null) background = gameObject.AddComponent<Image>();
        background.color = GetUnitColor(data.unitData);

        // 테두리
        var borderObj = new GameObject("Border");
        borderObj.transform.SetParent(transform, false);
        border = borderObj.AddComponent<Image>();
        border.color = Color.clear;
        var borderRect = borderObj.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-4, -4);
        borderRect.offsetMax = new Vector2(4, 4);
        border.raycastTarget = false;

        // 병종 이름
        var nameObj = new GameObject("Name");
        nameObj.transform.SetParent(transform, false);
        nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = data.unitData.unitName;
        nameText.fontSize = 22;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = Color.white;
        nameText.raycastTarget = false;
        if (koreanFont != null) nameText.font = koreanFont;
        var nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.5f);
        nameRect.anchorMax = new Vector2(1, 1f);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;

        // 병사 수
        var countObj = new GameObject("Count");
        countObj.transform.SetParent(transform, false);
        countText = countObj.AddComponent<TextMeshProUGUI>();
        countText.text = data.unitData.soldierCount + "명";
        countText.fontSize = 16;
        countText.alignment = TextAlignmentOptions.Center;
        countText.color = new Color(1, 1, 1, 0.8f);
        countText.raycastTarget = false;
        if (koreanFont != null) countText.font = koreanFont;
        var countRect = countObj.GetComponent<RectTransform>();
        countRect.anchorMin = new Vector2(0, 0);
        countRect.anchorMax = new Vector2(1, 0.5f);
        countRect.offsetMin = Vector2.zero;
        countRect.offsetMax = Vector2.zero;
    }

    // === 드래그 ===

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"드래그 시작: {cardData.unitData.unitName}");
        isDragging = true;
        hoverTween?.Kill();
        transform.localScale = Vector3.one;

        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();

        // 회전 초기화 (아치형 배치에서 기울어진 상태 해제)
        transform.localRotation = Quaternion.identity;

        // 캔버스 최상위로 이동 (다른 UI 위에 표시)
        Canvas rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
        transform.SetParent(rootCanvas.transform);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f;

        border.color = Color.white;
    }

    private BattleTile hoveredTile; // 드래그 중 호버된 3D 타일

    public void OnDrag(PointerEventData eventData)
    {
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        float scale = rootCanvas != null ? rootCanvas.scaleFactor : 1f;
        rectTransform.anchoredPosition += eventData.delta / scale;

        // 3D 타일 호버 감지
        if (BattleField.Instance != null)
        {
            var tile = BattleField.Instance.GetTileUnderMouse();
            if (tile != hoveredTile)
            {
                if (hoveredTile != null) hoveredTile.SetHover(false);
                hoveredTile = tile;
                if (hoveredTile != null && hoveredTile.isPlayerZone)
                    hoveredTile.SetHover(true);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        border.color = Color.clear;

        // 호버 해제
        if (hoveredTile != null)
        {
            hoveredTile.SetHover(false);

            hoveredTile = null;
        }

        // 배치되지 않았으면 핸드로 복귀
        if (gameObject.activeSelf && transform.parent != originalParent)
        {
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalSiblingIndex);
        }
    }

    /// <summary>
    /// 카드를 손패에서 제거 (배치됨)
    /// </summary>
    public void SetPlaced()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 카드를 손패로 복귀 (배치 취소)
    /// </summary>
    public void SetReturned()
    {
        // 시각 상태 초기화
        hoverTween?.Kill();
        transform.localScale = Vector3.one;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        border.color = Color.clear;
        isDragging = false;
        hoveredTile = null;

        gameObject.SetActive(true);

        // 레이아웃 강제 갱신하여 즉시 올바른 위치에 표시
        if (transform.parent != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent as RectTransform);
    }

    /// <summary>
    /// originalParent를 핸드로 갱신 (복귀 후 다시 드래그 가능하도록)
    /// </summary>
    public void SetHandParent(Transform handParent)
    {
        originalParent = handParent;
    }

    // === 호버 ===

    private Tween hoverTween;
    private bool isDragging = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDragging) return;
        hoverTween?.Kill();
        hoverTween = transform.DOScale(1.08f, 0.15f).SetEase(Ease.OutCubic);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDragging) return;
        hoverTween?.Kill();
        hoverTween = transform.DOScale(1f, 0.15f).SetEase(Ease.OutCubic);
    }

    /// <summary>
    /// CanvasGroup 접근용
    /// </summary>
    public CanvasGroup GetCanvasGroup() => canvasGroup;

    private Color GetUnitColor(UnitData data)
    {
        return data.category switch
        {
            UnitCategory.Cavalry => COLOR_CAVALRY,
            UnitCategory.Melee => data.unitName.Contains("민") ? COLOR_MILITIA : COLOR_SPEARMAN,
            UnitCategory.Ranged => COLOR_ARCHER,
            UnitCategory.Special => COLOR_TRAP,
            _ => COLOR_DEFAULT
        };
    }
}
