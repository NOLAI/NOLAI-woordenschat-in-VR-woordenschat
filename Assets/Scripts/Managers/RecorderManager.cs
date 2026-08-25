/*
 * RecorderManager.cs - Singleton manager for audio recording
 *
 * This script handles microphone recording:
 * - Singleton pattern for global access
 * - Records audio using Unity's Microphone API
 * - Converts and saves recordings as WAV files
 * - Detects silence to stop recording
 */

using UnityEngine;
using System.IO;

public class RecorderManager : MonoBehaviour
{
    private AudioClip audioClip;
    private string microphone;
    private bool isRecording = false;
    private bool isListening = false;
    private float silenceThreshold = 0.01f;
    private int sampleRate = 44100;
    private float silenceDuration = 1.0f; // Duration in seconds to consider as silence
    private float silenceTimer = 0f;
    private int recordedSamples = 0;
    private int bufferLength = 1; // Buffer length in seconds for listening
    private int sampleWindow = 1024; // Number of samples for RMS calculation
    private float[] sampleBuffer;

    private ClipToWavConverter wavConverter;
    private RecordSaver recordSaver;

    public static RecorderManager Instance { get; private set; }
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

    void Start()
    {
        wavConverter = new();
        recordSaver = new();

        // Get the default microphone
        if (Microphone.devices.Length > 0)
        {
            microphone = Microphone.devices[0];
        }
        else
        {
            Debug.LogError("No microphone found");
        }

        sampleBuffer = new float[sampleWindow];

    }

    void Update()
    {
        if (isListening)
        {
            ListenForSpeech();
        }
        else if(isRecording)
        {
            MonitorRecording();
        }
    }

    public void StartListening()
    {
        if (microphone != null)
        {
            audioClip = Microphone.Start(microphone, true, bufferLength, sampleRate); // Short buffer for listening
            isListening = true;
            Debug.Log("Listening for speech...");
        }
    }

    public void StopListening()
    {
        if (microphone != null)
        {
            Microphone.End(microphone); // Stop the listening audio clip
            isListening = false;
            Debug.Log("Stopped listening for speech...");
        }
    }

    void ListenForSpeech()
    {
        int micPosition = Microphone.GetPosition(microphone);
        if (micPosition < sampleWindow) return;

        audioClip.GetData(sampleBuffer, micPosition - sampleWindow);
        float rmsValue = CalculateRMS(sampleBuffer);

        if (rmsValue > silenceThreshold)
        {
            StartRecording();
        }
    }

    void StartRecording()
    {
        if (microphone != null)
        {
            Microphone.End(microphone); // Stop the listening audio clip
            audioClip = Microphone.Start(microphone, false, 60, sampleRate);
            isRecording = true;
            isListening = false;
            silenceTimer = 0f;
            recordedSamples = 0;
            Debug.Log("Recording started...");
        }
    }

    void MonitorRecording()
    {
        int micPosition = Microphone.GetPosition(microphone);
        if (micPosition < sampleWindow) return;

        audioClip.GetData(sampleBuffer, micPosition - sampleWindow);
        float rmsValue = CalculateRMS(sampleBuffer);

        if (rmsValue < silenceThreshold)
        {
            silenceTimer += Time.deltaTime;
        }
        else
        {
            silenceTimer = 0f;
        }

        recordedSamples = micPosition;

        if (silenceTimer >= silenceDuration)
        {
            StopRecording();
        }
    }

    void StopRecording()
    {
        if (microphone != null && isRecording)
        {
            Microphone.End(microphone);
            isRecording = false;

            // Trim the audio clip to the recorded length
            AudioClip trimmedClip = wavConverter.TrimAudioClip(audioClip, recordedSamples);

            recordSaver.SaveRecording(trimmedClip);
            recordSaver.SaveSeparateRecording(trimmedClip);

            Debug.Log("Recording stopped and saved...");
        }
    }

    float CalculateRMS(float[] samples)
    {
        float sum = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            sum += samples[i] * samples[i];
        }
        return Mathf.Sqrt(sum / samples.Length);
    }
}
