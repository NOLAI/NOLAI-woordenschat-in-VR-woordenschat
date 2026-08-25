using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class PostDataModel
{
    public string data;
}

public class PostData : MonoBehaviour, IPostData
{
    string setRoute;

    void Start()
    {
        UpdateURL();
    }

    public virtual void SendData(string data, string route)
    {
        StartCoroutine(SendDataToServer(data, route));
    }

    IEnumerator SendDataToServer(string data, string newRoute)
    {
        PostDataModel postData = new PostDataModel();
        postData.data = data;
        string jsonString = JsonUtility.ToJson(postData);
        Debug.Log("Verzonden data: " + jsonString);

        string route = $"{setRoute}{newRoute}";
        Debug.LogWarning(route);
        using (UnityWebRequest request = new UnityWebRequest(route, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Data succesvol verzonden naar Laravel: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Er is een fout opgetreden bij het versturen van de data: " + request.error);
            }
        }
    }

    void UpdateURL()
    {
        string url = this.GetComponent<NetworkingData>().serverURL;
        string port = this.GetComponent<NetworkingData>().serverPort;
        setRoute = $"http://{url}:{port}";
    }
}
