using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;

/// <summary>
/// 3D UI 담당: 페이즈 타이틀, 메시지, 버튼, 정보 텍스트.
/// 모든 UI가 카메라 자식으로 존재하여 카드/필드와 같은 공간에 렌더링.
/// </summary>
public class PlacementUI : MonoBehaviour
{
    public static PlacementUI Instance { get; private set; }

    [Header("폰트")]
    public TMP_FontAsset koreanFont;

    // 3D UI 요소
    private Camera mainCamera;
    private Transform uiAnchor;        // 카메라 자식, 모든 UI의 부모
    private TextMeshPro roundText;
    private TextMeshPro infoText;
    private GameObject confirmButton;
    private TextMeshPro confirmText;
    private Renderer confirmRenderer;
    private bool confirmInteractable = true;

    // UI 배치 계산용
    private float uiDepth;
    private float uiHalfH;
    private float uiHalfW;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // EventSystem (입력 처리용)
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        mainCamera = Camera.main;
        CreateUIAnchor();
        CreateUI();

        // GameManager 이벤트 구독
        GameManager.Instance.OnPhaseChanged += OnPhaseChanged;
        GameManager.Instance.OnCardsDrawn += OnCardsDrawn;
        GameManager.Instance.OnPlacementNotReady += OnPlacementNotReady;

