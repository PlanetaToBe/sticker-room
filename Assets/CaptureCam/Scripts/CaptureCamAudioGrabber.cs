using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OoniCaptureCam
{
    public class CaptureCamAudioGrabber : MonoBehaviour
    {

        public System.Action<string> CaptureStartedAction;
        public System.Action<string> CaptureFinishedAction;

        private int sampleRate;

        private bool isRecording = false;
        private CaptureCamWavEncoder encoder;

        private AudioListener listener;
        private AudioListener sceneListener;

        private float lastTime;

        void Start()
        {
            sampleRate = AudioSettings.outputSampleRate;
            sceneListener = FindObjectOfType<AudioListener>();
            listener = gameObject.AddComponent<AudioListener>();
            listener.enabled = false;
        }

        public void StartCapturing(string destinationFolder)
        {
            if (isRecording) return;
            ActivateListener();

            string filename = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".wav";
            string destinationPath = Path.Combine(destinationFolder, filename);

            CaptureCam.Log("Starting audio encode to " + destinationPath);

            encoder = new CaptureCamWavEncoder(destinationPath, sampleRate);
            encoder.FinishedAction += HandleEncodingComplete;
            isRecording = true;

            if (CaptureStartedAction != null)
            {
                CaptureStartedAction(destinationPath);
            }
        }

        public void StopCapturing()
        {
            if (!isRecording) return;
            DeactivateListener();

            isRecording = false;
            encoder.Close();
        }

        public void ActivateListener()
        {
            sceneListener.enabled = false;
            listener.enabled = true;
        }

        public void DeactivateListener()
        {
            listener.enabled = false;
            sceneListener.enabled = true;
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (isRecording) encoder.Write(data, channels);
        }

        void HandleEncodingComplete(string filePath)
        {
            CaptureCam.Log("Audio encode finished");

            encoder.FinishedAction -= HandleEncodingComplete;
            encoder = null;

            if (CaptureFinishedAction != null)
            {
                CaptureFinishedAction(filePath);
            }
        }
    }
}
