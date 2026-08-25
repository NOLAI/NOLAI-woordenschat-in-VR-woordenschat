/*
 * ChatGPT_General.cs - ScriptableObject for general ChatGPT context
 *
 * This script defines general information for AI conversations which can be requested by other scripts:
 * - Default prompt for AI interactions
 * - General game story and world description
 * - Player information for AI context
 * - Used as ScriptableObject for asset-based configuration
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ChatGPT/General Information", order = 1)]
public class ChatGPT_General : ScriptableObject
{
    [TextArea(4, 12)]
    [SerializeField] string defaultPrompt;
    [TextArea(4, 6)]
    [SerializeField] string generalGameStory;
    [TextArea(4, 6)]
    [SerializeField] string generalGameWorld;
    [TextArea(4, 6)]
    [SerializeField] string playerInformation;

    public string DefaultPrompt { get => defaultPrompt; }
    public string GeneralGameStory { get => generalGameStory; }
    public string GeneralGameWorld { get => generalGameWorld; }
    public string PlayerInformation { get => playerInformation; }
}

