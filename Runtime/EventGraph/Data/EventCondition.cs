using System;
using UnityEngine;

namespace MirrorRPG.EventGraph
{
    /// <summary>
    /// 이벤트 트리거 타입
    /// </summary>
    public enum EventTriggerType
    {
        OnInteract,         // NPC/오브젝트 상호작용
        OnEnterArea,        // 특정 영역 진입
        OnExitArea,         // 특정 영역 이탈
        OnItemPickup,       // 아이템 획득
        OnItemUse,          // 아이템 사용
        OnMissionComplete,  // 미션 완료
        OnMissionAccept,    // 미션 수락
        OnMonsterKill,      // 몬스터 처치
        OnLevelUp,          // 레벨업
        OnSceneLoad,        // 씬 로드
        Manual              // 수동 호출
    }

    /// <summary>
    /// 조건 비교 연산자
    /// </summary>
    public enum ConditionOperator
    {
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        GreaterOrEqual,
        LessOrEqual,
        Contains,
        HasItem,
        HasMission,
        MissionCompleted
    }

    /// <summary>
    /// 이벤트 조건 데이터
    /// </summary>
    [Serializable]
    public class EventCondition
    {
        [SerializeField] private string conditionId;
        [SerializeField] private ConditionType conditionType;
        [SerializeField] private ConditionOperator operatorType;
        [SerializeField] private string targetKey;
        [SerializeField] private string compareValue;

        public string ConditionId => conditionId;
        public ConditionType ConditionTypeValue => conditionType;
        public ConditionOperator OperatorType => operatorType;
        public string TargetKey => targetKey;
        public string CompareValue => compareValue;

        public EventCondition()
        {
            conditionId = Guid.NewGuid().ToString();
        }

        public EventCondition(ConditionType type, ConditionOperator op, string key, string value)
        {
            conditionId = Guid.NewGuid().ToString();
            conditionType = type;
            operatorType = op;
            targetKey = key;
            compareValue = value;
        }
    }

    /// <summary>
    /// 조건 타입
    /// </summary>
    public enum ConditionType
    {
        Variable,       // 게임 변수 체크
        Item,           // 아이템 보유 여부
        Mission,        // 미션 상태
        Level,          // 플레이어 레벨
        Stats,          // 플레이어 스탯
        Flag,           // 플래그 (bool)
        Custom          // 커스텀 조건
    }
}
