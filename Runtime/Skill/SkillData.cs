using UnityEngine;
using System.Collections.Generic;
using Combat;
using MirrorRPG.Combat;

namespace MirrorRPG.Skill
{
    /// <summary>
    /// Unified skill data for both monsters and players
    /// Uses the new SkillAction system for flexible skill design
    /// </summary>
    [CreateAssetMenu(fileName = "NewSkill", menuName = "MirrorRPG/Skill/Skill Data")]
    public class SkillData : ScriptableObject, ISkillData
    {
        [Header("Basic Info")]
        [Tooltip("스킬 이름")]
        public string skillName = "New Skill";

        [Tooltip("스킬 설명")]
        [TextArea(2, 4)]
        public string description;

        [Tooltip("스킬 아이콘")]
        public Sprite icon;

        [Header("Animation")]
        [Tooltip("애니메이션 트리거 이름")]
        public string animationTrigger = "Attack";

        [Tooltip("애니메이션 재생 속도")]
        [Range(0.5f, 2f)]
        public float animationSpeed = 1f;

        // duration 필드 제거됨 - 실제 애니메이션 클립 길이를 런타임에 사용

        [Header("Combat")]
        [Tooltip("전투 효과 (데미지, 버프 등)")]
        public CombatEffect combatEffect;

        [Tooltip("쿨다운 (초)")]
        public float cooldown = 1f;

        [Header("Range")]
        [Tooltip("스킬 사거리")]
        public float range = 2f;

        [Tooltip("효과 범위 각도 (근접 스킬용)")]
        [Range(0f, 360f)]
        public float angle = 60f;

        [Header("Actions")]
        [Tooltip("스킬 실행 중 발생하는 액션들")]
        [SerializeReference]
        public List<SkillAction> actions = new List<SkillAction>();

        [Header("Options")]
        [Tooltip("시전 중 이동 가능")]
        public bool canMoveWhileCasting = false;

        [Tooltip("시전 중 회전 가능")]
        public bool canRotateWhileCasting = true;

        [Tooltip("슈퍼아머 (피격 경직 무시)")]
        public bool hasSuperArmor = false;

        [Header("Resource Cost")]
        [Tooltip("마나/스태미나 소모량")]
        public float resourceCost = 0f;

        [Header("Combo Settings")]
        [Tooltip("다음 콤보 스킬 (null이면 콤보 종료)")]
        public SkillData nextComboSkill;

        [Tooltip("콤보 입력 윈도우 시작 (duration 기준 비율 0~1)")]
        [Range(0f, 1f)]
        public float comboInputWindowStart = 0.5f;

        [Tooltip("콤보 입력 윈도우 종료 (duration 기준 비율 0~1)")]
        [Range(0f, 1f)]
        public float comboInputWindowEnd = 0.8f;

        [Tooltip("리커버리 애니메이션 이름 (콤보 미연계 시 재생, 비어있으면 바로 Idle)")]
        public string recoveryAnimation;

        // recoveryDuration 필드 제거됨 - 실제 애니메이션 클립 길이를 런타임에 사용

        [Header("Legacy - Hit Timings (기존 시스템 호환용)")]
        [Tooltip("기존 히트 타이밍 (Actions 시스템 미사용 시)")]
        public List<SkillHitTiming> hitTimings = new List<SkillHitTiming>();

        #region ISkillData Implementation

        string ISkillData.SkillName => skillName;
        // Duration 제거됨 - 애니메이션 클립 길이를 런타임에 Animator에서 직접 가져옴
        float ISkillData.BaseDamage => combatEffect != null ? combatEffect.GetFinalValue() : 0f;
        DamageType ISkillData.DamageTypes => combatEffect != null ? combatEffect.damageType : DamageType.None;
        IReadOnlyList<SkillHitTiming> ISkillData.HitTimings => hitTimings;
        IReadOnlyList<SkillAction> ISkillData.Actions => actions;
        CombatEffect ISkillData.CombatEffect => combatEffect;
        string ISkillData.AnimationTrigger => animationTrigger;
        float ISkillData.Cooldown => cooldown;

        #endregion

        #region Action Helpers

        /// <summary>
        /// Get all actions that should trigger between previousTime and currentTime
        /// </summary>
        public List<SkillAction> GetTriggeredActions(float previousTime, float currentTime)
        {
            var triggered = new List<SkillAction>();
            foreach (var action in actions)
            {
                if (action != null && action.ShouldTrigger(previousTime, currentTime))
                {
                    triggered.Add(action);
                }
            }
            return triggered;
        }

        /// <summary>
        /// Get all duration-based actions (HitboxAction, etc.)
        /// </summary>
        public List<DurationSkillAction> GetDurationActions()
        {
            var durationActions = new List<DurationSkillAction>();
            foreach (var action in actions)
            {
                if (action is DurationSkillAction durationAction)
                {
                    durationActions.Add(durationAction);
                }
            }
            return durationActions;
        }

        /// <summary>
        /// Check if skill uses the new action system
        /// </summary>
        public bool UsesActionSystem => actions != null && actions.Count > 0;

        #endregion

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(skillName))
            {
                skillName = name;
            }
            if (cooldown < 0f) cooldown = 0f;
        }
    }
}
