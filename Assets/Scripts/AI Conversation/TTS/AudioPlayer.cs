/*
 * AudioPlayer.cs - Plays TTS audio from OpenAI API
 *
 * This script handles audio playback for TTS:
 * - Receives audio bytes from OpenAI API
 * - Saves audio to persistent data path
 * - Loads and plays MP3 audio
 * - Monitors audio amplitude for silence detection
 */

using System;
using System.IO;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class AudioPlayer : MonoBehaviour
{
    private AudioSource audioSource;
    private const bool deleteCachedFile = true;
    private bool stoppedManually = false;
    private bool ShouldPlayAudio = false;
    private float silenceThreshold = 0.001f; // Define a threshold for silence
    private bool isPlayingBreathAnimation = false;

    private void OnEnable()
    {
        if (!audioSource) this.audioSource = GetComponent<AudioSource>();
    }

    private void OnValidate() => OnEnable();

    public void ProcessAudioBytes(byte[] audioData)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "audio.mp3");
        File.WriteAllBytes(filePath, audioData);

        StartCoroutine(LoadAndPlayAudio(filePath));
    }

    private IEnumerator CheckAmplitudeRoutine()
    {
        while (ShouldPlayAudio)
        {
            float[] samples = new float[256];
            audioSource.GetOutputData(samples, 0);

            float currentAmplitude = 0f;
            foreach (float sample in samples)
            {
                currentAmplitude += Mathf.Abs(sample);
            }

            currentAmplitude /= samples.Length;


            if (currentAmplitude > silenceThreshold)
            {
                if (isPlayingBreathAnimation)
                {
                    PlayMainAnimation();
                }
            }
            else
            {
                if (!isPlayingBreathAnimation)
                {
                    PlayBreathAnimation();
                }
            }

            yield return new WaitForSeconds(0.1f); // Check every 100ms
        }
    }

    private IEnumerator LoadAndPlayAudio(string filePath)
    {
        using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
            audioSource.clip = audioClip;
            audioSource.Play();

            ShouldPlayAudio = true;
            StartCoroutine(CheckAmplitudeRoutine());

            while (audioSource.isPlaying)
                yield return null;

            ShouldPlayAudio = false;
            PlayIdleAnimation();

            if (!stoppedManually)
                TranscriptManager.Instance.EnableRecorder();
            else
                stoppedManually = false;
        }
        else Debug.LogError("Audio file loading error: " + www.error);

        if (deleteCachedFile) File.Delete(filePath);
    }

    private void PlayMainAnimation()
    {
        if (TranscriptUIManager.Instance != null)
            TranscriptUIManager.Instance.SetCanvas("speaking");

        if (TranscriptManager.Instance.Npc != null && TranscriptManager.Instance.Npc.Ani != null)
        {
            if (TranscriptManager.Instance.Npc.Ani.isMale)
                TranscriptManager.Instance.Npc.Ani.CrossfadeToAnimation("TPM_talk1", 0.1f);
            else
                TranscriptManager.Instance.Npc.Ani.CrossfadeToAnimation("TPF_talk2", 0.1f);
        }

        isPlayingBreathAnimation = false;
    }

    private void PlayBreathAnimation()
    {
        Debug.Log("Breathing");
        if (TranscriptManager.Instance.Npc != null && TranscriptManager.Instance.Npc.Ani != null)
        {
            if (TranscriptManager.Instance.Npc.Ani.isMale)
                TranscriptManager.Instance.Npc.Ani.CrossfadeToAnimation("TPM_idle1", 0.1f);
            else
                TranscriptManager.Instance.Npc.Ani.CrossfadeToAnimation("TPF_idle1", 0.1f);
        }

        isPlayingBreathAnimation = true;
    }

    private void PlayIdleAnimation()
    {
        if (TranscriptManager.Instance.Npc != null && TranscriptManager.Instance.Npc.Ani != null)
        {
            if (TranscriptManager.Instance.Npc.Ani.isMale)
                TranscriptManager.Instance.Npc.Ani.PlayNewAnimation("TPM_idle1");
            else
                TranscriptManager.Instance.Npc.Ani.PlayNewAnimation("TPF_idle1");
        }

        isPlayingBreathAnimation = false;
    }

    public void UpdateAudioSourceSetting(float pitch, float volume)
    {
        audioSource.pitch = pitch;
        audioSource.volume = volume;
    }

    public void StopPlaying()
    {
        print("Audioplayer stop");
        if (audioSource.isPlaying)
        {
            stoppedManually = true;
            audioSource.Stop();
        }
    }
}
