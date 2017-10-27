using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OoniCaptureCam
{
    public class CaptureCamWavEncoder
    {
        const int HEADER_SIZE = 44;

        public System.Action<string> FinishedAction;

        private string destinationPath;
        private int outputRate;
        private FileStream fileStream;

        public CaptureCamWavEncoder(string _destinationPath, int _outputRate)
        {
            destinationPath = _destinationPath;
            outputRate = _outputRate;
            fileStream = new FileStream(destinationPath, FileMode.Create);
        }

        public void Write(float[] data, int channels)
        {
            Int16[] intData = new Int16[data.Length];
            Byte[] bytesData = new Byte[data.Length * 2];

            const float rescaleFactor = 32767;

            for (int i = 0; i < data.Length; i++)
            {
                intData[i] = (short)(data[i] * rescaleFactor);
            }

            Buffer.BlockCopy(intData, 0, bytesData, 0, bytesData.Length);
            fileStream.Write(bytesData, 0, bytesData.Length);
        }

        public void Close()
        {
            WriteHeader(fileStream);
            fileStream.Close();

            if (FinishedAction != null)
            {
                CaptureCamDispatcher.instance.Invoke(() => FinishedAction(destinationPath));
            }
        }

        void WriteHeader(FileStream fileStream)
        {
            fileStream.Seek(0, SeekOrigin.Begin);

            Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
            fileStream.Write(riff, 0, 4);

            Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
            fileStream.Write(chunkSize, 0, 4);

            Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
            fileStream.Write(wave, 0, 4);

            Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
            fileStream.Write(fmt, 0, 4);

            Byte[] subChunk1 = BitConverter.GetBytes(16);
            fileStream.Write(subChunk1, 0, 4);

            UInt16 two = 2;
            UInt16 one = 1;

            Byte[] audioFormat = BitConverter.GetBytes(one);
            fileStream.Write(audioFormat, 0, 2);

            Byte[] numChannels = BitConverter.GetBytes(two);
            fileStream.Write(numChannels, 0, 2);

            Byte[] sampleRate = BitConverter.GetBytes(outputRate);
            fileStream.Write(sampleRate, 0, 4);

            Byte[] byteRate = BitConverter.GetBytes(outputRate * 4); // sampleRate * bytesPerSample*number of channels, here 44100*2*2
            fileStream.Write(byteRate, 0, 4);

            UInt16 four = 4;

            UInt16 blockAlign = four;
            fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

            UInt16 bps = 16;
            Byte[] bitsPerSample = BitConverter.GetBytes(bps);
            fileStream.Write(bitsPerSample, 0, 2);

            Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
            fileStream.Write(datastring, 0, 4);

            Byte[] subChunk2 = BitConverter.GetBytes(fileStream.Length - HEADER_SIZE);
            fileStream.Write(subChunk2, 0, 4);

            fileStream.Close();
        }
    }
}