using System.Collections;
using UnityEngine;

namespace MawangHR
{
    /// 던지기 공용 루틴 — 진짜 물리(Rigidbody)로 날아가 튕기고 구르다가,
    /// 멈추면(또는 시야 밖으로 나가면) 마법으로 사라지고 제자리에 재생성.
    public static class PropPhysics
    {
        public static IEnumerator ThrowAndRespawn(Transform t, Rigidbody rb, Collider col,
            Vector3 velocity, Vector3 angularVelocity, Vector3 home, Quaternion homeRot, DeskRig rig)
        {
            // 1) 물리에 맡기기 — 튕김·구르기는 엔진이 처리
            rb.isKinematic = false;
            rb.linearVelocity = velocity;
            rb.angularVelocity = angularVelocity;

            float elapsed = 0f;
            float settledTime = 0f;
            while (elapsed < 5f)
            {
                elapsed += Time.deltaTime;
                var p = t.position;
                if (p.y < -2f || p.magnitude > 9f) break; // 방 밖으로 사라짐

                if (elapsed > 0.4f)
                {
                    bool still = rb.linearVelocity.magnitude < 0.07f && rb.angularVelocity.magnitude < 0.4f;
                    settledTime = still ? settledTime + Time.deltaTime : 0f;
                    if (settledTime > 0.7f) break; // 완전히 멈춤 → 정리 시간
                }
                yield return null;
            }

            // 2) 마법 소멸
            rb.isKinematic = true;
            rig.MagicBurst(t.position + Vector3.up * 0.06f);
            Sfx.Poof();
            t.localScale = Vector3.one * 0.001f;
            col.enabled = false;

            yield return new WaitForSeconds(0.8f);

            // 3) 제자리에 마법 재생성
            t.position = home;
            t.rotation = homeRot;
            rig.MagicBurst(home + Vector3.up * 0.08f);
            Sfx.Shimmer();
            col.enabled = true;

            float g = 0f;
            const float grow = 0.22f;
            while (g < grow)
            {
                g += Time.deltaTime;
                t.localScale = Vector3.one * Mathf.SmoothStep(0f, 1f, g / grow);
                yield return null;
            }
            t.localScale = Vector3.one;
        }
    }

    /// 부딪힐 때 임팩트 크기에 비례한 "톡" 소리
    public class ImpactSound : MonoBehaviour
    {
        private float lastPlay;

        private void OnCollisionEnter(Collision c)
        {
            if (Time.time - lastPlay < 0.08f) return;
            float impact = c.relativeVelocity.magnitude;
            if (impact < 0.4f) return;
            lastPlay = Time.time;
            Sfx.Tok(impact / 4f);
        }
    }
}
