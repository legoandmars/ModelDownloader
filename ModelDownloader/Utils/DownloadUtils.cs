using System;
using IPA.Utilities;
using ModelDownloader.Types;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModelDownloader.Utils
{
    public static class DownloadUtils
    {
        private static List<string> InstalledSabers;
        private static List<string> InstalledBloqs;
        private static List<string> InstalledAvatars;
        private static List<string> InstalledPlatforms;

        public static void CheckDownloadedFiles()
        {
            IEnumerable<string> saberFilter = new List<string> { "*.saber" };
            InstalledSabers = GetFileNames(Path.Combine(UnityGame.InstallPath, "CustomSabers"), saberFilter, SearchOption.AllDirectories, true);

            IEnumerable<string> noteFilter = new List<string> { "*.bloq" };
            InstalledBloqs = GetFileNames(Path.Combine(UnityGame.InstallPath, "CustomNotes"), noteFilter, SearchOption.AllDirectories, true);

            IEnumerable<string> avatarFilter = new List<string> { "*.avatar" };
            InstalledAvatars = GetFileNames(Path.Combine(UnityGame.InstallPath, "CustomAvatars"), avatarFilter, SearchOption.AllDirectories, true);

            IEnumerable<string> platformFilter = new List<string> { "*.plat" };
            InstalledPlatforms = GetFileNames(Path.Combine(UnityGame.InstallPath, "CustomPlatforms"), platformFilter, SearchOption.AllDirectories, true);
        }

        public static bool CheckIfModelInstalled(ModelsaberEntry model)
        {
            string modelFileName = model.Download.Substring(model.Download.LastIndexOf('/') + 1);

            if (model.Type == "saber") return InstalledSabers.Contains(modelFileName);
            else if (model.Type == "bloq") return InstalledBloqs.Contains(modelFileName);
            else if (model.Type == "avatar") return InstalledAvatars.Contains(modelFileName);
            else if (model.Type == "platform") return InstalledPlatforms.Contains(modelFileName);
            else return false;
        }

        public static void AddToInstalledList(ModelsaberEntry model)
        {
            string modelFileName = model.Download.Substring(model.Download.LastIndexOf('/') + 1);

            if (model.Type == "saber") InstalledSabers.Add(modelFileName);
            else if (model.Type == "bloq") InstalledBloqs.Add(modelFileName);
            else if (model.Type == "avatar") InstalledAvatars.Add(modelFileName);
            else if (model.Type == "platform") InstalledPlatforms.Add(modelFileName);
        }

        public static void DownloadModel(ModelsaberEntry model)
        {
            if (model.Type == "saber") DownloadModel(model, Path.Combine(UnityGame.InstallPath, "CustomSabers"));
            else if (model.Type == "bloq") DownloadModel(model, Path.Combine(UnityGame.InstallPath, "CustomNotes"));
            else if (model.Type == "avatar") DownloadModel(model, Path.Combine(UnityGame.InstallPath, "CustomAvatars"));
            else if (model.Type == "platform") DownloadModel(model, Path.Combine(UnityGame.InstallPath, "CustomPlatforms"));
        }

        public static async void DownloadModel(ModelsaberEntry model, string downloadDirectoryPath)
        {
            byte[] fileBytes = await ModelsaberUtils.GetModelBytes(model);
            string modelFileName = model.Download.Substring(model.Download.LastIndexOf('/') + 1);

            Plugin.Log.Info("Checking hash...");
            if (string.Equals(model.Hash, MD5Checksum(fileBytes), StringComparison.OrdinalIgnoreCase))
            {
                Plugin.Log.Info($"Hash check for {model.Name} passed!");
            }
            else
            {
                Plugin.Log.Error($"HASH CHECK FAILED FOR {model.Name}!");
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
        public static async Task<AssetBundle> DownloadModelAsPreview(ModelsaberEntry model)
        {
            byte[] fileBytes = await ModelsaberUtils.GetModelBytes(model);
            string modelFileName = model.Download.Substring(model.Download.LastIndexOf('/') + 1);

            Plugin.Log.Info("Checking hash...");
            if (string.Equals(model.Hash, MD5Checksum(fileBytes), StringComparison.OrdinalIgnoreCase))
            {
                Plugin.Log.Info($"Hash check for {model.Name} passed!");
            }
            else
            {
                Plugin.Log.Error($"HASH CHECK FAILED FOR {model.Name}!");
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
        public static List<string> GetFileNames(string path, IEnumerable<string> filters, SearchOption searchOption, bool returnShortPath = false)
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