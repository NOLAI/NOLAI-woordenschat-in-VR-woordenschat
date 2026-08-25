using NUnit.Framework;
using UnityEngine;
using System.Reflection;

public class PostDataWhiteTests
{
    private GameObject go;
    private PostData postData;
    private NetworkingData mockNetData;

    [SetUp]
    public void Setup()
    {
        go = new GameObject("PostDataTestGO");

        // Voeg mock NetworkingData toe
        mockNetData = go.AddComponent<NetworkingData>();
        mockNetData.serverURL = "127.0.0.1";
        mockNetData.serverPort = "8080";

        postData = go.AddComponent<PostData>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(go);
    }

    [Test]
    public void UpdateURL_SetsCorrectRoute()
    {
        // Arrange: Roep private methode aan via reflectie
        var method = typeof(PostData).GetMethod("UpdateURL", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(postData, null);

        // Reflecteer private veld setRoute
        var routeField = typeof(PostData).GetField("setRoute", BindingFlags.NonPublic | BindingFlags.Instance);
        string actualRoute = (string)routeField.GetValue(postData);

        // Assert
        Assert.AreEqual("http://127.0.0.1:8080", actualRoute);
    }

    [Test]
    public void SendData_FormatsJsonCorrectly()
    {
        // Arrange
        string testData = "test123";
        PostDataModel model = new PostDataModel { data = testData };

        // Act
        string json = JsonUtility.ToJson(model);

        // Assert
        Assert.AreEqual("{\"data\":\"test123\"}", json);
    }
}
