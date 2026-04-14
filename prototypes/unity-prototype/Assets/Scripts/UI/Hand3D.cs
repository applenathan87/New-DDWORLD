using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

/// <summary>
/// 3D 공간에서 카드 핸드를 관리.
/// 카메라 뷰포트 하단에 카드를 아치형으로 배치.
/// </summary>
public class Hand3D : MonoBehaviour
{
    public static Hand3D Instance { get; private set; }

    [Header("카드 크기")]
    public float cardWidth = 0.2f;
    public float cardHeight = 0.27f;

    [Header("핸드 배치")]
    public float cardSpacing = 0.22f;
    public float arcHeight = 0.03f;
    public float arcMaxRotation = 2.5f;

    [Header("핸드 위치 (뷰포트 기준)")]
    public float handDepth = 6f;               // 카메라로부터의 거리

    [Header("폰트")]
    public TMP_FontAsset koreanFont;

    private Camera mainCamera;
    private Transform cardAnchor;       // 내 핸드 앵커 (카메라 하단, 가까움)
    private Transform enemyCardAnchor;  // 상대 핸드 앵커 (카메라 상단, 멀리)
    private Transform drawAnchor;       // 드로우 연출용 중앙 앵커
    private GameObject dimQuad;         // 3D 검은 오버레이 (카드 뒤, 그리드 앞)
    private float playerScaleFactor = 1f;
    private List<Card3D> myCards = new();
    private List<Card3D> enemyCards = new();

    private void Awake()
    {
        Instance = this;
    }

