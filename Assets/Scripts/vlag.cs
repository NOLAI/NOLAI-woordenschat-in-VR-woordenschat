/*
 * vlag - Manages flag bucket sequence activation
 *
 * This script handles the flag trigger sequence:
 * - Activates lamp and plays sound when verrekijker enters
 * - Sequentially activates buckets (bucket2 -> bucket3 -> bucket4)
 * - Auto-disables lamp after delay
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vlag : MonoBehaviour
{
    public GameObject lamp;
    public AudioSource soundEffect;
    public GameObject bucket2;
    public GameObject bucket3;
    public GameObject bucket4;
    public float delay = 2f;
    private bool bucket3Activated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "verrekijker")
        {
            lamp.SetActive(true);
            StartCoroutine(DisableLampAfterDelay());

            if (soundEffect != null)
            {
                soundEffect.Play();
            }
            if (!bucket3Activated)
            {
               
                    if (bucket2 != null && bucket3 != null)
                    {
                        bucket2.SetActive(false);
                        bucket3.SetActive(true);
                        bucket3Activated = true;
                    }

            }
            else if (bucket3Activated && bucket3 != null && bucket4 != null)
            {
                bucket3.SetActive(false);
                bucket4.SetActive(true);
            }
        }
    } 
    private IEnumerator DisableLampAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        lamp.SetActive(false);
    }
}