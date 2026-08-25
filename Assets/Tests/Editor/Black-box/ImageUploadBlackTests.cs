using NUnit.Framework;
using System.Collections.Generic;

public class ImageUploadBlackTests
{
    [Test]
    public void GetUploadBatches_BatchesCorrectly()
    {
        var logic = new ImageUploadLogic(null, null);
        logic.BatchSize = 3;

        logic.EnqueueImage("img1");
        logic.EnqueueImage("img2");
        logic.EnqueueImage("img3");
        logic.EnqueueImage("img4");
        logic.EnqueueImage("img5");
        logic.EnqueueImage("img6");
        logic.EnqueueImage("img7");

        // Nog geen stop requested, dus alleen volle batches (3 + 3)
        var batches = logic.GetUploadBatches();
        Assert.AreEqual(2, batches.Count);
        Assert.AreEqual(3, batches[0].Count);
        Assert.AreEqual(3, batches[1].Count);

        // Er is 1 over in de queue
        Assert.AreEqual(1, logic.UploadQueue.Count);

        // Nu stop requested, dus ook laatste batch meegeven
        logic.RequestStop();
        batches = logic.GetUploadBatches();
        Assert.AreEqual(1, batches.Count);
        Assert.AreEqual(1, batches[0].Count);
        Assert.AreEqual(0, logic.UploadQueue.Count);
    }
}
