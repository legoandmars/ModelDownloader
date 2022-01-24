using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IPA.Loader;
using JetBrains.Annotations;
using ModelDownloader.Downloaders;
using ModelDownloader.Types;
using SiraUtil.Logging;

namespace ModelDownloader.Downloaders
{
    internal class ModelSaberDownloader : Downloader
    {
        public static Dictionary<int, byte[]> modelDownloadCache = new();
        
        internal const string API_URL = "https://api.modelsaber.com/";
        internal const string GRAPHQL = "graphql";

        private readonly SiraLog _siraLog;

        public ModelSaberDownloader([NotNull] SiraLog siraLog) : base(siraLog)
        {
            _siraLog = siraLog;
        }

        public async Task<List<ModelSaberEntry>> GetModelsAsync(CancellationToken cancellationToken)
        {
            string url = API_URL + GRAPHQL;
            return await MakeJsonRequestAsync<List<ModelSaberEntry>>(url, cancellationToken);
        }
    }
}