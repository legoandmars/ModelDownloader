using System;
using System.Threading;
using System.Threading.Tasks;
using IPA.Utilities;
using ModelDownloader.Utils;
using Newtonsoft.Json;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.Networking;

namespace ModelDownloader.Downloaders
{
    public abstract class Downloader
    {
        internal string USER_AGENT { get; set; }
        
        private ConcurrentHashSet<UnityWebRequest> _ongoingWebRequests = new ConcurrentHashSet<UnityWebRequest>();
        private SiraLog _siraLog;
        
        public Downloader(SiraLog siraLog)
        {
            _siraLog = siraLog;
            USER_AGENT = $"Unity/{UnityEngine.Application.unityVersion} BeatSaber/{UnityGame.GameVersion} ModelDownloader/{Plugin.Version}";
        }

        ~Downloader()
        {
            foreach (var webRequest in _ongoingWebRequests)
            {
                webRequest.Abort();
            }
        }

        public void CancelAllDownloads()
        {
            foreach (var webRequest in _ongoingWebRequests)
            {
                webRequest.Abort();
            }
        }

        internal async Task<T> MakeJsonRequestAsync<T>(string url, CancellationToken cancellationToken, Action<float> progressCallback = null)
        {
            var www = await MakeRequestAsync(url, JsonConvert.SerializeObject(null), cancellationToken, progressCallback);

            if (www == null)
            {
                return default(T);
            }

            try
            {
                T response = JsonConvert.DeserializeObject<T>(www.downloadHandler.text);

                return response;
            }
            catch (Exception e)
            {
                _siraLog.Warn($"Error parsing response: {e.Message}");
                return default(T);
            }
        }
        
        internal async Task<Sprite> MakeImageRequestAsync(string url, CancellationToken cancellationToken, Action<float> progressCallback = null)
        {
            var www = await MakeRequestAsync(url, JsonConvert.SerializeObject(null), cancellationToken, progressCallback);

            if (www == null)
            {
                return null;
            }

            try
            {
                Sprite sprite = BeatSaberMarkupLanguage.Utilities.LoadSpriteRaw(www.downloadHandler.data);
                return sprite;
            }
            catch (Exception e)
            {
                _siraLog.Warn($"Error parsing image: {e.Message}");
                return null;
            }
        }

        internal async Task<UnityWebRequest> MakeRequestAsync(string url, string body, CancellationToken cancellationToken, Action<float> progressCallback = null)
        {
            var www = UnityWebRequest.Post(url, body);
            www.SetRequestHeader("User-Agent", USER_AGENT);
            www.timeout = 15;
#if DEBUG
            _siraLog.Debug($"Making POST request with url: {url} and body: {body}");
#endif
                _ongoingWebRequests.Add(www);

                www.SendWebRequest();

                while (!www.isDone)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        www.Abort();
                        throw new TaskCanceledException();
                    }
                    progressCallback?.Invoke(www.downloadProgress);
                    await Task.Yield();
                }
#if DEBUG
            _siraLog.Debug($"Finished web request: {url}"); 
#endif
            _ongoingWebRequests.Remove(www);

            if (www.isNetworkError || www.isHttpError)
            {
                _siraLog.Warn($"Error making request: {www.error}");
                return null;
            }
            
            return www;
        }
    }
}