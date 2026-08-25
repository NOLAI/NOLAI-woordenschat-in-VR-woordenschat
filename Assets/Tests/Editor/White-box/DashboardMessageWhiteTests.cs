using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class DashboardMessageWhiteTests
{
    private GameObject testObject;
    private GetDashboardMessage dashboardMessage;

    [SetUp]
    public void Setup()
    {
        testObject = new GameObject();
        dashboardMessage = testObject.AddComponent<GetDashboardMessage>();

        var networkingData = testObject.AddComponent<NetworkingData>();
        networkingData.serverURL = "localhost";
        networkingData.serverPort = "8080";
    }

    [Test]
    public void UpdateURL_SetsCorrectRoute()
    {
        // Act
        dashboardMessage.UpdateURL();

        // Assert
        Assert.AreEqual("http://localhost:8080/get-message", dashboardMessage.route);
    }
}
