using UnityEngine;
using TMPro;

/// <summary>
/// 플로팅 텍스트: 위로 떠오르면서 페이드아웃 후 자동 제거
/// </summary>
public class FloatingTextAnim : MonoBehaviour
{
    private Vector3 startPos;
    private float duration;
    private float elapsed;
    private TextMeshPro tmp;
    private Camera cam;

    public void Init(Vector3 pos, float dur)
    {
        startPos = pos;
        duration = dur;
        elapsed = 0f;
        tmp = GetComponent<TextMeshPro>();
        cam = Camera.main;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;

        if (t >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        // 위로 떠오름
        transform.position = startPos + Vector3.up * t * 0.3f;

        // 카메라를 바라봄
        if (cam != null)
            transform.rotation = cam.transform.rotation;

        // 페이드아웃
        if (tmp != null)
        {
            Color c = tmp.color;
            c.a = 1f - t;
            tmp.color = c;
        }
    }
}
