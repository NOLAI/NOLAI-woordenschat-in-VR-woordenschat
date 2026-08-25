/*
 * QuestSO.cs - ScriptableObject for quest definition
 *
 * This script defines quest structure:
 * - Quest key and description
 * - List of objectives to complete
 * - Completion event callback
 * - Used as ScriptableObject for asset-based configuration
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Add new Quest", menuName = "Quests/Quest", order = 1)]
public class QuestSO : ScriptableObject
{
    [SerializeField] string questKey;
    [SerializeField] string description;
    [SerializeField] List<QuestObjective> objectives;
    [SerializeField] UnityEvent onComplete;

    [NonSerialized]
    public bool IsCompleted;

    public string QuestKey { get => questKey; }
    public string Description { get => description; }
    public List<QuestObjective> Objectives { get => objectives; }
    public UnityEvent OnComplete { get => onComplete; }
}

[Serializable]
public class QuestObjective
{
    [SerializeField] string objectiveKey;
    [SerializeField] string description;

    [NonSerialized]
    public bool IsCompleted;

    public string ObjectiveKey { get => objectiveKey; }
    public string Description { get => description; }
}
