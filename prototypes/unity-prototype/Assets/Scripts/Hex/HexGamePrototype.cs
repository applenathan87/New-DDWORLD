using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public float cameraPitch = 35f;             // 내려다보는 각도(°). 작을수록 낮은 쿼터뷰
        public float cameraYaw = 0f;                // 좌우 회전(°)
        public float cameraDistance = 0f;           // 0 = 맵 크기에 맞춰 자동
        public float cameraFov = 40f;               // 시야각
        public Vector3 cameraPivot = Vector3.zero;  // 바라보는 중심점

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

        // ── 부트스트랩 ─────────────────────────────
        void Start()
        {
            gold = startGold;
            litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");
            if (litShader == null) litShader = Shader.Find("Sprites/Default");

            SetupCameraAndLight();
            hexMesh = BuildHexMesh(hexSize);
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

        // 카메라를 피벗 중심으로 pitch/yaw/거리/시야각에 맞춰 배치
        void PositionCamera()
        {
            if (cam == null) return;
            float dist = cameraDistance > 0f ? cameraDistance : (mapRadius + 2) * hexSize * 2.2f;
            float p = cameraPitch * Mathf.Deg2Rad;
            float y = cameraYaw * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(p) * Mathf.Sin(y), Mathf.Sin(p), -Mathf.Cos(p) * Mathf.Cos(y));
            cam.transform.position = cameraPivot + dir * dist;
            cam.transform.LookAt(cameraPivot);
            cam.fieldOfView = cameraFov;
        }

        void LateUpdate()
        {
            PositionCamera(); // 인스펙터 값 바꾸면 플레이 중에도 즉시 반영
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
            Vector3 sp = new Vector3(Event.current.mousePosition.x,
                                     Screen.height - Event.current.mousePosition.y, 0f);
            Ray ray = cam.ScreenPointToRay(sp);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                var t = hit.collider.GetComponent<MeshFilter>() ? FindTileByGo(hit.collider.gameObject) : null;
                if (t != null) { selected = t; RefreshColors(); }
            }
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
                Reset();

            // 선택 타일 패널
            if (selected != null) DrawSelectedPanel();

            // 월드 클릭 (버튼이 소비 안 한 MouseDown만)
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                HandleWorldClick();
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

        void Reset()
        {
            foreach (var t in tiles.Values) Destroy(t.go);
            tiles.Clear();
            selected = null;
            day = 1; gold = startGold; castleLevel = 1; claimedCount = 0;
            builtCount = 0; farmCount = 0; mineCount = 0; cards = 0; farmTimer = 0;
            log = "맵 리셋.";
            GenerateMap();
            RefreshColors();
        }

        // ── 헥사 메쉬 (pointy-top, 평면 윗면, 양면) ─────────────────────────────
        static Mesh BuildHexMesh(float size)
        {
            var mesh = new Mesh { name = "HexTile" };
            var verts = new Vector3[7];
            var normals = new Vector3[7];
            verts[0] = Vector3.zero; normals[0] = Vector3.up;
            for (int i = 0; i < 6; i++)
            {
                float ang = Mathf.Deg2Rad * (60f * i - 30f);
                verts[i + 1] = new Vector3(size * 0.96f * Mathf.Cos(ang), 0f, size * 0.96f * Mathf.Sin(ang));
                normals[i + 1] = Vector3.up;
            }
            // 양면(36개) — 컬링 방향에 상관없이 위에서 항상 보이게
            var tris = new int[36];
            for (int i = 0; i < 6; i++)
            {
                int a = 1 + i, b = 1 + (i + 1) % 6;
                // 앞면
                tris[i * 6] = 0; tris[i * 6 + 1] = a; tris[i * 6 + 2] = b;
                // 뒷면(역winding)
                tris[i * 6 + 3] = 0; tris[i * 6 + 4] = b; tris[i * 6 + 5] = a;
            }
            mesh.vertices = verts;
            mesh.normals = normals;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
