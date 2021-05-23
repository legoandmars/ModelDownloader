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
    internal class ModelListViewController : BSMLResourceViewController
    {
        public override string ResourceName => "ModelDownloader.Settings.UI.Views.modelList.bsml";

        public LoadingControl loadingSpinner;
        public int currentPage = 0;
        public ModelsaberSearchSort currentSort = 0;
        public string currentSearch = "";
        public bool searchingForPage = false;

        private List<ModelsaberEntry> _models = new List<ModelsaberEntry>();

        private static KawaseBlurRendererSO _kawaseBlurRenderer;

        private static PluginConfig _pluginConfig;

        [UIParams]
        internal BeatSaberMarkupLanguage.Parser.BSMLParserParams parserParams;

        [UIValue("model-type-options")]
        private List<object> modelTypeOptions = new object[] { "All Models", "Sabers", "Bloqs", "Platforms", "Avatars" }.ToList();

        [UIValue("model-type-choice")]
        private string modelTypeChoice = "All Models";

        [UIComponent("list")]
        public CustomListTableData customListTableData;

        [UIComponent("loadingModal")]
        public ModalView loadingModal;

        public Action<ModelsaberEntry, Sprite> didSelectModel;

        [UIAction("listSelect")]
        internal void Select(TableView tableView, int row)
        {
            didSelectModel?.Invoke(_models[row], customListTableData.data[row].icon);
        }
        [Inject]
        protected void Construct(LevelPackDetailViewController levelPackDetailViewController, PluginConfig pluginConfig)
        {
            _kawaseBlurRenderer = levelPackDetailViewController.GetField<KawaseBlurRendererSO, LevelPackDetailViewController>("_kawaseBlurRenderer");
            _pluginConfig = pluginConfig;
        }

        [UIAction("#post-parse")]
        public void SetupList()
        {
            (transform as RectTransform).sizeDelta = new Vector2(70, 0);
            (transform as RectTransform).anchorMin = new Vector2(0.5f, 0);
            (transform as RectTransform).anchorMax = new Vector2(0.5f, 1);

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
            ModelsaberSearch searchOptions = new ModelsaberSearch((ModelsaberSearchType)modelTypeOptions.IndexOf(modelTypeChoice), page, currentSort, currentSearch);
            List<ModelsaberEntry> entries = await ModelsaberUtils.GetPage(searchOptions);
            if (!searchingForPage) return;
            foreach (ModelsaberEntry entry in entries)
            {
                _models.Add(entry);
                if (DownloadUtils.CheckIfModelInstalled(entry)) customListTableData.data.Add(new ModelCellInfo(entry, CellDidSetImage, $"<#7F7F7F>{entry.Name}", entry.Author));
                else customListTableData.data.Add(new ModelCellInfo(entry, CellDidSetImage, entry.Name, entry.Author));
            }
            SetLoading(false);

            customListTableData.tableView.ReloadData();

            int pageScrollAmount = (page * searchOptions.PageLength) - 1;
            if (pageScrollAmount > customListTableData.data.Count) pageScrollAmount = customListTableData.data.Count;
            if (pageScrollAmount < 0) pageScrollAmount = 0;

            customListTableData.tableView.ScrollToCellWithIdx(pageScrollAmount, TableView.ScrollPositionType.Beginning, false);
            searchingForPage = false;
        }

        public class ModelCellInfo : CustomListTableData.CustomCellInfo
        {
            protected ModelsaberEntry _model;
            protected Action<CustomListTableData.CustomCellInfo> _callback;
            public ModelCellInfo(ModelsaberEntry model, Action<CustomListTableData.CustomCellInfo> callback, string text, string subtext = null) : base(text, subtext, null)
            {
                _model = model;
                _callback = callback;
                LoadImage();
            }
            protected async void LoadImage()
            {
                byte[] image = await _model.GetCoverImageBytes();
                Sprite icon = null;
                if(_model.Tags.Where(x => x.ToLower() == "nsfw").Count() > 0 && _pluginConfig.BlurNSFWImages)
                {
                    // nsfw image, blur it.
                    icon = SpriteUtils.LoadSpriteFromTexture(_kawaseBlurRenderer.Blur(SpriteUtils.LoadTextureRaw(image), KawaseBlurRendererSO.KernelSize.Kernel35, 2));
                }
                else icon = SpriteUtils.LoadSpriteRaw(image);

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
                    return;
                }
            }
        }

        internal void DisableDownloadsOnModel(ModelsaberEntry model)
        {
            foreach (var visibleCell in customListTableData.tableView.visibleCells)
            {
                LevelListTableCell levelCell = visibleCell as LevelListTableCell;
                TextMeshProUGUI _songNameText = ReflectionUtil.GetField<TextMeshProUGUI, LevelListTableCell>(levelCell, "_songNameText");
                if (_songNameText?.text == model.Name)
                {
                    _songNameText.color = new Color(0.498f, 0.498f, 0.498f);
                    return;
                }
            }
        }

        [UIComponent("warning-text")]
        public CurvedTextMeshPro NameText;

        bool ignoringWarnings = false;
        internal void DisplayWarningPromptIfNeeded(ModelsaberEntry model)
        {
            if (ignoringWarnings || _pluginConfig.DisableWarnings) return;
            string warningPromptText = "";
            if (model.Type == "bloq" && !ModUtils.CustomNotesInstalled) warningPromptText = "Custom Notes";
            else if (model.Type == "platform" && !ModUtils.CustomPlatformsInstalled) warningPromptText = "Custom Platforms";
            else if (model.Type == "avatar" && !ModUtils.CustomAvatarsInstalled) warningPromptText = "Custom Avatars";
            else if (model.Type == "saber" && !ModUtils.CustomSabersInstalled && !ModUtils.SaberFactoryInstalled) warningPromptText = "Saber Factory";
            if(warningPromptText != "")
            {
                NameText.text = $"This model requires the {warningPromptText} mod. Please install this mod from Mod Assistant or #pc-mods in BSMG to properly use this model.";
                parserParams.EmitEvent("open-warningModal");
            }
        }
        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            interactableGroup.gameObject.SetActive(true);
            ModelsaberUtils.ClearCache();

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
        public CustomListTableData sourceListTableData;

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
        public ModalKeyboard _searchKeyboard;

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
        VerticalLayoutGroup interactableGroup;

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
