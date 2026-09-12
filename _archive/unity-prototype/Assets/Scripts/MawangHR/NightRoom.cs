using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MawangHR
{
    /// 방 소품 클릭 핸들러 — NightFlow가 밤마다 콜백을 갈아끼운다.
    public class RoomProp : MonoBehaviour, IPointerClickHandler
    {
        public Action onClick;
        public void OnPointerClick(PointerEventData e)
        {
            if (e.button == PointerEventData.InputButton.Left) onClick?.Invoke();
        }
    }

    /// 밤 파트 무대 — 마왕성 기숙사 "내 방" 그레이박스.
    /// 책상 무대에서 멀리(z+30) 지어두고, 밤마다 카메라만 옮겨온다 (씬 전환 없음).
    /// 방은 세션 동안 살아있다 — 산 장식·커진 슬라임이 다음 밤에도 그대로 (방 = 성장의 거울).
    public class NightRoom : MonoBehaviour
    {
        public Camera Cam { get; private set; }
        public Vector3 TableTop { get; private set; }    // 신문·월급봉투가 놓이는 탁자 윗면 중심
        public Transform Bed { get; private set; }        // 클릭 = 취침
        public Transform Slime { get; private set; }      // 애완 슬라임 (귀여움 담당)
        public Transform DiceCup { get; private set; }    // 미니게임 티저 (직급 잠금)
        public Transform Crow { get; private set; }       // 까마귀 상점 (레벨 2 언락)
        public Transform Window { get; private set; }
        public RectTransform Poster { get; private set; } // 「이달의 사원」
        public bool Built { get; private set; }

        private static readonly Vector3 C = new Vector3(0f, 0f, 30f); // 방 중심 (책상에서 멀리)

        private Transform shelfBoard;
        private int shelfCount;
        private Vector3 slimeHome;
        private Vector3 slimeBaseScale;
        private float slimeGrow = 1f;
        private Light lamp;
        private Coroutine slimeCo;

        private static Material Mat(Color c)
        {
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.SetColor("_BaseColor", c);
            return m;
        }

        private GameObject Prim(PrimitiveType type, string name, Vector3 pos, Vector3 scale, Color c,
            bool clickable = false, Transform parent = null)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent == null ? transform : parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material = Mat(c);
            if (!clickable) Destroy(go.GetComponent<Collider>()); // 클릭 대상만 콜라이더 유지 (레이어 0)
            return go;
        }

        private GameObject Box(string name, Vector3 pos, Vector3 scale, Color c, bool clickable = false)
            => Prim(PrimitiveType.Cube, name, pos, scale, c, clickable);

        public void Build(Camera cam)
        {
            Cam = cam;

            // ─ 구조: 바닥 + 벽 3면 (앞은 카메라 쪽 개방) ─
            var wall = new Color(0.17f, 0.13f, 0.13f);
            Box("Floor", C + new Vector3(0, -0.05f, 0), new Vector3(4.4f, 0.1f, 4.4f), new Color(0.22f, 0.15f, 0.11f));
            Box("WallBack", C + new Vector3(0, 1.55f, 2.15f), new Vector3(4.4f, 3.3f, 0.2f), wall);
            Box("WallL", C + new Vector3(-2.2f, 1.55f, 0), new Vector3(0.2f, 3.3f, 4.4f), wall);
            Box("WallR", C + new Vector3(2.2f, 1.55f, 0), new Vector3(0.2f, 3.3f, 4.4f), wall);
            Box("Rug", C + new Vector3(0.1f, 0.006f, -0.15f), new Vector3(2.0f, 0.012f, 1.4f), new Color(0.34f, 0.15f, 0.19f));

            // ─ 침대 (클릭 = 취침) ─
            var bed = Box("Bed", C + new Vector3(-1.35f, 0.26f, 0.75f), new Vector3(1.3f, 0.5f, 2.1f),
                new Color(0.30f, 0.20f, 0.15f), clickable: true);
            Bed = bed.transform;
            Box("Blanket", C + new Vector3(-1.35f, 0.545f, 1.1f), new Vector3(1.22f, 0.1f, 1.35f), new Color(0.36f, 0.22f, 0.34f));
            Box("Pillow", C + new Vector3(-1.35f, 0.56f, 0.05f), new Vector3(0.78f, 0.12f, 0.42f), new Color(0.82f, 0.78f, 0.68f));

            // ─ 탁자 (신문·월급봉투 자리) + 양초 ─
            Box("Table", C + new Vector3(0, 0.36f, -0.5f), new Vector3(1.1f, 0.72f, 0.62f), new Color(0.30f, 0.21f, 0.13f));
            TableTop = C + new Vector3(0, 0.725f, -0.5f);
            Prim(PrimitiveType.Cylinder, "Candle", C + new Vector3(-0.44f, 0.79f, -0.6f),
                new Vector3(0.07f, 0.065f, 0.07f), new Color(0.9f, 0.85f, 0.7f));

            // ─ 창문 (뒷벽) + 달 + 박쥐 + 창턱 ─
            Box("WindowFrame", C + new Vector3(0.95f, 1.72f, 2.04f), new Vector3(1.18f, 1.18f, 0.08f), new Color(0.12f, 0.09f, 0.08f));
            var glass = Box("WindowGlass", C + new Vector3(0.95f, 1.72f, 2.0f), new Vector3(1.02f, 1.02f, 0.05f),
                new Color(0.09f, 0.11f, 0.22f), clickable: true);
            Window = glass.transform;
            Prim(PrimitiveType.Sphere, "Moon", C + new Vector3(1.14f, 1.94f, 1.955f), Vector3.one * 0.24f, new Color(0.93f, 0.90f, 0.72f));
            Box("Bat1", C + new Vector3(0.72f, 1.86f, 1.95f), new Vector3(0.11f, 0.03f, 0.02f), Color.black);
            Box("Bat2", C + new Vector3(0.86f, 1.60f, 1.95f), new Vector3(0.08f, 0.025f, 0.02f), Color.black);
            Box("Sill", C + new Vector3(0.95f, 1.10f, 1.98f), new Vector3(1.25f, 0.06f, 0.28f), new Color(0.12f, 0.09f, 0.08f));

            // ─ 선반 (뒷벽 좌측) — 상점에서 산 장식이 여기 진열된다 ─
            shelfBoard = Box("Shelf", C + new Vector3(-1.35f, 1.5f, 1.95f), new Vector3(1.15f, 0.07f, 0.38f),
                new Color(0.28f, 0.20f, 0.13f)).transform;

            // ─ 포스터 「이달의 사원」 (뒷벽 중앙) ─
            Poster = UiKit.MakeWorldCanvas("Poster", 300, 420, 0.0015f, cam);
            Poster.SetParent(transform, false);
            Poster.position = C + new Vector3(-0.32f, 1.72f, 2.03f);
            Poster.rotation = Quaternion.identity; // 카메라가 +z를 보므로 그대로 정면
            Poster.gameObject.AddComponent<UnityEngine.UI.Image>().color = UiKit.Paper;
            UiKit.LabelAt(Poster, "<b>이달의 사원</b>", 34, UiKit.Ink, 0, 22, 300, 44, TextAlignmentOptions.Center, true);
            var face = UiKit.PanelRect(Poster, "Face", new Color(0.85f, 0.83f, 0.75f)); // 해골색
            UiKit.Place(face, 100, 80, 100, 110);
            UiKit.LabelAt(Poster, "스켈레톤 인턴", 26, UiKit.Ink, 0, 210, 300, 36, TextAlignmentOptions.Center, true);
            UiKit.LabelAt(Poster, "(3개월 연속)", 20, UiKit.InkDim, 0, 250, 300, 30, TextAlignmentOptions.Center, true);
            UiKit.LabelAt(Poster, "“뼈가 시리도록 성실”", 19, UiKit.InkDim, 0, 330, 300, 30, TextAlignmentOptions.Center, true);

            // ─ 애완 슬라임 (바닥 — 귀여움 담당) ─
            slimeHome = C + new Vector3(0.82f, 0.15f, -0.32f);
            slimeBaseScale = new Vector3(0.36f, 0.26f, 0.36f);
            var slime = Prim(PrimitiveType.Sphere, "PetSlime", slimeHome, slimeBaseScale,
                new Color(0.35f, 0.68f, 0.55f), clickable: true);
            Slime = slime.transform;

            // ─ 스툴 + 주사위 컵 (미니게임 티저 — 직급 잠금) ─
            Prim(PrimitiveType.Cylinder, "Stool", C + new Vector3(1.55f, 0.21f, -0.2f), new Vector3(0.46f, 0.21f, 0.46f),
                new Color(0.24f, 0.17f, 0.11f));
            var cup = Prim(PrimitiveType.Cylinder, "DiceCup", C + new Vector3(1.55f, 0.53f, -0.2f),
                new Vector3(0.17f, 0.115f, 0.17f), new Color(0.42f, 0.28f, 0.16f), clickable: true);
            DiceCup = cup.transform;

            // ─ 문 (오른벽 — 장식) ─
            Box("Door", C + new Vector3(2.09f, 1.05f, -1.15f), new Vector3(0.12f, 2.1f, 0.92f), new Color(0.20f, 0.13f, 0.09f));

            // ─ 까마귀 (창턱 — 레벨 2부터 등장) ─
            var crowRoot = new GameObject("Crow");
            crowRoot.transform.SetParent(transform, false);
            crowRoot.transform.localPosition = C + new Vector3(0.58f, 1.21f, 1.93f);
            var crowCol = crowRoot.AddComponent<SphereCollider>();
            crowCol.radius = 0.14f;
            Prim(PrimitiveType.Sphere, "Body", new Vector3(0, 0, 0), new Vector3(0.17f, 0.14f, 0.17f),
                new Color(0.09f, 0.09f, 0.12f), false, crowRoot.transform);
            Prim(PrimitiveType.Sphere, "Head", new Vector3(-0.05f, 0.11f, -0.03f), Vector3.one * 0.10f,
                new Color(0.09f, 0.09f, 0.12f), false, crowRoot.transform);
            Prim(PrimitiveType.Cube, "Beak", new Vector3(-0.10f, 0.10f, -0.06f), new Vector3(0.06f, 0.025f, 0.025f),
                new Color(0.85f, 0.6f, 0.2f), false, crowRoot.transform);
            Crow = crowRoot.transform;
            crowRoot.SetActive(false);

            // ─ 방 램프 (양초 위 포인트라이트 + 플리커) ─
            var lampGo = new GameObject("RoomLamp");
            lampGo.transform.SetParent(transform, false);
            lampGo.transform.localPosition = C + new Vector3(-0.42f, 1.05f, -0.55f);
            lamp = lampGo.AddComponent<Light>();
            lamp.type = LightType.Point;
            lamp.color = new Color(1f, 0.76f, 0.5f);
            lamp.intensity = 1.7f;
            lamp.range = 5.5f; // 책상(30 밖)까지 안 닿음 — 상시 켜둬도 무해
            StartCoroutine(Flicker());

            Built = true;
        }

        /// 카메라를 방 뷰로 (즉시 컷 — 전환 연출은 GameFlow의 2D 화면이 덮는다)
        public void EnterCamera()
        {
            Cam.transform.position = C + new Vector3(0f, 1.6f, -2.35f);
            Cam.transform.rotation = Quaternion.LookRotation(
                (C + new Vector3(0f, 0.85f, 0.5f)) - Cam.transform.position);
            Cam.fieldOfView = 48f;
        }

        public void SetCrowVisible(bool v)
        {
            if (Crow != null) Crow.gameObject.SetActive(v);
        }

        /// 상점에서 산 장식을 선반에 진열 (세션 동안 유지 — 방이 점점 내 것이 된다)
        public void AddShelfDeco(string id)
        {
            Vector3 pos = shelfBoard.position + new Vector3(-0.38f + shelfCount * 0.32f, 0.16f, 0f);
            shelfCount++;
            switch (id)
            {
                case "statue": // 미니 마왕 조각상 — 보라 몸통 + 뿔
                    Box("Deco_statue", pos, new Vector3(0.13f, 0.22f, 0.10f), new Color(0.30f, 0.12f, 0.36f));
                    Box("Deco_statue_hornL", pos + new Vector3(-0.05f, 0.14f, 0f), new Vector3(0.03f, 0.07f, 0.03f), new Color(0.85f, 0.75f, 0.4f));
                    Box("Deco_statue_hornR", pos + new Vector3(0.05f, 0.14f, 0f), new Vector3(0.03f, 0.07f, 0.03f), new Color(0.85f, 0.75f, 0.4f));
                    break;
                case "plant": // 맨드레이크 화분 — 화분 + 잎
                    Prim(PrimitiveType.Cylinder, "Deco_plant_pot", pos, new Vector3(0.13f, 0.07f, 0.13f), new Color(0.55f, 0.30f, 0.18f));
                    Prim(PrimitiveType.Sphere, "Deco_plant_leaf", pos + new Vector3(0f, 0.13f, 0f), Vector3.one * 0.16f, new Color(0.30f, 0.55f, 0.25f));
                    break;
                default: // 기타 — 금색 트로피 박스
                    Box("Deco_" + id, pos, new Vector3(0.12f, 0.16f, 0.12f), new Color(0.8f, 0.65f, 0.25f));
                    break;
            }
        }

        public void SlimeBounce()
        {
            if (slimeCo != null) StopCoroutine(slimeCo);
            slimeCo = StartCoroutine(SlimeBounceCo());
        }

        /// 간식: 슬라임이 조금 커진다 (상한 있음 — 방을 삼키면 곤란)
        public void FeedSlime()
        {
            slimeGrow = Mathf.Min(slimeGrow + 0.16f, 1.7f);
            SlimeBounce();
        }

        private IEnumerator SlimeBounceCo()
        {
            Vector3 baseScale = slimeBaseScale * slimeGrow;
            float t = 0f;
            const float dur = 0.55f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = t / dur;
                float squash = 1f + Mathf.Sin(k * Mathf.PI * 3f) * 0.22f * (1f - k); // 출렁출렁 감쇠
                Slime.localScale = new Vector3(baseScale.x / squash, baseScale.y * squash, baseScale.z / squash);
                Slime.position = slimeHome + Vector3.up * Mathf.Abs(Mathf.Sin(k * Mathf.PI * 2f)) * 0.06f * (1f - k);
                yield return null;
            }
            Slime.localScale = baseScale;
            Slime.position = slimeHome;
            slimeCo = null;
        }

        private IEnumerator Flicker()
        {
            while (true)
            {
                lamp.intensity = 1.7f + Mathf.PerlinNoise(Time.time * 7f, 0.31f) * 0.5f;
                yield return null;
            }
        }
    }
}
