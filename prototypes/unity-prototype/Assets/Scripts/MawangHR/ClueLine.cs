using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MawangHR
{
    /// 이력서의 단서 한 줄 — 깃펜 방향 마킹.
    /// 좌클릭 = V (합격 신호) / 우클릭 = X (탈락 신호)
    public class ClueLine : MonoBehaviour, IPointerClickHandler
    {
        private int index;
        private Action<int, bool> onMark; // (단서 인덱스, 합격 신호인가)

        public void Init(int clueIndex, Action<int, bool> onMarkFn)
        {
            index = clueIndex;
            onMark = onMarkFn;
        }

        public void OnPointerClick(PointerEventData e)
        {
            if (e.button == PointerEventData.InputButton.Left) onMark?.Invoke(index, true);
            else if (e.button == PointerEventData.InputButton.Right) onMark?.Invoke(index, false);
        }
    }
}
