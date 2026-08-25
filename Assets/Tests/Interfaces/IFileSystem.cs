using System.Collections;
using System.Collections.Generic;

public interface IFileSystem
{
    byte[] ReadAllBytes(string path);
    bool Exists(string path);
    void Delete(string path);
}

public interface IImageUploader
{
    IEnumerator UploadImageBatch(List<string> filePaths);
}
