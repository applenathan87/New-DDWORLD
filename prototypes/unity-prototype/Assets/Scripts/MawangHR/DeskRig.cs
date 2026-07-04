using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MawangHR
{
    /// 3D 데스크 무대 — 책상·촛불(플리커)·서류(월드 캔버스)·서류 더미·카메라 2단 연출.
    /// 전부 프리미티브 그레이박스. 무드의 핵심 = 촛불 포인트 라이트.
    public class DeskRig : MonoBehaviour
    {
        public const float DeskTop = 0.75f;

        public Camera Cam { get; private set; }
        public RectTransform ResumeCanvas { get; private set; } // 이력서 종이 (마킹·도장 대상)
        public RectTransform JdCanvas { get; private set; }     // 공문/JD 종이
        public Vector3 ResumeHome { get; private set; }
        public Quaternion ResumeHomeRot { get; private set; }
        public Vector3 JdHome { get; private set; }
        public Quaternion JdHomeRot { get; private set; }

        // 카메라 포즈 (그레이박스 튜닝값 — 여기 숫자만 만지면 됨)
        private static readonly Vector3 DeskCamPos = new Vector3(0f, 1.60f, -1.25f);
        private static readonly Vector3 DeskCamTarget = new Vector3(0f, DeskTop, -0.05f);
        private const float DeskFov = 46f;
        private static readonly Vector3 ReadCamPos = new Vector3(0.05f, 1.15f, -0.50f);
        private static readonly Vector3 ReadCamTarget = new Vector3(0.05f, DeskTop, -0.02f);
        private const float ReadFov = 50f;

        private Light candle;
        private Transform pendingRoot, doneRoot;
        private int doneCount;
        private Coroutine camCo;
        private Transform quill, quillFeather;
        private Vector3 quillHome;
        private Quaternion featherBaseRot;
        private Coroutine quillCo, twitchCo;

        private static Material Mat(Color c)
        {
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.SetColor("_BaseColor", c);
            return m;
        }

        private GameObject Box(string name, Vector3 pos, Vector3 scale, Color c, Transform parent = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent == null ? transform : parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material = Mat(c);
            // 환경 콜라이더 제거 — "허공 클릭 = 종이 내려놓기" 판정을 위해 (도장만 콜라이더 보유)
            Destroy(go.GetComponent<Collider>());
            return go;
        }

        public void Build(Camera cam)
        {
            Cam = cam;

            // 방 (어두운 석실)
            Box("Floor", new Vector3(0, -0.02f, 0.6f), new Vector3(7, 0.04f, 6), new Color(0.15f, 0.11f, 0.09f));
            Box("BackWall", new Vector3(0, 1.5f, 1.6f), new Vector3(7, 3.2f, 0.1f), new Color(0.14f, 0.12f, 0.11f));

            // 책상
            var wood = new Color(0.34f, 0.23f, 0.15f);
            Box("DeskTop", new Vector3(0, DeskTop - 0.04f, 0), new Vector3(2.3f, 0.08f, 1.25f), wood);
            Box("LegFL", new Vector3(-1.05f, 0.355f, -0.55f), new Vector3(0.08f, 0.71f, 0.08f), wood);
            Box("LegFR", new Vector3(1.05f, 0.355f, -0.55f), new Vector3(0.08f, 0.71f, 0.08f), wood);
            Box("LegBL", new Vector3(-1.05f, 0.355f, 0.55f), new Vector3(0.08f, 0.71f, 0.08f), wood);
            Box("LegBR", new Vector3(1.05f, 0.355f, 0.55f), new Vector3(0.08f, 0.71f, 0.08f), wood);

            // 촛불 (무드의 심장)
            var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.name = "Candle";
            body.transform.SetParent(transform, false);
            body.transform.localPosition = new Vector3(-0.95f, DeskTop + 0.07f, -0.35f);
            body.transform.localScale = new Vector3(0.05f, 0.07f, 0.05f);
            body.GetComponent<Renderer>().material = Mat(new Color(0.92f, 0.88f, 0.78f));
            Destroy(body.GetComponent<Collider>());

            var flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flame.name = "Flame";
            flame.transform.SetParent(transform, false);
            flame.transform.localPosition = new Vector3(-0.95f, DeskTop + 0.17f, -0.35f);
            flame.transform.localScale = Vector3.one * 0.03f;
            flame.GetComponent<Renderer>().material = Mat(new Color(1f, 0.85f, 0.35f));
            Destroy(flame.GetComponent<Collider>());

            var lightGo = new GameObject("CandleLight");
            lightGo.transform.SetParent(transform, false);
            lightGo.transform.localPosition = new Vector3(-0.95f, DeskTop + 0.35f, -0.35f);
            candle = lightGo.AddComponent<Light>();
            candle.type = LightType.Point;
            candle.color = new Color(1f, 0.72f, 0.42f);
            candle.range = 4.5f;
            candle.intensity = 2.4f;
            candle.shadows = LightShadows.Soft;
            StartCoroutine(Flicker());

            // 서류: 이력서 (중앙) + 공문/JD (왼쪽)
            ResumeCanvas = UiKit.MakeWorldCanvas("ResumePaper", 900, 1240, 0.000444f, cam);
            ResumeCanvas.SetParent(transform, false);
            ResumeHomeRot = Quaternion.Euler(90, 0, -2f);
            ResumeCanvas.localRotation = ResumeHomeRot;
            ResumeHome = new Vector3(0.14f, DeskTop + 0.004f, 0.0f);
            ResumeCanvas.localPosition = ResumeHome;
            ResumeCanvas.gameObject.AddComponent<Image>().color = UiKit.Paper;

            JdCanvas = UiKit.MakeWorldCanvas("JdPaper", 620, 900, 0.000419f, cam);
            JdCanvas.SetParent(transform, false);
            JdHomeRot = Quaternion.Euler(90, 0, 5f);
            JdCanvas.localRotation = JdHomeRot;
            JdHome = new Vector3(-0.36f, DeskTop + 0.003f, 0.02f);
            JdCanvas.localPosition = JdHome;
            JdCanvas.gameObject.AddComponent<Image>().color = UiKit.PaperShade;

            // 서류 더미 (대기/완료)
            pendingRoot = new GameObject("Pending").transform;
            pendingRoot.SetParent(transform, false);
            pendingRoot.localPosition = new Vector3(-0.78f, DeskTop, -0.33f);
            doneRoot = new GameObject("Done").transform;
            doneRoot.SetParent(transform, false);
            doneRoot.localPosition = new Vector3(0.62f, DeskTop, 0.30f);

            // 깃펜 (마킹 도구 — 종이를 들면 손 위치로 날아옴)
            var quillRoot = new GameObject("Quill");
            quillRoot.transform.SetParent(transform, false);
            quillHome = new Vector3(0.74f, DeskTop, -0.16f);
            quillRoot.transform.localPosition = quillHome;
            quill = quillRoot.transform;

            var inkpot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            inkpot.name = "Inkpot";
            inkpot.transform.SetParent(quill, false);
            inkpot.transform.localPosition = new Vector3(0, 0.03f, 0);
            inkpot.transform.localScale = new Vector3(0.05f, 0.03f, 0.05f);
            inkpot.GetComponent<Renderer>().material = Mat(new Color(0.12f, 0.10f, 0.09f));
            Destroy(inkpot.GetComponent<Collider>());

            var feather = GameObject.CreatePrimitive(PrimitiveType.Cube);
            feather.name = "Feather";
            feather.transform.SetParent(quill, false);
            feather.transform.localPosition = new Vector3(0, 0.15f, 0);
            featherBaseRot = Quaternion.Euler(18f, 0f, -12f);
            feather.transform.localRotation = featherBaseRot;
            feather.transform.localScale = new Vector3(0.02f, 0.18f, 0.045f);
            feather.GetComponent<Renderer>().material = Mat(new Color(0.92f, 0.92f, 0.88f));
            Destroy(feather.GetComponent<Collider>());
            quillFeather = feather.transform;

            // 카메라 초기 상태
            cam.fieldOfView = DeskFov;
            cam.transform.position = DeskCamPos;
            cam.transform.rotation = Quaternion.LookRotation(DeskCamTarget - DeskCamPos);
        }

        /// 종이를 들면 깃펜이 손 위치(화면 우하단)로 날아와 대기
        public void SetQuillHeld(bool held)
        {
            if (quill == null) return;
            if (quillCo != null) StopCoroutine(quillCo);
            Vector3 pos;
            Quaternion rot;
            if (held)
            {
                Vector3 camPos = DeskCamPos + DeskFwd * 0.09f;
                Quaternion camRot = Quaternion.LookRotation(DeskCamTarget - DeskCamPos);
                pos = camPos + camRot * new Vector3(0.30f, -0.24f, 0.55f);
                rot = camRot * Quaternion.Euler(40f, 0f, -35f); // 필기 각도
            }
            else
            {
                pos = quillHome;
                rot = Quaternion.identity;
            }
            quillCo = StartCoroutine(TweenQuill(pos, rot, 0.25f));
        }

        private IEnumerator TweenQuill(Vector3 pos, Quaternion rot, float dur)
        {
            Vector3 fromP = quill.position;
            Quaternion fromR = quill.rotation;
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / dur);
                quill.position = Vector3.Lerp(fromP, pos, k);
                quill.rotation = Quaternion.Slerp(fromR, rot, k);
                yield return null;
            }
            quill.position = pos;
            quill.rotation = rot;
            quillCo = null;
        }

        /// 마킹 순간 깃털이 사각 긋는 트윗치
        public void QuillTwitch()
        {
            if (quillFeather == null) return;
            if (twitchCo != null) StopCoroutine(twitchCo);
            twitchCo = StartCoroutine(TwitchCo());
        }

        private IEnumerator TwitchCo()
        {
            Quaternion strokeRot = featherBaseRot * Quaternion.Euler(0f, 0f, -16f);
            float t = 0f;
            while (t < 0.05f)
            {
                t += Time.deltaTime;
                quillFeather.localRotation = Quaternion.Slerp(featherBaseRot, strokeRot, t / 0.05f);
                yield return null;
            }
            t = 0f;
            while (t < 0.08f)
            {
                t += Time.deltaTime;
                quillFeather.localRotation = Quaternion.Slerp(strokeRot, featherBaseRot, t / 0.08f);
                yield return null;
            }
            quillFeather.localRotation = featherBaseRot;
            twitchCo = null;
        }

        private IEnumerator Flicker()
        {
            while (true)
            {
                candle.intensity = 2.1f + Mathf.PerlinNoise(Time.time * 6f, 0.37f) * 1.0f;
                yield return null;
            }
        }

        // ─── 서류 더미 ───

        public void SetPending(int n)
        {
            foreach (Transform c in pendingRoot) Destroy(c.gameObject);
            for (int i = 0; i < Mathf.Min(n, 8); i++)
            {
                var sheet = Box("Sheet", new Vector3(0, 0.004f + i * 0.007f, 0),
                    new Vector3(0.30f, 0.006f, 0.42f), UiKit.PaperShade, pendingRoot);
                sheet.transform.localRotation = Quaternion.Euler(0, Random.Range(-6f, 6f), 0);
                Destroy(sheet.GetComponent<Collider>());
            }
        }

        public void AddDone()
        {
            var sheet = Box("Sheet", new Vector3(0, 0.004f + doneCount * 0.007f, 0),
                new Vector3(0.30f, 0.006f, 0.42f), UiKit.Paper, doneRoot);
            sheet.transform.localRotation = Quaternion.Euler(0, Random.Range(-9f, 9f), 0);
            Destroy(sheet.GetComponent<Collider>());
            doneCount++;
        }

        public void ClearDone()
        {
            foreach (Transform c in doneRoot) Destroy(c.gameObject);
            doneCount = 0;
        }

        // ─── 카메라 ───

        private static Vector3 DeskFwd => (DeskCamTarget - DeskCamPos).normalized;

        public void TweenToDesk() => TweenCam(DeskCamPos, DeskCamTarget, DeskFov, 0.35f);

        /// 종이를 들었을 때: 살짝만 밀어 들어가는 푸시인 (몬스터·책상은 프레임 유지)
        public void TweenToHold() => TweenCam(DeskCamPos + DeskFwd * 0.09f, DeskCamTarget, 44f, 0.3f);

        /// 들어 올린 종이의 목표 포즈 — 푸시인 완료 시점의 카메라 기준으로 계산 (결정적).
        /// rightOffset: 화면 좌우 슬롯 / yaw: 안쪽으로 트는 각도 (두 장 나란히 들기용)
        public void GetHeldPose(float distance, float upOffset, float rightOffset, float yaw,
            out Vector3 pos, out Quaternion rot)
        {
            Vector3 camPos = DeskCamPos + DeskFwd * 0.09f;
            Quaternion camRot = Quaternion.LookRotation(DeskCamTarget - DeskCamPos);
            Vector3 f = camRot * Vector3.forward;
            Vector3 u = camRot * Vector3.up;
            Vector3 r = camRot * Vector3.right;
            pos = camPos + f * distance + u * upOffset + r * rightOffset;
            // 캔버스 앞면(-z)이 카메라를 향하도록 + 목업풍 살짝 젖힘
            rot = Quaternion.LookRotation(f, u) * Quaternion.Euler(-8f, yaw, 2f);
        }

        private void TweenCam(Vector3 pos, Vector3 target, float fov, float dur)
        {
            if (camCo != null) StopCoroutine(camCo);
            camCo = StartCoroutine(TweenCamCo(pos, Quaternion.LookRotation(target - pos), fov, dur));
        }

        private IEnumerator TweenCamCo(Vector3 pos, Quaternion rot, float fov, float dur)
        {
            Vector3 fromP = Cam.transform.position;
            Quaternion fromR = Cam.transform.rotation;
            float fromF = Cam.fieldOfView;
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / dur);
                Cam.transform.position = Vector3.Lerp(fromP, pos, k);
                Cam.transform.rotation = Quaternion.Slerp(fromR, rot, k);
                Cam.fieldOfView = Mathf.Lerp(fromF, fov, k);
                yield return null;
            }
            Cam.transform.position = pos;
            Cam.transform.rotation = rot;
            Cam.fieldOfView = fov;
            camCo = null;
        }

        /// 도장 쾅 순간의 카메라 흔들림 (책상이 울리는 느낌)
        public IEnumerator CamShake()
        {
            Vector3 basePos = Cam.transform.position;
            float t = 0f;
            const float dur = 0.16f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float damp = 1f - t / dur;
                Cam.transform.position = basePos + new Vector3(
                    Random.Range(-0.008f, 0.008f),
                    Random.Range(-0.006f, 0.006f), 0) * damp * 2f;
                yield return null;
            }
            Cam.transform.position = basePos;
        }

        // ─── 도장 ───

        public StampDraggable3D MakeStamp(bool pass, string label, Color color, Vector3 home,
            System.Func<bool> canStamp, System.Action<bool, Vector2> onSlam, System.Action onBlocked)
        {
            var root = new GameObject(pass ? "StampPass" : "StampFail");
            root.transform.SetParent(transform, false);
            root.transform.localPosition = home;

            var col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 0.08f, 0);
            col.size = new Vector3(0.16f, 0.18f, 0.16f);

            var baseBox = Box("Base", new Vector3(0, 0.025f, 0), new Vector3(0.13f, 0.05f, 0.13f), color, root.transform);
            Destroy(baseBox.GetComponent<Collider>());
            var handle = Box("Handle", new Vector3(0, 0.105f, 0), new Vector3(0.05f, 0.11f, 0.05f),
                new Color(0.26f, 0.17f, 0.12f), root.transform);
            Destroy(handle.GetComponent<Collider>());

            // 도장 앞 라벨 (책상에 붙은 이름표)
            var tag = UiKit.MakeWorldCanvas("Tag", 240, 80, 0.0005f, Cam);
            tag.SetParent(transform, false);
            tag.localRotation = Quaternion.Euler(90, 0, 0);
            tag.localPosition = new Vector3(home.x, DeskTop + 0.002f, home.z - 0.14f);
            var tagText = UiKit.Label(tag, label, 44, color, TMPro.TextAlignmentOptions.Center);
            UiKit.Fill((RectTransform)tagText.transform);
            tagText.raycastTarget = false;

            var drag = root.AddComponent<StampDraggable3D>();
            drag.Init(pass, Cam, ResumeCanvas, DeskTop, canStamp, onSlam, onBlocked);
            return drag;
        }
    }
}
