using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class GetDashboardMessage : MonoBehaviour
{
    public string route = "/get-message";
    public float checkMessageInSeconds = 1f;
    CreateDashboard ButtonDashboard;

    public OutputRecorder outputRecorder;

    void Start()
    {
        ButtonDashboard = FindObjectOfType<CreateDashboard>();
        UpdateURL();
        StartCoroutine(CheckForMessages());
    }

    IEnumerator CheckForMessages()
    {
        while (true)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(route))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string message = request.downloadHandler.text;
                    if (!string.IsNullOrEmpty(message) && message != "{\"message\":\"\"}")
                    {
                        Debug.Log("Dashboard message: " + message);
                        if (message == "Start stream")
                        {
                            Debug.Log("stream gestart");
                            EnableStream(true);
                        } 
                        else if (message == "Stop stream")
                        {
                            EnableStream(false);
                        }
                        else
                        {
                            if (message.Contains("data")) //Action buttons triggering when button gets pressed in dashboard
                            {
                                Debug.Log("Action event received: " + message);
                                Debug.Log("Stop thinking animation");
                                FindObjectOfType<ThinkingAnimationManager>()?.StopThinkingAnimation();
                                ButtonDashboard = FindObjectOfType<CreateDashboard>();
                                if (ButtonDashboard != null)
                                {
                                    ButtonDashboard.ProcessJsonResponse(message);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("Request error: " + request.error);
                }
            }

            yield return new WaitForSeconds(checkMessageInSeconds);
        }
    }

    public void EnableStream(bool on)
    {
        if (on) 
        {
            OutputRecorder outputrecorder = GetComponent<OutputRecorder>();
            outputrecorder.StartRecording();
            Debug.Log("ingameaudio gestart: " +  outputrecorder);
            //GetComponent<MicrophoneRecorder>().StartRecording();
            ImageUploader imageUploader = GetComponent<ImageUploader>();
            imageUploader.StartCapture();
            Debug.Log("Image uploader gestart: " + imageUploader);

        }
        else 
        {
            GetComponent<OutputRecorder>().StopRecording();
            Debug.Log("Recording stopped");
            //GetComponent<MicrophoneRecorder>().StopRecording();
            GetComponent<ImageUploader>().StopCapture();
        }
    }

    public void UpdateURL()
    {
        string url = this.GetComponent<NetworkingData>().serverURL;
        string port = this.GetComponent<NetworkingData>().serverPort;
        route = $"http://{url}:{port}{route}";
    }
}
