using UnityEngine;

namespace MirrorRPG.StateMachine
{
    /// <summary>
    /// Interface for objects that own a state machine.
    /// Implement this on your Entity/Character classes.
    /// </summary>
    public interface IStateMachineOwner
    {
        /// <summary>
        /// The GameObject this owner is attached to
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// Whether this owner is currently alive/active
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// The animation controller for this owner (can be null)
        /// </summary>
        IAnimationController AnimationController { get; }
    }
}
