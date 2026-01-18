using UnityEngine;

namespace Combat
{
    /// <summary>
    /// 개별 허트박스 컴포넌트
    /// 각 본에 붙어서 충돌을 감지하고 HurtboxManager에 전달
    /// "피해를 받는 부위"를 나타냄
    /// </summary>
    public class Hurtbox : MonoBehaviour
    {
        [Header("Part Settings")]
        [Tooltip("부위 이름")]
        public string partName = "";

        [Tooltip("데미지 배율")]
        public float damageMultiplier = 1.0f;

        [Header("References")]
        [Tooltip("이 허트박스를 관리하는 매니저")]
        public HurtboxManager manager;

        [Header("Debug")]
        [Tooltip("Gizmo 색상")]
        public Color gizmoColor = Color.green;

        [Tooltip("Gizmo 항상 표시")]
        public bool alwaysShowGizmo = false;

        private Collider hurtCollider;

        void Awake()
        {
            hurtCollider = GetComponent<Collider>();
            if (hurtCollider != null)
            {
                hurtCollider.isTrigger = true;
            }
        }

        /// <summary>
        /// 허트박스 초기화
        /// </summary>
        public void Initialize(HurtboxPartData partData, HurtboxManager hurtboxManager)
        {
            partName = partData.partName;
            damageMultiplier = partData.damageMultiplier;
            gizmoColor = partData.gizmoColor;
            manager = hurtboxManager;
        }

        /// <summary>
        /// 히트 처리 (Projectile, WeaponHitbox 등에서 호출)
        /// </summary>
        public void OnHit(DamageInfo damageInfo)
        {
            // 부위 정보 추가
            damageInfo.PartMultiplier = damageMultiplier;
            damageInfo.PartName = partName;

            if (manager != null)
            {
                manager.ProcessHit(this, damageInfo);
            }
            else
            {
                Debug.LogWarning($"[Hurtbox] Manager not set for hurtbox: {partName}");
            }
        }

        /// <summary>
        /// 최종 데미지 계산
        /// </summary>
        public float CalculateDamage(float baseDamage)
        {
            return baseDamage * damageMultiplier;
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (alwaysShowGizmo)
            {
                DrawGizmo();
            }
        }

        void OnDrawGizmosSelected()
        {
            if (!alwaysShowGizmo)
            {
                DrawGizmo();
            }
        }

        private void DrawGizmo()
        {
            Gizmos.color = gizmoColor;
            Gizmos.matrix = transform.localToWorldMatrix;

            if (hurtCollider == null)
                hurtCollider = GetComponent<Collider>();

            if (hurtCollider is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
            else if (hurtCollider is CapsuleCollider capsule)
            {
                DrawWireCapsule(capsule.center, capsule.radius, capsule.height, capsule.direction);
            }
            else if (hurtCollider is BoxCollider box)
            {
                Gizmos.DrawWireCube(box.center, box.size);
            }

            if (!string.IsNullOrEmpty(partName))
            {
                UnityEditor.Handles.Label(transform.position, $"{partName} ({damageMultiplier:F1}x)");
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
