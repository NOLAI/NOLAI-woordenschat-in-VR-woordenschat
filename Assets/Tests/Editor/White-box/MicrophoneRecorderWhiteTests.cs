using NUnit.Framework;
using UnityEngine;
using System.Collections;
using UnityEngine.TestTools;
using System.Reflection;

public class MicrophoneRecorderWhiteTests
{
    private GameObject go;
    private MicrophoneRecorder recorder;

    [SetUp]
    public void Setup()
    {
        go = new GameObject();
        recorder = go.AddComponent<MicrophoneRecorder>();

        // Mock NetworkingData component met dummy waarden
        var netData = go.AddComponent<NetworkingData>();
        netData.serverURL = "localhost";
        netData.serverPort = "8080";
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(go);
    }

    [Test]
    public void UpdateURL_UpdatesRouteCorrectly()
    {
        // call private UpdateURL()
        MethodInfo updateUrlMethod = typeof(MicrophoneRecorder).GetMethod("UpdateURL", BindingFlags.NonPublic | BindingFlags.Instance);
        updateUrlMethod.Invoke(recorder, null);

        Assert.AreEqual("http://localhost:8080/upload-audio", recorder.route);
    }

    [Test]
    public void StartRecording_SetsIsRecordingTrueAndRecordsStartTime()
    {
        recorder.StartRecording();

        var isRecording = GetPrivateField<bool>(recorder, "isRecording");
        var recordingStartTime = GetPrivateField<float>(recorder, "recordingStartTime");

        Assert.IsTrue(isRecording);
        Assert.Greater(recordingStartTime, 0f);
    }

    [Test]
    public void StopRecording_DoesNotStopIfRecordingShorterThanOneSecond()
    {
        recorder.StartRecording();

        // Simuleer opname korter dan 1s
        SetPrivateField(recorder, "recordingStartTime", Time.time);

        LogAssert.Expect(LogType.Error, "Microphone audio must be at least a second long");

        recorder.StopRecording();

        var isRecording = GetPrivateField<bool>(recorder, "isRecording");
        Assert.IsTrue(isRecording); // nog steeds aan want stop niet door korte opname
    }

    [Test]
    public void TrimAudioClip_ReturnsCorrectlyTrimmedClip()
    {
        int samplesLength = 10;
        AudioClip clip = AudioClip.Create("TestClip", samplesLength, 1, 44100, false);
        float[] data = new float[samplesLength];
        for (int i = 0; i < samplesLength; i++) data[i] = i / 10f;
        clip.SetData(data, 0);

        // Roep private methode via reflection
        MethodInfo trimMethod = typeof(MicrophoneRecorder).GetMethod("TrimAudioClip", BindingFlags.NonPublic | BindingFlags.Instance);
        AudioClip trimmed = (AudioClip)trimMethod.Invoke(recorder, new object[] { clip, 5 });

        Assert.AreEqual(5, trimmed.samples);
        float[] trimmedData = new float[5];
        trimmed.GetData(trimmedData, 0);
        Assert.AreEqual(data[0], trimmedData[0]);
        Assert.AreEqual(data[4], trimmedData[4]);
    }

    [Test]
    public void ConvertAudioClipToWav_ReturnsValidWavByteArray()
    {
        int samplesLength = 5;
        AudioClip clip = AudioClip.Create("TestClip", samplesLength, 1, 44100, false);
        float[] data = new float[samplesLength];
        for (int i = 0; i < samplesLength; i++) data[i] = 0.5f;
        clip.SetData(data, 0);

        MethodInfo convertMethod = typeof(MicrophoneRecorder).GetMethod("ConvertAudioClipToWav", BindingFlags.NonPublic | BindingFlags.Instance);
        byte[] wavBytes = (byte[])convertMethod.Invoke(recorder, new object[] { clip });

        Assert.IsNotNull(wavBytes);
        Assert.IsTrue(wavBytes.Length > 44); // Min header size
        Assert.AreEqual((byte)'R', wavBytes[0]); // 'R' van RIFF header
        Assert.AreEqual((byte)'I', wavBytes[1]);
        Assert.AreEqual((byte)'F', wavBytes[2]);
        Assert.AreEqual((byte)'F', wavBytes[3]);
    }

    // Hulpmethoden om private fields te lezen en zetten
    private T GetPrivateField<T>(object obj, string fieldName)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        return (T)field.GetValue(obj);
    }
    private void SetPrivateField<T>(object obj, string fieldName, T value)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(obj, value);
    }
}
