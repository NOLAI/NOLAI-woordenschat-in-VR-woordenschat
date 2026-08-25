using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;

public class TeleportPlayer : MonoBehaviour
{

    //Use this script to teleport the player to the indicated position
    public Transform teleportPosition;
    public GameObject player;
    public UnityEvent OnTeleport;
    public AudioClip TeleportAudio;

    void OnEnable()
    {
        if (player == null) {
        player = GameObject.Find("XR Interaction Hands Setup");
        }
    }

    public void TriggerTeleport()
    {
        float distance = Vector3.Distance(player.transform.position, teleportPosition.position);
        if (distance > 1.0f)
        {
            StartCoroutine(FadeAndTeleport());
        }
        else
        {
            Debug.Log("Player is within 1 units of the teleport position. Ignoring teleport request.");
        }
    }

    private IEnumerator FadeAndTeleport()
    {
        // Play teleport sound if assigned
        if (TeleportAudio != null)
        {
            AudioSource audioSource = player.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = player.AddComponent<AudioSource>();
            }
            audioSource.clip = TeleportAudio;
            audioSource.volume = 0.4f; // Set volume as needed
            audioSource.Play();

            // Wait for the duration of the audio before fading out
            yield return new WaitForSeconds(TeleportAudio.length * 1.0f); // Adjust timing as needed
        }

        // Fade to black
        yield return StartCoroutine(FadeScreen(true));

        // Teleport the player
        Debug.Log("Teleporting player to " + teleportPosition.position);
        player.transform.position = teleportPosition.position;
        player.transform.rotation = teleportPosition.rotation;

        // Trigger events during the black screen
        OnTeleport?.Invoke();

        // Fade back to normal
        yield return StartCoroutine(FadeScreen(false));
    }


    private IEnumerator FadeScreen(bool fadeToBlack)
    {

        float fadeDuration = 1.0f; // Duration of the fade
        float fadeSpeed = 1.0f / fadeDuration;
        float alpha = fadeToBlack ? 0 : 1;
        float targetAlpha = fadeToBlack ? 1 : 0;

        CanvasGroup canvasGroup = FindObjectOfType<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("No CanvasGroup found in the scene.");
            yield break;
        }

        while (!Mathf.Approximately(alpha, targetAlpha))
        {
            alpha = Mathf.MoveTowards(alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            canvasGroup.alpha = alpha;
            yield return null;
        }
    }
}
