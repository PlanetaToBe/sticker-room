using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace OoniCaptureCam
{
    public class CaptureCamFFmpegMuxer
    {
        public System.Action<string> FinishedAction;

        private Process subprocess;
        private string outPath;

        public CaptureCamFFmpegMuxer(string videoPath, string audioPath, string _outPath)
        {
            string opt = "";
            outPath = _outPath;

            if (videoPath == null)
            {
                opt += "-i " + audioPath + " -c:a libmp3lame " + outPath;
            }
            else if (audioPath == null)
            {
                opt += "-i " + videoPath + " -c copy " + outPath; 
            }
            else
            {
                opt += "-i " + videoPath + " -i " + audioPath + " -c:a libmp3lame -c:v copy -shortest " + outPath;
            }

            CaptureCam.Log("Running FFmpeg mux command " + CaptureCam.ffmpegPath + opt);

            var info = new ProcessStartInfo(CaptureCam.ffmpegPath, opt);
            info.UseShellExecute = false;
            info.CreateNoWindow = true;

            subprocess = Process.Start(info);
            subprocess.EnableRaisingEvents = true;
            subprocess.Exited += HandleProcessClosed;
        }

        void HandleProcessClosed(object sender, System.EventArgs e)
        {
            subprocess.Exited -= HandleProcessClosed;
            subprocess.Dispose();

            if (FinishedAction != null)
            {
                CaptureCamDispatcher.instance.Invoke(() => FinishedAction(outPath));
            }
        }
    }
}