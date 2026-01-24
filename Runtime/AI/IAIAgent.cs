using UnityEngine;

namespace MirrorRPG.AI
{
    /// <summary>
    /// AI 에이전트 인터페이스
    /// Monster, NPC 등 AI 제어 대상이 구현
    /// </summary>
    public interface IAIAgent
    {
        /// <summary>
        /// 에이전트의 Transform
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// 현재 타겟
        /// </summary>
        Transform Target { get; }

        /// <summary>
        /// 애니메이터
        /// </summary>
        Animator Animator { get; }

        /// <summary>
        /// 살아있는지 여부
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// 현재 체력 비율 (0~1)
        /// </summary>
        float HealthRatio { get; }

        /// <summary>
        /// 이동 속도
        /// </summary>
        float MoveSpeed { get; }

        /// <summary>
        /// 목적지로 이동
        /// </summary>
        void MoveTo(Vector3 destination);

        /// <summary>
        /// 이동 중지
        /// </summary>
        void StopMoving();

        /// <summary>
        /// 타겟 방향으로 회전
        /// </summary>
        void LookAt(Vector3 position);

        /// <summary>
        /// 타겟 설정
        /// </summary>
        void SetTarget(Transform target);
    }
}
