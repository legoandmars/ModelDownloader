using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using ModelDownloader.Settings.UI;
using System;
using Zenject;

namespace ModelDownloader.Managers
{
    internal class MenuButtonManager : IInitializable, IDisposable
    {
        private readonly MenuButton _menuButton;
        private readonly MainFlowCoordinator _mainFlowCoordinator;
        private readonly ModelDownloaderFlowCoordinator _modelFlowCoordinator;
        private readonly GameplaySetupViewController _gameplaySetupViewController;

        public MenuButtonManager(MainFlowCoordinator mainFlowCoordinator, ModelDownloaderFlowCoordinator modelFlowCoordinator, GameplaySetupViewController gameplaySetupViewController)
        {
            _mainFlowCoordinator = mainFlowCoordinator;
            _modelFlowCoordinator = modelFlowCoordinator;
            _gameplaySetupViewController = gameplaySetupViewController;

            _menuButton = new MenuButton("More Models", "Download models from ModelSaber.com", ShowModelFlow, true);
        }

        public void Initialize()
        {
            MenuButtons.instance.RegisterButton(_menuButton);
        }

        public void Dispose()
        {
            if (MenuButtons.IsSingletonAvailable)
            {
                MenuButtons.instance.UnregisterButton(_menuButton);
            }
        }

        private void ShowModelFlow()
        {
            _mainFlowCoordinator.PresentFlowCoordinator(_modelFlowCoordinator);
        }
    }
}
