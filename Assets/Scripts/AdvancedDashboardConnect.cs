/*
 * AdvancedDashboardConnect - Connects to backend dashboard API for user interactions
 *
 * This script handles communication with the Advanced Dashboard backend to manage:
 * - Question/answer interactions via HTTP requests
 * - Group-based button organization
 * - Event-driven button click handling
 * - Scene loading based on dashboard responses
 * - Custom data serialization for dashboard communication
 */

using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

[System.Serializable]
public class AdvancedCustomDataWithEvent

{
    public string text = "Button";    
    public string group = "Group";   
    public Color color;
    public UnityEvent OnButtonClick;
    [HideInInspector]
    public int id = -1;

    // Add a method to convert Color to a simple object for JSON serialization
    [System.Serializable]
    public class ColorData
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public ColorData(Color color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }
    }

    // Create a Serializable class to store the data
    [System.Serializable]
    public class AdvancedCustomDataJson
    {
        public string text;
        public string group;
        public int id;
        public ColorData color;

        public AdvancedCustomDataJson(string text, string group, int id, ColorData color)
        {
            this.text = text;
            this.group = group;
            this.id = id;
            this.color = color;
        }
    }
    

    // Method to convert the class to JSON
    public string ToJson()
    {
        // Convert color to a serializable object
        ColorData colorData = new ColorData(color);
        AdvancedCustomDataJson jsonData = new AdvancedCustomDataJson(text, group, id, colorData);

        // Return the serialized JSON string of the AdvancedCustomDataJson object
        return JsonUtility.ToJson(jsonData);
    }
}

public class AdvancedDashboardConnect : MonoBehaviour
{
    // VRLL server running the PHP script to dynamically create th dashboard with its buttons
    public string url = "http://192.168.178.43:8080/trigger-buttons"; // Ensure this is an absolute URI
    private int LastGeneratedID = 0;
    string RandomDigit;
    [Tooltip("A random string to use to identify this session on the web server. If empty, a random one is generated.")]
    private string SessionId;
    [SerializeField]
    private List<AdvancedCustomDataWithEvent> dataList = new List<AdvancedCustomDataWithEvent>();

    // Method to trigger all events (useful to trigger from the inspector)
    public void TriggerAllEvents()
    {
        foreach (var data in dataList)
        {
            data.OnButtonClick?.Invoke();
        }
    }

    // Start is for initializing the session ID and sending the data to the server to create the session on the webserver as a json object
    void Start()
{
    
    // Use the project name and current scene name as SessionID
    string projectName = Application.productName;
    string sceneName = SceneManager.GetActiveScene().name;
    SessionId = Uri.EscapeDataString($"{projectName}SIP{sceneName}");
    RandomDigit = SessionId;


    //Debug.Log($"Session ID set to: {SessionId}");

    dataList.ForEach(data => {
    if (data.id == 0) 
    {
        data.id = GenerateRandomID();
    }
});

    GenerateSceneButtons(); // Auto-create buttons for scene switching
    SendDataAsync();
}

    // Generates buttons for each scene in the build settings and adds them to the data list. This makes it easier for users to switch between scenes
     private void GenerateSceneButtons()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            AdvancedCustomDataWithEvent sceneButton = new AdvancedCustomDataWithEvent
            {
                text = $"Scene ({sceneName})",
                group = "Scenes",
                id = GenerateRandomID(),
                color = Color.green
            };

            sceneButton.OnButtonClick = new UnityEvent();
            int sceneIndex = i; // Capture index for lambda
            sceneButton.OnButtonClick.AddListener(() => LoadScene(sceneIndex));

            dataList.Add(sceneButton);
        }
    }

    //Used to load the clicked scene. Make sure the scenes you wanna use are in the build settings to make them also usable in the actual build
     private void LoadScene(int sceneIndex)
    {
        Debug.Log("Loading scene index: " + sceneIndex);
        SceneManager.LoadScene(sceneIndex);
    }

    // Generate an ID for each button to keep track of the actions to trigger when clicked in the web dashboard
    private int GenerateRandomID()
    {
        LastGeneratedID++;
        //Debug.Log("Generated ID: " + LastGeneratedID);
        return LastGeneratedID;
    }


    // Send data as a POST request to the server
    private async void SendDataAsync()
    {
        Debug.Log("Sending data to dashboard server " + url);
        using (HttpClient client = new HttpClient { BaseAddress = new Uri(url) })
        {
            // Prepare data list as a JSON array
            List<string> jsonDataList = new List<string>();
            
            // Create the JSON string with the identifying digit
            string RequestID = "{\"RequestID\": \"" + RandomDigit + "\"}";

            jsonDataList.Add(RequestID);
            foreach (var data in dataList)
            {
                jsonDataList.Add(data.ToJson());
            }

            // Serialize list of data to JSON
            string jsonContent = "[" + string.Join(",", jsonDataList) + "]";
            Debug.Log(jsonContent);  // Useful to see the full json request which is sent to the server

            StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync("", content);
                Debug.Log(response);
                if (response.IsSuccessStatusCode)
                {
                    Debug.Log("Sent action buttons to dashboard server");
                }
                else
                {
                    Debug.Log("Error sending data: " + response.StatusCode);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Exception: " + e.Message);
            }
        }
    }



    public void ProcessJsonResponse(string json)
    {
        Debug.Log("Processing JSON response: " + json);

        // Deserialize the JSON into the wrapper class
        EventListWrapper response = JsonUtility.FromJson<EventListWrapper>(json);

        if (response == null || response.data == null)
        {
            Debug.LogError("Failed to parse JSON or 'data' is null.");
            return;
        }

        // Access the events inside the 'data' array
        List<AdvancedEventData> events = response.data;

        // Iterate over each event and match the 'id' with dataList
        foreach (var ev in events)
        {
            foreach (var data in dataList)
            {
                if (ev.id == data.id.ToString())
                {
                    Debug.Log("Firing event(s) connected to datalist " + data.text + " (ID:" + data.id + ")");
                    data.OnButtonClick?.Invoke();
                }
            }
        }
    }

    [System.Serializable]
    public class AdvancedEventData
    {
        public string id;
        public string time;
        public string session;
    }

    [System.Serializable]
    public class EventListWrapper
    {
        public string status;
        public List<AdvancedEventData> data;
    }
}
