using UnityEngine;

namespace MirrorRPG.StateMachine
{
    /// <summary>
    /// Generic base state class for state machines.
    /// </summary>
    /// <typeparam name="TOwner">The type of the state machine owner</typeparam>
    public abstract class StateBase<TOwner> where TOwner : class, IStateMachineOwner
    {
        protected readonly StateMachineBase<TOwner> stateMachine;
        protected readonly TOwner owner;
        protected readonly IAnimationController animController;

        /// <summary>
        /// Display name of this state
        /// </summary>
        public string StateName { get; protected set; }

        /// <summary>
        /// Name of the currently playing animation
        /// </summary>
        public string CurrentAnimation { get; protected set; }

        public StateBase(StateMachineBase<TOwner> stateMachine)
        {
            this.stateMachine = stateMachine;
            this.owner = stateMachine.Owner;
            this.animController = owner?.AnimationController;
            StateName = GetType().Name.Replace("State", "");
        }

        /// <summary>
        /// Called when entering this state
        /// </summary>
        public virtual void Enter()
        {
        }

        /// <summary>
        /// Called when exiting this state
        /// </summary>
        public virtual void Exit()
        {
        }

        /// <summary>
        /// Called every frame while in this state
        /// </summary>
        public virtual void Update()
        {
        }

        /// <summary>
        /// Called every fixed update while in this state
        /// </summary>
        public virtual void FixedUpdate()
        {
        }

        #region Animation Helpers

        /// <summary>
        /// Play an animation
        /// </summary>
        protected bool PlayAnimation(string animationName, float crossFadeDuration = 0.1f)
        {
            CurrentAnimation = animationName;
            if (animController != null)
            {
                bool success = animController.PlayAnimation(animationName, crossFadeDuration);
                if (!success)
                {
                    CurrentAnimation = $"{animationName} (not found)";
                }
                return success;
            }
            return false;
        }

        /// <summary>
        /// Set a boolean animation parameter
        /// </summary>
        protected void SetAnimatorBool(string param, bool value)
        {
            animController?.SetBool(param, value);
        }

        /// <summary>
        /// Set a float animation parameter
        /// </summary>
        protected void SetAnimatorFloat(string param, float value)
        {
            animController?.SetFloat(param, value);
        }

        /// <summary>
        /// Set an integer animation parameter
        /// </summary>
        protected void SetAnimatorInteger(string param, int value)
        {
            animController?.SetInteger(param, value);
        }

        /// <summary>
        /// Trigger an animation trigger
        /// </summary>
        protected void SetAnimatorTrigger(string param)
        {
            animController?.SetTrigger(param);
        }

        /// <summary>
        /// Check if an animation is finished
        /// </summary>
        protected bool IsAnimationFinished(string animationName, float normalizedTime = 0.95f)
        {
            if (animController == null) return true;
            return animController.IsAnimationFinished(animationName, normalizedTime);
        }

        /// <summary>
        /// Get the current animation's normalized time
        /// </summary>
        protected float GetAnimationNormalizedTime()
        {
            if (animController == null) return 1f;
            return animController.GetNormalizedTime();
        }

        /// <summary>
        /// Get the length of an animation in seconds
        /// </summary>
        protected float GetAnimationLength(string animationName)
        {
            if (animController == null) return 0f;
            return animController.GetAnimationLength(animationName);
        }

        #endregion
    }
}
