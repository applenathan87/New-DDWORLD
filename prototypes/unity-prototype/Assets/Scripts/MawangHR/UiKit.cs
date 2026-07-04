using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MawangHR
{
    /// uGUI를 코드로 조립하는 도우미. 레퍼런스 해상도 1920x1080 기준 절대 배치.
    /// 톤 = 촛불 켜진 마왕성 사무실 (짙은 갈색 + 양피지 + 주황 액센트).
    public static class UiKit
    {
        public static TMP_FontAsset Font;

        public static readonly Color Bg = Hex("#17110E");         // 사무실 어둠
        public static readonly Color Panel = Hex("#2A201A");      // 목재 패널
        public static readonly Color PanelLight = Hex("#3A2C22");
        public static readonly Color Paper = Hex("#E8D9B5");      // 양피지
        public static readonly Color PaperShade = Hex("#D8C49A");
        public static readonly Color Ink = Hex("#2E2418");        // 종이 위 잉크
        public static readonly Color InkDim = Hex("#6B5A42");
        public static readonly Color Text = Hex("#EADFC8");       // 어두운 배경 위 글씨
        public static readonly Color TextDim = Hex("#A6947C");
        public static readonly Color Accent = Hex("#D98F3E");     // 촛불 주황
        public static readonly Color Approve = Hex("#3E7D4E");    // 통과 도장
        public static readonly Color Reject = Hex("#9E3B32");     // 탈락 도장
        public static readonly Color StampInk = Hex("#B03A2E");   // 도장 잉크(빨강)

        public static void Init(TMP_FontAsset font) { Font = font; }

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }

        public static Canvas MakeCanvas()
        {
            var go = new GameObject("Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            // 폭 고정(match=width): 이 코드베이스는 좌상단 절대 배치(Place)라 폭이 줄면 우측 UI가 잘린다.
            // 0.5(타협값)였을 땐 16:10에서 폭이 1920→약 1821로 줄어 확정 버튼·메모 패드 우측이 화면 밖으로 나갔음.
            // 폭을 1920으로 고정하면 16:10에선 세로 여백만 늘어남. (21:9 등 초광폭은 미지원 — 하단 잘림)
            scaler.matchWidthOrHeight = 0f;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        /// 3D 공간에 놓이는 종이/라벨용 월드 스페이스 캔버스.
        /// 픽셀 단위로 레이아웃하고 scale로 실제 크기(m)를 정한다.
        public static RectTransform MakeWorldCanvas(string name, float widthPx, float heightPx, float scale, Camera cam)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = cam;
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(widthPx, heightPx);
            rt.localScale = Vector3.one * scale;
            go.AddComponent<GraphicRaycaster>();
            return rt;
        }

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        public static RectTransform PanelRect(Transform parent, string name, Color color)
        {
            var rt = Rect(name, parent);
            rt.gameObject.AddComponent<Image>().color = color;
            return rt;
        }

        /// 좌상단 원점(x,y) + 크기(w,h)로 절대 배치 (1920x1080 기준).
        public static void Place(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
        }

        public static void Fill(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static TextMeshProUGUI Label(Transform parent, string text, float size, Color color,
            TextAlignmentOptions align = TextAlignmentOptions.TopLeft, bool fit = false)
        {
            var rt = Rect("Label", parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            if (Font != null) t.font = Font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.richText = true;
            if (fit)
            {
                // 박스를 넘치면 글자를 줄여서 맞춘다 (레이아웃 오버플로 방지)
                t.enableAutoSizing = true;
                t.fontSizeMax = size;
                t.fontSizeMin = Mathf.Max(11f, size * 0.5f);
            }
            return t;
        }

        public static TextMeshProUGUI LabelAt(Transform parent, string text, float size, Color color,
            float x, float y, float w, float h, TextAlignmentOptions align = TextAlignmentOptions.TopLeft,
            bool fit = false)
        {
            var t = Label(parent, text, size, color, align, fit);
            Place((RectTransform)t.transform, x, y, w, h);
            return t;
        }

        public static Button MakeButton(Transform parent, string label, Color bg, Color fg, float fontSize,
            UnityAction onClick)
        {
            var rt = PanelRect(parent, "Btn_" + label, bg);
            var img = rt.GetComponent<Image>();
            var b = rt.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            var colors = b.colors;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            b.colors = colors;
            if (onClick != null) b.onClick.AddListener(onClick);
            var t = Label(rt, label, fontSize, fg, TextAlignmentOptions.Center);
            Fill((RectTransform)t.transform);
            return b;
        }
    }
}
