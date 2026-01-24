using UnityEngine;

namespace MirrorRPG.AI
{
    /// <summary>
    /// AI 조건의 베이스 클래스 (ScriptableObject)
    /// 상태 전이나 액션 실행 조건을 정의
    /// </summary>
    public abstract class AICondition : ScriptableObject
    {
        [Tooltip("조건 설명 (에디터용)")]
        [TextArea(1, 3)]
        public string description;

        [Tooltip("결과 반전 (NOT)")]
        public bool invert = false;

        /// <summary>
        /// 조건 평가 (invert 적용 후)
        /// </summary>
        public bool Evaluate(AIContext context)
        {
            bool result = EvaluateCondition(context);
            return invert ? !result : result;
        }

        /// <summary>
        /// 실제 조건 평가 로직 (상속받아 구현)
        /// </summary>
        protected abstract bool EvaluateCondition(AIContext context);

        /// <summary>
        /// 에디터에서 조건 이름 표시
        /// </summary>
        public virtual string GetDisplayName()
        {
            string baseName = GetType().Name.Replace("Condition", "");
            return invert ? $"NOT {baseName}" : baseName;
        }
    }
}
