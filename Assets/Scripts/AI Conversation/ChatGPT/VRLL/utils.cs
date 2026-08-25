/*
 * utils.cs - AI Talk utility for OpenAI ChatGPT integration
 *
 * This namespace provides utilities for AI communication:
 * - Defines ChatMessage and ChatRequestBody structures
 * - Sends requests to OpenAI Chat Completions API
 * - Handles JSON serialization/deserialization
 */

using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Text;
using System;
using System.Collections;
using Unity.VisualScripting;

namespace VRLL.Utilities
{
    public static class AITalk
    {
        [System.Serializable]
        public class ChatMessage
        {
            public string role;
            public string content;
        }

        [System.Serializable]
        public class ChatRequestBody
        {
            public string model;
            public List<ChatMessage> messages = new List<ChatMessage>();
        }

        public static IEnumerator GetAnswer(string question, Action<string> callback)
        {
            if (question == "")
            {
                Debug.LogError("You forgot to ask your question. (Question is empty)");
                yield return "e1";
            }
            else
            {

                string apiKey = "sk-proj-VvmUvCG28BrAhFkaf9RwT3BlbkFJIE4NDdPPtE0T7iBZxhev";
                string modelId = "gpt-3.5-turbo";
                string chatEndpoint = "https://api.openai.com/v1/chat/completions";

                // Fill all of the request's Body. We use this to give context to the AI
                ChatRequestBody requestBody = new ChatRequestBody();
                requestBody.model = modelId;
                ChatMessage message = new ChatMessage();
                message.role = "user";
                message.content = question;
                requestBody.messages.Add(message);

                // JSONify
                string jsonRequestBody = JsonUtility.ToJson(requestBody);

                // Do the actual request
                using (UnityWebRequest www = new UnityWebRequest(chatEndpoint, "POST"))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody);
                    www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    www.uploadHandler.contentType = "application/json";
                    www.downloadHandler = new DownloadHandlerBuffer();

                    // Headers (Needed to avoid 400 error)
                    www.SetRequestHeader("Authorization", "Bearer " + apiKey);

                    yield return www.SendWebRequest();

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError(www.error);
                        callback(null); // Error, so returning nothing
                    }
                    else
                    {
                        string jsonResponse = www.downloadHandler.text;

                        // Parse JSON manually
                        string content = ParseJsonResponse(jsonResponse);

                        callback(content); // AI Response
                    }
                }
            }
        }

        private static string ParseJsonResponse(string jsonResponse)
        {
            // Remove unwanted characters from the JSON string
            jsonResponse = jsonResponse.Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("\t", "");

            // Find the index of "content" key
            int contentIndex = jsonResponse.IndexOf("\"content\":");

            // If "content" key is found
            if (contentIndex != -1)
            {
                // Find the start index of the content value
                int startIndex = jsonResponse.IndexOf("\"", contentIndex + 10) + 1;

                // Find the end index of the content value
                int endIndex = jsonResponse.IndexOf("\"", startIndex);

                // Extract the content value
                string content = jsonResponse.Substring(startIndex, endIndex - startIndex);

                return content;
            }

            return null; // Return null if "content" key is not found
        }
    }
}
