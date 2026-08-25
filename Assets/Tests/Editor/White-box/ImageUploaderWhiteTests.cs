using NUnit.Framework;
using UnityEngine;
using System.Reflection;

public class ImageUploaderWhiteTests
{
    private GameObject testObject;
    private ImageUploader uploader;

    [SetUp]
    public void SetUp()
    {
        testObject = new GameObject();
        uploader = testObject.AddComponent<ImageUploader>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(testObject);
    }

    [Test]
    public void StartCapture_SetsIsCapturingAndClearsStopRequested()
    {
        uploader.StartCapture();

        Assert.IsTrue(GetPrivateField<bool>(uploader, "isCapturing"));
        Assert.IsFalse(GetPrivateField<bool>(uploader, "stopRequested"));
    }

    [Test]
    public void StopCapture_SetsStopRequestedAndIsCapturingFalse()
    {
        uploader.StartCapture(); // Zorg dat capturing eerst aan staat
        uploader.StopCapture();

        Assert.IsFalse(GetPrivateField<bool>(uploader, "isCapturing"));
        Assert.IsTrue(GetPrivateField<bool>(uploader, "stopRequested"));
    }

    [Test]
    public void UpdateURL_ConstructsCorrectRoute()
    {
        var networkingData = testObject.AddComponent<NetworkingData>();
        networkingData.serverURL = "localhost";
        networkingData.serverPort = "8080";

        // Roep de private methode correct aan via reflection
        var method = typeof(ImageUploader).GetMethod("UpdateURL", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(uploader, null);

        Assert.AreEqual("http://localhost:8080/store-images", uploader.route);
    }

    // Hulpmethode om private fields op te halen
    private T GetPrivateField<T>(object obj, string fieldName)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        return (T)field.GetValue(obj);
    }
}
