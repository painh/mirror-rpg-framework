using UnityEngine;

namespace MirrorRPG.StateMachine
{
    /// <summary>
    /// Interface for animation control abstraction.
    /// Allows state machines to work with different animation systems.
    /// </summary>
    public interface IAnimationController
    {
        /// <summary>
        /// Play an animation by name
        /// </summary>
        /// <param name="animationName">Name of the animation state</param>
        /// <param name="crossFadeDuration">Duration of crossfade transition</param>
        /// <returns>True if animation was found and played</returns>
        bool PlayAnimation(string animationName, float crossFadeDuration = 0.1f);

        /// <summary>
        /// Check if an animation state exists
        /// </summary>
        bool HasAnimation(string animationName);

        /// <summary>
        /// Set a boolean parameter
        /// </summary>
        void SetBool(string param, bool value);

        /// <summary>
        /// Set a float parameter
        /// </summary>
        void SetFloat(string param, float value);

        /// <summary>
        /// Set an integer parameter
        /// </summary>
        void SetInteger(string param, int value);

        /// <summary>
        /// Trigger a trigger parameter
        /// </summary>
        void SetTrigger(string param);

        /// <summary>
        /// Check if current animation is finished
        /// </summary>
        /// <param name="animationName">Name of the animation to check</param>
        /// <param name="normalizedTime">Normalized time threshold (0-1)</param>
        bool IsAnimationFinished(string animationName, float normalizedTime = 0.95f);

        /// <summary>
        /// Get the current animation's normalized time
        /// </summary>
        float GetNormalizedTime();

        /// <summary>
        /// Get the length of an animation clip in seconds
        /// </summary>
        float GetAnimationLength(string animationName);
    }
}
