using UnityEngine;

namespace DDworld.HexPrototype
{
    /// <summary>
    /// 축(axial) 헥사 좌표 (pointy-top). 던져버릴 프로토타입용 유틸.
    /// q,r 축 좌표 + 큐브 s(-q-r)로 거리/이웃 계산.
    /// </summary>
    public struct HexCoord
    {
        public int q;
        public int r;

        public HexCoord(int q, int r) { this.q = q; this.r = r; }

        public int s => -q - r;

        // 6방향 이웃 (pointy-top axial)
        public static readonly HexCoord[] Directions =
        {
            new HexCoord(1, 0), new HexCoord(1, -1), new HexCoord(0, -1),
            new HexCoord(-1, 0), new HexCoord(-1, 1), new HexCoord(0, 1),
        };

        public HexCoord Neighbor(int dir)
            => new HexCoord(q + Directions[dir].q, r + Directions[dir].r);

        public static int Distance(HexCoord a, HexCoord b)
            => (Mathf.Abs(a.q - b.q) + Mathf.Abs(a.r - b.r) + Mathf.Abs(a.s - b.s)) / 2;

        /// <summary>축 좌표 → 월드 위치 (XZ 평면, pointy-top). size = 헥사 외접 반지름.</summary>
        public Vector3 ToWorld(float size)
        {
            float x = size * Mathf.Sqrt(3f) * (q + r / 2f);
            float z = size * 1.5f * r;
            return new Vector3(x, 0f, z);
        }

        public override bool Equals(object obj) => obj is HexCoord h && h.q == q && h.r == r;
        public override int GetHashCode() => q * 100000 + r;
        public override string ToString() => $"({q},{r})";
    }
}
