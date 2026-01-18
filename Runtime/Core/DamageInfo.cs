using UnityEngine;

namespace Combat
{
    /// <summary>
    /// 데미지 정보를 담는 구조체
    /// </summary>
    public struct DamageInfo
    {
        /// <summary>
        /// 기본 데미지 양
        /// </summary>
        public float Damage;

        /// <summary>
        /// 크리티컬 히트 여부
        /// </summary>
        public bool IsCritical;

        /// <summary>
        /// 히트 위치 (null이면 대상 위치 사용)
        /// </summary>
        public Vector3? HitPosition;

        /// <summary>
        /// 공격자 GameObject
        /// </summary>
        public GameObject Attacker;

        /// <summary>
        /// 데미지 속성 타입
        /// </summary>
        public DamageType DamageType;

        /// <summary>
        /// 부위 배율 (Hurtbox에서 설정)
        /// </summary>
        public float PartMultiplier;

        /// <summary>
        /// 피격 부위 이름
        /// </summary>
        public string PartName;

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public DamageInfo(float damage, GameObject attacker = null)
        {
            Damage = damage;
            IsCritical = false;
            HitPosition = null;
            Attacker = attacker;
            DamageType = DamageType.None;
            PartMultiplier = 1f;
            PartName = "";
        }

        /// <summary>
        /// 최종 데미지 계산 (기본 데미지 * 부위 배율)
        /// </summary>
        public float FinalDamage => Damage * PartMultiplier;
    }
}
