using System;
using UnityEngine;
using MirrorRPG.EventGraph.Nodes;

namespace MirrorRPG.EventGraph
{
    /// <summary>
    /// 이벤트 그래프 실행 엔진
    /// </summary>
    public class EventGraphRunner
    {
        private readonly EventGraphAsset graph;
        private readonly IEventGraphHandler handler;

        private BaseEventNode currentNode;
        private bool isRunning;
        private bool isWaitingForInput;

        public event Action<EventGraphAsset> OnCompleted;
        public event Action<BaseEventNode> OnNodeEntered;
        public event Action<BaseEventNode> OnNodeExited;

        public bool IsRunning => isRunning;
        public bool IsWaitingForInput => isWaitingForInput;
        public BaseEventNode CurrentNode => currentNode;

        public EventGraphRunner(EventGraphAsset graph, IEventGraphHandler handler)
        {
            this.graph = graph;
            this.handler = handler;
        }

        /// <summary>
        /// 그래프 실행 시작
        /// </summary>
        public void Start(string startNodeId = null)
        {
            if (graph == null)
            {
                Debug.LogError("[EventGraphRunner] Graph is null");
                return;
            }

            isRunning = true;

            // 시작 노드 찾기
            if (!string.IsNullOrEmpty(startNodeId))
            {
                currentNode = graph.GetNode(startNodeId);
            }
            else
            {
                currentNode = graph.GetStartNode();
            }

            if (currentNode == null)
            {
                Debug.LogError("[EventGraphRunner] Start node not found");
                Complete();
                return;
            }

            ProcessCurrentNode();
        }

        /// <summary>
        /// 실행 중지
        /// </summary>
        public void Stop()
        {
            isRunning = false;
            isWaitingForInput = false;
            currentNode = null;
        }

        /// <summary>
        /// 다음 노드로 진행
        /// </summary>
        public void Advance()
        {
            if (!isRunning || !isWaitingForInput) return;

            isWaitingForInput = false;
            MoveToNextNode();
        }

        /// <summary>
        /// 선택지 선택
        /// </summary>
        public void SelectChoice(int choiceIndex)
        {
            if (!isRunning || !isWaitingForInput) return;
            if (currentNode is not ChoiceNode choiceNode) return;

            isWaitingForInput = false;

            // 선택지에 해당하는 출력으로 이동
            var nextNode = graph.GetNextNode(currentNode.NodeId, choiceIndex);
            if (nextNode != null)
            {
                OnNodeExited?.Invoke(currentNode);
                currentNode = nextNode;
                ProcessCurrentNode();
            }
            else
            {
                Complete();
            }
        }

        private void ProcessCurrentNode()
        {
            if (currentNode == null)
            {
                Complete();
                return;
            }

            OnNodeEntered?.Invoke(currentNode);

            switch (currentNode)
            {
                case StartNode:
                    ProcessStartNode();
                    break;

                case EndNode:
                    ProcessEndNode();
                    break;

                case DialogueNode dialogueNode:
                    ProcessDialogueNode(dialogueNode);
                    break;

                case ChoiceNode choiceNode:
                    ProcessChoiceNode(choiceNode);
                    break;

                case BranchNode branchNode:
                    ProcessBranchNode(branchNode);
                    break;

                case EventTriggerNode eventNode:
                    ProcessEventTriggerNode(eventNode);
                    break;

                case CutsceneNode cutsceneNode:
                    ProcessCutsceneNode(cutsceneNode);
                    break;

                default:
                    Debug.LogWarning($"[EventGraphRunner] Unknown node type: {currentNode.GetType().Name}");
                    MoveToNextNode();
                    break;
            }
        }

        private void ProcessStartNode()
        {
            MoveToNextNode();
        }

        private void ProcessEndNode()
        {
            Complete();
        }

        private void ProcessDialogueNode(DialogueNode node)
        {
            isWaitingForInput = node.WaitForInput;

            if (handler != null)
            {
                handler.ShowDialogue(node);
            }
            else
            {
                Debug.LogWarning($"[EventGraphRunner] Handler not found. Dialogue: {node.SpeakerKey} - {node.DialogueKey}");
            }

            if (!node.WaitForInput)
            {
                MoveToNextNode();
            }
        }

