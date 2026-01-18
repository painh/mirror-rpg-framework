using UnityEngine;
using System;
using System.Collections.Generic;

namespace Combat
{
    /// <summary>
    /// 엔티티에 붙어서 모든 허트박스를 관리하는 매니저
    /// </summary>
    public class HurtboxManager : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("허트박스 설정 데이터")]
        public HurtboxData hurtboxData;

        [Header("Critical Hit")]
        [Tooltip("크리티컬 판정 기준 배율 (이 값 초과 시 크리티컬)")]
        public float criticalThreshold = 1.5f;

        [Header("Debug")]
        [Tooltip("히트 로그 출력")]
        public bool debugLog = true;

        [Tooltip("Gizmo 항상 표시 (선택하지 않아도)")]
        public bool alwaysShowGizmos = false;

        [Header("Runtime Info (Read Only)")]
        [SerializeField]
        private List<Hurtbox> hurtboxes = new List<Hurtbox>();

        private IDamageable damageable;

        /// <summary>
        /// 데미지 처리 전 콜백 (네트워크 등 외부 시스템 연동용)
        /// true 반환 시 기본 처리를 건너뜀
        /// </summary>
        public Func<Hurtbox, DamageInfo, bool> OnBeforeProcessHit;

        /// <summary>
        /// 데미지 처리 후 콜백
        /// </summary>
        public Action<Hurtbox, DamageInfo> OnAfterProcessHit;

        void Awake()
        {
            damageable = GetComponent<IDamageable>();
            if (damageable == null)
            {
                damageable = GetComponentInParent<IDamageable>();
            }

            RefreshHurtboxList();
        }

        /// <summary>
        /// 허트박스 목록 새로고침
        /// </summary>
        public void RefreshHurtboxList()
        {
            hurtboxes.Clear();
            hurtboxes.AddRange(GetComponentsInChildren<Hurtbox>());

            foreach (var hurtbox in hurtboxes)
            {
                hurtbox.manager = this;
            }
        }

        /// <summary>
        /// 히트 처리 (Hurtbox에서 호출)
        /// 배율 계산 후 IDamageable.TakeDamage()에 위임
        /// </summary>
        public void ProcessHit(Hurtbox hurtbox, DamageInfo damageInfo)
        {
            // 크리티컬 판정
            damageInfo.IsCritical = hurtbox.damageMultiplier > criticalThreshold;
            damageInfo.HitPosition = hurtbox.transform.position;

            if (debugLog)
            {
                string critText = damageInfo.IsCritical ? " [CRITICAL!]" : "";
                Debug.Log($"[HurtboxManager] Hit on {hurtbox.partName}: {damageInfo.Damage} x {hurtbox.damageMultiplier:F1} = {damageInfo.FinalDamage:F1} damage{critText}");
            }

            // 커스텀 처리 콜백 (네트워크 등)
            if (OnBeforeProcessHit != null && OnBeforeProcessHit(hurtbox, damageInfo))
            {
                return; // 외부에서 처리됨
            }

            // 기본 처리 - IDamageable.TakeDamage()
            if (damageable != null)
            {
                damageable.TakeDamage(damageInfo);
            }
            else
            {
                Debug.LogWarning($"[HurtboxManager] No IDamageable found to apply damage to!");
            }

            // 후처리 콜백
            OnAfterProcessHit?.Invoke(hurtbox, damageInfo);
        }

        /// <summary>
        /// IDamageable 설정 (수동 설정용)
        /// </summary>
        public void SetDamageable(IDamageable target)
        {
            damageable = target;
        }

        /// <summary>
        /// HurtboxData를 기반으로 허트박스 자동 생성
        /// </summary>
        public void SetupHurtboxes()
        {
            if (hurtboxData == null)
            {
                Debug.LogError("[HurtboxManager] HurtboxData is not assigned!");
                return;
            }

            ClearHurtboxes();

            var allBones = GetComponentsInChildren<Transform>();
            int createdCount = 0;

            foreach (var bone in allBones)
            {
                if (bone.GetComponent<SkinnedMeshRenderer>() != null ||
                    bone.GetComponent<MeshRenderer>() != null)
                {
                    continue;
                }

                if (hurtboxData.excludeIKBones && bone.name.Contains("IK"))
                {
                    continue;
                }

                var partData = hurtboxData.FindMatchingPart(bone.name);
                if (partData == null) continue;

                CreateHurtbox(bone, partData);
                createdCount++;
            }

            RefreshHurtboxList();
            Debug.Log($"[HurtboxManager] Created {createdCount} hurtboxes");
        }

        private void CreateHurtbox(Transform bone, HurtboxPartData partData)
        {
            GameObject hurtboxObj = new GameObject($"Hurtbox_{partData.partName}");
            hurtboxObj.transform.SetParent(bone);

            Vector3 autoOffset = Vector3.zero;
            if (bone.childCount > 0)
            {
                for (int i = 0; i < bone.childCount; i++)
                {
                    Transform child = bone.GetChild(i);
                    if (!child.name.StartsWith("Hurtbox_"))
                    {
                        autoOffset = (child.localPosition) * 0.5f;
                        break;
                    }
                }
            }

            hurtboxObj.transform.localPosition = partData.colliderOffset + autoOffset;
            hurtboxObj.transform.localRotation = Quaternion.identity;
            hurtboxObj.transform.localScale = Vector3.one;

            int layer = LayerMask.NameToLayer(hurtboxData.hurtboxLayer);
            if (layer >= 0)
            {
                hurtboxObj.layer = layer;
            }

            Collider collider = AddCollider(hurtboxObj, partData);
            collider.isTrigger = true;

            Hurtbox hurtbox = hurtboxObj.AddComponent<Hurtbox>();
            hurtbox.Initialize(partData, this);
        }

        private Collider AddCollider(GameObject obj, HurtboxPartData partData)
        {
            switch (partData.colliderShape)
            {
                case HurtboxColliderShape.Sphere:
                    var sphere = obj.AddComponent<SphereCollider>();
                    sphere.radius = partData.colliderSize.x;
                    return sphere;

                case HurtboxColliderShape.Capsule:
                    var capsule = obj.AddComponent<CapsuleCollider>();
                    capsule.radius = partData.colliderSize.x;
                    capsule.height = partData.colliderSize.y;
                    capsule.direction = (int)partData.capsuleDirection;
                    return capsule;

                case HurtboxColliderShape.Box:
                    var box = obj.AddComponent<BoxCollider>();
                    box.size = partData.colliderSize;
                    return box;

                default:
                    var defaultSphere = obj.AddComponent<SphereCollider>();
                    defaultSphere.radius = partData.colliderSize.x;
                    return defaultSphere;
            }
        }

        /// <summary>
        /// 모든 허트박스 제거
        /// </summary>
        public void ClearHurtboxes()
        {
            var existingHurtboxes = GetComponentsInChildren<Hurtbox>();
            foreach (var hurtbox in existingHurtboxes)
            {
                if (Application.isPlaying)
                {
                    Destroy(hurtbox.gameObject);
                }
                else
                {
                    DestroyImmediate(hurtbox.gameObject);
                }
            }

            hurtboxes.Clear();
        }

        /// <summary>
        /// 현재 허트박스 목록 반환
        /// </summary>
        public List<Hurtbox> GetHurtboxes()
        {
            return hurtboxes;
        }

        /// <summary>
        /// 특정 부위의 허트박스 찾기
        /// </summary>
        public Hurtbox FindHurtbox(string partName)
        {
            return hurtboxes.Find(h => h.partName == partName);
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (alwaysShowGizmos)
            {
                DrawAllHurtboxGizmos();
            }
        }

        void OnDrawGizmosSelected()
        {
            if (!alwaysShowGizmos)
            {
                DrawAllHurtboxGizmos();
            }
        }

        private void DrawAllHurtboxGizmos()
        {
            var boxes = hurtboxes.Count > 0 ? hurtboxes : new List<Hurtbox>(GetComponentsInChildren<Hurtbox>());

            foreach (var hurtbox in boxes)
            {
                if (hurtbox == null) continue;

                var collider = hurtbox.GetComponent<Collider>();
                if (collider == null) continue;

                Gizmos.color = hurtbox.gizmoColor;
                Gizmos.matrix = hurtbox.transform.localToWorldMatrix;

                if (collider is SphereCollider sphere)
                {
                    Gizmos.DrawWireSphere(sphere.center, sphere.radius);
                }
                else if (collider is CapsuleCollider capsule)
                {
                    DrawWireCapsule(capsule.center, capsule.radius, capsule.height, capsule.direction);
                }
                else if (collider is BoxCollider box)
                {
                    Gizmos.DrawWireCube(box.center, box.size);
                }

                Gizmos.matrix = Matrix4x4.identity;
                UnityEditor.Handles.Label(hurtbox.transform.position,
                    $"{hurtbox.partName} ({hurtbox.damageMultiplier:F1}x)");
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
