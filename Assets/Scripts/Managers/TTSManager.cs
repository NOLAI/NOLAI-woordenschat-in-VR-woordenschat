/*
 * TTSManager.cs - Manages Text-to-Speech for NPCs
 *
 * This script handles TTS operations:
 * - Singleton pattern for global access
 * - Synthesizes speech from OpenAI API
 * - Plays audio with configurable settings
 * - Coordinates with OpenAIWrapper and AudioPlayer
 */

using System;
using UnityEngine;

public class TTSManager : MonoBehaviour
{
    private OpenAIWrapper openAIWrapper;
    [SerializeField] private AudioPlayer audioPlayer;

    public static TTSManager Instance { get; private set; }

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
        if (!openAIWrapper) this.openAIWrapper = FindObjectOfType<OpenAIWrapper>();
        if (!audioPlayer) this.audioPlayer = GetComponentInChildren<AudioPlayer>();
    }

    public async void SynthesizeAndPlay(string text, NPCTextToSpeech ttsModel)
    {
        Debug.Log("Trying to synthesize " + text);
        byte[] audioData = await openAIWrapper.RequestTextToSpeech(text, ttsModel.Model, ttsModel.Voice, ttsModel.Speed);
        if (audioData != null)
        {
            Debug.Log("Playing audio.");
            audioPlayer.UpdateAudioSourceSetting(ttsModel.Pitch, ttsModel.Volume);
            audioPlayer.ProcessAudioBytes(audioData);
        }
        else Debug.LogError("Failed to get audio data from OpenAI.");
    }

    public void StopSpeak()
    {
        audioPlayer.StopPlaying();
    }
}