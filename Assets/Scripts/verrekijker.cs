/*
 * verrekijker - Manages binocular lamp activation
 *
 * This script handles the binocular (verrekijker) trigger:
 * - Activates lamp when binocular tag enters trigger
 * - Auto-disables lamp after delay
 * - Plays sound effect on activation
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class verrekijker : MonoBehaviour
{
    public GameObject lamp;
    public AudioSource soundEffect;
    public float delay = 2f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "verrekijker")
        {
            lamp.SetActive(true);
            StartCoroutine(DisableLampAfterDelay());
        }
        if (soundEffect != null)
        {
            soundEffect.Play();
        }
    }
    private IEnumerator DisableLampAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        lamp.SetActive(false);
    }
}