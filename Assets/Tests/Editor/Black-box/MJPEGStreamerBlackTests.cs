using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

public class MJPEGStreamerBlackTests
{
    private GameObject go;
    private MJPEGStreamer streamer;

    [SetUp]
    public void Setup()
    {
        go = new GameObject("MJPEGStreamer");
        streamer = go.AddComponent<MJPEGStreamer>();

        // Maak een Camera.main aan omdat deze in EditMode normaal null is
        var camGO = new GameObject("MainCamera");
        camGO.AddComponent<Camera>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(go);

        var cam = Camera.main;
        if (cam != null)
            Object.DestroyImmediate(cam.gameObject);
    }

    [Test]
    public void SetAdress_ShouldSetServerAndStartStream()
    {
        streamer.SetAdress("localhost", "8080");  // alleen hostnaam, geen "http://"

        var serverField = typeof(MJPEGStreamer).GetField("server", BindingFlags.NonPublic | BindingFlags.Instance);
        string server = (string)serverField.GetValue(streamer);
        Assert.AreEqual("http://localhost:8080/stream/", server);

        var isStreamingField = typeof(MJPEGStreamer).GetField("isStreaming", BindingFlags.NonPublic | BindingFlags.Instance);
        bool isStreaming = (bool)isStreamingField.GetValue(streamer);
        Assert.IsTrue(isStreaming);

        streamer.StopStream();
    }

    [Test]
    public void StopStream_ShouldSetIsStreamingFalse()
    {
        streamer.SetAdress("localhost", "8080");
        streamer.StopStream();

        var isStreamingField = typeof(MJPEGStreamer).GetField("isStreaming", BindingFlags.NonPublic | BindingFlags.Instance);
        bool isStreaming = (bool)isStreamingField.GetValue(streamer);

        Assert.IsFalse(isStreaming);
    }

    [Test]
    public void Update_ShouldInvokeQueuedActions()
    {
        bool actionExecuted = false;

        var queueField = typeof(MJPEGStreamer).GetField("mainThreadQueue", BindingFlags.NonPublic | BindingFlags.Instance);
        var queue = (Queue<System.Action>)queueField.GetValue(streamer);

        queue.Enqueue(() => actionExecuted = true);

        var updateMethod = typeof(MJPEGStreamer).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
        updateMethod.Invoke(streamer, null);

        Assert.IsTrue(actionExecuted);
    }

    [Test]
    public void CaptureScreenshot_CoroutineRunsWithoutError()
    {
        LogAssert.NoUnexpectedReceived(); // maakt test fail als er foutlogs komen

        var method = typeof(MJPEGStreamer).GetMethod("CaptureScreenshot", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(method, "CaptureScreenshot method not found");

        IEnumerator routine = (IEnumerator)method.Invoke(streamer, null);
        while (routine.MoveNext()) { }

        Assert.Pass("CaptureScreenshot coroutine completed without error logs.");
    }
}
