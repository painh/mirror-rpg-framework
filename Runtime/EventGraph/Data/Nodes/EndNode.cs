using System;
using UnityEngine;

namespace MirrorRPG.EventGraph.Nodes
{
    /// <summary>
    /// 이벤트 그래프 종료 노드
    /// </summary>
    [Serializable]
    public class EndNode : BaseEventNode
    {
        public override string TypeName => "종료";
        public override Color NodeColor => new Color(0.6f, 0.2f, 0.2f); // 빨간색

        public EndNode() : base()
        {
            nodeName = "End";
        }
    }
}
