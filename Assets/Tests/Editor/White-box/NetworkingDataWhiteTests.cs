using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using System.Net;
using System.Net.Sockets;

public class NetworkingDataWhiteTests
{
    private GameObject go;
    private NetworkingData networkingData;

    [SetUp]
    public void Setup()
    {
        go = new GameObject("NetworkingDataTestGO");
        networkingData = go.AddComponent<NetworkingData>();
        networkingData.serverURL = "127.0.0.1";
        networkingData.serverPort = "8000";
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(go);
    }

    [Test]
    public void GetLocalURL_ReturnsExpectedValue()
    {
        SetPrivateField(networkingData, "localURL", "192.168.1.100");
        Assert.AreEqual("192.168.1.100", networkingData.GetLocalURL());
    }

    [Test]
    public void GetLocalIPAddress_ReturnsValidIPv4()
    {
        var method = typeof(NetworkingData).GetMethod("GetLocalIPAddress", BindingFlags.Instance | BindingFlags.NonPublic);
        string ip = (string)method.Invoke(networkingData, null);

        Assert.IsNotNull(ip);
        Assert.IsTrue(IPAddress.TryParse(ip, out var parsed));
        Assert.AreEqual(AddressFamily.InterNetwork, parsed.AddressFamily);
    }

    // Hulpmethode
    private void SetPrivateField<T>(object obj, string fieldName, T value)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(obj, value);
    }
}
