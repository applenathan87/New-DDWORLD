using UnityEngine;

/// <summary>
/// 병종 하나의 스탯을 정의하는 ScriptableObject.
/// Unity 에디터에서 Assets > Create > DDWORLD > Unit Data 로 생성 가능.
/// </summary>
[CreateAssetMenu(fileName = "NewUnit", menuName = "DDWORLD/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("기본 정보")]
    public string unitName;         // 병종 이름 (예: 기병)
    public Sprite icon;             // 카드에 표시할 아이콘
    public UnitCategory category;   // 분류
    public RpsType rpsType;         // 상성 타입

    [Header("분대 구성")]
    public int soldierCount;        // 분대 인원 수 (예: 기병 5, 민병 20)
    public int soldierHP;           // 병사 1명의 HP

    [Header("전투 스탯")]
    public int attack;              // 공격력
    public int defense;             // 방어력

    [Header("이동")]
    public int moveEveryTick;       // N틱마다 1칸 이동 (0이면 이동 안 함)

    [Header("원거리 (궁병 전용)")]
    public int attackRange;         // 사거리 (0이면 근접)
    public int shootEveryTick;      // N틱마다 1회 사격 (0이면 사격 안 함)

    [Header("특수 (함정 전용)")]
    public int trapDamage;          // 함정 폭발 데미지 (0이면 함정 아님)
}