        if (GameManager.Instance.PlayerDeck != null && GameManager.Instance.PlayerDeck.HandCount > 0)
        {
            roundText.text = $"라운드 {GameManager.Instance.CurrentRound} | 승: {GameManager.Instance.PlayerWins} - {GameManager.Instance.EnemyWins}";
            StartCoroutine(DrawPhaseSequence3D());
        }
    }

    private void Update()
    {
        // 3D 버튼 클릭 감지
        if (confirmInteractable && confirmButton != null && confirmButton.activeSelf)
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                {
                    if (hit.collider.gameObject == confirmButton)
                    {
                        OnConfirmClicked();
                    }
                }
            }
        }
    }

    /// <summary>
    /// 카메라 자식으로 UI 앵커 생성 (카드보다 앞에)
    /// </summary>
    private void CreateUIAnchor()
    {
        float handDepth = Hand3D.Instance != null ? Hand3D.Instance.handDepth : 6f;
        uiDepth = handDepth * 0.25f; // 카드보다 훨씬 가까이
        uiHalfH = uiDepth * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        uiHalfW = uiHalfH * mainCamera.aspect;

        var anchorObj = new GameObject("UIAnchor");
        anchorObj.transform.SetParent(mainCamera.transform);
        anchorObj.transform.localPosition = new Vector3(0, 0, uiDepth);
        anchorObj.transform.localRotation = Quaternion.identity;
        uiAnchor = anchorObj.transform;
    }

    private void CreateUI()
    {
        // 라운드 표시 (화면 상단)
        roundText = Create3DText("RoundText", "라운드 1 | 배치 페이즈",
            new Vector3(0, 0.796f, 0), 2f, 0.37f);

        // 배치 정보 (화면 중앙 약간 아래)
        infoText = Create3DText("InfoText", "카드를 드래그하여 필드에 배치하세요 (0/5)",
            new Vector3(0, -0.668f, 0), 1.2f, 0.39f);

        // 배치 확정 버튼 (화면 우하단)
        CreateConfirmButton();
    }

    private void CreateConfirmButton()
    {
        confirmButton = GameObject.CreatePrimitive(PrimitiveType.Quad);
        confirmButton.name = "ConfirmButton";
        confirmButton.transform.SetParent(uiAnchor);
        confirmButton.transform.localPosition = new Vector3(1.184f, -0.65f, 0.191f);
        confirmButton.transform.localRotation = Quaternion.identity;
        confirmButton.transform.localScale = new Vector3(0.34f, 0.12754f, 1.22785f);

        // 콜라이더 교체 (Quad 기본 MeshCollider → BoxCollider)
        Destroy(confirmButton.GetComponent<Collider>());
        var col = confirmButton.AddComponent<BoxCollider>();
        col.size = new Vector3(1, 1, 0.1f);

        confirmRenderer = confirmButton.GetComponent<Renderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.color = new Color(0.15f, 0.68f, 0.38f);
        confirmRenderer.material = mat;

        // 버튼 텍스트
        var textObj = new GameObject("ConfirmText");
        textObj.transform.SetParent(confirmButton.transform);
        textObj.transform.localPosition = new Vector3(0, 0, -0.01f);
        textObj.transform.localRotation = Quaternion.identity;
        // 버튼 스케일의 역수로 텍스트 균일하게
        float bx = 0.34f, by = 0.12754f;
        textObj.transform.localScale = new Vector3(1f / bx, 1f / by, 1f);

        confirmText = textObj.AddComponent<TextMeshPro>();
        confirmText.text = "배치 확정";
        confirmText.fontSize = 0.5f;
        confirmText.alignment = TextAlignmentOptions.Center;
        confirmText.color = Color.white;
        confirmText.raycastTarget = false;
        confirmText.rectTransform.sizeDelta = new Vector2(1f, 0.5f);
        if (koreanFont != null) confirmText.font = koreanFont;
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
                SetConfirmInteractable(false);
                roundText.text = $"라운드 {GameManager.Instance.CurrentRound} | 승: {GameManager.Instance.PlayerWins} - {GameManager.Instance.EnemyWins}";
                break;

            case GameManager.GamePhase.Placement:
                SetConfirmInteractable(true);
                break;

            case GameManager.GamePhase.Battle:
                ShowPhaseTitle("전투 페이즈");
                infoText.text = "전투 시작! (전투 로직은 추후 구현)";
                SetConfirmInteractable(false);
                break;
        }
    }

    private void SetConfirmInteractable(bool value)
    {
        confirmInteractable = value;
        if (confirmRenderer != null)
        {
            var c = confirmRenderer.material.color;
            c.a = value ? 1f : 0.4f;
            confirmRenderer.material.color = value ? new Color(0.15f, 0.68f, 0.38f) : new Color(0.3f, 0.3f, 0.3f);
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

        yield return new WaitForSeconds(0.3f);

        ShowPhaseTitle($"Round {GameManager.Instance.CurrentRound}");
        yield return new WaitForSeconds(1.5f);

        ShowPhaseTitle("드로우 페이즈");
        yield return new WaitForSeconds(1.2f);

        var gm = GameManager.Instance;
        Hand3D.Instance.CreatePlayerCards(gm.PlayerDeck.Hand);
        Hand3D.Instance.CreateEnemyCards(gm.EnemyDeck.Hand);

        yield return StartCoroutine(Hand3D.Instance.AnimateDrawSequence());

        yield return new WaitForSeconds(0.5f);

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
        titleObj.transform.SetParent(uiAnchor);
        titleObj.transform.localPosition = new Vector3(0, 0.124f, 0);
        titleObj.transform.localRotation = Quaternion.identity;

        var tmp = titleObj.AddComponent<TextMeshPro>();
        tmp.text = title;
        tmp.fontSize = 8f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        tmp.rectTransform.sizeDelta = new Vector2(uiHalfW * 8f, uiHalfH * 2f);
        tmp.enableWordWrapping = false; // 줄바꿈 방지
        if (koreanFont != null) tmp.font = koreanFont;

        var cg = titleObj.AddComponent<CanvasGroup>();

        titleObj.transform.localScale = Vector3.one * 0.18f * 0.7f;
        cg.alpha = 0f;

        Sequence seq = DOTween.Sequence();
        seq.Append(titleObj.transform.DOScale(0.18f, 0.3f).SetEase(Ease.OutBack));
        seq.Join(cg.DOFade(1f, 0.2f));
        seq.AppendInterval(0.8f);
        seq.Append(cg.DOFade(0f, 0.4f));
        seq.OnComplete(() => Destroy(titleObj));
    }

    public void ShowFloatingMessage(string message)
    {
        var msgObj = new GameObject("FloatingMessage");
        msgObj.transform.SetParent(uiAnchor);
        msgObj.transform.localPosition = Vector3.zero;
        msgObj.transform.localRotation = Quaternion.identity;

        var tmp = msgObj.AddComponent<TextMeshPro>();
        tmp.text = message;
        tmp.fontSize = 4f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.3f, 0.3f);
        tmp.raycastTarget = false;
        tmp.rectTransform.sizeDelta = new Vector2(uiHalfW * 3f, uiHalfH * 0.5f);
        if (koreanFont != null) tmp.font = koreanFont;

        var cg = msgObj.AddComponent<CanvasGroup>();

        Sequence seq = DOTween.Sequence();
        seq.Append(msgObj.transform.DOLocalMoveY(uiHalfH * 0.15f, 0.8f).SetEase(Ease.OutCubic));
        seq.Join(cg.DOFade(0f, 0.8f).SetDelay(0.3f));
        seq.OnComplete(() => Destroy(msgObj));
    }

    // === 유틸 ===

    private TextMeshPro Create3DText(string name, string content, Vector3 localPos, float fontSize, float scale = 1f)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(uiAnchor);
        obj.transform.localPosition = localPos;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one * scale;

        var tmp = obj.AddComponent<TextMeshPro>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        tmp.rectTransform.sizeDelta = new Vector2(uiHalfW * 2.5f, uiHalfH * 0.3f);
        if (koreanFont != null) tmp.font = koreanFont;

        return tmp;
    }
}
