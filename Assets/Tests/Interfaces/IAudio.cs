using System.Collections;
using UnityEngine;

namespace MyProject.IAudio
{
    public interface IMicrophoneService
    {
        bool HasDevices { get; }
        string GetDeviceName();
        void Start(string deviceName, bool loop, int lengthSec, int sampleRate);
        void Stop(string deviceName);
        bool IsRecording(string deviceName);
        int GetPosition(string deviceName);
        AudioClip GetAudioClip();
    }

    public interface IUploader
    {
        void UploadAudio(byte[] audioData, string filename);
    }

    public interface IConfirmDataUpload
    {
        void ConfirmUpload(string kind);
    }
}