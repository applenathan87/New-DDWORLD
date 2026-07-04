using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace MawangHR
{
    /// 3D 데스크 + pick-up-to-read:
    /// 책상 위 종이를 클릭하면 **종이가 카메라 앞으로 들려 올라온다** (카메라는 살짝 푸시인만).
    /// 들고 읽으며 단서 마킹 → 도장을 잡아 들린 종이 위에 그대로 내리찍기 → 결산(2D).
    public class GameFlow : MonoBehaviour
    {
        private GameData data;
        private Canvas canvas;
        private DeskRig rig;
        private RectTransform screen;

        private DayConfig day;
        private List<Applicant> lineup;
        private int index;
        private int revealIndex;
        private bool stamping;
        private bool screeningActive;
        private bool holding; // 종이 두 장을 들어 읽는 중인가

        // 판정 기록
        private readonly List<bool> verdicts = new List<bool>();
        private readonly List<bool> evidenceHits = new List<bool>();   // 결정적 단서를 올바른 방향으로 짚었는가
        private readonly List<bool> keyMisreads = new List<bool>();    // 결정적 단서를 반대 방향으로 읽었는가
        private readonly List<List<string>> markedRecords = new List<List<string>>();

        // 현재 지원자의 단서 마킹 상태 — 인덱스 → 방향 (true = V 합격 신호, false = X 탈락 신호)
        private readonly Dictionary<int, bool> marks = new Dictionary<int, bool>();
        private readonly List<string> clueTexts = new List<string>();
        private readonly List<string> clueEvidence = new List<string>();
        private readonly List<Image> clueBgs = new List<Image>();
        private readonly List<TextMeshProUGUI> clueGlyphs = new List<TextMeshProUGUI>();

        // 3D 페이즈 UI
        private RectTransform hud;
        private TextMeshProUGUI progressLabel;
        private TextMeshProUGUI hintLabel;
        private TextMeshProUGUI memoContent;
        private GameObject backBtn;
        private GameObject zoomOverlayResume, zoomOverlayJd;
        private RectTransform resumeContent, jdContent;
        private bool stampsCreated;
        private Coroutine warnCo, resumeTweenCo, jdTweenCo;

        private static readonly Color MarkPosBg = new Color(0.24f, 0.49f, 0.30f, 0.22f); // V 합격 신호
        private static readonly Color MarkNegBg = new Color(0.69f, 0.23f, 0.18f, 0.22f); // X 탈락 신호
        private const string DefaultHint = "종이 클릭 = 들기 · 단서에 좌클릭 = V(합격 신호) / 우클릭 = X(탈락 신호) · 도장을 서류에 쾅";
        private const string MemoDefault = "(깃펜: 단서에 좌클릭 V = 합격 신호\n우클릭 X = 탈락 신호)";

        public void Run(GameData gameData, Canvas mainCanvas, DeskRig deskRig)
        {
            data = gameData;
            canvas = mainCanvas;
            rig = deskRig;
            day = data.days[0];
            lineup = day.applicantIds
                .Select(id => data.applicants.FirstOrDefault(a => a.id == id))
                .Where(a => a != null)
                .ToList();
            verdicts.Clear();
            evidenceHits.Clear();
            keyMisreads.Clear();
            markedRecords.Clear();
            index = 0;
            revealIndex = 0;
            screeningActive = false;
            stamping = false;
            holding = false;
            rig.ClearDone();
            rig.SetPending(lineup.Count);
            DestroyHud();
            ShowIntro();
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (!screeningActive || !holding || stamping) return;

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                PutDown();

            // 허공(아무것도 안 맞은 곳) 클릭 = 내려놓기
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame &&
                EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject())
                PutDown();
#endif
        }

        private RectTransform NewScreen()
        {
            if (screen != null) Destroy(screen.gameObject);
            screen = UiKit.PanelRect(canvas.transform, "Screen", UiKit.Bg);
            UiKit.Fill(screen);
            return screen;
        }

        private void ClearScreen()
        {
            if (screen != null) { Destroy(screen.gameObject); screen = null; }
        }

        private Jd FindJd(string id) => data.jds.FirstOrDefault(j => j.id == id);

        private static string JdBlock(Jd jd)
        {
            if (jd == null) return "(JD 없음)";
            string req = string.Join("\n", jd.required.Select(r => "  · " + r));
            string ban = string.Join("\n", jd.banned.Select(b => "  · " + b));
            return $"<b>{jd.title}</b>\n<i>{jd.note}</i>\n\n<color=#3E7D4E><b>요구 자질</b></color>\n{req}\n\n<color=#9E3B32><b>결격 사유</b></color>\n{ban}";
        }

        private string FirstEvidenceText(Applicant a)
        {
            var line = a.resumeLines.FirstOrDefault(l => l.evidence == a.correct);
            if (line != null) return line.text;
            if (a.specialEvidence == a.correct) return a.special;
            return null;
        }

        // ─── 화면 1: 출근 (2D 공문 오버레이) ─────────────────────────────

        private void ShowIntro()
        {
            var s = NewScreen();

            UiKit.LabelAt(s, "마왕성 인사팀", 60, UiKit.Accent, 0, 60, 1920, 80, TextAlignmentOptions.Center);
            UiKit.LabelAt(s, day.title, 34, UiKit.Text, 0, 145, 1920, 50, TextAlignmentOptions.Center);

            var paper = UiKit.PanelRect(s, "Directive", UiKit.Paper);
            UiKit.Place(paper, 360, 220, 1200, 430);
            UiKit.LabelAt(paper, day.directive, 27, UiKit.Ink, 50, 35, 1100, 360, TextAlignmentOptions.TopLeft, true);

            var jdIds = lineup.Select(a => a.jdId).Distinct().ToList();
            float x = 360;
            float w = (1200f - 30f * (jdIds.Count - 1)) / Mathf.Max(1, jdIds.Count);
            foreach (var jdId in jdIds)
            {
                var card = UiKit.PanelRect(s, "JdCard", UiKit.Panel);
                UiKit.Place(card, x, 675, w, 290);
                UiKit.LabelAt(card, JdBlock(FindJd(jdId)), 20, UiKit.Text, 25, 18, w - 50, 254, TextAlignmentOptions.TopLeft, true);
                x += w + 30;
            }

            var btn = UiKit.MakeButton(s, "출근하기", UiKit.Accent, UiKit.Ink, 32, StartScreeningPhase);
            UiKit.Place((RectTransform)btn.transform, 810, 990, 300, 68);
        }

        // ─── 3D 심사 페이즈 ─────────────────────────────

        private void StartScreeningPhase()
        {
            ClearScreen();
            screeningActive = true;
            holding = false;
            BuildHud();

            if (!stampsCreated)
            {
                rig.MakeStamp(true, "통 과", UiKit.Approve, new Vector3(0.40f, DeskRig.DeskTop, -0.36f),
                    CanStampNow, OnSlam, OnStampBlocked);
                rig.MakeStamp(false, "탈 락", UiKit.Reject, new Vector3(0.58f, DeskRig.DeskTop, -0.36f),
                    CanStampNow, OnSlam, OnStampBlocked);
                stampsCreated = true;
            }

            ShowApplicantOnDesk();
        }

        private bool CanStampNow() => screeningActive && !stamping && marks.Count > 0;

        private void OnStampBlocked()
        {
            if (!screeningActive || stamping || marks.Count > 0) return;
            if (warnCo != null) StopCoroutine(warnCo);
            warnCo = StartCoroutine(WarnNoEvidence());
        }

        private void BuildHud()
        {
            DestroyHud();
            hud = UiKit.Rect("HUD", canvas.transform);
            UiKit.Fill(hud);

            var top = UiKit.PanelRect(hud, "TopBar", UiKit.Panel);
            UiKit.Place(top, 0, 0, 1920, 56);
            UiKit.LabelAt(top, "마왕성 인사팀 — " + day.title, 24, UiKit.Accent, 25, 13, 800, 36, TextAlignmentOptions.TopLeft, true);
            progressLabel = UiKit.LabelAt(top, "", 22, UiKit.TextDim, 900, 15, 995, 36, TextAlignmentOptions.TopRight, true);

            hintLabel = UiKit.LabelAt(hud, DefaultHint, 22, UiKit.TextDim, 0, 66, 1920, 36, TextAlignmentOptions.Center, true);

            var memo = UiKit.PanelRect(hud, "MemoPad", new Color(0.23f, 0.17f, 0.13f, 0.92f));
            UiKit.Place(memo, 1540, 120, 360, 330);
            UiKit.LabelAt(memo, "<b>근거 메모</b>", 22, UiKit.Accent, 20, 12, 320, 30);
            memoContent = UiKit.LabelAt(memo, MemoDefault, 19, UiKit.Text,
                20, 48, 320, 268, TextAlignmentOptions.TopLeft, true);

            var back = UiKit.MakeButton(hud, "종이 내려놓기 (ESC)", UiKit.Panel, UiKit.Text, 20, PutDown);
            UiKit.Place((RectTransform)back.transform, 20, 76, 240, 50);
            backBtn = back.gameObject;
            backBtn.SetActive(false);
        }

        private void DestroyHud()
        {
            if (hud != null) { Destroy(hud.gameObject); hud = null; }
        }

        private void UpdateHudProgress()
        {
            int passCount = verdicts.Count(v => v);
            if (progressLabel != null)
                progressLabel.text = $"서류 {index + 1} / {lineup.Count}    ·    통과 {passCount}명 (최소 {day.quotaMin}명)";
        }

        // ─── 종이 들어올리기 / 내려놓기 ───

        /// 어느 종이를 집든 이력서+공문이 **함께** 독서 포즈로 올라온다 (교차 대조가 코어라서)
        private void LiftPapers()
        {
            if (!screeningActive || stamping || holding) return;
            holding = true;
            rig.TweenToHold();
            rig.SetQuillHeld(true);
            Sfx.Swish();

            Vector3 pos; Quaternion rot;
            rig.GetHeldPose(0.70f, -0.03f, 0.03f, 3f, out pos, out rot);     // 이력서 = 중앙-약간 오른쪽 (오른쪽은 도장 공간)
            StartPaperTween(true, pos, rot);
            rig.GetHeldPose(0.76f, -0.02f, -0.28f, -10f, out pos, out rot);  // 공문 = 왼쪽, 살짝 뒤에 겹치게
            StartPaperTween(false, pos, rot);
            SyncHeldUi();
        }

        private void PutDown()
        {
            if (!screeningActive || stamping || !holding) return;
            holding = false;
            rig.TweenToDesk();
            rig.SetQuillHeld(false);
            Sfx.Swish();
            StartPaperTween(true, rig.ResumeHome, rig.ResumeHomeRot);
            StartPaperTween(false, rig.JdHome, rig.JdHomeRot);
            SyncHeldUi();
        }

        private void StartPaperTween(bool resume, Vector3 pos, Quaternion rot)
        {
            var co = StartCoroutine(TweenPaper(resume ? rig.ResumeCanvas : rig.JdCanvas, pos, rot, 0.22f));
            if (resume) { if (resumeTweenCo != null) StopCoroutine(resumeTweenCo); resumeTweenCo = co; }
            else { if (jdTweenCo != null) StopCoroutine(jdTweenCo); jdTweenCo = co; }
        }

        private IEnumerator TweenPaper(RectTransform paper, Vector3 toPos, Quaternion toRot, float dur)
        {
            Vector3 fromP = paper.position;
            Quaternion fromR = paper.rotation;
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / dur);
                paper.position = Vector3.Lerp(fromP, toPos, k);
                paper.rotation = Quaternion.Slerp(fromR, toRot, k);
                yield return null;
            }
            paper.position = toPos;
            paper.rotation = toRot;
        }

        /// 들었는지 여부에 따라 클릭 오버레이·내려놓기 버튼 동기화
        private void SyncHeldUi()
        {
            if (zoomOverlayResume != null) zoomOverlayResume.SetActive(!holding);
            if (zoomOverlayJd != null) zoomOverlayJd.SetActive(!holding);
            if (backBtn != null) backBtn.SetActive(holding);
        }

        /// 현재 지원자의 서류를 책상에 올린다
        private void ShowApplicantOnDesk()
        {
            stamping = false;
            holding = false;
            marks.Clear();
            clueTexts.Clear();
            clueEvidence.Clear();
            clueBgs.Clear();
            clueGlyphs.Clear();
            if (memoContent != null) memoContent.text = MemoDefault;

            var a = lineup[index];
            rig.SetPending(lineup.Count - index - 1);
            BuildJdContent(a);
            BuildResumeContent(a);
            EnsureZoomOverlays();
            SyncHeldUi();
            UpdateHudProgress();
            StartCoroutine(PaperSlideIn());
        }

        private void BuildJdContent(Applicant a)
        {
            if (jdContent != null) Destroy(jdContent.gameObject);
            jdContent = UiKit.Rect("Content", rig.JdCanvas);
            UiKit.Fill(jdContent);

            UiKit.LabelAt(jdContent, "<b>오늘의 공문</b>", 30, UiKit.StampInk, 40, 28, 540, 40);
            UiKit.LabelAt(jdContent, day.directive, 22, UiKit.Ink, 40, 76, 540, 380, TextAlignmentOptions.TopLeft, true);
            var divider = UiKit.PanelRect(jdContent, "Divider", UiKit.InkDim);
            UiKit.Place(divider, 40, 470, 540, 3);
            UiKit.LabelAt(jdContent, "<b>지원 직무</b>", 26, UiKit.StampInk, 40, 490, 540, 36);
            UiKit.LabelAt(jdContent, JdBlock(FindJd(a.jdId)), 22, UiKit.Ink, 40, 532, 540, 340, TextAlignmentOptions.TopLeft, true);
        }

        private void BuildResumeContent(Applicant a)
        {
            if (resumeContent != null) Destroy(resumeContent.gameObject);
            resumeContent = UiKit.Rect("Content", rig.ResumeCanvas);
            UiKit.Fill(resumeContent);
            var jd = FindJd(a.jdId);

            UiKit.LabelAt(resumeContent, $"지원자 서류  ·  MHR-{day.day:00}-{index + 1:000}", 20, UiKit.InkDim, 45, 30, 810, 32);
            UiKit.LabelAt(resumeContent, a.name, 48, UiKit.Ink, 45, 68, 810, 66, TextAlignmentOptions.TopLeft, true);
            UiKit.LabelAt(resumeContent,
                $"종족: {a.species}\n지원 직무: {(jd != null ? jd.title : a.jdId)}      희망 연봉: {a.salary}",
                24, UiKit.Ink, 45, 144, 810, 78, TextAlignmentOptions.TopLeft, true);
            UiKit.LabelAt(resumeContent, "“" + a.quote + "”", 23, UiKit.InkDim, 45, 230, 810, 56, TextAlignmentOptions.TopLeft, true);

            var divider = UiKit.PanelRect(resumeContent, "Divider", UiKit.PaperShade);
            UiKit.Place(divider, 45, 292, 810, 4);

            UiKit.LabelAt(resumeContent, "<b>이력</b>  <size=19><color=#6B5A42>(줄을 클릭해 판정 근거를 표시)</color></size>",
                26, UiKit.Ink, 45, 312, 810, 38);

            float y = 360;
            foreach (var line in a.resumeLines)
            {
                AddClueLine(resumeContent, "· " + line.text, line.evidence, UiKit.Ink, 45, y, 810, 66);
                y += 74;
            }

            UiKit.LabelAt(resumeContent, "<b>특이사항 (인사팀 메모)</b>", 24, UiKit.StampInk, 45, 640, 810, 36);
            AddClueLine(resumeContent, a.special, a.specialEvidence, UiKit.StampInk, 45, 682, 810, 100);
            // 아래 여백 = 도장 찍는 자리
        }

        private void EnsureZoomOverlays()
        {
            if (zoomOverlayResume == null)
                zoomOverlayResume = MakeZoomOverlay(rig.ResumeCanvas);
            if (zoomOverlayJd == null)
                zoomOverlayJd = MakeZoomOverlay(rig.JdCanvas);
            zoomOverlayResume.transform.SetAsLastSibling();
            zoomOverlayJd.transform.SetAsLastSibling();
            EnsurePaperBgButton(rig.ResumeCanvas);
            EnsurePaperBgButton(rig.JdCanvas);
        }

        /// 종이 배경(단서가 아닌 영역) 클릭 = 내려놓기.
        /// 캔버스 루트 Image에 버튼을 달아, 단서 줄이 아닌 모든 클릭이 여기로 버블링된다.
        /// 드래그가 있었던 제스처의 클릭은 무시 (재배치 후 놓을 때 내려놓기/마킹 오발 방지)
        private static bool ClickSuppressed => Time.frameCount <= PaperDraggable.LastDragFrame;

        private void EnsurePaperBgButton(RectTransform paperCanvas)
        {
            if (paperCanvas.GetComponent<Button>() != null) return;
            var btn = paperCanvas.gameObject.AddComponent<Button>();
            btn.targetGraphic = paperCanvas.GetComponent<Image>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => { if (holding && !ClickSuppressed) PutDown(); });

            // 들려 있는 동안 끌어서 재배치
            paperCanvas.gameObject.AddComponent<PaperDraggable>()
                .Init(rig.Cam, () => holding && !stamping);
        }

        private GameObject MakeZoomOverlay(RectTransform paperCanvas)
        {
            var rt = UiKit.Rect("ZoomOverlay", paperCanvas);
            UiKit.Fill(rt);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0);
            var btn = rt.gameObject.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(LiftPapers);
            return rt.gameObject;
        }

        // ─── 단서 마킹 ───

        private void AddClueLine(Transform parent, string text, string evidence, Color inkColor,
            float x, float y, float w, float h)
        {
            int clueIndex = clueTexts.Count;
            clueTexts.Add(text.StartsWith("· ") ? text.Substring(2) : text);
            clueEvidence.Add(evidence ?? "");

            var bg = UiKit.PanelRect(parent, "Clue" + clueIndex, Color.clear);
            UiKit.Place(bg, x - 8, y - 4, w + 16, h + 4);
            var img = bg.GetComponent<Image>();
            clueBgs.Add(img);

            bg.gameObject.AddComponent<ClueLine>().Init(clueIndex, ToggleMark);

            // 왼쪽 여백의 V/X 글리프 (깃펜 자국)
            var glyph = UiKit.Label(bg, "", 34, UiKit.Ink, TextAlignmentOptions.Center);
            var grt = (RectTransform)glyph.transform;
            UiKit.Place(grt, -38, 2, 36, 46);
            grt.localRotation = Quaternion.Euler(0, 0, -6f);
            glyph.fontStyle = FontStyles.Bold;
            glyph.raycastTarget = false;
            clueGlyphs.Add(glyph);

            var label = UiKit.Label(bg, text, 25, inkColor, TextAlignmentOptions.TopLeft, true);
            var lrt = (RectTransform)label.transform;
            UiKit.Fill(lrt);
            lrt.offsetMin = new Vector2(8, 2);
            lrt.offsetMax = new Vector2(-8, -2);
        }

        /// 깃펜 마킹 — positive: true = V(합격 신호) / false = X(탈락 신호). 같은 방향 재클릭 = 해제.
        private void ToggleMark(int clueIndex, bool positive)
        {
            if (stamping || !holding || ClickSuppressed) return; // 들고 있을 때만 마킹 (드래그 직후 클릭 무시)

            if (marks.TryGetValue(clueIndex, out bool current) && current == positive)
                marks.Remove(clueIndex);
            else
                marks[clueIndex] = positive;

            bool marked = marks.TryGetValue(clueIndex, out bool pol);
            var bg = clueBgs[clueIndex];
            bg.color = !marked ? Color.clear : (pol ? MarkPosBg : MarkNegBg);
            bg.transform.localRotation = marked
                ? Quaternion.Euler(0, 0, Random.Range(-1.2f, 1.2f))
                : Quaternion.identity;
            clueGlyphs[clueIndex].text = !marked ? "" : (pol ? "V" : "X");
            clueGlyphs[clueIndex].color = pol ? UiKit.Approve : UiKit.StampInk;

            Sfx.Scratch();
            rig.QuillTwitch();

            memoContent.text = marks.Count == 0
                ? MemoDefault
                : string.Join("\n", marks.OrderBy(kv => kv.Key).Select(kv =>
                    (kv.Value ? "<color=#3E7D4E><b>V</b></color> " : "<color=#B03A2E><b>X</b></color> ")
                    + clueTexts[kv.Key]));
        }

        /// 이 방향 표시가 단서의 실제 방향과 일치하는가
        private static bool PolarityMatches(bool positive, string evidence)
            => positive ? evidence == "PASS" : evidence == "FAIL";

        private IEnumerator WarnNoEvidence()
        {
            if (hintLabel == null) yield break;
            hintLabel.text = "근거를 최소 1개 표시해야 도장을 찍을 수 있습니다!";
            hintLabel.color = new Color(1f, 0.45f, 0.38f);
            yield return new WaitForSeconds(1.4f);
            if (hintLabel == null) yield break;
            hintLabel.text = DefaultHint;
            hintLabel.color = UiKit.TextDim;
        }

        // ─── 도장 낙하 ───

        private void OnSlam(bool pass, Vector2 screenPos)
        {
            if (stamping) return;
            stamping = true;
            var a = lineup[index];
            verdicts.Add(pass);
            // 근거 적중 = 결정적 단서를 "올바른 방향(V/X)"으로 표시했는가
            evidenceHits.Add(marks.Any(kv =>
                clueEvidence[kv.Key] == a.correct && PolarityMatches(kv.Value, clueEvidence[kv.Key])));
            keyMisreads.Add(marks.Any(kv =>
                clueEvidence[kv.Key] == a.correct && !PolarityMatches(kv.Value, clueEvidence[kv.Key])));
            markedRecords.Add(marks.OrderBy(kv => kv.Key).Select(kv =>
                (kv.Value ? "<color=#3E7D4E>[V]</color> " : "<color=#B03A2E>[X]</color> ") + clueTexts[kv.Key]).ToList());
            UpdateHudProgress();
            StartCoroutine(SlamRoutine(pass, screenPos));
        }

        private IEnumerator SlamRoutine(bool pass, Vector2 screenPos)
        {
            yield return new WaitForSeconds(0.06f); // 도장 낙하와 동기

            Sfx.Thunk();
            StartCoroutine(rig.CamShake());

            // 잉크 자국 — 도장이 떨어진 바로 그 지점 (스크린 광선 ↔ 종이 면 교차점)
            Vector2 lp;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rig.ResumeCanvas, screenPos, rig.Cam, out lp);
            var imprint = UiKit.Rect("Imprint", resumeContent);
            imprint.anchorMin = imprint.anchorMax = new Vector2(0.5f, 0.5f);
            imprint.pivot = new Vector2(0.5f, 0.5f);
            imprint.anchoredPosition = lp;
            imprint.sizeDelta = new Vector2(430, 170);
            imprint.localRotation = Quaternion.Euler(0, 0, Random.Range(-14f, -4f));

            var frame = UiKit.PanelRect(imprint, "Frame", new Color(0, 0, 0, 0));
            UiKit.Fill(frame);
            var outline = frame.gameObject.AddComponent<Outline>();
            outline.effectColor = pass ? UiKit.Approve : UiKit.StampInk;
            outline.effectDistance = new Vector2(5, 5);
            frame.GetComponent<Image>().raycastTarget = false;

            var label = UiKit.Label(imprint, pass ? "통  과" : "탈  락", 100,
                pass ? UiKit.Approve : UiKit.StampInk, TextAlignmentOptions.Center);
            UiKit.Fill((RectTransform)label.transform);
            label.raycastTarget = false;

            yield return new WaitForSeconds(0.8f);

            // 종이를 완료 더미로 — 들려 있던 자리에서 바로 날아간다
            holding = false;
            SyncHeldUi();
            rig.TweenToDesk();
            rig.SetQuillHeld(false);
            yield return StartCoroutine(PaperFlyToDone());
            rig.AddDone();
            index++;

            if (index < lineup.Count)
            {
                ShowApplicantOnDesk();
            }
            else
            {
                screeningActive = false;
                DestroyHud();
                yield return new WaitForSeconds(0.4f);
                revealIndex = 0;
                ShowReveal();
            }
        }

        private IEnumerator PaperSlideIn()
        {
            var paper = rig.ResumeCanvas;
            Sfx.Swish();
            Vector3 target = rig.ResumeHome;
            Vector3 from = target + new Vector3(0.85f, 0.05f, -0.05f);
            Quaternion targetRot = rig.ResumeHomeRot;
            Quaternion fromRot = Quaternion.Euler(90, 0, -16f);
            float t = 0f;
            const float dur = 0.22f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / dur);
                paper.localPosition = Vector3.Lerp(from, target, k);
                paper.localRotation = Quaternion.Slerp(fromRot, targetRot, k);
                yield return null;
            }
            paper.localPosition = target;
            paper.localRotation = targetRot;
        }

        /// 판정 끝난 종이가 현재 위치(들려 있든 책상이든)에서 완료 더미로 날아간다
        private IEnumerator PaperFlyToDone()
        {
            var paper = rig.ResumeCanvas;
            Sfx.Swish();
            Vector3 from = paper.position;
            Quaternion fromR = paper.rotation;
            Vector3 to = rig.ResumeHome + new Vector3(0.50f, 0.08f, 0.32f);
            Quaternion toR = rig.ResumeHomeRot;
            float t = 0f;
            const float dur = 0.25f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / dur);
                paper.position = Vector3.Lerp(from, to, k);
                paper.rotation = Quaternion.Slerp(fromR, toR, k);
                yield return null;
            }
            // 내용 비우고 원위치 (다음 지원자용)
            paper.position = rig.ResumeHome;
            paper.rotation = rig.ResumeHomeRot;
            if (resumeContent != null) { Destroy(resumeContent.gameObject); resumeContent = null; }
        }

        // ─── 화면 3: 결산 (2D 오버레이) ─────────────────────────────

        private void ShowReveal()
        {
            var s = NewScreen();
            var a = lineup[revealIndex];
            bool myPass = verdicts[revealIndex];
            bool right = myPass == a.CorrectIsPass;
            bool hit = evidenceHits[revealIndex];

            UiKit.LabelAt(s, $"퇴근 결산 — 판정 확인 ({revealIndex + 1}/{lineup.Count})", 34, UiKit.Accent,
                0, 50, 1920, 56, TextAlignmentOptions.Center);

            var card = UiKit.PanelRect(s, "RevealCard", UiKit.Panel);
            UiKit.Place(card, 410, 130, 1100, 760);

            UiKit.LabelAt(card, $"{a.name}  <size=26><color=#A6947C>({a.species} · {FindJd(a.jdId)?.title})</color></size>",
                38, UiKit.Text, 50, 35, 1000, 60, TextAlignmentOptions.TopLeft, true);

            string passWord = "<color=#3E7D4E>통과</color>";
            string failWord = "<color=#9E3B32>탈락</color>";
            UiKit.LabelAt(card,
                $"나의 판정:  {(myPass ? passWord : failWord)}      정답:  {(a.CorrectIsPass ? passWord : failWord)}",
                30, UiKit.Text, 50, 110, 1000, 46);

            UiKit.LabelAt(card, right ? "○  정확한 판단" : "×  오판",
                54, right ? UiKit.Approve : UiKit.Reject, 50, 165, 1000, 76);

            string evidenceText;
            Color evidenceColor;
            string keyClue = FirstEvidenceText(a);
            bool keyMis = keyMisreads[revealIndex];
            if (right && hit)
            {
                evidenceText = "근거·방향 적중 — 단서가 어느 쪽 신호인지까지 정확히 읽었다.";
                evidenceColor = UiKit.Accent;
            }
            else if (keyMis)
            {
                evidenceText = keyClue != null
                    ? $"단서는 짚었지만 <b>방향을 반대로</b> 읽었다 — 여긴 마왕성이다:  “{keyClue}”"
                    : "단서는 짚었지만 방향을 반대로 읽었다.";
                evidenceColor = new Color(0.85f, 0.55f, 0.35f);
            }
            else if (right)
            {
                evidenceText = keyClue != null
                    ? $"판단은 맞았지만, 결정적 단서는 따로 있었다:  “{keyClue}”"
                    : "판단은 맞았지만, 근거가 다소 빗나갔다.";
                evidenceColor = UiKit.TextDim;
            }
            else
            {
                evidenceText = keyClue != null
                    ? $"놓친 단서:  “{keyClue}”"
                    : "단서를 다시 보자.";
                evidenceColor = UiKit.TextDim;
            }
            UiKit.LabelAt(card, evidenceText, 25, evidenceColor, 50, 248, 1000, 66, TextAlignmentOptions.TopLeft, true);

            var divider = UiKit.PanelRect(card, "Divider", UiKit.PanelLight);
            UiKit.Place(divider, 50, 322, 1000, 4);

            UiKit.LabelAt(card, "<b>인사팀장의 총평</b>", 25, UiKit.Accent, 50, 344, 1000, 36);
            UiKit.LabelAt(card, a.reveal, 25, UiKit.Text, 50, 388, 1000, 250, TextAlignmentOptions.TopLeft, true);

            UiKit.LabelAt(card,
                "<color=#6B5A42>내가 표시한 근거:</color>  " + string.Join("  /  ", markedRecords[revealIndex]),
                19, UiKit.TextDim, 50, 650, 1000, 80, TextAlignmentOptions.TopLeft, true);

            bool last = revealIndex == lineup.Count - 1;
            var btn = UiKit.MakeButton(s, last ? "하루 마감" : "다음 서류", UiKit.Accent, UiKit.Ink, 28,
                () => { if (last) ShowSummary(); else { revealIndex++; ShowReveal(); } });
            UiKit.Place((RectTransform)btn.transform, 835, 930, 250, 66);
        }

        // ─── 화면 4: 요약 (2D 오버레이) ─────────────────────────────

        private void ShowSummary()
        {
            var s = NewScreen();

            int correctCount = lineup.Where((a, i) => verdicts[i] == a.CorrectIsPass).Count();
            int hitCount = evidenceHits.Count(h => h);
            int passCount = verdicts.Count(v => v);
            bool quotaOk = passCount >= day.quotaMin;
            bool promoted = correctCount >= 5 && quotaOk;

            UiKit.LabelAt(s, "Day 1 종료 — 인사 평가", 46, UiKit.Accent, 0, 80, 1920, 70, TextAlignmentOptions.Center);

            var card = UiKit.PanelRect(s, "Summary", UiKit.Panel);
            UiKit.Place(card, 510, 190, 900, 620);

            string quotaLine = quotaOk
                ? $"서류 통과: {passCount}명  (지침 {day.quotaMin}명 이상 — 충족)"
                : $"<color=#9E3B32>서류 통과: {passCount}명 — 지침 미달! 텅 빈 면접 대기석을 마왕님이 보셨습니다…</color>";

            UiKit.LabelAt(card,
                $"판정 정확도:  <b>{correctCount} / {lineup.Count}</b>\n근거 적중 (방향 포함):  <b>{hitCount} / {lineup.Count}</b>\n\n{quotaLine}",
                30, UiKit.Text, 60, 50, 780, 220, TextAlignmentOptions.TopLeft, true);

            string verdictText = promoted
                ? "<color=#3E7D4E><b>승진 예고!</b></color>\n\n“제법이군. 내일부터 자네가 1차 면접을 맡게.\n서류는 이제 스켈레톤 인턴이 볼 걸세.”\n\n<size=22><color=#A6947C>— 면접 루프는 다음 빌드에서 열립니다 —</color></size>"
                : "<color=#9E3B32><b>수습 연장의 위기…</b></color>\n\n“서류 보는 눈이 아직 멀었군.\n내일 다시 해보게. 이번엔 공문을 '읽고' 찍으라고.”";

            UiKit.LabelAt(card, verdictText, 28, UiKit.Text, 60, 290, 780, 280, TextAlignmentOptions.Top, true);

            var btn = UiKit.MakeButton(s, "처음부터 다시", UiKit.Accent, UiKit.Ink, 28,
                () => Run(data, canvas, rig));
            UiKit.Place((RectTransform)btn.transform, 810, 860, 300, 70);
        }
    }
}
