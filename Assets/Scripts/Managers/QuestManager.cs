/*
 * QuestManager.cs - Singleton manager for quest objectives
 *
 * This script manages the quest system:
 * - Singleton pattern for global access
 * - Tracks quest objectives and completion status
 * - Invokes completion events when all objectives done
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;
    private Dictionary<string, QuestSO> quests = new Dictionary<string, QuestSO>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void CompleteObjective(string questID, string objectiveID)
    {
        if (quests.ContainsKey(questID))
        {
            QuestSO quest = quests[questID];
            QuestObjective objective = quest.Objectives.Find(obj => obj.ObjectiveKey == objectiveID);
            if (objective != null)
            {
                objective.IsCompleted = true;
                CheckCompletion(quest);
            }
        }
    }
    public string CheckCompletion(QuestSO quest)
    {
        quest.IsCompleted = quest.Objectives.TrueForAll(obj => obj.IsCompleted);
        if (quest.IsCompleted)
        {
            quest.OnComplete?.Invoke();
            return "All";
        }
        else
        {
            for (int i = 0; i < quest.Objectives.Count; i++)
            {
                if (quest.Objectives[i].IsCompleted)
                    return quest.Objectives[i].ObjectiveKey;
            }
            return "None";
        }
    }

}
