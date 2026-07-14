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
        private readonly List<StampDraggable3D> stamps = new List<StampDraggable3D>();
        private Coroutine warnCo, resumeTweenCo, jdTweenCo;
        private string currentDefaultHint = DefaultHint;

        // 스케줄링 페이즈 (Day 1 후반 업무)
        private SchedulingFlow scheduling;
        private bool schedulingActive;
        private Button schedConfirmBtn;
        private List<string> schedulingViolations = new List<string>();

        // ─ 밤 파트 (내 방 — S3a) ─
        private NightRoom nightRoom;                                  // 방은 한 번 짓고 세션 동안 유지 (장식·슬라임 지속)
        private NightFlow night;
        private bool nightActive;
        private int gold;                                             // 세션 지속 골드 (밤 상점 소비)
        private readonly HashSet<string> ownedDeco = new HashSet<string>(); // 산 장식 (재구매 방지)
        private bool nightQpBuff;                                     // 드링크: 다음날 질문 포인트 +1
        private int dayQpBonus;                                       // 오늘 낮 적용분 (StartDay에서 소비)
        private bool crowIntroShown;                                  // 까마귀 상점 첫 등장 안내 1회

        // 면접 페이즈 (Day 2 — S2b)
        private int dayIndex;
        private bool lastPromoted;               // 직전 결산의 승진 여부 (다음날 분기)
        private int pointsLeft;                  // 이번 면접의 남은 질문 포인트
        private ResumeLayout resumeLayout;       // 템플릿 픽셀 명세 (layout.json — 눈대중 좌표 금지)
        private float stmtY, stmtX, stmtW, stmtStep, stmtH; // 진술 기록 흐름 (캔버스 좌표, 레이아웃에서 환산)
        private readonly List<RectTransform> cardRts = new List<RectTransform>();
        private readonly List<Vector3> cardHomes = new List<Vector3>();
        private readonly List<Quaternion> cardHomeRots = new List<Quaternion>();
        private bool[] cardUsed;
        private GameObject speechPanel;          // 지원자 대사 자막 (HUD) — 월드 말풍선은 글씨가 안 보여 화면 자막으로 교체
        private TextMeshProUGUI speechText;
        private Coroutine speechCo;
        private const float CardScale = 0.0007f; // 질문 카드 월드 스케일 (가독성 확보 크기)
        private bool IsInterview => day != null && day.phase == "interview";

        private static readonly Color MarkPosBg = new Color(0.24f, 0.49f, 0.30f, 0.22f); // V 합격 신호
        private static readonly Color MarkNegBg = new Color(0.69f, 0.23f, 0.18f, 0.22f); // X 탈락 신호
        private const string DefaultHint = "종이 클릭 = 들기 · 단서에 좌클릭 = V(합격 신호) / 우클릭 = X(탈락 신호) · 도장을 서류에 쾅";
        private const string MemoDefault = "(깃펜: 단서에 좌클릭 V = 합격 신호\n우클릭 X = 탈락 신호)";

        public void Run(GameData gameData, Canvas mainCanvas, DeskRig deskRig)
        {
            data = gameData;
            canvas = mainCanvas;
            rig = deskRig;
            resumeLayout = ResumeLayout.LoadOrDefault();
            StartDay(0);
        }

        /// 하루 시작 (인덱스 0 = 서류 심사 / 1 = 면접) — 상태 리셋 + 풀 뽑기 + 출근 공문
        private void StartDay(int idx)
        {
            dayIndex = Mathf.Clamp(idx, 0, data.days.Length - 1);
            day = data.days[dayIndex];
            var source = IsInterview ? data.interviewees : data.applicants; // 단계 독립 지원자 세트
            var pool = day.applicantIds
                .Select(id => source.FirstOrDefault(a => a.id == id))
                .Where(a => a != null)
                .ToList();
            lineup = DrawLineup(pool);
            verdicts.Clear();
            evidenceHits.Clear();
            keyMisreads.Clear();
            markedRecords.Clear();
            index = 0;
            revealIndex = 0;
            screeningActive = false;
            schedulingActive = false;
            stamping = false;
            holding = false;
            schedulingViolations.Clear();
            if (scheduling != null) { scheduling.Cleanup(); scheduling = null; }
            nightActive = false;
            if (night != null) { night.Cleanup(); night = null; }
            dayQpBonus = nightQpBuff ? 1 : 0; // 어젯밤 산 드링크는 오늘 소비
            nightQpBuff = false;
            rig.TweenToDesk(); // 밤 방에서 넘어와도 카메라 복귀 (책상이면 제자리 트윈)
            DestroyCards();
            HideMonsterBubble();
            rig.DespawnMonster();
            rig.ResumeCanvas.gameObject.SetActive(true);
            rig.JdCanvas.gameObject.SetActive(true);
            rig.ClearDone();
            rig.SetPending(lineup.Count);
            DestroyHud();
            ShowIntro();
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            // ─────────────────────────────────────────────────────────────
            // ⚠️ [개발 치트 — 임시] 페이즈 점프: 1 = 서류 · 2 = 스케줄 · 3 = 면접 · 4 = 밤(내 방)
            // 테스트 편의용. S4 리포트/빌드 전에 이 블록 통째로 삭제할 것.
            // (README "알려진 우려점"에도 제거 예정으로 기록해 둠)
            // ─────────────────────────────────────────────────────────────
            if (Keyboard.current != null && !stamping)
            {
                if (Keyboard.current.digit1Key.wasPressedThisFrame) { StartDay(0); return; }
                if (Keyboard.current.digit2Key.wasPressedThisFrame) { CheatGoScheduling(); return; }
                if (Keyboard.current.digit3Key.wasPressedThisFrame &&
                    data.days.Length > 1 && data.interviewees.Length > 0) { StartDay(1); return; }
                if (Keyboard.current.digit4Key.wasPressedThisFrame) { CheatGoNight(); return; }
            }
            // ───────────────────────────────────────────── [개발 치트 끝]

            if (!screeningActive || !holding || stamping) return;

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                PutDown();

            // 허공(아무것도 안 맞은 곳) 클릭 = 내려놓기
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame &&
                EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject())
                PutDown();
