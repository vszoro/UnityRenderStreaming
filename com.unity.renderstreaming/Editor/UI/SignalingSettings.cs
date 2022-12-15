using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.RenderStreaming.Editor.UI
{
    internal class SignalingSettings : VisualElement
    {
        const string kTemplatePath = "Packages/com.unity.renderstreaming/Editor/UXML/SignalingSettings.uxml";
        const string kStylePath = "Packages/com.unity.renderstreaming/Editor/Styles/SignalingSettings.uss";

        internal new class UxmlFactory : UxmlFactory<SignalingSettings>
        {
        }

        //todo: change codecs model class
        internal List<ICEServer> sourceList =
            new List<ICEServer> {new ICEServer {urls = new[] {"stun:stun.l.google.com:19302"}}};

        internal ObservableCollection<ICEServer> draft;
        internal VisualElementCache cache;

        private TextField signalingUrlField => cache.Get<TextField>("signalingUrlField");
        private IntegerField iceServerCountField => cache.Get<IntegerField>("iceServerCountField");
        private VisualElement iceServerList => cache.Get<VisualElement>("iceServerList");
        private VisualElement extensionSettingContainer => cache.Get<VisualElement>("extensionSettingContainer");

        public SignalingSettings()
        {
            var styleSheet = EditorGUIUtility.Load(kStylePath) as StyleSheet;
            styleSheets.Add(styleSheet);

            var template = EditorGUIUtility.Load(kTemplatePath) as VisualTreeAsset;
            var newVisualElement = new VisualElement();
            template.CloneTree(newVisualElement);
            this.Add(newVisualElement);

            cache = new VisualElementCache(newVisualElement);
            draft = new ObservableCollection<ICEServer>();

            var signalingSettings = RenderStreaming.Settings.signalingSettings;
            signalingUrlField.value = signalingSettings.urlSignaling;
            signalingUrlField.RegisterCallback<ChangeEvent<string>>(ev =>
            {
                signalingSettings.urlSignaling = ev.newValue;
            });
            iceServerCountField.RegisterCallback<ChangeEvent<int>>(ChangeSize);
        }

        public void ChangeSignalingType(Type newType)
        {
            if (!(Activator.CreateInstance(newType) is Unity.RenderStreaming.SignalingSettings newSettings))
            {
                throw new InvalidOperationException();
            }

            var oldSettings = RenderStreaming.Settings.signalingSettings;
            newSettings.runOnAwake = oldSettings.runOnAwake;
            newSettings.urlSignaling = oldSettings.urlSignaling;
            newSettings.iceServers = oldSettings.iceServers;
            RenderStreaming.Settings.signalingSettings = newSettings;

            extensionSettingContainer.Clear();
            var inspectorType = CustomSignalingSettingsEditor.FindCustomInspectorTypeByType(newType);
            if (inspectorType == null)
            {
                return;
            }

            if (!(Activator.CreateInstance(inspectorType) is ISignalingSettingEditor instance))
            {
                return;
            }

            extensionSettingContainer.Add(instance.ExtendInspectorGUI());
        }


        private void ChangeSize(ChangeEvent<int> evt)
        {
            var diff = evt.newValue - evt.previousValue;
            if (diff == 0)
            {
                return;
            }

            if (diff > 0)
            {
                for (int i = 0; i < diff; i++)
                {
                    draft.Add(new ICEServer());
                    iceServerList.Add(new IceServerSettings(draft.Count));
                }
            }
            else
            {
                for (int i = 0; i > diff; i--)
                {
                    draft.RemoveAt(draft.Count - 1);
                    iceServerList.RemoveAt(iceServerCountField.childCount - 1);
                }
            }
        }
    }
}
