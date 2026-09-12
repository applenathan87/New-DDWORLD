using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MawangHR
{
    /// 스케줄링용 사진 카드 — 클릭 = 수정구 통화 / 끌기 = 캘린더 배치.
    public class PhotoDraggable : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private Camera cam;
        private Func<bool> canUse;
        private Action onCall;                 // 클릭 (드래그 없이 놓음)
        private Action<Vector2> onDrop;        // 드래그 후 놓음 (스크린 좌표)
        private Action onGrab;                 // 잡는 순간 (끌면서 읽기용 확대 등)
        private Vector3 grabOffset;
        private bool dragging;
        private bool draggedInGesture;

        public void Init(Camera camera, Func<bool> canUseFn, Action onCallFn, Action<Vector2> onDropFn,
            Action onGrabFn = null)
        {
            cam = camera;
            canUse = canUseFn;
            onCall = onCallFn;
            onDrop = onDropFn;
            onGrab = onGrabFn;
        }

        public void OnPointerDown(PointerEventData e)
        {
            draggedInGesture = false;
            if (e.button != PointerEventData.InputButton.Left) return;
            if (canUse == null || !canUse()) return;
            if (RayToPlane(e.position, out var hit))
            {
                grabOffset = transform.position - hit;
                dragging = true;
                transform.localScale *= 1.06f;
                onGrab?.Invoke(); // 절대 스케일을 덮어써도 됨 — 놓을 때 onCall/onDrop이 최종 스케일을 다시 정한다
            }
        }

        public void OnDrag(PointerEventData e)
        {
            if (!dragging || !canUse()) return;
            draggedInGesture = true;
            if (RayToPlane(e.position, out var hit))
                transform.position = hit + grabOffset;
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (!dragging) return;
            dragging = false;
            transform.localScale /= 1.06f;
            if (draggedInGesture) onDrop?.Invoke(e.position);
            else onCall?.Invoke(); // 제자리 클릭 = 통화
        }

        private bool RayToPlane(Vector2 screenPos, out Vector3 hit)
        {
            var plane = new Plane(cam.transform.forward, transform.position);
            var ray = cam.ScreenPointToRay(screenPos);
            if (plane.Raycast(ray, out float d)) { hit = ray.GetPoint(d); return true; }
            hit = default;
            return false;
        }
    }
}
