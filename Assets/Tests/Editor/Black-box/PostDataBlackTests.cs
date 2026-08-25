using NUnit.Framework;
using UnityEngine;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.Networking;

public class PostDataBlackTests
{
    private GameObject go;
    private PostData postData;

    // Dummy NetworkingData met testwaarden
    public class DummyNetworkingData : MonoBehaviour
    {
        public string serverURL = "localhost";
        public string serverPort = "8080";
    }

    [SetUp]
    public void Setup()
    {
        go = new GameObject("PostDataGO");
        var networkingData = go.AddComponent<NetworkingData>();
        networkingData.serverURL = "localhost";
        networkingData.serverPort = "8080";

        postData = go.AddComponent<PostData>();
        
        // Forceer UpdateURL call om setRoute in te stellen
        var updateURL = typeof(PostData).GetMethod("UpdateURL", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        updateURL.Invoke(postData, null);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(go);
    }

    [Test]
    public void UpdateURL_SetsCorrectSetRoute()
    {
        var setRouteField = typeof(PostData).GetField("setRoute", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        string routeValue = (string)setRouteField.GetValue(postData);
        Assert.AreEqual("http://localhost:8080", routeValue);
    }

    [UnityTest]
    public IEnumerator SendData_StartsCoroutineAndLogs()
    {
        LogAssert.Expect(LogType.Log, "Verzonden data: {\"data\":\"testdata\"}");
        LogAssert.Expect(LogType.Warning, "http://localhost:8080/api/testroute");
        // De success/failure log kunnen we hier niet makkelijk voorspellen, dus die negeren we

        postData.SendData("testdata", "/api/testroute");

        // Coroutine draait, yield return null om één frame te wachten (StartCoroutine werkt async)
        yield return null;
    }
}
