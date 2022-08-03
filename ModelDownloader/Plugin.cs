using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPALogger = IPA.Logging.Logger;
using ModelDownloader.Configuration;
using ModelDownloader.HarmonyPatches;
using ModelDownloader.Installers;
using ModelDownloader.Utils;
using SiraUtil.Zenject;

namespace ModelDownloader
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        public void Init(IPALogger logger, Config config, Zenjector zenjector)
        {
            Instance = this;
            Log = logger;
            Log.Info("ModelDownloader initialized.");
            zenjector.Install<ModelDownloaderCoreInstaller>(Location.App, config.Generated<PluginConfig>());
            zenjector.Install<ModelDownloaderMenuInstaller>(Location.Menu);
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Log.Debug("OnApplicationStart");
            DownloadUtils.CheckDownloadedFiles();
            ModUtils.CheckInstalledMods();
            ModelDownloaderPatches.ApplyHarmonyPatches();
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            Log.Debug("OnApplicationQuit");
        }
    }
}
