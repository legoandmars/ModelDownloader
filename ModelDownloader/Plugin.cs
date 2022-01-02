﻿using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPALogger = IPA.Logging.Logger;
using ModelDownloader.Configuration;
using ModelDownloader.Installers;
using ModelDownloader.Managers;
using ModelDownloader.Utils;
using SiraUtil.Zenject;

namespace ModelDownloader
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static IPALogger Log { get; private set; }

        [Init]
        public void Init(IPALogger logger, Config config, Zenjector zenjector)
        {
            Log = logger;
            Log.Info("ModelDownloader initialized.");
            
            zenjector.UseLogger(logger);
            zenjector.UseHttpService();
            
            zenjector.Install<ModelDownloaderCoreInstaller>(Location.App, config.Generated<PluginConfig>());
            zenjector.Install<ModelDownloaderMenuInstaller>(Location.Menu);
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Log.Debug("OnApplicationStart");

            DownloadUtils.CheckDownloadedFiles();
            ModelDownloaderPatchManager.ApplyHarmonyPatches();
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            Log.Debug("OnApplicationQuit");

            ModelDownloaderPatchManager.RemoveHarmonyPatches();
        }
    }
}