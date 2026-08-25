using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class VRDebugConsole : MonoBehaviour
{
    public TextMeshProUGUI logText; // Verwijzing naar de TextMeshPro UI
    public ScrollRect scrollRect;   // Verwijzing naar de ScrollRect
    private Queue<string> logQueue = new Queue<string>();
    private const int maxLines = 20; // Max aantal regels

    void Awake()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        string message = $"[{type}] {logString}";
        logQueue.Enqueue(message);

        if (logQueue.Count > maxLines)
        {
            logQueue.Dequeue(); // Oudste verwijderen
        }

        logText.text = string.Join("\n", logQueue.ToArray());

        // Zorg dat de console automatisch naar beneden scrolt
        StartCoroutine(ScrollToBottom());
    }

    private System.Collections.IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame(); // Wacht tot UI is geüpdatet
        scrollRect.verticalNormalizedPosition = 0f; // Scrol helemaal naar beneden
    }
}
