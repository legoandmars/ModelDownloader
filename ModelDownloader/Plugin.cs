using IPA;
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
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        [Init]
        public Plugin(IPALogger logger, Config config, Zenjector zenjector)
        {
            zenjector.UseLogger(logger);
            zenjector.UseHttpService();
            
            zenjector.Install<ModelDownloaderCoreInstaller>(Location.App, config.Generated<PluginConfig>());
            zenjector.Install<ModelDownloaderMenuInstaller>(Location.Menu);
        }

        [OnEnable]
        public void OnApplicationStart()
        {
            DownloadUtils.CheckDownloadedFiles();
            ModelDownloaderPatchManager.ApplyHarmonyPatches();
        }

        [OnDisable]
        public void OnApplicationQuit()
        {
            ModelDownloaderPatchManager.RemoveHarmonyPatches();
        }
    }
}