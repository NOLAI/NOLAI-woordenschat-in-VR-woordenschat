/*
 * TranscriptUIManager.cs - Controls UI for conversation states
 *
 * This script manages UI visibility for conversation:
 * - Shows listening, thinking, or speaking icons
 * - Toggles icon visibility based on process
 * - Singleton pattern for global access
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranscriptUIManager : MonoBehaviour
{
    private GameObject listeningIconObject;
    private GameObject thinkingIconObject;
    private GameObject speakingIconObject;

    public static TranscriptUIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }
        transform.parent = null;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetAll();
    }

    public void SetCanvas(string process)
    {
        DisableAll();

        switch (process)
        {
            case "listening":
                listeningIconObject.SetActive(true);
                break;
            case "thinking":
                thinkingIconObject.SetActive(true);
                break;
            case "speaking":
                speakingIconObject.SetActive(true);
                break;
        }
    }

    public void SetAll()
    {
        ConversationProcessCanvas conversationProcessCanvas = FindObjectOfType<ConversationProcessCanvas>();
        if(conversationProcessCanvas == null)
        {
            Debug.LogWarning("No conversation canvas");
            return;
        }
        listeningIconObject = conversationProcessCanvas.ListeningIconObject;
        thinkingIconObject = conversationProcessCanvas.ThinkingIconObject;
        speakingIconObject = conversationProcessCanvas.SpeakingIconObject1;
    }

    public void DisableAll()
    {
        listeningIconObject.SetActive(false);
        thinkingIconObject.SetActive(false);
        speakingIconObject.SetActive(false);
    }
}