#endif
        }

        /// ⚠️ [개발 치트 — 임시] 스케줄링 페이즈 즉시 점프 — Day 1 상태로 리셋 후 일정 잡기 인트로부터.
        /// Update()의 치트 블록과 함께 삭제할 것.
        private void CheatGoScheduling()
        {
            StartDay(0);            // 전체 상태 리셋 (서류 인트로 화면이 뜨지만)
            ShowSchedulingIntro();  // 곧바로 일정 잡기 인트로로 덮어씀 (NewScreen이 이전 화면 파괴)
        }

        /// ⚠️ [개발 치트 — 임시] 밤 파트 즉시 점프 — 낮 성적이 없으면 기본급만 나온다.
        /// Update()의 치트 블록과 함께 삭제할 것.
        private void CheatGoNight()
        {
            if (data.night == null) { ShowHint("gamedata에 night 블록이 없습니다", 2f, true); return; }
            lastPromoted = true; // 상점 언락 상태로 테스트 (레벨 2)
            StartNightPhase();
        }

        private void LateUpdate()
        {
            // 스케줄링 진행 상태 폴링 — 확정 버튼 활성화 + 진행 표시
            if (!schedulingActive || scheduling == null) return;
            if (schedConfirmBtn != null)
                schedConfirmBtn.interactable = scheduling.AllPlaced && !stamping;
            if (progressLabel != null)
                progressLabel.text = $"배치 {scheduling.PlacedCount} / {scheduling.Total}    ·    통화로 사정을 들어보세요";
        }

        // ─── 풀 뽑기 (mvp-design §10a 프로토 선적용, 2026-07-04) ───

        // 재시작해도 유지 — 이미 나온 케이스는 뒤 순위로 밀려 새 얼굴이 먼저 나온다 (셔플백)
        private readonly HashSet<string> seenIds = new HashSet<string>();

        /// applicantIds = 풀 전체. 매 판 drawCount건을 뽑는다 (0 = 뽑기 없이 명단 순서 그대로).
        /// firstId 케이스는 아직 안 봤을 때만 첫 슬롯 고정 (반전 튜토리얼 케이스 보장용).
        private List<Applicant> DrawLineup(List<Applicant> pool)
        {
            if (day.drawCount <= 0) return pool;
            int count = Mathf.Min(day.drawCount, pool.Count);

            int seed = System.Environment.TickCount; // 시드 로그 = 버그 재현용
            var rng = new System.Random(seed);
            Debug.Log($"[MawangHR] 서류 뽑기 — 풀 {pool.Count}건 중 {count}건, 시드 {seed}");

            var rest = new List<Applicant>(pool);
            var result = new List<Applicant>();

            var first = rest.FirstOrDefault(a => a.id == day.firstId);
            if (first != null && !seenIds.Contains(first.id))
            {
                result.Add(first);
                rest.Remove(first);
            }

            // 미출현 우선 (그룹 내 랜덤) → 필요 수만큼
            var picked = rest.Where(a => !seenIds.Contains(a.id)).OrderBy(_ => rng.Next())
                .Concat(rest.Where(a => seenIds.Contains(a.id)).OrderBy(_ => rng.Next()))
                .Take(count - result.Count)
                .ToList();

            // 뽑은 뒤 순서 셔플 — 새/헌 케이스가 섞여 나오게 (고정 첫 슬롯은 제외)
            for (int i = picked.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (picked[i], picked[j]) = (picked[j], picked[i]);
            }
            result.AddRange(picked);

            foreach (var a in result) seenIds.Add(a.id);
            if (pool.All(a => seenIds.Contains(a.id)))
            {
                // 풀 소진 → 가방 리셋 (방금 판만 기억해 직후 재등장은 방지)
                seenIds.Clear();
                foreach (var a in result) seenIds.Add(a.id);
            }
            return result;
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
            if (a.answers != null)
            {
                var ans = a.answers.FirstOrDefault(x => x.evidence == a.correct);
                if (ans != null) return ans.text; // 결정적 단서가 답변에 있는 경우 (면접)
            }
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

            var btn = UiKit.MakeButton(s, IsInterview ? "면접 시작" : "출근하기", UiKit.Accent, UiKit.Ink, 32,
                () => { if (IsInterview) StartInterviewPhase(); else StartScreeningPhase(); });
            UiKit.Place((RectTransform)btn.transform, 810, 990, 300, 68);

            // 개발용 지름길 — 승진 없이 면접 페이즈 바로 테스트
            if (dayIndex == 0 && data.days.Length > 1 && data.interviewees.Length > 0)
            {
                var dev = UiKit.MakeButton(s, "[개발] Day 2 면접 바로가기", UiKit.Panel, UiKit.TextDim, 18,
                    () => StartDay(1));
                UiKit.Place((RectTransform)dev.transform, 20, 1005, 260, 46);
            }
        }

        // ─── 3D 심사 페이즈 ─────────────────────────────

        private void StartScreeningPhase()
        {
            ClearScreen();
            screeningActive = true;
            holding = false;
            BuildHud("마왕성 인사팀 — " + day.title, DefaultHint, "근거 메모", MemoDefault);
            EnsureStamps();
            ShowApplicantOnDesk();
        }

        private void EnsureStamps()
        {
            if (!stampsCreated)
            {
                stamps.Add(rig.MakeStamp(true, "통 과", UiKit.Approve, new Vector3(0.40f, DeskRig.DeskTop, -0.36f),
                    CanStampNow, OnSlam, OnStampBlocked));
                stamps.Add(rig.MakeStamp(false, "탈 락", UiKit.Reject, new Vector3(0.58f, DeskRig.DeskTop, -0.36f),
                    CanStampNow, OnSlam, OnStampBlocked));
                stampsCreated = true;
            }
            foreach (var s2 in stamps) s2.SetTarget(rig.ResumeCanvas); // 이력서(면접 = 서류+진술)가 도장 대상
        }

        // ─── 면접 페이즈 (Day 2 — S2b) ─────────────────────────────

        private const string InterviewHint = "카드를 잡아 지원자에게 던지면 질문 · 답변 줄 좌/우클릭 = V/X · 근거 표시 후 도장 쾅";

        private void StartInterviewPhase()
        {
            ClearScreen();
            screeningActive = true;
            holding = false;
            BuildHud("마왕성 인사팀 — " + day.title, InterviewHint, "근거 메모", MemoDefault);
            BuildSpeechPanel();
            EnsureStamps();
            BuildQuestionCards();
            ShowApplicantOnDesk();
        }

        /// 지원자 대사 자막 패널 (HUD 상단 중앙) — 데스크 거리에서도 읽히는 크기
        private void BuildSpeechPanel()
        {
            var panel = UiKit.PanelRect(hud, "Speech", new Color(0.16f, 0.11f, 0.09f, 0.93f));
            UiKit.Place(panel, 460, 118, 1000, 170);
            panel.GetComponent<Image>().raycastTarget = false;
            speechPanel = panel.gameObject;
            speechText = UiKit.LabelAt(panel, "", 29, UiKit.Text, 30, 20, 940, 132, TextAlignmentOptions.TopLeft, true);
            speechText.raycastTarget = false;
            speechPanel.SetActive(false);
        }

        /// 책상 앞줄에 질문 카드 배치 (월드 캔버스 + 드래그 — 지원자에게 던지면 발동)
        private void BuildQuestionCards()
        {
            DestroyCards();
            cardUsed = new bool[data.cards.Length];
            for (int i = 0; i < data.cards.Length; i++)
            {
                var c = data.cards[i];
                var rt = UiKit.MakeWorldCanvas("Card_" + c.id, 320, 200, CardScale, rig.Cam);
                rt.SetParent(rig.transform, false);
                // 카메라 앞 하단 '손패' 포즈 — 책상 위 종이와 공간이 분리돼 겹치지 않고, 항상 세워져 읽힌다.
                // (화면 UI가 아니라 월드 카드라 잡아 던지는 물성은 그대로)
                rig.GetFrontPose(0.62f, -0.20f, -0.30f + i * 0.20f, (i - 1.5f) * 3f, out var pos, out var rot);
                rt.position = pos;
                rt.rotation = rot * Quaternion.Euler(0, 0, Random.Range(-2f, 2f));
                rt.GetComponent<Canvas>().sortingOrder = 2 + i; // 손패 좌→우로 살짝 겹치는 카드게임 느낌
                rt.gameObject.AddComponent<Image>().color = UiKit.Paper;

                string stars = new string('★', Mathf.Max(1, c.cost));
                UiKit.LabelAt(rt, $"<b>{c.label}</b> <color=#B03A2E>{stars}</color>", 42, UiKit.Ink,
                    18, 14, 288, 54, TextAlignmentOptions.TopLeft, true);
                UiKit.LabelAt(rt, "“" + c.prompt + "”", 28, UiKit.InkDim, 18, 74, 288, 112, TextAlignmentOptions.TopLeft, true);

                cardRts.Add(rt);
                cardHomes.Add(rt.position);
                cardHomeRots.Add(rt.rotation);

                int idx = i;
                rt.gameObject.AddComponent<PhotoDraggable>().Init(
                    rig.Cam,
                    () => screeningActive && IsInterview && !stamping && cardUsed != null && !cardUsed[idx],
                    () => ShowHint($"{c.label} ({stars}) — “{c.prompt}”  · 지원자에게 끌어다 던지세요", 2.2f),
                    dropPos => TryAskCard(idx));
            }
        }

        private void DestroyCards()
        {
            foreach (var rt in cardRts) if (rt != null) Destroy(rt.gameObject);
            cardRts.Clear();
            cardHomes.Clear();
            cardHomeRots.Clear();
            cardUsed = null;
        }

        /// 새 지원자 — 카드 전부 복귀 + 질문 포인트 리셋
        private void ResetCardsForApplicant()
        {
            pointsLeft = Mathf.Max(1, day.questionPoints + dayQpBonus); // 드링크 보너스 (어젯밤 구매분)
            if (cardUsed == null) return;
            for (int i = 0; i < cardRts.Count; i++)
            {
                cardUsed[i] = false;
                if (cardRts[i] == null) continue;
                cardRts[i].gameObject.SetActive(true);
                cardRts[i].localScale = Vector3.one * CardScale;
                cardRts[i].position = cardHomes[i];
                cardRts[i].rotation = cardHomeRots[i];
            }
        }

        private void ReturnCardHome(int i)
        {
            if (cardRts[i] == null) return;
            cardRts[i].position = cardHomes[i];
            cardRts[i].rotation = cardHomeRots[i];
        }

        /// 카드 드롭 — 지원자 근처에 놓으면 질문 발동, 아니면 제자리 복귀
        private void TryAskCard(int i)
        {
            if (!IsInterview || stamping || rig.MonsterHead == null) { ReturnCardHome(i); return; }
            var def = data.cards[i];
            Vector2 cardScreen = rig.Cam.WorldToScreenPoint(cardRts[i].position);
            Vector2 monScreen = rig.Cam.WorldToScreenPoint(rig.MonsterHead.position);
            if (Vector2.Distance(cardScreen, monScreen) > Screen.height * 0.30f)
            {
                ReturnCardHome(i);
                return;
            }
            if (def.cost > pointsLeft)
            {
                ShowHint($"질문 포인트 부족! (남은 ★{pointsLeft} · 필요 {new string('★', def.cost)})", 1.6f, true);
                ReturnCardHome(i);
                return;
            }
            pointsLeft -= def.cost;
            cardUsed[i] = true;
            UpdateHudProgress();
            StartCoroutine(AskRoutine(i));
        }

        private IEnumerator AskRoutine(int i)
        {
            var def = data.cards[i];
            var a = lineup[index];
            var rt = cardRts[i];

            // 카드가 지원자에게 날아가 마법으로 흡수
            Sfx.Swish();
            Vector3 from = rt.position;
            Vector3 to = rig.MonsterHead != null ? rig.MonsterHead.position + Vector3.down * 0.15f : from;
            float t = 0f;
            const float fly = 0.22f;
            while (t < fly)
            {
                t += Time.deltaTime;
                if (rt == null) yield break;
                float k = Mathf.SmoothStep(0f, 1f, t / fly);
                rt.position = Vector3.Lerp(from, to, k);
                rt.localScale = Vector3.one * CardScale * (1f - 0.5f * k);
                yield return null;
            }
            if (rt != null)
            {
                rig.MagicBurst(rt.position);
                rt.localScale = Vector3.one * CardScale;
                rt.gameObject.SetActive(false);
            }

            // 질문 → (텀) → 답변 + 반응 지시문
            ShowMonsterBubble($"<color=#D98F3E><b>Q.</b></color> {def.prompt}", 30f);
            yield return new WaitForSeconds(0.8f);
            if (!IsInterview || index >= lineup.Count || lineup[index] != a) yield break; // 지원자가 바뀌었으면 중단

            Answer ans = null;
            foreach (var x in a.answers) if (x.cardId == def.id) { ans = x; break; }
            if (ans == null) yield break; // 검증기가 막지만 방어

            string bubble = $"“{ans.text}”";
            if (!string.IsNullOrEmpty(ans.tell))
            {
                bubble += $"\n<size=25><color=#A6947C><i>{ans.tell}</i></color></size>";
                rig.MonsterReact(true); // 반응 연출 = 긴장 움찔
            }
            ShowMonsterBubble(bubble, 6.5f);
            AddStatementLine(def, ans);
        }

        /// 답변을 이력서의 '진술 기록'에 단서 줄로 추가 (마킹 가능 — 답변도 단서다)
        private void AddStatementLine(QuestionCard def, Answer ans)
        {
            if (resumeContent == null || stamping) return;
            string text = $"<color=#6B5A42>[{def.label}]</color> “{ans.text}”";
            if (!string.IsNullOrEmpty(ans.tell))
                text += $"\n<size=19><color=#6B5A42><i>{ans.tell}</i></color></size>";
            AddClueLine(resumeContent, text, ans.evidence, UiKit.Ink, stmtX, stmtY, stmtW, stmtH);
            stmtY += stmtStep;
        }

        // ─ 지원자 대사 자막 (HUD) ─

        private void ShowMonsterBubble(string text, float duration)
        {
            if (speechPanel == null) return; // 면접 HUD가 없는 상태(전환 중)면 무시
            speechPanel.SetActive(true);
            speechText.text = text;
            if (speechCo != null) StopCoroutine(speechCo);
            speechCo = StartCoroutine(HideSpeechAfter(duration));
        }

        private IEnumerator HideSpeechAfter(float t)
        {
            yield return new WaitForSeconds(t);
            if (speechPanel != null) speechPanel.SetActive(false);
            speechCo = null;
        }

        private void HideMonsterBubble()
        {
            if (speechCo != null) { StopCoroutine(speechCo); speechCo = null; }
            if (speechPanel != null) speechPanel.SetActive(false);
        }

        private bool CanStampNow()
        {
            if (stamping || schedulingActive) return false; // 스케줄링 확정은 버튼으로
            return screeningActive && marks.Count > 0;
        }

        private void OnStampBlocked()
        {
            if (stamping || schedulingActive) return;
            if (!screeningActive || marks.Count > 0) return;
            ShowHint("근거를 최소 1개 표시해야 도장을 찍을 수 있습니다!", 1.4f, true);
        }

        /// 힌트 라벨에 잠깐 메시지 표시 후 기본 안내로 복귀
        private void ShowHint(string msg, float duration, bool warn = false)
        {
            if (hintLabel == null) return;
            if (warnCo != null) StopCoroutine(warnCo);
            warnCo = StartCoroutine(HintRoutine(msg, duration, warn));
        }

        private IEnumerator HintRoutine(string msg, float duration, bool warn)
        {
            hintLabel.text = msg;
            hintLabel.color = warn ? new Color(1f, 0.45f, 0.38f) : UiKit.Text;
            yield return new WaitForSeconds(duration);
            if (hintLabel == null) yield break;
            hintLabel.text = currentDefaultHint;
            hintLabel.color = UiKit.TextDim;
        }

        private void BuildHud(string title, string hint, string padTitle, string padDefault)
        {
            DestroyHud();
            currentDefaultHint = hint;
            hud = UiKit.Rect("HUD", canvas.transform);
            UiKit.Fill(hud);

            var top = UiKit.PanelRect(hud, "TopBar", UiKit.Panel);
            UiKit.Place(top, 0, 0, 1920, 56);
            UiKit.LabelAt(top, title, 24, UiKit.Accent, 25, 13, 800, 36, TextAlignmentOptions.TopLeft, true);
            progressLabel = UiKit.LabelAt(top, "", 22, UiKit.TextDim, 900, 15, 995, 36, TextAlignmentOptions.TopRight, true);

            hintLabel = UiKit.LabelAt(hud, hint, 22, UiKit.TextDim, 0, 66, 1920, 36, TextAlignmentOptions.Center, true);

            // padTitle이 비어 있으면 메모 패드 생략 (스케줄링은 책상 위 통화 기록 노트가 대신함)
            if (!string.IsNullOrEmpty(padTitle))
            {
                var memo = UiKit.PanelRect(hud, "MemoPad", new Color(0.23f, 0.17f, 0.13f, 0.92f));
                UiKit.Place(memo, 1540, 120, 360, 330);
                UiKit.LabelAt(memo, "<b>" + padTitle + "</b>", 22, UiKit.Accent, 20, 12, 320, 30);
                memoContent = UiKit.LabelAt(memo, padDefault, 19, UiKit.Text,
                    20, 48, 320, 268, TextAlignmentOptions.TopLeft, true);
            }
            else memoContent = null;

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
            if (progressLabel == null) return;
            int passCount = verdicts.Count(v => v);
            if (IsInterview)
            {
                string stars = pointsLeft > 0 ? new string('★', pointsLeft) : "소진";
                progressLabel.text = $"면접 {index + 1} / {lineup.Count}    ·    질문 포인트 {stars}    ·    합격 {passCount}명";
            }
            else
                progressLabel.text = $"서류 {index + 1} / {lineup.Count}    ·    통과 {passCount}명";
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

            // 면접 손패: 종이를 든 동안엔 숨김 — 들린 이력서 하단(진술 기록 마킹 영역)을 가리지 않게.
            // 읽을 땐 마킹, 내려놓으면 카드가 다시 올라오는 리듬.
            if (IsInterview && cardUsed != null)
                for (int i = 0; i < cardRts.Count; i++)
                    if (cardRts[i] != null) cardRts[i].gameObject.SetActive(!holding && !cardUsed[i]);
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
            if (IsInterview)
            {
                ResetCardsForApplicant();
                rig.SpawnMonster(SchedulingFlow.SpeciesColor(a.species));
                ShowMonsterBubble($"<b>{a.name}</b>  <color=#A6947C>({a.species})</color>\n“{a.quote}”", 4.5f);
            }
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

        /// 이력서 내용 — 좌표는 전부 resume_template.layout.json(템플릿 픽셀 명세) × 배율.
        /// 템플릿 구역: 상단 타이틀 / 좌측 초상화 액자 / 우측 정보 4행(점선) / 인용구 상자 /
        /// 이력 배너+3줄 상자 / 특이사항(면접 = 진술 기록) 배너+상자 / 하단 상자 = 도장 자리.
        private void BuildResumeContent(Applicant a)
        {
            if (resumeContent != null) Destroy(resumeContent.gameObject);
            resumeContent = UiKit.Rect("Content", rig.ResumeCanvas);
            UiKit.Fill(resumeContent);
            var jd = FindJd(a.jdId);

            var L = resumeLayout;
            float S = rig.ResumeCanvas.sizeDelta.x / Mathf.Max(1f, L.templateW); // PNG px → 캔버스 px 배율
            void PlaceR(RectTransform rt, LayoutRect r) => UiKit.Place(rt, r.x * S, r.y * S, r.w * S, r.h * S);

            // 상단 타이틀 (중앙)
            var title = UiKit.Label(resumeContent, $"<b>지원자 서류</b>  ·  MHR-{day.day:00}-{index + 1:000}",
                36, UiKit.Ink, TextAlignmentOptions.Center, true);
            PlaceR((RectTransform)title.transform, L.title);

            // 초상화 (액자 안) — 규약: StreamingAssets/MawangHR/Portraits/{id}.png, 없으면 종족색
            var portrait = UiKit.PanelRect(resumeContent, "Portrait", SchedulingFlow.SpeciesColor(a.species));
            PlaceR(portrait, L.portrait);
            var portraitImg = portrait.GetComponent<Image>();
            portraitImg.raycastTarget = false;
            var pSprite = UiKit.LoadSprite("MawangHR/Portraits/" + a.id + ".png");
            if (pSprite != null) { portraitImg.sprite = pSprite; portraitImg.color = Color.white; }

            // 정보 4행 — 점선(lineY) 위에 라벨/값이 앉게
            void InfoRow(string label, string value, float lineY)
            {
                UiKit.LabelAt(resumeContent, label, 20, UiKit.InkDim,
                    L.infoLabelX * S, lineY * S - 36, (L.infoValueX - L.infoLabelX) * S, 34, TextAlignmentOptions.BottomLeft);
                UiKit.LabelAt(resumeContent, value, 28, UiKit.Ink,
                    L.infoValueX * S, lineY * S - 42, (L.infoRightX - L.infoValueX) * S, 40, TextAlignmentOptions.BottomLeft, true);
            }
            InfoRow("이름:", "<b>" + a.name + "</b>", L.infoRowYs[0]);
            InfoRow("종족:", a.species, L.infoRowYs[1]);
            InfoRow("지원 직무:", jd != null ? jd.title : a.jdId, L.infoRowYs[2]);
            InfoRow("희망 연봉:", a.salary, L.infoRowYs[3]);

            // 인용구 상자 (테두리 안쪽으로 인셋)
            var quote = UiKit.Label(resumeContent, "“" + a.quote + "”", 21, UiKit.Ink, TextAlignmentOptions.Left, true);
            UiKit.Place((RectTransform)quote.transform,
                (L.quote.x + 26) * S, (L.quote.y + 6) * S, (L.quote.w - 52) * S, (L.quote.h - 12) * S);

            // 배너 1 (그림은 템플릿에 — 글자만 얹는다)
            var b1 = UiKit.Label(resumeContent, IsInterview ? "<b>서류 요약</b>" : "<b>이력</b>",
                26, UiKit.Text, TextAlignmentOptions.Center);
            PlaceR((RectTransform)b1.transform, L.banner1);
            UiKit.LabelAt(resumeContent,
                IsInterview ? "(인턴 작성 · 줄 클릭 = V/X)" : "(줄을 클릭해 판정 근거를 표시)",
                18, UiKit.InkDim, L.hintX * S, (L.banner1.y + 12) * S, 400, 30);

            // 이력 줄 — 상자의 점선 위에 한 줄씩
            for (int i = 0; i < a.resumeLines.Length && i < L.clueLineYs.Length; i++)
                AddClueLine(resumeContent, "· " + a.resumeLines[i].text, a.resumeLines[i].evidence, UiKit.Ink,
                    L.rowX * S, (L.clueLineYs[i] - L.rowH - 2) * S, L.rowW * S, L.rowH * S);

            // 배너 2 + 하단 내용
            var b2 = UiKit.Label(resumeContent, IsInterview ? "<b>진술 기록</b>" : "<b>특이사항</b>",
                26, UiKit.Text, TextAlignmentOptions.Center);
            PlaceR((RectTransform)b2.transform, L.banner2);
            UiKit.LabelAt(resumeContent,
                IsInterview ? "(카드를 던져 질문 — 답변도 단서다)" : "(인사팀 메모)",
                18, UiKit.InkDim, L.hintX * S, (L.banner2.y + 12) * S, 400, 30);

            stmtX = L.rowX * S; stmtW = L.rowW * S; stmtStep = L.stmtStep * S; stmtH = L.stmtRowH * S;
            if (IsInterview)
            {
                stmtY = L.stmtStartY * S;
                if (!string.IsNullOrEmpty(a.special))
                {
                    AddClueLine(resumeContent, a.special, a.specialEvidence, UiKit.StampInk, stmtX, stmtY, stmtW, stmtH * 0.8f);
                    stmtY += stmtStep;
                }
            }
            else
            {
                AddClueLine(resumeContent, a.special, a.specialEvidence, UiKit.StampInk,
                    L.special.x * S, L.special.y * S, L.special.w * S, L.special.h * S);
            }
            // 하단 상자 = 도장 찍는 자리
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

            // 마킹 잉크 스트로크 — 글자와 분리된 레이어 (마킹 시 이것만 칠해지고 기울어짐, 글자는 고정)
            var fx = UiKit.Rect("MarkFx", bg);
            fx.anchorMin = Vector2.zero;
            fx.anchorMax = new Vector2(1, 0);
            fx.pivot = new Vector2(0.5f, 0);
            fx.anchoredPosition = new Vector2(0, 2);
            fx.sizeDelta = new Vector2(-12, Mathf.Min(46f, h * 0.8f));
            var fxImg = fx.gameObject.AddComponent<Image>();
            fxImg.color = Color.clear;
            fxImg.raycastTarget = false;
            clueBgs.Add(fxImg);

            bg.gameObject.AddComponent<ClueLine>().Init(clueIndex, ToggleMark);

            // 왼쪽 여백의 V/X 글리프 (깃펜 자국) — 줄(하단)에 맞춰 배치
            var glyph = UiKit.Label(bg, "", 34, UiKit.Ink, TextAlignmentOptions.Center);
            var grt = (RectTransform)glyph.transform;
            UiKit.Place(grt, -38, h - 44, 36, 46);
            grt.localRotation = Quaternion.Euler(0, 0, -6f);
            glyph.fontStyle = FontStyles.Bold;
            glyph.raycastTarget = false;
            clueGlyphs.Add(glyph);

            // 글자는 하단 정렬 — 종이의 점선 위에 '앉게' (상단 정렬이면 반 칸 떠 보인다)
            var label = UiKit.Label(bg, text, 25, inkColor, TextAlignmentOptions.BottomLeft, true);
            var lrt = (RectTransform)label.transform;
            UiKit.Fill(lrt);
            lrt.offsetMin = new Vector2(8, 4);
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
            var fx = clueBgs[clueIndex]; // 잉크 스트로크 레이어만 칠하고 기울인다 — 글자는 움직이지 않음
            fx.color = !marked ? Color.clear : (pol ? MarkPosBg : MarkNegBg);
            fx.transform.localRotation = marked
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

        // ─── 도장 낙하 ───

        private void OnSlam(bool pass, Vector2 screenPos)
        {
            if (stamping || schedulingActive) return;
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
            SpawnImprint(rig.ResumeCanvas, resumeContent, screenPos,
                pass ? "통  과" : "탈  락", pass ? UiKit.Approve : UiKit.StampInk);

            yield return new WaitForSeconds(0.8f);

            if (IsInterview)
            {
                HideMonsterBubble();
                // 퇴장을 끝까지 본 뒤에 다음으로 — 병렬로 돌리면 퇴장 코루틴이 다음 몬스터를 소멸시키는 버그 + 반응 연출이 잘림
                yield return StartCoroutine(rig.MonsterExit(pass));
            }

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
            else if (IsInterview)
            {
                // 면접 끝 → 바로 퇴근 결산 (스케줄링은 서류 심사 날 전용)
                // 몬스터는 MonsterExit 코루틴이 마법 포프로 정리 중 — 여기서 즉시 파괴하지 않는다
                screeningActive = false;
                DestroyCards();
                HideMonsterBubble();
                DestroyHud();
                yield return new WaitForSeconds(0.4f);
                revealIndex = 0;
                ShowReveal();
            }
            else
            {
                // 심사 끝 → 오후 업무 (면접 일정 잡기)
                screeningActive = false;
                DestroyHud();
                yield return new WaitForSeconds(0.4f);
                ShowSchedulingIntro();
            }
        }

        /// 도장 잉크 자국 — 스크린 광선 ↔ 종이 면 교차점에 찍기.
        /// 자체 정렬 캔버스를 부여해 사진 카드 등 다른 월드 캔버스보다 항상 위에 찍힌다.
        private void SpawnImprint(RectTransform refCanvas, Transform parent, Vector2 screenPos, string text, Color color)
        {
            Vector2 lp;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(refCanvas, screenPos, rig.Cam, out lp);
            var imprint = UiKit.Rect("Imprint", parent);
            var sortCanvas = imprint.gameObject.AddComponent<Canvas>();
            sortCanvas.overrideSorting = true;
            sortCanvas.sortingOrder = 10;
            imprint.anchorMin = imprint.anchorMax = new Vector2(0.5f, 0.5f);
            imprint.pivot = new Vector2(0.5f, 0.5f);
            imprint.anchoredPosition = lp;
            imprint.sizeDelta = new Vector2(430, 170);
            imprint.localRotation = Quaternion.Euler(0, 0, Random.Range(-14f, -4f));

            var frame = UiKit.PanelRect(imprint, "Frame", new Color(0, 0, 0, 0));
            UiKit.Fill(frame);
            var outline = frame.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(5, 5);
            frame.GetComponent<Image>().raycastTarget = false;

            var label = UiKit.Label(imprint, text, 100, color, TextAlignmentOptions.Center);
            UiKit.Fill((RectTransform)label.transform);
            label.raycastTarget = false;
        }

        // ─── 스케줄링 페이즈 (오후 업무 — 면접 일정 잡기) ─────────────────────────────

        private void ShowSchedulingIntro()
        {
            // 스케줄링 데이터 없으면 바로 퇴근 결산으로
            if (data.scheduling == null || data.scheduling.candidates == null || data.scheduling.candidates.Length == 0)
            {
                revealIndex = 0;
                ShowReveal();
                return;
            }

            var s = NewScreen();

            UiKit.LabelAt(s, "오후 업무 — 면접 일정 잡기", 52, UiKit.Accent, 0, 130, 1920, 80, TextAlignmentOptions.Center);

            var paper = UiKit.PanelRect(s, "IntroPaper", UiKit.Paper);
            UiKit.Place(paper, 460, 260, 1000, 440);
            UiKit.LabelAt(paper, data.scheduling != null ? data.scheduling.intro : "(scheduling 데이터 없음)",
                27, UiKit.Ink, 50, 40, 900, 360, TextAlignmentOptions.TopLeft, true);

            var btn = UiKit.MakeButton(s, "연락 시작", UiKit.Accent, UiKit.Ink, 32, StartSchedulingPhase);
            UiKit.Place((RectTransform)btn.transform, 810, 760, 300, 70);
        }

        private void StartSchedulingPhase()
        {
            ClearScreen();
            schedulingActive = true;
            stamping = false; // 마지막 서류의 도장 연출에서 넘어온 플래그 해제 (확정 버튼 조건)
            // 통화 기록은 HUD 메모 대신 수정구 옆 책상 노트가 담당 (물성)
            BuildHud("마왕성 인사팀 — 면접 일정 잡기",
                "사진 클릭 = 확대 · 수정구에 끌어다 대면 통화 · 일정표에 배치 — 전원 배치하면 확정 버튼이 켜집니다",
                "", "");

            // 책상 종이는 치우기 (수정구 가림 방지 — 이 업무엔 지침서 불필요)
            rig.ResumeCanvas.gameObject.SetActive(false);
            rig.JdCanvas.gameObject.SetActive(false);
            if (rig.OrbProp != null) rig.OrbProp.enabled = false; // 통화 장비를 던지는 사고 방지

            rig.TweenToHold();
            scheduling = new GameObject("SchedulingFlow").AddComponent<SchedulingFlow>();
            // 매판 뽑기 — 풀에서 환경·지원자를 뽑되 솔버가 해 존재를 보장한 시나리오만 출제
            scheduling.Begin(SchedulingFlow.Roll(data.scheduling), rig, (msg, dur) => ShowHint(msg, dur));
            foreach (var s2 in stamps) s2.SetTarget(null); // 스케줄링 중 도장은 휴식

            // 확정 버튼 — 전원 배치 전까지 비활성 (LateUpdate에서 폴링)
            schedConfirmBtn = UiKit.MakeButton(hud, "일정 확정", UiKit.Accent, UiKit.Ink, 28, OnSchedConfirmClicked);
            UiKit.Place((RectTransform)schedConfirmBtn.transform, 1580, 980, 320, 72);
            schedConfirmBtn.interactable = false;
        }

        private void OnSchedConfirmClicked()
        {
            if (stamping || scheduling == null || !scheduling.AllPlaced) return;
            stamping = true;
            StartCoroutine(SchedConfirmRoutine());
        }

        private IEnumerator SchedConfirmRoutine()
        {
            scheduling.Lock();
            yield return new WaitForSeconds(0.06f);

            Sfx.Thunk();
            StartCoroutine(rig.CamShake());
            Vector2 centerScreen = rig.Cam.WorldToScreenPoint(scheduling.CalendarCanvas.position);
            SpawnImprint(scheduling.CalendarCanvas, scheduling.CalendarCanvas, centerScreen, "확  정", UiKit.Approve);

            schedulingViolations = scheduling.GetViolations();
            yield return new WaitForSeconds(1.0f);

            schedulingActive = false;
            stamping = false;
            foreach (var s2 in stamps) s2.SetTarget(rig.ResumeCanvas);
            scheduling.Cleanup();
            scheduling = null;
            rig.ResumeCanvas.gameObject.SetActive(true); // 치웠던 종이 복귀
            rig.JdCanvas.gameObject.SetActive(true);
            if (rig.OrbProp != null) rig.OrbProp.enabled = true;
            rig.TweenToDesk();
            DestroyHud();
            yield return new WaitForSeconds(0.4f);

            revealIndex = 0;
            ShowReveal();
        }

        // ─── 밤 파트 (내 방 — 정산·석간·까마귀 상점, S3a) ─────────────────────────────

        private void StartNightPhase()
        {
            ClearScreen();
            DestroyHud();
            schedulingActive = false;
            if (scheduling != null) { scheduling.Cleanup(); scheduling = null; }
            if (night != null) { night.Cleanup(); night = null; } // 치트로 재진입해도 안전
            HideMonsterBubble();
            rig.DespawnMonster();
            DestroyCards();
            nightActive = true;

            if (nightRoom == null)
            {
                nightRoom = new GameObject("NightRoom").AddComponent<NightRoom>();
                nightRoom.Build(rig.Cam);
            }
            nightRoom.EnterCamera();

            int level = lastPromoted ? 2 : 1;
            var cfg = data.night;

            // 오늘 낮 성적 → 급여 명세 (톤 원칙: 청구서도 명세서 위의 코미디 한 줄)
            int correct = 0;
            for (int i = 0; i < verdicts.Count && i < lineup.Count; i++)
                if (verdicts[i] == lineup[i].CorrectIsPass) correct++;
            int hits = evidenceHits.Count(h => h);
            int vio = schedulingViolations.Count;
            int wage = Mathf.Max(0, cfg.basePay + correct * cfg.payPerCorrect + hits * cfg.payPerHit
                - vio * cfg.finePerViolation);

            var payLines = new List<string> { $"기본급   +{cfg.basePay}G" };
            if (correct > 0) payLines.Add($"판정 정확 {correct}건   +{correct * cfg.payPerCorrect}G");
            if (hits > 0) payLines.Add($"근거 적중 {hits}건   +{hits * cfg.payPerHit}G");
            if (vio > 0) payLines.Add($"<color=#B03A2E>사고 청구서 {vio}건   −{vio * cfg.finePerViolation}G</color>");
            payLines.Add("");
            payLines.Add($"<b>실수령   {wage}G</b>");
            if (wage == 0) payLines.Add("<size=19><color=#6B5A42>경리부: “힘내라는 뜻으로 봉투는 드립니다”</color></size>");

            BuildHud("마왕성 기숙사 — 내 방", "신문·월급봉투 클릭 = 확인 · 침대 클릭 = 취침", "", "");
            UpdateNightGold();

            night = new GameObject("NightFlow").AddComponent<NightFlow>();
            night.Begin(nightRoom, cfg, level, BuildNightArticles(), payLines, wage, hud,
                () => gold,
                delta => { gold += delta; UpdateNightGold(); },
                NightBuyBlockReason, TryBuyNight,
                (msg, dur) => ShowHint(msg, dur), OnNightSleep);

            if (level >= 2 && !crowIntroShown)
            {
                crowIntroShown = true;
                ShowHint("승진의 밤 — 창가에 <b>까마귀 상점</b>이 찾아왔습니다", 3.5f);
            }
        }

        /// 석간 기사 조립 — 오판(최대 3) + 스케줄 위반(최대 2) + 무사고 미담 + 플레이버 1
        private List<string> BuildNightArticles()
        {
            var arts = new List<string>();
            int wrong = 0;
            for (int i = 0; i < verdicts.Count && i < lineup.Count && wrong < 3; i++)
            {
                var a = lineup[i];
                if (verdicts[i] == a.CorrectIsPass) continue;
                wrong++;
                arts.Add(verdicts[i]
                    ? $"<b>「사고」</b> 신입 {a.name}({a.species}), 벌써부터 사고 조짐 — “뽑은 사람 누구냐” 인사팀 책임론"
                    : $"<b>「이직」</b> 본성이 떨어뜨린 {a.name}, 경쟁 던전 ‘옆동네 지하실’ 입사… 그쪽 사장은 함박웃음");
            }
            for (int i = 0; i < schedulingViolations.Count && i < 2; i++)
                arts.Add("<b>「사건」</b> " + schedulingViolations[i]);
            if (wrong == 0 && schedulingViolations.Count == 0)
                arts.Insert(0, "<b>「미담」</b> 오늘 인사팀 무사고 — 팀장 “신입치고 수상할 정도로 완벽하다”");
            if (data.night.flavorArticles.Length > 0)
                arts.Add("<b>「성내」</b> " + data.night.flavorArticles[UnityEngine.Random.Range(0, data.night.flavorArticles.Length)]);
            return arts;
        }

        /// 구매 불가 사유 (null = 구매 가능) — 상점 UI 표기와 실제 구매 검사가 같은 규칙을 쓴다
        private string NightBuyBlockReason(NightShopItem item)
        {
            if (item.effect == "deco" && ownedDeco.Contains(item.id)) return "진열됨";
            if (item.effect == "qp1" && nightQpBuff) return "내일 치 충전 완료";
            if (gold < item.price) return "골드 부족";
            return null;
        }

        private string TryBuyNight(NightShopItem item)
        {
            string block = NightBuyBlockReason(item);
            if (block != null) return block;
            gold -= item.price;
            UpdateNightGold();
            switch (item.effect)
            {
                case "qp1": nightQpBuff = true; break;
                case "deco": ownedDeco.Add(item.id); nightRoom.AddShelfDeco(item.id); break;
                case "pet": nightRoom.FeedSlime(); break;
            }
            Sfx.Shimmer();
            return null;
        }

        private void UpdateNightGold()
        {
            if (nightActive && progressLabel != null)
                progressLabel.text = $"골드   <b>{gold} G</b>";
        }

        private void OnNightSleep()
        {
            nightActive = false;
            if (night != null) { night.Cleanup(); night = null; }
            DestroyHud();
            rig.TweenToDesk(); // 카메라 복귀 (다음 화면이 2D 전면이라 비행은 안 보임)
            if (IsInterview) { ShowDemoEnd(); return; }
            bool canInterview = lastPromoted && data.days.Length > 1 && data.interviewees.Length > 0;
            StartDay(canInterview ? 1 : 0);
        }

        /// 다음날 아침 — 스케줄 위반의 대가 (지연 사고 시스템의 미리보기)
        /// ※ night 블록이 있으면 쓰이지 않음 (사고 보고서 = 석간 신문으로 흡수) — 폴백 전용
        private void ShowMorningReport()
        {
            var s = NewScreen();

            UiKit.LabelAt(s, "다음날 아침 — 사건 보고서", 48, UiKit.Accent, 0, 100, 1920, 70, TextAlignmentOptions.Center);

            var card = UiKit.PanelRect(s, "Report", UiKit.Panel);
            UiKit.Place(card, 410, 210, 1100, 580);

            string body = schedulingViolations.Count == 0
                ? "보고할 사고 없음.\n\n인사팀장: “일정에서 사고가 하나도 없다니…\n신입치고는 수상할 정도로 완벽하군.”"
                : string.Join("\n\n", schedulingViolations)
                  + "\n\n<color=#A6947C>인사팀장: “…내일부터는 통화 내용을 좀 듣고 일정을 잡게.”</color>";

            UiKit.LabelAt(card, body, 27, UiKit.Text, 60, 50, 980, 470, TextAlignmentOptions.TopLeft, true);

            // 승진했으면 면접으로 출근, 미달이면 새 서류로 재도전 (풀 뽑기가 새 얼굴 보장)
            bool canInterview = lastPromoted && data.days.Length > 1 && data.interviewees.Length > 0;
            var btn = UiKit.MakeButton(s, canInterview ? "Day 2 출근 — 1차 면접" : "Day 1 재도전 (새 서류)",
                UiKit.Accent, UiKit.Ink, 28, () => StartDay(canInterview ? 1 : 0));
            UiKit.Place((RectTransform)btn.transform, 785, 840, 350, 70);
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
            // 공적 = 승진 게이지 (보상 경제 #13 — 표시·판정만, 저장·누적은 S3).
            // 원점수(정확 ×2 + 근거 적중 ×1)를 meritGoal 기준 100점으로 환산 — 게이지 100 도달 = 승진.
            int merit = correctCount * 2 + hitCount;
            int meritScaled = Mathf.Min(100, Mathf.RoundToInt(merit * 100f / Mathf.Max(1, day.meritGoal)));
            bool promoted = meritScaled >= 100;
            lastPromoted = promoted; // 다음날 분기 (승진 → 면접 / 미달 → 재도전)

            UiKit.LabelAt(s, $"Day {day.day} 종료 — 인사 평가", 46, UiKit.Accent, 0, 80, 1920, 70, TextAlignmentOptions.Center);

            var card = UiKit.PanelRect(s, "Summary", UiKit.Panel);
            UiKit.Place(card, 510, 190, 900, 620);

            UiKit.LabelAt(card,
                $"판정 정확도:  <b>{correctCount} / {lineup.Count}</b>\n근거 적중 (방향 포함):  <b>{hitCount} / {lineup.Count}</b>\n서류 통과: {passCount}명",
                30, UiKit.Text, 60, 50, 780, 150, TextAlignmentOptions.TopLeft, true);

            // ─ 공적 게이지: 0→100 차오르고, 중앙 숫자가 같이 뛰고, 만땅 순간 승진 ─
            UiKit.LabelAt(card, "<b>인사기록 카드 — 공적</b>", 24, UiKit.Accent, 60, 214, 420, 34);
            UiKit.LabelAt(card, "정확 ×2 + 근거 적중 ×1 → 100점 환산", 19, UiKit.TextDim, 480, 220, 360, 30, TextAlignmentOptions.TopRight, true);

            var track = UiKit.PanelRect(card, "MeritTrack", new Color(0.12f, 0.09f, 0.07f));
            UiKit.Place(track, 60, 256, 780, 46);
            var fill = UiKit.PanelRect(track, "MeritFill", UiKit.Accent);
            UiKit.Place(fill, 0, 0, 0, 46);
            fill.GetComponent<Image>().raycastTarget = false;
            var counter = UiKit.Label(track, "0 / 100", 27, UiKit.Text, TextAlignmentOptions.Center);
            UiKit.Fill((RectTransform)counter.transform);
            counter.fontStyle = FontStyles.Bold;
            counter.raycastTarget = false;

            var resultTag = UiKit.LabelAt(card, promoted ? "승진 심사 통과!" : "공적 미달 — 수습 연장",
                26, promoted ? UiKit.Approve : UiKit.Reject, 60, 312, 780, 36, TextAlignmentOptions.Center);
            resultTag.fontStyle = FontStyles.Bold;
            resultTag.gameObject.SetActive(false);

            string verdictText;
            if (IsInterview)
                verdictText = promoted
                    ? "<color=#3E7D4E><b>2차 면접관 승진 예고!</b></color>\n\n“면접 보는 눈이 제법이야. 위층에서 자넬 찾더군.”\n\n<size=22><color=#A6947C>— S2b 데모는 여기까지. 3일 구조·가젯은 S3에서 —</color></size>"
                    : "<color=#9E3B32><b>1차 면접관 유지…</b></color>\n\n“질문은 던졌는데 답을 못 들었군.\n내일은 카드를 좀 더 아프게 골라보게.”";
            else
                verdictText = promoted
                    ? "<color=#3E7D4E><b>승진 예고!</b></color>\n\n“제법이군. 내일부터 자네가 1차 면접을 맡게.\n서류는 이제 스켈레톤 인턴이 볼 걸세.”"
                    : "<color=#9E3B32><b>수습 연장의 위기…</b></color>\n\n“서류 보는 눈이 아직 멀었군.\n내일 다시 해보게. 이번엔 공문을 '읽고' 찍으라고.”";
            var verdictLabel = UiKit.LabelAt(card, verdictText, 28, UiKit.Text, 60, 358, 780, 230, TextAlignmentOptions.Top, true);
            verdictLabel.gameObject.SetActive(false);

            StartCoroutine(MeritGaugeRoutine(fill, counter, resultTag.gameObject, verdictLabel.gameObject, meritScaled, promoted));

            // 밤 파트가 있으면 퇴근 → 내 방 (사고 보고서는 석간 신문으로 흡수), 없으면 기존 흐름
            var btn = UiKit.MakeButton(s,
                data.night != null ? "퇴근 — 내 방으로 →" : (IsInterview ? "데모 마무리 →" : "다음날 아침 →"),
                UiKit.Accent, UiKit.Ink, 28,
                () =>
                {
                    if (data.night != null) StartNightPhase();
                    else if (IsInterview) ShowDemoEnd();
                    else ShowMorningReport();
                });
            UiKit.Place((RectTransform)btn.transform, 810, 860, 300, 70);
        }

        /// S2b 데모 엔딩 — 하루 업무 전체(서류→일정→면접)가 열린 상태
        private void ShowDemoEnd()
        {
            var s = NewScreen();
            UiKit.LabelAt(s, "— 여기까지가 S2b 데모입니다 —", 46, UiKit.Accent, 0, 320, 1920, 70, TextAlignmentOptions.Center);
            UiKit.LabelAt(s,
                "서류 심사 → 면접 일정 잡기 → 1차 면접까지, 인사팀의 하루가 전부 열렸습니다.\n3일 구조 · 승진 가젯(촛불/돋보기) · Day 3 이중반전은 S3에서 이어집니다.",
                26, UiKit.Text, 0, 420, 1920, 100, TextAlignmentOptions.Center, true);
            var btn = UiKit.MakeButton(s, "처음부터 다시 (Day 1)", UiKit.Accent, UiKit.Ink, 28, () => StartDay(0));
            UiKit.Place((RectTransform)btn.transform, 785, 580, 350, 70);
        }

        /// 공적 게이지 연출 — 바가 0부터 차오르고 중앙 카운터가 함께 뛴다.
        /// 100 도달(승진)이면 번쩍+샤라랑, 미달이면 멈춤 → 결과 태그·총평 순차 공개.
        private IEnumerator MeritGaugeRoutine(RectTransform fill, TextMeshProUGUI counter,
            GameObject resultTag, GameObject verdictObj, int target, bool promoted)
        {
            yield return new WaitForSeconds(0.35f);
            Sfx.Swish();
            const float trackW = 780f;
            float dur = Mathf.Lerp(0.7f, 1.6f, target / 100f); // 많이 찰수록 오래 — 빌드업
            float t = 0f;
            int shown = -1;
            while (t < dur)
            {
                t += Time.deltaTime;
                if (fill == null || counter == null) yield break; // 화면이 넘어갔으면 중단
                float k = Mathf.SmoothStep(0f, 1f, t / dur);
                fill.sizeDelta = new Vector2(trackW * target * k / 100f, 46f);
                int cur = Mathf.RoundToInt(target * k);
                if (cur != shown) { shown = cur; counter.text = cur + " / 100"; }
                yield return null;
            }
            if (fill == null || counter == null) yield break;
            fill.sizeDelta = new Vector2(trackW * target / 100f, 46f);
            counter.text = target + " / 100";

            if (promoted)
            {
                Sfx.Shimmer(); // 레벨업(승진) 순간
                var img = fill.GetComponent<Image>();
                float p = 0f;
                const float flashDur = 0.55f;
                while (p < flashDur)
                {
                    p += Time.deltaTime;
                    if (img == null) yield break;
                    float pulse = Mathf.Abs(Mathf.Sin(p / flashDur * Mathf.PI * 3f));
                    img.color = Color.Lerp(UiKit.Accent, new Color(1f, 0.95f, 0.72f), pulse);
                    yield return null;
                }
                img.color = UiKit.Accent;
            }
            else
            {
                Sfx.Pick(); // 미달 — 짧게 멈추는 딸깍
            }
            if (resultTag != null) resultTag.SetActive(true);
            yield return new WaitForSeconds(0.25f);
            if (verdictObj != null) verdictObj.SetActive(true);
        }
    }
}
