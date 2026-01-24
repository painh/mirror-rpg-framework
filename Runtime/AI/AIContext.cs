using UnityEngine;

namespace MirrorRPG.AI
{
    /// <summary>
    /// 피격 정보
    /// </summary>
    public class HitInfo
    {
        public Vector3 hitPoint;
        public Vector3 direction;
        public float damage;
        public float timestamp;

        public HitInfo(Vector3 hitPoint, Vector3 direction, float damage)
        {
            this.hitPoint = hitPoint;
            this.direction = direction;
            this.damage = damage;
            this.timestamp = Time.time;
        }
    }

    /// <summary>
    /// AI 액션 실행 시 필요한 컨텍스트 정보
    /// IAIAgent, Target, Blackboard 등의 참조를 제공
    /// </summary>
    public class AIContext
    {
        public IAIAgent Agent { get; private set; }
        public Transform Target => Agent?.Target;
        public AIBlackboard Blackboard { get; private set; }

        /// <summary>
        /// 현재 상태 이름
        /// </summary>
        public string CurrentStateName { get; set; }

        /// <summary>
        /// 상태 진입 시간
        /// </summary>
        public float StateEnterTime { get; set; }

        /// <summary>
        /// 현재 상태에서 경과한 시간
        /// </summary>
        public float StateTime => Time.time - StateEnterTime;

        /// <summary>
        /// 마지막 피격 정보 (래그돌 등에 사용)
        /// </summary>
        public HitInfo LastHitInfo { get; set; }

        /// <summary>
        /// 사용자 정의 데이터 (게임별 확장용)
        /// </summary>
        public object CustomData { get; set; }

        public AIContext(IAIAgent agent, AIBlackboard blackboard)
        {
            Agent = agent;
            Blackboard = blackboard;
        }

        /// <summary>
        /// 상태 전이 시 호출
        /// </summary>
        public void OnStateEnter(string stateName)
        {
            CurrentStateName = stateName;
            StateEnterTime = Time.time;
        }

        /// <summary>
        /// 피격 정보 기록
        /// </summary>
        public void RecordHit(Vector3 hitPoint, Vector3 direction, float damage)
        {
            LastHitInfo = new HitInfo(hitPoint, direction, damage);
        }
    }
}
