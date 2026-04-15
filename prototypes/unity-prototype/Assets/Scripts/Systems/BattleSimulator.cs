using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 전투 시뮬레이션. 개별 병사가 독립적으로 이동/타겟팅/공격한다.
/// BattleSimulator는 병사 리스트 관리, 승패 판정, 상성 계산만 담당.
/// </summary>
public class BattleSimulator : MonoBehaviour
{
    public static BattleSimulator Instance { get; private set; }

    [Header("폰트")]
    public TMP_FontAsset koreanFont;

    [Header("시뮬레이션 설정")]
    public float moveSpeedBase = 2f;
    public float typeAdvantage = 1.5f;
    public float typeDisadvantage = 0.5f;

    private List<Soldier> playerSoldiers = new();
    private List<Soldier> enemySoldiers = new();
    private bool battleRunning;

    // 전장 경계
    private float fieldMinX;
    private float fieldMaxX;
    private float fieldMinZ;
    private float fieldMaxZ;

    private void Awake()
    {
        Instance = this;
    }

    public void ResetState()
    {
        battleRunning = false;
        StopAllCoroutines();
    }

    public void StartBattle()
    {
        battleRunning = false; // 이전 상태 리셋

        var bf = BattleField.Instance;
        if (bf != null)
        {
            float halfTile = bf.tileSize * 0.5f;
            fieldMinX = bf.GetTileWorldPosition(0, 0).x - halfTile;
            fieldMaxX = bf.GetTileWorldPosition(bf.columns - 1, 0).x + halfTile;
            fieldMinZ = bf.GetTileWorldPosition(0, 0).z - halfTile;
            fieldMaxZ = bf.GetTileWorldPosition(0, bf.rows - 1).z + halfTile;
        }

        // 타일 색 리셋 (병종 색 → 원래 그리드 색)
        ResetTileColors();

        // 경계선 시각화
        DrawFieldBounds();
        DrawLaneBoundaryLines();

        CollectSoldiers();
        StartCoroutine(RunBattle());
    }

    private void CollectSoldiers()
    {
        playerSoldiers.Clear();
        enemySoldiers.Clear();

        var gm = GameManager.Instance;
        var bf = BattleField.Instance;

        // 플레이어 배치 → 개별 병사 수집
        foreach (var kvp in gm.PlayerPlacements)
        {
            var tile = bf.GetBattleTile(kvp.Key.y, kvp.Key.x);
            if (tile == null) continue;

            foreach (var soldier in tile.Soldiers)
            {
                soldier.StartBattle(kvp.Value.unitData, true, moveSpeedBase);
                playerSoldiers.Add(soldier);
            }
        }

        // 적 배치 → 개별 병사 수집
        foreach (var kvp in gm.EnemyPlacements)
        {
            int gridCol = 9 + (4 - kvp.Key.y);
            var tile = bf.GetBattleTile(gridCol, kvp.Key.x);
            if (tile == null) continue;

            foreach (var soldier in tile.Soldiers)
            {
                soldier.StartBattle(kvp.Value.unitData, false, moveSpeedBase);
                enemySoldiers.Add(soldier);
            }
        }

        Debug.Log($"[전투 시작] 플레이어 {playerSoldiers.Count}명 vs 적 {enemySoldiers.Count}명");
    }

    private IEnumerator RunBattle()
    {
        battleRunning = true;

        yield return new WaitForSeconds(1f);
        PlacementUI.Instance?.ShowPhaseTitle("전투 개시!");
        yield return new WaitForSeconds(0.5f);

        while (battleRunning)
        {
            // 각 병사가 스스로 행동 (아군 리스트도 전달 → 분리 처리)
            foreach (var s in playerSoldiers)
                s.UpdateBattle(playerSoldiers, enemySoldiers, fieldMinX, fieldMaxX, fieldMinZ, fieldMaxZ);
            foreach (var s in enemySoldiers)
                s.UpdateBattle(enemySoldiers, playerSoldiers, fieldMinX, fieldMaxX, fieldMinZ, fieldMaxZ);

            // 승패 체크
            int playerAlive = CountAlive(playerSoldiers);
            int enemyAlive = CountAlive(enemySoldiers);

            if (playerAlive == 0 || enemyAlive == 0)
            {
                yield return new WaitForSeconds(0.5f);
                EndBattle(playerAlive, enemyAlive);
                yield break;
            }

            yield return null;
        }
    }

