using UnityEngine;

namespace Combat
{
    /// <summary>
    /// 데미지를 받을 수 있는 엔티티의 인터페이스
    /// Entity 클래스 대신 이 인터페이스를 구현하여 Combat 시스템과 연동
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// 이 엔티티의 GameObject
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// 살아있는 상태인지
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// 데미지 처리
        /// </summary>
        /// <param name="info">데미지 정보</param>
        void TakeDamage(DamageInfo info);
    }
}
