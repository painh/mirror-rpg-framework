using UnityEngine;
using System;
using System.Collections.Generic;

namespace Combat
{
    /// <summary>
    /// 방패에 붙는 가드 충돌체
    /// 가드 중에만 활성화되어 공격을 막음
    /// WeaponHitbox의 공격을 감지하여 OnGuardHit 콜백을 통해 알림
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ShieldHitbox : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("방패 소유자 (자동 설정됨)")]
        [SerializeField] private GameObject owner;

        [Header("Debug")]
        [Tooltip("디버그 로그 출력")]
        public bool debugLog = false;

        [Tooltip("비활성 상태 Gizmo 색상")]
        public Color inactiveColor = new Color(0f, 0f, 0.5f, 0.5f); // 어두운 파란색

        [Tooltip("활성 상태 Gizmo 색상")]
        public Color activeColor = new Color(0f, 0.5f, 1f, 0.7f); // 밝은 파란색

        private Collider shieldCollider;
        private bool isActive;

        // 이번 가드에서 이미 막은 공격 목록 (다단 히트 방지)
        private HashSet<IDamageDealer> blockedAttacks = new HashSet<IDamageDealer>();

        /// <summary>
        /// 가드 히트 콜백
        /// Parameters: damage, hitPoint, attacker
        /// Returns: true if guard was successful, false if guard failed (guard break)
        /// </summary>
        public Func<float, Vector3, GameObject, bool> OnGuardHitCallback { get; set; }

        /// <summary>
        /// 투사체 가드 히트 콜백
        /// Parameters: damage, hitPoint, attacker, projectileObject
        /// Returns: true if guard was successful
        /// </summary>
        public Func<float, Vector3, GameObject, GameObject, bool> OnProjectileGuardHitCallback { get; set; }

        /// <summary>
        /// 방패 소유자
        /// </summary>
        public GameObject Owner => owner;

        void Awake()
        {
            shieldCollider = GetComponent<Collider>();
            shieldCollider.isTrigger = true;

            // Kinematic Rigidbody 추가 (트리거 충돌 감지에 필요)
            var rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            SetActive(false);
        }

        /// <summary>
        /// 방패 초기화
        /// </summary>
        public void Initialize(GameObject shieldOwner)
        {
            owner = shieldOwner;
        }

        /// <summary>
        /// 방패 활성화/비활성화
        /// </summary>
        public void SetActive(bool active)
        {
            isActive = active;

            if (shieldCollider != null)
            {
                shieldCollider.enabled = active;
            }

            if (active)
            {
                blockedAttacks.Clear();

                if (debugLog)
                {
                    Debug.Log($"[ShieldHitbox] Activated: {gameObject.name}");
                }
            }
            else
            {
                if (debugLog)
                {
                    Debug.Log($"[ShieldHitbox] Deactivated: {gameObject.name}");
                }
            }
        }

        /// <summary>
        /// 현재 활성 상태 확인
        /// </summary>
        public bool IsActive => isActive;

        void OnTriggerEnter(Collider other)
        {
            if (!isActive) return;

            // 자기 자신 공격은 무시
            if (IsOwner(other.gameObject)) return;

            // WeaponHitbox인지 확인
            var weaponHitbox = other.GetComponent<WeaponHitbox>();
            if (weaponHitbox != null)
            {
                HandleWeaponHit(weaponHitbox, other);
                return;
            }

            // IDamageDealer 인터페이스 확인 (Projectile 등)
            var damageDealer = other.GetComponent<IDamageDealer>();
            if (damageDealer != null)
            {
                HandleDamageDealerHit(damageDealer, other);
                return;
            }
        }

        /// <summary>
        /// 무기 공격 처리
        /// </summary>
        protected virtual void HandleWeaponHit(WeaponHitbox weaponHitbox, Collider other)
        {
            // 이미 막은 공격인지 확인
            if (blockedAttacks.Contains(weaponHitbox))
            {
                return;
            }

            float damage = weaponHitbox.BaseDamage;
            Vector3 hitPoint = CalculateHitPoint(other);

            if (debugLog)
            {
                Debug.Log($"[ShieldHitbox] Blocked weapon attack! Damage: {damage}, From: {weaponHitbox.Owner?.name ?? "Unknown"}");
            }

            // 콜백을 통해 가드 처리
            bool guardSuccess = true;
            if (OnGuardHitCallback != null)
            {
                guardSuccess = OnGuardHitCallback.Invoke(damage, hitPoint, weaponHitbox.Owner);
            }

            if (guardSuccess)
            {
                // 가드 성공 - 공격 등록
                blockedAttacks.Add(weaponHitbox);

                // WeaponHitbox에도 히트 등록 (다단히트 방지)
                var ownerDamageable = owner?.GetComponent<IDamageable>();
                if (ownerDamageable != null)
                {
                    weaponHitbox.RegisterHit(ownerDamageable);
                }
            }
        }

        /// <summary>
        /// 일반 IDamageDealer (Projectile 등) 처리
        /// </summary>
        protected virtual void HandleDamageDealerHit(IDamageDealer damageDealer, Collider other)
        {
            // 이미 막은 공격인지 확인
            if (blockedAttacks.Contains(damageDealer))
            {
                return;
            }

            float damage = damageDealer.BaseDamage;
            Vector3 hitPoint = CalculateHitPoint(other);

            if (debugLog)
            {
                Debug.Log($"[ShieldHitbox] Blocked attack! Damage: {damage}, From: {damageDealer.Owner?.name ?? "Unknown"}");
            }

            // 콜백을 통해 가드 처리
            bool guardSuccess = true;
            if (OnProjectileGuardHitCallback != null)
            {
                guardSuccess = OnProjectileGuardHitCallback.Invoke(damage, hitPoint, damageDealer.Owner, other.gameObject);
            }
            else if (OnGuardHitCallback != null)
            {
                guardSuccess = OnGuardHitCallback.Invoke(damage, hitPoint, damageDealer.Owner);
            }

            if (guardSuccess)
            {
                blockedAttacks.Add(damageDealer);
            }
        }

        /// <summary>
        /// 충돌 지점 계산
        /// </summary>
        protected Vector3 CalculateHitPoint(Collider other)
        {
            Vector3 pointOnShield = shieldCollider.ClosestPoint(other.bounds.center);
            Vector3 pointOnAttacker = other.ClosestPoint(shieldCollider.bounds.center);
            return (pointOnShield + pointOnAttacker) / 2f;
        }

        /// <summary>
        /// 소유자인지 확인
        /// </summary>
        protected bool IsOwner(GameObject obj)
        {
            if (obj == null || owner == null) return false;
            return obj == owner || obj.transform.root.gameObject == owner;
        }

        /// <summary>
        /// 블록된 공격 목록 초기화
        /// </summary>
        public void ClearBlockedAttacks()
        {
            blockedAttacks.Clear();
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (shieldCollider == null)
                shieldCollider = GetComponent<Collider>();

            if (shieldCollider == null) return;

            Gizmos.color = isActive ? activeColor : inactiveColor;
            Gizmos.matrix = transform.localToWorldMatrix;

            if (shieldCollider is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
            else if (shieldCollider is CapsuleCollider capsule)
            {
                DrawWireCapsule(capsule.center, capsule.radius, capsule.height, capsule.direction);
            }
            else if (shieldCollider is BoxCollider box)
            {
                Gizmos.DrawWireCube(box.center, box.size);
            }
        }

        private void DrawWireCapsule(Vector3 center, float radius, float height, int direction)
        {
            float halfHeight = Mathf.Max(0, (height / 2) - radius);

            Vector3 up = direction switch
            {
                0 => Vector3.right,
                1 => Vector3.up,
                2 => Vector3.forward,
                _ => Vector3.up
            };

            Vector3 top = center + up * halfHeight;
            Vector3 bottom = center - up * halfHeight;

            Gizmos.DrawWireSphere(top, radius);
            Gizmos.DrawWireSphere(bottom, radius);

            Vector3 right = direction == 0 ? Vector3.up : Vector3.right;
            Vector3 forward = direction == 2 ? Vector3.up : Vector3.forward;

            Gizmos.DrawLine(top + right * radius, bottom + right * radius);
            Gizmos.DrawLine(top - right * radius, bottom - right * radius);
            Gizmos.DrawLine(top + forward * radius, bottom + forward * radius);
            Gizmos.DrawLine(top - forward * radius, bottom - forward * radius);
        }
#endif
    }
}
