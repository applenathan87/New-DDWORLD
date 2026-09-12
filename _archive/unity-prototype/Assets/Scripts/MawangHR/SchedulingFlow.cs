using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MawangHR
{
    /// Day 1 후반 업무 — 면접 일정 잡기.
    /// 사진을 수정구에 가져다 대면 통화(말풍선은 수정구 위에) / 끌어서 캘린더 슬롯에 배치 /
    /// 전원 배치 후 확정 버튼. 제약 위반 배치 허용 — 대가는 다음날 아침 사고 보고서.
    public class SchedulingFlow : MonoBehaviour
    {
        public RectTransform CalendarCanvas { get; private set; }
        public bool AllPlaced => photos.All(p => p.slotIndex >= 0);
        public int PlacedCount => photos.Count(p => p.slotIndex >= 0);
        public int Total => photos.Count;

        private class Photo
        {
            public SchedCandidate data;
            public RectTransform rt;
            public Vector3 home;
            public Quaternion homeRot;
            public int slotIndex = -1;
            public bool called;
            public TextMeshProUGUI ruleLabel;
        }

        private const float InspectScale = 0.00075f; // 클릭해서 눈앞으로 가져왔을 때
        private const float DragScale = 0.00070f;    // 잡고 끄는 동안 (끌면서 읽을 수 있는 크기)
        private const float TrayScale = 0.00028f;    // 명단 트레이에 놓였을 때
        private const float SlotScale = 0.00025f;    // 일정표 칸에 꽂혔을 때 (칸 안에 정확히 들어가는 크기)
        private const float PadScale = 0.00042f;     // 통화 기록 노트 — 책상 위
        private const float PadUpScale = 0.00050f;   // 통화 기록 노트 — 세워서 볼 때

        private SchedulingData data;
        private DeskRig rig;
        private TextMeshProUGUI callPad;              // 수정구 옆 "통화 기록" 노트
        private RectTransform tray;                   // 지원자 명단 트레이 (사진들의 홈)
        private RectTransform notepad;                // 통화 기록 노트 캔버스
        private Vector3 padHomePos;                   // 노트가 놓여 있던 책상 위 자리
        private Quaternion padHomeRot;
        private bool notepadUp;                       // 노트를 세워서 보는 중인가
        private Action<string, float> showHint;
        private readonly List<Photo> photos = new List<Photo>();
        private readonly List<RectTransform> slotRects = new List<RectTransform>();
        private readonly List<RectTransform> slotInnerRects = new List<RectTransform>(); // 사진이 정확히 앉는 자리
        private readonly List<TextMeshProUGUI> slotOccupants = new List<TextMeshProUGUI>();
        private readonly List<TextMeshProUGUI> slotWarns = new List<TextMeshProUGUI>();  // 환경 경고 (빈 칸=중앙 크게, 점유=구석 작게)
        private readonly List<Image> slotInners = new List<Image>();
        private int inspectIndex = -1;                // 눈앞으로 가져온 사진
        private int inspectPrevSlot = -1;             // 확대 전에 꽂혀 있던 슬롯 (해제 시 복귀용)
        private RectTransform bubble;                 // 통화 말풍선 (화면 하단 중앙)
        private TextMeshProUGUI bubbleText;
        private Coroutine bubbleCo;
        private bool locked;

        private static readonly Dictionary<string, string> TagKo = new Dictionary<string, string>
        {
            { "day2", "내일" }, { "day3", "모레" },
            { "am", "오전" }, { "pm", "오후" }, { "night", "밤" },
            { "rain", "비 오는 날" }, { "fullmoon", "보름달" },
        };

        public void Begin(SchedulingData schedData, DeskRig deskRig, Action<string, float> showHintFn)
        {
            data = schedData;
            rig = deskRig;
            showHint = showHintFn;
            locked = false;
            BuildCalendar();
            BuildTray();
            BuildPhotos();
            BuildNotepad();
            BuildBubble();
        }

        // ─── 시나리오 생성 (매판 뽑기 + 해 보장) ───

        /// 풀에서 매판 시나리오를 뽑는다: 환경(비·보름달) 랜덤 배치 + 지원자 랜덤 뽑기.
        /// 단, 백트래킹 솔버로 "전원 무위반 배치가 존재하는 판"만 통과시킨다 —
        /// 억지로 마이너스를 받아야 하는 판은 애초에 출제되지 않는다.
        public static SchedulingData Roll(SchedulingData src)
        {
            int draw = src.drawCount > 0 ? src.drawCount : 5;
            int minCon = Mathf.Min(src.minConstrained > 0 ? src.minConstrained : 3, draw);

            for (int attempt = 0; attempt < 400; attempt++)
            {
                bool relaxed = attempt >= 300; // 계속 실패하면 난이도 하한만 풀고 해 존재는 계속 보장
                var slots = RollSlots(src);
                var cands = src.candidates.OrderBy(_ => UnityEngine.Random.value).Take(draw).ToArray();

                int constrained = cands.Count(c => !string.IsNullOrEmpty(c.requiredTag) || !string.IsNullOrEmpty(c.bannedTag));
                if (!relaxed && constrained < minCon) continue;
                if (!Solvable(cands, slots)) continue;

                return new SchedulingData { intro = src.intro, slots = slots, candidates = cands };
            }

            // 이론상 도달하기 어려운 폴백 — 환경 없이 제약 적은 순으로
            Debug.LogWarning("[MawangHR] 스케줄 시나리오 400회 생성 실패 — 무환경 폴백 사용 (풀 제약 구성을 점검하세요)");
            var baseSlots = src.slots.Select(CloneSlot).ToArray();
            var loose = src.candidates
                .OrderBy(c => (string.IsNullOrEmpty(c.requiredTag) ? 0 : 1) + (string.IsNullOrEmpty(c.bannedTag) ? 0 : 1))
                .Take(draw).ToArray();
            return new SchedulingData { intro = src.intro, slots = baseSlots, candidates = loose };
        }

        private static SchedSlot CloneSlot(SchedSlot s) =>
            new SchedSlot { label = s.label, warn = s.warn, tags = (string[])s.tags.Clone() };

        /// 기본 프레임을 복제하고 환경(비·보름달)을 랜덤 슬롯에 얹는다. 슬롯당 환경 1개(경고 표기 겹침 방지).
        private static SchedSlot[] RollSlots(SchedulingData src)
        {
            var slots = src.slots.Select(CloneSlot).ToArray();
            if (src.envs == null) return slots;
            var taken = new bool[slots.Length];
            foreach (var env in src.envs)
            {
                int count = UnityEngine.Random.Range(0, env.maxCount + 1);
                var picks = Enumerable.Range(0, slots.Length)
                    .Where(i => !taken[i] && slots[i].tags.Any(t => env.allowed.Contains(t)))
                    .OrderBy(_ => UnityEngine.Random.value)
                    .Take(count);
                foreach (int i in picks)
                {
                    taken[i] = true;
                    slots[i].warn = env.warn;
                    slots[i].tags = slots[i].tags.Append(env.tag).ToArray();
                }
            }
            return slots;
        }

        private static bool Fits(SchedCandidate c, SchedSlot s) =>
            (string.IsNullOrEmpty(c.requiredTag) || s.tags.Contains(c.requiredTag)) &&
            (string.IsNullOrEmpty(c.bannedTag) || !s.tags.Contains(c.bannedTag));

        /// 전원 무위반 배치가 존재하는가 — 백트래킹 (6P5 상한이라 즉시 끝난다)
        private static bool Solvable(SchedCandidate[] cands, SchedSlot[] slots)
        {
            var order = cands.OrderBy(c => slots.Count(s => Fits(c, s))).ToArray(); // 갈 곳 적은 후보부터 (가지치기)
            var used = new bool[slots.Length];
            bool Place(int i)
            {
                if (i >= order.Length) return true;
                for (int s = 0; s < slots.Length; s++)
                {
                    if (used[s] || !Fits(order[i], slots[s])) continue;
                    used[s] = true;
                    if (Place(i + 1)) return true;
                    used[s] = false;
                }
                return false;
            }
            return Place(0);
        }

        private static void SetSorting(RectTransform canvasRt, int order)
        {
            var c = canvasRt.GetComponent<Canvas>();
            if (c != null) c.sortingOrder = order;
        }

        /// GetHeldPose에서 목업풍 젖힘(-8° 피치 + 2° 롤)을 제거한 정면 직립 포즈 —
        /// 일정표·트레이·카드가 삐뚤어져 보이지 않게 한다.
        private void GetStraightPose(float distance, float upOffset, float rightOffset,
            out Vector3 pos, out Quaternion rot)
        {
            rig.GetHeldPose(distance, upOffset, rightOffset, 0f, out pos, out rot);
            rot *= Quaternion.Inverse(Quaternion.Euler(-8f, 0f, 2f));
        }

        /// 클릭 = 콜백 / 드래그 = 지정 평면 위 이동 (통화 기록 노트용 — 직립이면 카메라면, 눕힘이면 책상면 슬라이드)
        private class DragCatcher : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
        {
            public Camera cam;
            public Func<bool> canUse;
            public Action onClick;      // 드래그 없이 놓았을 때
            public Func<Plane> plane;   // 드래그 평면 공급자
            private Vector3 grabOffset;
            private bool dragging;
            private bool moved;

            public void OnPointerDown(PointerEventData e)
            {
                moved = false;
                if (e.button != PointerEventData.InputButton.Left) return;
                if (canUse == null || !canUse()) return;
                if (Ray(e.position, out var hit))
                {
                    grabOffset = transform.position - hit;
                    dragging = true;
                }
            }

            public void OnDrag(PointerEventData e)
            {
                if (!dragging || !canUse()) return;
                moved = true;
                if (Ray(e.position, out var hit)) transform.position = hit + grabOffset;
            }

            public void OnPointerUp(PointerEventData e)
            {
                if (!dragging) return;
                dragging = false;
                if (!moved) onClick?.Invoke();
            }

            private bool Ray(Vector2 screenPos, out Vector3 hit)
            {
                var p = plane();
                var ray = cam.ScreenPointToRay(screenPos);
                if (p.Raycast(ray, out float d)) { hit = ray.GetPoint(d); return true; }
                hit = default;
                return false;
            }
        }

        // ─── 캘린더 (목업형: 내일/모레 2열 × 오전/오후/밤 3행) ───

        private const float GridX = 175f, GridY = 150f, CellW = 396f, CellH = 252f, CellGap = 10f;
        private static readonly string[] RowHeads = { "오전", "오후", "밤" };
        private static readonly string[] RowHours = { "09:00 ~ 12:00", "13:00 ~ 17:00", "18:00 ~ 22:00" };
        private static readonly string[] ColHeads = { "내일", "모레" };

        private static int SlotCol(SchedSlot s) => s.tags.Contains("day3") ? 1 : 0;
        private static int SlotRow(SchedSlot s) => s.tags.Contains("night") ? 2 : s.tags.Contains("pm") ? 1 : 0;

        private void BuildCalendar()
        {
            CalendarCanvas = UiKit.MakeWorldCanvas("Calendar", 1000, 960, 0.00044f, rig.Cam);
            CalendarCanvas.SetParent(rig.transform, false);
            Vector3 pos; Quaternion rot;
            GetStraightPose(0.76f, 0.03f, 0.045f, out pos, out rot);
            CalendarCanvas.position = pos;
            CalendarCanvas.rotation = rot;
            CalendarCanvas.gameObject.AddComponent<Image>().color = UiKit.Paper;
            SetSorting(CalendarCanvas, 1);

            UiKit.LabelAt(CalendarCanvas, "<b>면접 일정표</b>", 40, UiKit.Ink, 0, 22, 1000, 52, TextAlignmentOptions.Center, true);
            UiKit.LabelAt(CalendarCanvas, "666년 5월 3주차 · 사진을 끌어다 붙이세요", 20, UiKit.InkDim,
                0, 72, 1000, 28, TextAlignmentOptions.Center, true);

            // 열 머리 — 내일 / 모레
            for (int c = 0; c < 2; c++)
                UiKit.LabelAt(CalendarCanvas, "<b>" + ColHeads[c] + "</b>", 30, UiKit.Ink,
                    GridX + c * (CellW + CellGap), 106, CellW, 38, TextAlignmentOptions.Center, true);

            // 행 머리 — 시간대 + 시각
            for (int r = 0; r < 3; r++)
                UiKit.LabelAt(CalendarCanvas,
                    $"<b>{RowHeads[r]}</b>\n<size=19><color=#6B5A42>{RowHours[r]}</color></size>",
                    28, UiKit.Ink, 14, GridY + r * (CellH + CellGap) + CellH / 2f - 44, 152, 92,
                    TextAlignmentOptions.Center, true);

            for (int i = 0; i < data.slots.Length; i++)
            {
                var slot = data.slots[i];
                int col = SlotCol(slot);
                int row = SlotRow(slot);

                var cell = UiKit.PanelRect(CalendarCanvas, "Slot" + i, UiKit.PaperShade);
                UiKit.Place(cell, GridX + col * (CellW + CellGap), GridY + row * (CellH + CellGap), CellW, CellH);
                slotRects.Add(cell);

                var inner = UiKit.PanelRect(cell, "Drop", new Color(0, 0, 0, 0.06f));
                UiKit.Place(inner, 14, 34, 368, 188);
                var innerImg = inner.GetComponent<Image>();
                innerImg.raycastTarget = false;
                slotInners.Add(innerImg);
                slotInnerRects.Add(inner);

                // 환경 경고 — 빈 칸일 땐 중앙에 크게 (목업), 사진이 앉으면 위 구석으로
                var warnLbl = UiKit.LabelAt(cell, string.IsNullOrEmpty(slot.warn) ? "" : "[!] " + slot.warn,
                    26, UiKit.StampInk, 14, 34, 368, 188, TextAlignmentOptions.Center, true);
                warnLbl.raycastTarget = false;
                slotWarns.Add(warnLbl);

                // 점유 표시 (슬롯 자체가 배치 상태를 보여줌)
                var occ = UiKit.LabelAt(cell, "", 24, UiKit.InkDim, 14, 224, 368, 26, TextAlignmentOptions.Center, true);
                occ.raycastTarget = false;
                slotOccupants.Add(occ);
            }
        }

        /// 환경 경고 라벨 재배치 — occupied면 위 구석 작게, 아니면 중앙 크게
        private void LayoutWarn(int s, bool occupied)
        {
            var lbl = slotWarns[s];
            if (string.IsNullOrEmpty(lbl.text)) return;
            if (occupied)
            {
                UiKit.Place(lbl.rectTransform, 170, 6, 212, 30);
                lbl.fontSizeMax = 18; // fit(오토사이징) 라벨이라 fontSize 대신 Max로 제어
                lbl.alignment = TextAlignmentOptions.TopRight;
            }
            else
            {
                UiKit.Place(lbl.rectTransform, 14, 34, 368, 188);
                lbl.fontSizeMax = 26;
                lbl.alignment = TextAlignmentOptions.Center;
            }
        }

        // ─── 지원자 명단 트레이 ───

        private void BuildTray()
        {
            tray = UiKit.MakeWorldCanvas("Tray", 400, 850, 0.00042f, rig.Cam);
            tray.SetParent(rig.transform, false);
            Vector3 pos; Quaternion rot;
            GetStraightPose(0.765f, 0.045f, 0.365f, out pos, out rot);
            tray.position = pos;
            tray.rotation = rot;
            tray.gameObject.AddComponent<Image>().color = UiKit.Panel;
            SetSorting(tray, 0);

            UiKit.LabelAt(tray, "<b>지원자 명단</b>", 30, UiKit.Accent, 0, 18, 400, 40, TextAlignmentOptions.Center, true);
            UiKit.LabelAt(tray, "클릭 = 확대 · 끌면 배치 · 수정구 = 통화", 17, UiKit.TextDim, 0, 58, 400, 26, TextAlignmentOptions.Center, true);
        }

        // ─── 사진 카드들 ───

        private void BuildPhotos()
        {
            int n = data.candidates.Length;
            for (int i = 0; i < n; i++)
            {
                var c = data.candidates[i];
                var p = new Photo { data = c };

                var rt = UiKit.MakeWorldCanvas("Photo_" + c.id, 250, 320, TrayScale, rig.Cam);
                rt.SetParent(rig.transform, false);
                Vector3 pos; Quaternion rot;
                // 트레이 안 2열 격자 (트레이 = 0.765/0.045/0.365, 사진은 살짝 앞)
                float x = 0.321f + (i % 2) * 0.088f;
                float y = 0.135f - (i / 2) * 0.102f;
                GetStraightPose(0.75f, y, x, out pos, out rot);
                rt.position = pos;
                rt.rotation = rot;
                p.rt = rt;
                p.home = pos;
                p.homeRot = rot;
                SetSorting(rt, 3); // 캘린더·다른 종이 위에 항상 그리기 (사이드 칸 뒤로 숨는 버그 방지)

                rt.gameObject.AddComponent<Image>().color = UiKit.Paper;

                var face = UiKit.PanelRect(rt, "Face", SpeciesColor(c.species));
                UiKit.Place(face, 75, 14, 100, 100);
                face.GetComponent<Image>().raycastTarget = false;

                UiKit.LabelAt(rt, "<b>" + c.name + "</b>", 28, UiKit.Ink, 12, 122, 226, 36, TextAlignmentOptions.Center, true);
                UiKit.LabelAt(rt, c.species, 19, UiKit.InkDim, 12, 160, 226, 28, TextAlignmentOptions.Center, true);
                UiKit.LabelAt(rt, c.hint, 18, UiKit.InkDim, 12, 192, 226, 62, TextAlignmentOptions.Center, true);
                p.ruleLabel = UiKit.LabelAt(rt, "<color=#6B5A42>(수정구로 끌어가 통화)</color>", 19, UiKit.StampInk,
                    12, 258, 226, 54, TextAlignmentOptions.Center, true);

                int idx = i;
                rt.gameObject.AddComponent<PhotoDraggable>().Init(
                    rig.Cam,
                    () => !locked,
                    () => ToggleInspect(idx),   // 제자리 클릭 = 눈앞으로 가져와 읽기 (다시 클릭 = 내려놓기)
                    dropPos => HandleDrop(idx),
                    () => // 잡는 순간: 끌면서 읽을 크기로 확대 + 다른 카드들 위로 (슬롯 카드 밑에 깔림 방지)
                    {
                        photos[idx].rt.localScale = Vector3.one * DragScale;
                        SetSorting(photos[idx].rt, 8);
                    });

                photos.Add(p);
            }
        }

        // ─── 확대 보기 (클릭 = 눈앞으로) ───

        private void ToggleInspect(int i)
        {
            if (locked) return;
            if (inspectIndex == i) { DismissInspect(); return; }
            DismissInspect(); // 다른 카드를 보고 있었다면 먼저 제자리로

            var p = photos[i];
            inspectPrevSlot = p.slotIndex;      // 슬롯에서 집어 들었다면 해제 시 그 칸으로 복귀
            ClearSlotVisual(p.slotIndex);
            p.slotIndex = -1;
            inspectIndex = i;

            Vector3 pos; Quaternion rot;
            GetStraightPose(0.45f, -0.015f, 0f, out pos, out rot);
            p.rt.position = pos;
            p.rt.rotation = rot;
            p.rt.localScale = Vector3.one * InspectScale;
            SetSorting(p.rt, 8); // 눈앞 카드가 슬롯 카드에 가려지지 않게 맨 위로
            Sfx.Pick();
        }

        /// 확대 해제 — 원래 슬롯이 아직 비어 있으면 그 칸으로, 아니면 트레이로
        private void DismissInspect()
        {
            if (inspectIndex < 0) return;
            var p = photos[inspectIndex];
            int back = inspectPrevSlot;
            inspectIndex = -1;
            inspectPrevSlot = -1;
            if (back >= 0 && !photos.Any(o => o.slotIndex == back)) PlaceInSlot(p, back);
            else SendHome(p);
        }

        /// 종족 대표색 — 스케줄링 사진 카드 + 면접 몬스터 그레이박스 공용
        public static Color SpeciesColor(string species)
        {
            if (species.Contains("뱀파이어")) return new Color(0.45f, 0.15f, 0.20f);
            if (species.Contains("슬라임")) return new Color(0.30f, 0.55f, 0.75f);
            if (species.Contains("스켈레톤")) return new Color(0.85f, 0.83f, 0.75f);
            if (species.Contains("오크")) return new Color(0.35f, 0.50f, 0.28f);
            if (species.Contains("마녀")) return new Color(0.45f, 0.30f, 0.55f);
            if (species.Contains("임프")) return new Color(0.82f, 0.42f, 0.24f);
            if (species.Contains("드래곤")) return new Color(0.80f, 0.62f, 0.22f);
            if (species.Contains("나이트메어")) return new Color(0.18f, 0.16f, 0.28f);
            if (species.Contains("미라")) return new Color(0.78f, 0.72f, 0.58f);
            if (species.Contains("비홀더")) return new Color(0.60f, 0.30f, 0.45f);
            if (species.Contains("나가")) return new Color(0.20f, 0.50f, 0.55f);
            if (species.Contains("트롤")) return new Color(0.25f, 0.42f, 0.38f);
            if (species.Contains("미믹")) return new Color(0.55f, 0.40f, 0.22f);
            if (species.Contains("하피")) return new Color(0.62f, 0.46f, 0.26f);
            if (species.Contains("데스나이트")) return new Color(0.24f, 0.24f, 0.30f);
            if (species.Contains("픽시")) return new Color(0.85f, 0.55f, 0.70f);
            if (species.Contains("수인")) return new Color(0.70f, 0.58f, 0.42f);
            if (species.Contains("운디네")) return new Color(0.35f, 0.60f, 0.70f);
            if (species.Contains("늑대인간")) return new Color(0.48f, 0.40f, 0.32f);
            if (species.Contains("유령")) return new Color(0.72f, 0.78f, 0.82f);
            return new Color(0.5f, 0.45f, 0.4f);
        }

        // ─── 통화 기록 노트 (수정구 옆 책상 위) ───

        private void BuildNotepad()
        {
            notepad = UiKit.MakeWorldCanvas("CallPad", 460, 620, PadScale, rig.Cam);
            notepad.SetParent(rig.transform, false);
            // 수정구 앞쪽(카메라 쪽) 책상 위에 평평하게 놓인 메모장
            padHomePos = new Vector3(-0.40f, DeskRig.DeskTop + 0.012f, -0.38f);
            padHomeRot = Quaternion.Euler(90f, -6f, 0f);
            notepad.position = padHomePos;
            notepad.rotation = padHomeRot;
            SetSorting(notepad, 2);
            notepad.gameObject.AddComponent<Image>().color = UiKit.Paper;

            // 클릭 = 세우기/눕히기 토글, 드래그 = 이동 (직립이면 카메라면, 책상 위면 책상면 슬라이드)
            var drag = notepad.gameObject.AddComponent<DragCatcher>();
            drag.cam = rig.Cam;
            drag.canUse = () => !locked;
            drag.onClick = ToggleNotepad;
            drag.plane = () => notepadUp
                ? new Plane(rig.Cam.transform.forward, notepad.position)
                : new Plane(Vector3.up, notepad.position);

            UiKit.LabelAt(notepad, "<b>통화 기록</b>  <size=17><color=#6B5A42>(클릭 = 세우기 · 드래그 = 이동)</color></size>",
                30, UiKit.Ink, 0, 20, 460, 38, TextAlignmentOptions.Center, true);
            callPad = UiKit.LabelAt(notepad, "(사진을 수정구로 끌어가면\n통화 내용이 기록됩니다)", 23, UiKit.InkDim,
                28, 70, 404, 528, TextAlignmentOptions.TopLeft, true);
            callPad.raycastTarget = false;
        }

        /// 통화 기록 노트 토글 — 클릭하면 눈앞에 세워서 읽고, 다시 클릭하면 책상 위 제자리로
        private void ToggleNotepad()
        {
            if (locked) return;
            notepadUp = !notepadUp;
            Sfx.Pick();
            if (notepadUp)
            {
                Vector3 pos; Quaternion rot;
                GetStraightPose(0.50f, 0.0f, -0.16f, out pos, out rot);
                notepad.position = pos;
                notepad.rotation = rot;
                notepad.localScale = Vector3.one * PadUpScale;
                SetSorting(notepad, 7); // 확대 카드(3)·말풍선(6)보다 위
            }
            else
            {
                notepad.position = padHomePos;
                notepad.rotation = padHomeRot;
                notepad.localScale = Vector3.one * PadScale;
                SetSorting(notepad, 2);
            }
        }

        // ─── 말풍선 (통화 대화창 — 화면 하단 중앙) ───

        private void BuildBubble()
        {
            bubble = UiKit.MakeWorldCanvas("CallBubble", 780, 250, 0.0005f, rig.Cam);
            bubble.SetParent(rig.transform, false);
            // 수정구 위는 화면 왼쪽 끝이라 글씨가 잘림 — 자막처럼 하단 중앙 고정
            Vector3 pos; Quaternion rot;
            GetStraightPose(0.70f, -0.215f, 0f, out pos, out rot);
            bubble.position = pos;
            bubble.rotation = rot;
            SetSorting(bubble, 6);
            bubble.gameObject.AddComponent<Image>().color = UiKit.Paper;
            bubbleText = UiKit.LabelAt(bubble, "", 26, UiKit.Ink, 30, 22, 720, 206, TextAlignmentOptions.TopLeft, true);
            bubbleText.raycastTarget = false;
            bubble.gameObject.SetActive(false);
        }

        private void ShowBubble(string text, float duration)
        {
            if (bubbleCo != null) StopCoroutine(bubbleCo);
            bubble.gameObject.SetActive(true);
            bubbleText.text = text;
            bubbleCo = StartCoroutine(HideBubbleAfter(duration));
        }

        private IEnumerator HideBubbleAfter(float t)
        {
            yield return new WaitForSeconds(t);
            if (bubble != null) bubble.gameObject.SetActive(false);
            bubbleCo = null;
        }

        // ─── 배치 / 통화 (드롭 분기) ───

        private void HandleDrop(int i)
        {
            var p = photos[i];

            // 확대 중이던 카드를 직접 끌었다면 확대 상태만 해제 (카드는 드롭 위치가 결정)
            if (inspectIndex == i) { inspectIndex = -1; inspectPrevSlot = -1; }
            else DismissInspect(); // 다른 카드를 조작하면 보던 카드는 제자리로

            // 판정 기준 = 사진의 중심 (플레이어가 보는 그대로)
            Vector2 photoCenter = rig.Cam.WorldToScreenPoint(p.rt.position);

            // 1) 수정구 위에 놓으면 = 통화 — 카드는 사라지고 원래 자리에 다시 생긴다
            Vector2 orbCenter = rig.Cam.WorldToScreenPoint(rig.Orb.position);
            if (Vector2.Distance(photoCenter, orbCenter) < Screen.height * 0.11f)
            {
                CallCandidate(i);
                return;
            }

            // 2) 캘린더 슬롯
            for (int s = 0; s < slotRects.Count; s++)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(slotRects[s], photoCenter, rig.Cam))
                    continue;
                PlaceInSlot(p, s);
                return;
            }

            SendHome(p);
        }

        /// 사진을 일정표 칸 안에 정확히 앉힌다 (기존 점유자는 트레이로)
        private void PlaceInSlot(Photo p, int s)
        {
            var occupant = photos.FirstOrDefault(o => o.slotIndex == s);
            if (occupant != null && occupant != p) SendHome(occupant);

            ClearSlotVisual(p.slotIndex);
            p.slotIndex = s;

            var inner = slotInnerRects[s];
            Vector3 center = inner.TransformPoint(inner.rect.center);
            p.rt.position = center - CalendarCanvas.forward * 0.02f;
            p.rt.rotation = CalendarCanvas.rotation;
            p.rt.localScale = Vector3.one * SlotScale;

            slotOccupants[s].text = "<b>" + p.data.name + "</b>";
            slotInners[s].color = new Color(0.24f, 0.49f, 0.30f, 0.18f);
            LayoutWarn(s, true);
            SetSorting(p.rt, 3); // 내려놓으면 기본 순서로 (들고 있는 카드가 항상 위에 오도록)
            Sfx.Scratch();
        }

        /// 수정구 통화 — 말풍선(화면 하단 중앙) + 통화 기록 갱신, 카드는 즉시 원래 자리로
        private void CallCandidate(int i)
        {
            var p = photos[i];
            Sfx.Pick();
            ShowBubble($"<b>{p.data.name}</b>  <color=#6B5A42>({p.data.species})</color>\n“{p.data.callLine}”", 4.5f);

            if (!p.called)
            {
                p.called = true;
                p.ruleLabel.text = RuleText(p.data);
                RefreshCallPad();
            }
            SendHome(p); // 수정구에 붙잡아두지 않는다
        }

        private string RuleText(SchedCandidate c)
        {
            if (!string.IsNullOrEmpty(c.requiredTag)) return $"<b>{TagLabel(c.requiredTag)}만 가능</b>";
            if (!string.IsNullOrEmpty(c.bannedTag)) return $"<b>{TagLabel(c.bannedTag)} 불가</b>";
            return "<color=#3E7D4E><b>아무 때나 가능</b></color>";
        }

        /// 사전에 없는 태그(콘텐츠 확장·오타)여도 크래시 대신 원문 표기 + 경고
        private static string TagLabel(string tag)
        {
            if (TagKo.TryGetValue(tag, out string ko)) return ko;
            Debug.LogWarning($"[MawangHR] 태그 '{tag}'의 한글 표기가 TagKo 사전에 없습니다 — 원문으로 표시");
            return tag;
        }

        private void RefreshCallPad()
        {
            if (callPad == null) return;
            var lines = photos.Where(p => p.called)
                .Select(p => $"<color=#D98F3E>›</color> {p.data.name} — {RuleText(p.data)}");
            callPad.text = photos.Any(p => p.called)
                ? string.Join("\n\n", lines)
                : "(사진을 수정구로 끌어가면\n통화 내용이 기록됩니다)";
        }

        private void SendHome(Photo p)
        {
            ClearSlotVisual(p.slotIndex);
            p.slotIndex = -1;
            p.rt.position = p.home;
            p.rt.rotation = p.homeRot;
            p.rt.localScale = Vector3.one * TrayScale;
            SetSorting(p.rt, 3); // 내려놓으면 기본 순서로
        }

        private void ClearSlotVisual(int s)
        {
            if (s < 0) return;
            slotOccupants[s].text = "";
            slotInners[s].color = new Color(0, 0, 0, 0.06f);
            LayoutWarn(s, false);
        }

        // ─── 확정 & 채점 ───

        public void Lock()
        {
            DismissInspect();                    // 확대 중이던 카드는 제자리로
            if (notepadUp) ToggleNotepad();      // 세워 둔 노트는 책상으로 (locked 전에 호출)
            locked = true;
        }

        public List<string> GetViolations()
        {
            var result = new List<string>();
            foreach (var p in photos)
            {
                if (p.slotIndex < 0) continue;
                var slot = data.slots[p.slotIndex];
                var c = p.data;
                bool bad =
                    (!string.IsNullOrEmpty(c.requiredTag) && !slot.tags.Contains(c.requiredTag)) ||
                    (!string.IsNullOrEmpty(c.bannedTag) && slot.tags.Contains(c.bannedTag));
                if (bad)
                    result.Add($"<b>[{slot.label} · {c.name}]</b>  {c.violationLine}");
            }
            return result;
        }

        public void Cleanup()
        {
            foreach (var p in photos)
                if (p.rt != null) Destroy(p.rt.gameObject);
            photos.Clear();
            slotRects.Clear();
            slotInnerRects.Clear();
            slotOccupants.Clear();
            slotWarns.Clear();
            slotInners.Clear();
            if (bubble != null) Destroy(bubble.gameObject);
            if (tray != null) Destroy(tray.gameObject);
            if (notepad != null) Destroy(notepad.gameObject);
            if (CalendarCanvas != null) Destroy(CalendarCanvas.gameObject);
            Destroy(gameObject);
        }
    }
}
