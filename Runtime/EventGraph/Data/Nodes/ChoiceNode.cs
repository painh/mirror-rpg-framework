using System;
using System.Collections.Generic;
using UnityEngine;

namespace MirrorRPG.EventGraph.Nodes
{
    /// <summary>
    /// 선택지 노드 - 플레이어에게 선택지 제공
    /// </summary>
    [Serializable]
    public class ChoiceNode : BaseEventNode
    {
        [SerializeField] private string promptKey;  // 선택지 질문 (Localization 키)
        [SerializeField] private List<DialogueChoice> choices = new List<DialogueChoice>();

        public string PromptKey => promptKey;
        public IReadOnlyList<DialogueChoice> Choices => choices;

        public override string TypeName => "선택지";
        public override Color NodeColor => new Color(0.6f, 0.5f, 0.2f); // 주황색

        public ChoiceNode() : base()
        {
            nodeName = "Choice";
        }

#if UNITY_EDITOR
        public void SetPromptKey(string key) => promptKey = key;

        public void AddChoice(DialogueChoice choice)
        {
            choices.Add(choice);
        }

        public void RemoveChoice(int index)
        {
            if (index >= 0 && index < choices.Count)
            {
                choices.RemoveAt(index);
            }
        }

        public void ClearChoices()
        {
            choices.Clear();
        }

        public List<DialogueChoice> GetChoicesMutable() => choices;
#endif
    }

    /// <summary>
    /// 대화 선택지 데이터
    /// </summary>
    [Serializable]
    public class DialogueChoice
    {
        [SerializeField] private string choiceId;
        [SerializeField] private string choiceTextKey;  // 선택지 텍스트 (Localization 키)
        [SerializeField] private string conditionId;    // 표시 조건 (옵션)
        [SerializeField] private int outputIndex;       // 출력 포트 인덱스

        public string ChoiceId => choiceId;
        public string ChoiceTextKey => choiceTextKey;
        public string ConditionId => conditionId;
        public int OutputIndex => outputIndex;

        public DialogueChoice()
        {
            choiceId = Guid.NewGuid().ToString();
        }

        public DialogueChoice(string textKey, int outIndex = 0)
        {
            choiceId = Guid.NewGuid().ToString();
            choiceTextKey = textKey;
            outputIndex = outIndex;
        }

#if UNITY_EDITOR
        public void SetChoiceTextKey(string key) => choiceTextKey = key;
        public void SetConditionId(string id) => conditionId = id;
        public void SetOutputIndex(int index) => outputIndex = index;
#endif
    }
}
