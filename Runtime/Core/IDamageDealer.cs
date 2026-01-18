using UnityEngine;

namespace Combat
{
    /// <summary>
    /// 데미지를 주는 오브젝트의 공통 인터페이스
    /// Projectile, WeaponHitbox 등이 구현
    /// </summary>
    public interface IDamageDealer
    {
        /// <summary>
        /// 이 데미지 딜러의 소유자 (공격자)
        /// </summary>
        GameObject Owner { get; }

        /// <summary>
        /// 기본 데미지 값
        /// </summary>
        float BaseDamage { get; }

        /// <summary>
        /// 해당 IDamageable에 이미 데미지를 줬는지 확인
        /// </summary>
        bool HasHitTarget(IDamageable target);

        /// <summary>
        /// 타겟에 데미지를 줬음을 등록 (다중 히트 방지)
        /// </summary>
        void RegisterHit(IDamageable target);

        /// <summary>
        /// 히트 추적 초기화 (새 공격 시작 시)
        /// </summary>
        void ResetHitTracking();
    }
}
