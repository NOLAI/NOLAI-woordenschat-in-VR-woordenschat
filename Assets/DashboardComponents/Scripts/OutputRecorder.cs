using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class OutputRecorder : MonoBehaviour
{
    public static OutputRecorder Instance { get; private set; }
    public string route = "/upload-ingame-audio";

    int outputRate;
    bool recOutput = false;
    AudioClip audioClip;
    int audioClipPosition = 0;
    public AudioSource audioSource;

    // Thread-safe queue voor audio-data
    private ConcurrentQueue<float[]> audioDataQueue = new ConcurrentQueue<float[]>();
    private object audioClipLock = new object();
    private List<float> recordedSamples = new List<float>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            outputRate = AudioSettings.outputSampleRate;
            Debug.Log($"Output sample rate: {outputRate}");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateURL();
    }

    void Update()
    {
        // Verwerk de queue op de main thread
        while (audioDataQueue.TryDequeue(out float[] data))
        {
            if (recOutput)
            {
                lock (audioClipLock)
                {
                    recordedSamples.AddRange(data);
                }
            }
        }
    }

    public void StartRecording()
    {
        if (!recOutput)
        {
            recOutput = true;
            recordedSamples.Clear();
            Debug.Log("In-game audio recording started...");
        }
    }

    public void StopRecording()
    {
        recOutput = false;
        Debug.Log("In-game audio recording stopped");

        // Maak een AudioClip van de opgenomen samples
        audioClip = AudioClip.Create("GameAudio", recordedSamples.Count / 2, 2, outputRate, false);
        audioClip.SetData(recordedSamples.ToArray(), 0);

        byte[] wavFile = ConvertAudioClipToWav(audioClip);
        StartCoroutine(UploadAudioToServer(wavFile, "ingame_audio.wav"));
    }

    public void ReceiveAudioData(float[] data)
    {
        if (recOutput)
        {
            audioDataQueue.Enqueue(data.Clone() as float[]);
        }
    }

    private byte[] ConvertAudioClipToWav(AudioClip clip)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(memoryStream))
            {
                int sampleRate = clip.frequency;
                int channels = clip.channels;
                int sampleCount = clip.samples * channels;
                int byteCount = sampleCount * 2; // 16-bit audio

                // RIFF chunk descriptor
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + byteCount); // File size - 8
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

                // FMT sub-chunk
                writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16); // Subchunk1Size (16 for PCM)
                writer.Write((short)1); // AudioFormat (1 for PCM)
                writer.Write((short)channels); // NumChannels
                writer.Write(sampleRate); // SampleRate
                writer.Write(sampleRate * channels * 2); // ByteRate
                writer.Write((short)(channels * 2)); // BlockAlign
                writer.Write((short)16); // BitsPerSample

                // DATA sub-chunk
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                writer.Write(byteCount); // Subchunk2Size

                // Audio data schrijven
                float[] samples = new float[sampleCount];
                clip.GetData(samples, 0);

                foreach (float sample in samples)
                {
                    short shortSample = (short)(sample * 32767f); // Convert float to 16-bit PCM
                    writer.Write(shortSample);
                }
            }
            return memoryStream.ToArray();
        }
    }

    IEnumerator UploadAudioToServer(byte[] audioBytes, string filename)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("ingame_audio", audioBytes, filename, "audio/wav");

        using (UnityWebRequest www = UnityWebRequest.Post(route, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("In-game audio upload successful: " + www.downloadHandler.text);
            }
            else
            {
                Debug.LogError("In-game audio upload failed: " + www.error);
            }
        }

        GetComponent<ConfirmDataUpload>().ConfirmUpload();
    }

    void UpdateURL()
    {
        string url = this.GetComponent<NetworkingData>().serverURL;
        string port = this.GetComponent<NetworkingData>().serverPort;
        route = $"http://{url}:{port}{route}";
    }
}
