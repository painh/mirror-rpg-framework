using UnityEngine;
using System;

namespace Combat
{
    /// <summary>
    /// 데미지 처리 공유 로직
    /// Projectile과 WeaponHitbox에서 공통으로 사용
    /// </summary>
    public static class DamageHelper
    {
        /// <summary>
        /// 커스텀 데미지 처리 콜백 (네트워크 등 외부 시스템 연동용)
        /// true 반환 시 기본 데미지 처리를 건너뜀
        /// </summary>
        public static Func<IDamageable, DamageInfo, bool> OnBeforeDamage;

        /// <summary>
        /// 데미지 처리 후 콜백
        /// </summary>
        public static Action<IDamageable, DamageInfo> OnAfterDamage;

        /// <summary>
        /// 히트 오브젝트에 데미지 적용
        /// </summary>
        /// <param name="hitObject">충돌한 오브젝트</param>
        /// <param name="baseDamage">기본 데미지</param>
        /// <param name="attacker">공격자 (owner)</param>
        /// <param name="damageDealer">데미지 딜러 (히트 추적용)</param>
        /// <param name="damageType">데미지 속성 타입</param>
        /// <returns>데미지 적용 성공 여부</returns>
        public static bool ApplyDamage(
            GameObject hitObject,
            float baseDamage,
            GameObject attacker,
            IDamageDealer damageDealer = null,
            DamageType damageType = DamageType.None)
        {
            if (hitObject == null) return false;

            // IDamageable 찾기 (Hurtbox든 일반 콜라이더든)
            IDamageable damageable = hitObject.GetComponent<IDamageable>();
            if (damageable == null)
            {
                damageable = hitObject.GetComponentInParent<IDamageable>();
            }

            // IDamageable이 없으면 데미지 처리 불가
            if (damageable == null) return false;

            // 다단 히트 방지 - 같은 타겟에 이미 데미지를 줬으면 무시
            if (damageDealer != null)
            {
                if (damageDealer.HasHitTarget(damageable)) return false;
                damageDealer.RegisterHit(damageable);
            }

            // DamageInfo 생성
            var damageInfo = new DamageInfo(baseDamage, attacker)
            {
                DamageType = damageType,
                HitPosition = hitObject.transform.position
            };

            // 1. Hurtbox 체크 (부위별 데미지 배율 적용)
            Hurtbox hurtbox = hitObject.GetComponent<Hurtbox>();
            if (hurtbox != null)
            {
                hurtbox.OnHit(damageInfo);
                return true;
            }

            // 2. Hurtbox가 없는 일반 콜라이더 - 기본 데미지 처리
            return ApplyDamageDirect(damageable, damageInfo);
        }

        /// <summary>
        /// IDamageable에 직접 데미지 적용
        /// </summary>
        public static bool ApplyDamageDirect(IDamageable damageable, DamageInfo damageInfo)
        {
            if (damageable == null || !damageable.IsAlive) return false;

            // 커스텀 처리 콜백 (네트워크 등)
            if (OnBeforeDamage != null && OnBeforeDamage(damageable, damageInfo))
            {
                return true; // 외부에서 처리됨
            }

            // 기본 데미지 처리
            damageable.TakeDamage(damageInfo);

            // 후처리 콜백
            OnAfterDamage?.Invoke(damageable, damageInfo);

            return true;
        }

        /// <summary>
        /// 공격자 자신인지 확인
        /// </summary>
        public static bool IsOwner(GameObject obj, GameObject owner)
        {
            if (obj == null || owner == null) return false;
            return obj == owner || obj.transform.root.gameObject == owner;
        }
    }
}
