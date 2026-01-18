using UnityEngine;
using System.Collections.Generic;

namespace Combat
{
    /// <summary>
    /// 무기에 붙는 공격 충돌체
    /// 공격 중에만 활성화되어 대상에게 데미지를 줌
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class WeaponHitbox : MonoBehaviour, IDamageDealer
    {
        [Header("Settings")]
        [Tooltip("기본 데미지 (컨트롤러에서 오버라이드 가능)")]
        [SerializeField] private float baseDamage = 10f;

        [Tooltip("데미지 속성 타입")]
        [SerializeField] private DamageType damageType = DamageType.PhysicalHit;

        [Tooltip("공격 주체 (자동 설정됨)")]
        [SerializeField] private GameObject owner;

        [Header("Debug")]
        [Tooltip("디버그 로그 출력")]
        public bool debugLog = false;

        [Tooltip("Gizmo 색상")]
        public Color gizmoColor = Color.red;

        // 이번 공격에서 이미 히트한 타겟 목록
        private HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();
        private Collider hitboxCollider;
        private bool isActive;

        // 공유 히트 추적용 컨트롤러 참조
        private WeaponHitboxController controller;

        // IDamageDealer 구현
        public GameObject Owner => owner;
        public float BaseDamage => baseDamage;

        void Awake()
        {
            hitboxCollider = GetComponent<Collider>();
            hitboxCollider.isTrigger = true;
            SetActive(false);
        }

        /// <summary>
        /// 히트박스 초기화
        /// </summary>
        public void Initialize(GameObject weaponOwner, float damage = 0f)
        {
            owner = weaponOwner;
            if (damage > 0)
            {
                baseDamage = damage;
            }
        }

        /// <summary>
        /// 데미지 값 설정
        /// </summary>
        public void SetDamage(float damage)
        {
            baseDamage = damage;
        }

        /// <summary>
        /// 데미지 타입 설정
        /// </summary>
        public void SetDamageType(DamageType type)
        {
            damageType = type;
        }

        /// <summary>
        /// 컨트롤러 설정 (공유 히트 추적용)
        /// </summary>
        public void SetController(WeaponHitboxController hitboxController)
        {
            controller = hitboxController;
        }

        /// <summary>
        /// 히트박스 활성화/비활성화
        /// </summary>
        public void SetActive(bool active)
        {
            isActive = active;
            hitboxCollider.enabled = active;

            if (active)
            {
                ResetHitTracking();

                if (debugLog)
                {
                    Debug.Log($"[WeaponHitbox] Activated: {gameObject.name}");
                }
            }
            else
            {
                if (debugLog)
                {
                    Debug.Log($"[WeaponHitbox] Deactivated: {gameObject.name}");
                }
            }
        }

        /// <summary>
        /// 공격 시작 (히트 추적 초기화 + 활성화)
        /// </summary>
        public void BeginAttack()
        {
            SetActive(true);
        }

        /// <summary>
        /// 공격 종료 (비활성화)
        /// </summary>
        public void EndAttack()
        {
            SetActive(false);
        }

        // IDamageDealer 구현
        public bool HasHitTarget(IDamageable target)
        {
            return hitTargets.Contains(target);
        }

        public void RegisterHit(IDamageable target)
        {
            hitTargets.Add(target);
        }

        public void ResetHitTracking()
        {
            hitTargets.Clear();
        }

        void OnTriggerEnter(Collider other)
        {
            if (!isActive) return;
            if (DamageHelper.IsOwner(other.gameObject, owner)) return;

            // 공유 히트 추적 확인 (컨트롤러가 있고 스킬이 활성화된 경우)
            if (controller != null && controller.UseSharedHitTracking && controller.IsSkillActive)
            {
                IDamageable targetDamageable = other.GetComponent<IDamageable>();
                if (targetDamageable == null)
                {
                    targetDamageable = other.GetComponentInParent<IDamageable>();
                }

                if (targetDamageable != null)
                {
                    if (controller.HasHitTargetInSkill(targetDamageable))
                    {
                        if (debugLog)
                        {
                            Debug.Log($"[WeaponHitbox] Skipped (already hit in skill): {other.gameObject.name}");
                        }
                        return;
                    }

                    controller.RegisterHitInSkill(targetDamageable);
                }
            }

            bool applied = DamageHelper.ApplyDamage(other.gameObject, baseDamage, owner, this, damageType);

            if (applied && debugLog)
            {
                Debug.Log($"[WeaponHitbox] Hit: {other.gameObject.name}, Damage: {baseDamage}, Type: {damageType}");
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (hitboxCollider == null)
                hitboxCollider = GetComponent<Collider>();

            if (hitboxCollider == null) return;

            Gizmos.color = isActive ? gizmoColor : new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;

            if (hitboxCollider is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
            else if (hitboxCollider is CapsuleCollider capsule)
            {
                DrawWireCapsule(capsule.center, capsule.radius, capsule.height, capsule.direction);
            }
            else if (hitboxCollider is BoxCollider box)
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
