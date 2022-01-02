using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Loader;
using ModelDownloader.Types;
using ModelDownloader.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace ModelDownloader.Settings.UI
{
    [HotReload(RelativePathToLayout = @"./Views/modelPreview.bsml")]
    [ViewDefinition("ModelDownloader.Settings.UI.Views.modelPreview.bsml")]
    internal class ModelPreviewViewController : BSMLAutomaticViewController
    {
        private GameplaySetupViewController _gameplaySetupViewController = null!;
        private DownloadUtils _downloadUtils = null!;
        private ModUtils _modUtils = null!;

        private ModelSaberEntry _model;
        private GameObject _previewHolder;
        private AssetBundle _bundle;

        [UIComponent("loading-text")]
        public CurvedTextMeshPro LoadingText = null;

        [Inject]
        internal void Construct(GameplaySetupViewController gameplaySetupViewController, DownloadUtils downloadUtils, ModUtils modUtils)
        {
            _gameplaySetupViewController = gameplaySetupViewController;
            _downloadUtils = downloadUtils;
            _modUtils = modUtils;
        }

        internal void ClearData()
        {
            if (LoadingText != null)
            {
                LoadingText.text = "";
            }

            if (_previewHolder != null)
            {
                Destroy(_previewHolder);
                _previewHolder = null;
            }

            if (_bundle != null)
            {
                _bundle.Unload(true);
            }
        }

        internal async void CreatePreview(ModelSaberEntry model)
        {
            ClearData();
            LoadingText.text = "Loading Preview...";
            _model = model;
            _previewHolder = new GameObject("ModelPreviewHolder");
            _previewHolder.transform.parent = null;
            _previewHolder.transform.position = Vector3.zero;
            _previewHolder.transform.localScale = Vector3.one;
            _previewHolder.transform.rotation = Quaternion.identity;

            AssetBundle? bundle = await _downloadUtils.DownloadModelAsPreview(model);
            if (bundle == null)
            {
                return;
            }

            _bundle = bundle;
            if (model.Type == "saber")
            {
                GameObject saber = bundle.LoadAsset<GameObject>("_CustomSaber");
                CreateSaberPreview(saber);
            }
            else if (model.Type == "bloq")
            {
                GameObject notes = bundle.LoadAsset<GameObject>("assets/_customnote.prefab");
                CreateNotePreview(notes);
            }

            LoadingText.text = "";
        }

        // SABER PREVIEW UTILS
        private void CreateSaberPreview(GameObject saber)
        {
            _previewHolder.transform.position = new Vector3(3.0f, 1.3f, 1.0f);
            _previewHolder.transform.Rotate(0.0f, 330.0f, 0.0f);

            Vector3 sabersPos = new Vector3(0, 0, 0);
            Vector3 saberLeftPos = new Vector3(0, 0, 0);
            Vector3 saberRightPos = new Vector3(0, 0.5f, 0);

            var previewSabers = CreatePreviewSaber(saber, _previewHolder.transform, sabersPos);
            PositionPreviewSaber(saberLeftPos, previewSabers?.transform.Find("LeftSaber").gameObject);
            PositionPreviewSaber(saberRightPos, previewSabers?.transform.Find("RightSaber").gameObject);

            previewSabers?.transform.Find("LeftSaber").gameObject.SetActive(true);
            ColorizeSaber(previewSabers?.transform.Find("LeftSaber").gameObject, _gameplaySetupViewController.colorSchemesSettings.GetSelectedColorScheme().saberAColor);
            //previewSabers?.transform.Find("LeftSaber").gameObject.gameObject.AddComponent<DummySaber>();
            previewSabers?.transform.Find("RightSaber").gameObject.SetActive(true);
            ColorizeSaber(previewSabers?.transform.Find("RightSaber").gameObject, _gameplaySetupViewController.colorSchemesSettings.GetSelectedColorScheme().saberBColor);
            //previewSabers?.transform.Find("RightSaber").gameObject.gameObject.AddComponent<DummySaber>();
        }

        private GameObject CreatePreviewSaber(GameObject saber, Transform transform, Vector3 localPosition)
        {
            if (!saber) return null;
            var saberObject = InstantiateGameObject(saber, transform);
            saberObject.name = "Preview Saber Object";
            PositionPreviewSaber(localPosition, saberObject);
            return saberObject;
        }

        private void PositionPreviewSaber(Vector3 vector, GameObject saberObject)
        {
            if (saberObject && vector != null)
            {
                saberObject.transform.localPosition = vector;
            }
        }

        private void ColorizeSaber(GameObject saber, Color color)
        {
            foreach (var r in saber.GetComponentsInChildren<Renderer>())
            {
                foreach (var renderMaterial in r.materials)
                {
                    if (renderMaterial.HasProperty("_CustomColors"))
                    {
                        if (renderMaterial.GetFloat("_CustomColors") > 0)
                            renderMaterial.SetColor("_Color", color);
                    }
                    else if (renderMaterial.HasProperty("_Glow") && renderMaterial.GetFloat("_Glow") > 0
                             || renderMaterial.HasProperty("_Bloom") && renderMaterial.GetFloat("_Bloom") > 0)
                    {
                        renderMaterial.SetColor("_Color", color);
                    }
                }
            }
        }

        // NOTE PREVIEW UTILS
        private void CreateNotePreview(GameObject note)
        {
            Vector3 leftDotPos = new Vector3(0.0f, 1.5f, 0.0f);
            Vector3 leftArrowPos = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 rightDotPos = new Vector3(1.5f, 1.5f, 0.0f);
            Vector3 rightArrowPos = new Vector3(1.5f, 0.0f, 0.0f);
            Vector3 bombPos = new Vector3(3.0f, 0.75f, 0.0f);

            GameObject NoteLeft = note.transform.Find("NoteLeft").gameObject;
            GameObject NoteRight = note.transform.Find("NoteRight").gameObject;
            Transform NoteDotLeftTransform = note.transform.Find("NoteDotLeft");
            Transform NoteDotRightTransform = note.transform.Find("NoteDotRight");
            GameObject NoteDotLeft = NoteDotLeftTransform != null ? NoteDotLeftTransform.gameObject : NoteLeft;
            GameObject NoteDotRight = NoteDotRightTransform != null ? NoteDotRightTransform.gameObject : NoteRight;
            GameObject NoteBomb = note.transform.Find("NoteBomb")?.gameObject;
            _previewHolder.transform.Rotate(0.0f, 60.0f, 0.0f);
            _previewHolder.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

            _previewHolder.transform.position = NoteBomb ? new Vector3(3.05f, 0.9f, 2.0f) : new Vector3(2.90f, 0.9f, 1.85f);

            GameObject noteLeft = CreatePreviewNote(NoteLeft, transform, leftArrowPos);
            GameObject noteDotLeft = CreatePreviewNote(NoteDotLeft, transform, leftDotPos);
            GameObject noteRight = CreatePreviewNote(NoteRight, transform, rightArrowPos);
            GameObject noteDotRight = CreatePreviewNote(NoteDotRight, transform, rightDotPos);
            GameObject noteBomb = CreatePreviewNote(NoteBomb, transform, bombPos);

            ColorizeCustomNote(_gameplaySetupViewController.colorSchemesSettings.GetSelectedColorScheme().saberAColor, 1, noteLeft);
            ColorizeCustomNote(_gameplaySetupViewController.colorSchemesSettings.GetSelectedColorScheme().saberAColor, 1, noteDotLeft);
            ColorizeCustomNote(_gameplaySetupViewController.colorSchemesSettings.GetSelectedColorScheme().saberBColor, 1, noteRight);
            ColorizeCustomNote(_gameplaySetupViewController.colorSchemesSettings.GetSelectedColorScheme().saberBColor, 1, noteDotRight);
            // todo fake arrows
        }

        private GameObject CreatePreviewNote(GameObject note, Transform transform, Vector3 localPosition)
        {
            GameObject noteObject = InstantiateGameObject(note, transform);
            PositionPreviewNote(localPosition, noteObject);
            return noteObject;
        }

        private GameObject InstantiateGameObject(GameObject gameObject, Transform transform = null)
        {
            if (gameObject)
            {
                return transform ? Instantiate(gameObject, transform) : Instantiate(gameObject);
            }

            return null;
        }

        private void PositionPreviewNote(Vector3 vector, GameObject noteObject)
        {
            if (noteObject && vector != null)
            {
                noteObject.transform.SetParent(_previewHolder.transform);
                noteObject.transform.localPosition = vector;
                noteObject.transform.localScale = new Vector3(1, 1, 1);
                noteObject.transform.Rotate(0.0f, 60f, 0.0f);
                //noteObject.transform.localRotation = Quaternion.identity;
            }
        }

        public void ColorizeCustomNote(Color color, float colorStrength, GameObject noteObject)
        {
            Type disableNoteColorType = null;
            if (_modUtils.CustomNotesInstalled)
            {
                disableNoteColorType = PluginManager.EnabledPlugins.Where(x => x.Name == "CustomNotes").First().Assembly.GetType("CustomNotes.DisableNoteColorOnGameobject");
            }

            if (!noteObject || color == null)
            {
                return;
            }

            Color noteColor = color * colorStrength;

            IEnumerable<Transform> childTransforms = noteObject.GetComponentsInChildren<Transform>(true);
            foreach (Transform childTransform in childTransforms)
            {
                bool colorDisabled = false;

                if (disableNoteColorType != null)
                {
                    colorDisabled = childTransform.GetComponent(disableNoteColorType);
                }

                if (!colorDisabled)
                {
                    Renderer childRenderer = childTransform.GetComponent<Renderer>();
                    if (childRenderer)
                    {
                        childRenderer.material.SetColor("_Color", noteColor);
                    }
                }
            }
        }


        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            ClearData();
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
        }
    }
}