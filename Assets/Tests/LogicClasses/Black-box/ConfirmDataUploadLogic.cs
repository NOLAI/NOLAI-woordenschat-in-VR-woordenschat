using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmDataUploadLogic
{
    private int confirmations = 0;
    private bool confirmBusy = false;
    private Queue<string> messageQueue = new Queue<string>();
    private List<string> sentMessages = new List<string>();

    public IReadOnlyList<string> SentMessages => sentMessages;

    public void ConfirmUpload(string uploadKind)
    {
        confirmations++;

        if (uploadKind == "Images")
            Response("Images uploaded");
        else if (uploadKind == "Microphone")
            Response("Microphone uploaded");
        else if (uploadKind == "In-game")
            Response("In-game uploaded");

        if (confirmations >= 3)
        {
            confirmations = 0;
            Response("Data opgeslagen");
        }
    }

    private void Response(string message)
    {
        if (confirmBusy)
        {
            messageQueue.Enqueue(message);
        }
        else
        {
            Post(message);
        }
    }

    private void Post(string message)
    {
        confirmBusy = true;
        sentMessages.Add(message); // In plaats van postData.SendData
        // Simuleer delayed reset direct in test
    }

    public void ResetFlag()
    {
        confirmBusy = false;
        if (messageQueue.Count > 0)
        {
            string nextMessage = messageQueue.Dequeue();
            Post(nextMessage);
        }
    }
}