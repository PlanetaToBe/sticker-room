using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using UnityEngine;

namespace OoniCaptureCam
{
    public enum CaptureType
    {
        Video,
        Audio,
        VideoAndAudio
    }

    public enum CaptureOptimizationType
    {
        SmoothFramerate,
        AudioSync
    }

    public class CaptureCam : MonoBehaviour
    {
        public static string ffmpegPath;
        public static bool debugMode;

        [Header("System")]
        public bool _debugMode;
        public bool enableKeyControls = true;
        public bool enableViveControls = false;

        [Header("General")]
        public Camera sourceCamera;
        public GameObject visibleCamera;
        public Material screenMaterial;
        public CaptureType defaultMediaType;
        [Tooltip("Smooth framerate ensures that the video looks smooth, AudioSync may stutter but ensures video and audio lengths will stay in sync.")]
        public CaptureOptimizationType captureOptimizationType;

        [Header("Follow settings")]
        public GameObject target;
        [Range(0, 1)]
        public float followSpeed = 0.1f;

        [Header("Capture settings")]
        [Tooltip("Headset resolution while capture camera is rendering. Using a lower resolution helps offset capture performance costs.")]
        public float headsetResolutionAdjustment = 0.5f;
        public string filePrefix = "Drops";
        public string exportFolder = "Export";

        [Header("Video export settings")]
        public int videoWidth = 1280;
        public int videoHeight = 800;
        [Range(1, 30)]
        public int targetFramerate = 30;

        public Action ShowCameraAction;
        public Action HideCameraAction;
        public Action StartCaptureAction;
        public Action FinishCaptureAction;
        public Action<string> StartEncodingAction;
        public Action<string> FinishEncodingAction;
        public Action ReadyToRecordAction;

        private CaptureCamScreenGrabber screenGrabber;
        private CaptureCamAudioGrabber audioGrabber;

        [HideInInspector]
        public bool isCapturing;
        [HideInInspector]
        public bool isEncoding;
        [HideInInspector]
        public bool isVisible;

        private CaptureType captureType;
        private string tempVideoPath;
        private string tempAudioPath;

        private string captureDataPath;
        private string userDataPath;
        private string exportDataPath;

        private CaptureCamFFmpegEncoder pipe;

        private void Awake()
        {
            debugMode = _debugMode;
            ffmpegPath = Application.streamingAssetsPath + "/CaptureCam/ffmpeg";
        }

        void Start()
        {
            screenGrabber = sourceCamera.gameObject.AddComponent<CaptureCamScreenGrabber>();
            screenGrabber.sourceCamera = sourceCamera;
            screenGrabber.CaptureFinishedAction += HandleVideoCaptureFinished;
            screenGrabber.targetFramerate = targetFramerate;
            screenGrabber.width = videoWidth;
            screenGrabber.height = videoHeight;
            screenGrabber.captureOptimizationType = captureOptimizationType;

            audioGrabber = gameObject.AddComponent<CaptureCamAudioGrabber>();
            audioGrabber.CaptureFinishedAction += HandleAudioCaptureFinished;

            userDataPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), Application.productName);
            exportDataPath = Path.Combine(userDataPath, exportFolder);
            captureDataPath = Application.persistentDataPath;

            MkDir(userDataPath);
            MkDir(exportDataPath);

            if (enableKeyControls)
            {
                gameObject.AddComponent<CaptureCamKeyControls>();
            }