    private int CountAlive(List<Soldier> soldiers)
    {
        int count = 0;
        foreach (var s in soldiers)
            if (!s.isDead && !s.isTrap) count++;
        return count;
    }

    private void EndBattle(int playerAlive, int enemyAlive)
    {
        battleRunning = false;

        string result;
        if (playerAlive > 0 && enemyAlive == 0)
        {
            result = $"승리! ({playerAlive}명 생존)";
            GameManager.Instance.ReportRoundResult(true);
        }
        else if (enemyAlive > 0 && playerAlive == 0)
        {
            result = $"패배... (적 {enemyAlive}명 생존)";
            GameManager.Instance.ReportRoundResult(false);
        }
        else
        {
            result = "무승부!";
            GameManager.Instance.ReportRoundResult(true);
            GameManager.Instance.ReportRoundResult(false);
        }

        Debug.Log($"=== 라운드 종료: {result} ===");
        LogBattleStats();
        PlacementUI.Instance?.ShowPhaseTitle(result);
        ShowResultScreen(result);
    }

    private GameObject resultScreen;

    private void ShowResultScreen(string result)
    {
        // 이전 결과 화면 제거
        if (resultScreen != null) Destroy(resultScreen);

        resultScreen = new GameObject("ResultScreen");

        var cam = Camera.main;
        if (cam == null) return;

        resultScreen.transform.SetParent(cam.transform);
        resultScreen.transform.localPosition = new Vector3(0, 0, 5f);
        resultScreen.transform.localRotation = Quaternion.identity;

        // 통계 텍스트 조립
        string stats = $"<size=150%>{result}</size>\n\n";
        stats += BuildSideStats("아군", playerSoldiers);
        stats += "\n";
        stats += BuildSideStats("적군", enemySoldiers);
        stats += "\n<size=80%><color=#888>R: 재배치  B: 전투 시작</color></size>";

        var textObj = new GameObject("ResultText");
        textObj.transform.SetParent(resultScreen.transform);
        textObj.transform.localPosition = new Vector3(0, 0, -0.01f);
        textObj.transform.localRotation = Quaternion.identity;

        var tmp = textObj.AddComponent<TextMeshPro>();
        if (koreanFont != null) tmp.font = koreanFont;
        tmp.text = stats;
        tmp.fontSize = 1.5f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.rectTransform.sizeDelta = new Vector2(1.4f, 0.95f);
        tmp.raycastTarget = false;
        tmp.richText = true;
    }

    /// <summary>
    /// 결과 화면 제거 (BalanceTestManager에서 호출)
    /// </summary>
    public void ClearResultScreen()
    {
        if (resultScreen != null) Destroy(resultScreen);
    }

    private string BuildSideStats(string sideName, List<Soldier> soldiers)
    {
        var stats = new Dictionary<string, (int total, int alive, int dmgDealt, int dmgTaken, int kills)>();

        foreach (var s in soldiers)
        {
            string name = s.unitData.unitName;
            if (!stats.ContainsKey(name))
                stats[name] = (0, 0, 0, 0, 0);

            var st = stats[name];
            st.total++;
            if (!s.isDead) st.alive++;
            st.dmgDealt += s.totalDamageDealt;
            st.dmgTaken += s.totalDamageTaken;
            st.kills += s.killCount;
            stats[name] = st;
        }

        string text = $"<color=#FFD700>[ {sideName} ]</color>\n";
        foreach (var kvp in stats)
        {
            var s = kvp.Value;
            string aliveColor = s.alive > 0 ? "#4CAF50" : "#F44336";
            text += $"  {kvp.Key}: <color={aliveColor}>{s.alive}/{s.total}</color> | 딜 {s.dmgDealt} | 피해 {s.dmgTaken} | 처치 {s.kills}\n";
        }
        return text;
    }

