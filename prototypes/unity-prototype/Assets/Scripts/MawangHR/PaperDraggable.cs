using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MawangHR
{
    /// 들려 있는 종이를 끌어서 자유 재배치.
    /// 드래그가 있었던 제스처의 클릭(내려놓기·마킹)을 억제하기 위해 LastDragFrame을 기록한다.
    public class PaperDraggable : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public static int LastDragFrame = -999;

        private Camera cam;
        private Func<bool> canDrag;
        private Vector3 grabOffset;
        private bool dragging;
        private bool draggedInGesture;

        public void Init(Camera camera, Func<bool> canDragFn)
        {
            cam = camera;
            canDrag = canDragFn;
        }

        public void OnPointerDown(PointerEventData e)
        {
            draggedInGesture = false;
            if (canDrag == null || !canDrag()) return;
            if (RayToPlane(e.position, out var hit))
            {
                grabOffset = transform.position - hit;
                dragging = true;
            }
        }

        public void OnDrag(PointerEventData e)
        {
            if (!dragging || canDrag == null || !canDrag()) return;
            draggedInGesture = true;
            LastDragFrame = Time.frameCount;
            if (RayToPlane(e.position, out var hit))
                transform.position = ClampToView(hit + grabOffset);
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (draggedInGesture) LastDragFrame = Time.frameCount; // 릴리즈 프레임의 클릭 억제
            dragging = false;
        }

        /// 종이의 현재 깊이에서 카메라를 마주보는 평면과 광선의 교차점
        private bool RayToPlane(Vector2 screenPos, out Vector3 hit)
        {
            var plane = new Plane(cam.transform.forward, transform.position);
            var ray = cam.ScreenPointToRay(screenPos);
            if (plane.Raycast(ray, out float d)) { hit = ray.GetPoint(d); return true; }
            hit = default;
            return false;
        }

        /// 화면 밖으로 끌고 나가지 못하게 시야 기준으로 클램프
        private Vector3 ClampToView(Vector3 pos)
        {
            var ct = cam.transform;
            float dist = Vector3.Dot(pos - ct.position, ct.forward);
            Vector3 center = ct.position + ct.forward * dist;
            float r = Mathf.Clamp(Vector3.Dot(pos - center, ct.right), -0.45f, 0.45f);
            float u = Mathf.Clamp(Vector3.Dot(pos - center, ct.up), -0.26f, 0.26f);
            return center + ct.right * r + ct.up * u;
        }
    }
}
