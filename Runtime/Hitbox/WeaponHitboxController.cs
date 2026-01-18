using UnityEngine;
using System.Collections.Generic;

namespace Combat
{
    /// <summary>
    /// 엔티티당 여러 WeaponHitbox를 관리하는 컨트롤러
    /// 스킬 단위로 공유 히트 추적을 관리하여 다단히트 방지
    /// </summary>
    public class WeaponHitboxController : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("자동으로 자식에서 WeaponHitbox 찾기")]
        public bool autoFindHitboxes = true;

        [Tooltip("공유 히트 추적 사용 (스킬 내 중복 데미지 방지)")]
        public bool useSharedHitTracking = true;

        [Header("Registered Hitboxes")]
        [SerializeField]
        private List<WeaponHitbox> weaponHitboxes = new List<WeaponHitbox>();

        [Header("Debug")]
        public bool debugLog = false;

        private bool isAttacking;

        // 스킬 단위 공유 히트 추적
        private HashSet<IDamageable> sharedHitTargets = new HashSet<IDamageable>();
        private bool isSkillActive;

        void Awake()
        {
            if (autoFindHitboxes)
            {
                RefreshHitboxList();
            }

            InitializeHitboxes();
        }

        /// <summary>
        /// 히트박스 목록 새로고침
        /// </summary>
        public void RefreshHitboxList()
        {
            weaponHitboxes.Clear();
            weaponHitboxes.AddRange(GetComponentsInChildren<WeaponHitbox>(true));

            if (debugLog)
            {
                Debug.Log($"[WeaponHitboxController] Found {weaponHitboxes.Count} weapon hitboxes");
            }
        }

        private void InitializeHitboxes()
        {
            foreach (var hitbox in weaponHitboxes)
            {
                if (hitbox != null)
                {
                    hitbox.Initialize(gameObject);
                }
            }
        }

        /// <summary>
        /// WeaponHitbox 등록
        /// </summary>
        public void RegisterHitbox(WeaponHitbox hitbox)
        {
            if (hitbox != null && !weaponHitboxes.Contains(hitbox))
            {
                weaponHitboxes.Add(hitbox);
                hitbox.Initialize(gameObject);
            }
        }

        /// <summary>
        /// WeaponHitbox 등록 해제
        /// </summary>
        public void UnregisterHitbox(WeaponHitbox hitbox)
        {
            weaponHitboxes.Remove(hitbox);
        }

        /// <summary>
        /// 모든 히트박스 공격 시작
        /// </summary>
        public void BeginAttack(float damage = 0f)
        {
            if (isAttacking) return;
            isAttacking = true;

            foreach (var hitbox in weaponHitboxes)
            {
                if (hitbox != null)
                {
                    if (damage > 0)
                    {
                        hitbox.SetDamage(damage);
                    }
                    hitbox.BeginAttack();
                }
            }

            if (debugLog)
            {
                Debug.Log($"[WeaponHitboxController] Attack started with {weaponHitboxes.Count} hitboxes, damage: {damage}");
            }
        }

        /// <summary>
        /// 모든 히트박스 공격 종료
        /// </summary>
        public void EndAttack()
        {
            if (!isAttacking) return;
            isAttacking = false;

            foreach (var hitbox in weaponHitboxes)
            {
                if (hitbox != null)
                {
                    hitbox.EndAttack();
                }
            }

            if (debugLog)
            {
                Debug.Log("[WeaponHitboxController] Attack ended");
            }
        }

        /// <summary>
        /// 특정 이름의 히트박스만 공격 시작
        /// </summary>
        public void BeginAttackByName(string hitboxName, float damage = 0f)
        {
            foreach (var hitbox in weaponHitboxes)
            {
                if (hitbox != null && hitbox.gameObject.name == hitboxName)
                {
                    if (damage > 0)
                    {
                        hitbox.SetDamage(damage);
                    }
                    hitbox.BeginAttack();
                }
            }
        }

        /// <summary>
        /// 특정 이름의 히트박스만 공격 종료
        /// </summary>
        public void EndAttackByName(string hitboxName)
        {
            foreach (var hitbox in weaponHitboxes)
            {
                if (hitbox != null && hitbox.gameObject.name == hitboxName)
                {
                    hitbox.EndAttack();
                }
            }
        }

        /// <summary>
        /// 현재 공격 중인지 확인
        /// </summary>
        public bool IsAttacking => isAttacking;

        /// <summary>
        /// 등록된 히트박스 목록 반환
        /// </summary>
        public List<WeaponHitbox> GetHitboxes() => weaponHitboxes;

        #region Shared Hit Tracking (스킬 단위 중복 데미지 방지)

        /// <summary>
        /// 스킬 시작 - 공유 히트 추적 초기화
        /// </summary>
        public void BeginSkill()
        {
            if (isSkillActive) return;
            isSkillActive = true;
            sharedHitTargets.Clear();

            if (debugLog)
            {
                Debug.Log("[WeaponHitboxController] Skill started, shared hit tracking initialized");
            }
        }

        /// <summary>
        /// 스킬 종료 - 공유 히트 추적 정리
        /// </summary>
        public void EndSkill()
        {
            if (!isSkillActive) return;
            isSkillActive = false;
            sharedHitTargets.Clear();

            EndAttack();

            if (debugLog)
            {
                Debug.Log("[WeaponHitboxController] Skill ended, shared hit tracking cleared");
            }
        }

        /// <summary>
        /// 공유 히트 추적: 이미 이 스킬에서 해당 타겟을 타격했는지 확인
        /// </summary>
        public bool HasHitTargetInSkill(IDamageable target)
        {
            if (!useSharedHitTracking) return false;
            return sharedHitTargets.Contains(target);
        }

        /// <summary>
        /// 공유 히트 추적: 타겟 타격 등록
        /// </summary>
        public void RegisterHitInSkill(IDamageable target)
        {
            if (!useSharedHitTracking) return;
            sharedHitTargets.Add(target);

            if (debugLog)
            {
                Debug.Log($"[WeaponHitboxController] Registered hit on {target.GameObject.name} in current skill");
            }
        }

        /// <summary>
        /// 스킬이 활성화 상태인지 확인
        /// </summary>
        public bool IsSkillActive => isSkillActive;

        /// <summary>
        /// 공유 히트 추적 사용 중인지 확인
        /// </summary>
        public bool UseSharedHitTracking => useSharedHitTracking;

        #endregion

        #region Skill-based Attack

        /// <summary>
        /// 스킬 기반 히트박스 활성화 (특정 히트박스만)
        /// </summary>
        public void BeginSkillHit(float damage, string[] hitboxNames = null, DamageType damageType = DamageType.PhysicalHit)
        {
            if (!isSkillActive)
            {
                BeginSkill();
            }

            foreach (var hitbox in weaponHitboxes)
            {
                if (hitbox == null) continue;

                bool shouldActivate = hitboxNames == null || hitboxNames.Length == 0;
                if (!shouldActivate)
                {
                    foreach (var name in hitboxNames)
                    {
                        if (string.Equals(hitbox.gameObject.name, name, System.StringComparison.OrdinalIgnoreCase))
                        {
                            shouldActivate = true;
                            break;
                        }
                    }
                }

                if (shouldActivate)
                {
                    hitbox.SetDamage(damage);
                    hitbox.SetDamageType(damageType);
                    hitbox.SetController(this);
                    hitbox.BeginAttack();
                }
            }

            if (debugLog)
            {
                string names = hitboxNames != null ? string.Join(", ", hitboxNames) : "all";
                Debug.Log($"[WeaponHitboxController] Skill hit started: {names}, damage: {damage}, type: {damageType}");
            }
        }

        /// <summary>
        /// 스킬 기반 히트박스 비활성화 (특정 히트박스만)
        /// </summary>
        public void EndSkillHit(string[] hitboxNames = null)
        {
            foreach (var hitbox in weaponHitboxes)
            {
                if (hitbox == null) continue;

                bool shouldDeactivate = hitboxNames == null || hitboxNames.Length == 0;
                if (!shouldDeactivate)
                {
                    foreach (var name in hitboxNames)
                    {
                        if (string.Equals(hitbox.gameObject.name, name, System.StringComparison.OrdinalIgnoreCase))
                        {
                            shouldDeactivate = true;
                            break;
                        }
                    }
                }

                if (shouldDeactivate)
                {
                    hitbox.EndAttack();
                }
            }

            if (debugLog)
            {
                string names = hitboxNames != null ? string.Join(", ", hitboxNames) : "all";
                Debug.Log($"[WeaponHitboxController] Skill hit ended: {names}");
            }
        }

        #endregion

        void OnDestroy()
        {
            EndSkill();
        }
    }
}
