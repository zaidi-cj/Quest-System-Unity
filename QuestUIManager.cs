using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUIManager : MonoBehaviour
{ 
    public Transform mainQuestContent;
    public Transform sideQuestContent;
    public Transform weeklyQuestContent;

    public GameObject questPrefab;
    public GameObject goalPrefab;  
    private bool isGoalsVisible;
    public bool isAnyQuestActive;
    public QuestData questData;
    public Canvas questCanvas;
    public Button questCanvasBtn;
    public TextMeshProUGUI weeklyCountDownTxt;
    private DateTime weeklyQuestEndTime;

    QuestManager questManager;
    public Dictionary<Goals, GoalsUI> goalUIDictionary = new Dictionary<Goals, GoalsUI>();
    public Dictionary<Quest,QuestUI> questUIDictionary = new Dictionary<Quest, QuestUI>();
    private void Start()
    {
        questManager = GetComponent<QuestManager>();
        DisplayQuests();
        StartCoroutine(HandleWeeklyQuest());
        questCanvas.gameObject.SetActive(false);
    }



    private void DisplayQuests()
    {
        foreach (Quest quest in questData.Quests)
        {
            GameObject questInstance = Instantiate(questPrefab);
            QuestUI questUI = questInstance.GetComponent<QuestUI>();

            questUI.questTitle.text = quest.Name;
            questInstance.name = quest.Name;

            questUIDictionary.Add(quest, questUI);
            switch (quest.Type)
            {
                case QuestType.MainQuest:
                    questInstance.transform.SetParent(mainQuestContent, false);
                    break;
                case QuestType.SideQuest:
                    questInstance.transform.SetParent(sideQuestContent, false);
                    break;
                case QuestType.WeeklyQuest:
                    questInstance.transform.SetParent(weeklyQuestContent, false);
                    questInstance.SetActive(false);
                    break;
            }
             
            questUI.goalsScrollView.gameObject.SetActive(false); 
            
            questUI.goalsViewBtn.onClick.AddListener(() => ToggleQuest(quest, questUI.goalsScrollView, questInstance));
           
        }
    }
    private IEnumerator HandleWeeklyQuest()
    {
        List<Quest> weeklyQuests = questData.Quests.Where(q => q.Type == QuestType.WeeklyQuest).ToList();

        int currentQuestIndex = 0;
        if(weeklyQuests.Count == 0)
        {
            weeklyCountDownTxt.text = "No more weekly quests";
            yield break;
        }
        while (true)
        {
            Quest currentWeeklyQuest = weeklyQuests[currentQuestIndex];
            QuestUI questUI = questUIDictionary[currentWeeklyQuest];
            foreach(var q in weeklyQuests)
            {
                QuestUI qUI = questUIDictionary[q];
                qUI.gameObject.SetActive(q == currentWeeklyQuest);
            }

            weeklyQuestEndTime = DateTime.Now.AddDays(7);
            while(DateTime.Now < weeklyQuestEndTime)
            {
                TimeSpan remainingTime = weeklyQuestEndTime - DateTime.Now;
                weeklyCountDownTxt.text = $"Next Quest in : {remainingTime.Days}d {remainingTime.Hours}h {remainingTime.Minutes}m {remainingTime.Seconds}s";
                yield return new WaitForSeconds(1);
            }
            if(currentWeeklyQuest.State != QuestState.IsCompleted)
            {
                currentWeeklyQuest.State = QuestState.CannotStart;
            }
            currentQuestIndex++;
            if(currentQuestIndex >= weeklyQuests.Count)
            {
                questUI.gameObject.SetActive(false);
                weeklyCountDownTxt.text = "No More Weekly Quests";
                yield break;
            }
            yield return new WaitForSeconds(1f);

        }
    }

    private void ToggleQuest(Quest quest, ScrollRect goalsScrollView, GameObject clickedQuestInstance)
    {
        isGoalsVisible = goalsScrollView.gameObject.activeSelf;
        QuestUI clickedQuestUI = clickedQuestInstance.GetComponent<QuestUI>();

        if(quest.State == QuestState.IsCompleted)
        {
            clickedQuestUI.questInfo.text = "Completed";
            clickedQuestUI.startQuestBtn.gameObject.SetActive(false);
            clickedQuestUI.goalsViewBtn.gameObject.SetActive(false);
            goalsScrollView.gameObject.SetActive(false);
            if (quest.Type != QuestType.WeeklyQuest)
            {
                EnableAllQuests(clickedQuestInstance.transform.parent);
            }
            return;
        }
        if(quest.State == QuestState.IsFailed)
        {
            clickedQuestUI.questInfo.text = "Quest Failed";
            clickedQuestUI.startQuestBtn.gameObject.SetActive(false);
            clickedQuestUI.goalsViewBtn.gameObject.SetActive(false);
            goalsScrollView.gameObject.SetActive(false);
            if (quest.Type != QuestType.WeeklyQuest)
            {
                EnableAllQuests(clickedQuestInstance.transform.parent);
            }
            return;
        }
        if (quest.State == QuestState.IsStarted)
        {
            clickedQuestUI.questInfo.text = "Quest Started";
            StartQuestUI(quest, clickedQuestUI);
        }


        if (quest.State == QuestState.CanStart && !isAnyQuestActive)
        {
            clickedQuestUI.startQuestBtn.gameObject.SetActive(true);
            clickedQuestUI.startQuestBtn.onClick.AddListener(() => StartQuestUI(quest, clickedQuestUI));
            UpdateStartButton(clickedQuestUI);
        }
        else
        {
            clickedQuestUI.startQuestBtn.gameObject.SetActive(false);
        }

        if (isGoalsVisible)
        {
            goalsScrollView.gameObject.SetActive(false);
            if (quest.Type != QuestType.WeeklyQuest)
            {
                EnableAllQuests(clickedQuestInstance.transform.parent);
            }
        }
        else 
        {
            goalsScrollView.gameObject.SetActive(true);
            DisableAllOtherQuests(clickedQuestInstance.transform.parent, clickedQuestInstance);
            Transform goalContent = goalsScrollView.content;
            foreach (Transform child in goalContent)
            {
                Destroy(child.gameObject);
            }

            foreach (Goals goal in quest.Goals)
            {
                GameObject goalInstance = Instantiate(goalPrefab, goalContent);
                GoalsUI goalUI = goalInstance.GetComponent<GoalsUI>();
                goalUIDictionary.Add(goal, goalUI);
                if (goal.Timed)
                {
                    goalUI.goalTime.gameObject.SetActive(true);
                    goalUI.goalTime.text = $"Time: {goal.TimeLimit} m";
                }
                else
                {
                    goalUI.goalTime.gameObject.SetActive(false);
                }
                if (goal.Type == GoalType.KillGoal)
                {
                    goalUI.goalProgress.text = "0 / " + goal.EnemiesToKill;
                }
                else if (goal.Type == GoalType.GatheringGoal)
                {
                    goalUI.goalProgress.text = "0 / " + goal.AmountToGather;
                }
                else if (goal.Type == GoalType.ItemRetrievalGoal)
                {
                    goalUI.goalProgress.text = "0 / " + goal.AmountToRetrieve;
                }
                else
                {
                    goalUI.goalProgress.text = "";
                }
                goalUI.goalDescription.text = goal.Name; 
            }
        }
    }

    private void StartQuestUI(Quest quest, QuestUI questUI)
    {
        if (isAnyQuestActive)
        {
            questUI.startQuestBtn.gameObject.SetActive(false);
            return;
        }
        isAnyQuestActive = true;

        questManager.StartQuest(quest);
        questUI.questInfo.text = "Quest Started";

        questUI.startQuestBtn.gameObject.SetActive(false);

        questUI.goalsViewBtn.onClick.AddListener(() => { UpdateQuestProgressUI(quest, questUI); });
    }
    public void UpdateQuestProgressUI(Quest quest, QuestUI questUI)
    {
        if(quest.State == QuestState.IsCompleted)
        {
            CompleteQuestUI(quest, questUI);
            return;
        }
        if(quest.State == QuestState.IsFailed)
        {
            FailedQuestUI(quest, questUI);
            return;
        }
        int completedGoals = quest.Goals.FindAll(g => g.Completed).Count;
        questUI.questInfo.text = $"{ completedGoals} / { quest.Goals.Count}";
    }
    public void CompleteQuestUI(Quest quest, QuestUI questUI)
    {
        questUI.questInfo.text = "Completed";
        isAnyQuestActive = false;
      //  UpdateStartButton();
    }
    public void FailedQuestUI(Quest quest, QuestUI questUI)
    {
        questUI.questInfo.text = "Quest Failed";
        isAnyQuestActive = false;
    }
    public void UpdateGoalsUI(Quest quest, QuestUI questUI)
    {

        Transform goalContent = questUI.goalsScrollView.content;
        for(int i = 0; i < quest.Goals.Count; i++)
        {
            Goals goal = quest.Goals[i];
            Transform goalUIElement = goalContent.GetChild(i);
            GoalsUI goalUI = goalUIElement.GetComponent<GoalsUI>();

            switch (goal.Type)
            {
                case GoalType.KillGoal:
                    goalUI.goalProgress.text = $"{goal.enemiesKilled}/{goal.EnemiesToKill}";
                    break;
                case GoalType.GatheringGoal:
                    goalUI.goalProgress.text = $"{goal.itemsGathered}/{goal.AmountToGather}";
                    break;
                case GoalType.ItemRetrievalGoal:
                    goalUI.goalProgress.text = $"{goal.itemsRetrieved}/{goal.AmountToRetrieve}";
                    break;
                case GoalType.EscortGoal:
                    goalUI.goalProgress.text = $"Distance left: {goal.DistanceToEscort}";
                    if (goal.Completed)
                    {
                        goalUI.goalProgress.text = $"{goal.NpcToEscort} escorted";
                    }
                    break;
            }
        }
    }

    private void UpdateStartButton(QuestUI clickedQuestUI)
    {
  //      isAnyQuestActive = false;
        foreach(var entery in questUIDictionary)
        {
            Quest quest = entery.Key;
            QuestUI questUI = entery.Value;
            if (isAnyQuestActive)
            {
                questUI.startQuestBtn.gameObject.SetActive(false);
            }
            else
            {
                if(quest.State == QuestState.CanStart)
                {
                    clickedQuestUI.startQuestBtn.gameObject.SetActive(true);
                }
                else
                {
                    questUI.startQuestBtn.gameObject.SetActive(false);
                }
            }

        }
    }

    private void DisableAllOtherQuests(Transform questContentParent, GameObject clickedQuestInstance)
    {
        
        foreach (Transform questInstance in questContentParent)
        {
            if (questInstance.gameObject != clickedQuestInstance)
            {
                questInstance.gameObject.SetActive(false);
            }
        }
    }

    private void EnableAllQuests(Transform questContentParent)
    {
        foreach (Transform questInstance in questContentParent)
        {
            questInstance.gameObject.SetActive(true);
        }

    }

    public void OpenQuestCanvas()
    {
        questCanvasBtn.gameObject.SetActive(false);
        questCanvas.gameObject.SetActive(true);
    }
    public void CloseQuestCanvas()
    {
        questCanvasBtn.gameObject.SetActive(true);
        questCanvas.gameObject.SetActive(false);
    }

}
