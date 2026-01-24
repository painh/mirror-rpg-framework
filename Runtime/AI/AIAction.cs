using UnityEngine;

namespace MirrorRPG.AI
{
    /// <summary>
    /// AI 액션의 베이스 클래스 (ScriptableObject)
    /// 모든 AI 액션은 이 클래스를 상속받아 구현
    /// </summary>
    public abstract class AIAction : ScriptableObject
    {
        [Tooltip("액션 설명 (에디터용)")]
        [TextArea(1, 3)]
        public string description;

        /// <summary>
        /// 액션 시작 시 호출
        /// </summary>
        public virtual void OnStart(AIContext context) { }

        /// <summary>
        /// 매 프레임 호출
        /// </summary>
        public abstract void OnUpdate(AIContext context);

        /// <summary>
        /// 액션 종료 시 호출
        /// </summary>
        public virtual void OnEnd(AIContext context) { }

        /// <summary>
        /// 액션이 완료되었는지 체크 (기본: 항상 false = 계속 실행)
        /// </summary>
        public virtual bool IsComplete(AIContext context) => false;

        #region Helper Methods

        /// <summary>
        /// 애니메이션 재생 헬퍼 (상태 존재 여부 확인 및 폴백 처리)
        /// </summary>
        protected void PlayAnimation(AIContext context, string animationName, float crossFade = 0.15f)
        {
            if (context.Agent == null) return;

            var animator = context.Agent.Animator;
            if (animator == null) return;

            int stateHash = Animator.StringToHash(animationName);

            // 상태가 존재하면 직접 재생
            if (animator.HasState(0, stateHash))
            {
                animator.CrossFadeInFixedTime(animationName, crossFade);
            }
            // Walk/Idle는 Locomotion 블렌드 트리로 폴백
            else if (animationName == "Walk" || animationName == "Idle")
            {
                // CurrentGait 파라미터로 제어 (0=Idle, 1=Walk, 2=Run)
                if (HasAnimatorParameter(context, "CurrentGait"))
                {
                    animator.SetInteger("CurrentGait", animationName == "Idle" ? 0 : 1);
                }
                // Locomotion 블렌드 트리 폴백
                else if (animator.HasState(0, Animator.StringToHash("Locomotion")))
                {
                    animator.CrossFadeInFixedTime("Locomotion", crossFade);
                    if (HasAnimatorParameter(context, "Locomotion"))
                        animator.SetFloat("Locomotion", animationName == "Idle" ? 0f : 0.5f);
                }
                else if (animator.HasState(0, Animator.StringToHash("Ground Locomotion")))
                {
                    animator.CrossFadeInFixedTime("Ground Locomotion", crossFade);
                }
            }
            // 그 외 상태가 없으면 경고만 출력 (오류 방지)
            else
            {
                Debug.LogWarning($"[AIAction] Animation state '{animationName}' not found on {context.Agent.Transform.name}");
                return;
            }

            context.Blackboard.CurrentAnimation = animationName;
            context.Blackboard.AnimationStartTime = Time.time;
        }

        /// <summary>
        /// 현재 애니메이션 재생 시간 반환
        /// </summary>
        protected float GetAnimationTime(AIContext context)
        {
            return Time.time - context.Blackboard.AnimationStartTime;
        }

        /// <summary>
        /// 애니메이터 파라미터 존재 여부 체크
        /// </summary>
        protected bool HasAnimatorParameter(AIContext context, string paramName)
        {
            if (context.Agent?.Animator == null) return false;

            foreach (var param in context.Agent.Animator.parameters)
            {
                if (param.name == paramName)
                    return true;
            }
            return false;
        }

        #endregion
    }
}
