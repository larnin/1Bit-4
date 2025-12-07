using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class QuestSystem : SerializedMonoBehaviour
{
    class Objective
    {
        public string ID;
        public string name;
        public QuestObjective objective;
        public List<QuestSaveConnection> connections = new List<QuestSaveConnection>();
    }

    class ObjectiveOutput
    {
        public int objectiveIndex;
        public string outputName;
    }

    class InputObjectiveCompletion
    {
        public int objectiveIndex;
        public List<int> validInputIndexs;
    }

    class OngoingQuest
    {
        public OngoingQuest(QuestSaveData data, string name)
        {
            m_name = name;

            //create nodes
            foreach(var node in data.nodes)
            {
                if (node.nodeType != QuestSystemNodeType.Objective)
                    continue;

                var objective = new Objective();
                objective.objective = node.data as QuestObjective;
                if (objective.objective == null)
                    continue;

                objective.name = node.name;
                objective.ID = node.ID;

                m_objectives.Add(objective);
            }

            List<Objective> objectivesToStart = new List<Objective>();

            //make connexions
            foreach (var node in data.nodes)
            {
                if (node.nodeType == QuestSystemNodeType.Complete || node.nodeType == QuestSystemNodeType.Fail)
                    continue;

                if (node.nodeType == QuestSystemNodeType.Start)
                {
                    foreach (var output in node.outNodes)
                    {
                        var objective = GetObjective(output.nextNodeName);
                        if (objective != null)
                            objectivesToStart.Add(objective);
                    }
                }
                else if (node.nodeType == QuestSystemNodeType.Objective)
                {
                    var currentIndex = GetObjectiveIndex(node.name);
                    if (currentIndex < 0)
                        continue;

                    var current = m_objectives[currentIndex];

                    foreach (var output in node.outNodes)
                    {
                        var objective = GetObjective(output.nextNodeName);
                        if (objective != null)
                            current.connections.Add(output);
                        else
                        {
                            var saveNode = GetSaveDataNode(data, output.nextNodeName);
                            if (saveNode != null)
                            {
                                var nodeOutput = new ObjectiveOutput();
                                nodeOutput.objectiveIndex = currentIndex;
                                nodeOutput.outputName = output.currentPortName;

                                if (saveNode.nodeType == QuestSystemNodeType.Complete)
                                    m_completeExits.Add(nodeOutput);
                                else if (saveNode.nodeType == QuestSystemNodeType.Fail)
                                    m_failExits.Add(nodeOutput);
                            }
                        }
                    }
                }
            }

            //startObjectives
            foreach (var o in objectivesToStart)
                StartObjective(o);

        }

        public void Update(float deltaTime)
        {
            List<KeyValuePair<Objective, string>> completedObjectives = new List<KeyValuePair<Objective, string>>();

            foreach(var objectiveIndex in m_currentObjectives)
            {
                var objective = m_objectives[objectiveIndex];
                objective.objective.Update(deltaTime);

                if (objective.objective.IsFail())
                {
                    string nodeName = objective.objective.GetFailNode();
                    if(nodeName.Length > 0)
                    {
                        completedObjectives.Add(new KeyValuePair<Objective, string>(objective, nodeName));
                        continue;
                    }
                }

                if (objective.objective.IsCompleted())
                {
                    completedObjectives.Add(new KeyValuePair<Objective, string>(objective, "Out"));
                    continue;
                }
            }

            foreach(var c in completedObjectives)
            {
                OnQuestObjectiveComplete(c.Key, c.Value);
            }
        }

        void OnQuestObjectiveComplete(Objective objective, string outputName)
        {
            int objectiveIndex = GetObjectiveIndex(objective);
            if (objectiveIndex < 0)
                return;

            m_currentObjectives.Remove(objectiveIndex);
            m_completedObjectives.Add(objective.name);
            objective.objective.End();

            if(IsFailExit(objective, outputName))
            {
                OnFail();
                return;
            }

            if(IsCompleteExit(objective, outputName))
            {
                OnComplete();
                return;
            }

            foreach(var c in objective.connections)
            {
                if (c.currentPortName != outputName)
                    continue;

                var nextNode = GetObjective(c.nextNodeName);
                if (nextNode == null)
                    continue;

                AddInputObjectiveCompletion(objective, nextNode);
            }

            TryStartObjectivesWithInputCompletion();
        }

        void StartObjective(Objective objective)
        {
            int objectiveIndex = GetObjectiveIndex(objective);
            if (objectiveIndex < 0)
                return;

            m_currentObjectives.Add(objectiveIndex);
            objective.objective.Start();
        }

        QuestSaveNode GetSaveDataNode(QuestSaveData data, string nodeName)
        {
            foreach(var n in data.nodes)
            {
                if (n.name == nodeName)
                    return n;
            }
            return null;
        }

        Objective GetObjective(string name)
        {
            foreach(var o in m_objectives)
            {
                if (o.name == name)
                    return o;
            }

            return null;
        }

        int GetObjectiveIndex(Objective objective)
        {
            for(int i = 0; i < m_objectives.Count; i++)
            {
                if (m_objectives[i] == objective)
                    return i;
            }

            return -1;
        }

        int GetObjectiveIndex(string name)
        {
            for (int i = 0; i < m_objectives.Count; i++)
            {
                if (m_objectives[i].name == name)
                    return i;
            }

            return -1;
        }

        bool IsCompleteExit(Objective objective, string portName)
        {
            int objectiveIndex = GetObjectiveIndex(objective);
            if (objectiveIndex < 0)
                return false;

            foreach (var e in m_completeExits)
            {
                if (e.objectiveIndex == objectiveIndex && e.outputName == portName)
                    return true;
            }

            return false;
        }

        bool IsFailExit(Objective objective, string portName)
        {
            int objectiveIndex = GetObjectiveIndex(objective);
            if (objectiveIndex < 0)
                return false;

            foreach (var e in m_failExits)
            {
                if (e.objectiveIndex == objectiveIndex && e.outputName == portName)
                    return true;
            }

            return false;
        }

        void OnComplete()
        {
            Event<QuestEndLevelEvent>.Broadcast(new QuestEndLevelEvent(true));
        }

        void OnFail()
        {
            Event<QuestEndLevelEvent>.Broadcast(new QuestEndLevelEvent(false));
        }

        void AddInputObjectiveCompletion(Objective input, Objective output)
        {
            if (m_completedObjectives.Contains(output.name))
                return;

            int inputIndex = GetObjectiveIndex(input);
            int outputIndex = GetObjectiveIndex(output);
            if (inputIndex < 0 || outputIndex < 0)
                return;

            foreach(var completion in m_completionsToNextNodes)
            {
                if(completion.objectiveIndex == outputIndex)
                {
                    if (!completion.validInputIndexs.Contains(inputIndex))
                        completion.validInputIndexs.Add(inputIndex);
                    return;
                }
            }

            var newCompletion = new InputObjectiveCompletion();
            newCompletion.objectiveIndex = outputIndex;
            newCompletion.validInputIndexs.Add(inputIndex);
            m_completionsToNextNodes.Add(newCompletion);
        }

        void TryStartObjectivesWithInputCompletion()
        {
            List<Objective> objectivesToStart = new List<Objective>();

            foreach(var completion in m_completionsToNextNodes)
            {
                var objective = m_objectives[completion.objectiveIndex];
                if(objective.objective.multipleInputOperator == QuestOperator.OR)
                {
                    objectivesToStart.Add(objective);
                    continue;
                }

                int nbTransitionsToObjective = 0;

                foreach(var o in m_objectives)
                {
                    foreach(var c in o.connections)
                    {
                        if (c.nextNodeName == objective.name)
                            nbTransitionsToObjective++;
                    }
                }

                if(objective.objective.multipleInputOperator == QuestOperator.AND)
                {
                    if (nbTransitionsToObjective == completion.validInputIndexs.Count)
                        objectivesToStart.Add(objective);
                }
            }

            foreach (var o in objectivesToStart)
            {
                int objectiveIndex = GetObjectiveIndex(o);

                m_completionsToNextNodes.RemoveAll(x => { return x.objectiveIndex == objectiveIndex; });
                StartObjective(o);
            }
        }

        public bool IsQuestComplete()
        {
            return m_currentObjectives.Count() == 0;
        }

        public int GetCompletedObjectiveCount()
        {
            return m_completedObjectives.Count();
        }

        public string GetCompletedObjectiveName(int index)
        {
            if (index < 0 || index >= m_completedObjectives.Count)
                return "";
            return m_completedObjectives[index];
        }

        public bool IsObjectiveCompleted(string name)
        {
            return m_completedObjectives.Contains(name);
        }

        public bool IsObjectiveOngoing(string name)
        {
            foreach(var index in m_currentObjectives)
            {
                var objective = m_objectives[index];
                if (objective.name == name)
                    return true;
            }

            return false;
        }

        public string GetName()
        {
            return m_name;
        }

        string m_name;
        List<Objective> m_objectives = new List<Objective>();
        List<ObjectiveOutput> m_completeExits = new List<ObjectiveOutput>();
        List<ObjectiveOutput> m_failExits = new List<ObjectiveOutput>();
        List<int> m_currentObjectives = new List<int>();
        List<InputObjectiveCompletion> m_completionsToNextNodes = new List<InputObjectiveCompletion>();
        List<string> m_completedObjectives = new List<string>();
    }

    class CompletedQuest
    {
        public string name;
        public List<string> objectives; 
    }

    static QuestSystem m_instance = null;
    public static QuestSystem instance { get { return m_instance; } }

    List<CompletedQuest> m_completedQuests = new List<CompletedQuest>();
    List<OngoingQuest> m_ongoingQuest = new List<OngoingQuest>();

    private void Awake()
    {
        m_instance = this;
    }

    private void OnDestroy()
    {
        if (m_instance == this)
            m_instance = null;
    }

    public void StartQuest(QuestSaveData data, string name)
    {
        var newQuest = new OngoingQuest(data, name);
        m_ongoingQuest.Add(newQuest);
    }

    public void StopQuest(string name)
    {
        var quest = m_ongoingQuest.Find(x => { return x.GetName() == name; });
        if (quest == null)
            return;

        m_ongoingQuest.Remove(quest);
    }

    public QuestObjectiveCompletionType GetQuestStatus(string name)
    {
        var quest = m_ongoingQuest.Find(x => { return x.GetName() == name; });
        if (quest != null)
            return QuestObjectiveCompletionType.Ongoing;

        var completed = m_completedQuests.Find(x => { return x.name == name; });
        if (completed != null)
            return QuestObjectiveCompletionType.Completed;

        return QuestObjectiveCompletionType.NotStarted;
    }

    public QuestObjectiveCompletionType GetQuestObjectiveStatus(string name, string objective)
    {
        var quest = m_ongoingQuest.Find(x => { return x.GetName() == name; });
        if(quest != null)
        {
            if (quest.IsObjectiveOngoing(objective))
                return QuestObjectiveCompletionType.Ongoing;
            if (quest.IsObjectiveCompleted(objective))
                return QuestObjectiveCompletionType.Completed;
            return QuestObjectiveCompletionType.NotStarted;
        }

        var completed = m_completedQuests.Find(x => { return x.name == name; });
        if(completed != null)
        {
            if (completed.objectives.Contains(objective))
                return QuestObjectiveCompletionType.Completed;
            return QuestObjectiveCompletionType.NotStarted;
        }

        return QuestObjectiveCompletionType.NotStarted;
    }

    private void Update()
    {
        var deltaTime = Time.deltaTime;

        List<OngoingQuest> completedQuests = new List<OngoingQuest>();

        foreach(var quest in m_ongoingQuest)
        {
            quest.Update(deltaTime);

            if (quest.IsQuestComplete())
                completedQuests.Add(quest);
        }

        foreach(var quest in completedQuests)
        {
            m_ongoingQuest.Remove(quest);

            var completed = new CompletedQuest();
            completed.name = quest.GetName();

            int nbObjective = quest.GetCompletedObjectiveCount();
            for(int i = 0; i < nbObjective; i++)
                completed.objectives.Add(quest.GetCompletedObjectiveName(i));

            m_completedQuests.Add(completed);
        }
    }
}
