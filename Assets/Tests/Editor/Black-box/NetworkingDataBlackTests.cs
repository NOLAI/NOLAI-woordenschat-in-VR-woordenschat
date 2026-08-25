using NUnit.Framework;
using UnityEngine;
using System.Collections;
using UnityEngine.TestTools;

public class NetworkingDataBlackTests
{
    GameObject go;
    NetworkingData networking;
    MJPEGStreamer fakeStreamer;

    class FakeMJPEGStreamer : MJPEGStreamer
    {
        public string lastIP;
        public string lastPort;

        public override void SetAdress(string ip, string port)
        {
            lastIP = ip;
            lastPort = port;
        }
    }

    [SetUp]
    public void Setup()
    {
        go = new GameObject("NetworkingData");
        networking = go.AddComponent<NetworkingData>();
        fakeStreamer = go.AddComponent<FakeMJPEGStreamer>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Start_ShouldSetLocalIPAddress()
    {
        networking.serverURL = "dummy"; // Required to prevent null in coroutine
        networking.serverPort = "1234";

        // Call Start manually
        typeof(NetworkingData)
            .GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(networking, null);

        string localURL = networking.GetLocalURL();

        Assert.IsFalse(string.IsNullOrEmpty(localURL));
        Assert.AreNotEqual("0.0.0.0", localURL);
    }

    [UnityTest]
    public IEnumerator SendStreamData_ShouldSendPostRequest()
    {
        networking.serverURL = "example.com"; // Geen echte server
        networking.serverPort = "8000";

        // Verwacht een foutmelding van Debug.LogError zonder exception
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Fout bij updaten van Laravel:.*"));

        var routine = networking.SendStreamData("127.0.0.1", "5000");

        while (routine.MoveNext()) yield return null;

        Assert.Pass("Coroutine completed, foutmelding verwacht en opgevangen.");
    }
}
