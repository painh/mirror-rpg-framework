using UnityEngine;
using System.Collections.Generic;
using MirrorRPG.Skill.Actions;

namespace MirrorRPG.Skill
{
    /// <summary>
    /// Component that executes skill actions during skill playback
    /// Attach to entities that can use skills (Player, Monster, etc.)
    /// </summary>
    public class SkillExecutor : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Default spawn point for projectiles/VFX")]
        [SerializeField] private Transform defaultSpawnPoint;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

        // Current skill execution state
        private ISkillData currentSkill;
        private float skillTimer;
        private float previousTimer;
        private bool isExecuting;
        private SkillActionContext context;

        // Track active duration actions
        private List<DurationSkillAction> activeDurationActions = new List<DurationSkillAction>();

        // Events
        public event System.Action<ISkillData> OnSkillStarted;
        public event System.Action<ISkillData> OnSkillEnded;
        public event System.Action<ISkillData> OnSkillCancelled;

        /// <summary>
        /// Is a skill currently being executed?
        /// </summary>
        public bool IsExecuting => isExecuting;

        /// <summary>
        /// Current skill being executed
        /// </summary>
        public ISkillData CurrentSkill => currentSkill;

        /// <summary>
        /// Current skill execution time
        /// </summary>
        public float CurrentTime => skillTimer;

        /// <summary>
        /// Spawn point for projectiles/VFX
        /// </summary>
        public Transform SpawnPoint
        {
            get => defaultSpawnPoint != null ? defaultSpawnPoint : transform;
            set => defaultSpawnPoint = value;
        }

        private void Update()
        {
            if (!isExecuting || currentSkill == null) return;

            previousTimer = skillTimer;
            skillTimer += Time.deltaTime;

            // Process actions
            ProcessActions();

            // Check if skill ended
            if (skillTimer >= currentSkill.Duration)
            {
                EndSkill();
            }
        }

        /// <summary>
        /// Start executing a skill
        /// </summary>
        /// <param name="skill">Skill data to execute</param>
        /// <param name="target">Optional target</param>
        /// <param name="direction">Optional direction</param>
        /// <returns>True if skill started successfully</returns>
        public bool ExecuteSkill(ISkillData skill, GameObject target = null, Vector3? direction = null)
        {
            if (skill == null)
            {
                Debug.LogWarning("[SkillExecutor] Cannot execute null skill");
                return false;
            }

            if (isExecuting)
            {
                if (debugMode) Debug.Log($"[SkillExecutor] Already executing skill: {currentSkill?.SkillName}");
                return false;
            }

            // Setup execution
            currentSkill = skill;
            skillTimer = 0f;
            previousTimer = 0f;
            isExecuting = true;
            activeDurationActions.Clear();

            // Create context
            context = new SkillActionContext
            {
                Owner = gameObject,
                SkillData = skill,
                SpawnPoint = SpawnPoint,
                Target = target,
                Direction = direction ?? transform.forward
            };

            if (debugMode) Debug.Log($"[SkillExecutor] Started skill: {skill.SkillName}");

            OnSkillStarted?.Invoke(skill);

            // Process immediate actions (startTime = 0)
            ProcessActions();

            return true;
        }

        /// <summary>
        /// Cancel the current skill execution
        /// </summary>
        public void CancelSkill()
        {
            if (!isExecuting) return;

            // Cancel all active duration actions
            foreach (var action in activeDurationActions)
            {
                action.OnCancel(context);
            }
            activeDurationActions.Clear();

            // Cancel all actions
            if (currentSkill?.Actions != null)
            {
                foreach (var action in currentSkill.Actions)
                {
                    action?.OnCancel(context);
                }
            }

            var cancelledSkill = currentSkill;
            currentSkill = null;
            isExecuting = false;
            context = null;

            if (debugMode) Debug.Log($"[SkillExecutor] Cancelled skill: {cancelledSkill?.SkillName}");

            OnSkillCancelled?.Invoke(cancelledSkill);
        }

        private void ProcessActions()
        {
            if (currentSkill?.Actions == null) return;

            foreach (var action in currentSkill.Actions)
            {
                if (action == null) continue;

                // Handle duration actions
                if (action is DurationSkillAction durationAction)
                {
                    ProcessDurationAction(durationAction);
                }
                // Handle instant actions
                else if (action.ShouldTrigger(previousTimer, skillTimer))
                {
                    if (debugMode) Debug.Log($"[SkillExecutor] Triggering action: {action.GetType().Name} at {skillTimer:F2}s");
                    action.Execute(context);
                }
            }
        }

        private void ProcessDurationAction(DurationSkillAction action)
        {
            bool wasActive = activeDurationActions.Contains(action);
            bool isActive = action.IsActive(skillTimer);

            // Just started
            if (!wasActive && action.JustStarted(previousTimer, skillTimer))
            {
                if (debugMode) Debug.Log($"[SkillExecutor] Duration action started: {action.GetType().Name}");
                activeDurationActions.Add(action);
                action.OnStart(context);
            }
            // Just ended
            else if (wasActive && action.JustEnded(previousTimer, skillTimer))
            {
                if (debugMode) Debug.Log($"[SkillExecutor] Duration action ended: {action.GetType().Name}");
                activeDurationActions.Remove(action);
                action.OnEnd(context);
            }
        }

        private void EndSkill()
        {
            // End all remaining duration actions
            foreach (var action in activeDurationActions)
            {
                action.OnEnd(context);
            }
            activeDurationActions.Clear();

            var endedSkill = currentSkill;
            currentSkill = null;
            isExecuting = false;
            context = null;

            if (debugMode) Debug.Log($"[SkillExecutor] Ended skill: {endedSkill?.SkillName}");

            OnSkillEnded?.Invoke(endedSkill);
        }

        /// <summary>
        /// Get current context (for external access during skill execution)
        /// </summary>
        public SkillActionContext GetContext() => context;

        /// <summary>
        /// Update context target during execution
        /// </summary>
        public void SetTarget(GameObject target)
        {
            if (context != null)
            {
                context.Target = target;
            }
        }

        /// <summary>
        /// Update context direction during execution
        /// </summary>
        public void SetDirection(Vector3 direction)
        {
            if (context != null)
            {
                context.Direction = direction;
            }
        }
    }
}
