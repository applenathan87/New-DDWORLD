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
        nameText.fontSize = 5;
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
        countText.text = data.unitData.soldierCount + "명";
        countText.fontSize = 3.5f;
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
        gameObject.SetActive(true);
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
        isDragging = true;
        hoverTween?.Kill();
        transform.localScale = baseScale;

        // 드래그 평면 설정 (카드가 있는 Y 높이)
        dragPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));

        // 마우스와 카드 위치 차이 저장
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (dragPlane.Raycast(ray, out float enter))
        {
            dragOffset = transform.position - ray.GetPoint(enter);
        }
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (dragPlane.Raycast(ray, out float enter))
        {
            transform.position = ray.GetPoint(enter) + dragOffset;
        }

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

    private void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        // 호버 해제
        if (hoveredTile != null)
        {
            hoveredTile.SetHover(false);

            if (hoveredTile.isPlayerZone && Hand3D.Instance != null)
            {
                Hand3D.Instance.PlaceCardOnTile(hoveredTile, this);
                hoveredTile = null;
                if (!gameObject.activeSelf) return;
            }
            hoveredTile = null;
        }

        // 배치 안 됐으면 핸드로 복귀
        transform.DOMove(handPosition, 0.3f).SetEase(Ease.OutCubic);
        transform.DORotateQuaternion(handRotation, 0.3f).SetEase(Ease.OutCubic);
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
