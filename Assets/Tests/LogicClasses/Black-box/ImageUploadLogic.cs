using System.Collections.Generic;

public class ImageUploadLogic
{
    private readonly IFileSystem fileSystem;
    private readonly IImageUploader uploader;

    public Queue<string> UploadQueue { get; } = new Queue<string>();
    public int BatchSize { get; set; } = 5;
    public bool StopRequested { get; private set; } = false;

    public ImageUploadLogic(IFileSystem fileSystem, IImageUploader uploader)
    {
        this.fileSystem = fileSystem;
        this.uploader = uploader;
    }

    public void EnqueueImage(string path)
    {
        UploadQueue.Enqueue(path);
    }

    public void RequestStop()
    {
        StopRequested = true;
    }

    // Deze methode kan je unit testen, simuleert upload logica zonder coroutines:
    public List<List<string>> GetUploadBatches()
    {
        List<List<string>> batches = new List<List<string>>();

        while (UploadQueue.Count >= BatchSize)
        {
            var batch = new List<string>();
            for (int i = 0; i < BatchSize && UploadQueue.Count > 0; i++)
            {
                batch.Add(UploadQueue.Dequeue());
            }
            batches.Add(batch);
        }

        if (StopRequested && UploadQueue.Count > 0)
        {
            var remaining = new List<string>();
            while (UploadQueue.Count > 0)
                remaining.Add(UploadQueue.Dequeue());
            batches.Add(remaining);
        }

        return batches;
    }
}
