using UnityEngine;
using System;

namespace Combat
{
    /// <summary>
    /// 허트박스 콜라이더 형태
    /// </summary>
    public enum HurtboxColliderShape
    {
        Sphere,
        Capsule,
        Box
    }

    /// <summary>
    /// 캡슐 콜라이더 방향
    /// </summary>
    public enum HurtboxCapsuleDirection
    {
        XAxis = 0,
        YAxis = 1,
        ZAxis = 2
    }

    /// <summary>
    /// 허트박스 부위 설정 데이터
    /// </summary>
    [Serializable]
    public class HurtboxPartData
    {
        [Header("Part Info")]
        [Tooltip("부위 이름 (표시용)")]
        public string partName = "";

        [Header("Bone Matching")]
        [Tooltip("본 이름 패턴 (* 와일드카드 지원, 예: Head, Neck*, Tail*)")]
        public string boneNamePattern = "";

        [Header("Damage Settings")]
        [Tooltip("데미지 배율 (1.0 = 100%)")]
        [Range(0.1f, 5.0f)]
        public float damageMultiplier = 1.0f;

        [Header("Collider Settings")]
        public HurtboxColliderShape colliderShape = HurtboxColliderShape.Sphere;

        [Tooltip("콜라이더 크기 (Sphere: X만 사용, Capsule: X=반지름 Y=높이, Box: XYZ 모두 사용)")]
        public Vector3 colliderSize = new Vector3(0.5f, 1f, 0.5f);

        [Tooltip("콜라이더 오프셋")]
        public Vector3 colliderOffset = Vector3.zero;

        [Tooltip("캡슐 방향 (Capsule만 해당)")]
        public HurtboxCapsuleDirection capsuleDirection = HurtboxCapsuleDirection.YAxis;

        [Header("Display")]
        [Tooltip("에디터에서 표시할 색상")]
        public Color gizmoColor = Color.green;

        /// <summary>
        /// 본 이름이 패턴과 일치하는지 확인
        /// </summary>
        public bool MatchesBoneName(string boneName)
        {
            if (string.IsNullOrEmpty(boneNamePattern) || string.IsNullOrEmpty(boneName))
                return false;

            string pattern = boneNamePattern.ToLower();
            string name = boneName.ToLower();

            // 와일드카드가 없는 경우 - 단순 포함 검사
            if (!pattern.Contains("*"))
            {
                return name.Contains(pattern);
            }

            // 와일드카드가 있는 경우 - 분할 후 순서대로 검사
            string[] parts = pattern.Split('*');
            int lastIndex = 0;
            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part)) continue;
                int foundIndex = name.IndexOf(part, lastIndex);
                if (foundIndex < 0) return false;
                lastIndex = foundIndex + part.Length;
            }
            return true;
        }
    }
}
