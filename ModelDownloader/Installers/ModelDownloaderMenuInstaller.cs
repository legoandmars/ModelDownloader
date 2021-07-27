using HMUI;
using ModelDownloader.Managers;
using ModelDownloader.Settings.UI;
using SiraUtil;
using Zenject;

namespace ModelDownloader.Installers
{
    internal class ModelDownloaderMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<ModelListViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<ModelDetailViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<ModelPreviewViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<NavigationController>().WithId("com.legoandmars.modeldownloader.navigationcontroller").FromNewComponentAsViewController().AsCached();
            Container.Bind<ModelDownloaderFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();

            Container.BindInterfacesTo<MenuButtonManager>().AsSingle();
            Container.BindInterfacesTo<ModelSettingsController>().AsSingle();
        }
    }
}
