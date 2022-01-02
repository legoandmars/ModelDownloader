using BeatSaberMarkupLanguage;
using HMUI;
using ModelDownloader.Types;
using UnityEngine;
using Zenject;

namespace ModelDownloader.Settings.UI
{
    internal class ModelDownloaderFlowCoordinator : FlowCoordinator
    {
        private MainFlowCoordinator _mainFlow;
        private ModelListViewController _modelList;
        private ModelDetailViewController _modelDetail;
        private ModelPreviewViewController _modelPreview;

        [Inject]
        public void Construct(MainFlowCoordinator mainFlow, ModelListViewController modelList, ModelDetailViewController modelDetail, ModelPreviewViewController modelPreview)
        {
            _mainFlow = mainFlow;
            _modelList = modelList;
            _modelDetail = modelDetail;
            _modelPreview = modelPreview;
            _modelList.didSelectModel += HandleDidSelectModel;
            _modelDetail.didClickAuthor += HandleDidSelectAuthor;
            _modelDetail.downloadPressed += HandleDownload;
            _modelDetail.previewPressed += HandlePreview;
            _modelDetail.donatePressed += HandleDonate;
        }

        [Inject(Id = "com.legoandmars.modeldownloader.navigationcontroller")]
        private NavigationController _modelNavigationController;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                SetTitle("Model Downloader");
                showBackButton = true;

                SetViewControllersToNavigationController(_modelNavigationController, _modelList);
                ProvideInitialViewControllers(_modelNavigationController);
            }
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            // Dismiss ourselves
            _mainFlow.DismissFlowCoordinator(this);
        }

        internal void HandleDidSelectModel(ModelsaberEntry model, Sprite cover = null)
        {
            // _modelDetail.ClearData();
            if (!_modelDetail.isInViewControllerHierarchy)
            {
                PushViewControllerToNavigationController(_modelNavigationController, _modelDetail);
            }

            SetRightScreenViewController(_modelPreview, ViewController.AnimationType.None);
            _modelPreview.ClearData();

            _modelDetail.Initialize(model, cover);
            //_songDetailView.Initialize(song, cover);
        }

        internal void HandleDidSelectAuthor(string author)
        {
            _modelList.currentSearch = author;
            _modelList.ClearData();
            _modelList.GetModelPages(0);
        }

        internal void HandleDownload(ModelsaberEntry model)
        {
            _modelList.DisableDownloadsOnModel(model);
            _modelList.DisplayWarningPromptIfNeeded(model);
        }

        internal void HandlePreview(ModelsaberEntry model)
        {
            _modelPreview.CreatePreview(model);
        }

        internal void HandleDonate()
        {
            _modelList.OpenDonateModal();
        }
    }
}