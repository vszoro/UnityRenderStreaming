using System.Collections.Generic;
using System.Linq;
using Unity.RenderStreaming.Editor;
using Unity.RenderStreaming.Editor.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.RenderStreaming
{
    internal class RenderStreamingProjectSettingsProvider : SettingsProvider
    {
        internal VisualElement rootVisualElement { get; private set; }

        public const string kEditorBuildSettingsConfigKey = "com.unity.renderstreaming.settings";
        const string kSettingsPath = "Project/Render Streaming";
        const string kTemplatePath = "Packages/com.unity.renderstreaming/Editor/UXML/RenderStreamingProjectSettings.uxml";
        const string kStylePath = "Packages/com.unity.renderstreaming/Editor/Styles/RenderStreamingProjectSettings.uss";

        private VisualElementCache cache;
        private ObjectField renderStreamingSettingsField => cache.Get<ObjectField>("renderStreamingSettingsField");
        private HelpBox createAssetHelpBox => cache.Get<HelpBox>("createAssetHelpBox");
        private Button createAssetButton => cache.Get<Button>("createAssetButton");
        private VisualElement settingsPropertyContainer => cache.Get<VisualElement>("settingsPropertyContainer");
        private Toggle automaticStreaming => cache.Get<Toggle>("automaticStreaming");
        private VisualElement signalingTypeContainer => cache.Get<VisualElement>("signalingTypeContainer");
        private Editor.UI.SignalingSettings signalingSettings => cache.Get<Editor.UI.SignalingSettings>("signalingSettings");

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new RenderStreamingProjectSettingsProvider(kSettingsPath, SettingsScope.Project, new List<string>()
            {
                L10n.Tr("experimental"),
                L10n.Tr("streaming"),
                L10n.Tr("webrtc"),
                L10n.Tr("video"),
                L10n.Tr("audio"),
            });
        }

        private RenderStreamingSettings settings;
        private int settingDirtyCount;
        private string[] availableRenderStreamingSettingsAssets;
        private int currentSelectedInputSettingsAsset;

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            var styleSheet = EditorGUIUtility.Load(kStylePath) as StyleSheet;

            rootVisualElement = new ScrollView();
            rootVisualElement.StretchToParentSize();
            rootVisualElement.styleSheets.Add(styleSheet);
            rootElement.Add(rootVisualElement);

            var template = EditorGUIUtility.Load(kTemplatePath) as VisualTreeAsset;

            VisualElement newVisualElement = new VisualElement();
            template.CloneTree(newVisualElement);
            rootVisualElement.Add(newVisualElement);

            cache = new VisualElementCache(newVisualElement);

            renderStreamingSettingsField.RegisterCallback<ChangeEvent<Object>>(ev =>
            {
                settings = ev.newValue as RenderStreamingSettings;
                settingsPropertyContainer.SetEnabled(settings != null);
                InitializeWithCurrentSettingsIfNecessary();
            });

            InitializeSettingPropertyElement();
            InitializeWithCurrentSettingsIfNecessary();

            if (availableRenderStreamingSettingsAssets.Length == 0)
            {
                createAssetHelpBox.style.display = DisplayStyle.Flex;
                createAssetButton.style.display = DisplayStyle.Flex;
                createAssetButton.clicked += () =>
                {
                    CreateNewSettingsAsset("Assets/RenderStreamingSettings.asset");
                    Repaint();
                };
            }
            else
            {
                createAssetHelpBox.style.display = DisplayStyle.None;
                createAssetButton.style.display = DisplayStyle.None;
                renderStreamingSettingsField.value = settings;
            }
            settingsPropertyContainer.SetEnabled(availableRenderStreamingSettingsAssets.Length != 0);
        }

        private void InitializeSettingPropertyElement()
        {
            automaticStreaming.value = RenderStreaming.Settings.automaticStreaming;
            automaticStreaming.RegisterCallback<ChangeEvent<bool>>(ev =>
            {
                settings.automaticStreaming = ev.newValue;
            });

            signalingSettings.settings = RenderStreaming.Settings.signalingSettings;
            var signalingSettingsType = RenderStreaming.Settings.signalingSettings.GetType();
            var popupField = new SignalingTypePopup("Signaling Type", signalingSettingsType.Name);
            popupField.ChangeEvent += newType => signalingSettings.ChangeSignalingType(newType);
            signalingSettings.ChangeSignalingType(signalingSettingsType);
            signalingTypeContainer.Add(popupField);
        }

        private void InitializeWithCurrentSettingsIfNecessary()
        {
            if (RenderStreaming.Settings == settings && settings != null && settingDirtyCount == EditorUtility.GetDirtyCount(settings))
                return;

            InitializeWithCurrentSettings();
        }

        private void InitializeWithCurrentSettings()
        {
            // Find the set of available assets in the project.
            availableRenderStreamingSettingsAssets = FindRenderStreamingSettingsInProject();

            // See which is the active one.
            settings = RenderStreaming.Settings;
            settingDirtyCount = EditorUtility.GetDirtyCount(settings);
            var currentSettingsPath = AssetDatabase.GetAssetPath(settings);
            if (string.IsNullOrEmpty(currentSettingsPath))
            {
                if (availableRenderStreamingSettingsAssets.Length != 0)
                {
                    currentSelectedInputSettingsAsset = 0;
                    settings = AssetDatabase.LoadAssetAtPath<RenderStreamingSettings>(availableRenderStreamingSettingsAssets[0]);
                    RenderStreaming.Settings = settings;
                }
            }
            else
            {
                currentSelectedInputSettingsAsset = ArrayHelpers.IndexOf(availableRenderStreamingSettingsAssets, currentSettingsPath);
                if (currentSelectedInputSettingsAsset == -1)
                {
                    // This is odd and shouldn't happen. Solve by just adding the path to the list.
                    currentSelectedInputSettingsAsset =
                        ArrayHelpers.Append(ref availableRenderStreamingSettingsAssets, currentSettingsPath);
                }

                ////REVIEW: should we store this by platform?
                EditorBuildSettings.AddConfigObject(kEditorBuildSettingsConfigKey, settings, true);
            }
        }

        private static string[] FindRenderStreamingSettingsInProject()
        {
            var guids = AssetDatabase.FindAssets("t:RenderStreamingSettings");
            return guids.Select(AssetDatabase.GUIDToAssetPath).ToArray();
        }

        private static void CreateNewSettingsAsset(string relativePath)
        {
            // Create settings file.
            var settings = ScriptableObject.CreateInstance<RenderStreamingSettings>();
            AssetDatabase.CreateAsset(settings, relativePath);
            EditorGUIUtility.PingObject(settings);
            // Install the settings. This will lead to an InputSystem.onSettingsChange event which in turn
            // will cause us to re-initialize.
            RenderStreaming.Settings = settings;
        }

        public RenderStreamingProjectSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
            : base(path, scopes, keywords)
        {
        }
    }
}
