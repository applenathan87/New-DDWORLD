using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개별 병사. HP, 이동, 타겟팅, 공격을 스스로 처리한다.
/// 배치 시에는 대형 안에서 비주얼만 담당하고,
/// 전투 시작 시 StartBattle()로 독립 행동을 시작한다.
/// </summary>
public class Soldier : MonoBehaviour
{
    public int maxHP;
    public int currentHP;
    public bool isDead;

    // 전투 상태
    [HideInInspector] public UnitData unitData;
    [HideInInspector] public bool isPlayerSide;
    [HideInInspector] public bool battleStarted;

    // 타겟팅
    [HideInInspector] public Soldier target;
    private float attackTimer;
    private float moveSpeed;
    private float attackRange;   // 공격 사거리 (월드 단위)
    private float attackInterval;

    // HP바
    private GameObject hpBarBg;
    private GameObject hpBarFill;
    private Transform hpBarRoot;
    private bool hpBarVisible;
    private Camera mainCamera;

    private const float BAR_WIDTH = 0.18f;
    private const float BAR_HEIGHT = 0.025f;
    private const float MELEE_RANGE = 0.15f;

    public void Setup(int hp, Color soldierColor)
    {
        maxHP = hp;
        currentHP = hp;
        isDead = false;
        hpBarVisible = false;
        battleStarted = false;
        mainCamera = Camera.main;

        CreateHPBar(soldierColor);
        hpBarRoot.gameObject.SetActive(false);
    }

    /// <summary>
    /// 전투 시작: 대형에서 풀려나 독립 행동 개시
    /// </summary>
    public void StartBattle(UnitData data, bool playerSide, float baseMoveSpeed)
    {
        unitData = data;
        isPlayerSide = playerSide;
        battleStarted = true;
        attackTimer = 0f;

        // 이동 속도
        if (data.moveEveryTick > 0)
            moveSpeed = baseMoveSpeed / data.moveEveryTick;
        else
            moveSpeed = 0f;

        // 공격 사거리
        if (data.attackRange > 0)
        {
            float tileUnit = BattleField.Instance != null
                ? BattleField.Instance.tileSize + BattleField.Instance.tileGap
                : 1.08f;
            attackRange = data.attackRange * tileUnit;
        }
        else
        {
            attackRange = MELEE_RANGE;
        }

        // 공격 간격
        attackInterval = data.shootEveryTick > 0
            ? data.shootEveryTick * 0.5f
            : 0.5f;

        // 아웃라인 색 변경 (아군: 파랑, 적: 빨강)
        var outline = transform.Find("Outline_" + name.Replace("Soldier_", ""));
        if (outline != null)
        {
            var outlineRenderer = outline.GetComponent<Renderer>();
            if (outlineRenderer != null)
            {
                Color outlineColor = isPlayerSide
                    ? new Color(0.15f, 0.4f, 0.9f)
                    : new Color(0.9f, 0.2f, 0.15f);
                outlineRenderer.material.color = outlineColor;
            }
        }

        // 부모에서 분리 → 월드 좌표로 독립
        transform.SetParent(null);

        // 사거리 시각화 (원거리 유닛만)
        if (data.attackRange > 0)
            CreateRangeIndicator();
    }

    // === 사거리 시각화 ===

    private GameObject rangeIndicator;

    private void CreateRangeIndicator()
    {
        int segments = 48;
        rangeIndicator = new GameObject("RangeIndicator");
        // 부모를 병사가 아닌 월드에 두고, 매 프레임 위치만 따라감
        rangeIndicator.transform.position = transform.position;

        var lr = rangeIndicator.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.positionCount = segments;
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color"));
        Color rangeColor = isPlayerSide
            ? new Color(0.3f, 0.6f, 1f, 0.4f)
            : new Color(1f, 0.3f, 0.3f, 0.4f);
        mat.color = rangeColor;
        mat.SetFloat("_Surface", 1);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3050;
        lr.material = mat;

        UpdateRangeIndicatorPositions();
    }

