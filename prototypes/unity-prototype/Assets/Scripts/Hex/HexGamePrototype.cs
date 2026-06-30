using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;          // Volume, VolumeProfile
using UnityEngine.Rendering.Universal; // DepthOfField, DepthOfFieldMode, GetUniversalAdditionalCameraData

namespace DDworld.HexPrototype
{
    /// <summary>
    /// DDworld 헥사 프로토타입 v0 — "왕국이 자라는 손맛" 루프 검증용 (던져버릴 코드).
    ///
    /// 사용법:
    ///   1) 빈 씬에서 빈 GameObject 생성 → 이 스크립트 부착 → Play.
    ///   2) 카메라·조명·헥사 그리드는 코드가 자동 생성. 에디터 세팅 불필요.
    ///
    /// 검증 목표: 타일 정착/정복 → 건물 → 영토·골드·카드 성장 → 더 확장. 의 루프가 재밌는가.
    /// 빠진 것(의도): 진짜 전투(combat.md)·보셀 아트·멀티자원·퀘스트·세이브 — 전부 나중.
    /// </summary>
    public class HexGamePrototype : MonoBehaviour
    {
        // ── 설정값 (튜닝) ─────────────────────────────
        [Header("Map")]
        public int mapRadius = 4;       // 중심에서 헥사 거리
        public float hexSize = 1f;      // 헥사 외접 반지름
        public float tileHeight = 0.4f; // 타일 두께(릴리프) — 0이면 평면 (Yield! Tacticon 레퍼 차용)
        public int mapSeed = 12345;     // 적 배치 시드(재현용)

        [Header("Economy / Tuning")]
        public int startGold = 50;
        public int incomePerClaimedTile = 5;  // 정복 타일당 day 골드
        public int mineBonus = 12;            // 금광 추가 골드/day
        public int costFarm = 30;
        public int costMine = 40;
        public int farmIntervalDays = 2;      // 농장: N day마다 카드 1장
        public int tilesPerCastleLevel = 4;   // 정복 N개마다 본성 레벨업

        [Header("Camera (플레이 중 실시간 조정 가능)")]
        public float cameraPitch = 23f;             // 내려다보는 각도(°). 작을수록 낮은 쿼터뷰
        public float cameraYaw = 0.2f;              // 좌우 회전(°)
        public float cameraDistance = 10f;          // 0 = 맵 크기에 맞춰 자동
        public float cameraFov = 30f;               // 시야각
        public Vector3 cameraPivot = Vector3.zero;  // 바라보는 중심점(기준점)

        [Header("Camera Nudge (타일 선택 시 살짝 이동)")]
        public bool cameraNudge = true;     // 선택 시 카메라가 그쪽으로 살짝 이동 on/off
        public float nudgeDelay = 0.15f;    // 선택 후 이동 시작까지 딜레이(초) — 잠깐 멈췄다가 움직임
        [Range(0f, 1f)] public float nudgeFactor = 0.5f; // 타일 방향 이동 비율 (0=정지, 1=타일이 화면 중앙). 클수록 더 가운데로
        public float nudgeSmooth = 0.4f;    // 이동 부드러움 (SmoothDamp 시간, 클수록 느긋)
        public float nudgeMaxDist = 3f;     // 최대 이동 거리 (과도한 드리프트/멀미 방지)

        [Header("Camera Pan (우클릭 드래그로 이동)")]
        public bool cameraPan = true;       // 우클릭 드래그 팬 on/off
        public float panSpeed = 1f;         // 팬 속도 (1 ≈ grab 1:1)
        public bool panInvert = false;      // 팬 방향 반대로

