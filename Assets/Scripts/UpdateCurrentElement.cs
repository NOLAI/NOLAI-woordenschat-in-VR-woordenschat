/*
 * UpdateCurrentElement - Sends current element/fase data to backend
 *
 * This script communicates with the backend server:
 * - Sends element ID and fase to /update-current-element endpoint so the admin knows what level the user is currently on
 * - Uses UnityWebRequest for POST requests
 * - Includes error handling for failed requests
 */

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class UpdateCurrentElement : MonoBehaviour
{
    public string route = "/update-current-element";
    private string url;

    void Start()
    {
        UpdateURL();
    }

    public void UpdateCurrentElementToBackend(int elementId, int fase)
    {
        StartCoroutine(PostCurrentElement(elementId, fase));
    }

    IEnumerator PostCurrentElement(int elementId, int fase)
    {
        // Haal de vraag op uit de CreateDashboard lijst
        //string question = FindObjectOfType<CreateDashboard>().GetQuestionForElement(elementId);

        WWWForm form = new WWWForm();
        form.AddField("element_id", elementId.ToString());
        form.AddField("fase", fase.ToString());
        //form.AddField("question", question ?? ""); 

        using (UnityWebRequest request = UnityWebRequest.Post(url + route, form))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error updating current element: " + request.error);
            }
            else
            {
                Debug.Log($"Current element updated to: {elementId} (Fase: {fase})");
            }
        }
    }


    public void UpdateURL()
    {
        string url = this.GetComponent<NetworkingData>().serverURL;
        string port = this.GetComponent<NetworkingData>().serverPort;
        route = $"http://{url}:{port}{route}";
    }
}
