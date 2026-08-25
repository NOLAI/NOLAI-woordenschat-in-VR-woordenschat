/*
 * NpcInformation.cs - Defines NPC characteristics for AI context
 *
 * This script provides NPC data for AI conversations:
 * - Stores NPC name, profession, personality
 * - Defines way of speaking and task description
 * - Generates prompt for AI using all NPC attributes
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using ToonPeople;

public class NpcInformation : MonoBehaviour
{
    [SerializeField] private Playanimation ani;
    [Header("What characteristics does the NPC have?")]
    [SerializeField] private string npcName = "";
    [SerializeField] private Profession npcProfession;
    [SerializeField] private Personality npcPersonality;
    [SerializeField] private WayOfSpeaking npcWayOfSpeaking;

    [Header("What does the NPC do?")]
    [TextArea(4, 6)]
    [SerializeField] private string npcTask;

    [Header("How does the NPC talk?")]
    [SerializeField] NPCTextToSpeech ttsModel;

    public NPCTextToSpeech TtsModel { get => ttsModel; }
    public Playanimation Ani { get => ani; }

    public string GetPrompt()
    {
        return $"NPC Information: \n" +
               $"NPC Name: {npcName}\n" +
               $"NPC Profession: {npcProfession}\n" +
               $"NPC Personality: {npcPersonality}\n" +
               $"NPC Way of speaking: {npcWayOfSpeaking}\n" +
               $"NPC Task: {npcTask}\n";
    }

    public void UpdateTask(string task)
    {
        npcTask = task;
    }
}

public enum Profession
{
    Captain_Of_Ship,
    Crewmember_Of_Ship,
    Cashier_Of_Local_Supermarket,
    Miller,
    Peer_Of_Player,
    Boat_Builder,
    Fisherman
}

[Flags]
public enum Personality
{
    Adventurous = 1 << 0,
    Charismatic = 1 << 1,
    Companionly = 1 << 2,
    Creative = 1 << 3,
    Helpful = 1 << 4,
    Leaderly = 1 << 5,
    Optimistic = 1 << 6,
    Independent = 1 << 7,
    Supportive = 1 << 8,
    Resilient = 1 << 9
}

[Flags]
public enum WayOfSpeaking
{
    Blunt = 1 << 0,
    Verbose = 1 << 1,
    Cheerful = 1 << 2,
    Sarcastic = 1 << 3,
    Formal = 1 << 4,
    Monotone = 1 << 5,
    Grumpy = 1 << 6,
    Nervous = 1 << 7,
    Respectful = 1 << 8,
    Friendly = 1 <<9
}
