using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;

/// <summary>
/// 3D 공간에 존재하는 카드. Quad 메시 기반.
/// 핸드에 있을 때는 카드 모습, 필드에 배치되면 비활성화.
/// </summary>
public class Card3D : MonoBehaviour
{
    public CardData cardData;
    public bool interactable = true; // false면 호버만 가능, 드래그/클릭 불가

    private MeshRenderer meshRenderer;
    private Material material;
    private TextMeshPro nameText;
    private TextMeshPro countText;
    private BoxCollider boxCollider;

    // 드래그
    private bool isDragging;
    private Camera mainCamera;
    private Plane dragPlane;
    private Vector3 dragOffset;
    private BattleTile hoveredTile;

    // 핸드 위치 (복귀용)
    private Vector3 handPosition;
    private Quaternion handRotation;
    private int handIndex;

    // 호버
    private Tween hoverTween;
    private Vector3 baseScale;

    // 병종별 색상
    private static readonly Color COLOR_CAVALRY  = new Color(0.91f, 0.30f, 0.24f);
    private static readonly Color COLOR_MILITIA   = new Color(0.15f, 0.68f, 0.38f);
    private static readonly Color COLOR_SPEARMAN  = new Color(0.16f, 0.50f, 0.73f);
    private static readonly Color COLOR_ARCHER    = new Color(0.95f, 0.61f, 0.07f);
    private static readonly Color COLOR_TRAP      = new Color(0.56f, 0.27f, 0.68f);
    private static readonly Color COLOR_DEFAULT   = new Color(0.4f, 0.4f, 0.4f);

    public void Setup(CardData data, float cardWidth, float cardHeight)
    {
        cardData = data;
        mainCamera = Camera.main;
        baseScale = new Vector3(cardWidth, cardHeight, 1f);
        transform.localScale = baseScale;

        // Unity 기본 Quad 메시 사용
        var meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");

        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        // URP Unlit, 없으면 fallback
        var shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        material = new Material(shader);
        material.color = GetUnitColor(data.unitData);
        meshRenderer.material = material;

        // 콜라이더
        boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.size = new Vector3(1, 1, 0.05f);

        // 병종 이름
        var nameObj = new GameObject("Name");
        nameObj.transform.SetParent(transform);
        nameObj.transform.localPosition = new Vector3(0, 0.15f, -0.01f);
        nameObj.transform.localRotation = Quaternion.identity;
        nameObj.transform.localScale = new Vector3(1f / cardWidth, 1f / cardHeight, 1f);

        nameText = nameObj.AddComponent<TextMeshPro>();
        nameText.text = data.unitData.unitName;
        nameText.fontSize = 3;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = Color.white;
        nameText.rectTransform.sizeDelta = new Vector2(cardWidth * 1.5f, cardHeight * 0.5f);
        nameText.raycastTarget = false;

        // 병사 수
        var countObj = new GameObject("Count");
        countObj.transform.SetParent(transform);
        countObj.transform.localPosition = new Vector3(0, -0.2f, -0.01f);
        countObj.transform.localRotation = Quaternion.identity;
        countObj.transform.localScale = new Vector3(1f / cardWidth, 1f / cardHeight, 1f);

        countText = countObj.AddComponent<TextMeshPro>();
        string unit = data.unitData.category == UnitCategory.Special ? "개" : "명";
        countText.text = data.unitData.soldierCount + unit;
        countText.fontSize = 2f;
        countText.alignment = TextAlignmentOptions.Center;
        countText.color = new Color(1, 1, 1, 0.8f);
        countText.rectTransform.sizeDelta = new Vector2(cardWidth * 1.5f, cardHeight * 0.3f);
        countText.raycastTarget = false;
    }

    public void SetFont(TMP_FontAsset font)
    {
        if (font == null) return;
        if (nameText != null) nameText.font = font;
        if (countText != null) countText.font = font;
    }

    // === 핸드 위치 ===

    public void SetHandTransform(Vector3 position, Quaternion rotation, int index)
    {
        handPosition = position;
        handRotation = rotation;
        handIndex = index;
    }

    /// <summary>
    /// 핸드 스케일 갱신 (축소된 상태를 기준으로 호버 동작)
    /// </summary>
    public void SetBaseScale(Vector3 scale)
    {
        baseScale = scale;
    }

    public void SnapToHand()
    {
        transform.position = handPosition;
        transform.rotation = handRotation;
        transform.localScale = baseScale;
    }

    // === 상태 ===

    public void SetPlaced()
    {
        gameObject.SetActive(false);
    }

    public void SetReturned()
    {
        isDragging = false;
        hoverTween?.Kill();
        transform.localScale = baseScale;
        if (boxCollider != null) boxCollider.enabled = true;
        gameObject.SetActive(true);
    }

    // 픽업 원본 타일 (타일→타일 이동 시 드롭 실패하면 복귀용)
    private BattleTile sourceTile;

    /// <summary>
    /// 타일에서 픽업하여 드래그 시작 (Hand3D.PickUpFromTile에서 호출)
    /// </summary>
    public void StartDragFromTile(BattleTile fromTile)
    {
        BeginDrag(fromTile);
    }

    // === 마우스 상호작용 ===

    private void OnMouseEnter()
    {
        if (isDragging) return;
        hoverTween?.Kill();
        hoverTween = transform.DOScale(baseScale * 1.1f, 0.15f).SetEase(Ease.OutCubic);
    }

    private void OnMouseExit()
    {
        if (isDragging) return;
        hoverTween?.Kill();
        hoverTween = transform.DOScale(baseScale, 0.15f).SetEase(Ease.OutCubic);
    }

    private void OnMouseDown()
    {
        if (!interactable) return;
        BeginDrag(null);
    }

    private void BeginDrag(BattleTile fromTile)
    {
        sourceTile = fromTile;
        isDragging = true;
        hoverTween?.Kill();
        transform.localScale = baseScale;

        if (boxCollider != null) boxCollider.enabled = false;

        dragPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (dragPlane.Raycast(ray, out float enter))
        {
            dragOffset = transform.position - ray.GetPoint(enter);
        }
    }

    private void Update()
    {
        if (!isDragging) return;

        // 드래그 중 위치 갱신
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (dragPlane.Raycast(ray, out float enter))
        {
            transform.position = ray.GetPoint(enter) + dragOffset;
        }

        // 타일 호버 감지
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

        // 마우스 버튼 놓으면 드롭
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            EndDrag();
        }
    }

    private void EndDrag()
    {
        isDragging = false;

        if (boxCollider != null) boxCollider.enabled = true;

        if (hoveredTile != null)
        {
            hoveredTile.SetHover(false);

            if (hoveredTile.isPlayerZone && Hand3D.Instance != null)
            {
                if (sourceTile != null && hoveredTile != sourceTile)
                {
                    Hand3D.Instance.MoveCardBetweenTiles(sourceTile, hoveredTile, this);
                    hoveredTile = null;
                    sourceTile = null;
                    if (!gameObject.activeSelf) return;
                }
                else if (sourceTile == null)
                {
                    Hand3D.Instance.PlaceCardOnTile(hoveredTile, this);
                    hoveredTile = null;
                    if (!gameObject.activeSelf) return;
                }
            }
            hoveredTile = null;
        }

        if (sourceTile != null)
        {
            Hand3D.Instance?.ReturnCardToTile(sourceTile, this);
            sourceTile = null;
        }
        else
        {
            transform.DOMove(handPosition, 0.3f).SetEase(Ease.OutCubic);
            transform.DORotateQuaternion(handRotation, 0.3f).SetEase(Ease.OutCubic);
        }
    }

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
