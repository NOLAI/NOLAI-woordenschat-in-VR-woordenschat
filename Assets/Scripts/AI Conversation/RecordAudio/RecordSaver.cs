/*
 * RecordSaver.cs - Saves recorded audio to WAV files
 *
 * This utility class handles audio recording storage:
 * - Creates recordings directory structure
 * - Converts audio to WAV format
 * - Saves to persistent data path
 * - Supports both single and separate recordings
 */

using UnityEngine;
using System.IO;


public class RecordSaver
{
    private string outputFilePath;
    private string separateRecordingsPath;
    private ClipToWavConverter wavConverter;

    void Setup()
    {
        outputFilePath = Path.Combine(Application.persistentDataPath, "recordedAudio.wav");
        separateRecordingsPath = Path.Combine(Application.persistentDataPath, "Recordings");

        // Create the recordings directory if it doesn't exist
        if (!Directory.Exists(separateRecordingsPath))
        {
            Directory.CreateDirectory(separateRecordingsPath);
        }

        wavConverter = new();
    }

    public void SaveRecording(AudioClip clip)
    {
        Setup();

        if (clip == null)
        {
            Debug.LogError("No audio clip to save");
            return;
        }

        // Create a file with the WAV format
        var samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);
        byte[] wavFile = wavConverter.ConvertAudioClipToWav(samples, clip.channels, clip.frequency);
        File.WriteAllBytes(outputFilePath, wavFile);
        Debug.Log("Audio saved to " + outputFilePath);

        TranscriptManager.Instance.CheckToSendToWhisperAI(wavFile, "recordedAudio.wav");
    }

    public void SaveSeparateRecording(AudioClip clip)
    {
        Setup();

        if (clip == null)
        {
            Debug.LogError("No audio clip to save");
            return;
        }

        // Generate a unique file name based on the current time
        string timestamp = System.DateTime.Now.ToString("dd-MM-yyyy_HH-mm");
        string filePath = Path.Combine(separateRecordingsPath, "VoiceRec (" + timestamp + ").wav");

        // Create a file with the WAV format
        var samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);
        byte[] wavFile = wavConverter.ConvertAudioClipToWav(samples, clip.channels, clip.frequency);
        File.WriteAllBytes(filePath, wavFile);
        Debug.Log("Separate recording saved to " + filePath);
    }
}
