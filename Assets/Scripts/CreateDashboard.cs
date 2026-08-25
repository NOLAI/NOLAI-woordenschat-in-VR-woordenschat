/*
 * CreateDashboard - Creates and manages the question/answer dashboard UI
 *
 * This script handles backend communication for the dashboard:
 * - Sends user answers to the backend dashboard
 * - Receives question data from the server
 * - Manages creation of button groups and phases
 * - Handles scene transitions based on responses
 * - Serializes custom data for API communication
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

[System.Serializable]
public class AnswerOption
{
    public string text;
    public int id;

    public AnswerOption(string text, int id)
    {
        this.text = text;
        this.id = id;
    }
}

[System.Serializable]
public class CustomDataWithEvent
{
    public string text = "Button"; //The text to show inside the buttons in the dashboard
    public string group = "Group"; //Group the button together of the same phases
    public string Question = ""; //Optional, provide the question asked which the child has provided an answer for
    //public Color color; //Define a color for the button
    public UnityEvent OnButtonClick;
    [HideInInspector]
    public int id = -1; //Each button needs a unique ID to connct to the Unity events. THis gets randomised for each button, and each session
    public List<AnswerOption> answerOptions = new List<AnswerOption>();

    // Add a method to convert Color to a simple object for JSON serialization
    //[System.Serializable]
    //public class ColorData
    //{
    //    public float r;
    //    public float g;
    //    public float b;
    //    public float a;

    //    public ColorData(Color color)
    //    {
    //        r = color.r;
    //        g = color.g;
    //        b = color.b;
    //        a = color.a;
    //    }
    //}


    // Create a Serializable class to store the data
    [System.Serializable]
    public class CustomDataJson
    {
        public string text;
        public string group;
        public int id;
        //public ColorData color;
        public string Question;
        public List <AnswerOption> answerOptions;

        public CustomDataJson(string text, string group, int id, string question, List<AnswerOption> answerOptions  )
        {
            this.text = text;
            this.group = group;
            this.id = id;
            //this.color = color;
            this.Question = question;
            this.answerOptions = answerOptions;
        }
    }


    // Method to convert the class to JSON
    public string ToJson()
    {
        // Convert color to a serializable object
        //ColorData colorData = new ColorData(color);
        CustomDataJson jsonData = new CustomDataJson(text, group, id, Question, answerOptions);

        // Return the serialized JSON string of the CustomDataJson object
        return JsonUtility.ToJson(jsonData);
    }
}

public class CreateDashboard : MonoBehaviour
{
    private int LastGeneratedID = 0;
    private string url = "http://url:8080/trigger-buttons";
    string RandomDigit;
    [Tooltip("A random string to use to identify this session on the web server. If empty, a random one is generated.")]
    public string SessionId;
    //public string currentElementId = -1;
    [SerializeField]
    private List<CustomDataWithEvent> dataList = new List<CustomDataWithEvent>();

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

        dataList.ForEach(data =>
        {
            if (data.id == 0)
            {
                data.id = GenerateRandomID();
            }
        });

        GenerateSceneButtons(); // Auto-create buttons for scene switching

        var NetworkManager = FindObjectOfType<NetworkingData>();
        if (NetworkManager != null)
        {
            url = "http://" + NetworkManager.serverURL + ":" + NetworkManager.serverPort + "/trigger-buttons";
            Debug.Log("Using local server URL: " + url);
            SendDataAsyncLocal(); // Start sending the data to the local server
        }
        else
        {
            url = "https://unity.vrlearninglab.nl/connect/trigger-buttons.php";
            Debug.Log("Using remote server URL: " + url);
            InvokeRepeating("FetchWebEvents", 1f, 1f);
            SendDataAsync(); // Start sending the request to external VRLL server
        }


    }

    // Zoek antwoordopties voor een vraag
    private List<AnswerOption> FindAnswerOptions(string questionText, int questionId)
    {
        List<AnswerOption> options = new List<AnswerOption>();

        // Gebruik regex om het nummer van de vraag te vinden
        var match = Regex.Match(questionText, @"^(\d+)\)");
        if (!match.Success)
            return options;

        string number = match.Groups[1].Value;
        string pattern = @"^" + number + @"[a-z]?\)";

        foreach (var data in dataList)
        {
            if (data.id == questionId)
                continue; // Skip de vraag zelf

            if (Regex.IsMatch(data.text, pattern))
            {
                options.Add(new AnswerOption(data.text, data.id));
            }
        }

        return options;
    }

    // Haal vraag en antwoordopties op voor een element ID
    public (string question, List<AnswerOption> answerOptions) GetQuestionAndAnswerOptionsForElement(int elementId)
    {
        foreach (var data in dataList)
        {
            if (data.id == elementId)
            {
                // Als de antwoordopties nog niet zijn gezet, zoek ze dan
                if (data.answerOptions.Count == 0 && !string.IsNullOrEmpty(data.Question))
                {
                    data.answerOptions = FindAnswerOptions(data.text, data.id);
                }
                Debug.Log("antwoord opties: " + data.answerOptions);
                return (data.Question, data.answerOptions);
            }
        }
        return (null, new List<AnswerOption>());
    }

    // Generates buttons for each scene in the build settings and adds them to the data list. This makes it easier for users to switch between scenes
    private void GenerateSceneButtons()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            CustomDataWithEvent sceneButton = new CustomDataWithEvent
            {
                text = sceneName,
                group = "Scenes",
                id = GenerateRandomID(),
                //color = Color.green
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
        return LastGeneratedID;
    }


    // Send data as a POST request to the server
    private async void SendDataAsync()
    {
        using (HttpClient client = new HttpClient())
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
            //Debug.Log(jsonContent);  // Useful to see the full json request which is send to the server

            StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // VRLL server running the PHP script to dynamically create th dashboard with its buttons
            string url = "https://unity.vrlearninglab.nl/connect/index.php";

            try
            {
                HttpResponseMessage response = await client.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    Debug.Log("Find your control panel at https://unity.vrlearninglab.nl/connect?session=" + SessionId);
#if UNITY_EDITOR
                    Application.OpenURL("https://unity.vrlearninglab.nl/connect?session=" + SessionId); //Only open the web page if running within the editor
#endif
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

    private async void SendDataAsyncLocal()
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
    void FetchWebEvents()
    {
        StartCoroutine(SendWebRequest());
    }

    // Web request to actually fetch the events to be fired from the server
    private IEnumerator SendWebRequest()
    {
        string url = "https://unity.vrlearninglab.nl/connect/UnityEventTriggerDB.php";
        // Create a UnityWebRequest (this is a GET request to avoid nasty ssh errors)
        UnityWebRequest request = UnityWebRequest.Get(url);

        // Send the request and wait for the response
        yield return request.SendWebRequest();

        // Check if there's an error
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error retrieving Unity Connect Fire Requests: " + request.error); // Log error if the request failed
        }
        else
        {
            // If successful, parse the JSON response
            string jsonResponse = request.downloadHandler.text;
            //Debug.Log("Response received: " + jsonResponse);

            // Process the received JSON
            ProcessJsonResponse(jsonResponse);
        }
    }

    public void ProcessJsonResponse(string json)
    {
        try
        {
            //Debug.Log("Processing JSON response: " + json);
            // Deserialize the JSON into the wrapper class
            EventListWrapper response = JsonUtility.FromJson<EventListWrapper>(json);

            // Access the events inside the 'data' array
            List<EventData> events = response.data;

            // Iterate over each event and print the 'id'
            foreach (var ev in events)
            {
                foreach (var data in dataList)
                {
                    if (ev.id == data.id.ToString())
                    {
                        try
                        {
                            Debug.Log("Firing event(s) connected to datalist " + data.text + " (ID:" + data.id + ")");
                            data.OnButtonClick?.Invoke();
                            if (!string.IsNullOrWhiteSpace(data.Question))
                            {
                                // We also have the question data here; save or handle it for later (e.g., AI grading).
                                Debug.Log($"Received question for ID {data.id}: {data.Question}");
                                var recorder = FindObjectOfType<MicrophoneRecorder>();
                                if (recorder != null)
                                {
                                    recorder.AskedQuestion = data.Question; //Set the question asked
                                }
                                else
                                {
                                    Debug.LogWarning("MicrophoneRecorder not found in scene. Cannot set AskedQuestion.");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Error triggering event for data ID {data.id}: {e.Message}");
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing JSON response: {e.Message}");
        }
    }

    [System.Serializable]
    public class EventData
    {
        public string id;
        public string time;
        public string session;
        public string question;
    }

    [System.Serializable]
    public class EventListWrapper
    {
        public string status;
        public List<EventData> data;
    }
}
