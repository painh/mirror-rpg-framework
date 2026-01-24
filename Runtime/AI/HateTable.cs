using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Combat;

namespace MirrorRPG.AI
{
    /// <summary>
    /// 헤이트(어그로) 테이블
    /// 몬스터가 각 엔티티에 대한 적대값을 관리
    /// </summary>
    [System.Serializable]
    public class HateTable
    {
        [System.Serializable]
        public class HateEntry
        {
            public GameObject target;
            public float hateValue;
            public float lastUpdateTime;

            public HateEntry(GameObject target, float hate)
            {
                this.target = target;
                this.hateValue = hate;
                this.lastUpdateTime = Time.time;
            }
        }

        // 헤이트 목록
        private List<HateEntry> entries = new List<HateEntry>();

        // 설정
        [Tooltip("헤이트 감소율 (초당)")]
        public float decayRate = 1f;

        [Tooltip("헤이트가 이 값 이하로 떨어지면 목록에서 제거")]
        public float removeThreshold = 0.1f;

        [Tooltip("헤이트 감소 활성화")]
        public bool enableDecay = true;

        /// <summary>
        /// 헤이트 추가
        /// </summary>
        public void AddHate(GameObject target, float amount)
        {
            if (target == null || amount <= 0) return;

            var entry = entries.Find(e => e.target == target);
            if (entry != null)
            {
                entry.hateValue += amount;
                entry.lastUpdateTime = Time.time;
            }
            else
            {
                entries.Add(new HateEntry(target, amount));
            }
        }

        /// <summary>
        /// 헤이트 설정 (덮어쓰기)
        /// </summary>
        public void SetHate(GameObject target, float amount)
        {
            if (target == null) return;

            var entry = entries.Find(e => e.target == target);
            if (entry != null)
            {
                entry.hateValue = amount;
                entry.lastUpdateTime = Time.time;
            }
            else if (amount > 0)
            {
                entries.Add(new HateEntry(target, amount));
            }
        }

        /// <summary>
        /// 특정 대상의 헤이트 조회
        /// </summary>
        public float GetHate(GameObject target)
        {
            if (target == null) return 0f;
            var entry = entries.Find(e => e.target == target);
            return entry?.hateValue ?? 0f;
        }

        /// <summary>
        /// 가장 높은 헤이트를 가진 타겟 반환
        /// </summary>
        public GameObject GetHighestHateTarget()
        {
            CleanupInvalidEntries();

            if (entries.Count == 0) return null;

            HateEntry highest = null;
            foreach (var entry in entries)
            {
                if (entry.target == null) continue;

                // IDamageable 체크 (살아있는지)
                var damageable = entry.target.GetComponent<IDamageable>();
                if (damageable != null && !damageable.IsAlive) continue;

                if (highest == null || entry.hateValue > highest.hateValue)
                {
                    highest = entry;
                }
            }

            return highest?.target;
        }

        /// <summary>
        /// 범위 내에서 가장 높은 헤이트를 가진 타겟 반환
        /// </summary>
        public GameObject GetHighestHateTargetInRange(Vector3 position, float range)
        {
            CleanupInvalidEntries();

            if (entries.Count == 0) return null;

            HateEntry highest = null;
            float rangeSqr = range * range;

            foreach (var entry in entries)
            {
                if (entry.target == null) continue;

                // 범위 체크
                float distSqr = (entry.target.transform.position - position).sqrMagnitude;
                if (distSqr > rangeSqr) continue;

                // IDamageable 체크 (살아있는지)
                var damageable = entry.target.GetComponent<IDamageable>();
                if (damageable != null && !damageable.IsAlive) continue;

                if (highest == null || entry.hateValue > highest.hateValue)
                {
                    highest = entry;
                }
            }

            return highest?.target;
        }

        /// <summary>
        /// 헤이트 목록이 비어있는지 확인
        /// </summary>
        public bool IsEmpty()
        {
            CleanupInvalidEntries();
            return entries.Count == 0;
        }

        /// <summary>
        /// 헤이트 목록 개수
        /// </summary>
        public int Count => entries.Count;

        /// <summary>
        /// 모든 헤이트 초기화
        /// </summary>
        public void Clear()
        {
            entries.Clear();
        }

        /// <summary>
        /// 특정 대상 헤이트 제거
        /// </summary>
        public void RemoveHate(GameObject target)
        {
            entries.RemoveAll(e => e.target == target);
        }

        /// <summary>
        /// 헤이트 감소 업데이트 (매 프레임 호출)
        /// </summary>
        public void Update()
        {
            if (!enableDecay) return;

            for (int i = entries.Count - 1; i >= 0; i--)
            {
                var entry = entries[i];

                // 시간에 따른 감소
                entry.hateValue -= decayRate * Time.deltaTime;

                // 임계값 이하면 제거
                if (entry.hateValue <= removeThreshold)
                {
                    entries.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 유효하지 않은 엔트리 정리
        /// </summary>
        private void CleanupInvalidEntries()
        {
            entries.RemoveAll(e => e.target == null);
        }

        /// <summary>
        /// 디버그용: 모든 헤이트 목록 반환
        /// </summary>
        public IEnumerable<HateEntry> GetAllEntries()
        {
            CleanupInvalidEntries();
            return entries.OrderByDescending(e => e.hateValue);
        }
    }
}
