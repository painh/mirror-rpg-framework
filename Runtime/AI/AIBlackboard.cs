using System.Collections.Generic;
using UnityEngine;

namespace MirrorRPG.AI
{
    /// <summary>
    /// AI 상태 간 데이터 공유를 위한 블랙보드
    /// Key-Value 형태로 다양한 타입의 데이터 저장
    /// </summary>
    public class AIBlackboard
    {
        private Dictionary<string, object> data = new Dictionary<string, object>();
        private Dictionary<string, float> timers = new Dictionary<string, float>();

        #region Generic Data Access

        public void Set<T>(string key, T value)
        {
            data[key] = value;
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            if (data.TryGetValue(key, out object value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }

        public bool Has(string key)
        {
            return data.ContainsKey(key);
        }

        public void Remove(string key)
        {
            data.Remove(key);
        }

        public void Clear()
        {
            data.Clear();
            timers.Clear();
        }

        #endregion

        #region Common Data Shortcuts

        // 타겟 관련
        public Vector3 LastKnownTargetPosition
        {
            get => Get<Vector3>("LastKnownTargetPosition");
            set => Set("LastKnownTargetPosition", value);
        }

        public float LastTargetSeenTime
        {
            get => Get<float>("LastTargetSeenTime");
            set => Set("LastTargetSeenTime", value);
        }

        // 순찰 관련
        public Vector3 PatrolDestination
        {
            get => Get<Vector3>("PatrolDestination");
            set => Set("PatrolDestination", value);
        }

        public bool HasPatrolDestination
        {
            get => Has("PatrolDestination");
        }

        // 공격 관련
        public float LastAttackTime
        {
            get => Get<float>("LastAttackTime");
            set => Set("LastAttackTime", value);
        }

        public int AttackComboCount
        {
            get => Get<int>("AttackComboCount");
            set => Set("AttackComboCount", value);
        }

        // 전투 시간 관련
        public float CombatStartTime
        {
            get => Get<float>("CombatStartTime");
            set => Set("CombatStartTime", value);
        }

        public bool IsInCombat => Has("CombatStartTime");

        public float CombatElapsedTime => IsInCombat ? Time.time - CombatStartTime : 0f;

        // 애니메이션 관련
        public string CurrentAnimation
        {
            get => Get<string>("CurrentAnimation");
            set => Set("CurrentAnimation", value);
        }

        public float AnimationStartTime
        {
            get => Get<float>("AnimationStartTime");
            set => Set("AnimationStartTime", value);
        }

        #endregion

        #region Timer Functions

        /// <summary>
        /// 타이머 시작/리셋
        /// </summary>
        public void StartTimer(string timerName)
        {
            timers[timerName] = Time.time;
        }

        /// <summary>
        /// 타이머 경과 시간 반환
        /// </summary>
        public float GetTimerElapsed(string timerName)
        {
            if (timers.TryGetValue(timerName, out float startTime))
            {
                return Time.time - startTime;
            }
            return 0f;
        }

        /// <summary>
        /// 타이머가 지정된 시간을 경과했는지 체크
        /// </summary>
        public bool HasTimerElapsed(string timerName, float duration)
        {
            return GetTimerElapsed(timerName) >= duration;
        }

        /// <summary>
        /// 타이머 존재 여부
        /// </summary>
        public bool HasTimer(string timerName)
        {
            return timers.ContainsKey(timerName);
        }

        /// <summary>
        /// 타이머 제거
        /// </summary>
        public void RemoveTimer(string timerName)
        {
            timers.Remove(timerName);
        }

        #endregion
    }
}
