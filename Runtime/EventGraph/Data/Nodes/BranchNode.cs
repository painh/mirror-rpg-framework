using System;
using System.Collections.Generic;
using UnityEngine;

namespace MirrorRPG.EventGraph.Nodes
{
    /// <summary>
    /// 분기 노드 - 조건에 따라 다른 경로로 분기
    /// </summary>
    [Serializable]
    public class BranchNode : BaseEventNode
    {
        [SerializeField] private List<EventCondition> conditions = new List<EventCondition>();
        [SerializeField] private BranchMode branchMode = BranchMode.All; // 모든 조건 만족 or 하나라도 만족

        public IReadOnlyList<EventCondition> Conditions => conditions;
        public BranchMode Mode => branchMode;

        public override string TypeName => "분기";
        public override Color NodeColor => new Color(0.5f, 0.3f, 0.6f); // 보라색

        public BranchNode() : base()
        {
            nodeName = "Branch";
        }

#if UNITY_EDITOR
        public void SetBranchMode(BranchMode mode) => branchMode = mode;

        public void AddCondition(EventCondition condition)
        {
            conditions.Add(condition);
        }

        public void RemoveCondition(int index)
        {
            if (index >= 0 && index < conditions.Count)
            {
                conditions.RemoveAt(index);
            }
        }

        public void ClearConditions()
        {
            conditions.Clear();
        }

        public List<EventCondition> GetConditionsMutable() => conditions;
#endif
    }

    /// <summary>
    /// 분기 모드
    /// </summary>
    public enum BranchMode
    {
        All,    // 모든 조건 만족 (AND)
        Any     // 하나라도 만족 (OR)
    }
}
