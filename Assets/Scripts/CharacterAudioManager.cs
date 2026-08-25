/*
 * CharacterAudioManager - Manages character animations based on audio playback
 *
 * This script synchronizes character animations with audio:
 * - Automatically switches to talk animation when audio is playing
 * - Returns to idle animation when audio stops
 * - Uses RMS threshold to detect silence vs. speech
 * - Crossfades between animations for smooth transitions
 */

using UnityEngine;

public class CharacterAudioManager : MonoBehaviour
{
    [Header("Animator and Audio Settings")]
    public Animator animator; // Reference to the Animator component

    [Header("Animation Names")]
    public string talkAnimationName = "TPF_talk1"; // Name of the talk animation
    public string idleAnimationName = "TPF_idle2"; // Name of the idle animation

    [Header("Audio Threshold Settings")]
    public float silenceThreshold = 0.0002f; // Threshold for considering audio as playing (based on RMS value)
    public float transitionDuration = 0.2f; // Duration of animation crossfade
    public AudioSource audioSource; // Reference to the AudioSource to check audio levels
    public bool PrintCurrentVolume = false;

    private string currentPlayingAnimation;

    private float lastUpdateTime = 0f; // Tracks the last time the audio was detected as playing
    private const float updateInterval = 0.75f; // Minimum time for talk animation to play (in seconds)

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("No Animator found on this GameObject!");
                return;
            }
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("No AudioSource found to monitor!");
                return;
            }
        }

        // Set the default animation state to idle
        currentPlayingAnimation = idleAnimationName;
        animator.Play(idleAnimationName);
    }

    private void Update()
    {
        // If missing components are detected, return early
        if (animator == null || audioSource == null) return;



        // Determine if audio is playing based on the audio level
        if (IsAudioPlaying())
        {
            // Transition to talk animation if not already playing
            if (currentPlayingAnimation != talkAnimationName)
            {
                animator.CrossFade(talkAnimationName, transitionDuration);
                currentPlayingAnimation = talkAnimationName;
            }

            // Update the last update time when audio is playing
            lastUpdateTime = Time.time;
        }
        else
        {
            
            // Check if enough time has passed since audio was last playing
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                // Transition to idle animation if not already playing
                if (currentPlayingAnimation != idleAnimationName)
                {
                    animator.CrossFade(idleAnimationName, transitionDuration);
                    currentPlayingAnimation = idleAnimationName;
                }
            }
        }
    }

    private bool IsAudioPlaying()
    {
        // Get the RMS (root mean square) value of the audio source to determine volume
        float[] samples = new float[256];
        audioSource.GetOutputData(samples, 0);
        

        float sum = 0f;
        foreach (float sample in samples)
        {
            sum += sample * sample; // Square the samples to get positive values
        }

        float rmsValue = Mathf.Sqrt(sum / samples.Length); // Calculate the RMS value (average power)
        if (PrintCurrentVolume) {
        Debug.Log("Current RMS value: " + rmsValue); // Optional: for debugging
        }

        // Return true if the RMS value is greater than the silence threshold
        return rmsValue > silenceThreshold;
    }
}
