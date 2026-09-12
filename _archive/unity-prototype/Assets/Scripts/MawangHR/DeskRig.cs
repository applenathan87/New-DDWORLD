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
        public Transform Orb { get; private set; }           // 수정구 구체 (스케줄링 통화 기준점)
        public ThrowableProp OrbProp { get; private set; }   // 수정구 던지기 (스케줄링 중 비활성화용)

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

        // 물리 반발 재질 (튕김) — 환경·소품 공용
        private static PhysicsMaterial bouncyMat;
        private static PhysicsMaterial BouncyMat
        {
            get
            {
                if (bouncyMat == null)
                    bouncyMat = new PhysicsMaterial("Bouncy")
                    {
                        bounciness = 0.45f,
                        bounceCombine = PhysicsMaterialCombine.Maximum,
                        dynamicFriction = 0.35f,
                        staticFriction = 0.4f,
                    };
                return bouncyMat;
            }
        }

        private GameObject Box(string name, Vector3 pos, Vector3 scale, Color c, Transform parent = null, bool solid = false)
            => Prim(PrimitiveType.Cube, name, pos, scale, c, parent, solid);

        private GameObject Prim(PrimitiveType type, string name, Vector3 pos, Vector3 scale, Color c,
            Transform parent = null, bool solid = false)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent == null ? transform : parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material = Mat(c);
            if (solid)
            {
                // 물리용 환경 콜라이더 — 단, 레이어 2로 빼서 포인터 레이캐스트에선 제외
                // ("허공 클릭 = 종이 내려놓기" 판정 유지, 던진 물건은 여기 부딪혀 튕김)
                go.layer = 2;
                go.GetComponent<Collider>().material = BouncyMat;
            }
            else
            {
                Destroy(go.GetComponent<Collider>());
            }
            return go;
        }

        /// 월드 캔버스 종이에 두께 슬랩을 붙인다 — 치수는 캔버스 픽셀 단위 (두께 9px ≈ 4mm).
        /// 캔버스 앞면(-z) 뒤로 1px 띄워 UI와 z-파이팅 방지. 콜라이더 없음 (클릭 판정 무관).
        private void AddPaperBody(RectTransform paper, float wPx, float hPx, Color edge)
        {
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "PaperBody";
            Destroy(body.GetComponent<Collider>());
            body.transform.SetParent(paper, false);
            body.transform.localScale = new Vector3(wPx, hPx, 9f);
            body.transform.localPosition = new Vector3(0, 0, 5.5f);
            body.GetComponent<Renderer>().material = Mat(edge);
        }

        /// 집어 던질 수 있는 소품 루트 생성 (자식으로 형태를 채운 뒤 Init 호출)
        private ThrowableProp MakeProp(string name, Vector3 home, Vector3 colCenter, Vector3 colSize,
            bool sphereCollider = false)
        {
            var root = new GameObject(name);
            root.transform.SetParent(transform, false);
            root.transform.localPosition = home;
            Collider col;
            if (sphereCollider)
            {
                var sc = root.AddComponent<SphereCollider>(); // 구형 = 진짜로 구른다
                sc.center = colCenter;
                sc.radius = colSize.x * 0.5f;
                col = sc;
            }
            else
            {
                var bc = root.AddComponent<BoxCollider>();
                bc.center = colCenter;
                bc.size = colSize;
                col = bc;
            }
            col.material = BouncyMat;
            return root.AddComponent<ThrowableProp>();
        }

        public void Build(Camera cam)
        {
            Cam = cam;

            // 방 (어두운 석실) — solid = 던진 물건이 부딪혀 튕기는 면
            Box("Floor", new Vector3(0, -0.02f, 0.6f), new Vector3(7, 0.04f, 6), new Color(0.15f, 0.11f, 0.09f), null, true);
            Box("BackWall", new Vector3(0, 1.5f, 1.6f), new Vector3(7, 3.2f, 0.1f), new Color(0.14f, 0.12f, 0.11f), null, true);

            // 책상
            var wood = new Color(0.34f, 0.23f, 0.15f);
            Box("DeskTop", new Vector3(0, DeskTop - 0.04f, 0), new Vector3(2.3f, 0.08f, 1.25f), wood, null, true);
            Box("LegFL", new Vector3(-1.05f, 0.355f, -0.55f), new Vector3(0.08f, 0.71f, 0.08f), wood, null, true);
            Box("LegFR", new Vector3(1.05f, 0.355f, -0.55f), new Vector3(0.08f, 0.71f, 0.08f), wood, null, true);
            Box("LegBL", new Vector3(-1.05f, 0.355f, 0.55f), new Vector3(0.08f, 0.71f, 0.08f), wood, null, true);
            Box("LegBR", new Vector3(1.05f, 0.355f, 0.55f), new Vector3(0.08f, 0.71f, 0.08f), wood, null, true);

            // 촛불 (무드의 심장) — 던질 수 있음. 빛도 같이 날아간다.
            var candleProp = MakeProp("CandleProp", new Vector3(-0.95f, DeskTop, -0.35f),
                new Vector3(0, 0.13f, 0), new Vector3(0.13f, 0.3f, 0.13f));
            Prim(PrimitiveType.Cylinder, "Candle", new Vector3(0, 0.07f, 0),
                new Vector3(0.05f, 0.07f, 0.05f), new Color(0.92f, 0.88f, 0.78f), candleProp.transform);
            Prim(PrimitiveType.Sphere, "Flame", new Vector3(0, 0.17f, 0),
                Vector3.one * 0.03f, new Color(1f, 0.85f, 0.35f), candleProp.transform);
            var lightGo = new GameObject("CandleLight");
            lightGo.transform.SetParent(candleProp.transform, false);
            lightGo.transform.localPosition = new Vector3(0, 0.35f, 0);
            candle = lightGo.AddComponent<Light>();
            candle.type = LightType.Point;
            candle.color = new Color(1f, 0.72f, 0.42f);
            candle.range = 4.5f;
            candle.intensity = 2.4f;
            candle.shadows = LightShadows.Soft;
            StartCoroutine(Flicker());
            candleProp.Init(cam, this);

            // 서류: 이력서 (중앙) + 공문/JD (왼쪽)
            // 캔버스 1274px = 이력서 템플릿(1054×1492) 비율에 맞춤
            ResumeCanvas = UiKit.MakeWorldCanvas("ResumePaper", 900, 1274, 0.000444f, cam);
            ResumeCanvas.SetParent(transform, false);
            ResumeHomeRot = Quaternion.Euler(90, 0, -2f);
            ResumeCanvas.localRotation = ResumeHomeRot;
            ResumeHome = new Vector3(0.14f, DeskTop + 0.004f, 0.0f);
            ResumeCanvas.localPosition = ResumeHome;
            var resumeBg = ResumeCanvas.gameObject.AddComponent<Image>();
            var resumeTmpl = UiKit.LoadSprite("MawangHR/resume_template.png");
            if (resumeTmpl != null) { resumeBg.sprite = resumeTmpl; resumeBg.color = Color.white; }
            else resumeBg.color = UiKit.Paper; // 템플릿 파일이 없으면 단색 폴백

            JdCanvas = UiKit.MakeWorldCanvas("JdPaper", 620, 900, 0.000419f, cam);
            JdCanvas.SetParent(transform, false);
            JdHomeRot = Quaternion.Euler(90, 0, 5f);
            JdCanvas.localRotation = JdHomeRot;
            JdHome = new Vector3(-0.36f, DeskTop + 0.003f, 0.02f);
            JdCanvas.localPosition = JdHome;
            JdCanvas.gameObject.AddComponent<Image>().color = UiKit.PaperShade;

            // 종이 두께 — 평면 캔버스 뒤에 얇은 슬랩 (들거나 기울었을 때 실물 종이처럼 보이게)
            AddPaperBody(ResumeCanvas, 860, 1230, new Color(0.84f, 0.77f, 0.62f)); // 찢어진 가장자리 안쪽으로 살짝 작게
            AddPaperBody(JdCanvas, 606, 886, new Color(0.79f, 0.70f, 0.53f));

            // 서류 더미 (대기/완료)
            pendingRoot = new GameObject("Pending").transform;
            pendingRoot.SetParent(transform, false);
            pendingRoot.localPosition = new Vector3(-0.78f, DeskTop, -0.33f);
            doneRoot = new GameObject("Done").transform;
            doneRoot.SetParent(transform, false);
            doneRoot.localPosition = new Vector3(0.62f, DeskTop, 0.30f);

            // 수정구 (마왕성의 전화) — 받침만 남고 구슬은 던질 수 있음
            Prim(PrimitiveType.Cylinder, "OrbBase", new Vector3(-0.62f, DeskTop + 0.015f, -0.18f),
                new Vector3(0.10f, 0.015f, 0.10f), new Color(0.16f, 0.12f, 0.10f));
            OrbProp = MakeProp("OrbProp", new Vector3(-0.62f, DeskTop, -0.18f),
                new Vector3(0, 0.09f, 0), new Vector3(0.18f, 0.2f, 0.18f), sphereCollider: true);
            var orbSphere = Prim(PrimitiveType.Sphere, "CrystalOrb", new Vector3(0, 0.09f, 0),
                Vector3.one * 0.14f, new Color(0.45f, 0.20f, 0.62f), OrbProp.transform);
            Orb = orbSphere.transform;
            OrbProp.Init(cam, this);

            // 던질 수 있는 잡동사니 소품들 (책상 = 놀이터)
            var skull = MakeProp("SkullProp", new Vector3(-0.92f, DeskTop, 0.12f),
                new Vector3(0, 0.06f, 0), new Vector3(0.13f, 0.13f, 0.13f), sphereCollider: true);
            Prim(PrimitiveType.Sphere, "Skull", new Vector3(0, 0.055f, 0),
                Vector3.one * 0.11f, new Color(0.88f, 0.85f, 0.78f), skull.transform);
            Box("EyeL", new Vector3(-0.022f, 0.06f, -0.045f), Vector3.one * 0.022f, new Color(0.1f, 0.08f, 0.07f), skull.transform);
            Box("EyeR", new Vector3(0.022f, 0.06f, -0.045f), Vector3.one * 0.022f, new Color(0.1f, 0.08f, 0.07f), skull.transform);
            skull.Init(cam, this);

            var books = MakeProp("BooksProp", new Vector3(0.92f, DeskTop, 0.10f),
                new Vector3(0, 0.06f, 0), new Vector3(0.26f, 0.13f, 0.2f));
            Box("Book1", new Vector3(0, 0.018f, 0), new Vector3(0.22f, 0.035f, 0.15f), new Color(0.42f, 0.18f, 0.15f), books.transform);
            var b2 = Box("Book2", new Vector3(0.01f, 0.053f, 0), new Vector3(0.20f, 0.035f, 0.14f), new Color(0.20f, 0.32f, 0.22f), books.transform);
            b2.transform.localRotation = Quaternion.Euler(0, 9f, 0);
            var b3 = Box("Book3", new Vector3(-0.01f, 0.088f, 0), new Vector3(0.18f, 0.035f, 0.13f), new Color(0.30f, 0.24f, 0.38f), books.transform);
            b3.transform.localRotation = Quaternion.Euler(0, -7f, 0);
            books.Init(cam, this);

            var mug = MakeProp("MugProp", new Vector3(-0.13f, DeskTop, -0.47f),
                new Vector3(0, 0.05f, 0), new Vector3(0.12f, 0.11f, 0.12f));
            Prim(PrimitiveType.Cylinder, "Mug", new Vector3(0, 0.045f, 0),
                new Vector3(0.07f, 0.045f, 0.07f), new Color(0.25f, 0.35f, 0.35f), mug.transform);
            Box("Handle", new Vector3(0.045f, 0.045f, 0), new Vector3(0.02f, 0.05f, 0.02f), new Color(0.25f, 0.35f, 0.35f), mug.transform);
            mug.Init(cam, this);

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

        /// 데스크 뷰 카메라 기준의 고정 포즈 — 손패(질문 카드)처럼 화면에 붙어 보이는 월드 오브젝트용.
        /// 카메라 푸시인과 무관하게 기본 데스크 카메라 기준으로 계산.
        public void GetFrontPose(float distance, float upOffset, float rightOffset, float yaw,
            out Vector3 pos, out Quaternion rot)
        {
            Quaternion camRot = Quaternion.LookRotation(DeskCamTarget - DeskCamPos);
            Vector3 f = camRot * Vector3.forward;
            Vector3 u = camRot * Vector3.up;
            Vector3 r = camRot * Vector3.right;
            pos = DeskCamPos + f * distance + u * upOffset + r * rightOffset;
            rot = Quaternion.LookRotation(f, u) * Quaternion.Euler(-8f, yaw, 0f);
        }

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

        /// 보라색 복셀 마법 버스트 — 소품 소멸/재생성 연출
        public void MagicBurst(Vector3 pos) => StartCoroutine(MagicBurstCo(pos));

        private IEnumerator MagicBurstCo(Vector3 pos)
        {
            const int n = 10;
            var parts = new Transform[n];
            var vels = new Vector3[n];
            var mat = Mat(new Color(0.72f, 0.42f, 1f));
            for (int i = 0; i < n; i++)
            {
                var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                c.name = "MagicBit";
                Destroy(c.GetComponent<Collider>());
                c.GetComponent<Renderer>().material = mat;
                c.transform.position = pos;
                c.transform.localScale = Vector3.one * 0.026f;
                parts[i] = c.transform;
                var dir = Random.onUnitSphere;
                dir.y = Mathf.Abs(dir.y) * 0.8f + 0.3f;
                vels[i] = dir.normalized * Random.Range(0.5f, 1.3f);
            }
            var lgo = new GameObject("PoofLight");
            lgo.transform.position = pos + Vector3.up * 0.08f;
            var flash = lgo.AddComponent<Light>();
            flash.type = LightType.Point;
            flash.color = new Color(0.7f, 0.4f, 1f);
            flash.range = 1.4f;
            flash.intensity = 3f;

            float t = 0f;
            const float dur = 0.45f;
            while (t < dur)
            {
                float dt = Time.deltaTime;
                t += dt;
                float k = 1f - t / dur;
                for (int i = 0; i < n; i++)
                {
                    parts[i].position += vels[i] * dt;
                    vels[i] *= 0.93f;
                    parts[i].localScale = Vector3.one * 0.026f * k;
                    parts[i].Rotate(200f * dt, 160f * dt, 0);
                }
                flash.intensity = 3f * k;
                yield return null;
            }
            foreach (var p in parts) Destroy(p.gameObject);
            Destroy(lgo);
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

        // ─── 면접 몬스터 (그레이박스 — 복셀 교체는 4단계) ───

        public Transform Monster { get; private set; }
        public Transform MonsterHead { get; private set; }
        private Coroutine monsterReactCo;

        /// 책상 너머에 지원 몬스터 등장 (프리미티브 + 마법 버스트)
        public void SpawnMonster(Color skin)
        {
            DespawnMonster();
            var root = new GameObject("Monster");
            root.transform.SetParent(transform, false);
            root.transform.localPosition = new Vector3(0f, 0f, 1.02f);

            // 카메라(데스크 뷰) 가시 창: z≈1.0에서 y 0.58~1.10 — 머리 상단이 1.06을 넘으면 프레임에 잘린다
            Prim(PrimitiveType.Capsule, "Body", new Vector3(0, 0.62f, 0),
                new Vector3(0.55f, 0.34f, 0.45f), skin, root.transform);
            var head = Prim(PrimitiveType.Cube, "Head", new Vector3(0, 0.92f, 0),
                new Vector3(0.28f, 0.26f, 0.26f), skin, root.transform);
            Prim(PrimitiveType.Cube, "EyeL", new Vector3(-0.07f, 0.94f, -0.145f),
                Vector3.one * 0.05f, new Color(0.95f, 0.9f, 0.7f), root.transform);
            Prim(PrimitiveType.Cube, "EyeR", new Vector3(0.07f, 0.94f, -0.145f),
                Vector3.one * 0.05f, new Color(0.95f, 0.9f, 0.7f), root.transform);

            // 던진 물건이 맞고 튕기는 몸통 콜라이더 — 레이어 2(포인터 레이캐스트 제외)라
            // "허공 클릭 = 내려놓기" 판정은 유지되고, 물리 충돌만 받는다
            root.layer = 2;
            var col = root.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0, 0.60f, 0);
            col.height = 1.15f;
            col.radius = 0.26f;
            col.material = BouncyMat;
            root.AddComponent<MonsterHitReact>().Init(this);

            Monster = root.transform;
            MonsterHead = head.transform;
            MagicBurst(root.transform.position + Vector3.up * 0.55f);
            Sfx.Shimmer();
        }

        public void DespawnMonster()
        {
            if (monsterReactCo != null) { StopCoroutine(monsterReactCo); monsterReactCo = null; }
            if (Monster != null) Destroy(Monster.gameObject);
            Monster = null;
            MonsterHead = null;
        }

        /// 리액션 2종 — nervous = 움찔움찔(긴장) / 아니면 = 안도의 들썩임
        public void MonsterReact(bool nervous)
        {
            if (Monster == null) return;
            if (monsterReactCo != null) StopCoroutine(monsterReactCo);
            monsterReactCo = StartCoroutine(MonsterReactCo(nervous));
        }

        private IEnumerator MonsterReactCo(bool nervous)
        {
            var m = Monster;
            Vector3 basePos = m.localPosition;
            float t = 0f;
            const float dur = 0.45f;
            while (t < dur)
            {
                t += Time.deltaTime;
                if (m == null) yield break;
                if (nervous)
                    m.localPosition = basePos + new Vector3(Mathf.Sin(t * 55f) * 0.02f * (1f - t / dur), 0, 0);
                else
                    m.localPosition = basePos + Vector3.up * (Mathf.Abs(Mathf.Sin(t / dur * Mathf.PI)) * 0.06f);
                yield return null;
            }
            if (m != null) m.localPosition = basePos;
            monsterReactCo = null;
        }

        /// 판정 후 퇴장 — 반응(합격 = 신남 / 탈락 = 움찔) 후 마법 소멸.
        /// ⚠️ 시작 시점의 몬스터만 만진다 — 대기 중 다음 지원자가 등장해도 새 몬스터는 건드리지 않음
        public IEnumerator MonsterExit(bool pass)
        {
            var m = Monster;
            if (m == null) yield break;
            MonsterReact(!pass);
            yield return new WaitForSeconds(0.55f);
            if (m == null) yield break; // 그 사이 이미 정리됨 (파괴된 오브젝트 = fake null)
            MagicBurst(m.position + Vector3.up * 0.55f);
            Sfx.Poof();
            if (Monster == m) DespawnMonster();     // 아직 현역이면 전역 참조까지 정리
            else Destroy(m.gameObject);             // 새 몬스터가 등장했으면 옛 몸만 제거
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
            drag.Init(pass, Cam, this, ResumeCanvas, DeskTop, canStamp, onSlam, onBlocked);
            return drag;
        }
    }

    /// 면접 몬스터 피격 반응 — 던진 소품에 맞으면 움찔 (튕김 자체는 콜라이더+반발 재질이 처리)
    public class MonsterHitReact : MonoBehaviour
    {
        private DeskRig rig;
        private float lastHit;

        public void Init(DeskRig deskRig) { rig = deskRig; }

        private void OnCollisionEnter(Collision c)
        {
            if (Time.time - lastHit < 0.6f) return;          // 연타 방지
            if (c.relativeVelocity.magnitude < 0.6f) return; // 스치는 접촉은 무시
            lastHit = Time.time;
            rig.MonsterReact(true); // 맞으면 움찔 — "면접자에게 던지기" 코미디
        }
    }
}
