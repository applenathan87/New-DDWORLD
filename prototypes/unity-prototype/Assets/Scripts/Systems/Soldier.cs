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

    // 전투 통계
    [HideInInspector] public int totalDamageDealt;
    [HideInInspector] public int totalDamageTaken;
    [HideInInspector] public int killCount;

    // 타겟팅
    [HideInInspector] public Soldier target;
    private float attackTimer;
    private float moveSpeed;
    private float currentSpeed;
    private float acceleration;
    private float attackRange;
    private float attackInterval;

    // 레인 기반 이동
    private bool inLanePhase = true;  // true: 행 직진, false: 자유 추적
    private float laneBoundaryX;      // 적 3열째 X 좌표 (여기 도달하면 자유 추적)
    private float laneDirection;      // +1(플레이어→적) or -1(적→플레이어)
    private bool hasEngaged;          // 전투 경험 여부 (교전 후 자유 추적 전환)
    private float lastSpeedBeforeAttack; // 공격 직전 속도 (돌격 판정용)
    [HideInInspector] public bool isTrap; // 함정 여부 (타겟팅에서 제외)

    // 넉백
    private Vector3 knockbackVelocity;
    private const float KNOCKBACK_DECAY = 8f;

    // HP바
    private GameObject hpBarBg;
    private GameObject hpBarFill;
    private Transform hpBarRoot;
    private bool hpBarVisible;
    private Camera mainCamera;

    private const float BAR_WIDTH = 0.18f;
    private const float BAR_HEIGHT = 0.025f;
    private const float MELEE_RANGE = 0.35f;

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
        currentSpeed = 0f;
        acceleration = 1.5f; // 모든 병종 동일 가속도 — 기병은 빨리 도달, 궁병은 느리게

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

        // 레인 설정
        inLanePhase = true;
        laneDirection = isPlayerSide ? 1f : -1f;
        isTrap = data.trapDamage > 0;

        // 적 진영 3열째 X 좌표 계산
        var bf = BattleField.Instance;
        if (bf != null)
        {
            float tileUnit = bf.tileSize + bf.tileGap;
            // 플레이어: 적 3열째 = col 11 (9+2)
            // 적: 플레이어 3열째 = col 2 (4-2)
            int targetCol = isPlayerSide ? 11 : 2;
            laneBoundaryX = bf.GetTileWorldPosition(targetCol, 0).x;
        }

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

        // 사거리 시각화 + 방향 화살표
        CreateRangeIndicator();
        CreateDirectionArrow();
    }

    /// <summary>
    /// 외부에서 호출: 사거리 원, 방향 화살표 등 기즈모 제거
    /// </summary>
    public void CleanupGizmos()
    {
        if (rangeIndicator != null) Destroy(rangeIndicator);
        if (dirArrow != null) Destroy(dirArrow.gameObject);
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
        bool isRanged = unitData.attackRange > 0;
        Color rangeColor;
        if (isRanged)
            rangeColor = isPlayerSide ? new Color(0.3f, 0.6f, 1f, 0.4f) : new Color(1f, 0.3f, 0.3f, 0.4f);
        else
            rangeColor = isPlayerSide ? new Color(0.3f, 0.9f, 0.4f, 0.3f) : new Color(0.9f, 0.5f, 0.2f, 0.3f);
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
        if (isDead || !battleStarted || isTrap) return;

        float dt = Time.deltaTime;

        // 타겟 정리
        if (target != null && (target.isDead || target.isTrap))
            target = null;

        // 레인 → 자유 전환 체크 (경계 도달 OR 전투 경험 후 타겟 사망)
        if (inLanePhase)
        {
            bool reached = isPlayerSide
                ? transform.position.x >= laneBoundaryX
                : transform.position.x <= laneBoundaryX;
            if (reached || (hasEngaged && (target == null || target.isDead)))
            {
                inLanePhase = false;
                currentSpeed = 0f;  // 자유 추적 전환 시 속도 리셋 (돌격 방지)
                target = null;      // 새로 탐색
            }
        }

        if (inLanePhase)
        {
            // === 레인 페이즈 ===
            bool isRanged = unitData.attackRange > 0;

            if (isRanged)
            {
                // 궁병: 사거리 내 아무 적이든 찾으면 멈춰서 사격
                if (target == null)
                    target = FindEnemyInRange(enemies);

                if (target != null)
                {
                    lastSpeedBeforeAttack = currentSpeed;
                    currentSpeed = 0f;
                    Attack();
                }
                else if (moveSpeed > 0)
                {
                    // 사거리에 아무도 없으면 전진
                    currentSpeed = Mathf.MoveTowards(currentSpeed, moveSpeed, acceleration * dt);
                    Vector3 pos = transform.position;
                    pos.x += laneDirection * currentSpeed * dt;
                    pos.x = Mathf.Clamp(pos.x, fieldMinX, fieldMaxX);
                    transform.position = pos;
                }
            }
            else
            {
                // 근접: 공격 범위 내 적 확인 (인접 행 포함)
                Soldier inRange = FindEnemyInMeleeRange(enemies);
                if (inRange != null)
                {
                    target = inRange;
                    lastSpeedBeforeAttack = currentSpeed;
                    currentSpeed = 0f;
                    Attack();
                }
                else
                {
                    // 공격 범위에 아무도 없으면 X 방향 직진
                    if (target == null)
                        target = FindEnemyInLane(enemies);

                    currentSpeed = Mathf.MoveTowards(currentSpeed, moveSpeed, acceleration * dt);
                    Vector3 pos = transform.position;
                    pos.x += laneDirection * currentSpeed * dt;
                    pos.x = Mathf.Clamp(pos.x, fieldMinX, fieldMaxX);
                    transform.position = pos;
                }
            }
        }
        else
        {
            // === 자유 페이즈: 가장 가까운 적 추적 ===
            if (target == null)
                target = FindNearestEnemy(enemies);

            if (target != null)
            {
                float dist = Vector3.Distance(transform.position, target.transform.position);
                if (dist <= attackRange)
                {
                    lastSpeedBeforeAttack = currentSpeed;
                    currentSpeed = 0f;
                    Attack();
                }
                else if (moveSpeed > 0)
                {
                    currentSpeed = Mathf.MoveTowards(currentSpeed, moveSpeed, acceleration * dt);
                    Vector3 moveDir = (target.transform.position - transform.position).normalized;
                    Vector3 pos = transform.position + moveDir * currentSpeed * dt;
                    pos.x = Mathf.Clamp(pos.x, fieldMinX, fieldMaxX);
                    pos.z = Mathf.Clamp(pos.z, fieldMinZ, fieldMaxZ);
                    transform.position = pos;
                }
            }
        }

        // 함정 밟기 체크
        CheckTrapCollision(enemies);

        // 넉백 감쇠
        if (knockbackVelocity.sqrMagnitude > 0.001f)
        {
            Vector3 pos = transform.position + knockbackVelocity * dt;
            pos.x = Mathf.Clamp(pos.x, fieldMinX, fieldMaxX);
            pos.z = Mathf.Clamp(pos.z, fieldMinZ, fieldMaxZ);
            pos.y = transform.position.y;
            transform.position = pos;
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, KNOCKBACK_DECAY * dt);
        }

        ApplySeparation(allies, fieldMinX, fieldMaxX, fieldMinZ, fieldMaxZ, dt);
    }

    /// <summary>
    /// 근접 공격 범위 내 적 탐색 (레인 직진 중 옆 행 포함)
    /// </summary>
    private Soldier FindEnemyInMeleeRange(List<Soldier> enemies)
    {
        Soldier nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var e in enemies)
        {
            if (e.isDead || e.isTrap) continue;
            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist <= attackRange && dist < nearestDist)
            {
                nearestDist = dist;
                nearest = e;
            }
        }
        return nearest;
    }

    /// <summary>
    /// 사거리 내 가장 가까운 적 탐색 (궁병 레인 페이즈용, 행 무관)
    /// </summary>
    private Soldier FindEnemyInRange(List<Soldier> enemies)
    {
        Soldier nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var e in enemies)
        {
            if (e.isDead || e.isTrap) continue;
            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist <= attackRange && dist < nearestDist)
            {
                nearestDist = dist;
                nearest = e;
            }
        }
        return nearest;
    }

    /// <summary>
    /// 같은 행에서 정면의 가장 가까운 적 탐색 (레인 페이즈용)
    /// </summary>
    private Soldier FindEnemyInLane(List<Soldier> enemies)
    {
        Soldier nearest = null;
        float nearestDist = float.MaxValue;
        float myZ = transform.position.z;

        foreach (var e in enemies)
        {
            if (e.isDead || e.isTrap) continue;

            // 같은 행 판정: Z 좌표 차이가 작은 적 (±타일 1칸 이내)
            float zDiff = Mathf.Abs(e.transform.position.z - myZ);
            if (zDiff > 1.2f) continue;

            // 정면에 있는 적만 (이미 지나친 적 제외)
            float xDiff = (e.transform.position.x - transform.position.x) * laneDirection;
            if (xDiff < 0) continue; // 뒤에 있는 적은 무시

            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = e;
            }
        }
        return nearest;
    }

    /// <summary>
    /// 함정 밟기 체크 — 함정에 닿으면 타일 전체 범위 폭발 + 넉백
    /// </summary>
    private void CheckTrapCollision(List<Soldier> enemies)
    {
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            var trap = enemies[i];
            if (!trap.isTrap || trap.isDead) continue;

            float dist = Vector3.Distance(transform.position, trap.transform.position);
            if (dist < 0.4f)
            {
                // 함정 폭발! 타일 범위 내 모든 적에게 데미지 + 넉백
                TriggerTrapExplosion(trap);
            }
        }
    }

    private void TriggerTrapExplosion(Soldier trap)
    {
        int trapDmg = trap.unitData.trapDamage > 0 ? trap.unitData.trapDamage : 10;
        // 폭발 반경: BattleSimulator 인스펙터 값 사용 (기본 1.5, 인접 4방향 + 대각선 일부)
        float explosionRadius = BattleSimulator.Instance != null
            ? BattleSimulator.Instance.trapExplosionRadius
            : 1.5f;
        Vector3 trapPos = trap.transform.position;

        // 범위 내 모든 적(= 함정과 반대편 병사)에게 데미지 + 넉백
        var allSoldiers = FindObjectsByType<Soldier>(FindObjectsSortMode.None);
        foreach (var s in allSoldiers)
        {
            if (s.isDead || s.isTrap) continue;
            if (s.isPlayerSide == trap.isPlayerSide) continue; // 같은 편이면 무시

            float d = Vector3.Distance(s.transform.position, trapPos);
            if (d <= explosionRadius)
            {
                // 함정의 통계에 데미지 + 처치 카운트
                trap.totalDamageDealt += trapDmg;
                bool killed = s.TakeDamage(trapDmg, trap);
                if (killed) trap.killCount++;
            }
        }

        // 플로팅 텍스트
        trap.ShowFloatingText("폭발!", Color.red);
        Debug.Log($"[함정 폭발] 범위 {explosionRadius} 내 적에게 {trapDmg} 데미지");

        // 빨간 네모 이펙트 (타일 크기)
        CreateExplosionEffect(trapPos, explosionRadius);

        // 함정 제거
        trap.isDead = true;
        trap.battleStarted = false;
        trap.gameObject.SetActive(false);
    }

    private void CreateExplosionEffect(Vector3 pos, float size)
    {
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "ExplosionEffect";
        quad.transform.position = new Vector3(pos.x, 0.04f, pos.z);
        quad.transform.rotation = Quaternion.Euler(90, 0, 0);
        quad.transform.localScale = new Vector3(size, size, 1f);
        Object.Destroy(quad.GetComponent<Collider>());

        var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
        var mat = new Material(shader);
        mat.color = new Color(1f, 0.1f, 0.1f, 0.6f);
        mat.SetFloat("_Surface", 1);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3050;
        quad.GetComponent<Renderer>().material = mat;

        // 2초 후 서서히 사라짐
        Object.Destroy(quad, 2f);
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
            if (e.isDead || e.isTrap) continue;
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

        int damage = unitData.attack;

        // === 기병 돌격 (최고 속도 도달 시 발동) ===
        bool isCavalry = unitData.category == UnitCategory.Cavalry;
        bool targetIsSpearman = target.unitData.rpsType == RpsType.Scissors;
        bool atFullSpeed = lastSpeedBeforeAttack >= moveSpeed * 0.9f;

        if (isCavalry && atFullSpeed)
        {
            hasEngaged = true;

            if (targetIsSpearman)
            {
                // 창병 돌격 방어: 기병은 6만 주고, 16 반사 받음
                int chargeDealtToSpearman = 6;
                int reflectDamage = 12;

                totalDamageDealt += chargeDealtToSpearman;
                target.TakeDamage(chargeDealtToSpearman, this);
                TakeDamage(reflectDamage, target);

                ShowFloatingText("돌격 방어!", new Color(0.2f, 0.6f, 1f));
                target.ShowFloatingText("반사! 16", new Color(1f, 0.3f, 0.2f));
                Debug.Log($"[돌격 방어] 창병이 기병 돌격을 막음! 기병 -{reflectDamage}, 창병 -{chargeDealtToSpearman}");
                return;
            }
            else
            {
                // 일반 돌격: 풀 데미지
                totalDamageDealt += damage;
                bool killed = target.TakeDamage(damage, this);
                if (killed) killCount++;

                ShowFloatingText("돌격 공격!", new Color(1f, 0.8f, 0.2f));
                Debug.Log($"[돌격 공격] {unitData.unitName} → {target.unitData.unitName} {damage} 데미지");
                return;
            }
        }

        hasEngaged = true;

        // === 일반 공격 ===
        if (unitData.attackRange > 0)
        {
            Arrow.Fire(this, target, damage, 0.75f);
        }
        else
        {
            totalDamageDealt += damage;
            bool killed = target.TakeDamage(damage, this);
            if (killed) killCount++;
        }
    }

    /// <summary>
    /// 머리 위에 플로팅 텍스트 표시
    /// </summary>
    public void ShowFloatingText(string text, Color color)
    {
        var obj = new GameObject("FloatText");
        obj.transform.position = transform.position + Vector3.up * 0.6f;

        var tmp = obj.AddComponent<TMPro.TextMeshPro>();
        // 한글 폰트 연결
        if (BattleSimulator.Instance != null && BattleSimulator.Instance.koreanFont != null)
            tmp.font = BattleSimulator.Instance.koreanFont;
        tmp.text = text;
        tmp.fontSize = 5f;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.rectTransform.sizeDelta = new Vector2(3f, 1f);
        tmp.raycastTarget = false;

        var mono = obj.AddComponent<FloatingTextAnim>();
        mono.Init(obj.transform.position, 1.5f);
    }

    // === 방향 화살표 ===

    private LineRenderer dirArrow;

    /// <summary>
    /// 병사 GameObject 파괴 시 자기가 만든 부속 GameObject도 함께 정리.
    /// (DirArrow와 RangeIndicator는 root에 생성되어 부모-자식 관계가 없음)
    /// </summary>
    private void OnDestroy()
    {
        if (dirArrow != null && dirArrow.gameObject != null)
            Destroy(dirArrow.gameObject);
        if (rangeIndicator != null)
            Destroy(rangeIndicator);
    }
    private const float ARROW_LENGTH = 0.2f;
    private const float ARROW_HEAD = 0.06f;

    private void CreateDirectionArrow()
    {
        var obj = new GameObject("DirArrow");
        dirArrow = obj.AddComponent<LineRenderer>();
        dirArrow.useWorldSpace = true;
        dirArrow.positionCount = 4; // 줄기 2점 + 화살촉 2점
        dirArrow.startWidth = 0.015f;
        dirArrow.endWidth = 0.015f;
        dirArrow.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color"));
        mat.color = isPlayerSide ? new Color(0.8f, 0.8f, 1f, 0.6f) : new Color(1f, 0.8f, 0.8f, 0.6f);
        dirArrow.material = mat;
    }

    private void UpdateDirectionArrow()
    {
        if (dirArrow == null) return;

        if (isDead || target == null || target.isDead)
        {
            dirArrow.enabled = false;
            return;
        }
        dirArrow.enabled = true;

        Vector3 pos = transform.position;
        pos.y = 0.03f;

        Vector3 dir = (target.transform.position - transform.position).normalized;
        dir.y = 0;

        Vector3 tip = pos + dir * ARROW_LENGTH;
        Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;

        // 줄기
        dirArrow.SetPosition(0, pos);
        dirArrow.SetPosition(1, tip);
        // 화살촉 (< 모양)
        dirArrow.SetPosition(2, tip - dir * ARROW_HEAD + right * ARROW_HEAD);
        dirArrow.SetPosition(3, tip - dir * ARROW_HEAD - right * ARROW_HEAD);
    }

    // === HP ===

    private const float KNOCKBACK_FORCE = 0.08f; // 데미지 1당 밀려나는 거리

    public bool TakeDamage(int damage)
    {
        return TakeDamage(damage, null);
    }

    public bool TakeDamage(int damage, Soldier attacker)
    {
        if (isDead) return true;

        currentHP -= damage;
        totalDamageTaken += damage;

        bool willDie = currentHP <= 0;

        // 넉백: 살아남았을 때만 — 속도를 부여하여 부드럽게 밀려남
        if (attacker != null && !willDie)
        {
            Vector3 knockDir = (transform.position - attacker.transform.position).normalized;
            knockDir.y = 0;
            knockbackVelocity += knockDir * damage * KNOCKBACK_FORCE * 10f;
            currentSpeed = 0f;
        }

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
        if (dirArrow != null) dirArrow.enabled = false;
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
            UpdateRangeIndicatorPositions();

        // 방향 화살표
        UpdateDirectionArrow();
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
