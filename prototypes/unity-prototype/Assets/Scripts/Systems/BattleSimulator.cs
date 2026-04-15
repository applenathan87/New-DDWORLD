using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 시뮬레이션. 개별 병사가 독립적으로 이동/타겟팅/공격한다.
/// BattleSimulator는 병사 리스트 관리, 승패 판정, 상성 계산만 담당.
/// </summary>
public class BattleSimulator : MonoBehaviour
{
    public static BattleSimulator Instance { get; private set; }

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

    public void StartBattle()
    {
        if (battleRunning) return;

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
            if (!s.isDead) count++;
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
        PlacementUI.Instance?.ShowPhaseTitle(result);
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

    /// <summary>
    /// 상성 배수 계산 (Soldier.Attack에서 호출)
    /// </summary>
    public float GetTypeMultiplier(RpsType attacker, RpsType defender)
    {
        if (attacker == RpsType.None || defender == RpsType.None) return 1f;
        if (attacker == defender) return 1f;

        // Rock(기병) > Paper(궁병), Scissors(창병) > Rock(기병), Paper(궁병) > Scissors(창병)
        if ((attacker == RpsType.Rock && defender == RpsType.Paper) ||
            (attacker == RpsType.Scissors && defender == RpsType.Rock) ||
            (attacker == RpsType.Paper && defender == RpsType.Scissors))
        {
            return typeAdvantage;
        }

        return typeDisadvantage;
    }
}
