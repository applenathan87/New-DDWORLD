using UnityEngine;

/// <summary>
/// 화살 투사체. 포물선으로 날아가서 도착 시 명중 판정.
/// 지금은 Quad 색상, 나중에 PNG 스프라이트로 교체 가능.
/// </summary>
public class Arrow : MonoBehaviour
{
    private Vector3 startPos;
    private Vector3 targetPos;
    private float arcHeight;
    private float flightDuration;
    private float elapsed;

    private Soldier shooter;
    private Soldier target;
    private int damage;
    private float accuracy; // 0~1, 1이면 100% 명중

    private bool arrived;

    /// <summary>
    /// 화살 발사
    /// </summary>
    public static Arrow Fire(Soldier from, Soldier to, int dmg, float acc = 0.75f)
    {
        var obj = new GameObject("Arrow");
        var arrow = obj.AddComponent<Arrow>();
        arrow.Init(from, to, dmg, acc);
        return arrow;
    }

    private void Init(Soldier from, Soldier to, int dmg, float acc)
    {
        shooter = from;
        target = to;
        damage = dmg;
        accuracy = acc;

        startPos = from.transform.position + Vector3.up * 0.15f;
        targetPos = to.transform.position + Vector3.up * 0.05f;

        float dist = Vector3.Distance(startPos, targetPos);
        arcHeight = dist * 0.3f;           // 거리에 비례한 포물선 높이
        flightDuration = dist * 0.15f;     // 거리에 비례한 비행 시간
        flightDuration = Mathf.Clamp(flightDuration, 0.3f, 1.2f);
        elapsed = 0f;
        arrived = false;

        transform.position = startPos;

        // 비주얼: 작은 Quad (나중에 PNG 교체)
        CreateVisual(from.isPlayerSide);
    }

    private void CreateVisual(bool isPlayer)
    {
        var meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");

        var renderer = gameObject.AddComponent<MeshRenderer>();
        var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
        var mat = new Material(shader);
        // 아군 화살: 흰색, 적 화살: 연빨강
        mat.color = isPlayer
            ? new Color(1f, 1f, 0.8f)
            : new Color(1f, 0.6f, 0.5f);
        renderer.material = mat;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        transform.localScale = new Vector3(0.06f, 0.02f, 1f);
    }

    private void Update()
    {
        if (arrived) return;

        elapsed += Time.deltaTime;
        float t = elapsed / flightDuration;

        if (t >= 1f)
        {
            t = 1f;
            arrived = true;
        }

        // 타겟이 살아있으면 위치 추적 (유도)
        if (target != null && !target.isDead)
            targetPos = target.transform.position + Vector3.up * 0.05f;

        // XZ 선형 보간
        Vector3 pos = Vector3.Lerp(startPos, targetPos, t);

        // Y 포물선: 0에서 시작 → 중간에 최고점 → 0으로 복귀
        float yArc = arcHeight * 4f * t * (1f - t);
        pos.y += yArc;

        transform.position = pos;

        // 화살이 진행 방향을 바라봄
        if (t < 0.95f)
        {
            float nextT = Mathf.Min(t + 0.05f, 1f);
            Vector3 nextPos = Vector3.Lerp(startPos, targetPos, nextT);
            float nextY = arcHeight * 4f * nextT * (1f - nextT);
            nextPos.y += nextY;

            Vector3 dir = (nextPos - pos).normalized;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        // 도착
        if (arrived)
            OnArrive();
    }

    private void OnArrive()
    {
        // 명중 판정
        bool hit = Random.value <= accuracy;

        if (hit && target != null && !target.isDead)
        {
            if (shooter != null) shooter.totalDamageDealt += damage;
            bool killed = target.TakeDamage(damage, shooter);
            if (killed && shooter != null) shooter.killCount++;
        }

        // 화살 제거 (살짝 딜레이)
        Destroy(gameObject, 0.1f);
    }
}
