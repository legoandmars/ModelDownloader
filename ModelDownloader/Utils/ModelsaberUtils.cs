using Newtonsoft.Json;
using ModelDownloader.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SiraUtil.Web;
using UnityEngine;

namespace ModelDownloader
{
    public class ModelsaberUtils
    {
        // TODO: Actually inject this thing
        static readonly IHttpService client;

        public static async Task<List<ModelsaberEntry>?> GetPage(ModelsaberSearch searchOptions)
        {
            // Call asynchronous network methods in a try/catch block to handle exceptions.

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

            string constructedURL = $"https://modelsaber.com/api/v2/get.php?type={(searchOptions.ModelType).ToString().ToLower()}&start={searchOptions.Page * searchOptions.PageLength}&end={(searchOptions.Page + 1) * searchOptions.PageLength}{sortString}";
            // Plugin.Log.Info(constructedURL);
            if (!string.IsNullOrWhiteSpace(searchOptions.Search))
            {
                constructedURL += "&filter=" + searchOptions.Search;
            }

            IHttpResponse response = await client.GetAsync(constructedURL);
            if (!response.Successful)
            {
                Plugin.Log.Warn($"Call to endpoint: {constructedURL} returned an unsuccessful status code {response.Code}");
                return null;
            }

            string responseBody = await response.ReadAsStringAsync();

            // Plugin.Log.Info(responseBody);
            Dictionary<string, ModelsaberEntry> modelPageResponse = JsonConvert.DeserializeObject<Dictionary<string, ModelsaberEntry>>(responseBody);

            return modelPageResponse.Values.ToList();
        }

        public static async Task<byte[]?> GetCoverImageBytes(ModelsaberEntry? entry)
        {
            if (entry == null)
            {
                return null;
            }

            var thumbnailUrl = entry.Thumbnail;
            if (!Uri.TryCreate(thumbnailUrl, UriKind.Absolute, out _))
            {
                thumbnailUrl = thumbnailUrl.Substring(0, entry.Download.LastIndexOf('/')) + '/' + entry.Thumbnail;
            }

            var response = await client.GetAsync(thumbnailUrl);

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
            return await response.ReadAsByteArrayAsync();
        }

        public static Dictionary<int, byte[]> modelDownloadCache = new Dictionary<int, byte[]>();

        public static async Task<byte[]?> GetModelBytes(ModelsaberEntry? entry)
        {
            if (entry == null)
            {
                return null;
            }

            if (modelDownloadCache.ContainsKey(entry.Id)) return modelDownloadCache[entry.Id];
            string downloadUrl = entry.Download;
            if (!Uri.TryCreate(downloadUrl, UriKind.Absolute, out _))
            {
                downloadUrl = entry.Download.Substring(0, entry.Download.LastIndexOf("/")) + "/" + entry.Download;
            }

            var response = await client.GetAsync(downloadUrl);
            byte[] modelBytes = await response.ReadAsByteArrayAsync();

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