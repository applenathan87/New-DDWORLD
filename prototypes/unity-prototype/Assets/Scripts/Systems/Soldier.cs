using UnityEngine;

/// <summary>
/// 개별 병사. 캡슐 오브젝트에 붙어서 HP와 HP바를 관리한다.
/// </summary>
public class Soldier : MonoBehaviour
{
    public int maxHP;
    public int currentHP;
    public bool isDead;

    private GameObject hpBarBg;
    private GameObject hpBarFill;
    private Transform hpBarRoot;
    private bool hpBarVisible;

    // HP바 크기
    private const float BAR_WIDTH = 0.1f;
    private const float BAR_HEIGHT = 0.012f;
    private const float BAR_OFFSET_Z = -0.1f; // 캡슐 위 (로컬 -Z = 위)

    public void Setup(int hp, Color soldierColor)
    {
        maxHP = hp;
        currentHP = hp;
        isDead = false;
        hpBarVisible = false;

        CreateHPBar(soldierColor);
        hpBarRoot.gameObject.SetActive(false);
    }

    /// <summary>
    /// 데미지를 받는다. 죽으면 true 반환.
    /// </summary>
    public bool TakeDamage(int damage)
    {
        if (isDead) return true;

        currentHP -= damage;

        // 처음 데미지를 받으면 HP바 표시
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
        // 캡슐을 어둡게 + 쓰러뜨리기
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Color c = renderer.material.color;
            renderer.material.color = new Color(c.r * 0.3f, c.g * 0.3f, c.b * 0.3f, 0.5f);
        }

        // 아웃라인도 숨기기
        var outline = transform.parent?.Find($"Outline_{name.Replace("Soldier_", "")}");
        if (outline != null) outline.gameObject.SetActive(false);

        // 쓰러지는 연출 (로컬 회전)
        transform.localRotation *= Quaternion.Euler(0, 0, 90);
        transform.localPosition += new Vector3(0, 0, 0.03f); // 살짝 내려감

        // HP바 숨기기
        if (hpBarRoot != null) hpBarRoot.gameObject.SetActive(false);
    }

    private void CreateHPBar(Color soldierColor)
    {
        var shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");

        // HP바 루트 (캡슐 머리 위)
        var root = new GameObject("HPBar");
        root.transform.SetParent(transform);
        root.transform.localPosition = new Vector3(0, 0, BAR_OFFSET_Z);
        // 타일 로컬 좌표계에서 카메라를 향하도록 회전
        root.transform.localRotation = Quaternion.Euler(90, 0, 0);
        hpBarRoot = root.transform;

        // 배경 (검은색)
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

        // 체력바 (초록 → 빨강)
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

        // 스케일: 왼쪽 정렬
        hpBarFill.transform.localScale = new Vector3(BAR_WIDTH * ratio, BAR_HEIGHT, 1f);
        // 위치: 줄어든 만큼 왼쪽으로 이동
        float xOffset = -BAR_WIDTH * (1f - ratio) * 0.5f;
        hpBarFill.transform.localPosition = new Vector3(xOffset, 0, -0.001f);

        // 색상: 초록 → 노랑 → 빨강
        Color barColor;
        if (ratio > 0.5f)
            barColor = Color.Lerp(Color.yellow, Color.green, (ratio - 0.5f) * 2f);
        else
            barColor = Color.Lerp(Color.red, Color.yellow, ratio * 2f);

        hpBarFill.GetComponent<Renderer>().material.color = barColor;
    }
}
