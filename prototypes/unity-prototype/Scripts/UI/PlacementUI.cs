using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// 오버레이 UI 담당: 페이즈 타이틀, 메시지, 버튼, 딤 오버레이, 정보 텍스트.
/// 카드/핸드는 Hand3D가 담당.
/// </summary>
public class PlacementUI : MonoBehaviour
{
    public static PlacementUI Instance { get; private set; }

    [Header("폰트")]
    public TMP_FontAsset koreanFont;

    private Canvas canvas;
    private TextMeshProUGUI infoText;
    private TextMeshProUGUI roundText;
    private Button confirmButton;
    private Image dimOverlay;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // EventSystem이 없으면 생성
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        CreateCanvas();
        CreateUI();

        // GameManager 이벤트 구독
        GameManager.Instance.OnPhaseChanged += OnPhaseChanged;
        GameManager.Instance.OnCardsDrawn += OnCardsDrawn;
        GameManager.Instance.OnPlacementNotReady += OnPlacementNotReady;

        // 이미 카드가 드로우된 상태면 드로우 페이즈 시퀀스 시작
        if (GameManager.Instance.PlayerDeck != null && GameManager.Instance.PlayerDeck.HandCount > 0)
        {
            roundText.text = $"라운드 {GameManager.Instance.CurrentRound} | 승: {GameManager.Instance.PlayerWins} - {GameManager.Instance.EnemyWins}";
            StartCoroutine(DrawPhaseSequence3D());
        }
    }

    private void CreateCanvas()
    {
        var canvasObj = new GameObject("OverlayCanvas");
        canvasObj.transform.SetParent(transform);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();
    }

    private void CreateUI()
    {
        // 라운드 표시
        roundText = CreateText(canvas.transform, "RoundText", "라운드 1 | 배치 페이즈",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -10), 20);

        // 배치 정보
        infoText = CreateText(canvas.transform, "InfoText", "카드를 드래그하여 필드에 배치하세요 (0/5)",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -50), 16);

        // 까만색 오버레이
        CreateDimOverlay();

        // 배치 확정 버튼
        CreateConfirmButton();
    }

    private void CreateDimOverlay()
    {
        var overlayObj = new GameObject("DimOverlay");
        overlayObj.transform.SetParent(canvas.transform, false);
        var rect = overlayObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        dimOverlay = overlayObj.AddComponent<Image>();
        dimOverlay.color = new Color(0, 0, 0, 0);
        dimOverlay.raycastTarget = false;
    }

    private void CreateConfirmButton()
    {
        var btnObj = new GameObject("ConfirmButton");
        btnObj.transform.SetParent(canvas.transform, false);
        var btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1f, 0f);
        btnRect.anchorMax = new Vector2(1f, 0f);
        btnRect.anchoredPosition = new Vector2(-80, 40);
        btnRect.sizeDelta = new Vector2(120, 40);

        var btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.15f, 0.68f, 0.38f);

        confirmButton = btnObj.AddComponent<Button>();
        confirmButton.onClick.AddListener(OnConfirmClicked);

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        var text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "배치 확정";
        text.fontSize = 16;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;
        if (koreanFont != null) text.font = koreanFont;
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    private void OnConfirmClicked()
    {
        GameManager.Instance.ConfirmPlacement();
    }

    // === 이벤트 핸들러 ===

    private void OnPhaseChanged(GameManager.GamePhase phase)
    {
        switch (phase)
        {
            case GameManager.GamePhase.Draw:
                ShowPhaseTitle("드로우 페이즈");
                if (confirmButton != null) confirmButton.interactable = false;
                roundText.text = $"라운드 {GameManager.Instance.CurrentRound} | 승: {GameManager.Instance.PlayerWins} - {GameManager.Instance.EnemyWins}";
                break;

            case GameManager.GamePhase.Placement:
                if (confirmButton != null) confirmButton.interactable = true;
                break;

            case GameManager.GamePhase.Battle:
                ShowPhaseTitle("전투 페이즈");
                infoText.text = "전투 시작! (전투 로직은 추후 구현)";
                if (confirmButton != null) confirmButton.interactable = false;
                break;
        }
    }

    private void OnPlacementNotReady(int remaining)
    {
        ShowFloatingMessage($"유닛을 {remaining}개 더 배치해주세요!");
    }

    private void OnCardsDrawn(List<CardData> drawn)
    {
        StartCoroutine(DrawPhaseSequence3D());
    }

    // === 3D 드로우 페이즈 ===

    private IEnumerator DrawPhaseSequence3D()
    {
        if (Hand3D.Instance == null) yield break;

        // 오버레이 + 라운드 표시
        dimOverlay.transform.SetAsLastSibling();
        dimOverlay.DOFade(0.8f, 0.4f);
        yield return new WaitForSeconds(0.5f);

        ShowPhaseTitle($"Round {GameManager.Instance.CurrentRound}");
        yield return new WaitForSeconds(1.5f);

        ShowPhaseTitle("드로우 페이즈");
        yield return new WaitForSeconds(1.2f);

        // 3D 카드 생성
        var gm = GameManager.Instance;
        Hand3D.Instance.CreatePlayerCards(gm.PlayerDeck.Hand);
        Hand3D.Instance.CreateEnemyCards(gm.EnemyDeck.Hand);

        // 드로우 애니메이션
        yield return StartCoroutine(Hand3D.Instance.AnimateDrawSequence());

        // 오버레이 페이드아웃
        dimOverlay.DOFade(0f, 0.8f);
        yield return new WaitForSeconds(0.5f);

        // 배치 페이즈 진입
        GameManager.Instance.EnterPlacementPhase();
    }

    // === 공개 메서드 ===

    public void UpdateInfoText()
    {
        int placed = GameManager.Instance.PlayerPlacements.Count;
        infoText.text = $"카드를 드래그하여 필드에 배치하세요 ({placed}/5)";
        roundText.text = $"라운드 {GameManager.Instance.CurrentRound} | 승: {GameManager.Instance.PlayerWins} - {GameManager.Instance.EnemyWins}";
    }

    public void ShowPhaseTitle(string title)
    {
        var titleObj = new GameObject("PhaseTitle");
        titleObj.transform.SetParent(canvas.transform, false);
        titleObj.transform.SetAsLastSibling();

        var rect = titleObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(600, 80);

        var tmp = titleObj.AddComponent<TextMeshProUGUI>();
        tmp.text = title;
        tmp.fontSize = 48;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        if (koreanFont != null) tmp.font = koreanFont;

        var cg = titleObj.AddComponent<CanvasGroup>();

        titleObj.transform.localScale = Vector3.one * 0.7f;
        cg.alpha = 0f;

        Sequence seq = DOTween.Sequence();
        seq.Append(titleObj.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
        seq.Join(cg.DOFade(1f, 0.2f));
        seq.AppendInterval(0.8f);
        seq.Append(cg.DOFade(0f, 0.4f));
        seq.OnComplete(() => Destroy(titleObj));
    }

    public void ShowFloatingMessage(string message)
    {
        var msgObj = new GameObject("FloatingMessage");
        msgObj.transform.SetParent(canvas.transform, false);

        var rect = msgObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, 0);
        rect.sizeDelta = new Vector2(600, 60);

        var tmp = msgObj.AddComponent<TextMeshProUGUI>();
        tmp.text = message;
        tmp.fontSize = 30;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.3f, 0.3f);
        tmp.raycastTarget = false;
        if (koreanFont != null) tmp.font = koreanFont;

        var cg = msgObj.AddComponent<CanvasGroup>();

        Sequence seq = DOTween.Sequence();
        seq.Append(rect.DOAnchorPosY(30, 0.8f).SetEase(Ease.OutCubic));
        seq.Join(cg.DOFade(0f, 0.8f).SetDelay(0.3f));
        seq.OnComplete(() => Destroy(msgObj));
    }

    // === 유틸 ===

    private TextMeshProUGUI CreateText(Transform parent, string name, string content,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, int fontSize)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(500, 30);

        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        if (koreanFont != null) tmp.font = koreanFont;

        return tmp;
    }
}
