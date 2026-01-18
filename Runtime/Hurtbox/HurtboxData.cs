using UnityEngine;
using System.Collections.Generic;

namespace Combat
{
    /// <summary>
    /// 허트박스 설정을 저장하는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New HurtboxData", menuName = "Combat/Hurtbox Data")]
    public class HurtboxData : ScriptableObject
    {
        [Header("General Settings")]
        [Tooltip("이 설정의 이름 (표시용)")]
        public string displayName = "New Hurtbox Config";

        [Tooltip("허트박스에 사용할 레이어")]
        public string hurtboxLayer = "Hurtbox";

        [Tooltip("IK 본 제외 (이름에 'IK'가 포함된 본 무시)")]
        public bool excludeIKBones = true;

        [Header("Hurtbox Parts")]
        [Tooltip("각 부위별 허트박스 설정")]
        public List<HurtboxPartData> parts = new List<HurtboxPartData>();

        /// <summary>
        /// 본 이름에 매칭되는 부위 데이터 찾기
        /// </summary>
        public HurtboxPartData FindMatchingPart(string boneName)
        {
            foreach (var part in parts)
            {
                if (part.MatchesBoneName(boneName))
                {
                    return part;
                }
            }
            return null;
        }

        /// <summary>
        /// 드래곤용 기본 프리셋 생성
        /// </summary>
        public static HurtboxData CreateDragonPreset()
        {
            var data = CreateInstance<HurtboxData>();
            data.displayName = "Dragon";
            data.parts = new List<HurtboxPartData>
            {
                new HurtboxPartData
                {
                    partName = "Head",
                    boneNamePattern = "*DragonNeck5",
                    damageMultiplier = 2.0f,
                    colliderShape = HurtboxColliderShape.Box,
                    colliderSize = new Vector3(0.5f, 0.5f, 0.5f),
                    gizmoColor = Color.red
                },
                new HurtboxPartData
                {
                    partName = "Neck",
                    boneNamePattern = "*DragonNeck*",
                    damageMultiplier = 1.5f,
                    colliderShape = HurtboxColliderShape.Box,
                    colliderSize = new Vector3(0.3f, 0.3f, 0.3f),
                    gizmoColor = new Color(1f, 0.5f, 0f)
                },
                new HurtboxPartData
                {
                    partName = "Spine",
                    boneNamePattern = "*DragonSpine*",
                    damageMultiplier = 1.0f,
                    colliderShape = HurtboxColliderShape.Box,
                    colliderSize = new Vector3(0.8f, 0.5f, 0.5f),
                    gizmoColor = Color.green
                },
                new HurtboxPartData
                {
                    partName = "Tail",
                    boneNamePattern = "*DragonTail*",
                    damageMultiplier = 0.5f,
                    colliderShape = HurtboxColliderShape.Box,
                    colliderSize = new Vector3(0.3f, 0.3f, 0.3f),
                    gizmoColor = Color.yellow
                }
            };
            return data;
        }

        /// <summary>
        /// 휴머노이드용 기본 프리셋 생성
        /// </summary>
        public static HurtboxData CreateHumanoidPreset()
        {
            var data = CreateInstance<HurtboxData>();
            data.displayName = "Humanoid";
            data.parts = new List<HurtboxPartData>
            {
                new HurtboxPartData
                {
                    partName = "Head",
                    boneNamePattern = "Head",
                    damageMultiplier = 2.0f,
                    colliderShape = HurtboxColliderShape.Box,
                    colliderSize = new Vector3(0.3f, 0.3f, 0.3f),
                    gizmoColor = Color.red
                },
                new HurtboxPartData
                {
                    partName = "Torso",
                    boneNamePattern = "*Spine*",
                    damageMultiplier = 1.0f,
                    colliderShape = HurtboxColliderShape.Box,
                    colliderSize = new Vector3(0.4f, 0.5f, 0.3f),
                    gizmoColor = Color.green
                },
                new HurtboxPartData
                {
                    partName = "Arm",
                    boneNamePattern = "*Arm*",
                    damageMultiplier = 0.8f,
                    colliderShape = HurtboxColliderShape.Box,
                    colliderSize = new Vector3(0.2f, 0.4f, 0.2f),
                    gizmoColor = Color.blue
                },
                new HurtboxPartData
                {
                    partName = "Leg",
                    boneNamePattern = "*Leg*",
                    damageMultiplier = 0.7f,
                    colliderShape = HurtboxColliderShape.Box,
                    colliderSize = new Vector3(0.3f, 0.5f, 0.3f),
                    gizmoColor = Color.cyan
                }
            };
            return data;
        }
    }
}
