using System;

namespace Combat
{
    /// <summary>
    /// 데미지 속성 타입
    /// 스킬은 복합 속성을 가질 수 있음 (Flags 사용)
    /// </summary>
    [Flags]
    public enum DamageType
    {
        None = 0,

        // 물리 계열
        PhysicalHit = 1 << 0,      // 물리 타격
        PhysicalSlash = 1 << 1,    // 물리 베기

        // 원소 계열
        Fire = 1 << 2,             // 화염
        Ice = 1 << 3,              // 얼음
        Lightning = 1 << 4,        // 번개

        // 특수 계열
        Light = 1 << 5,            // 빛
        Darkness = 1 << 6,         // 어둠
        Poison = 1 << 7,           // 독
        Bleeding = 1 << 8,         // 출혈
    }
}
