using UnityEngine;

namespace MirrorRPG.Skill
{
    /// <summary>
    /// Base class for skill actions that occur at specific times during skill execution
    /// Use [SerializeReference] in SkillData to support polymorphic serialization
    /// </summary>
    [System.Serializable]
    public abstract class SkillAction
    {
        [Tooltip("Action start time (seconds from skill start)")]
        public float startTime = 0f;

        [Tooltip("Action description (for editor display)")]
        public string description;

        /// <summary>
        /// Check if this action should trigger at the given time
        /// </summary>
        public virtual bool ShouldTrigger(float previousTime, float currentTime)
        {
            return previousTime < startTime && currentTime >= startTime;
        }

        /// <summary>
        /// Execute this action
        /// </summary>
        /// <param name="context">Skill execution context</param>
        public abstract void Execute(SkillActionContext context);

        /// <summary>
        /// Called when the skill is interrupted or cancelled
        /// Override to clean up (e.g., turn off hitboxes)
        /// </summary>
        public virtual void OnCancel(SkillActionContext context) { }
    }

    /// <summary>
    /// Base class for actions that have a duration (start and end time)
    /// </summary>
    [System.Serializable]
    public abstract class DurationSkillAction : SkillAction
    {
        [Tooltip("Action end time (seconds from skill start)")]
        public float endTime = 0.5f;

        /// <summary>
        /// Check if this action is currently active
        /// </summary>
        public bool IsActive(float currentTime)
        {
            return currentTime >= startTime && currentTime < endTime;
        }

        /// <summary>
        /// Check if this action just started
        /// </summary>
        public bool JustStarted(float previousTime, float currentTime)
        {
            return previousTime < startTime && currentTime >= startTime;
        }

        /// <summary>
        /// Check if this action just ended
        /// </summary>
        public bool JustEnded(float previousTime, float currentTime)
        {
            return previousTime < endTime && currentTime >= endTime;
        }

        /// <summary>
        /// Called when the action starts
        /// </summary>
        public abstract void OnStart(SkillActionContext context);

        /// <summary>
        /// Called when the action ends
        /// </summary>
        public abstract void OnEnd(SkillActionContext context);

        /// <summary>
        /// Execute handles both start and end
        /// </summary>
        public override void Execute(SkillActionContext context)
        {
            // Duration actions use OnStart/OnEnd instead
        }

        /// <summary>
        /// Update this action (called every frame while skill is active)
        /// </summary>
        public virtual void Update(SkillActionContext context, float previousTime, float currentTime)
        {
            if (JustStarted(previousTime, currentTime))
            {
                OnStart(context);
            }
            if (JustEnded(previousTime, currentTime))
            {
                OnEnd(context);
            }
        }
    }

    /// <summary>
    /// Context passed to skill actions during execution
    /// </summary>
    public class SkillActionContext
    {
        /// <summary>
        /// The entity executing the skill (Monster, Player, etc.)
        /// </summary>
        public GameObject Owner { get; set; }

        /// <summary>
        /// The skill data being executed
        /// </summary>
        public ISkillData SkillData { get; set; }

        /// <summary>
        /// Transform to use as spawn point (can be hand, weapon, etc.)
        /// </summary>
        public Transform SpawnPoint { get; set; }

        /// <summary>
        /// Current target (if any)
        /// </summary>
        public GameObject Target { get; set; }

        /// <summary>
        /// Direction to face/shoot
        /// </summary>
        public Vector3 Direction { get; set; }

        /// <summary>
        /// Custom data dictionary for action-specific data
        /// </summary>
        public System.Collections.Generic.Dictionary<string, object> CustomData { get; }
            = new System.Collections.Generic.Dictionary<string, object>();
    }
}
