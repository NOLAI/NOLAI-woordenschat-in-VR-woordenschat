using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class MicrophoneRecorder : MonoBehaviour
{
    // URL naar het endpoint voor het uploaden van audio
    public string route = "/upload-microphone-audio";

    // Veld om de huidige vraag op te slaan
    public string AskedQuestion = "";

    // Lijst om de huidige antwoordopties op te slaan
    public List<string> AnswerOptions = new List<string>();
    private int currentElementId = 1;

    // Variabelen voor het opnemen van audio
    AudioClip audioClip;
    string microphoneName;
    bool isRecording = false;
    int sampleRate = 44100;
    float recordingStartTime;

    // Start wordt aangeroepen bij het initialiseren van het script
    void Start()
    {
        // Update de URL naar de server
        UpdateURL();

        // Controleer of er een microfoon beschikbaar is
        if (Microphone.devices.Length > 0)
        {
            microphoneName = Microphone.devices[0];
            Debug.Log("Microphone selected: " + microphoneName);
        }
        else
        {
            Debug.LogError("Could not connect to a microphone. Is a microphone connected to the host device?");
        }
    }

    // Start het opnemen van audio
    public void StartRecording()
    {
        if (isRecording) return;

        Debug.Log("Microphone audio recording started..." );
        isRecording = true;
        recordingStartTime = Time.time;
        audioClip = Microphone.Start(microphoneName, false, 600, sampleRate);

        StartCoroutine(WaitForMicrophoneStart());
    }

    // Wacht tot de microfoon actief is
    IEnumerator WaitForMicrophoneStart()
    {
        yield return new WaitForSeconds(0.5f);

        if (Microphone.IsRecording(microphoneName))
        {
            Debug.Log("Microphone Active.");
        }
        else
        {
            Debug.LogError("Microphone is not active.");
            isRecording = false;
        }
    }

    // Stop het opnemen van audio
    public void StopRecording()
    {
        if (!isRecording) return;

        if (Time.time - recordingStartTime < 1f)
        {
            Debug.LogError("Microphone audio must be at least a second long");
            return;
        }

        Debug.Log("Microphone audio recording stopped.");
        int length = Microphone.GetPosition(microphoneName);
        Microphone.End(microphoneName);
        isRecording = false;

        if (length <= 0)
        {
            Debug.LogError("There was no audio recorded.");
            return;
        }

        // Haal de vraag en antwoordopties op
        GetCurrentQuestionAndOptions();

        // Trim de audio en converteer naar WAV
        AudioClip trimmedClip = TrimAudioClip(audioClip, length);
        byte[] wavData = ConvertAudioClipToWav(trimmedClip);

        // Upload de audio naar de server
        StartCoroutine(UploadAudio(wavData, "microphone_audio.wav")); 
    }

    public void SetElementId(int elementId)
    {
        currentElementId = elementId;
        Debug.Log("Current element ID set to: " + currentElementId);
    }

    // haal de vraag en antwoorden op uit het unity createdashboard met de element id van taskmanager
    private void GetCurrentQuestionAndOptions()
    {
        CreateDashboard createDashboard = FindObjectOfType<CreateDashboard>();
        if (createDashboard != null)
        {
            var (currentQuestion, currentAnswerOptions) = createDashboard.GetQuestionAndAnswerOptionsForElement(currentElementId);
            AskedQuestion = currentQuestion;
            AnswerOptions = currentAnswerOptions.ConvertAll(option => option.text);
            Debug.Log("Current question: " + AskedQuestion);
            Debug.Log("Current answer options count: " + AnswerOptions.Count);
        }
        else
        {
            Debug.LogError("CreateDashboard not found!");
        }
    }


    // Trim de audio om onnodige stilte te verwijderen
    private AudioClip TrimAudioClip(AudioClip clip, int length)
    {
        float[] samples = new float[length * clip.channels];
        clip.GetData(samples, 0);
        AudioClip trimmedClip = AudioClip.Create("TrimmedClip", length, clip.channels, clip.frequency, false);
        trimmedClip.SetData(samples, 0);
        return trimmedClip;
    }

    // Converteer de AudioClip naar WAV-formaat
    private byte[] ConvertAudioClipToWav(AudioClip clip)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            int sampleCount = clip.samples * clip.channels;
            int byteCount = sampleCount * 2;

            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                // Schrijf de WAV-header
                writer.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"));
                writer.Write(36 + byteCount);
                writer.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"));
                writer.Write(System.Text.Encoding.UTF8.GetBytes("fmt "));
                writer.Write(16);
                writer.Write((ushort)1);
                writer.Write((ushort)clip.channels);
                writer.Write(clip.frequency);
                writer.Write(clip.frequency * clip.channels * 2);
                writer.Write((ushort)(clip.channels * 2));
                writer.Write((ushort)16);
                writer.Write(System.Text.Encoding.UTF8.GetBytes("data"));
                writer.Write(byteCount);

                // Schrijf de audio samples
                float[] samples = new float[sampleCount];
                clip.GetData(samples, 0);
                foreach (float sample in samples)
                {
                    short value = (short)(sample * short.MaxValue);
                    writer.Write(value);
                }
            }

            return stream.ToArray();
        }
    }

    // Upload de audio naar de server
    IEnumerator UploadAudio(byte[] audioData, string filename)
    {
        // Maak een WWWForm om de audio en metadata te versturen
        WWWForm form = new WWWForm();
        form.AddBinaryData("microphone_audio", audioData, filename, "audio/wav");
        form.AddField("AskedQuestion", AskedQuestion);

        // Combineer de antwoordopties tot een enkele string gescheiden door komma's
        string answerOptionsString = string.Join(",", AnswerOptions);
        form.AddField("AnswerOptions", answerOptionsString);

        // Stuur de audio en metadata naar de server
        using (UnityWebRequest www = UnityWebRequest.Post(route, form))
        {
            yield return www.SendWebRequest();

            // Controleer of het uploaden succesvol was
            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Microfoon audio upload successful: " + www.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Microfoon audio upload failed: " + www.error);
            }
        }

        // Bevestig dat de data is geüpload
        GetComponent<ConfirmDataUpload>().ConfirmUpload();
    }

    // Klasse om de JSON-response te parsen
    [Serializable]
    public class ServerResponse
    {
        public bool success;
        public string error;
        public string step;
        public string message;
        public int evaluation;
        public string transcription;
    }

    // Update de URL naar de server
    void UpdateURL()
    {
        NetworkingData networkingData = GetComponent<NetworkingData>();
        if (networkingData != null)
        {
            string url = networkingData.serverURL;
            string port = networkingData.serverPort;
            route = $"http://{url}:{port}{route}";
        }
        else
        {
            Debug.LogError("NetworkingData component not found!");
        }
    }
}