        [Header("DOF — 틸트시프트 (런타임 Volume 생성)")]
        public bool dofEnabled = true;                            // DOF on/off
        public DepthOfFieldMode dofMode = DepthOfFieldMode.Bokeh; // Bokeh=근/원경 블러(미니어처), Gaussian=원경만(art-bible 표기)
        [Header("DOF · Bokeh 모드")]
        [Min(0.1f)] public float dofFocusDistance = 10f;     // 선명한 거리(카메라~중심). cameraDistance(10)와 맞추면 중앙 선명
        [Range(1f, 32f)] public float dofAperture = 1f;      // 조리개 — 작을수록 블러 강함. URP 유효범위 1~32 (1=최대 블러)
        [Range(1f, 300f)] public float dofFocalLength = 59f; // 초점거리 — 클수록 블러 강함
        [Header("DOF · Gaussian 모드")]
        [Min(0f)] public float dofGaussStart = 11f;          // 블러 시작 거리
        [Min(0f)] public float dofGaussEnd = 28f;            // 최대 블러 거리
        [Range(0.5f, 1.5f)] public float dofGaussRadius = 1.2f; // 최대 블러 반경

        // ── 상태 ─────────────────────────────
        int day = 1;
        int gold;
        int castleLevel = 1;
        int claimedCount = 0;     // 본성 포함 정복 타일 수
        int builtCount = 0;       // 건설된 건물 수
        int farmCount = 0;
        int mineCount = 0;
        int cards = 0;            // 추상 카드 수 (전투 없음)
        int farmTimer = 0;
        string log = "환영합니다 — 인접한 빈 땅을 정착하거나 적 타일을 정복하세요.";
        bool busy = false;        // 이동 연출 중 입력 잠금

        int BuildLimit => castleLevel + 1;          // 본성 레벨이 건설 한도 게이트
        int ProductionSlots => castleLevel;         // (v0: 농장 최대치 = 슬롯)

        // ── 타일 ─────────────────────────────
        enum TileState { Castle, ClaimedEmpty, ClaimedFarm, ClaimedMine, UnclaimedEmpty, UnclaimedEnemy }

        class Tile
        {
            public HexCoord coord;
            public TileState state;
            public GameObject go;
            public MeshRenderer mr;
        }

        readonly Dictionary<HexCoord, Tile> tiles = new();
        Tile selected;
        Camera cam;
        GameObject marker;
        Mesh hexMesh;
        Shader litShader;

        // 카메라 nudge 런타임 상태
        Vector3 pivotCurrent;   // 카메라가 실제로 바라보는 현재 피벗 (애니메이션됨)
        Vector3 pivotTarget;    // 목표 피벗 (기준점 + 선택 타일 방향 오프셋)
        Vector3 pivotVel;       // SmoothDamp 속도 버퍼
        float nudgeTimer;       // >0 = 딜레이 대기 중 (이동 보류)

        // DOF 런타임 상태
        Volume postVolume;
        DepthOfField dof;

        // ── 부트스트랩 ─────────────────────────────
        void Start()
        {
            gold = startGold;
            pivotCurrent = pivotTarget = cameraPivot; // 카메라 nudge 초기값 = 기준점
            litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");
            if (litShader == null) litShader = Shader.Find("Sprites/Default");

            SetupCameraAndLight();
            SetupPostFX();
            hexMesh = BuildHexMesh(hexSize, tileHeight);
            GenerateMap();
            SetupMarker();
            RefreshColors();
        }

        void SetupCameraAndLight()
        {
            if (Camera.main != null) cam = Camera.main;
            else
            {
                var camGo = new GameObject("PrototypeCamera");
                cam = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
            }
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.07f, 0.12f); // 트와일라잇 배경
            PositionCamera();

            var lightGo = new GameObject("Sun");
            var l = lightGo.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = new Color(1f, 0.92f, 0.78f); // 따뜻한 빛
            l.intensity = 1.1f;
            lightGo.transform.rotation = Quaternion.Euler(52f, -35f, 0f);
            RenderSettings.ambientLight = new Color(0.32f, 0.34f, 0.45f); // 차가운 앰비언트
        }

        // 런타임으로 글로벌 Volume + DepthOfField를 만들어 틸트시프트 DOF를 켠다 (씬에 Volume 없어도 동작).
        void SetupPostFX()
        {
            if (cam == null) return;
            // URP: 카메라가 포스트프로세싱을 렌더하도록 보장
            var camData = cam.GetUniversalAdditionalCameraData();
            if (camData != null) camData.renderPostProcessing = true;

            var go = new GameObject("PrototypePostFX");
            postVolume = go.AddComponent<Volume>();
            postVolume.isGlobal = true;
            postVolume.priority = 100f; // 씬 볼륨보다 우선 — DOF 확실히 적용
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            postVolume.profile = profile;
            dof = profile.Add<DepthOfField>(true);
            ApplyDof();
        }

