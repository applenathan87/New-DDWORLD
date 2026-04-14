using UnityEngine;

/// <summary>
/// 카드 한 장을 나타내는 데이터.
/// 덱에 들어가는 단위. 같은 UnitData를 참조하는 카드가 여러 장 존재할 수 있음.
/// </summary>
[System.Serializable]
public class CardData
{
    public UnitData unitData;   // 이 카드가 소환하는 병종
    public int cardId;          // 고유 ID (덱 내에서 구분용)

    public CardData(UnitData data, int id)
    {
        unitData = data;
        cardId = id;
    }
}
