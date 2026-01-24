using MirrorRPG.EventGraph.Nodes;

namespace MirrorRPG.EventGraph
{
    /// <summary>
    /// 이벤트 그래프 실행 시 필요한 핸들러 인터페이스
    /// 대화 표시, 조건 체크, 게임 이벤트 실행 등을 담당
    /// </summary>
    public interface IEventGraphHandler
    {
        /// <summary>
        /// 대화 표시
        /// </summary>
        void ShowDialogue(DialogueNode node);

        /// <summary>
        /// 선택지 표시
        /// </summary>
        void ShowChoices(ChoiceNode node);

        /// <summary>
        /// 조건 체크
        /// </summary>
        bool CheckCondition(EventCondition condition);

        /// <summary>
        /// 플래그 설정
        /// </summary>
        void SetFlag(string flagId, bool value);

        /// <summary>
        /// 변수 설정 (int)
        /// </summary>
        void SetVariable(string variableId, int value);

        /// <summary>
        /// 변수 설정 (string)
        /// </summary>
        void SetVariable(string variableId, string value);

        /// <summary>
        /// 게임 이벤트 트리거
        /// </summary>
        void TriggerGameEvent(string eventId, object data);
    }
}
