/*
 * StartWhisperAI.cs - Launches local WhisperAI Flask server
 *
 * This script starts the WhisperAI speech recognition:
 * - Spawns Python process to run Flask server
 * - Manages process lifecycle
 * - Detects when server has exited
 */

using System.Diagnostics;
using System.IO;
using UnityEngine;

public class StartWhisperAI : MonoBehaviour
{
    private Process process;

    private void Start()
    {
        //print("Process: " + process.Responding);
        StartPython();
    }

    //Starts the the local flask server with the speech recognition
    private void StartPython()
    {
        string path = Path.GetFullPath("..\\WhisperAI");
        print(path);
        string script = "main.py";

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "python.exe", // Assuming 'python.exe' is in the system PATH
            Arguments = Path.Combine(path, script),
            WorkingDirectory = path,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        process = new Process { StartInfo = startInfo };

        process.Start();

        print("Process: " + process.Responding);
    }

    private void Update()
    {
        //print("Process Update: " + process.Responding);
        if (process != null && process.HasExited)
        {
            OnApplicationQuit();
        }
    }

    private void OnApplicationQuit()
    {
        if (process != null && !process.HasExited)
        {
            // Stop the process if it's still running
            process.Kill();
            process.WaitForExit(); // Wait for the process to finish cleaning up resources
        }
    }
}