    private void DrawFieldBounds()
    {
        var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
        var mat = new Material(shader);
        mat.color = new Color(0f, 1f, 0f, 0.8f);

        var obj = new GameObject("FieldBounds");
        var lr = obj.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.positionCount = 4;
        lr.startWidth = 0.03f;
        lr.endWidth = 0.03f;
        lr.material = mat;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        float y = 0.03f;
        lr.SetPosition(0, new Vector3(fieldMinX, y, fieldMinZ));
        lr.SetPosition(1, new Vector3(fieldMaxX, y, fieldMinZ));
        lr.SetPosition(2, new Vector3(fieldMaxX, y, fieldMaxZ));
        lr.SetPosition(3, new Vector3(fieldMinX, y, fieldMaxZ));
    }

    /// <summary>
    /// 자유 추적 전환 라인 (하얀 선 2개: 플레이어→적3열, 적→플레이어3열)
    /// </summary>
    private void DrawLaneBoundaryLines()
    {
        var bf = BattleField.Instance;
        if (bf == null) return;

        var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
        var mat = new Material(shader);
        mat.color = new Color(1f, 1f, 1f, 0.5f);

        float y = 0.04f;

        // 플레이어 측 라인: 적 3열째 (col 11)
        DrawVerticalLine("LaneLine_Player", bf.GetTileWorldPosition(11, 0).x, y, mat);
        // 적 측 라인: 플레이어 3열째 (col 2)
        DrawVerticalLine("LaneLine_Enemy", bf.GetTileWorldPosition(2, 0).x, y, mat);
    }

    private void DrawVerticalLine(string name, float x, float y, Material mat)
    {
        var obj = new GameObject(name);
        var lr = obj.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = 2;
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.material = mat;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        lr.SetPosition(0, new Vector3(x, y, fieldMinZ));
        lr.SetPosition(1, new Vector3(x, y, fieldMaxZ));
    }

    private void ResetTileColors()
    {
        var bf = BattleField.Instance;
        if (bf == null) return;

        for (int col = 0; col < bf.columns; col++)
            for (int row = 0; row < bf.rows; row++)
            {
                var tile = bf.GetBattleTile(col, row);
                if (tile != null) tile.ResetForBattle();
            }
    }

    private void LogBattleStats()
    {
        Debug.Log("=== 전투 통계 ===");

        Debug.Log("--- 아군 ---");
        LogSideStats(playerSoldiers);

        Debug.Log("--- 적군 ---");
        LogSideStats(enemySoldiers);
    }

    private void LogSideStats(List<Soldier> soldiers)
    {
        // 병종별로 그룹핑
        var stats = new Dictionary<string, (int total, int alive, int dmgDealt, int dmgTaken, int kills)>();

        foreach (var s in soldiers)
        {
            string name = s.unitData.unitName;
            if (!stats.ContainsKey(name))
                stats[name] = (0, 0, 0, 0, 0);

            var st = stats[name];
            st.total++;
            if (!s.isDead) st.alive++;
            st.dmgDealt += s.totalDamageDealt;
            st.dmgTaken += s.totalDamageTaken;
            st.kills += s.killCount;
            stats[name] = st;
        }

        foreach (var kvp in stats)
        {
            var s = kvp.Value;
            Debug.Log($"  {kvp.Key}: {s.alive}/{s.total}생존 | 딜 {s.dmgDealt} | 피해 {s.dmgTaken} | 처치 {s.kills}");
        }
    }

    /// <summary>
    /// 상성 배수 계산 — 현재 비활성화 (행동 패턴으로 차별화)
    /// </summary>
    public float GetTypeMultiplier(RpsType attacker, RpsType defender)
    {
        return 1f;
    }
}
