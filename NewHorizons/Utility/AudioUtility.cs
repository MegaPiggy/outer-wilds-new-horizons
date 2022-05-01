using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace NewHorizons.Utility
{
    public static class AudioUtility
    {
        public static AudioClip LoadAudio(string filePath)
        {
            var task = Task.Run(async () => await GetAudioClip(filePath));
            task.Wait();
            return task.Result;
        }

        private static async Task<AudioClip> GetAudioClip(string filePath)
        {
            var extension = filePath.Split(new char[] { '.' }).Last();

            UnityEngine.AudioType audioType;

            switch (extension)
            {
                case ("wav"):
                    audioType = UnityEngine.AudioType.WAV;
                    break;
                case ("ogg"):
                    audioType = UnityEngine.AudioType.OGGVORBIS;
                    break;
                case ("mp3"):
                    Logger.LogWarning($".mp3 files take up a lot of memory! Please use .wav or .ogg instead.");
                    audioType = UnityEngine.AudioType.MPEG;
                    break;
                default:
                    Logger.LogError($"Invalid audio file extension ({extension}) must be .wav or .ogg");
                    return null;
            }

            if (audioType == UnityEngine.AudioType.MPEG)
            {
                string fileProtocolPath = $"file://{filePath}";
                DownloadHandlerAudioClip dh = new DownloadHandlerAudioClip(fileProtocolPath, UnityEngine.AudioType.MPEG);
                dh.compressed = true;
                using (UnityWebRequest www = new UnityWebRequest(fileProtocolPath, "GET", dh, null))
                {
                    var result = www.SendWebRequest();

                    while (!result.isDone) { await Task.Delay(100); }

                    if (www.isNetworkError)
                    {
                        Debug.Log(www.error);
                        return null;
                    }
                    else
                        return dh.audioClip;
                }
            }
            else
            {
                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePath, audioType))
                {
                    var result = www.SendWebRequest();

                    while (!result.isDone) { await Task.Delay(100); }

                    if (www.isNetworkError)
                    {
                        Debug.Log(www.error);
                        return null;
                    }
                    else
                    {
                        return DownloadHandlerAudioClip.GetContent(www);
                    }
                }
            }
        }
    }
}
