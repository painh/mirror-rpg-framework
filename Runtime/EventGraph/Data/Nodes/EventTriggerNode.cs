using System;
using UnityEngine;

namespace MirrorRPG.EventGraph.Nodes
{
    /// <summary>
    /// 이벤트 발생 노드 - 게임 이벤트를 트리거
    /// </summary>
    [Serializable]
    public class EventTriggerNode : BaseEventNode
    {
        [SerializeField] private GameEventType eventType;
        [SerializeField] private string eventId;          // 이벤트 식별자
        [SerializeField] private string targetId;         // 대상 ID (미션 ID, 아이템 ID 등)
        [SerializeField] private string parameter;        // 추가 파라미터

        public GameEventType EventType => eventType;
        public string EventId => eventId;
        public string TargetId => targetId;
        public string Parameter => parameter;

        public override string TypeName => "이벤트";
        public override Color NodeColor => new Color(0.7f, 0.4f, 0.3f); // 갈색

        public EventTriggerNode() : base()
        {
            nodeName = "Event";
        }

#if UNITY_EDITOR
        public void SetEventType(GameEventType type) => eventType = type;
        public void SetEventId(string id) => eventId = id;
        public void SetTargetId(string id) => targetId = id;
        public void SetParameter(string param) => parameter = param;
#endif
    }

    /// <summary>
    /// 게임 이벤트 타입
    /// </summary>
    public enum GameEventType
    {
        // 미션 관련
        StartMission,
        AcceptMission,          // NPC 컨텍스트에서 미션 수락
        AcceptMissionById,      // 특정 미션 ID로 미션 수락
        CompleteMission,
        FailMission,
        UpdateMissionProgress,

        // 아이템 관련
        GiveItem,
        RemoveItem,

        // 플레이어 관련
        GiveExperience,
        GiveGold,
        Teleport,
        SetSpawnPoint,

        // UI 관련
        ShowToast,
        ShowAchievement,

        // 플래그/변수
        SetFlag,
        SetVariable,

        // 씬 관련
        LoadScene,
        UnloadScene,

        // 사운드
        PlaySound,
        PlayMusic,
        StopMusic,

        // 커스텀
        Custom
    }
}
