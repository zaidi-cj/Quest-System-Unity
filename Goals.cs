using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Goals
{
    public string Name;
    public string Description;
    public GoalType Type;
    public bool Completed;
    public bool failed;
    public bool Timed;
    public int TimeLimit;
    // Kill Goal
    public int EnemiesToKill;
    // Item Retrieval Goal
    public string ItemToRetrieve;
    public int AmountToRetrieve;
    // Escort Goal
    public string NpcToEscort;
    public float DistanceToEscort;
    public Vector3 destinationPos;
    // Gathering Goal
    public string ItemsToGather;
    public int AmountToGather;

    //count varriables
    public int enemiesKilled;
    public int itemsGathered;
    public int itemsRetrieved;
    public float distanceEscorted;


}
public enum GoalType { KillGoal, GatheringGoal, ItemRetrievalGoal, EscortGoal }