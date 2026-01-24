using System;
using UnityEngine;

namespace MirrorRPG.EventGraph.Nodes
{
    /// <summary>
    /// 대화 노드 - NPC 대화 또는 나레이션 표시
    /// </summary>
    [Serializable]
    public class DialogueNode : BaseEventNode
    {
        [SerializeField] private string speakerKey;      // 화자 이름 (Localization 키)
        [SerializeField] private string dialogueKey;     // 대사 텍스트 (Localization 키)
        [SerializeField] private string portraitId;      // 초상화 리소스 ID
        [SerializeField] private float autoAdvanceTime;  // 자동 진행 시간 (0이면 수동)
        [SerializeField] private bool waitForInput = true; // 입력 대기 여부

        public string SpeakerKey => speakerKey;
        public string DialogueKey => dialogueKey;
        public string PortraitId => portraitId;
        public float AutoAdvanceTime => autoAdvanceTime;
        public bool WaitForInput => waitForInput;

        public override string TypeName => "대화";
        public override Color NodeColor => new Color(0.3f, 0.5f, 0.7f); // 파란색

        public DialogueNode() : base()
        {
            nodeName = "Dialogue";
        }

#if UNITY_EDITOR
        public void SetSpeakerKey(string key) => speakerKey = key;
        public void SetDialogueKey(string key) => dialogueKey = key;
        public void SetPortraitId(string id) => portraitId = id;
        public void SetAutoAdvanceTime(float time) => autoAdvanceTime = time;
        public void SetWaitForInput(bool wait) => waitForInput = wait;
#endif
    }
}
