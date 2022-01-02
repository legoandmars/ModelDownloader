using BeatSaberMarkupLanguage.Animations;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Utilities;
using ModelDownloader.Configuration;
using ModelDownloader.Types;
using ModelDownloader.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ModelDownloader.Settings.UI
{
    [HotReload(RelativePathToLayout = @"./Views/modelList.bsml")]
    [ViewDefinition("ModelDownloader.Settings.UI.Views.modelList.bsml")]
    internal class ModelListViewController : BSMLAutomaticViewController
    {
        public LoadingControl loadingSpinner;
        public int currentPage = 0;
        public ModelsaberSearchSort currentSort = 0;
        public string currentSearch = "";
        public bool searchingForPage = false;
        public bool firstSearch = true;

        private readonly List<ModelSaberEntry> _models = new();

        private PluginConfig _pluginConfig = null!;
        private ModUtils _modUtils = null!;
        private ModelSaberUtils _modelSaberUtils = null!;
        private DiContainer _container = null!;
        private KawaseBlurRendererSO _kawaseBlurRenderer = null!;

        [UIParams]
        internal BeatSaberMarkupLanguage.Parser.BSMLParserParams parserParams = null!;

        [UIValue("model-type-options")]
        private List<object> modelTypeOptions = new() { "All Models", "Sabers", "Bloqs", "Platforms", "Avatars" };

        [UIValue("model-type-choice")]
        private string modelTypeChoice = "All Models";

        [UIComponent("list")]
        public CustomListTableData customListTableData = null!;

        [UIComponent("loadingModal")]
        public ModalView loadingModal = null!;

        public Action<ModelSaberEntry, Sprite> didSelectModel;

        [UIAction("listSelect")]
        internal void Select(TableView tableView, int row)
        {
            didSelectModel?.Invoke(_models[row], customListTableData.data[row].icon);
        }

        [Inject]
        protected void Construct(PluginConfig pluginConfig, ModUtils modUtils, ModelSaberUtils modelSaberUtils, LevelPackDetailViewController levelPackDetailViewController, DiContainer container)
        {
            _pluginConfig = pluginConfig;
            _modUtils = modUtils;
            _modelSaberUtils = modelSaberUtils;
            _container = container;
            _kawaseBlurRenderer = levelPackDetailViewController.GetField<KawaseBlurRendererSO, LevelPackDetailViewController>("_kawaseBlurRenderer");
        }

        [UIAction("#post-parse")]
        public void SetupList()
        {
            rectTransform.sizeDelta = new Vector2(70, 0);
            rectTransform.anchorMin = new Vector2(0.5f, 0);
            rectTransform.anchorMax = new Vector2(0.5f, 1);

            sourceListTableData.data.Clear();
            sourceListTableData.data.Add(new CustomListTableData.CustomCellInfo("Newest"));
            sourceListTableData.data.Add(new CustomListTableData.CustomCellInfo("Oldest"));
            sourceListTableData.data.Add(new CustomListTableData.CustomCellInfo("Name"));
            sourceListTableData.data.Add(new CustomListTableData.CustomCellInfo("Author"));
            sourceListTableData.tableView.ReloadData();

            KEYBOARD.KEY tagKey = new KEYBOARD.KEY(_searchKeyboard.keyboard, new Vector2(-35, 11f), "Tag:", 15, 10, new Color(0.92f, 0.64f, 0));
            _searchKeyboard.keyboard.keys.Add(tagKey);
            tagKey.keyaction += TagKeyPressed;

            GetModelPages(0);
        }

        [UIAction("donateClicked")]
        public void DonateClicked()
        {
            //button.interactable = false;
            //linkOpened.gameObject.SetActive(true);
            //StartCoroutine(SecondRemove(button));
            parserParams.EmitEvent("close-patreonModal");
            Application.OpenURL("https://www.patreon.com/bobbievr");
        }

        [UIAction("closePressed")]
        public void ClosePressed()
        {
            parserParams.EmitEvent("close-patreonModal");
        }

        public void OpenDonateModal()
        {
            parserParams.EmitEvent("open-patreonModal");
        }

        [UIAction("#model-type-changed")]
        public void ModelTypeChanged(string modelType)
        {
            modelTypeChoice = modelType;
            ClearData();
            GetModelPages(currentPage);
        }

        [UIAction("pageUpPressed")]
        internal void PageUpPressed()
        {
        }

        [UIAction("pageDownPressed")]
        internal void PageDownPressed()
        {
            if ((customListTableData.data.Count() - customListTableData.tableView.visibleCells.Last().idx) <= 1 && searchingForPage == false)
            {
                currentPage++;
                GetModelPages(currentPage);
            }
        }

        public void SetLoading(bool value, double progress = 0, string details = "")
        {
            if (loadingSpinner == null)
                loadingSpinner = GameObject.Instantiate(Resources.FindObjectsOfTypeAll<LoadingControl>().First(), loadingModal.transform);
            Destroy(loadingSpinner.GetComponent<Touchable>());
            if (value)
            {
                parserParams.EmitEvent("open-loadingModal");
                loadingSpinner.ShowDownloadingProgress("Fetching More Models... " + details, (float)progress);
            }
            else
            {
                parserParams.EmitEvent("close-loadingModal");
            }
        }

        [UIAction("abortClicked")]
        internal void AbortPageFetch()
        {
            searchingForPage = false;
            parserParams.EmitEvent("close-loadingModal");
            ClearData();
        }

        public async void GetModelPages(int page)
        {
            searchingForPage = true;
            SetLoading(true, 0, $"Page {page + 1}/{page + 1}");
            parserParams.EmitEvent("open-loadingModal");
            ModelSaberSearch searchOptions = new ModelSaberSearch((ModelsaberSearchType)modelTypeOptions.IndexOf(modelTypeChoice), page, currentSort, currentSearch);
            List<ModelSaberEntry> entries = await _modelSaberUtils.GetPage(searchOptions);
            if (!searchingForPage) return;
            foreach (ModelSaberEntry entry in entries)
            {
                _models.Add(entry);
                _container.Inject(entry);
                customListTableData.data.Add(new ModelCellInfo(_pluginConfig, _kawaseBlurRenderer, CellDidSetImage, entry, DownloadUtils.CheckIfModelInstalled(entry)? $"<#7F7F7F>{entry.Name}" : entry.Name, entry.Author));
            }

            SetLoading(false);

            customListTableData.tableView.ReloadData();

            int pageScrollAmount = (page * ModelSaberSearch.PageLength) - 1;
            if (pageScrollAmount > customListTableData.data.Count) pageScrollAmount = customListTableData.data.Count;
            if (pageScrollAmount < 0) pageScrollAmount = 0;
            customListTableData.tableView.ScrollToCellWithIdx(pageScrollAmount, TableView.ScrollPositionType.Beginning, false);
            searchingForPage = false;
            /*
             * Popup disabled temporarily due to potentially being controversial
            if (firstSearch)
            {
                firstSearch = false;
                if (_pluginConfig.ShowPopup == "NextStartup") _pluginConfig.ShowPopup = "True";
                else if (_pluginConfig.ShowPopup == "True")
                {
                    // social experiment
                    parserParams.EmitEvent("open-patreonModal");
                    _pluginConfig.ShowPopup = "False";
                }
            }
            */
        }

        internal class ModelCellInfo : CustomListTableData.CustomCellInfo
        {
            private readonly PluginConfig _config;
            private readonly KawaseBlurRendererSO _kawaseBlurRenderer;
            private readonly Action<CustomListTableData.CustomCellInfo> _callback;

            public ModelSaberEntry Model;

            public ModelCellInfo(PluginConfig config, KawaseBlurRendererSO blurRenderer, Action<CustomListTableData.CustomCellInfo> callback, ModelSaberEntry model, string text, string? subtext = null)
                : base(text, subtext)
            {
                _config = config;
                _kawaseBlurRenderer = blurRenderer;
                _callback = callback;

                Model = model;

                LoadImage();
            }

            protected async void LoadImage()
            {
                var imageBytes = await Model.GetCoverImageBytes();
                if (imageBytes == null)
                {
                    return;
                }

                Sprite icon;

                if (Model.Thumbnail.EndsWith(".gif"))
                {
                    var animationData = await SpriteUtils.LoadSpriteRawAnimated(imageBytes, Model.Id + ".gif");
                    icon = animationData.sprites[0];
                    icon.name = Model.Id + ".gif";
                }
                else
                {
                    if (Model.Tags.Where(x => x.ToLower() == "nsfw").Any() && _config.BlurNSFWImages)
                    {
                        // nsfw image, blur it.
                        icon = SpriteUtils.LoadSpriteFromTexture(_kawaseBlurRenderer.Blur(SpriteUtils.LoadTextureRaw(imageBytes), KawaseBlurRendererSO.KernelSize.Kernel35, 2));
                    }
                    else
                    {
                        icon = SpriteUtils.LoadSpriteRaw(imageBytes);
                    }
                }

                base.icon = icon;
                _callback(this);
            }
        }

        internal void ClearData()
        {
            searchingForPage = false;
            currentPage = 0;
            _models.Clear();

            customListTableData.tableView.ClearSelection();
            customListTableData.data.Clear();
            customListTableData.tableView.ReloadData();
            customListTableData.tableView.ScrollToCellWithIdx(0, TableView.ScrollPositionType.Beginning, false);
        }

        internal void CellDidSetImage(CustomListTableData.CustomCellInfo cell)
        {
            foreach (var visibleCell in customListTableData.tableView.visibleCells)
            {
                LevelListTableCell levelCell = visibleCell as LevelListTableCell;
                if (ReflectionUtil.GetField<TextMeshProUGUI, LevelListTableCell>(levelCell, "_songNameText")?.text == cell.text)
                {
                    customListTableData.tableView.RefreshCellsContent();
                    break;
                }
            }

            FixAnimatedIcons();
            //customListTableData.tableView.RefreshCellsContent();
        }

        internal void FixAnimatedIcons()
        {
            //overcomplicated way of doing this but god damn
            foreach (var visibleCell in customListTableData.tableView.visibleCells)
            {
                LevelListTableCell levelCell = visibleCell as LevelListTableCell;
                foreach (var dataCell in customListTableData.data)
                {
                    if (ReflectionUtil.GetField<TextMeshProUGUI, LevelListTableCell>(levelCell, "_songNameText")?.text == dataCell.text)
                    {
                        if (dataCell.icon == null) continue;
                        var image = ReflectionUtil.GetField<Image, LevelListTableCell>(levelCell, "_coverImage");

                        if (dataCell.icon.name.EndsWith(".gif"))
                        {
                            var foundAnimation = AnimationController.instance.RegisteredAnimations.FirstOrDefault(x => x.Key == dataCell.icon.name);
                            if (foundAnimation.Value != null)
                            {
                                /*if (foundAnimation.Value.activeImages.Count > 0)
                                {
                                    Plugin.Log.Info("OH NO");
                                }
                                List<Image> newImagelist = new List<Image>();
                                newImagelist.Add(image);
                                image.sprite = foundAnimation.Value.sprites[0];
                                foundAnimation.Value.activeImages = newImagelist;*/
                                foundAnimation.Value.activeImages.Add(image);
                            }
                        }
                        else
                        {
                            image.sprite = dataCell.icon;
                        }
                    }
                }
            }
        }

        internal void DisableDownloadsOnModel(ModelSaberEntry model)
        {
            foreach (var visibleCell in customListTableData.tableView.visibleCells)
            {
                LevelListTableCell levelCell = visibleCell as LevelListTableCell;
                TextMeshProUGUI _songNameText = levelCell.GetField<TextMeshProUGUI, LevelListTableCell>("_songNameText");
                if (_songNameText?.text == model.Name)
                {
                    _songNameText.color = new Color(0.498f, 0.498f, 0.498f);
                    return;
                }
            }
        }

        [UIComponent("warning-text")]
        public CurvedTextMeshPro NameText = null;

        bool ignoringWarnings = false;

        internal void DisplayWarningPromptIfNeeded(ModelSaberEntry model)
        {
            if (ignoringWarnings || _pluginConfig.DisableWarnings)
            {
                return;
            }

            string warningPromptText = model.Type switch
            {
                "bloq" when !_modUtils.CustomNotesInstalled => "Custom Notes",
                "platform" when !_modUtils.CustomPlatformsInstalled => "Custom Platforms",
                "avatar" when !_modUtils.CustomAvatarsInstalled => "Custom Avatars",
                "saber" when !_modUtils.CustomSabersInstalled && !_modUtils.SaberFactoryInstalled => "Saber Factory",
                _ => ""
            };
            if (warningPromptText != "")
            {
                NameText.text = $"This model requires the {warningPromptText} mod. Please install this mod from Mod Assistant or #pc-mods in BSMG to properly use this model.";
                parserParams.EmitEvent("open-warningModal");
            }
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            interactableGroup.gameObject.SetActive(true);
            ModelSaberUtils.ClearCache();

            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
        }

        [UIAction("warningDontShowPressed")]
        internal void WarningDontShowPressed()
        {
            ignoringWarnings = true;
            parserParams.EmitEvent("close-warningModal");
        }

        [UIAction("warningOKPressed")]
        internal void WarningOKPressed()
        {
            parserParams.EmitEvent("close-warningModal");
        }

        // =========================
        // SORT CODE
        // =========================
        [UIComponent("sourceList")]
        public CustomListTableData sourceListTableData = null;

        [UIAction("sortPressed")]
        internal void SortPressed()
        {
            sourceListTableData.tableView.ClearSelection();
        }

        [UIAction("sourceSelect")]
        internal void SelectedSource(TableView tableView, int row)
        {
            parserParams.EmitEvent("close-sourceModal");
            if (currentSort != (ModelsaberSearchSort)row)
            {
                currentSort = (ModelsaberSearchSort)row;
                ClearData();
                GetModelPages(currentPage);
            }
        }

        // =========================
        // SEARCH CODE
        // =========================
        [UIComponent("searchKeyboard")]
        public ModalKeyboard _searchKeyboard = null;

        private string _searchValue = "";

        [UIValue("searchValue")]
        public string SearchValue
        {
            get => _searchValue;
            set
            {
                _searchValue = value;
                NotifyPropertyChanged();
            }
        }

        [UIAction("searchOpened")]
        internal void SearchOpened()
        {
            interactableGroup.gameObject.SetActive(false);
            customListTableData.tableView.ClearSelection();
            // filterDidChange?.Invoke();
        }

        [UIComponent("interactableGroup")]
        VerticalLayoutGroup interactableGroup = null;

        [UIAction("searchPressed")]
        internal void SearchPressed(string text)
        {
            interactableGroup.gameObject.SetActive(true);
            if (text == currentSearch) return;
            currentSearch = text;

            ClearData();
            GetModelPages(currentPage);
            //   Plugin.log.Info("Search Pressed: " + text);
            //_currentSearch = text;
            //_currentFilter = Filters.FilterMode.Search;
            //ClearData();
            //filterDidChange?.Invoke();
            //await GetNewPage(3);
        }

        internal void TagKeyPressed(KEYBOARD.KEY key)
        {
            _searchKeyboard.keyboard.KeyboardText.text = "tag:";
        }
    }
}