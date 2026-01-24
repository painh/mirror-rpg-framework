using System;
using System.Collections.Generic;
using UnityEngine;

namespace MirrorRPG.EventGraph.Nodes
{
    /// <summary>
    /// 컷씬 노드 - 카메라, 캐릭터 연출 등
    /// </summary>
    [Serializable]
    public class CutsceneNode : BaseEventNode
    {
        [SerializeField] private List<CutsceneAction> actions = new List<CutsceneAction>();
        [SerializeField] private bool blockPlayerInput = true;  // 컷씬 중 입력 차단
        [SerializeField] private bool waitForCompletion = true; // 완료 대기

        public IReadOnlyList<CutsceneAction> Actions => actions;
        public bool BlockPlayerInput => blockPlayerInput;
        public bool WaitForCompletion => waitForCompletion;

        public override string TypeName => "컷씬";
        public override Color NodeColor => new Color(0.6f, 0.3f, 0.5f); // 핑크

        public CutsceneNode() : base()
        {
            nodeName = "Cutscene";
        }

#if UNITY_EDITOR
        public void SetBlockPlayerInput(bool block) => blockPlayerInput = block;
        public void SetWaitForCompletion(bool wait) => waitForCompletion = wait;

        public void AddAction(CutsceneAction action)
        {
            actions.Add(action);
        }

        public void RemoveAction(int index)
        {
            if (index >= 0 && index < actions.Count)
            {
                actions.RemoveAt(index);
            }
        }

        public void ClearActions()
        {
            actions.Clear();
        }

        public List<CutsceneAction> GetActionsMutable() => actions;
#endif
    }

    /// <summary>
    /// 컷씬 액션 데이터
    /// </summary>
    [Serializable]
    public class CutsceneAction
    {
        [SerializeField] private CutsceneActionType actionType;
        [SerializeField] private float duration = 1f;
        [SerializeField] private float delay;
        [SerializeField] private string targetId;
        [SerializeField] private Vector3 targetPosition;
        [SerializeField] private Vector3 targetRotation;
        [SerializeField] private string animationName;
        [SerializeField] private string resourcePath;
        [SerializeField] private AnimationCurve easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public CutsceneActionType ActionType => actionType;
        public float Duration => duration;
        public float Delay => delay;
        public string TargetId => targetId;
        public Vector3 TargetPosition => targetPosition;
        public Vector3 TargetRotation => targetRotation;
        public string AnimationName => animationName;
        public string ResourcePath => resourcePath;
        public AnimationCurve EasingCurve => easingCurve;

#if UNITY_EDITOR
        public void SetActionType(CutsceneActionType type) => actionType = type;
        public void SetDuration(float d) => duration = d;
        public void SetDelay(float d) => delay = d;
        public void SetTargetId(string id) => targetId = id;
        public void SetTargetPosition(Vector3 pos) => targetPosition = pos;
        public void SetTargetRotation(Vector3 rot) => targetRotation = rot;
        public void SetAnimationName(string name) => animationName = name;
        public void SetResourcePath(string path) => resourcePath = path;
        public void SetEasingCurve(AnimationCurve curve) => easingCurve = curve;
#endif
    }

    /// <summary>
    /// 컷씬 액션 타입
    /// </summary>
    public enum CutsceneActionType
    {
        // 카메라
        CameraMove,
        CameraRotate,
        CameraZoom,
        CameraShake,
        CameraFollow,

        // 캐릭터
        CharacterMove,
        CharacterRotate,
        CharacterAnimation,
        CharacterLookAt,
        CharacterSpawn,
        CharacterDespawn,

        // 화면 효과
        FadeIn,
        FadeOut,
        ScreenFlash,

        // 오디오
        PlaySound,
        PlayMusic,
        StopMusic,

        // 대기
        Wait,

        // 오브젝트
        SpawnObject,
        DestroyObject,
        EnableObject,
        DisableObject,

        // 타임라인
        PlayTimeline
    }
}
