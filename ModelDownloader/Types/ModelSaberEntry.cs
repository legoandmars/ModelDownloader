using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModelDownloader.Utils;
using SiraUtil.Logging;
using Zenject;

namespace ModelDownloader.Types
{
    internal class ModelSaberEntry
    {
        [Inject]
        private readonly ModelSaberUtils _modelSaberUtils = null!;

        [Inject]
        private readonly SiraLog _siraLog = null!;

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("bsaber")]
        public string Bsaber { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("discordid")]
        public string Discordid { get; set; }

        [JsonProperty("discord")]
        public string Discord { get; set; }

        [JsonProperty("variationid")]
        public object Variationid { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("download")]
        public string Download { get; set; }

        [JsonProperty("install_link")]
        public string InstallLink { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        public async Task<byte[]?> GetCoverImageBytes()
        {
            try
            {
                return await _modelSaberUtils.GetCoverImageBytes(this);
            }
            catch (Exception e)
            {
                _siraLog.Error("FAILED TO GET COVER IMAGE:");
                _siraLog.Error(e);
                _siraLog.Error(Thumbnail);
                return null;
            }
        }
    }
}