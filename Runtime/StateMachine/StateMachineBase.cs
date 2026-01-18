using UnityEngine;
using System;

namespace MirrorRPG.StateMachine
{
    /// <summary>
    /// Generic base state machine that can work with any owner type.
    /// </summary>
    /// <typeparam name="TOwner">The type of the state machine owner</typeparam>
    public abstract class StateMachineBase<TOwner> : MonoBehaviour where TOwner : class, IStateMachineOwner
    {
        /// <summary>
        /// The owner of this state machine
        /// </summary>
        public TOwner Owner { get; protected set; }

        /// <summary>
        /// The current active state
        /// </summary>
        public StateBase<TOwner> CurrentState { get; protected set; }

        /// <summary>
        /// Name of the current state
        /// </summary>
        public string CurrentStateName => CurrentState?.StateName ?? "None";

        /// <summary>
        /// Name of the current animation being played
        /// </summary>
        public string CurrentAnimationName => CurrentState?.CurrentAnimation ?? "None";

        /// <summary>
        /// Event fired when state changes. Parameters: (stateName, animationName)
        /// </summary>
        public event Action<string, string> OnStateChanged;

        protected virtual void Awake()
        {
            Owner = GetOwner();
        }

        protected virtual void Start()
        {
            InitializeStates();
        }

        protected virtual void Update()
        {
            CurrentState?.Update();
        }

        protected virtual void FixedUpdate()
        {
            CurrentState?.FixedUpdate();
        }

        /// <summary>
        /// Get the owner instance. Override to provide custom owner retrieval.
        /// Default implementation uses GetComponent.
        /// </summary>
        protected virtual TOwner GetOwner()
        {
            return GetComponent<TOwner>();
        }

        /// <summary>
        /// Override this to initialize entity-specific states
        /// </summary>
        protected abstract void InitializeStates();

        /// <summary>
        /// Change to a new state
        /// </summary>
        public virtual void ChangeState(StateBase<TOwner> newState)
        {
            if (newState == null) return;
            if (CurrentState == newState) return;

            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState.Enter();

            OnStateChanged?.Invoke(CurrentStateName, CurrentAnimationName);
        }

        /// <summary>
        /// Notify listeners that animation has changed (for UI updates)
        /// </summary>
        public void NotifyAnimationChanged()
        {
            OnStateChanged?.Invoke(CurrentStateName, CurrentAnimationName);
        }
    }
}
