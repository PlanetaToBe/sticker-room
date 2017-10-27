// Based on https://github.com/keijiro/FFmpegOut/blob/7cde820d33ffc91ae6581a62b428d685093c00f4/Assets/FFmpegOut/FFmpegPipe.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System;

namespace OoniCaptureCam
{
    class CaptureCamFFmpegEncoder
    {
        public Action<string> FinishedAction;

        private Queue<byte[]> frameQueue;
        private bool processingQueue = true;
        private string destinationPath;

        Process ffmpegProcess;
        BinaryWriter ffmpegStdin;

        public CaptureCamFFmpegEncoder(string _destinationPath, int framerate, int width, int height)
        {
            destinationPath = _destinationPath;

            string opt =
                "-y -r " + framerate + " -f rawvideo -codec rawvideo -s " + width + "x" + height + " " + "-pixel_format rgba " +
                "-i pipe:0 -vf \"transpose = 3,transpose = 1\" -r " + framerate + " -threads 8 -c:v libx264 " +
                "-preset faster -crf 23 -pix_fmt yuv420p " + destinationPath;

            CaptureCam.Log("Running video encode command: " + CaptureCam.ffmpegPath + opt);

            var info = new ProcessStartInfo(CaptureCam.ffmpegPath, opt);
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.RedirectStandardInput = true;

            ffmpegProcess = Process.Start(info);
            ffmpegProcess.EnableRaisingEvents = true;
            ffmpegProcess.Exited += HandlePipeClosed;
            ffmpegStdin = new BinaryWriter(ffmpegProcess.StandardInput.BaseStream);

            frameQueue = new Queue<byte[]>();
            Thread processingThread = new Thread(new ThreadStart(ProcessFrameQueue));
            processingThread.Start();
        }

        public void SendFrame(byte[] frameData, int count)
        {
            for (int i = 0; i < count; ++i)
            {
                frameQueue.Enqueue(frameData);
            }
        }

        void ProcessFrameQueue()
        {
            while (processingQueue)
            {
                if (frameQueue.Count > 0)
                {
                    Write(frameQueue.Dequeue());
                }
                else
                {
                    Thread.Sleep(5);
                }
            }
        }

        void Write(byte[] data)
        {
            if (ffmpegProcess == null) return;

            ffmpegStdin.Write(data);
            ffmpegStdin.Flush();
        }

        public void Close()
        {
            if (ffmpegProcess == null) return;

            processingQueue = false;

            ffmpegProcess.Close();
            ffmpegProcess.Dispose();

            ffmpegProcess = null;
            ffmpegStdin = null;
        }

        void HandlePipeClosed(object sender, EventArgs e)
        {
            if (FinishedAction != null)
            {
                CaptureCamDispatcher.instance.Invoke(() => {
                    FinishedAction(destinationPath);
                });
            }
        }
    }
}