using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using UnityEngine.SceneManagement;
using System.Threading;

public class MJPEGStreamerWhiteTests
{
    private GameObject go;
    private MJPEGStreamer streamer;

    [SetUp]
    public void Setup()
    {
        go = new GameObject();
        streamer = go.AddComponent<MJPEGStreamer>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(go);
    }

    [Test]
    public void StopStream_SetsIsStreamingFalseAndNullsOutFields()
    {
        SetPrivateField(streamer, "isStreaming", true);
        SetPrivateField(streamer, "listener", new System.Net.HttpListener());

        // Maak en start een dummy thread die meteen eindigt
        var thread = new System.Threading.Thread(() => Thread.Sleep(10));
        thread.Start();
        SetPrivateField(streamer, "streamThread", thread);

        streamer.StopStream();

        Assert.IsFalse(GetPrivateField<bool>(streamer, "isStreaming"));
        Assert.IsNull(GetPrivateField<System.Net.HttpListener>(streamer, "listener"));
        Assert.IsNull(GetPrivateField<System.Threading.Thread>(streamer, "streamThread"));
    }

    [Test]
    public void OnApplicationQuit_CallsStopStreamAndCleansUp()
    {
        var renderTex = new RenderTexture(640, 360, 24);
        SetPrivateField(streamer, "renderTexture", renderTex);
        SetPrivateField(streamer, "isStreaming", true);

        var method = typeof(MJPEGStreamer).GetMethod("OnApplicationQuit", BindingFlags.Instance | BindingFlags.NonPublic);
        method.Invoke(streamer, null);

        var isStreaming = GetPrivateField<bool>(streamer, "isStreaming");
        var cleanedRT = GetPrivateField<RenderTexture>(streamer, "renderTexture");

        Assert.IsFalse(isStreaming);
        Assert.IsTrue(cleanedRT == null); // Unity's null check
    }

    // Hulpmethoden
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