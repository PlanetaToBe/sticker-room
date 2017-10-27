using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using UnityEngine.UI;

namespace OoniCaptureCam
{
    public class CaptureCamScreenGrabber : MonoBehaviour
    {
        public Camera sourceCamera;

        public int width;
        public int height;
        [Range(1, 60)]
        public int targetFramerate = 30;
        public float renderScaleWhenActive = .5f;
        public CaptureOptimizationType captureOptimizationType;

        public Action<string> CaptureStartedAction;
        public Action<string> CaptureFinishedAction;

        // Whether to copy camera view to render texture
        private bool isCopying = false;
        // Whether to save render texture frames to disk
        private bool isRecording = false;

        private Rect renderRect;
        [HideInInspector]
        public RenderTexture renderTexture;
        private Texture2D texture2D;

        private CaptureCamFFmpegEncoder encoder;
        private IEnumerator renderLoop;
        private float initialRenderScale;

        private float captureStartTime;
        private int capturedFrames;
        private float lastFrameTime;

        void Start()
        {
            texture2D = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture2D.hideFlags = HideFlags.HideAndDontSave;
            texture2D.wrapMode = TextureWrapMode.Clamp;
            texture2D.filterMode = FilterMode.Point;
            texture2D.anisoLevel = 0;

            renderTexture = new RenderTexture(width, height, 24);

            renderRect = new Rect(0, 0, width, height);
            initialRenderScale = UnityEngine.XR.XRSettings.eyeTextureResolutionScale;
            renderLoop = RenderLoop();

#if UNITY_2017_1_OR_NEWER
		    Application.onBeforeRender += OnBeforeAppRender;
#else
            Camera.onPreCull += OnPreCull;
#endif
        }

        public void StartUpdatingTexture()
        {
            if (isCopying) return;
            UnityEngine.XR.XRSettings.eyeTextureResolutionScale = renderScaleWhenActive;
            isCopying = true;
        }

        public void StopUpdatingTexture()
        {
            if (!isCopying) return;
            UnityEngine.XR.XRSettings.eyeTextureResolutionScale = initialRenderScale;
            isCopying = false;
        }

        public void StartCapturing(string destinationFolder)
        {
            if (isRecording) return;
            StartUpdatingTexture();
            sourceCamera.Render();

            lastFrameTime = GetTime();
            captureStartTime = GetTime();
            capturedFrames = 0;

            string filename = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".mp4";
            string destinationPath = Path.Combine(destinationFolder, filename);
            encoder = new CaptureCamFFmpegEncoder(destinationPath, targetFramerate, width, height);
            Debug.Log("Created encoder " + encoder);
            encoder.FinishedAction += OnEncoderFinished;

            isRecording = true;
        }

        public void StopCapturing()
        {
            if (!isRecording) return;
            isRecording = false;
            encoder.Close();

            float elapsedTime = (lastFrameTime - captureStartTime);
            float idealFrames = elapsedTime * targetFramerate;

            CaptureCam.Log(capturedFrames + " frames captured in " + elapsedTime.ToString("0.00") + " seconds (Should have been " + idealFrames + ")");
        }

#if UNITY_2017_1_OR_NEWER
        private void OnBeforeAppRender()
        {
            UpdateRenderLoop();
        }
#else
        private void OnPreCull(Camera cam)
        {
            // Only update poses on the first camera per frame.
            if (Time.frameCount != lastFrameCount)
            {
                lastFrameCount = Time.frameCount;
                UpdateRenderLoop();
            }
        }
        static int lastFrameCount = -1;
#endif

        private void UpdateRenderLoop()
        {
            if (isCopying) renderLoop.MoveNext();
        }

        private IEnumerator RenderLoop()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();

                RenderTexture.active = renderTexture;
                texture2D.ReadPixels(renderRect, 0, 0);

                // Camera rendering will trigger onRenderImage function 
                // synchronously, blitting the render output to our render texture
                sourceCamera.Render();

                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();

                if (isRecording)
                {
                    // Note that we use Time.realtimeSinceStartup instead of Time.time.
                    // Using Time.time will produce smoother videos but causes issues with audio sync.
                    float elapsedTime = (GetTime() - captureStartTime);
                    float idealFrameCount = elapsedTime * targetFramerate;
                    int framesToCapture = (int)idealFrameCount - capturedFrames;

                    capturedFrames += framesToCapture;

                    byte[] data = texture2D.GetRawTextureData();

                    if (captureOptimizationType == CaptureOptimizationType.SmoothFramerate)
                    {
                        framesToCapture = 1;
                    }

                    encoder.SendFrame(data, framesToCapture);
                }

                lastFrameTime = GetTime();
            }
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, renderTexture);
        }

        void OnEncoderFinished(string path)
        {
            CaptureCam.Log("Video encode finished");
            encoder.FinishedAction -= OnEncoderFinished;
            encoder = null;

            if (CaptureFinishedAction != null)
            {
                CaptureFinishedAction(path);
            }
        }

        float GetTime()
        {
            if (captureOptimizationType == CaptureOptimizationType.AudioSync)
            {
                return Time.realtimeSinceStartup;
            } else
            {
                return Time.time;
            }
        }
    }
}