    private bool debugLogged = false;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (!debugLogged && cardAnchor != null && myCards.Count > 0)
        {
            debugLogged = true;
            Debug.Log($"[Hand3D] 앵커 월드위치: {cardAnchor.position}");
            Debug.Log($"[Hand3D] 앵커 로컬위치: {cardAnchor.localPosition}");
            Debug.Log($"[Hand3D] 카드0 월드위치: {myCards[0].transform.position}");
            Debug.Log($"[Hand3D] 카드0 부모: {myCards[0].transform.parent?.name}");
            Debug.Log($"[Hand3D] 카메라 위치: {mainCamera.transform.position}");
        }
    }

    /// <summary>
    /// 카메라 하위에 카드 앵커 생성 (최초 호출 시 1회만)
    /// </summary>
    private void EnsureAnchors()
    {
        if (cardAnchor != null) return;

        mainCamera = Camera.main;
        float halfH = handDepth * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfW = halfH * mainCamera.aspect;

        // --- 내 핸드 앵커 (카메라 하단, 가까움) ---
        float playerDepth = handDepth * 0.55f;
        playerScaleFactor = playerDepth / handDepth;
        float playerHalfH = playerDepth * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);

        var anchorObj = new GameObject("PlayerHandAnchor");
        anchorObj.transform.SetParent(mainCamera.transform);
        anchorObj.transform.localPosition = new Vector3(0, -playerHalfH * 0.7f, playerDepth);
        anchorObj.transform.localRotation = Quaternion.identity;
        cardAnchor = anchorObj.transform;

        // --- 상대 핸드 앵커 (카메라 상단, 멀리) ---
        var enemyObj = new GameObject("EnemyHandAnchor");
        enemyObj.transform.SetParent(mainCamera.transform);
        enemyObj.transform.localPosition = new Vector3(0, halfH * 0.75f, handDepth);
        enemyObj.transform.localRotation = Quaternion.identity;
        enemyCardAnchor = enemyObj.transform;

        // --- 드로우 연출용 중앙 앵커 (화면 정중앙) ---
        var drawObj = new GameObject("DrawAnchor");
        drawObj.transform.SetParent(mainCamera.transform);
        drawObj.transform.localPosition = new Vector3(0, 0, handDepth * 0.7f);
        drawObj.transform.localRotation = Quaternion.identity;
        drawAnchor = drawObj.transform;

        // --- 3D 딤 오버레이 (카드 뒤, 그리드 앞) ---
        dimQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        dimQuad.name = "DimOverlay3D";
        dimQuad.transform.SetParent(mainCamera.transform);
        // drawAnchor보다 살짝 뒤에 배치
        dimQuad.transform.localPosition = new Vector3(0, 0, handDepth * 0.75f);
        dimQuad.transform.localRotation = Quaternion.identity;
        // 화면 전체를 확실히 덮도록 고정 크기
        dimQuad.transform.localScale = new Vector3(50f, 50f, 1f);

        Destroy(dimQuad.GetComponent<Collider>());
        var dimMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        dimMat.color = new Color(0, 0, 0, 0);
        dimMat.SetFloat("_Surface", 1); // Transparent
        dimMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        dimMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        dimMat.SetInt("_ZWrite", 0);
        dimMat.renderQueue = 3100;
        dimQuad.GetComponent<Renderer>().material = dimMat;
        dimQuad.SetActive(false);
    }

    /// <summary>
    /// 3D 딤 오버레이 페이드
    /// </summary>
    public void FadeDim(float targetAlpha, float duration)
    {
        if (dimQuad == null) return;
        if (targetAlpha > 0) dimQuad.SetActive(true);

        var renderer = dimQuad.GetComponent<Renderer>();
        Color c = renderer.material.color;
        DOTween.To(() => c.a, a => {
            c.a = a;
            renderer.material.color = c;
        }, targetAlpha, duration).OnComplete(() => {
            if (targetAlpha <= 0) dimQuad.SetActive(false);
        });
    }

    // === 카드 생성 ===

    /// <summary>
    /// 플레이어 손패 카드 생성
    /// </summary>
    public List<Card3D> CreatePlayerCards(List<CardData> hand)
    {
        EnsureAnchors();
        ClearCards(myCards);
        foreach (var data in hand)
        {
            var card = CreateCard3D(data, isEnemy: false);
            myCards.Add(card);
        }
        return myCards;
    }

    /// <summary>
    /// 상대 손패 카드 생성
    /// </summary>
    public List<Card3D> CreateEnemyCards(List<CardData> hand)
    {
        ClearCards(enemyCards);
        foreach (var data in hand)
        {
            var card = CreateCard3D(data, isEnemy: true);
            card.interactable = false; // 상대 카드는 호버만 가능
            enemyCards.Add(card);
        }
        return enemyCards;
    }

    private Card3D CreateCard3D(CardData data, bool isEnemy = false)
    {
        var obj = new GameObject(data.unitData.unitName);
        // 드로우 연출 시 drawAnchor에 배치되므로 일단 drawAnchor에 넣음
        obj.transform.SetParent(drawAnchor);
        obj.transform.localRotation = Quaternion.identity;

        // 모든 카드 동일 크기로 생성 (핸드 이동 시 스케일 조절)
        var card = obj.AddComponent<Card3D>();
        card.Setup(data, cardWidth, cardHeight);
        if (koreanFont != null) card.SetFont(koreanFont);

        return card;
    }

    private void ClearCards(List<Card3D> cards)
    {
        foreach (var c in cards)
            if (c != null) Destroy(c.gameObject);
        cards.Clear();
    }

    // === 핸드 레이아웃 ===

    /// <summary>
    /// 활성 카드만 모아서 아치형 배치 적용
    /// </summary>
    public void LayoutPlayerHand(bool animated = false)
    {
        var activeCards = GetActiveCards(myCards);
        LayoutCards(activeCards, false, animated);
    }

    public void LayoutEnemyHand()
    {
        LayoutCards(enemyCards, true, false);
    }

    private List<Card3D> GetActiveCards(List<Card3D> cards)
    {
        var active = new List<Card3D>();
        foreach (var c in cards)
            if (c != null && c.gameObject.activeSelf) active.Add(c);
        return active;
    }

    /// <summary>
    /// 카드를 앵커 기준 로컬 좌표로 배치 (카메라에 고정)
    /// </summary>
    private void LayoutCards(List<Card3D> cards, bool isEnemy, bool animated)
    {
        int n = cards.Count;
        if (n == 0) return;

        Transform anchor = isEnemy ? enemyCardAnchor : cardAnchor;

        float spacing = isEnemy ? cardSpacing * 0.85f : cardSpacing * playerScaleFactor;
        float totalWidth = (n - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < n; i++)
        {
            float xOffset = startX + i * spacing;

            // 아치 (플레이어만)
            float yOffset = 0;
            float zRotation = 0;
            if (!isEnemy && n > 1)
            {
                float t = (i - (n - 1) / 2f) / ((n - 1) / 2f);
                yOffset = arcHeight * (1f - t * t);
                zRotation = -arcMaxRotation * t;
            }

            // 로컬 좌표 (앵커 기준)
            Vector3 localPos = new Vector3(xOffset, yOffset, 0);
            Quaternion localRot = Quaternion.Euler(0, 0, zRotation);

            // 월드 좌표로 변환하여 저장
            Vector3 worldPos = anchor.TransformPoint(localPos);
            Quaternion worldRot = anchor.rotation * localRot;

            cards[i].SetHandTransform(worldPos, worldRot, i);

            // 부모를 앵커로 설정
            cards[i].transform.SetParent(anchor);

            // 내 핸드 카드는 가까이 있으므로 스케일 축소
            Vector3 targetScale = isEnemy
                ? new Vector3(cardWidth, cardHeight, 1f)
                : new Vector3(cardWidth * playerScaleFactor, cardHeight * playerScaleFactor, 1f);

            cards[i].SetBaseScale(targetScale);

            if (animated)
            {
                cards[i].transform.DOLocalMove(localPos, 0.35f).SetEase(Ease.OutCubic);
                cards[i].transform.DOLocalRotateQuaternion(localRot, 0.35f).SetEase(Ease.OutCubic);
                cards[i].transform.DOScale(targetScale, 0.35f).SetEase(Ease.OutCubic);
            }
            else
            {
                cards[i].transform.localPosition = localPos;
                cards[i].transform.localRotation = localRot;
                cards[i].transform.localScale = targetScale;
            }
        }
    }

    // === 배치 ===

    /// <summary>
    /// 카드를 3D 타일에 배치
    /// </summary>
    public void PlaceCardOnTile(BattleTile tile, Card3D card)
    {
        if (card == null || tile == null) return;

        // 이미 유닛이 있으면 스왑
        if (tile.IsOccupied)
        {
            Card3D oldCard = ReturnCardFromTile(tile);
            if (oldCard != null)
            {
                oldCard.SetReturned();
                LayoutPlayerHand(animated: true);
            }
        }

        // 필드 가득 찼는지 확인
        if (GameManager.Instance.PlayerPlacements.Count >= 5)
        {
            PlacementUI.Instance?.ShowFloatingMessage("필드가 가득 찼습니다!");
            return;
        }

        bool success = GameManager.Instance.PlaceCard(card.cardData, tile.row, tile.col);
        if (!success) return;

        tile.PlaceUnit3D(card);
        card.SetPlaced();

        PlacementUI.Instance?.UpdateInfoText();
        LayoutPlayerHand(animated: true);
    }

    /// <summary>
    /// 타일에서 카드 제거하여 핸드로 복귀
    /// </summary>
    public Card3D ReturnCardFromTile(BattleTile tile)
    {
        if (!tile.IsOccupied) return null;

        Card3D card = tile.RemoveUnit3D();
        GameManager.Instance.RemovePlacement(tile.row, tile.col);

        return card;
    }

    /// <summary>
    /// 타일 클릭으로 카드 제거 + 핸드 복귀 애니메이션
    /// </summary>
    public void RemoveAndReturnToHand(BattleTile tile)
    {
        Card3D card = ReturnCardFromTile(tile);
        if (card == null) return;

        card.SetReturned();
        card.transform.SetParent(cardAnchor);

        // 핸드 레이아웃 재계산 (로컬 좌표, 애니메이션)
        LayoutPlayerHand(animated: true);

        PlacementUI.Instance?.UpdateInfoText();
    }

    // === 드로우 애니메이션 ===

    /// <summary>
    /// 드로우 페이즈 전체 연출
    /// </summary>
    public IEnumerator AnimateDrawSequence()
    {
        int totalPlayer = myCards.Count;
        int totalEnemy = enemyCards.Count;
        float ps = playerScaleFactor;

        // 모든 카드 동일 크기
        Vector3 drawCardScale = new Vector3(cardWidth, cardHeight, 1f);
        float drawSpacing = cardSpacing * 0.85f;

        // 2행 배치: 상대(위), 나(아래) — drawAnchor 로컬 좌표
        float rowGap = cardHeight * 1.2f; // 두 줄 사이 간격

        // 상대 카드 위치 (윗줄)
        float eW = (totalEnemy - 1) * drawSpacing;
        float eStartX = -eW / 2f;
        Vector3[] enemyCenterLocal = new Vector3[totalEnemy];
        for (int i = 0; i < totalEnemy; i++)
            enemyCenterLocal[i] = new Vector3(eStartX + i * drawSpacing, rowGap * 0.5f, 0);

        // 내 카드 위치 (아랫줄)
        float pW = (totalPlayer - 1) * drawSpacing;
        float pStartX = -pW / 2f;
        Vector3[] playerCenterLocal = new Vector3[totalPlayer];
        for (int i = 0; i < totalPlayer; i++)
            playerCenterLocal[i] = new Vector3(pStartX + i * drawSpacing, -rowGap * 0.5f, 0);

        // 덱 출발 위치 (좌하단 / 우상단)
        float drawHalfH = drawAnchor.localPosition.z * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float drawHalfW = drawHalfH * mainCamera.aspect;
        Vector3 playerDeckPos = new Vector3(-drawHalfW * 0.8f, -drawHalfH * 0.6f, 0);
        Vector3 enemyDeckPos = new Vector3(drawHalfW * 0.8f, drawHalfH * 0.6f, 0);

        // 3D 딤 오버레이 페이드인
        FadeDim(0.8f, 0.4f);

        // 모든 카드를 drawAnchor에서 덱 위치에 숨김
        for (int i = 0; i < totalPlayer; i++)
        {
            myCards[i].transform.SetParent(drawAnchor);
            myCards[i].transform.localPosition = playerDeckPos;
            myCards[i].transform.localRotation = Quaternion.identity;
            myCards[i].transform.localScale = Vector3.zero;
        }
        for (int i = 0; i < totalEnemy; i++)
        {
            enemyCards[i].transform.SetParent(drawAnchor);
            enemyCards[i].transform.localPosition = enemyDeckPos;
            enemyCards[i].transform.localRotation = Quaternion.identity;
            enemyCards[i].transform.localScale = Vector3.zero;
        }

        // === 1단계: 교대로 각자의 줄로 드로우 ===
        int pi = 0, ei = 0;
        bool playerTurn = true;

        while (pi < totalPlayer || ei < totalEnemy)
        {
            if (playerTurn && pi < totalPlayer)
            {
                myCards[pi].transform.DOLocalMove(playerCenterLocal[pi], 0.3f).SetEase(Ease.OutCubic);
                myCards[pi].transform.DOScale(drawCardScale, 0.3f).SetEase(Ease.OutBack);
                pi++;
            }
            else if (!playerTurn && ei < totalEnemy)
            {
                enemyCards[ei].transform.DOLocalMove(enemyCenterLocal[ei], 0.3f).SetEase(Ease.OutCubic);
                enemyCards[ei].transform.DOScale(drawCardScale, 0.3f).SetEase(Ease.OutBack);
                ei++;
            }
            playerTurn = !playerTurn;
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(0.8f);

        // === 2단계: 병종별 정렬 ===
        List<Card3D> sortedPlayer = new(myCards);
        sortedPlayer.Sort((a, b) =>
            GetSortOrder(a.cardData.unitData).CompareTo(GetSortOrder(b.cardData.unitData)));

        List<Card3D> sortedEnemy = new(enemyCards);
        sortedEnemy.Sort((a, b) =>
            GetSortOrder(a.cardData.unitData).CompareTo(GetSortOrder(b.cardData.unitData)));

        PlacementUI.Instance?.ShowPhaseTitle("배치 페이즈");
        yield return new WaitForSeconds(0.5f);

        // === 3단계: 정렬 순서대로 핸드로 이동 (로컬 좌표) ===
        int nPlayer = sortedPlayer.Count;
        int nEnemy = sortedEnemy.Count;
        float playerSpacing = cardSpacing * ps;
        float handTotalW = (nPlayer - 1) * playerSpacing;
        float handStartX = -handTotalW / 2f;
        float enemySpacing = cardSpacing * 0.85f;
        float enemyTotalW = (nEnemy - 1) * enemySpacing;
        float enemyStartX = -enemyTotalW / 2f;
        Vector3 playerCardScale = new Vector3(cardWidth * ps, cardHeight * ps, 1f);
        Vector3 enemyCardScale = new Vector3(cardWidth, cardHeight, 1f);

        int maxCards = Mathf.Max(nPlayer, nEnemy);
        for (int i = 0; i < maxCards; i++)
        {
            if (i < nPlayer)
            {
                var card = sortedPlayer[i];
                card.transform.SetParent(cardAnchor);

                float t = nPlayer > 1 ? (i - (nPlayer - 1) / 2f) / ((nPlayer - 1) / 2f) : 0;
                Vector3 localPos = new Vector3(handStartX + i * playerSpacing, arcHeight * ps * (1f - t * t), 0);
                Quaternion localRot = Quaternion.Euler(0, 0, -arcMaxRotation * t);

                card.SetHandTransform(cardAnchor.TransformPoint(localPos), cardAnchor.rotation * localRot, i);
                card.SetBaseScale(playerCardScale);
                card.transform.DOLocalMove(localPos, 0.3f).SetEase(Ease.OutCubic);
                card.transform.DOScale(playerCardScale, 0.3f).SetEase(Ease.OutBack);
                card.transform.DOLocalRotateQuaternion(localRot, 0.3f).SetEase(Ease.OutCubic);
            }

            if (i < nEnemy)
            {
                var card = sortedEnemy[i];
                card.transform.SetParent(enemyCardAnchor);

                Vector3 localPos = new Vector3(enemyStartX + i * enemySpacing, 0, 0);
                Quaternion localRot = Quaternion.identity;

                card.SetHandTransform(enemyCardAnchor.TransformPoint(localPos), enemyCardAnchor.rotation, i);
                card.transform.DOLocalMove(localPos, 0.3f).SetEase(Ease.OutCubic);
                card.transform.DOScale(enemyCardScale, 0.3f).SetEase(Ease.OutBack);
            }

            yield return new WaitForSeconds(0.12f);
        }

        yield return new WaitForSeconds(0.35f);

        // 3D 딤 오버레이 페이드아웃
        FadeDim(0f, 0.6f);

        // 정렬된 순서로 리스트 갱신
        myCards = sortedPlayer;
        enemyCards = sortedEnemy;
    }

    public List<Card3D> GetMyCards() => myCards;
    public List<Card3D> GetEnemyCards() => enemyCards;

    private int GetSortOrder(UnitData data)
    {
        return data.category switch
        {
            UnitCategory.Cavalry => 0,
            UnitCategory.Melee => data.unitName.Contains("민") ? 1 : 2,
            UnitCategory.Ranged => 3,
            UnitCategory.Special => 4,
            _ => 5
        };
    }
}
