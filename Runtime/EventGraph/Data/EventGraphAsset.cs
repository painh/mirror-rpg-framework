using System;
using System.Collections.Generic;
using UnityEngine;

namespace MirrorRPG.EventGraph
{
    /// <summary>
    /// 이벤트 그래프 전체 데이터를 저장하는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "NewEventGraph", menuName = "MirrorRPG/Event Graph", order = 100)]
    public class EventGraphAsset : ScriptableObject
    {
        [SerializeField] private string graphId;
        [SerializeField] private string graphName;
        [SerializeField] private string description;

        [SerializeReference] private List<BaseEventNode> nodes = new List<BaseEventNode>();
        [SerializeField] private List<NodeConnection> connections = new List<NodeConnection>();

        [SerializeField] private string startNodeId;

        public string GraphId => graphId;
        public string GraphName => graphName;
        public string Description => description;
        public IReadOnlyList<BaseEventNode> Nodes => nodes;
        public IReadOnlyList<NodeConnection> Connections => connections;
        public string StartNodeId => startNodeId;

        public void Initialize()
        {
            if (string.IsNullOrEmpty(graphId))
            {
                graphId = Guid.NewGuid().ToString();
            }
        }

        public BaseEventNode GetNode(string nodeId)
        {
            return nodes.Find(n => n.NodeId == nodeId);
        }

        public BaseEventNode GetStartNode()
        {
            if (!string.IsNullOrEmpty(startNodeId))
            {
                return GetNode(startNodeId);
            }

            // StartNode 타입 찾기
            return nodes.Find(n => n is Nodes.StartNode);
        }

        public List<BaseEventNode> GetConnectedNodes(string nodeId)
        {
            var result = new List<BaseEventNode>();
            foreach (var conn in connections)
            {
                if (conn.OutputNodeId == nodeId)
                {
                    var node = GetNode(conn.InputNodeId);
                    if (node != null)
                    {
                        result.Add(node);
                    }
                }
            }
            return result;
        }

        public BaseEventNode GetNextNode(string currentNodeId, int outputIndex = 0)
        {
            int count = 0;
            foreach (var conn in connections)
            {
                if (conn.OutputNodeId == currentNodeId)
                {
                    if (count == outputIndex)
                    {
                        return GetNode(conn.InputNodeId);
                    }
                    count++;
                }
            }
            return null;
        }

#if UNITY_EDITOR
        public void SetGraphName(string name) => graphName = name;
        public void SetDescription(string desc) => description = desc;
        public void SetStartNodeId(string nodeId) => startNodeId = nodeId;

        public void AddNode(BaseEventNode node)
        {
            if (node == null) return;
            if (string.IsNullOrEmpty(node.NodeId))
            {
                node.GenerateId();
            }
            nodes.Add(node);
        }

        public void RemoveNode(string nodeId)
        {
            nodes.RemoveAll(n => n.NodeId == nodeId);
            connections.RemoveAll(c => c.OutputNodeId == nodeId || c.InputNodeId == nodeId);
        }

        public void AddConnection(NodeConnection connection)
        {
            if (connection == null) return;

            // 중복 연결 방지
            if (!connections.Exists(c =>
                c.OutputNodeId == connection.OutputNodeId &&
                c.OutputPortName == connection.OutputPortName &&
                c.InputNodeId == connection.InputNodeId &&
                c.InputPortName == connection.InputPortName))
            {
                connections.Add(connection);
            }
        }

        public void RemoveConnection(string outputNodeId, string outputPortName, string inputNodeId, string inputPortName)
        {
            connections.RemoveAll(c =>
                c.OutputNodeId == outputNodeId &&
                c.OutputPortName == outputPortName &&
                c.InputNodeId == inputNodeId &&
                c.InputPortName == inputPortName);
        }

        public void ClearAllConnections(string nodeId)
        {
            connections.RemoveAll(c => c.OutputNodeId == nodeId || c.InputNodeId == nodeId);
        }

        public List<BaseEventNode> GetNodesMutable() => nodes;
        public List<NodeConnection> GetConnectionsMutable() => connections;
#endif
    }
}
