using UnityEngine;
using System.Collections;
using System.IO;
using System;
using UnityEngine.Networking;

public class MicrophoneClip : MonoBehaviour
{
    public string shortStreamRoute = "/stream-audio";
    public float duration = 5f;
    public AITestConsole aITestConsole;

    string microphoneName;
    readonly int sampleRate = 16000;
    string newActivity;
    string newMoment;

    void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            microphoneName = Microphone.devices[0];
        }
        else
        {
            Debug.LogError("Geen microfoon beschikbaar.");
        }
    }

    public void StreamShortRecording(string activity, string moment) // OB, A1, A2; M1, M2, Mn
    {
        Debug.Log("Microphone is active");

        newActivity = activity;
        newMoment = moment;

        StartCoroutine(ShortStreamRoutine(duration));
    }

    IEnumerator ShortStreamRoutine(float duration)
    {
        AudioClip shortClip = Microphone.Start(microphoneName, false, Mathf.CeilToInt(duration), sampleRate);
        yield return new WaitForSeconds(duration);

        int position = Microphone.GetPosition(microphoneName);
        Microphone.End(microphoneName);

        if (position <= 0)
        {
            Debug.LogError("Geen audio opgenomen.");
            yield break;
        }

        AudioClip trimmedClip = TrimAudioClip(shortClip, position);
        byte[] wavData = ConvertAudioClipToWav(trimmedClip);

        StartCoroutine(UploadAudio(wavData, "short_stream.wav"));
    }

    AudioClip TrimAudioClip(AudioClip clip, int length)
    {
        float[] samples = new float[length * clip.channels];
        clip.GetData(samples, 0);

        AudioClip trimmedClip = AudioClip.Create("TrimmedClip", length, clip.channels, clip.frequency, false);
        trimmedClip.SetData(samples, 0);

        return trimmedClip;
    }

    byte[] ConvertAudioClipToWav(AudioClip clip)
    {
        using MemoryStream stream = new MemoryStream();
        int sampleCount = clip.samples * clip.channels;
        int byteCount = sampleCount * 2;
        int headerSize = 36 + byteCount;

        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            writer.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"));
            writer.Write(headerSize);
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
    IEnumerator UploadAudio(byte[] audioData, string filename)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("audio", audioData, filename, "audio/wav");
        form.AddField("activity", newActivity);
        form.AddField("moment", newMoment);

        string url = GetComponent<NetworkingData>()?.serverURL ?? "localhost";
        string port = GetComponent<NetworkingData>()?.serverPort ?? "8000";
        string fullRoute = $"http://{url}:{port}{shortStreamRoute}";

        Debug.Log("Sending audio clip...");
        using UnityWebRequest www = UnityWebRequest.Post(fullRoute, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Clip succesvol verwerkt");

            string jsonResponse = www.downloadHandler.text;
            // Parse de JSON naar een object
            ServerResponse responseData = JsonUtility.FromJson<ServerResponse>(jsonResponse);

            ReturnOutput(responseData.output.response);
        }
        else
        {
            Debug.LogError("Upload van korte audio mislukt: " + www.error);
        }
    }

    void ReturnOutput(string output)
    {
        Debug.Log(output);
        aITestConsole.DisplayResponse(output); // alleen voor de test scenen, line kan weg

        // vanaf hier verwerking van de output
    }
}

[Serializable]
public class PythonResponse
{
    public string response;
}

[Serializable]
public class ServerResponse
{
    public string message;
    public PythonResponse output;
    public string error;  // in geval van fouten
}