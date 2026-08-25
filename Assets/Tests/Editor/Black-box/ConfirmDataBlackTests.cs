using NUnit.Framework;

public class ConfirmDataBlackTests
{
    [Test]
    public void ConfirmUpload_SendsCorrectMessages()
    {
        var logic = new ConfirmDataUploadLogic();

        logic.ConfirmUpload("Images");
        Assert.AreEqual(1, logic.SentMessages.Count);
        Assert.AreEqual("Images uploaded", logic.SentMessages[0]);

        logic.ResetFlag();

        logic.ConfirmUpload("Microphone");
        logic.ResetFlag();

        logic.ConfirmUpload("In-game");
        logic.ResetFlag();

        // 3 bevestigingen: zou nu ook "Data opgeslagen" moeten posten
        Assert.AreEqual(4, logic.SentMessages.Count);
        Assert.AreEqual("Data opgeslagen", logic.SentMessages[3]);
    }

    [Test]
    public void ConfirmUpload_QueuesWhenBusy()
    {
        var logic = new ConfirmDataUploadLogic();

        logic.ConfirmUpload("Images"); // bevestiging 1, gaat direct
        logic.ConfirmUpload("Microphone"); // bevestiging 2, wordt in queue

        // Nog niet gereset
        Assert.AreEqual(1, logic.SentMessages.Count); // alleen eerste

        logic.ResetFlag(); // zou nu tweede moeten posten

        Assert.AreEqual(2, logic.SentMessages.Count);
        Assert.AreEqual("Microphone uploaded", logic.SentMessages[1]);
    }
}
