using UnityEngine;
using System.Net;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class MJPEGStreamer : MonoBehaviour
{
    HttpListener listener;
    bool isStreaming = false; // Streaming start niet automatisch
    string server;
    [Range(1, 60)] public int FPS = 10;

    public int streamWidth = 640;
    public int streamHeight = 360;

    Texture2D currentFrame;
    byte[] currentJPG;
    Queue<Action> mainThreadQueue = new Queue<Action>();

    float frameTime;
    Camera mainCamera;
    RenderTexture renderTexture;
    Thread streamThread;

    void Start()
    {
        frameTime = 1f / FPS;
        mainCamera = Camera.main;
        renderTexture = new RenderTexture(streamWidth, streamHeight, 24);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        lock (mainThreadQueue)
        {
            while (mainThreadQueue.Count > 0)
            {
                mainThreadQueue.Dequeue()?.Invoke();
            }
        }

        if (isStreaming && Time.time % frameTime < Time.deltaTime)
        {
            StartCoroutine(CaptureScreenshot());
        }
    }

    void StartStream()
    {
        if (isStreaming) return;

        try
        {
            listener = new HttpListener();
            listener.Prefixes.Add(server);
            listener.Start();
            Debug.Log("MJPEG Server gestart op " + server);
            isStreaming = true;

            streamThread = new Thread(() =>
            {
                while (isStreaming)
                {
                    listener.BeginGetContext(ProcessRequest, listener);
                    Thread.Sleep(10);
                }
            });
            streamThread.IsBackground = true;
            streamThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError("Kan server niet starten: " + e.Message);
        }
    }

    public void StopStream()
    {
        if (!isStreaming) return;

        isStreaming = false;

        if (listener != null)
        {
            listener.Stop();
            listener.Close();
            listener = null;
        }

        if (streamThread != null)
        {
            streamThread.Join();
            streamThread = null;
        }

        Debug.Log("MJPEG Server gestopt.");
    }

    IEnumerator CaptureScreenshot()
    {
        yield return new WaitForEndOfFrame();

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("Hoofdcamera niet gevonden.");
                yield break;
            }
        }

        renderTexture = new RenderTexture(streamWidth, streamHeight, 24);
        mainCamera.targetTexture = renderTexture;
        mainCamera.Render();

        AsyncGPUReadback.Request(renderTexture, 0, TextureFormat.RGB24, request =>
        {
            if (request.hasError)
            {
                Debug.LogError("GPU Readback failed");
                return;
            }

            Texture2D tex = new Texture2D(streamWidth, streamHeight, TextureFormat.RGB24, false);
            tex.LoadRawTextureData(request.GetData<byte>());
            tex.Apply();

            byte[] jpg = tex.EncodeToJPG();

            lock (this)
            {
                if (currentFrame != null)
                    Destroy(currentFrame);

                currentFrame = tex;
                currentJPG = jpg;
            }
        });

        mainCamera.targetTexture = null;
        RenderTexture.active = null;
        if (Application.isPlaying)
            Destroy(renderTexture);
        else
            DestroyImmediate(renderTexture);
    }

    void ProcessRequest(IAsyncResult result)
    {
        if (!isStreaming) return;

        try
        {
            var context = listener.EndGetContext(result);
            var response = context.Response;
            response.ContentType = "multipart/x-mixed-replace; boundary=frame";
            response.StatusCode = 200;

            using (var stream = response.OutputStream)
            {
                while (isStreaming)
                {
                    byte[] imageBytes;
                    lock (this)
                    {
                        if (currentJPG == null)
                        {
                            Thread.Sleep(1000 / FPS);
                            continue;
                        }
                        imageBytes = currentJPG;
                    }

                    string header = "--frame\r\nContent-Type: image/jpeg\r\nContent-Length: " + imageBytes.Length + "\r\n\r\n";
                    byte[] headerBytes = System.Text.Encoding.ASCII.GetBytes(header);

                    stream.Write(headerBytes, 0, headerBytes.Length);
                    stream.Write(imageBytes, 0, imageBytes.Length);
                    stream.Flush();
                    Thread.Sleep(1000 / FPS);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Fout in stream: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        StopStream();
        if (renderTexture != null)
        {
            if (Application.isPlaying)
                Destroy(renderTexture);
            else
                DestroyImmediate(renderTexture);
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public virtual void SetAdress(string ip, string port)
    {
        server = $"http://{ip}:{port}/stream/";
        StartStream();
    }
}