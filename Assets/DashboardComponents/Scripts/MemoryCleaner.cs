using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryCleaner : MonoBehaviour
{
    public float cleanupIntervalInSeconds = 60f;

    void Start()
    {
        StartCoroutine(CleanupRoutine());
    }

    IEnumerator CleanupRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(cleanupIntervalInSeconds);
            CleanupMemory();
        }
    }

    void CleanupMemory()
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }
}
