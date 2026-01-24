using System;
using UnityEngine;

namespace MirrorRPG.EventGraph
{
    /// <summary>
    /// 노드 간 연결 정보
    /// </summary>
    [Serializable]
    public class NodeConnection
    {
        [SerializeField] private string outputNodeId;
        [SerializeField] private string outputPortName;
        [SerializeField] private string inputNodeId;
        [SerializeField] private string inputPortName;

        public string OutputNodeId => outputNodeId;
        public string OutputPortName => outputPortName;
        public string InputNodeId => inputNodeId;
        public string InputPortName => inputPortName;

        public NodeConnection() { }

        public NodeConnection(string outputNodeId, string outputPortName, string inputNodeId, string inputPortName)
        {
            this.outputNodeId = outputNodeId;
            this.outputPortName = outputPortName;
            this.inputNodeId = inputNodeId;
            this.inputPortName = inputPortName;
        }
    }
}
