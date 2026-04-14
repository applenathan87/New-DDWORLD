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
    public float handDepth = 1.5f;             // 카메라로부터의 거리

    [Header("폰트")]
    public TMP_FontAsset koreanFont;

    private Camera mainCamera;
    private Transform cardAnchor;      // 카메라 자식, 카드의 부모
    private Transform enemyCardAnchor;
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
        Debug.Log($"[Hand3D] 앵커 생성 - 카메라 위치: {mainCamera.transform.position}, 회전: {mainCamera.transform.eulerAngles}");
        Debug.Log($"[Hand3D] 앵커 월드 위치 확인은 LateUpdate에서 출력");

        // 내 핸드 앵커 (카메라 하단)
        var anchorObj = new GameObject("PlayerHandAnchor");
        anchorObj.transform.SetParent(mainCamera.transform);
        anchorObj.transform.localPosition = new Vector3(0, -0.4f, handDepth);
        anchorObj.transform.localRotation = Quaternion.identity;
        cardAnchor = anchorObj.transform;

        // 상대 핸드 앵커 (카메라 상단)
        var enemyObj = new GameObject("EnemyHandAnchor");
        enemyObj.transform.SetParent(mainCamera.transform);
        enemyObj.transform.localPosition = new Vector3(0, 0.35f, handDepth);
        enemyObj.transform.localRotation = Quaternion.identity;
        enemyCardAnchor = enemyObj.transform;
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
            enemyCards.Add(card);
        }
        return enemyCards;
    }

    private Card3D CreateCard3D(CardData data, bool isEnemy = false)
    {
        var obj = new GameObject(data.unitData.unitName);
        // 카메라 앵커 하위에 생성 (카메라를 따라다님)
        obj.transform.SetParent(isEnemy ? enemyCardAnchor : cardAnchor);
        obj.transform.localRotation = Quaternion.identity;

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

        float totalWidth = (n - 1) * cardSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < n; i++)
        {
            float xOffset = startX + i * cardSpacing;

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

            if (animated)
            {
                cards[i].transform.DOLocalMove(localPos, 0.35f).SetEase(Ease.OutCubic);
                cards[i].transform.DOLocalRotateQuaternion(localRot, 0.35f).SetEase(Ease.OutCubic);
            }
            else
            {
                cards[i].transform.localPosition = localPos;
                cards[i].transform.localRotation = localRot;
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

        // 드로우 중에는 카드를 cardAnchor 기준 로컬 좌표로 배치
        // 덱 출발 위치 (로컬: 왼쪽 밖 / 오른쪽 밖)
        Vector3 playerDeckLocal = new Vector3(-1.5f, -0.2f, 0);
        Vector3 enemyDeckLocal = new Vector3(1.5f, 0.2f, 0);

        // 가운데 배열 위치 (로컬)
        float drawSpacing = cardSpacing * 1.3f;

        Vector3[] playerCenterLocal = new Vector3[totalPlayer];
        float pStartX = -(totalPlayer - 1) * drawSpacing / 2f;
        for (int i = 0; i < totalPlayer; i++)
            playerCenterLocal[i] = new Vector3(pStartX + i * drawSpacing, -0.2f, 0);

        Vector3[] enemyCenterLocal = new Vector3[totalEnemy];
        float eStartX = -(totalEnemy - 1) * drawSpacing / 2f;
        for (int i = 0; i < totalEnemy; i++)
            enemyCenterLocal[i] = new Vector3(eStartX + i * drawSpacing, 0.5f, 0);

        // 모든 카드를 앵커 하위에서 덱 위치에 숨김
        for (int i = 0; i < totalPlayer; i++)
        {
            myCards[i].transform.SetParent(cardAnchor);
            myCards[i].transform.localPosition = playerDeckLocal;
            myCards[i].transform.localRotation = Quaternion.identity;
            myCards[i].transform.localScale = Vector3.zero;
        }
        for (int i = 0; i < totalEnemy; i++)
        {
            enemyCards[i].transform.SetParent(cardAnchor); // 드로우 중엔 같은 앵커
            enemyCards[i].transform.localPosition = enemyDeckLocal;
            enemyCards[i].transform.localRotation = Quaternion.identity;
            enemyCards[i].transform.localScale = Vector3.zero;
        }

        // === 1단계: 덱에서 가운데로 교대 드로우 ===
        int pi = 0, ei = 0;
        bool playerTurn = true;
        float drawScale = 1.1f;
        Vector3 drawScaleVec = new Vector3(cardWidth * drawScale, cardHeight * drawScale, 1f);

        while (pi < totalPlayer || ei < totalEnemy)
        {
            if (playerTurn && pi < totalPlayer)
            {
                myCards[pi].transform.DOLocalMove(playerCenterLocal[pi], 0.3f).SetEase(Ease.OutCubic);
                myCards[pi].transform.DOScale(drawScaleVec, 0.3f).SetEase(Ease.OutBack);
                pi++;
            }
            else if (!playerTurn && ei < totalEnemy)
            {
                enemyCards[ei].transform.DOLocalMove(enemyCenterLocal[ei], 0.3f).SetEase(Ease.OutCubic);
                enemyCards[ei].transform.DOScale(drawScaleVec, 0.3f).SetEase(Ease.OutBack);
                ei++;
            }
            playerTurn = !playerTurn;
            yield return new WaitForSeconds(0.25f);
        }

        yield return new WaitForSeconds(0.6f);

        // === 2단계: 정렬 ===
        List<Card3D> sortedPlayer = new(myCards);
        sortedPlayer.Sort((a, b) =>
            GetSortOrder(a.cardData.unitData).CompareTo(GetSortOrder(b.cardData.unitData)));

        List<Card3D> sortedEnemy = new(enemyCards);
        sortedEnemy.Sort((a, b) =>
            GetSortOrder(a.cardData.unitData).CompareTo(GetSortOrder(b.cardData.unitData)));

        // 배치 페이즈 타이틀
        PlacementUI.Instance?.ShowPhaseTitle("배치 페이즈");
        yield return new WaitForSeconds(0.5f);

        // === 3단계: 정렬 순서대로 핸드로 이동 (로컬 좌표) ===
        int nPlayer = sortedPlayer.Count;
        int nEnemy = sortedEnemy.Count;
        float handTotalW = (nPlayer - 1) * cardSpacing;
        float handStartX = -handTotalW / 2f;
        float enemyTotalW = (nEnemy - 1) * cardSpacing;
        float enemyStartX = -enemyTotalW / 2f;
        Vector3 cardScale = new Vector3(cardWidth, cardHeight, 1f);

        int maxCards = Mathf.Max(nPlayer, nEnemy);
        for (int i = 0; i < maxCards; i++)
        {
            if (i < nPlayer)
            {
                var card = sortedPlayer[i];
                card.transform.SetParent(cardAnchor);

                float t = nPlayer > 1 ? (i - (nPlayer - 1) / 2f) / ((nPlayer - 1) / 2f) : 0;
                Vector3 localPos = new Vector3(handStartX + i * cardSpacing, arcHeight * (1f - t * t), 0);
                Quaternion localRot = Quaternion.Euler(0, 0, -arcMaxRotation * t);

                card.SetHandTransform(cardAnchor.TransformPoint(localPos), cardAnchor.rotation * localRot, i);
                card.transform.DOLocalMove(localPos, 0.3f).SetEase(Ease.OutCubic);
                card.transform.DOScale(cardScale, 0.3f).SetEase(Ease.OutBack);
                card.transform.DOLocalRotateQuaternion(localRot, 0.3f).SetEase(Ease.OutCubic);
            }

            if (i < nEnemy)
            {
                var card = sortedEnemy[i];
                card.transform.SetParent(enemyCardAnchor);

                Vector3 localPos = new Vector3(enemyStartX + i * cardSpacing, 0, 0);
                Quaternion localRot = Quaternion.identity;

                card.SetHandTransform(enemyCardAnchor.TransformPoint(localPos), enemyCardAnchor.rotation, i);
                card.transform.DOLocalMove(localPos, 0.3f).SetEase(Ease.OutCubic);
                card.transform.DOScale(cardScale, 0.3f).SetEase(Ease.OutBack);
            }

            yield return new WaitForSeconds(0.12f);
        }

        yield return new WaitForSeconds(0.35f);

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
