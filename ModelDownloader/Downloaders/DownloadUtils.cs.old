using System;
using IPA.Utilities;
using ModelDownloader.Types;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SiraUtil.Logging;
using UnityEngine;

namespace ModelDownloader.Utils
{
    internal class DownloadUtils
    {
        private readonly SiraLog _siraLog;
        private readonly ModelSaberUtils _modelSaberUtils;

        private static List<string> _installedSabers = new();
        private static List<string> _installedBloqs = new();
        private static List<string> _installedAvatars = new();
        private static List<string> _installedPlatforms = new();

        public DownloadUtils(SiraLog siraLog, ModelSaberUtils modelSaberUtils)
        {
            _siraLog = siraLog;
            _modelSaberUtils = modelSaberUtils;
        }

        public static void CheckDownloadedFiles()
        {
            IEnumerable<string> saberFilter = new List<string> { "*.saber" };
            _installedSabers = GetFileNames(Path.Combine(UnityGame.InstallPath, "CustomSabers"), saberFilter, SearchOption.AllDirectories, true);

            IEnumerable<string> noteFilter = new List<string> { "*.bloq" };
            _installedBloqs = GetFileNames(Path.Combine(UnityGame.InstallPath, "CustomNotes"), noteFilter, SearchOption.AllDirectories, true);

            IEnumerable<string> avatarFilter = new List<string> { "*.avatar" };
            _installedAvatars = GetFileNames(Path.Combine(UnityGame.InstallPath, "CustomAvatars"), avatarFilter, SearchOption.AllDirectories, true);

            IEnumerable<string> platformFilter = new List<string> { "*.plat" };
            _installedPlatforms = GetFileNames(Path.Combine(UnityGame.InstallPath, "CustomPlatforms"), platformFilter, SearchOption.AllDirectories, true);
        }

        public static bool CheckIfModelInstalled(ModelSaberEntry model)
        {
            string modelFileName = model.Download.Substring(model.Download.LastIndexOf('/') + 1);

            return model.Type switch
            {
                "saber" => _installedSabers.Contains(modelFileName),
                "bloq" => _installedBloqs.Contains(modelFileName),
                "avatar" => _installedAvatars.Contains(modelFileName),
                "platform" => _installedPlatforms.Contains(modelFileName),
                _ => false
            };
        }

        public static void AddToInstalledList(ModelSaberEntry model)
        {
            string modelFileName = model.Download.Substring(model.Download.LastIndexOf('/') + 1);

            switch (model.Type)
            {
                case "saber":
                    _installedSabers.Add(modelFileName);
                    break;
                case "bloq":
                    _installedBloqs.Add(modelFileName);
                    break;
                case "avatar":
                    _installedAvatars.Add(modelFileName);
                    break;
                case "platform":
                    _installedPlatforms.Add(modelFileName);
                    break;
            }
        }

        public void DownloadModel(ModelSaberEntry model)
        {
            switch (model.Type)
            {
                case "saber":
                    DownloadModel(model, Path.Combine(UnityGame.InstallPath, "CustomSabers"));
                    break;
                case "bloq":
                    DownloadModel(model, Path.Combine(UnityGame.InstallPath, "CustomNotes"));
                    break;
                case "avatar":
                    DownloadModel(model, Path.Combine(UnityGame.InstallPath, "CustomAvatars"));
                    break;
                case "platform":
                    DownloadModel(model, Path.Combine(UnityGame.InstallPath, "CustomPlatforms"));
                    break;
            }
        }

        public async Task DownloadModel(ModelSaberEntry model, string downloadDirectoryPath)
        {
            var fileBytes = await _modelSaberUtils.GetModelBytes(model);
            if (fileBytes == null)
            {
                return;
            }

            string modelFileName = model.Download.Substring(model.Download.LastIndexOf('/') + 1);

            _siraLog.Info("Checking hash...");
            if (string.Equals(model.Hash, MD5Checksum(fileBytes), StringComparison.OrdinalIgnoreCase))
            {
                _siraLog.Info($"Hash check for {model.Name} passed!");
            }
            else
            {
                _siraLog.Error($"HASH CHECK FAILED FOR {model.Name}!");
                return;
            }

            // Actually save the file
            if (!Directory.Exists(downloadDirectoryPath))
            {
                Directory.CreateDirectory(downloadDirectoryPath);
            }

            string downloadPath = Path.Combine(downloadDirectoryPath, modelFileName);
            if (!File.Exists(downloadPath))
            {
                File.WriteAllBytes(downloadPath, fileBytes);
                AddToInstalledList(model);
            }
        }

        public static string MD5Checksum(byte[] inputBytes)
        {
            // Use input string to calculate MD5 hash
            using (var md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }

                return sb.ToString();
            }
        }

        // Some methods to help with previews
        public async Task<AssetBundle?> DownloadModelAsPreview(ModelSaberEntry model)
        {
            var fileBytes = await _modelSaberUtils.GetModelBytes(model);
            if (fileBytes == null)
            {
                return null;
            }

            _siraLog.Info("Checking hash...");
            if (string.Equals(model.Hash, MD5Checksum(fileBytes), StringComparison.OrdinalIgnoreCase))
            {
                _siraLog.Info($"Hash check for {model.Name} passed!");
            }
            else
            {
                _siraLog.Error($"HASH CHECK FAILED FOR {model.Name}!");
                return null;
            }

            return AssetBundle.LoadFromMemory(fileBytes);
        }

        /// <summary>
        /// Gets every file matching the filter in a path.
        /// </summary>
        /// <param name="path">Directory to search in.</param>
        /// <param name="filters">Pattern(s) to search for.</param>
        /// <param name="searchOption">Search options.</param>
        /// <param name="returnShortPath">Remove path from filepaths.</param>
        private static List<string> GetFileNames(string path, IEnumerable<string> filters, SearchOption searchOption, bool returnShortPath = false)
        {
            IList<string> filePaths = new List<string>();

            foreach (string filter in filters)
            {
                if (!Directory.Exists(path)) continue;
                IEnumerable<string> directoryFiles = Directory.GetFiles(path, filter, searchOption);

                if (returnShortPath)
                {
                    foreach (string directoryFile in directoryFiles)
                    {
                        string filePath = directoryFile.Replace(path, "");
                        if (filePath.Length > 0 && filePath.StartsWith("\\"))
                        {
                            filePath = filePath.Substring(1, filePath.Length - 1);
                        }

                        if (!string.IsNullOrWhiteSpace(filePath) && !filePaths.Contains(filePath))
                        {
                            filePaths.Add(filePath);
                        }
                    }
                }
                else
                {
                    filePaths = filePaths.Union(directoryFiles).ToList();
                }
            }

            return filePaths.Distinct().ToList();
        }
    }
}