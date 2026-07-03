using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MawangHR
{
    /// 3D 도장 — 잡으면 떠오르고, 서류(책상 위든 들려 있든) 위에서 놓으면
    /// **커서가 가리킨 바로 그 지점**으로 낙하해 찍는다 (잉크 자국과 정확히 일치).
    public class StampDraggable3D : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private Camera cam;
        private Vector3 home;
        private Quaternion homeRot;
        private bool isPass;
        private RectTransform paper;   // 이력서 캔버스 (드롭 판정·낙하 목표)
        private float deskTop;
        private Func<bool> canStamp;
        private Action<bool, Vector2> onSlam;
        private Action onBlocked;

        private bool dragging;
        private Coroutine anim;

        private const float HoverLift = 0.22f;   // 책상 위 공중 높이
        private const float PaperHover = 0.13f;  // 종이 면 위 공중 높이
        private const float BaseHalf = 0.055f;   // 도장 밑면 오프셋

        public void Init(bool pass, Camera camera, RectTransform paperRect, float deskTopY,
            Func<bool> canStampFn, Action<bool, Vector2> onSlamFn, Action onBlockedFn)
        {
            isPass = pass;
            cam = camera;
            paper = paperRect;
            deskTop = deskTopY;
            canStamp = canStampFn;
            onSlam = onSlamFn;
            onBlocked = onBlockedFn;
            home = transform.position;
            homeRot = transform.rotation;
        }

        /// 종이 앞면 법선 (캔버스 front = -forward)
        private Vector3 PaperNormal => -paper.forward;

        public void OnPointerDown(PointerEventData e)
        {
            if (anim != null) { StopCoroutine(anim); anim = null; }
            dragging = true;
            Sfx.Pick();
            MoveToPointer(e.position);
        }

        public void OnDrag(PointerEventData e)
        {
            if (dragging) MoveToPointer(e.position);
        }

        private void MoveToPointer(Vector2 screenPos)
        {
            var ray = cam.ScreenPointToRay(screenPos);

            // 커서가 서류 위에 있으면 — 서류 면을 따라 떠서 이동 (들려 있는 종이 포함)
            if (paper != null && RectTransformUtility.RectangleContainsScreenPoint(paper, screenPos, cam))
            {
                Vector3 n = PaperNormal;
                var pplane = new Plane(n, paper.position);
                if (pplane.Raycast(ray, out float pd))
                {
                    transform.position = ray.GetPoint(pd) + n * PaperHover;
                    transform.rotation = Quaternion.FromToRotation(Vector3.up, n) * Quaternion.Euler(-7f, 0f, 7f);
                    return;
                }
            }

            // 그 외 — 책상 위 공중 평면을 따라 이동
            var plane = new Plane(Vector3.up, new Vector3(0, deskTop + HoverLift, 0));
            if (plane.Raycast(ray, out float d))
            {
                var p = ray.GetPoint(d);
                p.x = Mathf.Clamp(p.x, -1.05f, 1.05f);
                p.z = Mathf.Clamp(p.z, -0.58f, 0.50f);
                transform.position = p;
            }
            transform.rotation = Quaternion.Euler(-7f, 0f, 7f);
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (!dragging) return;
            dragging = false;

            bool overPaper = paper != null &&
                RectTransformUtility.RectangleContainsScreenPoint(paper, e.position, cam);

            if (overPaper && canStamp != null && canStamp())
            {
                onSlam?.Invoke(isPass, e.position); // 판정 기록 + 잉크·쾅은 GameFlow (낙하와 동기)
                anim = StartCoroutine(SlamAnim(e.position));
                return;
            }

            if (overPaper) onBlocked?.Invoke();
            anim = StartCoroutine(ReturnHome(0.18f));
        }

        private IEnumerator SlamAnim(Vector2 screenPos)
        {
            // 낙하 목표 = 커서 광선이 종이 면과 만나는 바로 그 지점 (잉크 자국과 일치)
            Vector3 n = PaperNormal;
            var pplane = new Plane(n, paper.position);
            var ray = cam.ScreenPointToRay(screenPos);
            Vector3 target = pplane.Raycast(ray, out float d)
                ? ray.GetPoint(d) + n * BaseHalf
                : transform.position;
            Quaternion slamRot = Quaternion.FromToRotation(Vector3.up, n);

            Vector3 from = transform.position;
            Quaternion fromRot = transform.rotation;
            float t = 0f;
            const float fall = 0.06f;
            while (t < fall)
            {
                t += Time.deltaTime;
                float k = t / fall;
                transform.position = Vector3.Lerp(from, target, k);
                transform.rotation = Quaternion.Slerp(fromRot, slamRot, k);
                yield return null;
            }
            transform.position = target;
            transform.rotation = slamRot;
            yield return new WaitForSeconds(0.22f);
            yield return ReturnHome(0.25f);
        }

        private IEnumerator ReturnHome(float dur)
        {
            Vector3 from = transform.position;
            Quaternion fromRot = transform.rotation;
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / dur);
                transform.position = Vector3.Lerp(from, home, k)
                    + Vector3.up * (Mathf.Sin(k * Mathf.PI) * 0.10f);
                transform.rotation = Quaternion.Slerp(fromRot, homeRot, k);
                yield return null;
            }
            transform.position = home;
            transform.rotation = homeRot;
            anim = null;
        }
    }
}
