using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MawangHR
{
    /// 집어서 던질 수 있는 책상 소품 — 진짜 물리로 날아가 튕기고 구른다.
    /// 멈추면 마법처럼 사라지고 제자리에 재생성 (이 성의 물건들은 마법이 관리한다).
    /// 날아가는 중에도 다시 잡을 수 있다.
    public class ThrowableProp : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private Camera cam;
        private DeskRig rig;
        private Vector3 home;
        private Quaternion homeRot;
        private Collider col;
        private Rigidbody rb;

        private bool dragging;
        private Vector3 velocity;
        private Vector3 lastPos;
        private float lastTime;
        private Coroutine anim;

        private const float HoverLift = 0.25f;
        private const float MaxSpeed = 5.5f;

        public void Init(Camera camera, DeskRig deskRig)
        {
            cam = camera;
            rig = deskRig;
            home = transform.position;
            homeRot = transform.rotation;
            col = GetComponent<Collider>();

            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            gameObject.AddComponent<ImpactSound>();
        }

        public void OnPointerDown(PointerEventData e)
        {
            if (col != null && !col.enabled) return; // 마법 소멸 중
            if (anim != null) { StopCoroutine(anim); anim = null; }
            rb.isKinematic = true; // 날아가던 것도 잡으면 손에 붙음
            dragging = true;
            velocity = Vector3.zero;
            lastPos = transform.position;
            lastTime = Time.time;
            Sfx.Pick();
            MoveToPointer(e.position);
        }

        public void OnDrag(PointerEventData e)
        {
            if (!dragging) return;
            MoveToPointer(e.position);

            float dt = Mathf.Max(Time.time - lastTime, 0.001f);
            Vector3 instant = (transform.position - lastPos) / dt;
            velocity = Vector3.Lerp(velocity, instant, 0.5f);
            lastPos = transform.position;
            lastTime = Time.time;
        }

        private void MoveToPointer(Vector2 screenPos)
        {
            var plane = new Plane(Vector3.up, new Vector3(0, DeskRig.DeskTop + HoverLift, 0));
            var ray = cam.ScreenPointToRay(screenPos);
            if (plane.Raycast(ray, out float d))
            {
                var p = ray.GetPoint(d);
                p.x = Mathf.Clamp(p.x, -1.5f, 1.5f);
                p.z = Mathf.Clamp(p.z, -0.62f, 1.1f);
                transform.position = p;
            }
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (!dragging) return;
            dragging = false;

            Vector3 v = Vector3.ClampMagnitude(velocity, MaxSpeed);
            Vector3 spin = Random.onUnitSphere * Mathf.Lerp(1f, 9f, v.magnitude / MaxSpeed);
            anim = StartCoroutine(PropPhysics.ThrowAndRespawn(
                transform, rb, col, v, spin, home, homeRot, rig));
        }
    }
}
