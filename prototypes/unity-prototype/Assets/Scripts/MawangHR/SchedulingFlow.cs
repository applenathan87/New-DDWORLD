using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
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

        private const float BaseScale = 0.00040f;  // 트레이/드래그 중 사진 크기
        private const float SlotScale = 0.00031f;  // 슬롯에 꽂혔을 때 (칸 안에 정확히 들어가는 크기)

        private SchedulingData data;
        private DeskRig rig;
        private TextMeshProUGUI callPad;              // HUD 통화 기록 패드
        private Action<string, float> showHint;
        private readonly List<Photo> photos = new List<Photo>();
        private readonly List<RectTransform> slotRects = new List<RectTransform>();
        private readonly List<RectTransform> slotInnerRects = new List<RectTransform>(); // 사진이 정확히 앉는 자리
        private readonly List<TextMeshProUGUI> slotOccupants = new List<TextMeshProUGUI>();
        private readonly List<Image> slotInners = new List<Image>();
        private int dockedIndex = -1;                 // 수정구에 붙어 있는 사진
        private RectTransform bubble;                 // 수정구 위 말풍선
        private TextMeshProUGUI bubbleText;
        private Coroutine bubbleCo;
        private bool locked;

        private static readonly Dictionary<string, string> TagKo = new Dictionary<string, string>
        {
            { "day2", "내일" }, { "day3", "모레" },
            { "am", "오전" }, { "pm", "오후" }, { "night", "밤" },
            { "rain", "비 오는 날" }, { "fullmoon", "보름달" },
        };

        public void Begin(SchedulingData schedData, DeskRig deskRig,
            TextMeshProUGUI callPadLabel, Action<string, float> showHintFn)
        {
            data = schedData;
            rig = deskRig;
            callPad = callPadLabel;
            showHint = showHintFn;
            locked = false;
            BuildCalendar();
            BuildPhotos();
            BuildBubble();
        }

        private static void SetSorting(RectTransform canvasRt, int order)
        {
            var c = canvasRt.GetComponent<Canvas>();
            if (c != null) c.sortingOrder = order;
        }

        // ─── 캘린더 ───

        private void BuildCalendar()
        {
            CalendarCanvas = UiKit.MakeWorldCanvas("Calendar", 1150, 800, 0.00045f, rig.Cam);
            CalendarCanvas.SetParent(rig.transform, false);
            Vector3 pos; Quaternion rot;
            rig.GetHeldPose(0.74f, 0.10f, 0.02f, 0f, out pos, out rot);
            CalendarCanvas.position = pos;
            CalendarCanvas.rotation = rot;
            CalendarCanvas.gameObject.AddComponent<Image>().color = UiKit.Paper;
            SetSorting(CalendarCanvas, 1);

            UiKit.LabelAt(CalendarCanvas, "<b>면접 일정표</b>  <size=22><color=#6B5A42>· 666년 5월 3주차 · 사진을 끌어다 붙이세요</color></size>",
                34, UiKit.Ink, 40, 22, 1070, 50);

            for (int i = 0; i < data.slots.Length; i++)
            {
                int col = i % 3;
                int row = i / 3;
                var slot = data.slots[i];

                var cell = UiKit.PanelRect(CalendarCanvas, "Slot" + i, UiKit.PaperShade);
                UiKit.Place(cell, 30 + col * 370, 95 + row * 350, 350, 335);
                slotRects.Add(cell);

                UiKit.LabelAt(cell, "<b>" + slot.label + "</b>", 26, UiKit.Ink, 16, 10, 200, 34);
                if (!string.IsNullOrEmpty(slot.warn))
                    UiKit.LabelAt(cell, "[!] " + slot.warn, 22, UiKit.StampInk, 180, 12, 160, 32, TextAlignmentOptions.TopRight, true);

                var inner = UiKit.PanelRect(cell, "Drop", new Color(0, 0, 0, 0.06f));
                UiKit.Place(inner, 16, 50, 318, 270);
                var innerImg = inner.GetComponent<Image>();
                innerImg.raycastTarget = false;
                slotInners.Add(innerImg);
                slotInnerRects.Add(inner);

                // 점유 표시 (슬롯 자체가 배치 상태를 보여줌)
                var occ = UiKit.LabelAt(cell, "", 24, UiKit.InkDim, 16, 296, 318, 34, TextAlignmentOptions.Center, true);
                occ.raycastTarget = false;
                slotOccupants.Add(occ);
            }
        }

        // ─── 사진 카드들 ───

        private void BuildPhotos()
        {
            int n = data.candidates.Length;
            for (int i = 0; i < n; i++)
            {
                var c = data.candidates[i];
                var p = new Photo { data = c };

                var rt = UiKit.MakeWorldCanvas("Photo_" + c.id, 250, 320, BaseScale, rig.Cam);
                rt.SetParent(rig.transform, false);
                Vector3 pos; Quaternion rot;
                float x = -0.34f + i * 0.17f;
                rig.GetHeldPose(0.64f, -0.165f, x, 0f, out pos, out rot);
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
                    () => showHint?.Invoke("사진을 <b>수정구</b>에 가져다 대면 통화합니다", 2.0f),
                    dropPos => HandleDrop(idx));

                photos.Add(p);
            }
        }

        private static Color SpeciesColor(string species)
        {
            if (species.Contains("뱀파이어")) return new Color(0.45f, 0.15f, 0.20f);
            if (species.Contains("슬라임")) return new Color(0.30f, 0.55f, 0.75f);
            if (species.Contains("스켈레톤")) return new Color(0.85f, 0.83f, 0.75f);
            if (species.Contains("오크")) return new Color(0.35f, 0.50f, 0.28f);
            if (species.Contains("마녀")) return new Color(0.45f, 0.30f, 0.55f);
            return new Color(0.5f, 0.45f, 0.4f);
        }

        // ─── 말풍선 (수정구 위 대화창) ───

        private void BuildBubble()
        {
            bubble = UiKit.MakeWorldCanvas("CallBubble", 780, 250, 0.0005f, rig.Cam);
            bubble.SetParent(rig.transform, false);
            bubble.position = rig.Orb.position + Vector3.up * 0.32f;
            bubble.rotation = Quaternion.LookRotation(rig.Cam.transform.forward, rig.Cam.transform.up);
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
            if (dockedIndex == i) dockedIndex = -1; // 수정구에서 떼어냄

            // 판정 기준 = 사진의 중심 (플레이어가 보는 그대로)
            Vector2 photoCenter = rig.Cam.WorldToScreenPoint(p.rt.position);

            // 1) 수정구 위에 놓으면 = 통화 (사진이 수정구에 붙음)
            Vector2 orbCenter = rig.Cam.WorldToScreenPoint(rig.Orb.position);
            if (Vector2.Distance(photoCenter, orbCenter) < Screen.height * 0.11f)
            {
                DockToOrb(i);
                return;
            }

            // 2) 캘린더 슬롯
            for (int s = 0; s < slotRects.Count; s++)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(slotRects[s], photoCenter, rig.Cam))
                    continue;

                var occupant = photos.FirstOrDefault(o => o.slotIndex == s);
                if (occupant != null && occupant != p) SendHome(occupant);

                ClearSlotVisual(p.slotIndex);
                p.slotIndex = s;

                // 칸 안(inner)에 정확히 앉히기
                var inner = slotInnerRects[s];
                Vector3 center = inner.TransformPoint(inner.rect.center);
                p.rt.position = center - CalendarCanvas.forward * 0.02f;
                p.rt.rotation = CalendarCanvas.rotation;
                p.rt.localScale = Vector3.one * SlotScale;

                slotOccupants[s].text = "<b>" + p.data.name + "</b>";
                slotInners[s].color = new Color(0.24f, 0.49f, 0.30f, 0.18f);
                Sfx.Scratch();
                return;
            }

            SendHome(p);
        }

        /// 사진이 수정구에 붙고, 말풍선이 수정구 위에 뜬다
        private void DockToOrb(int i)
        {
            var p = photos[i];

            // 기존에 붙어 있던 사진은 트레이로
            if (dockedIndex >= 0 && dockedIndex != i) SendHome(photos[dockedIndex]);
            dockedIndex = i;

            ClearSlotVisual(p.slotIndex);
            p.slotIndex = -1;

            var camT = rig.Cam.transform;
            p.rt.position = rig.Orb.position + Vector3.up * 0.16f;
            p.rt.rotation = Quaternion.LookRotation(camT.forward, camT.up);
            p.rt.localScale = Vector3.one * BaseScale;

            Sfx.Pick();
            ShowBubble($"<b>{p.data.name}</b>  <color=#6B5A42>({p.data.species})</color>\n“{p.data.callLine}”", 4.5f);

            if (!p.called)
            {
                p.called = true;
                p.ruleLabel.text = RuleText(p.data);
                RefreshCallPad();
            }
        }

        private string RuleText(SchedCandidate c)
        {
            if (!string.IsNullOrEmpty(c.requiredTag)) return $"<b>{TagKo[c.requiredTag]}만 가능</b>";
            if (!string.IsNullOrEmpty(c.bannedTag)) return $"<b>{TagKo[c.bannedTag]} 불가</b>";
            return "<color=#3E7D4E><b>아무 때나 가능</b></color>";
        }

        private void RefreshCallPad()
        {
            if (callPad == null) return;
            var lines = photos.Where(p => p.called)
                .Select(p => $"<color=#D98F3E>›</color> {p.data.name} — {RuleText(p.data)}");
            callPad.text = photos.Any(p => p.called)
                ? string.Join("\n", lines)
                : "(사진을 수정구로 끌어가면 통화 내용이 기록됩니다)";
        }

        private void SendHome(Photo p)
        {
            if (dockedIndex >= 0 && photos[dockedIndex] == p) dockedIndex = -1;
            ClearSlotVisual(p.slotIndex);
            p.slotIndex = -1;
            p.rt.position = p.home;
            p.rt.rotation = p.homeRot;
            p.rt.localScale = Vector3.one * BaseScale;
        }

        private void ClearSlotVisual(int s)
        {
            if (s < 0) return;
            slotOccupants[s].text = "";
            slotInners[s].color = new Color(0, 0, 0, 0.06f);
        }

        // ─── 확정 & 채점 ───

        public void Lock() => locked = true;

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
            slotInners.Clear();
            if (bubble != null) Destroy(bubble.gameObject);
            if (CalendarCanvas != null) Destroy(CalendarCanvas.gameObject);
            Destroy(gameObject);
        }
    }
}
