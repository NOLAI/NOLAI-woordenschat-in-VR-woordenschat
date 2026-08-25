/*
 * TranscriptManager.cs - Manages AI conversation transcripts
 *
 * This script handles conversation processing:
 * - Singleton pattern for global access
 * - Manages conversation messages with AI
 * - Supports WhisperAI or ChatGPT backends
 * - Integrates with NPC information for context
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRLL.Utilities;

public class TranscriptManager : MonoBehaviour
{
    [SerializeField] ChatGPT_General generalInformation;
    [SerializeField] bool useWhisperAI;
    [SerializeField] bool useChatGPT;
    [SerializeField] string crashBackupResponse;

    [Header("Dev Options: ")]
    [SerializeField] bool startRec = false;
    [SerializeField] bool stopRec = false;

    private HttpClientUnity httpClientUnity;
    [SerializeField] private NpcInformation npc;

    private List<string> conversationMessages = new();
    private int amountOfTries = 0;

    public static TranscriptManager Instance { get; private set; }
    public NpcInformation Npc { get => npc; }


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
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        httpClientUnity = GetComponent<HttpClientUnity>();
    }

    // Update is called once per frame
    void Update()
    {
        if (startRec)
        {
            EnableRecorder();
            startRec = false;
        }
        if (stopRec)
        {
            DisableRecorder(true);
            stopRec = false;
        }
    }

    public void EnableRecorder()
    {
        RecorderManager.Instance.StartListening();

        if(TranscriptUIManager.Instance != null)
            TranscriptUIManager.Instance.SetCanvas("listening");

        if(npc != null && npc.Ani != null)
        {
            if (npc.Ani.isMale)
                npc.Ani.PlayNewAnimation("TPM_idle1");
            else
                npc.Ani.PlayNewAnimation("TPF_idle1");
        }
    }

    public void DisableRecorder(bool endConvo)
    {
        RecorderManager.Instance.StopListening();

        if(endConvo)
            conversationMessages.Clear();

        if (TranscriptUIManager.Instance != null)
            TranscriptUIManager.Instance.DisableAll();
    }

    public void SetNPC(NpcInformation newNpc)
    {
        DisableRecorder(true);
        npc = newNpc;
    }

    public async void CheckToSendToWhisperAI(byte[] audioBytes, string fileName)
    {
        if (useWhisperAI)
        {
            DisableRecorder(false);
            if (TranscriptUIManager.Instance != null)
                TranscriptUIManager.Instance.SetCanvas("thinking");

            string responseBody = await httpClientUnity.SendAudio(audioBytes, fileName);
            CheckToSendToChatGPT(responseBody);
        }
    }

    public void CheckToSendToChatGPT(string transcription)
    {
        if (useChatGPT)
        {
            conversationMessages.Add($"Q: {transcription} \n");

            // Call GetAnswer with a question and a callback function
            StartCoroutine(AITalk.GetAnswer(CreateFullPrompt(), HandleResponse));
        }
    }

    private string CreateFullPrompt()
    {
        string prompt = "";

        prompt = generalInformation.DefaultPrompt + "\n\n";
        prompt += "The following info is the info about the game itself: \n" +
                  $"The Game Story is: {generalInformation.GeneralGameStory} \n" +
                  $"The Game World is: {generalInformation.GeneralGameWorld} \n";

        prompt += "The following info is the info about you, the NPC: \n" +
                  npc.GetPrompt();

        prompt += "The following info is info about the player who is talking to you: \n\n" +
                  generalInformation.PlayerInformation + "\n\n";

        prompt += "The following info is info on the ongoing conversation between you and the player: \n";

        foreach (string message in conversationMessages)
            prompt += message;

        //prompt += $"The question or message the player said: {question}";

        print("Full Prompt = " + prompt);
        return prompt;
    }

    private void HandleResponse(string response)
    {
        print("ChatGPT answer is: " + response);
        amountOfTries++;

        if(amountOfTries >= 3)
        {
            conversationMessages.Add($"A: {crashBackupResponse} \n");
            TTSManager.Instance.SynthesizeAndPlay(crashBackupResponse, npc.TtsModel);
            amountOfTries = 0;
            return;
        }

        if (response.Length < 10 || response.ToLower().Contains("error") || response.Contains("\\"))
        {
            StartCoroutine(AITalk.GetAnswer(CreateFullPrompt(), HandleResponse));
            return;
        }

        amountOfTries = 0;
        conversationMessages.Add($"A: {response} \n");

        TTSManager.Instance.SynthesizeAndPlay(response, npc.TtsModel);
    }

    public void StartWithNPCLine(string line)
    {
        conversationMessages.Add($"A: {line} \n");
        if (TranscriptUIManager.Instance != null)
            TranscriptUIManager.Instance.SetCanvas("thinking");
        TTSManager.Instance.SynthesizeAndPlay(line, npc.TtsModel);
    }
}
