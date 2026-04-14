/// <summary>
/// 병종 분류 (상성 판정에 사용)
/// </summary>
public enum UnitCategory
{
    Melee,   // 근접 (민병, 창병)
    Cavalry, // 기병
    Ranged,  // 원거리 (궁병)
    Special  // 특수 (함정 등)
}

/// <summary>
/// 상성 관계 정의
/// </summary>
public enum RpsType
{
    None,     // 상성 없음 (함정 등)
    Rock,     // 바위 — 기병
    Scissors, // 가위 — 창병
    Paper     // 보 — 궁병
}
