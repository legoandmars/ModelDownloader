using BeatSaberMarkupLanguage.Animations;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using ModelDownloader.Configuration;
using ModelDownloader.Types;
using ModelDownloader.Utils;
using System;
using System.Collections;
using System.Linq;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ModelDownloader.Settings.UI
{
    [HotReload(RelativePathToLayout = @"./Views/modelDetail.bsml")]
    [ViewDefinition("ModelDownloader.Settings.UI.Views.modelDetail.bsml")]
    internal class ModelDetailViewController : BSMLAutomaticViewController
    {
        private SiraLog _siraLog = null!;
        private PluginConfig _pluginConfig = null!;
        private DownloadUtils _downloadUtils = null!;
        
        private ModelSaberEntry _currentModel;
        
        private bool _downloadInteractable = false;
        private bool _previewInteractable = false;

        public Action<string> didClickAuthor;
        public Action<ModelSaberEntry> downloadPressed;
        public Action<ModelSaberEntry> previewPressed;
        public Action donatePressed;

        [Inject]
        protected void Construct(SiraLog siraLog, PluginConfig pluginConfig, DownloadUtils downloadUtils)
        {
            _siraLog = siraLog;
            _pluginConfig = pluginConfig;
            _downloadUtils = downloadUtils;
        }

        [UIComponent("donateButton")]
        public Button donateButton = null;

        [UIAction("donateClicked")]
        public void DonateClicked()
        {
            //parserParams.EmitEvent("close-patreonModal");
            donateButton.interactable = false;
            Application.OpenURL("https://www.patreon.com/bobbievr");
            StartCoroutine(DonateActiveAgain());
        }

        [UIAction("donateHelpClicked")]
        public void DonateHelpClicked()
        {
            donatePressed?.Invoke();
            // parserParams.EmitEvent("open-patreonModal");
        }
        private IEnumerator DonateActiveAgain()
        {
            yield return new WaitForSeconds(3);
            donateButton.interactable = true;
            // openedText.gameObject.SetActive(false);
        }

        [UIValue("downloadInteractable")]
        public bool DownloadInteractable
        {
            get => _downloadInteractable;
            set
            {
                _downloadInteractable = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("previewInteractable")]
        public bool PreviewInteractable
        {
            get => _previewInteractable;
            set
            {
                _previewInteractable = value;
                NotifyPropertyChanged();
            }
        }

        [UIAction("#post-parse")]
        internal void Setup()
        {
            rectTransform.sizeDelta = new Vector2(70, 0);
            rectTransform.anchorMin = new Vector2(0.5f, 0);
            rectTransform.anchorMax = new Vector2(0.5f, 1);

            SetupDetailView();
        }

        [UIComponent("thumbnail")]
        public ImageView Thumbnail = null;

        [UIComponent("name")]
        public CurvedTextMeshPro NameText = null;

        [UIComponent("author-name")]
        public CurvedTextMeshPro AuthorText = null;

        internal void SetupDetailView()
        {
        }

        internal void Initialize(ModelSaberEntry model, Sprite cover) {
            _currentModel = model;
            Thumbnail.sprite = cover;

            if (model.Thumbnail.EndsWith(".gif"))
            {
                var foundAnimation = AnimationController.instance.RegisteredAnimations.FirstOrDefault(x => x.Key == model.Id + ".gif");
                if (foundAnimation.Value != null) foundAnimation.Value.activeImages.Add(Thumbnail);
            }
            NameText.text = model.Name;
            AuthorText.text = model.Author;

            DownloadInteractable = !DownloadUtils.CheckIfModelInstalled(model);
            PreviewInteractable = !DownloadUtils.CheckIfModelInstalled(model) && (model.Type != "platform" && model.Type != "avatar");
            // Plugin.Log.Info(_pluginConfig.AutomaticallyGeneratePreviews.ToString());
            if (_pluginConfig.AutomaticallyGeneratePreviews && PreviewInteractable) PreviewPressed();
        }

        [UIAction("author-name-click")]
        internal void OnAuthorNameClick()
        {
            _siraLog.Info("Clicked author name...");
            didClickAuthor?.Invoke("author:"+AuthorText.text);
        }

        [UIAction("downloadPressed")]
        internal void DownloadPressed()
        {
            DownloadInteractable = false;
            _downloadUtils.DownloadModel(_currentModel);
            downloadPressed?.Invoke(_currentModel);
        }

        [UIAction("previewPressed")]
        internal void PreviewPressed()
        {
            PreviewInteractable = false;
            previewPressed?.Invoke(_currentModel);
        }

    }
}
