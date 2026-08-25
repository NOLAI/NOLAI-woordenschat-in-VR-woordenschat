/*
 * AI_Talk.cs - Example script for AI conversation interactions
 *
 * This script demonstrates how to use the AI Talk utility:
 * - Sends questions to AI via GetAnswer coroutine
 * - Handles responses with callback function
 * - Logs and processes AI responses
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRLL.Utilities;

public class UtilityTest : MonoBehaviour
{
    public string AIVraag;
    void Start()
    {
        // Call GetAnswer with a question and a callback function
        StartCoroutine(AITalk.GetAnswer(AIVraag, HandleResponse));
    }

    // Callback function to handle the response
    void HandleResponse(string response)
    {
        if (response != null)
        {
            Debug.Log(response);
            // Do something with the response here
        }
        else
        {
            Debug.LogError("Failed to get response");
            // Handle failure here
        }
    }
}
