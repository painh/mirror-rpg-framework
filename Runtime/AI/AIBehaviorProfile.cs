using System;
using System.Collections.Generic;
using UnityEngine;

namespace MirrorRPG.AI
{
    /// <summary>
    /// 상태별 설정을 직접 포함하는 구조체
    /// </summary>
    [Serializable]
    public class StateConfig
    {
        [Tooltip("상태 이름")]
        public string stateName;

        [Header("On Enter")]
        [Tooltip("상태 진입 시 실행할 액션들")]
        public List<AIAction> onEnterActions = new List<AIAction>();

        [Header("On Update")]
        [Tooltip("매 프레임 평가할 조건부 액션들")]
        public List<ConditionalAction> updateActions = new List<ConditionalAction>();

        [Header("On Exit")]
        [Tooltip("상태 종료 시 실행할 액션들")]
        public List<AIAction> onExitActions = new List<AIAction>();
    }

    /// <summary>
    /// AI 타입별 전체 행동 프로필
    /// 각 상태에서 어떤 행동을 할지 정의
    /// </summary>
    [CreateAssetMenu(fileName = "NewBehaviorProfile", menuName = "MirrorRPG/AI/Behavior Profile")]
    public class AIBehaviorProfile : ScriptableObject
    {
        [Header("Profile Info")]
        [Tooltip("프로필 이름")]
        public string profileName;

        [TextArea(2, 4)]
        [Tooltip("프로필 설명")]
        public string description;

        [Header("Initial State")]
        [Tooltip("시작 상태")]
        public string initialState = "Idle";

        [Header("State Configurations")]
        [Tooltip("각 상태별 설정")]
        public List<StateConfig> stateConfigs = new List<StateConfig>();

        // 런타임 캐시
        private Dictionary<string, StateConfig> stateConfigCache;

        private void OnEnable()
        {
            BuildCache();
        }

        private void OnValidate()
        {
            BuildCache();
        }

        private void BuildCache()
        {
            stateConfigCache = new Dictionary<string, StateConfig>();
            foreach (var config in stateConfigs)
            {
                if (!string.IsNullOrEmpty(config.stateName))
                {
                    stateConfigCache[config.stateName] = config;
                }
            }
        }

        /// <summary>
        /// 상태 설정 가져오기
        /// </summary>
        public StateConfig GetStateConfig(string stateName)
        {
            if (stateConfigCache == null)
                BuildCache();

            stateConfigCache.TryGetValue(stateName, out StateConfig config);
            return config;
        }

        /// <summary>
        /// 상태가 정의되어 있는지 확인
        /// </summary>
        public bool HasState(string stateName)
        {
            if (stateConfigCache == null)
                BuildCache();

            return stateConfigCache.ContainsKey(stateName);
        }

        /// <summary>
        /// 모든 상태 이름 반환
        /// </summary>
        public IEnumerable<string> GetAllStateNames()
        {
            if (stateConfigCache == null)
                BuildCache();

            return stateConfigCache.Keys;
        }

        #region State Execution

        /// <summary>
        /// 상태 진입 처리
        /// </summary>
        public void ExecuteStateEnter(string stateName, AIContext context)
        {
            var config = GetStateConfig(stateName);
            if (config == null) return;

            foreach (var action in config.onEnterActions)
            {
                if (action != null)
                {
                    action.OnStart(context);
                    action.OnUpdate(context);
                }
            }
        }

        /// <summary>
        /// 상태 업데이트 처리
        /// </summary>
        /// <returns>전이할 상태 이름 (없으면 null)</returns>
        public string ExecuteStateUpdate(string stateName, AIContext context)
        {
            var config = GetStateConfig(stateName);
            if (config == null) return null;

            // 우선순위 순으로 정렬
            var sortedActions = new List<ConditionalAction>(config.updateActions);
            sortedActions.Sort((a, b) => b.priority.CompareTo(a.priority));

            foreach (var conditionalAction in sortedActions)
            {
                if (conditionalAction.EvaluateConditions(context))
                {
                    // 액션 실행
                    foreach (var action in conditionalAction.actions)
                    {
                        if (action != null)
                        {
                            action.OnUpdate(context);
                        }
                    }

                    // 상태 전이가 있으면 반환
                    if (conditionalAction.HasTransition)
                    {
                        return conditionalAction.transitionToState;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 상태 종료 처리
        /// </summary>
        public void ExecuteStateExit(string stateName, AIContext context)
        {
            var config = GetStateConfig(stateName);
            if (config == null) return;

            foreach (var action in config.onExitActions)
            {
                if (action != null)
                {
                    action.OnStart(context);
                    action.OnUpdate(context);
                    action.OnEnd(context);
                }
            }
        }

        #endregion

        #region Editor Helpers

        /// <summary>
        /// 기본 상태들로 초기화 (에디터용)
        /// </summary>
        public void InitializeDefaultStates()
        {
            stateConfigs.Clear();

            string[] defaultStates = { "Idle", "Patrol", "Chase", "Attack", "Search", "Hit", "Death" };
            foreach (var state in defaultStates)
            {
                stateConfigs.Add(new StateConfig { stateName = state });
            }

            BuildCache();
        }

        #endregion
    }
}
