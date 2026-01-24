using System;
using UnityEngine;

namespace MirrorRPG.EventGraph.Nodes
{
    /// <summary>
    /// 이벤트 그래프 시작 노드
    /// </summary>
    [Serializable]
    public class StartNode : BaseEventNode
    {
        public override string TypeName => "시작";
        public override Color NodeColor => new Color(0.2f, 0.6f, 0.2f); // 녹색

        public StartNode() : base()
        {
            nodeName = "Start";
        }
    }
}
