using ModelDownloader.Configuration;
using ModelDownloader.Utils;
using Zenject;

namespace ModelDownloader.Installers
{
    internal class ModelDownloaderCoreInstaller : Installer<PluginConfig, ModelDownloaderCoreInstaller>
    {
        private readonly PluginConfig _pluginConfig;

        public ModelDownloaderCoreInstaller(PluginConfig pluginConfig)
        {
            _pluginConfig = pluginConfig;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(_pluginConfig).AsSingle();

            Container.Bind<DownloadUtils>().AsSingle();
            Container.Bind<ModelSaberUtils>().AsSingle();
            Container.BindInterfacesAndSelfTo<ModUtils>().AsSingle();
        }
    }
}
