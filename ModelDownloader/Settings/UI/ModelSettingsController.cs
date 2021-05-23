using System;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Settings;
using ModelDownloader.Configuration;
using Zenject;

namespace ModelDownloader.Settings.UI
{
    public class ModelSettingsController : IInitializable, IDisposable
    {
        private PluginConfig _pluginConfig;

        [UIValue("blur-images")]
        public bool BlurNsfwImages
        {
            get => _pluginConfig.BlurNSFWImages;
            set => _pluginConfig.BlurNSFWImages = value;
        }

        [UIValue("disable-warnings")]
        public bool DisableWarnings
        {
            get => _pluginConfig.DisableWarnings;
            set => _pluginConfig.DisableWarnings = value;
        }

        [UIValue("autogen-previews")]
        public bool AutogeneratePreviews
        {
            get => _pluginConfig.AutomaticallyGeneratePreviews;
            set => _pluginConfig.AutomaticallyGeneratePreviews = value;
        }

        internal ModelSettingsController(PluginConfig pluginConfig)
        {
            _pluginConfig = pluginConfig;
        }

        public void Initialize()
        {
            BSMLSettings.instance.AddSettingsMenu("Model Downloader", "ModelDownloader.Settings.UI.Views.modelSettings.bsml", this);
        }

        public void Dispose()
        {
            BSMLSettings.instance.RemoveSettingsMenu(this);
        }
    }
}
