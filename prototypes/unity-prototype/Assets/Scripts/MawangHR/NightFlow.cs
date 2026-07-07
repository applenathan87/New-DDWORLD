using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MawangHR
{
    /// 밤 파트 진행 — 내 방에서: 석간 신문(여파) → 월급봉투(정산) → 자유 행동(상점·소품) → 침대(취침).
    /// 행동 횟수 제한 없음 (톤 원칙: 긴장은 낮의 몫, 밤은 이완과 보상의 몫).
    public class NightFlow : MonoBehaviour
    {
        private NightRoom room;
        private NightData cfg;
        private int level;
        private Transform hudParent;
        private Func<int> getGold;
        private Action<int> addGold;
        private Func<NightShopItem, string> buyBlockReason; // null = 구매 가능, 아니면 사유
        private Func<NightShopItem, string> tryBuy;         // null = 성공, 아니면 실패 사유
        private Action<string, float> showHint;
        private Action onSleep;

        private RectTransform paper;      // 석간 신문
        private Vector3 paperHome; private Quaternion paperHomeRot;
        private RectTransform envelope;   // 월급봉투 (열기 전)
        private RectTransform slip;       // 급여 명세서 (열면 등장)
        private Vector3 slipHome; private Quaternion slipHomeRot;
        private RectTransform shopPanel;  // 까마귀 상점 (HUD 오버레이)
        private RectTransform lifted;     // 지금 눈앞에 들려 있는 종이
        private int wage;
        private bool paid;
        private bool sleeping;

        private static readonly string[] SlimeLines =
        {
            "뽀잉. (반겨주는 것 같다)",
            "출렁… 출렁… (오늘도 수고했다는 뜻이다. 아마도.)",
            "슬라임이 발등에 살짝 올라왔다가 내려갔다.",
            "찹— (신발이 조금 촉촉해졌다)",
        };

        public void Begin(NightRoom nightRoom, NightData config, int playerLevel,
            List<string> articles, List<string> wageLines, int wageTotal,
            Transform hudRoot, Func<int> goldFn, Action<int> addGoldFn,
            Func<NightShopItem, string> blockReasonFn, Func<NightShopItem, string> tryBuyFn,
            Action<string, float> showHintFn, Action onSleepFn)
        {
            room = nightRoom;
            cfg = config;
            level = playerLevel;
            hudParent = hudRoot;
            getGold = goldFn;
            addGold = addGoldFn;
            buyBlockReason = blockReasonFn;
            tryBuy = tryBuyFn;
            showHint = showHintFn;
            onSleep = onSleepFn;
            wage = wageTotal;
            paid = false;
            sleeping = false;

            BuildNewspaper(articles);
            BuildEnvelope(wageLines);
            WireProps();
            room.SetCrowVisible(level >= 2);
        }

        private static void SetSorting(RectTransform canvasRt, int order)
        {
            var c = canvasRt.GetComponent<Canvas>();
            if (c != null) c.sortingOrder = order;
        }

        private static RoomProp Wire(Transform t, Action fn)
        {
            if (t == null) return null;
            var p = t.GetComponent<RoomProp>();
            if (p == null) p = t.gameObject.AddComponent<RoomProp>();
            p.onClick = fn;
            return p;
        }

        // ─── 석간 신문 ───

        private void BuildNewspaper(List<string> articles)
        {
            paper = UiKit.MakeWorldCanvas("EveningPaper", 700, 920, 0.00042f, room.Cam);
            paper.SetParent(room.transform, false);
            paperHome = room.TableTop + new Vector3(-0.15f, 0.004f, 0.05f);
            paperHomeRot = Quaternion.Euler(90f, -7f, 0f);
            paper.position = paperHome;
            paper.rotation = paperHomeRot;
            paper.gameObject.AddComponent<Image>().color = UiKit.Paper;
            SetSorting(paper, 2);

            UiKit.LabelAt(paper, "<b>마왕성 석간</b>", 44, UiKit.Ink, 0, 26, 700, 56, TextAlignmentOptions.Center, true);
            UiKit.LabelAt(paper, "666년 5월 20일 밤 · 구독료: 영혼 약간", 19, UiKit.InkDim, 0, 82, 700, 28, TextAlignmentOptions.Center, true);
            var divider = UiKit.PanelRect(paper, "Divider", UiKit.InkDim);
            UiKit.Place(divider, 40, 118, 620, 3);
            UiKit.LabelAt(paper, string.Join("\n\n", articles), 25, UiKit.Ink,
                40, 140, 620, 750, TextAlignmentOptions.TopLeft, true);

            Wire(paper, TogglePaper);
        }

        private void TogglePaper() => ToggleLift(paper, paperHome, paperHomeRot);

        // ─── 월급봉투 + 명세서 ───

        private void BuildEnvelope(List<string> wageLines)
        {
            envelope = UiKit.MakeWorldCanvas("PayEnvelope", 380, 230, 0.0004f, room.Cam);
            envelope.SetParent(room.transform, false);
            envelope.position = room.TableTop + new Vector3(0.24f, 0.004f, -0.12f);
            envelope.rotation = Quaternion.Euler(90f, 6f, 0f);
            envelope.gameObject.AddComponent<Image>().color = new Color(0.82f, 0.72f, 0.52f); // 봉투색
            SetSorting(envelope, 2);
            UiKit.LabelAt(envelope, "<b>월급봉투</b>", 34, UiKit.Ink, 0, 70, 380, 44, TextAlignmentOptions.Center, true);
            UiKit.LabelAt(envelope, "(클릭해서 열기)", 20, UiKit.InkDim, 0, 120, 380, 30, TextAlignmentOptions.Center, true);
            Wire(envelope, OpenEnvelope);

            // 명세서 — 봉투를 열면 등장
            slip = UiKit.MakeWorldCanvas("PaySlip", 560, 680, 0.00045f, room.Cam);
            slip.SetParent(room.transform, false);
            slipHome = room.TableTop + new Vector3(0.24f, 0.006f, -0.05f);
            slipHomeRot = Quaternion.Euler(90f, 4f, 0f);
            slip.gameObject.AddComponent<Image>().color = UiKit.Paper;
            UiKit.LabelAt(slip, "<b>급여 명세서</b>", 36, UiKit.Ink, 0, 28, 560, 46, TextAlignmentOptions.Center, true);
            UiKit.LabelAt(slip, "마왕성 인사팀 · 경리부 (문의는 받지 않음)", 18, UiKit.InkDim, 0, 76, 560, 26, TextAlignmentOptions.Center, true);
            var div = UiKit.PanelRect(slip, "Divider", UiKit.InkDim);
            UiKit.Place(div, 40, 110, 480, 3);
            UiKit.LabelAt(slip, string.Join("\n", wageLines), 26, UiKit.Ink,
                50, 132, 460, 500, TextAlignmentOptions.TopLeft, true);
            Wire(slip, ToggleSlip);
            slip.gameObject.SetActive(false);
        }

        private void OpenEnvelope()
        {
            if (paid) return;
            paid = true;
            addGold(wage);
            Sfx.Pick();
            Sfx.Shimmer();
            envelope.gameObject.SetActive(false);
            slip.gameObject.SetActive(true);
            slip.position = slipHome;
            slip.rotation = slipHomeRot;
            SetSorting(slip, 2);
            ToggleSlip(); // 열자마자 눈앞으로
            showHint($"월급 <b>{wage}G</b> 입금 — 명세서를 클릭하면 내려놓습니다", 2.5f);
        }

        private void ToggleSlip() => ToggleLift(slip, slipHome, slipHomeRot);

        /// 종이를 눈앞으로 / 제자리로 — 한 번에 하나만 들려 있는다
        private void ToggleLift(RectTransform rt, Vector3 home, Quaternion homeRot)
        {
            if (lifted == rt)
            {
                rt.position = home;
                rt.rotation = homeRot;
                SetSorting(rt, 2);
                lifted = null;
                Sfx.Swish();
                return;
            }
            if (lifted != null)
            {
                // 들고 있던 다른 종이는 제자리로
                if (lifted == paper) { paper.position = paperHome; paper.rotation = paperHomeRot; }
                else if (lifted == slip) { slip.position = slipHome; slip.rotation = slipHomeRot; }
                SetSorting(lifted, 2);
            }
            var camT = room.Cam.transform;
            rt.position = camT.position + camT.forward * 0.56f + camT.up * -0.01f;
            rt.rotation = Quaternion.LookRotation(camT.forward, camT.up);
            SetSorting(rt, 8);
            lifted = rt;
            Sfx.Swish();
        }

        // ─── 방 소품 배선 ───

        private void WireProps()
        {
            Wire(room.Bed, Sleep);
            Wire(room.Slime, () =>
            {
                room.SlimeBounce();
                Sfx.Pick();
                showHint(SlimeLines[UnityEngine.Random.Range(0, SlimeLines.Length)], 2.2f);
            });
            Wire(room.Window, () => showHint("달이 밝다. 박쥐들이 야근을 나간다.", 2.2f));
            Wire(room.Poster, () => showHint("「이달의 사원」 스켈레톤 인턴. …나도 언젠간.", 2.2f));
            Wire(room.DiceCup, () => showHint(level >= 3
                ? "주사위 미니게임은 준비 중입니다. (본편에서!)"
                : "<b>「인사 규정 §66」</b> 주사위 도박은 3급부터. 다음 승진을 노리자.", 2.6f));
            Wire(room.Crow, ToggleShop);
        }

        // ─── 까마귀 상점 (HUD 오버레이) ───

        private void ToggleShop()
        {
            if (shopPanel != null)
            {
                Destroy(shopPanel.gameObject);
                shopPanel = null;
                return;
            }
            Sfx.Pick();
            BuildShopPanel();
        }

        private void BuildShopPanel()
        {
            shopPanel = UiKit.PanelRect(hudParent, "CrowShop", new Color(0.16f, 0.12f, 0.09f, 0.96f));
            UiKit.Place(shopPanel, 1400, 120, 490, 800);

            UiKit.LabelAt(shopPanel, "<b>까마귀 상점</b>", 30, UiKit.Accent, 30, 22, 300, 40);
            UiKit.LabelAt(shopPanel, "“까악 — 좋은 물건 있습니다”", 19, UiKit.TextDim, 30, 62, 380, 28);
            var close = UiKit.MakeButton(shopPanel, "X", UiKit.Panel, UiKit.Text, 22, ToggleShop);
            UiKit.Place((RectTransform)close.transform, 430, 18, 44, 44);

            float y = 110f;
            foreach (var item in cfg.shop)
            {
                if (item.unlockLevel > level) continue;
                var row = UiKit.PanelRect(shopPanel, "Item_" + item.id, new Color(1f, 1f, 1f, 0.05f));
                UiKit.Place(row, 20, y, 450, 150);

                UiKit.LabelAt(row, $"<b>{item.name}</b>", 24, UiKit.Text, 18, 12, 300, 34, TextAlignmentOptions.TopLeft, true);
                UiKit.LabelAt(row, $"{item.price} G", 24, UiKit.Accent, 330, 12, 100, 34, TextAlignmentOptions.TopRight, true);
                UiKit.LabelAt(row, item.desc, 18, UiKit.TextDim, 18, 50, 414, 56, TextAlignmentOptions.TopLeft, true);

                string reason = buyBlockReason(item);
                if (reason == null)
                {
                    var it = item; // 클로저 캡처
                    var buy = UiKit.MakeButton(row, "구매", UiKit.Accent, UiKit.Ink, 20, () => OnBuy(it));
                    UiKit.Place((RectTransform)buy.transform, 330, 100, 100, 40);
                }
                else
                {
                    UiKit.LabelAt(row, reason, 18, UiKit.TextDim, 240, 110, 190, 30, TextAlignmentOptions.TopRight, true);
                }
                y += 160f;
            }

            UiKit.LabelAt(shopPanel, $"보유:  <b>{getGold()} G</b>", 24, UiKit.Text, 30, y + 8, 420, 36);
        }

        private void OnBuy(NightShopItem item)
        {
            string err = tryBuy(item);
            if (err != null)
            {
                showHint(err, 1.8f);
                return;
            }
            showHint($"<b>{item.name}</b> 구매!", 2.0f);
            // 패널 갱신 (품절 표시·보유 골드)
            Destroy(shopPanel.gameObject);
            shopPanel = null;
            BuildShopPanel();
        }

        // ─── 취침 ───

        private void Sleep()
        {
            if (sleeping) return;
            sleeping = true;
            if (!paid)
            {
                addGold(wage);
                paid = true;
                showHint($"월급봉투를 챙기는 걸 깜빡할 뻔 — <b>{wage}G</b> 자동 입금", 2.2f);
            }
            Sfx.Swish();
            StartCoroutine(SleepCo());
        }

        private IEnumerator SleepCo()
        {
            yield return new WaitForSeconds(0.45f);
            onSleep?.Invoke();
        }

        public void Cleanup()
        {
            // 방 소품 핸들러 해제 (방 자체는 세션 동안 유지)
            Wire(room.Bed, null);
            Wire(room.Slime, null);
            Wire(room.Window, null);
            Wire(room.Poster, null);
            Wire(room.DiceCup, null);
            Wire(room.Crow, null);
            if (paper != null) Destroy(paper.gameObject);
            if (envelope != null) Destroy(envelope.gameObject);
            if (slip != null) Destroy(slip.gameObject);
            if (shopPanel != null) Destroy(shopPanel.gameObject);
            Destroy(gameObject);
        }
    }
}
