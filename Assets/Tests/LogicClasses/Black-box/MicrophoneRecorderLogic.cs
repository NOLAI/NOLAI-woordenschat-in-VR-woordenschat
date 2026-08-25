using System.Collections;
using MyProject.IAudio;
using UnityEngine;

namespace MyProject.Logic
{
    public class MicrophoneRecorderLogic
    {
        private readonly IMicrophoneService _micService;
        private readonly IUploader _uploader;
        private readonly IConfirmDataUpload _confirm;

        private float _recordingStartTime;
        private bool _recordingStarted = false;

        public MicrophoneRecorderLogic(IMicrophoneService micService, IUploader uploader, IConfirmDataUpload confirm)
        {
            _micService = micService;
            _uploader = uploader;
            _confirm = confirm;
        }

        public void StartRecording(float startTime)
        {
            if (!_micService.HasDevices)
                return;

            string device = _micService.GetDeviceName();
            _micService.Start(device, false, 10, 16000);
            _recordingStartTime = startTime;
            _recordingStarted = true;
        }

        public string StopRecording(float stopTime)
        {
            if (!_recordingStarted)
                return "No recording started";

            float duration = stopTime - _recordingStartTime;
            if (duration < 1f)
            {
                _micService.Stop(_micService.GetDeviceName());
                _recordingStarted = false;
                return "Recording too short";
            }

            int position = _micService.GetPosition(_micService.GetDeviceName());
            if (position == 0)
            {
                _micService.Stop(_micService.GetDeviceName());
                _recordingStarted = false;
                return "No audio recorded";
            }

            _micService.Stop(_micService.GetDeviceName());
            _recordingStarted = false;

            // Simuleer upload - hier gebruik je Coroutine in echte code; in test kan je dit negeren
            var dummyAudio = new byte[1000]; 
            _uploader.UploadAudio(dummyAudio, "microphone_audio.wav");
            _confirm.ConfirmUpload("Microphone");

            return "Success";
        }
    }
}
