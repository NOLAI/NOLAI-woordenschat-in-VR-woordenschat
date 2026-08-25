using UnityEngine;

public class AudioForwarder : MonoBehaviour
{
    void OnAudioFilterRead(float[] data, int channels)
    {
        if (OutputRecorder.Instance != null)
        {
            OutputRecorder.Instance.ReceiveAudioData(data);
        }
    }
}
