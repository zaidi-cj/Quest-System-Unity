using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [NonSerialized]
    public Quest activeQuest;
    public QuestUIManager questUIManager;

    private void Start()
    {
        questUIManager = GetComponent<QuestUIManager>();
    }
    private void OnEnable()
    {
        EventHandler.EnemyKilled += KillGoalUpdate;
        EventHandler.ItemGathered += GatheringGoalUpdate;
        EventHandler.ItemRetrieved += ItemRetrievalGoalUpdate;
        EventHandler.NpcEscorted += EscortGoalUpdate;
    }
    private void OnDisable()
    {
        EventHandler.EnemyKilled -= KillGoalUpdate;
        EventHandler.ItemGathered -= GatheringGoalUpdate;
        EventHandler.ItemRetrieved -= ItemRetrievalGoalUpdate;
        EventHandler.NpcEscorted -= EscortGoalUpdate;
    }
    private void KillGoalUpdate()
    {
        if (activeQuest != null)
        {
            foreach (var goal in activeQuest.Goals)
            {
                if (goal.Type == GoalType.KillGoal)
                {
                    goal.enemiesKilled++;
                    if (goal.enemiesKilled >= goal.EnemiesToKill)
                    {
                        goal.Completed = true;
                    }
                }
            }

            QuestUI questUI = FindQuestUI(activeQuest);
            UpdateQuestProgress(activeQuest, questUI);
        }
    }


    private void GatheringGoalUpdate(String itemName)
    {
        if (activeQuest != null)
        {
            foreach (var goal in activeQuest.Goals)
            {
                if (goal.Type == GoalType.GatheringGoal)
                {
                    goal.itemsGathered++;

                    if (goal.itemsGathered >= goal.AmountToGather && goal.ItemsToGather == itemName)
                    {
                        goal.Completed = true;
                    }
                }
            }

            QuestUI questUI = FindQuestUI(activeQuest);
            UpdateQuestProgress(activeQuest, questUI);
        }
    }

    private void ItemRetrievalGoalUpdate(string itemName)
    {
        if (activeQuest != null)
        {
            foreach (var goal in activeQuest.Goals)
            {
                if (goal.Type == GoalType.ItemRetrievalGoal)
                {
                    goal.itemsRetrieved++;

                    if (goal.itemsRetrieved >= goal.AmountToRetrieve && goal.ItemToRetrieve == itemName)
                    {
                        goal.Completed = true;
                    }
                }
            }

            QuestUI questUI = FindQuestUI(activeQuest);
            UpdateQuestProgress(activeQuest, questUI);
        }
    }

    private void EscortGoalUpdate(float remainingDistance)
    {
        if(activeQuest != null)
        {
            foreach(var goal in activeQuest.Goals)
            {
                if(goal.Type == GoalType.EscortGoal)
                {
                    goal.DistanceToEscort = (int)remainingDistance;
                    if(goal.DistanceToEscort <= 1)
                    {
                        goal.Completed = true;
                    }
                }
            }
            QuestUI questUI = FindQuestUI(activeQuest);
            UpdateQuestProgress(activeQuest, questUI);
        }
    }

    private QuestUI FindQuestUI(Quest activeQuest)
    {
        if (questUIManager.questUIDictionary.TryGetValue(activeQuest, out QuestUI questUI))
        {
            return questUI;
        }
        return null;
    }

    public void StartQuest(Quest quest)
    {
        if (activeQuest != null)
        {
            Debug.Log("cannot start quest");
            return;
        }

        quest.State = QuestState.IsStarted;
        activeQuest = quest;
        foreach (var goal in activeQuest.Goals)
        {
            goal.enemiesKilled = 0;
            goal.itemsGathered = 0;
            goal.itemsRetrieved = 0;
            goal.distanceEscorted = 0;
            if (goal.Timed)
            {
               StartCoroutine(StartGoalTimer(goal, activeQuest));
            }
        }
    }
    private IEnumerator StartGoalTimer(Goals goal, Quest quest)
    {

        DateTime goalEndTime = DateTime.Now.AddMinutes(goal.TimeLimit);
        while (DateTime.Now < goalEndTime && !goal.Completed)
        {
            TimeSpan remainingGoalTime = goalEndTime - DateTime.Now;
            GoalsUI goalUI = questUIManager.goalUIDictionary[goal];
            goalUI.goalTime.text = $"Time: {remainingGoalTime.Minutes}m {remainingGoalTime.Seconds}s";
            yield return new WaitForSeconds(1);
            
        }
        if (!goal.Completed)
        {
            goal.failed = true;
            QuestUI questUI = FindQuestUI(activeQuest);
            QuestFailed(quest, questUI);

        }


    }

    public void UpdateQuestProgress(Quest quest, QuestUI questUI)
    {
        if (quest != activeQuest)
        {
            return;
        }
        int completedGoals = 0;
        foreach (var goal in quest.Goals)
        {
            if (goal.Completed)
            {
                completedGoals++;
            }
            if (goal.failed)
            {
                QuestFailed(quest, questUI);
                return;
            }
        }
        if (completedGoals == quest.Goals.Count)
        {
            QuestCompleted(quest, questUI);
        }
        questUIManager.UpdateQuestProgressUI(quest, questUI);
        questUIManager.UpdateGoalsUI(quest, questUI);
    }

    private void QuestCompleted(Quest quest, QuestUI questUI)
    {
        if (quest == activeQuest)
        {
            quest.State = QuestState.IsCompleted;
            activeQuest = null;
        }
        questUIManager.CompleteQuestUI(quest, questUI);
    }
    private void QuestFailed(Quest quest, QuestUI questUI)
    {
        if (quest == activeQuest)
        {
            quest.State = QuestState.IsFailed;
            activeQuest = null;
        }
        questUIManager.FailedQuestUI(quest, questUI);
    }
    public bool HasActiveItemRetrievalGoal(out Goals itemRetrievalGoal)
    {
        if(activeQuest != null)
        {
            foreach (var goal in activeQuest.Goals)
            {
                if(goal.Type == GoalType.ItemRetrievalGoal && !goal.Completed)
                {
                    itemRetrievalGoal = goal;
                    return true;
                }
            }
        }
        itemRetrievalGoal = null;
        return false;
    }
    public bool HasActiveEscortGoal(out Goals escortGoal)
    {
        if (activeQuest != null)
        {
            foreach(var goal in activeQuest.Goals)
            {
                if (goal.Type == GoalType.EscortGoal && !goal.Completed)
                {
                    escortGoal = goal;
                    return true;
                }
            }

        }
        escortGoal = null;
        return false;
    }
}