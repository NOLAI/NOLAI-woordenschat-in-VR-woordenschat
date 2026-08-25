/*
 * ClipToWavConverter.cs - Converts Unity AudioClips to WAV format
 *
 * This utility class handles audio conversion:
 * - Trims AudioClip to specific sample count
 * - Converts float samples to WAV binary format
 * - Creates proper RIFF/WAVE headers for compatibility
 */

using UnityEngine;
using System.IO;


public class ClipToWavConverter
{
    public AudioClip TrimAudioClip(AudioClip clip, int recordedSamples)
    {
        float[] samples = new float[recordedSamples * clip.channels];
        clip.GetData(samples, 0);

        AudioClip trimmedClip = AudioClip.Create(clip.name, recordedSamples, clip.channels, clip.frequency, false);
        trimmedClip.SetData(samples, 0);

        return trimmedClip;
    }

    public byte[] ConvertAudioClipToWav(float[] samples, int channels, int frequency)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        int sampleCount = samples.Length;
        int byteRate = frequency * channels * 2;

        writer.Write("RIFF".ToCharArray());
        writer.Write(36 + sampleCount * 2);
        writer.Write("WAVE".ToCharArray());
        writer.Write("fmt ".ToCharArray());
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)channels);
        writer.Write(frequency);
        writer.Write(byteRate);
        writer.Write((short)(channels * 2));
        writer.Write((short)16);
        writer.Write("data".ToCharArray());
        writer.Write(sampleCount * 2);

        foreach (var sample in samples)
        {
            short val = (short)(sample * 32767);
            writer.Write(val);
        }

        writer.Flush();
        return stream.ToArray();
    }
}
