using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;
using System;

public class ImageUploader : MonoBehaviour
{
    public string route = "/store-image-batch";
    [Range(1, 30)] public float FPS = 5f;
    public int imageWidth = 640;
    public int imageHeight = 360;

    float uploadInterval;
    float timeSinceLastUpload;
    Camera mainCamera;
    RenderTexture renderTexture;
    bool isCapturing = false;

    List<string> imagePaths = new List<string>(); // Bewaart bestandsnamen van beelden
    //public int batchSize = 100;
    public int batchSize = 10;
    string tempFolderPath;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject); // Behoud dit object tussen scenes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        UpdateURL();
        uploadInterval = 1f / FPS;
        //timeSinceLastUpload = 1f;
        timeSinceLastUpload = 0f;

        tempFolderPath = Path.Combine(Application.persistentDataPath, "TempImages");
        if (!Directory.Exists(tempFolderPath))
            Directory.CreateDirectory(tempFolderPath);

        FindMainCamera();
    }

    void Update()
    {
        if (!isCapturing || mainCamera == null) return;
        timeSinceLastUpload += Time.deltaTime;
        if (timeSinceLastUpload >= uploadInterval)
        {
            timeSinceLastUpload = 0f;
            StartCoroutine(CaptureImage());
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene switched: " + scene.name);
        FindMainCamera();
    }

    void FindMainCamera()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("Geen Main Camera gevonden in de scene.");
        }
        else
        {
            renderTexture = new RenderTexture(imageWidth, imageHeight, 24);
        }
    }

    //gecalld vanuit getdashboardmessage
    public void StartCapture()
    {
        if (!isCapturing)
        {
            isCapturing = true;
            Debug.Log("Image capturing started.");
        }
    }

    //public void StopCapture()
    //{
    //    //if (isCapturing)
    //    //{
    //    //    isCapturing = false;
    //    //    Debug.Log("Image capturing stopped. Uploading remaining images...");

    //    //    if (imagePaths.Count > 0)
    //    //    {
    //    //        StartCoroutine(UploadImageBatch(new List<string>(imagePaths)));
    //    //        imagePaths.Clear();
    //    //    }
    //    //}

    //    if (isCapturing)
    //    {
    //        isCapturing = false;
    //        Debug.Log("Image capturing stopped. Uploading remaining images...");

    //        // Upload in kleinere batches
    //        for (int i = 0; i < imagePaths.Count; i += batchSize)
    //        {
    //            List<string> batch = imagePaths.GetRange(i, Math.Min(batchSize, imagePaths.Count - i));
    //            StartCoroutine(UploadImageBatch(batch));
    //        }
    //        imagePaths.Clear();
    //    }
    //}

    public void StopCapture()
    {
        if (isCapturing)
        {
            isCapturing = false;
            Debug.Log($"Image capturing stopped. {imagePaths.Count} images in queue. Uploading...");

            StartCoroutine(UploadAllBatches());
        }
    }

    IEnumerator UploadAllBatches()
    {
        for (int i = 0; i < imagePaths.Count; i += batchSize)
        {
            List<string> batch = imagePaths.GetRange(i, Math.Min(batchSize, imagePaths.Count - i));
            Debug.Log($"Uploading batch {i / batchSize + 1} with {batch.Count} images.");
            yield return UploadImageBatch(batch);
        }
        imagePaths.Clear();
        Debug.Log("All batches uploaded.");
    }

    IEnumerator CaptureImage()
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("Geen camera gevonden om beeld te capturen.");
            yield break;
        }

        yield return new WaitForEndOfFrame();

        mainCamera.targetTexture = renderTexture;
        mainCamera.Render();

        AsyncGPUReadback.Request(renderTexture, 0, TextureFormat.RGB24, request =>
        {
            if (request.hasError)
            {
                Debug.LogError("GPU Readback failed");
                return;
            }

            Texture2D tex = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
            tex.LoadRawTextureData(request.GetData<byte>());
            tex.Apply();

            string filePath = Path.Combine(tempFolderPath, $"frame_{System.DateTime.Now.Ticks}.jpg");
            File.WriteAllBytes(filePath, tex.EncodeToJPG());
            imagePaths.Add(filePath);


            Destroy(tex); // Voorkom memory leaks
        });

        mainCamera.targetTexture = null;
    }

    IEnumerator UploadImageBatch(List<string> filePaths)
    {
        if (filePaths == null || filePaths.Count == 0)
        {
            Debug.LogWarning("Image batch is empty or null.");
            yield break;
        }

        WWWForm form = new WWWForm();
        for (int i = 0; i < filePaths.Count; i++)
        {
            byte[] imageData = File.ReadAllBytes(filePaths[i]);
            form.AddBinaryData($"image[{i}]", imageData, Path.GetFileName(filePaths[i]), "image/jpeg");
        }

        using (UnityWebRequest www = UnityWebRequest.Post(route, form))
        {
            yield return www.SendWebRequest();
            //Debug.Log("Response code: " + www.responseCode);
            //Debug.Log("Response text: " + www.downloadHandler.text);

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Upload fout: " + www.error);
            }
            else
            {
                Debug.Log("Batch upload succesvol.");

                foreach (string filePath in filePaths)
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
            }
            GetComponent<ConfirmDataUpload>().ConfirmUpload();
        }
    }

    void UpdateURL()
    {
        string url = this.GetComponent<NetworkingData>().serverURL;
        string port = this.GetComponent<NetworkingData>().serverPort;
        route = $"http://{url}:{port}{route}";
    }

    void OnApplicationQuit()
    {
        if (isCapturing)
        {
            isCapturing = false;
            StopCapture();
        }

        // Verwijder overgebleven tijdelijke bestanden
        foreach (string filePath in imagePaths)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        if (Directory.Exists(tempFolderPath))
        {
            Directory.Delete(tempFolderPath, true);
        }

        if (renderTexture != null)
        {
            Destroy(renderTexture);
        }
    }
}