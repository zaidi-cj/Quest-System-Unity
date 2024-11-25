using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestData", menuName = "Quests")]
public class QuestData : ScriptableObject
{
    public List<Quest> Quests = new List<Quest>();
}

public enum QuestType { MainQuest, SideQuest, WeeklyQuest }
public enum QuestState { CannotStart, CanStart, IsStarted, IsCompleted, IsFailed}
[System.Serializable]
public class Quest
{
    public string Name;
    public QuestType Type;
    public QuestState State;
    public List<Goals> Goals = new List<Goals>(); 

}
