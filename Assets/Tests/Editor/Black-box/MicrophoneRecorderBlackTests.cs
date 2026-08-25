using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyProject.IAudio;
using MyProject.Logic;

public class MicrophoneRecorderBlackTests
{
    class FakeMicrophoneService : IMicrophoneService
    {
        public bool HasDevices => true;
        public string GetDeviceName() => "FakeMic";

        private bool _isRecording = false;
        public bool IsRecording(string deviceName) => _isRecording;

        public void Start(string deviceName, bool loop, int lengthSec, int sampleRate)
        {
            _isRecording = true;
        }

        public void Stop(string deviceName)
        {
            _isRecording = false;
        }

        public virtual int GetPosition(string deviceName) => 1000;

        public AudioClip GetAudioClip() => AudioClip.Create("TestClip", 1000, 1, 16000, false);
    }

    class FakeMicrophoneServiceOverridePosition : FakeMicrophoneService
    {
        public override int GetPosition(string deviceName) => 0;
    }

    class FakeUploader : IUploader
    {
        public List<byte[]> UploadedData = new List<byte[]>();
        public List<string> Filenames = new List<string>();

        public void UploadAudio(byte[] audioData, string filename)
        {
            UploadedData.Add(audioData);
            Filenames.Add(filename);
        }
    }

    class FakeConfirmDataUpload : IConfirmDataUpload
    {
        public int ConfirmCount = 0;
        public string LastKind = null;

        public void ConfirmUpload(string kind)
        {
            ConfirmCount++;
            LastKind = kind;
        }
    }

    [Test]
    public void StartStopRecording_Success()
    {
        var micService = new FakeMicrophoneService();
        var uploader = new FakeUploader();
        var confirm = new FakeConfirmDataUpload();

        var logic = new MicrophoneRecorderLogic(micService, uploader, confirm);

        logic.StartRecording(0f);
        Assert.IsTrue(micService.IsRecording("FakeMic"));

        string result = logic.StopRecording(2f);
        Assert.AreEqual("Success", result);

        Assert.AreEqual(1, confirm.ConfirmCount);
        Assert.AreEqual("Microphone", confirm.LastKind);
        Assert.AreEqual(1, uploader.Filenames.Count);
        Assert.AreEqual("microphone_audio.wav", uploader.Filenames[0]);
    }

    [Test]
    public void StopRecording_TooShort()
    {
        var micService = new FakeMicrophoneService();
        var uploader = new FakeUploader();
        var confirm = new FakeConfirmDataUpload();

        var logic = new MicrophoneRecorderLogic(micService, uploader, confirm);

        logic.StartRecording(0f);
        string result = logic.StopRecording(0.5f);
        Assert.AreEqual("Recording too short", result);
    }

    [Test]
    public void StopRecording_NoAudio()
    {
        var micService = new FakeMicrophoneServiceOverridePosition();
        var uploader = new FakeUploader();
        var confirm = new FakeConfirmDataUpload();

        var logic = new MicrophoneRecorderLogic(micService, uploader, confirm);

        logic.StartRecording(0f);

        string result = logic.StopRecording(2f);
        Assert.AreEqual("No audio recorded", result);
    }
}
