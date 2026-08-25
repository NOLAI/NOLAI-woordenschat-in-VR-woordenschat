/*
 * SceneLoader - Loads scenes programmatically
 *
 * This script provides a simple method to load scenes:
 * - Exposes a public LoadScene method
 * - Uses SceneManager to load by name
 * - Can be triggered from other scripts
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Import this namespace to use SceneManager

public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string SceneName)
    {
        // Use SceneManager to load the scene by name
        SceneManager.LoadScene(SceneName);
    }
}