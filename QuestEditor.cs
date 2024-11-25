using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(QuestData))]
public class QuestDataEditor : Editor
{
    SerializedProperty questsList;

    private void OnEnable()
    {
        questsList = serializedObject.FindProperty("Quests");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Iterate through the quests list
        for (int i = 0; i < questsList.arraySize; i++)
        {
            SerializedProperty quest = questsList.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Quest {i + 1}", EditorStyles.boldLabel);

            // Show Quest properties
            SerializedProperty questName = quest.FindPropertyRelative("Name");
            SerializedProperty questType = quest.FindPropertyRelative("Type");
            SerializedProperty questState = quest.FindPropertyRelative("State");

            EditorGUILayout.PropertyField(questName);
            EditorGUILayout.PropertyField(questType);
            EditorGUILayout.PropertyField(questState);

            // Goals list for the current quest
            SerializedProperty goalsList = quest.FindPropertyRelative("Goals");

            EditorGUILayout.LabelField("Goals:");
            for (int j = 0; j < goalsList.arraySize; j++)
            {
                SerializedProperty goal = goalsList.GetArrayElementAtIndex(j);

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"Goal {j + 1}", EditorStyles.miniBoldLabel);

                // Goal properties
                SerializedProperty goalName = goal.FindPropertyRelative("Name");
                SerializedProperty goalType = goal.FindPropertyRelative("Type");
                SerializedProperty goalCompleted = goal.FindPropertyRelative("Completed");
                SerializedProperty goalFailed = goal.FindPropertyRelative("failed");
                SerializedProperty goalTimed = goal.FindPropertyRelative("Timed");



                EditorGUILayout.PropertyField(goalName);
                EditorGUILayout.PropertyField(goalType);
                EditorGUILayout.PropertyField(goalCompleted);
                EditorGUILayout.PropertyField(goalFailed);
                EditorGUILayout.PropertyField(goalTimed);

                if (goalTimed.boolValue)
                {
                    SerializedProperty timeLimit = goal.FindPropertyRelative("TimeLimit");
                    EditorGUILayout.PropertyField(timeLimit);
                }
                // Show goal-specific fields based on the selected GoalType
                switch ((GoalType)goalType.enumValueIndex)
                {
                    case GoalType.KillGoal:
                        SerializedProperty enemiesToKill = goal.FindPropertyRelative("EnemiesToKill");
                        EditorGUILayout.PropertyField(enemiesToKill);
                        break;

                    case GoalType.ItemRetrievalGoal:
                        SerializedProperty itemToRetrieve = goal.FindPropertyRelative("ItemToRetrieve");
                        SerializedProperty amountToRetrieve = goal.FindPropertyRelative("AmountToRetrieve");
                        EditorGUILayout.PropertyField(amountToRetrieve);
                        EditorGUILayout.PropertyField(itemToRetrieve);
                        break;

                    case GoalType.GatheringGoal:
                        SerializedProperty itemsToGather = goal.FindPropertyRelative("ItemsToGather");
                        SerializedProperty amountToGather = goal.FindPropertyRelative("AmountToGather");
                        EditorGUILayout.PropertyField(itemsToGather);
                        EditorGUILayout.PropertyField(amountToGather);
                        break;

                    case GoalType.EscortGoal:
                        SerializedProperty npcToEscort = goal.FindPropertyRelative("NpcToEscort");
                        SerializedProperty destinationPos = goal.FindPropertyRelative("destinationPos");

                        EditorGUILayout.PropertyField(npcToEscort);
                        EditorGUILayout.PropertyField(destinationPos);
                        break;
                }

                // Remove Goal Button
                if (GUILayout.Button("Remove Goal"))
                {
                    goalsList.DeleteArrayElementAtIndex(j);
                }

                EditorGUILayout.EndVertical();
            }

            // Add Goal Button
            if (GUILayout.Button("Add Goal"))
            {
                goalsList.arraySize++;
            }

            // Remove Quest Button
            if (GUILayout.Button("Remove Quest"))
            {
                questsList.DeleteArrayElementAtIndex(i);
            }

            EditorGUILayout.EndVertical();
        }

        // Add Quest Button
        if (GUILayout.Button("Add Quest"))
        {
            questsList.arraySize++;
        }

        serializedObject.ApplyModifiedProperties();
    }
}