    private void UpdateRangeIndicatorPositions()
    {
        if (rangeIndicator == null) return;
        var lr = rangeIndicator.GetComponent<LineRenderer>();
        if (lr == null) return;

        Vector3 center = transform.position;
        center.y = 0.02f; // 바닥 바로 위

        int segments = lr.positionCount;
        float angleStep = 360f / segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = center.x + Mathf.Cos(angle) * attackRange;
            float z = center.z + Mathf.Sin(angle) * attackRange;
            lr.SetPosition(i, new Vector3(x, center.y, z));
        }
    }

    private const float SEPARATION_RADIUS = 0.3f;   // 이 거리 이내면 밀어냄
    private const float SEPARATION_FORCE = 2.5f;     // 밀어내는 힘

    /// <summary>
    /// 매 프레임 호출 (BattleSimulator에서)
    /// </summary>
    public void UpdateBattle(List<Soldier> allies, List<Soldier> enemies, float fieldMinX, float fieldMaxX, float fieldMinZ, float fieldMaxZ)
    {
        if (isDead || !battleStarted) return;

        if (target != null && target.isDead)
            target = null;

        if (target == null)
            target = FindNearestEnemy(enemies);

        if (target == null) return;

        float dt = Time.deltaTime;
        float dist = Vector3.Distance(transform.position, target.transform.position);

        if (dist <= attackRange)
        {
            Attack();
        }
        else if (moveSpeed > 0)
        {
            Vector3 moveDir = (target.transform.position - transform.position).normalized;
            Vector3 pos = transform.position + moveDir * moveSpeed * dt;
            pos.x = Mathf.Clamp(pos.x, fieldMinX, fieldMaxX);
            pos.z = Mathf.Clamp(pos.z, fieldMinZ, fieldMaxZ);
            transform.position = pos;
        }

        ApplySeparation(allies, fieldMinX, fieldMaxX, fieldMinZ, fieldMaxZ, dt);
    }

    private void ApplySeparation(List<Soldier> allies, float fieldMinX, float fieldMaxX, float fieldMinZ, float fieldMaxZ, float dt)
    {
        Vector3 push = Vector3.zero;
        int count = 0;

        foreach (var ally in allies)
        {
            if (ally == this || ally.isDead) continue;

            Vector3 diff = transform.position - ally.transform.position;
            float dist = diff.magnitude;

            if (dist < SEPARATION_RADIUS && dist > 0.001f)
            {
                // 가까울수록 강하게 밀어냄
                push += diff.normalized * (SEPARATION_RADIUS - dist);
                count++;
            }
        }

        if (count > 0)
        {
            Vector3 pos = transform.position + push * SEPARATION_FORCE * dt;
            pos.x = Mathf.Clamp(pos.x, fieldMinX, fieldMaxX);
            pos.z = Mathf.Clamp(pos.z, fieldMinZ, fieldMaxZ);
            pos.y = transform.position.y;
            transform.position = pos;
        }
    }

    private Soldier FindNearestEnemy(List<Soldier> enemies)
    {
        Soldier nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var e in enemies)
        {
            if (e.isDead) continue;
            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = e;
            }
        }
        return nearest;
    }

    private void Attack()
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer > 0) return;
        attackTimer = attackInterval;

        if (target == null || target.isDead) return;

        float multiplier = BattleSimulator.Instance != null
            ? BattleSimulator.Instance.GetTypeMultiplier(unitData.rpsType, target.unitData.rpsType)
            : 1f;

        int damage = Mathf.Max(1, Mathf.RoundToInt(unitData.attack * multiplier) - target.unitData.defense);
        target.TakeDamage(damage);
    }

    // === HP ===

    public bool TakeDamage(int damage)
    {
        if (isDead) return true;

        currentHP -= damage;

        if (!hpBarVisible)
        {
            hpBarVisible = true;
            hpBarRoot.gameObject.SetActive(true);
        }

        UpdateHPBar();

        if (currentHP <= 0)
        {
            currentHP = 0;
            isDead = true;
            Die();
            return true;
        }
        return false;
    }

    private void Die()
    {
        battleStarted = false;

        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Color c = renderer.material.color;
            renderer.material.color = new Color(c.r * 0.3f, c.g * 0.3f, c.b * 0.3f, 0.5f);
        }

        // 아웃라인 숨기기 (같은 부모 아래 있을 때만)
        var outlineName = $"Outline_{name.Replace("Soldier_", "")}";
        var outline = transform.parent?.Find(outlineName);
        if (outline != null) outline.gameObject.SetActive(false);

        // 쓰러지는 연출
        transform.localRotation *= Quaternion.Euler(0, 0, 90);
        transform.position += Vector3.down * 0.03f;

        if (hpBarRoot != null) hpBarRoot.gameObject.SetActive(false);
        if (rangeIndicator != null) rangeIndicator.SetActive(false);
    }

    // === HP바 ===

    private void LateUpdate()
    {
        if (hpBarVisible && hpBarRoot != null && mainCamera != null)
        {
            hpBarRoot.rotation = mainCamera.transform.rotation;
        }

        // 사거리 원이 병사를 따라감
        if (rangeIndicator != null && !isDead)
        {
            UpdateRangeIndicatorPositions();
        }
    }

    private void CreateHPBar(Color soldierColor)
    {
        var shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");

        var root = new GameObject("HPBar");
        root.transform.SetParent(transform);
        root.transform.position = transform.position + Vector3.up * 0.25f;
        hpBarRoot = root.transform;

        hpBarBg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        hpBarBg.name = "HPBarBg";
        hpBarBg.transform.SetParent(hpBarRoot);
        hpBarBg.transform.localPosition = Vector3.zero;
        hpBarBg.transform.localRotation = Quaternion.identity;
        hpBarBg.transform.localScale = new Vector3(BAR_WIDTH, BAR_HEIGHT, 1f);
        Destroy(hpBarBg.GetComponent<Collider>());

        var bgMat = new Material(shader);
        bgMat.color = new Color(0, 0, 0, 0.8f);
        bgMat.SetFloat("_Surface", 1);
        bgMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        bgMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        bgMat.SetInt("_ZWrite", 0);
        bgMat.renderQueue = 3200;
        hpBarBg.GetComponent<Renderer>().material = bgMat;
        hpBarBg.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        hpBarFill = GameObject.CreatePrimitive(PrimitiveType.Quad);
        hpBarFill.name = "HPBarFill";
        hpBarFill.transform.SetParent(hpBarRoot);
        hpBarFill.transform.localPosition = new Vector3(0, 0, -0.001f);
        hpBarFill.transform.localRotation = Quaternion.identity;
        hpBarFill.transform.localScale = new Vector3(BAR_WIDTH, BAR_HEIGHT, 1f);
        Destroy(hpBarFill.GetComponent<Collider>());

        var fillMat = new Material(shader);
        fillMat.color = Color.green;
        fillMat.renderQueue = 3201;
        hpBarFill.GetComponent<Renderer>().material = fillMat;
        hpBarFill.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    private void UpdateHPBar()
    {
        if (hpBarFill == null) return;

        float ratio = (float)currentHP / maxHP;

        hpBarFill.transform.localScale = new Vector3(BAR_WIDTH * ratio, BAR_HEIGHT, 1f);
        float xOffset = -BAR_WIDTH * (1f - ratio) * 0.5f;
        hpBarFill.transform.localPosition = new Vector3(xOffset, 0, -0.001f);

        Color barColor;
        if (ratio > 0.5f)
            barColor = Color.Lerp(Color.yellow, Color.green, (ratio - 0.5f) * 2f);
        else
            barColor = Color.Lerp(Color.red, Color.yellow, ratio * 2f);

        hpBarFill.GetComponent<Renderer>().material.color = barColor;
    }
}
