using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudioOnGrab : MonoBehaviour
{
    public AudioClip AudioClipToPlay;
    public bool HasPlayed = false;

    // Method to get the selected audio clip
    public AudioClip GetSelectedAudioClip()
    {
        Debug.Log("Playing audio clip after pickup: " + AudioClipToPlay.name);
        return AudioClipToPlay;
    }
}
