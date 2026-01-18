using UnityEngine;
using System.Collections.Generic;

namespace MirrorRPG.StateMachine
{
    /// <summary>
    /// Default IAnimationController implementation using Unity's Animator.
    /// </summary>
    public class AnimatorAdapter : IAnimationController
    {
        private readonly Animator animator;
        private readonly Dictionary<string, string> animationFallbacks;

        /// <summary>
        /// Create an animator adapter
        /// </summary>
        /// <param name="animator">The Unity Animator component</param>
        /// <param name="fallbacks">Optional animation fallback mappings (e.g., "Walk" -> "Locomotion")</param>
        public AnimatorAdapter(Animator animator, Dictionary<string, string> fallbacks = null)
        {
            this.animator = animator;
            this.animationFallbacks = fallbacks ?? new Dictionary<string, string>();
        }

        public bool PlayAnimation(string animationName, float crossFadeDuration = 0.1f)
        {
            if (animator == null) return false;

            int stateHash = Animator.StringToHash(animationName);
            if (animator.HasState(0, stateHash))
            {
                animator.CrossFadeInFixedTime(animationName, crossFadeDuration);
                return true;
            }

            // Try fallback
            if (animationFallbacks.TryGetValue(animationName, out string fallback))
            {
                int fallbackHash = Animator.StringToHash(fallback);
                if (animator.HasState(0, fallbackHash))
                {
                    animator.CrossFadeInFixedTime(fallback, crossFadeDuration);
                    return true;
                }
            }

            return false;
        }

        public bool HasAnimation(string animationName)
        {
            if (animator == null) return false;
            return animator.HasState(0, Animator.StringToHash(animationName));
        }

        public void SetBool(string param, bool value)
        {
            animator?.SetBool(param, value);
        }

        public void SetFloat(string param, float value)
        {
            animator?.SetFloat(param, value);
        }

        public void SetInteger(string param, int value)
        {
            animator?.SetInteger(param, value);
        }

        public void SetTrigger(string param)
        {
            animator?.SetTrigger(param);
        }

        public bool IsAnimationFinished(string animationName, float normalizedTime = 0.95f)
        {
            if (animator == null) return true;

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsName(animationName) && stateInfo.normalizedTime >= normalizedTime;
        }

        public float GetNormalizedTime()
        {
            if (animator == null) return 1f;
            return animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        }

        public float GetAnimationLength(string animationName)
        {
            if (animator == null) return 0f;

            // Search through all clips in the animator's runtime controller
            var controller = animator.runtimeAnimatorController;
            if (controller != null)
            {
                foreach (var clip in controller.animationClips)
                {
                    if (clip.name == animationName)
                    {
                        return clip.length;
                    }
                }
            }

            return 0f;
        }
    }
}
