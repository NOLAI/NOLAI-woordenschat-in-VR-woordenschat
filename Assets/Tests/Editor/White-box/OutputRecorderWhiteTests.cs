using NUnit.Framework;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

public class OutputRecorderWhiteTests
{
    private OutputRecorder recorder;
    private GameObject go;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject("TestOutputRecorder");
        recorder = go.AddComponent<OutputRecorder>();
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void StartRecording_SetsRecOutputAndClearsData()
    {
        // Arrange
        var audioDataField = typeof(OutputRecorder)
            .GetField("audioData", BindingFlags.NonPublic | BindingFlags.Instance);
        List<byte> audioList = (List<byte>)audioDataField.GetValue(recorder);
        audioList.AddRange(new byte[] { 1, 2, 3 }); // simuleer bestaand data

        // Act
        recorder.StartRecording(); // 1e keer — moet data clearen

        // Assert
        Assert.IsTrue(audioList.Count == 0, "audioData was not cleared on StartRecording");
    }

    [Test]
    public void ReceiveAudioData_WhenRecording_CallsConvertAndWrite()
    {
        // Arrange
        recorder.StartRecording(); // zet recOutput = true
        float[] input = { 0.0f, 0.5f, -1.0f };

        // Act
        recorder.ReceiveAudioData(input);

        // Reflecteer audioData
        var audioDataField = typeof(OutputRecorder)
            .GetField("audioData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        List<byte> audioList = (List<byte>)audioDataField.GetValue(recorder);

        // 3 floats → 3 * 2 bytes = 6 bytes
        Assert.AreEqual(6, audioList.Count);
    }

    [Test]
    public void ConvertAndWrite_ConvertsFloatArrayToBytes()
    {
        // Arrange
        float[] input = new float[] { 0f, 1f, -1f };
        var method = typeof(OutputRecorder).GetMethod("ConvertAndWrite",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Reflecteer lijst
        var audioDataField = typeof(OutputRecorder).GetField("audioData",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var localList = new List<byte>();
        audioDataField.SetValue(recorder, localList);

        // Act
        method.Invoke(recorder, new object[] { input });

        // Assert
        Assert.AreEqual(6, localList.Count); // 3 samples * 2 bytes (16-bit PCM)
        short firstSample = BitConverter.ToInt16(localList.ToArray(), 0);
        Assert.AreEqual(0, firstSample); // 0.0f * 32767 = 0
    }

    [Test]
    public void CreateWavFile_GeneratesValidHeaderAndData()
    {
        // Arrange
        byte[] dummyAudio = new byte[4] { 0x01, 0x02, 0x03, 0x04 };
        var method = typeof(OutputRecorder).GetMethod("CreateWavFile",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Inject dummy output rate
        var outputRateField = typeof(OutputRecorder).GetField("outputRate",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        outputRateField.SetValue(recorder, 44100);

        // Act
        byte[] wavBytes = (byte[])method.Invoke(recorder, new object[] { dummyAudio.ToList() });

        // Assert WAV header: starts with "RIFF" and "WAVE"
        Assert.AreEqual("RIFF", System.Text.Encoding.UTF8.GetString(wavBytes.Take(4).ToArray()));
        Assert.AreEqual("WAVE", System.Text.Encoding.UTF8.GetString(wavBytes.Skip(8).Take(4).ToArray()));
        Assert.IsTrue(wavBytes.Length > 44); // WAV header + data
    }
}
