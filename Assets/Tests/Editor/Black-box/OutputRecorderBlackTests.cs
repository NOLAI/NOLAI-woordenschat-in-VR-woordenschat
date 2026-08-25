using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using UnityEngine.TestTools;

public class OutputRecorderBlackTests
{
    private GameObject go;
    private OutputRecorder recorder;

    [SetUp]
    public void Setup()
    {
        go = new GameObject("Recorder");
        recorder = go.AddComponent<OutputRecorder>();
        // Forceer Awake (optioneel)
        typeof(OutputRecorder).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(recorder, null);
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(go);
    }

    [Test]
    public void UpdateURL_LogsError_WhenNetworkingDataMissing()
    {
        LogAssert.Expect(LogType.Error, "NetworkingData component missing or URL/Port not set!");

        // Roep de private UpdateURL aan via reflection
        var method = typeof(OutputRecorder).GetMethod("UpdateURL", BindingFlags.Instance | BindingFlags.NonPublic);
        method.Invoke(recorder, null);

        LogAssert.NoUnexpectedReceived(); // Zorgt dat alleen deze error is gelogd
    }

    [Test]
    public void Start_LogsError_WhenNetworkingDataMissing()
    {
        LogAssert.Expect(LogType.Error, "NetworkingData component missing or URL/Port not set!");

        typeof(OutputRecorder).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(recorder, null);

        LogAssert.NoUnexpectedReceived();
    }
}
