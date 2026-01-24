using System;
using UnityEngine;

namespace MirrorRPG.EventGraph
{
    /// <summary>
    /// 모든 이벤트 노드의 기본 클래스
    /// </summary>
    [Serializable]
    public abstract class BaseEventNode
    {
        [SerializeField] protected string nodeId;
        [SerializeField] protected string nodeName;
        [SerializeField] protected Vector2 position;

        public string NodeId => nodeId;
        public string NodeName => nodeName;
        public Vector2 Position => position;

        /// <summary>
        /// 노드 타입 이름 (에디터 표시용)
        /// </summary>
        public abstract string TypeName { get; }

        /// <summary>
        /// 노드 색상 (에디터 표시용)
        /// </summary>
        public virtual Color NodeColor => new Color(0.3f, 0.3f, 0.3f);

        protected BaseEventNode()
        {
            GenerateId();
        }

        public void GenerateId()
        {
            nodeId = Guid.NewGuid().ToString();
        }

        public void SetPosition(Vector2 newPosition)
        {
            position = newPosition;
        }

        public void SetName(string name)
        {
            nodeName = name;
        }
    }
}
