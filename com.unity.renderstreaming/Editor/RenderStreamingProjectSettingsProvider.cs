using System.Collections.Generic;
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

        const string kSettingsPath = "Project/Render Streaming";
        const string kTemplatePath = "Packages/com.unity.renderstreaming/Editor/UXML/RenderStreamingProjectSettings.uxml";
        const string kStylePath = "Packages/com.unity.renderstreaming/Editor/Styles/RenderStreamingProjectSettings.uss";

        private VisualElementCache cache;
        private ObjectField renderStreamingSettingsField => cache.Get<ObjectField>("renderStreamingSettingsField");
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
                RenderStreaming.Settings = (RenderStreamingSettings)ev.newValue;
                this.Repaint();
            });

            automaticStreaming.value = RenderStreaming.Settings.automaticStreaming;
            automaticStreaming.RegisterCallback<ChangeEvent<bool>>(ev =>
            {
                RenderStreaming.Settings.automaticStreaming = ev.newValue;
            });

            var signalingSettingsType = RenderStreaming.Settings.signalingSettings.GetType();
            var popupField = new SignalingTypePopup("Signaling Type", signalingSettingsType.Name);
            popupField.ChangeEvent += newType => signalingSettings.ChangeSignalingType(newType);
            signalingSettings.ChangeSignalingType(signalingSettingsType);
            signalingTypeContainer.Add(popupField);
        }

        public RenderStreamingProjectSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
            : base(path, scopes, keywords)
        {
        }
    }
}