        private void ProcessChoiceNode(ChoiceNode node)
        {
            isWaitingForInput = true;

            if (handler != null)
            {
                handler.ShowChoices(node);
            }
            else
            {
                Debug.LogWarning($"[EventGraphRunner] Handler not found. Choice: {node.PromptKey} ({node.Choices.Count} choices)");
            }
        }

        private void ProcessBranchNode(BranchNode node)
        {
            bool conditionMet = EvaluateBranchConditions(node);
            int outputIndex = conditionMet ? 0 : 1;
            var nextNode = graph.GetNextNode(currentNode.NodeId, outputIndex);

            if (nextNode != null)
            {
                OnNodeExited?.Invoke(currentNode);
                currentNode = nextNode;
                ProcessCurrentNode();
            }
            else
            {
                Complete();
            }
        }

        private bool EvaluateBranchConditions(BranchNode node)
        {
            if (handler == null || node.Conditions.Count == 0) return true;

            bool result = node.Mode == BranchMode.All;

            foreach (var condition in node.Conditions)
            {
                bool conditionResult = handler.CheckCondition(condition);

                if (node.Mode == BranchMode.All)
                {
                    result = result && conditionResult;
                    if (!result) break;
                }
                else
                {
                    result = result || conditionResult;
                    if (result) break;
                }
            }

            return result;
        }

        private void ProcessEventTriggerNode(EventTriggerNode node)
        {
            ExecuteGameEvent(node);
            MoveToNextNode();
        }

        private void ExecuteGameEvent(EventTriggerNode node)
        {
            if (handler == null) return;

            switch (node.EventType)
            {
                case GameEventType.SetFlag:
                    handler.SetFlag(node.TargetId, true);
                    break;

                case GameEventType.SetVariable:
                    if (int.TryParse(node.Parameter, out int intValue))
                    {
                        handler.SetVariable(node.TargetId, intValue);
                    }
                    else
                    {
                        handler.SetVariable(node.TargetId, node.Parameter);
                    }
                    break;

                case GameEventType.ShowToast:
                    handler.TriggerGameEvent("ShowToast", node.Parameter);
                    break;

                case GameEventType.ShowAchievement:
                    handler.TriggerGameEvent("ShowAchievement", node.Parameter);
                    break;

                case GameEventType.AcceptMission:
                    // NPC 컨텍스트의 미션 수락 (TargetId가 비어있으면 현재 NPC의 미션 사용)
                    handler.TriggerGameEvent("AcceptMission", new
                    {
                        MissionId = node.TargetId,  // 비어있으면 NPC 컨텍스트 사용
                        Parameter = node.Parameter
                    });
                    break;

                case GameEventType.AcceptMissionById:
                    // 특정 미션 ID로 미션 수락
                    handler.TriggerGameEvent("AcceptMissionById", new
                    {
                        MissionId = node.TargetId,
                        Parameter = node.Parameter
                    });
                    break;

                default:
                    handler.TriggerGameEvent(node.EventId, new
                    {
                        Type = node.EventType,
                        TargetId = node.TargetId,
                        Parameter = node.Parameter
                    });
                    break;
            }
        }

        private void ProcessCutsceneNode(CutsceneNode node)
        {
            isWaitingForInput = node.WaitForCompletion;

            if (handler != null)
            {
                handler.TriggerGameEvent("PlayCutscene", node);
            }
            else
            {
                Debug.Log($"[EventGraphRunner] Cutscene: {node.Actions.Count} actions");
            }

            if (!node.WaitForCompletion)
            {
                MoveToNextNode();
            }
        }

        private void MoveToNextNode()
        {
            if (!isRunning) return;

            OnNodeExited?.Invoke(currentNode);

            var nextNode = graph.GetNextNode(currentNode.NodeId);
            if (nextNode != null)
            {
                currentNode = nextNode;
                ProcessCurrentNode();
            }
            else
            {
                Complete();
            }
        }

        private void Complete()
        {
            isRunning = false;
            isWaitingForInput = false;

            if (currentNode != null)
            {
                OnNodeExited?.Invoke(currentNode);
            }

            OnCompleted?.Invoke(graph);
        }
    }
}