        // 인스펙터 노브 → DepthOfField 파라미터 반영 (LateUpdate에서 매 프레임 = 실시간 튜닝).
        void ApplyDof()
        {
            if (dof == null) return;
            dof.active = dofEnabled;
            dof.mode.Override(dofEnabled ? dofMode : DepthOfFieldMode.Off);
            // Bokeh (근/원경 블러 — 포커스 거리 기준)
            dof.focusDistance.Override(dofFocusDistance);
            dof.aperture.Override(dofAperture);
            dof.focalLength.Override(dofFocalLength);
            // Gaussian (원경 블러 — 거리 램프)
            dof.gaussianStart.Override(dofGaussStart);
            dof.gaussianEnd.Override(dofGaussEnd);
            dof.gaussianMaxRadius.Override(dofGaussRadius);
            dof.highQualitySampling.Override(true);
        }

        // 카메라를 피벗 중심으로 pitch/yaw/거리/시야각에 맞춰 배치
        void PositionCamera()
        {
            if (cam == null) return;
            float dist = cameraDistance > 0f ? cameraDistance : (mapRadius + 2) * hexSize * 2.2f;
            float p = cameraPitch * Mathf.Deg2Rad;
            float y = cameraYaw * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(p) * Mathf.Sin(y), Mathf.Sin(p), -Mathf.Cos(p) * Mathf.Cos(y));
            cam.transform.position = pivotCurrent + dir * dist;
            cam.transform.LookAt(pivotCurrent);
            cam.fieldOfView = cameraFov;
        }

        void LateUpdate()
        {
            UpdateCameraNudge();
            PositionCamera(); // 인스펙터 값 바꾸면 플레이 중에도 즉시 반영
            ApplyDof();       // DOF 노브 실시간 반영
        }

        // 선택 시 카메라가 그쪽으로 살짝 이동 — 딜레이 동안은 멈췄다가, 끝나면 SmoothDamp로 스르륵.
        void UpdateCameraNudge()
        {
            if (!cameraNudge) pivotTarget = cameraPivot;        // 꺼져 있으면 피벗은 기준점을 따라감
            if (nudgeTimer > 0f) { nudgeTimer -= Time.deltaTime; return; } // 딜레이 중엔 정지
            pivotCurrent = Vector3.SmoothDamp(pivotCurrent, pivotTarget, ref pivotVel, nudgeSmooth);
        }

        // 선택 타일 방향으로 목표 피벗을 잡고 딜레이 타이머 시작.
        void NudgeToward(Tile t)
        {
            Vector3 toward = t.coord.ToWorld(hexSize) - cameraPivot;
            toward.y = 0f;
            pivotTarget = cameraPivot + Vector3.ClampMagnitude(toward * nudgeFactor, nudgeMaxDist);
            nudgeTimer = nudgeDelay; // 딜레이 후 이동 시작
        }

        // 우클릭 드래그 팬 — 마우스 픽셀 이동량을 지면(XZ) 이동으로 변환 (grab 스타일).
        void HandlePan(Vector2 deltaPixels)
        {
            if (cam == null) return;
            Vector3 right = cam.transform.right; right.y = 0f; right.Normalize();
            Vector3 fwd = cam.transform.forward; fwd.y = 0f; fwd.Normalize();
            float dist = cameraDistance > 0f ? cameraDistance : (mapRadius + 2) * hexSize * 2.2f;
            // 화면 1픽셀당 월드 거리 ≈ 피벗 평면 프러스텀 높이 / 화면 높이 (대략 1:1 grab)
            float worldPerPixel = 2f * dist * Mathf.Tan(cameraFov * 0.5f * Mathf.Deg2Rad) / Mathf.Max(1, Screen.height);
            float s = panSpeed * worldPerPixel * (panInvert ? -1f : 1f);
            // 드래그 방향으로 보드가 끌려오게 → 피벗은 반대로(가로) / 화면 위로 갈수록 안쪽(세로)
            Vector3 worldDelta = (-right * deltaPixels.x + fwd * deltaPixels.y) * s;
            // 기준점·nudge목표·현재피벗을 함께 이동 → nudge와 공존 + 드래그 중 즉시 따라감
            cameraPivot += worldDelta;
            pivotTarget += worldDelta;
            pivotCurrent += worldDelta;
        }

        void SetupMarker()
        {
            marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "SortieMarker";
            marker.transform.localScale = Vector3.one * (hexSize * 0.4f);
            Destroy(marker.GetComponent<Collider>());
            ApplyColor(marker.GetComponent<MeshRenderer>(), new Color(0.3f, 0.7f, 1f));
            marker.SetActive(false);
        }

        // ── 맵 생성 ─────────────────────────────
        void GenerateMap()
        {
            var rng = new System.Random(mapSeed);
            for (int q = -mapRadius; q <= mapRadius; q++)
            {
                int r1 = Mathf.Max(-mapRadius, -q - mapRadius);
                int r2 = Mathf.Min(mapRadius, -q + mapRadius);
                for (int r = r1; r <= r2; r++)
                {
                    var c = new HexCoord(q, r);
                    int dist = HexCoord.Distance(new HexCoord(0, 0), c);
                    TileState st;
                    if (dist == 0) st = TileState.Castle;
                    else
                    {
                        // 멀수록 적 밀도↑ (안쪽 ~0, 바깥 ~0.6)
                        float enemyChance = Mathf.InverseLerp(1f, mapRadius, dist) * 0.6f;
                        st = rng.NextDouble() < enemyChance ? TileState.UnclaimedEnemy : TileState.UnclaimedEmpty;
                    }
                    CreateTile(c, st);
                }
            }
            claimedCount = 1; // 본성
        }

        void CreateTile(HexCoord c, TileState st)
        {
            var go = new GameObject($"Hex_{c}");
            go.transform.position = c.ToWorld(hexSize);
            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = hexMesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.material = new Material(litShader);
            var mc = go.AddComponent<MeshCollider>();
            mc.sharedMesh = hexMesh;
            tiles[c] = new Tile { coord = c, state = st, go = go, mr = mr };
        }

        // ── 입력 (OnGUI 이벤트 기반 — 인풋 백엔드 무관) ─────────────────────────────
        void HandleWorldClick()
        {
            if (busy) return;
            Vector2 m = Event.current.mousePosition;
            if (IsOverGui(m)) return; // HUD/로그/버튼/패널 위 클릭은 뒤 타일 선택으로 흘리지 않음 (click-through 방지)
            Vector3 sp = new Vector3(m.x, Screen.height - m.y, 0f);
            Ray ray = cam.ScreenPointToRay(sp);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                var t = hit.collider.GetComponent<MeshFilter>() ? FindTileByGo(hit.collider.gameObject) : null;
                if (t != null) { selected = t; RefreshColors(); if (cameraNudge) NudgeToward(t); }
            }
        }

        // OnGUI에서 그리는 패널들과 동일한 Rect — 그 위를 클릭하면 월드 raycast를 건너뜀.
        // (GUI.Box/Label은 마우스 이벤트를 소비하지 않아 박스 위 클릭이 뒤 타일을 선택하던 문제 차단)
        bool IsOverGui(Vector2 m)
        {
            if (new Rect(8, 8, 360, 92).Contains(m)) return true;                                 // 상단 HUD
            if (new Rect(8, Screen.height - 40, Screen.width - 16, 32).Contains(m)) return true;   // 하단 로그
            if (new Rect(Screen.width - 150, 10, 140, 72).Contains(m)) return true;                // End Turn + 맵 리셋 버튼
            if (selected != null && new Rect(8, 110, 300, 150).Contains(m)) return true;           // 선택 패널
            return false;
        }

        Tile FindTileByGo(GameObject go)
        {
            foreach (var t in tiles.Values) if (t.go == go) return t;
            return null;
        }

        // ── 액션 ─────────────────────────────
        bool IsUnclaimed(TileState s) => s == TileState.UnclaimedEmpty || s == TileState.UnclaimedEnemy;
        bool IsClaimed(TileState s) => !IsUnclaimed(s);

        bool IsReachable(Tile t)
        {
            if (!IsUnclaimed(t.state)) return false;
            for (int d = 0; d < 6; d++)
                if (tiles.TryGetValue(t.coord.Neighbor(d), out var n) && IsClaimed(n.state))
                    return true;
            return false;
        }

        void StartSortie(Tile t)
        {
            if (busy || !IsReachable(t)) return;
            busy = true;
            bool isBattle = t.state == TileState.UnclaimedEnemy;
            Vector3 from = new HexCoord(0, 0).ToWorld(hexSize);
            Vector3 to = t.coord.ToWorld(hexSize);
            StartCoroutine(MoveMarker(from, to, 0.5f, () =>
            {
                if (isBattle) log = $"전투 승리! {t.coord} 정복 (placeholder)";
                else log = $"{t.coord} 평화 정착 완료";
                Claim(t);
                busy = false;
            }));
        }

        void Claim(Tile t)
        {
            t.state = TileState.ClaimedEmpty;
            claimedCount++;
            AdvanceDay();
            // 본성 레벨 = 정복(본성 제외) / N + 1
            int newLevel = 1 + (claimedCount - 1) / tilesPerCastleLevel;
            if (newLevel > castleLevel)
            {
                castleLevel = newLevel;
                log += $" — 본성 레벨업! Lv{castleLevel} (건설한도 {BuildLimit}, 생산슬롯 {ProductionSlots})";
            }
            RefreshColors();
        }

        void Build(Tile t, bool farm)
        {
            if (t.state != TileState.ClaimedEmpty) return;
            if (builtCount >= BuildLimit) { log = "건설 한도 초과 — 본성 레벨업 필요"; return; }
            int cost = farm ? costFarm : costMine;
            if (gold < cost) { log = "골드 부족"; return; }
            if (farm && farmCount >= ProductionSlots) { log = $"생산 슬롯 부족 (현재 {ProductionSlots})"; return; }

            gold -= cost;
            builtCount++;
            if (farm) { t.state = TileState.ClaimedFarm; farmCount++; log = "농장 건설 — 카드 생산 시작"; }
            else { t.state = TileState.ClaimedMine; mineCount++; log = "금광 건설 — 골드 수입 증가"; }
            RefreshColors();
        }

        void AdvanceDay()
        {
            day++;
            gold += claimedCount * incomePerClaimedTile + mineCount * mineBonus;
            // 농장 카드 생산
            // TODO(claude/검토): farmTimer가 전역 공유 + cards += farmCount 라서, 기존 농장이 타이머를
            //   쌓아둔 상태에서 새 농장을 지으면 그 농장도 1일 만에 카드를 소급 생산함(농장별 독립 타이머 부재).
            //   프로토타입 손맛 검증 목적상 의도적으로 방치 — production 경제 설계 시 농장별 builtDay 기반으로
            //   (day - builtDay) % farmIntervalDays == 0 처리로 교체할 것. (적대적 리뷰 2026-06-30 확정 결함 #5)
            if (farmCount > 0)
            {
                farmTimer++;
                if (farmTimer >= farmIntervalDays) { cards += farmCount; farmTimer = 0; }
            }
        }

        // ── 이동 연출 (코루틴 — 외부 의존 없음) ─────────────────────────────
        IEnumerator MoveMarker(Vector3 from, Vector3 to, float dur, System.Action onDone)
        {
            marker.SetActive(true);
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / dur);
                marker.transform.position = Vector3.Lerp(from, to, k) + Vector3.up * 0.5f;
                yield return null;
            }
            marker.SetActive(false);
            onDone?.Invoke();
        }

        // ── 색/하이라이트 ─────────────────────────────
        void RefreshColors()
        {
            foreach (var t in tiles.Values)
            {
                Color c = ColorFor(t.state);
                if (IsReachable(t)) c = Color.Lerp(c, Color.white, 0.18f); // 확장 가능 타일 살짝 밝게
                float y = 0f;
                if (t == selected) { c = Color.Lerp(c, new Color(0.3f, 0.8f, 1f), 0.4f); y = 0.18f; }
                t.go.transform.position = new Vector3(t.go.transform.position.x, y, t.go.transform.position.z);
                ApplyColor(t.mr, c);
            }
        }

        Color ColorFor(TileState s) => s switch
        {
            TileState.Castle => new Color(1f, 0.85f, 0.32f),
            TileState.ClaimedEmpty => new Color(0.45f, 0.68f, 0.36f),
            TileState.ClaimedFarm => new Color(0.85f, 0.78f, 0.30f),
            TileState.ClaimedMine => new Color(0.80f, 0.60f, 0.25f),
            TileState.UnclaimedEnemy => new Color(0.52f, 0.20f, 0.20f),
            _ => new Color(0.28f, 0.33f, 0.30f), // UnclaimedEmpty
        };

        void ApplyColor(MeshRenderer mr, Color c)
        {
            mr.material.SetColor("_BaseColor", c); // URP
            mr.material.color = c;                 // Built-in/Standard
        }

        // ── UI (OnGUI — 캔버스 세팅 불필요) ─────────────────────────────
        void OnGUI()
        {
            GUI.skin.label.fontSize = 14;
            GUI.skin.button.fontSize = 14;

            // 상단 HUD
            GUI.Box(new Rect(8, 8, 360, 92), "");
            GUI.Label(new Rect(18, 12, 360, 22),
                $"<b>Royal Dominion</b>   Day {day}   본성 Lv{castleLevel}");
            GUI.Label(new Rect(18, 36, 360, 22),
                $"골드 {gold}   (+{claimedCount * incomePerClaimedTile + mineCount * mineBonus}/day)");
            GUI.Label(new Rect(18, 58, 360, 22),
                $"정복 {claimedCount}   건설 {builtCount}/{BuildLimit}   농장 {farmCount}/{ProductionSlots}   카드 {cards}");

            // 하단 로그
            GUI.Box(new Rect(8, Screen.height - 40, Screen.width - 16, 32), "");
            GUI.Label(new Rect(18, Screen.height - 36, Screen.width - 28, 24), log);

            // 우상단: End Turn / 리셋
            if (GUI.Button(new Rect(Screen.width - 150, 10, 140, 30), "End Turn (하루 보내기)"))
            { if (!busy) { AdvanceDay(); log = $"하루 경과 — Day {day}"; } }
            if (GUI.Button(new Rect(Screen.width - 150, 46, 140, 30), "맵 리셋"))
                ResetMap();

            // 선택 타일 패널
            if (selected != null) DrawSelectedPanel();

            // 월드 클릭 (버튼이 소비 안 한 MouseDown만)
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                HandleWorldClick();

            // 우클릭 드래그 = 카메라 팬
            if (cameraPan && Event.current.type == EventType.MouseDrag && Event.current.button == 1)
            { HandlePan(Event.current.delta); Event.current.Use(); }
        }

        void DrawSelectedPanel()
        {
            var rect = new Rect(8, 110, 300, 150);
            GUI.Box(rect, "");
            GUI.Label(new Rect(18, 116, 280, 22), $"<b>선택:</b> {selected.coord}  [{StateLabel(selected.state)}]");

            float y = 144;
            if (IsUnclaimed(selected.state))
            {
                if (IsReachable(selected))
                {
                    string label = selected.state == TileState.UnclaimedEnemy ? "전투로 정복" : "정착 (하루)";
                    if (GUI.Button(new Rect(18, y, 200, 30), label)) StartSortie(selected);
                }
                else GUI.Label(new Rect(18, y, 280, 22), "내 영토에 인접하지 않음 (확장 불가)");
            }
            else if (selected.state == TileState.ClaimedEmpty)
            {
                if (GUI.Button(new Rect(18, y, 135, 30), $"농장 ({costFarm}G)")) Build(selected, true);
                if (GUI.Button(new Rect(160, y, 135, 30), $"금광 ({costMine}G)")) Build(selected, false);
                GUI.Label(new Rect(18, y + 34, 280, 22), "농장=카드 생산 / 금광=골드 수입");
            }
            else
            {
                GUI.Label(new Rect(18, y, 280, 22), "이미 개발된 타일");
            }
        }

        string StateLabel(TileState s) => s switch
        {
            TileState.Castle => "본성",
            TileState.ClaimedEmpty => "정복(빈 땅)",
            TileState.ClaimedFarm => "농장",
            TileState.ClaimedMine => "금광",
            TileState.UnclaimedEnemy => "적 타일",
            _ => "빈 땅",
        };

        // 주의: 'Reset'은 MonoBehaviour 예약 메시지(에디터에서 컴포넌트 부착/컨텍스트 Reset 시 자동 호출)라 이름을 피함.
        void ResetMap()
        {
            // 진행 중 이동 연출을 끊고 입력 잠금/마커를 정리 — 코루틴 완료 콜백이 새 맵 상태를 오염시키는 것 방지.
            StopAllCoroutines();
            busy = false;
            if (marker != null) marker.SetActive(false);
            // 타일별 머티리얼 인스턴스도 함께 파괴 (GameObject만 Destroy하면 인스턴스 머티리얼이 누수됨).
            foreach (var t in tiles.Values) { if (t.mr != null) Destroy(t.mr.material); Destroy(t.go); }
            tiles.Clear();
            selected = null;
            day = 1; gold = startGold; castleLevel = 1; claimedCount = 0;
            builtCount = 0; farmCount = 0; mineCount = 0; cards = 0; farmTimer = 0;
            log = "맵 리셋.";
            GenerateMap();
            RefreshColors();
        }

        // ── 헥사 메쉬 (pointy-top 3D 프리즘: 윗면 + 6 옆면, 양면) ─────────────────────────────
        // height=0 이면 기존처럼 평면. height>0 이면 두께 있는 타일(릴리프)이 된다.
        // 윗면 노멀=up, 옆면 노멀=바깥쪽 → 낮은 쿼터뷰(pitch 23°)에서 옆면이 입체로 보인다.
        static Mesh BuildHexMesh(float size, float height)
        {
            var mesh = new Mesh { name = "HexTile" };
            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var tris = new List<int>();

            // 윗면/바닥 링 좌표 (윗면 y=height, 바닥 y=0)
            var top = new Vector3[6];
            var bot = new Vector3[6];
            for (int i = 0; i < 6; i++)
            {
                float ang = Mathf.Deg2Rad * (60f * i - 30f);
                float x = size * 0.96f * Mathf.Cos(ang);
                float z = size * 0.96f * Mathf.Sin(ang);
                top[i] = new Vector3(x, height, z);
                bot[i] = new Vector3(x, 0f, z);
            }

            // ── 윗면 (양면, 노멀 up) ──
            int center = verts.Count;
            verts.Add(new Vector3(0f, height, 0f)); normals.Add(Vector3.up);
            int ring = verts.Count;
            for (int i = 0; i < 6; i++) { verts.Add(top[i]); normals.Add(Vector3.up); }
            for (int i = 0; i < 6; i++)
            {
                int a = ring + i, b = ring + (i + 1) % 6;
                tris.Add(center); tris.Add(a); tris.Add(b);   // 앞면
                tris.Add(center); tris.Add(b); tris.Add(a);   // 뒷면(역winding) — 컬링 방향 무관하게 보이게
            }

            // ── 옆면 6개 (양면, 노멀 바깥쪽) ──
            if (height > 0f)
            {
                for (int i = 0; i < 6; i++)
                {
                    int j = (i + 1) % 6;
                    Vector3 outward = (top[i] + top[j]) * 0.5f; outward.y = 0f; outward.Normalize();
                    int v = verts.Count;
                    verts.Add(top[i]); verts.Add(top[j]); verts.Add(bot[i]); verts.Add(bot[j]);
                    for (int k = 0; k < 4; k++) normals.Add(outward);
                    // 쿼드(top_i, top_j, bot_i, bot_j) 양면
                    tris.Add(v); tris.Add(v + 2); tris.Add(v + 1);
                    tris.Add(v + 1); tris.Add(v + 2); tris.Add(v + 3);
                    tris.Add(v); tris.Add(v + 1); tris.Add(v + 2);       // 역winding
                    tris.Add(v + 1); tris.Add(v + 3); tris.Add(v + 2);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetNormals(normals);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
