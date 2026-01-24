using System;
using System.Collections.Generic;
using UnityEngine;

namespace MirrorRPG.AI
{
    /// <summary>
    /// 조건부 액션 - 조건이 충족되면 액션을 실행하거나 상태 전이
    /// </summary>
    [Serializable]
    public class ConditionalAction
    {
        [Tooltip("실행 우선순위 (높을수록 먼저 평가)")]
        public int priority = 0;

        [Tooltip("실행 조건들 (모두 만족해야 함 - AND)")]
        public List<AICondition> conditions = new List<AICondition>();

        [Tooltip("조건 충족 시 실행할 액션들")]
        public List<AIAction> actions = new List<AIAction>();

        [Tooltip("조건 충족 시 전이할 상태 (비어있으면 전이 안함)")]
        public string transitionToState;

        /// <summary>
        /// 모든 조건 평가
        /// </summary>
        public bool EvaluateConditions(AIContext context)
        {
            if (conditions == null || conditions.Count == 0)
                return true;

            foreach (var condition in conditions)
            {
                if (condition == null) continue;
                if (!condition.Evaluate(context))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 상태 전이가 있는지
        /// </summary>
        public bool HasTransition => !string.IsNullOrEmpty(transitionToState);
    }

    /// <summary>
    /// State별 액션 묶음 (ScriptableObject)
    /// OnEnter, OnUpdate, OnExit 시점에 실행할 액션들 정의
    /// </summary>
    [CreateAssetMenu(fileName = "NewActionPack", menuName = "MirrorRPG/AI/Action Pack")]
    public class AIActionPack : ScriptableObject
    {
        [Header("State Info")]
        [Tooltip("이 ActionPack이 적용되는 상태 이름")]
        public string stateName;

        [Header("On Enter")]
        [Tooltip("상태 진입 시 실행할 액션들")]
        public List<AIAction> onEnterActions = new List<AIAction>();

        [Header("On Update")]
        [Tooltip("매 프레임 평가할 조건부 액션들 (우선순위 순)")]
        public List<ConditionalAction> updateActions = new List<ConditionalAction>();

        [Header("On Exit")]
        [Tooltip("상태 종료 시 실행할 액션들")]
        public List<AIAction> onExitActions = new List<AIAction>();

        // 런타임 상태
        private List<AIAction> currentRunningActions = new List<AIAction>();

        /// <summary>
        /// 상태 진입 시 호출
        /// </summary>
        public void OnEnter(AIContext context)
        {
            currentRunningActions.Clear();

            foreach (var action in onEnterActions)
            {
                if (action != null)
                {
                    action.OnStart(context);
                    action.OnUpdate(context);
                }
            }
        }

        /// <summary>
        /// 매 프레임 호출
        /// </summary>
        /// <returns>전이할 상태 이름 (없으면 null)</returns>
        public string OnUpdate(AIContext context)
        {
            // 우선순위 순으로 정렬된 조건부 액션 평가
            var sortedActions = new List<ConditionalAction>(updateActions);
            sortedActions.Sort((a, b) => b.priority.CompareTo(a.priority));

            foreach (var conditionalAction in sortedActions)
            {
                if (conditionalAction.EvaluateConditions(context))
                {
                    // 액션 실행
                    ExecuteActions(conditionalAction.actions, context);

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
        /// 상태 종료 시 호출
        /// </summary>
        public void OnExit(AIContext context)
        {
            // 실행 중인 액션들 종료
            foreach (var action in currentRunningActions)
            {
                if (action != null)
                {
                    action.OnEnd(context);
                }
            }
            currentRunningActions.Clear();

            // OnExit 액션 실행
            foreach (var action in onExitActions)
            {
                if (action != null)
                {
                    action.OnStart(context);
                    action.OnUpdate(context);
                    action.OnEnd(context);
                }
            }
        }

        private void ExecuteActions(List<AIAction> actions, AIContext context)
        {
            if (actions == null) return;

            foreach (var action in actions)
            {
                if (action == null) continue;

                // 새 액션이면 시작
                if (!currentRunningActions.Contains(action))
                {
                    action.OnStart(context);
                    currentRunningActions.Add(action);
                }

                // 업데이트 실행
                action.OnUpdate(context);

                // 완료된 액션 제거
                if (action.IsComplete(context))
                {
                    action.OnEnd(context);
                    currentRunningActions.Remove(action);
                }
            }
        }
    }
}
