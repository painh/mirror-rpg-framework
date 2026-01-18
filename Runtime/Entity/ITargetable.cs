using UnityEngine;

namespace MirrorRPG.Entity
{
    /// <summary>
    /// Interface for entities that can target and track other entities.
    /// Common for AI-controlled entities.
    /// </summary>
    public interface ITargetable
    {
        /// <summary>
        /// Current target transform
        /// </summary>
        Transform Target { get; }

        /// <summary>
        /// Whether we have a valid target
        /// </summary>
        bool HasTarget { get; }

        /// <summary>
        /// Set a new target
        /// </summary>
        void SetTarget(Transform newTarget);

        /// <summary>
        /// Clear the current target
        /// </summary>
        void ClearTarget();

        /// <summary>
        /// Whether the target is within detection range
        /// </summary>
        bool IsTargetInDetectionRange { get; }

        /// <summary>
        /// Whether the target is within attack range
        /// </summary>
        bool IsTargetInAttackRange { get; }

        /// <summary>
        /// Check if we can see the target (line of sight)
        /// </summary>
        bool CanSeeTarget();
    }

    /// <summary>
    /// Interface for entities with perception capabilities (vision, hearing).
    /// </summary>
    public interface IPerceptor
    {
        /// <summary>
        /// Field of view in degrees
        /// </summary>
        float FieldOfView { get; }

        /// <summary>
        /// Hearing range
        /// </summary>
        float HearingRange { get; }

        /// <summary>
        /// Whether we currently have line of sight to the target
        /// </summary>
        bool IsTargetVisible { get; }

        /// <summary>
        /// Last known position of the target
        /// </summary>
        Vector3 LastKnownTargetPosition { get; }

        /// <summary>
        /// Whether we still remember the target's last position
        /// </summary>
        bool IsRememberingTarget { get; }

        /// <summary>
        /// Forget the target completely
        /// </summary>
        void ForgetTarget();
    }

    /// <summary>
    /// Interface for entities with attack capabilities.
    /// </summary>
    public interface IAttacker
    {
        /// <summary>
        /// Attack range
        /// </summary>
        float AttackRange { get; }

        /// <summary>
        /// Base attack damage
        /// </summary>
        float AttackDamage { get; }

        /// <summary>
        /// Time between attacks
        /// </summary>
        float AttackCooldown { get; }

        /// <summary>
        /// Whether we can currently attack (cooldown check)
        /// </summary>
        bool CanAttack { get; }

        /// <summary>
        /// Perform an attack
        /// </summary>
        void PerformAttack();
    }
}