            if (enableViveControls)
            {
                gameObject.AddComponent<CaptureCamViveControls>();
            }
        }

        void Update()
        {
            if (isVisible) SetPosition();
        }

        void SetPosition()
        {
            SetPosition(false);
        }

        void SetPosition(bool immediate)
        {
            if (target == null) return;
            float lerpAmt = immediate ? 1 : followSpeed;
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, target.transform.position, lerpAmt);
            gameObject.transform.rotation = Quaternion.Lerp(gameObject.transform.rotation, target.transform.rotation, lerpAmt);
        }

        public void ShowCamera()
        {
            isVisible = true;
            SetPosition(true);

            screenMaterial.SetTexture("_MainTex", screenGrabber.renderTexture);
            screenGrabber.StartUpdatingTexture();
            audioGrabber.ActivateListener();

            visibleCamera.SetActive(true);

            if (ShowCameraAction != null)
            {
                ShowCameraAction();
            }
        }

        public void HideCamera()
        {
            if (isCapturing)
            {
                FinishCapture();
            }

            isVisible = false;
            visibleCamera.SetActive(false);
            screenGrabber.StopUpdatingTexture();
            audioGrabber.DeactivateListener();

            if (HideCameraAction != null)
            {
                HideCameraAction();
            }
        }

        public void ToggleCapture()
        {
            if (isCapturing)
            {
                FinishCapture();
            }
            else
            {
                StartCapture();
            }
        }
   
        public void StartVideoAndAudioCapture()
        {
            StartCapture(CaptureType.Video);
        }

        public void StartVideoCapture()
        {
            StartCapture(CaptureType.Video);
        }

        public void StartAudioCapture()
        {
            StartCapture(CaptureType.Audio);
        }

        public void StartCapture()
        {
            StartCapture(defaultMediaType);
        }

        public void StartCapture(CaptureType _captureType)
        {
            if (isCapturing || isEncoding) return;

            Log("Starting capture (" + _captureType + ", " + captureOptimizationType + ")");

            if (!isVisible)
            {
                ShowCamera();
            }

            isCapturing = true;
            captureType = _captureType;
            tempVideoPath = null;
            tempAudioPath = null;

            if (captureType == CaptureType.VideoAndAudio || captureType == CaptureType.Video)
            {
                screenGrabber.StartCapturing(captureDataPath);
            }

            if (captureType == CaptureType.VideoAndAudio || captureType == CaptureType.Audio)
            {
                audioGrabber.StartCapturing(captureDataPath);
            }

            if (StartCaptureAction != null)
            {
                StartCaptureAction();
            }
        }

        public void FinishCapture()
        {
            isCapturing = false;
            isEncoding = true;

            if (captureType == CaptureType.VideoAndAudio || captureType == CaptureType.Video)
            {
                Log("Stopping video capture");
                screenGrabber.StopCapturing();
            }

            if (captureType == CaptureType.VideoAndAudio || captureType == CaptureType.Audio)
            {
                Log("Stopping audio capture");
                audioGrabber.StopCapturing();
            }

            if (FinishCaptureAction != null)
            {
                FinishCaptureAction();
            }
        }

        void HandleVideoCaptureFinished(string videoPath)
        {
            tempVideoPath = videoPath;
            MaybePerformFinalEncode();
        }

        void HandleAudioCaptureFinished(string audioPath)
        {
            tempAudioPath = audioPath;
            MaybePerformFinalEncode();
        }

        void MaybePerformFinalEncode()
        {
            bool hasVideo = tempVideoPath != null;
            bool hasAudio = tempAudioPath != null;
            bool shouldStart = false;

            string baseFileName = filePrefix + "_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string basePath = Path.Combine(exportDataPath, baseFileName);
            string finalPath = "";

            if (
                (captureType == CaptureType.VideoAndAudio && hasVideo && hasAudio) ||
                (captureType == CaptureType.Video && hasVideo)
            )
            {
                shouldStart = true;
                finalPath = basePath + ".mp4";
            }
            else if (captureType == CaptureType.Audio && hasAudio)
            {
                shouldStart = true;
                finalPath = basePath + ".mp3";
            }

            if (shouldStart) {
                Log("Beginning final encode to: " + finalPath);
                
                if (StartEncodingAction != null)
                {
                    StartEncodingAction(finalPath);
                }

                CaptureCamFFmpegMuxer muxer = new CaptureCamFFmpegMuxer(tempVideoPath, tempAudioPath, finalPath);
                muxer.FinishedAction += HandleFinalEncodeComplete;
            }
        }

        void HandleFinalEncodeComplete(string path)
        {
            Cleanup();
            Log("Final encode complete. File is at " + path);

            if (FinishEncodingAction != null)
            {
                FinishEncodingAction(path);
            }

            isEncoding = false;
        }

        void Cleanup()
        {
            if (tempVideoPath != null) File.Delete(tempVideoPath);
            if (tempAudioPath != null) File.Delete(tempAudioPath);

            tempVideoPath = null;
            tempAudioPath = null;
        }

        void MkDir(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }

        public static void Log(string message)
        {
            if (debugMode) UnityEngine.Debug.Log(message);
        }
    }
}