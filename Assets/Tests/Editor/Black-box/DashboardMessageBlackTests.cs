using NUnit.Framework;
using System.Collections.Generic;

public class DashboardMessageBlackTests
{
    private TestStreamController testStream;
    private TestButtonDashboard testDashboard;
    private DashboardMessageProcessor processor;

    [SetUp]
    public void Setup()
    {
        testStream = new TestStreamController();
        testDashboard = new TestButtonDashboard();
        processor = new DashboardMessageProcessor(testStream, testDashboard);
    }

    [Test]
    public void Message_StartStream_EnablesStream()
    {
        processor.HandleMessage("Start stream");
        Assert.IsTrue(testStream.LastState);
    }

    [Test]
    public void Message_StopStream_DisablesStream()
    {
        processor.HandleMessage("Stop stream");
        Assert.IsFalse(testStream.LastState);
    }

    [Test]
    public void Message_Data_CallsButtonDashboard()
    {
        string message = "{\"data\":\"some action\"}";
        processor.HandleMessage(message);
        
        Assert.AreEqual(1, testDashboard.ReceivedMessages.Count);
        Assert.AreEqual(message, testDashboard.ReceivedMessages[0]);
        Assert.AreEqual(1, processor.ReceivedActions.Count);
    }

    [Test]
    public void Message_Empty_DoesNothing()
    {
        processor.HandleMessage("");
        processor.HandleMessage(null);
        processor.HandleMessage("{\"message\":\"\"}");

        Assert.IsNull(testStream.LastState); // Never called
        Assert.AreEqual(0, testDashboard.ReceivedMessages.Count);
    }
}

// Test stubs (fakes)
public class TestStreamController : IStreamController
{
    public bool? LastState = null;

    public void EnableStream(bool on)
    {
        LastState = on;
    }
}

public class TestButtonDashboard : IButtonDashboard
{
    public List<string> ReceivedMessages = new List<string>();

    public void ProcessJsonResponse(string message)
    {
        ReceivedMessages.Add(message);
    }
}
