using UnityEngine;

namespace MirrorRPG.Entity
{
    /// <summary>
    /// Interface for entities that can move.
    /// Provides common movement properties and methods.
    /// </summary>
    public interface IMovable
    {
        /// <summary>
        /// Walking speed
        /// </summary>
        float WalkSpeed { get; }

        /// <summary>
        /// Running speed
        /// </summary>
        float RunSpeed { get; }

        /// <summary>
        /// Current movement speed
        /// </summary>
        float CurrentSpeed { get; }

        /// <summary>
        /// Current movement direction
        /// </summary>
        Vector3 MoveDirection { get; }

        /// <summary>
        /// Current velocity
        /// </summary>
        Vector3 Velocity { get; }

        /// <summary>
        /// Whether the entity is on the ground
        /// </summary>
        bool IsGrounded { get; }

        /// <summary>
        /// Whether the entity is currently moving
        /// </summary>
        bool IsMoving { get; }

        /// <summary>
        /// Whether the entity is running
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Move in a direction
        /// </summary>
        /// <param name="direction">Movement direction (will be normalized)</param>
        /// <param name="run">Whether to run</param>
        void Move(Vector3 direction, bool run = false);

        /// <summary>
        /// Move towards a target position
        /// </summary>
        /// <param name="targetPosition">Target position to move towards</param>
        /// <param name="run">Whether to run</param>
        void MoveTowards(Vector3 targetPosition, bool run = false);

        /// <summary>
        /// Stop all movement
        /// </summary>
        void StopMoving();

        /// <summary>
        /// Rotate to face a direction
        /// </summary>
        /// <param name="direction">Direction to face</param>
        void RotateTowards(Vector3 direction);

        /// <summary>
        /// Rotate to look at a target
        /// </summary>
        /// <param name="target">Target to look at</param>
        void LookAt(Transform target);

        /// <summary>
        /// Get 3D distance to a position
        /// </summary>
        float GetDistanceTo(Vector3 position);

        /// <summary>
        /// Get horizontal (XZ) distance to a position
        /// </summary>
        float GetHorizontalDistanceTo(Vector3 position);
    }
}
