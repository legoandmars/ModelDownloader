using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Notify;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using ModelDownloader.Configuration;
using ModelDownloader.Types;
using ModelDownloader.Utils;
using System;
using UnityEngine;
using Zenject;

namespace ModelDownloader.Settings.UI
{
    internal class ModelDetailViewController : BSMLResourceViewController, INotifiableHost
    {
        public override string ResourceName => "ModelDownloader.Settings.UI.Views.modelDetail.bsml";

        public Action<string> didClickAuthor;
        public Action<ModelsaberEntry> downloadPressed;
        public Action<ModelsaberEntry> previewPressed;

        private bool _downloadInteractable = false;
        private bool _previewInteractable = false;

        private ModelsaberEntry _currentModel;

        private PluginConfig _pluginConfig;

        [Inject]
        protected void Construct(PluginConfig pluginConfig)
        {
            _pluginConfig = pluginConfig;
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
        public bool PreviewInteractible
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
            (transform as RectTransform).sizeDelta = new Vector2(70, 0);
            (transform as RectTransform).anchorMin = new Vector2(0.5f, 0);
            (transform as RectTransform).anchorMax = new Vector2(0.5f, 1);

            SetupDetailView();
        }

        [UIComponent("thumbnail")]
        public ImageView Thumbnail;

        [UIComponent("name")]
        public CurvedTextMeshPro NameText;

        [UIComponent("author-name")]
        public CurvedTextMeshPro AuthorText;

        internal void SetupDetailView()
        {
        }

        internal void Initialize(ModelsaberEntry model, Sprite cover) {
            _currentModel = model;

            Thumbnail.sprite = cover;
            NameText.text = model.Name;
            AuthorText.text = model.Author;

            DownloadInteractable = !DownloadUtils.CheckIfModelInstalled(model);
            PreviewInteractible = !DownloadUtils.CheckIfModelInstalled(model) && (model.Type != "platform" && model.Type != "avatar");
            Plugin.Log.Info(_pluginConfig.AutomaticallyGeneratePreviews.ToString());
            if (_pluginConfig.AutomaticallyGeneratePreviews && PreviewInteractible) PreviewPressed();
        }

        [UIAction("author-name-click")]
        internal void OnAuthorNameClick()
        {
            Plugin.Log.Info("Clicked author name...");
            didClickAuthor?.Invoke("author:"+AuthorText.text);
        }

        [UIAction("downloadPressed")]
        internal void DownloadPressed()
        {
            DownloadInteractable = false;
            DownloadUtils.DownloadModel(_currentModel);
            downloadPressed?.Invoke(_currentModel);
        }

        [UIAction("previewPressed")]
        internal void PreviewPressed()
        {
            PreviewInteractible = false;
            previewPressed?.Invoke(_currentModel);
        }

    }
}
