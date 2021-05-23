using Newtonsoft.Json;
using ModelDownloader.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace ModelDownloader
{
    public static class ModelsaberUtils
    {
        static readonly HttpClient client = new HttpClient();

        public static async Task<List<ModelsaberEntry>> GetPage(ModelsaberSearch searchOptions)
        {
            client.BaseAddress = new Uri("https://modelsaber.com/api/v2/");
            // Call asynchronous network methods in a try/catch block to handle exceptions.
            try
            {
                string sortString = "";
                switch (searchOptions.ModelSort)
                {
                    case ModelsaberSearchSort.Newest:
                        sortString = "&sort=date&sortDirection=desc";
                        break;
                    case ModelsaberSearchSort.Oldest:
                        sortString = "&sort=date&sortDirection=asc";
                        break;
                    case ModelsaberSearchSort.Name:
                        sortString = "&sort=name&sortDirection=asc";
                        break;
                    case ModelsaberSearchSort.Author:
                        sortString = "&sort=author&sortDirection=asc";
                        break;
                }
                string constructedURL = $"get.php?type={(searchOptions.ModelType).ToString().ToLower()}&start={searchOptions.Page * searchOptions.PageLength}&end={(searchOptions.Page + 1) * searchOptions.PageLength}{sortString}";
                // Plugin.Log.Info(constructedURL);
                if (!string.IsNullOrWhiteSpace(searchOptions.Search))
                {
                    constructedURL += "&filter=" + searchOptions.Search;
                }
                HttpResponseMessage response = await client.GetAsync(constructedURL);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                // Plugin.Log.Info(responseBody);
                Dictionary<string, ModelsaberEntry> modelPageResponse = JsonConvert.DeserializeObject<Dictionary<string, ModelsaberEntry>>(responseBody);

                return modelPageResponse.Values.ToList();
            }
            catch (HttpRequestException e)
            {
                Plugin.Log.Info("\nException Caught!");
                Plugin.Log.Info("Message : " + e.Message);
                return null;
            }
        }

        public static async Task<byte[]> GetCoverImageBytes(ModelsaberEntry entry)
        {
            if (entry == null) return null;
            Uri thumbnailURL;
            if(!Uri.TryCreate(entry.Thumbnail, UriKind.Absolute, out thumbnailURL))
            {
                thumbnailURL = new Uri(entry.Download.Substring(0, entry.Download.LastIndexOf("/")) + "/" + entry.Thumbnail);
            }
            client.BaseAddress = null;
            HttpResponseMessage response = await client.GetAsync(thumbnailURL);

            // GIF loading code doesn't work, fix it later. it's pretty scuffed anyways, maybe just make it actually work as a gif...
            /*if (thumbnailURL.ToString().EndsWith(".gif"))
            {
                // oh boy, it's a gif. time for some processing stuff so we can just get the first frame.
                // Maybe you can add a proper animation to this later

                byte[] byteArray = await response.Content.ReadAsByteArrayAsync();
                return await Task.Run(() =>
                {
                    TaskCompletionSource<byte[]> source = new TaskCompletionSource<byte[]>();
                    AnimationLoader.Process(AnimationType.GIF, byteArray, (Texture2D tex, Rect[] uvs, float[] delays, int width, int height) =>
                    {
                        // Why in the world are these textures not readable
                        // BSML PLS
                        // Some hacky magic to make it work
                        Texture2D duplicatedTexture = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, false);
                        Graphics.CopyTexture(tex, duplicatedTexture);

                        source.SetResult(duplicatedTexture.EncodeToPNG());
                    });
                    return source.Task;
                });
            }*/
            return await response.Content.ReadAsByteArrayAsync();
        }
        public static Dictionary<int, byte[]> modelDownloadCache = new Dictionary<int, byte[]>();
        public static async Task<byte[]> GetModelBytes(ModelsaberEntry entry)
        {
            if (entry == null) return null;
            if (modelDownloadCache.ContainsKey(entry.Id)) return modelDownloadCache[entry.Id];
            Uri downloadURL;
            if (!Uri.TryCreate(entry.Download, UriKind.Absolute, out downloadURL))
            {
                downloadURL = new Uri(entry.Download.Substring(0, entry.Download.LastIndexOf("/")) + "/" + entry.Download);
            }
            client.BaseAddress = null;
            HttpResponseMessage response = await client.GetAsync(downloadURL);
            byte[] modelBytes = await response.Content.ReadAsByteArrayAsync();

            modelDownloadCache.Add(entry.Id, modelBytes);
            return modelBytes;
        }

        public static void ClearCache() => modelDownloadCache = new Dictionary<int, byte[]>();

        private static byte[] GetCoverImageBytesFromGIF(Texture2D tex, Rect[] uvs, float[] delays, int width, int height)
        {
            return tex.EncodeToPNG();
        }
    }
}
