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

        private Unity.RenderStreaming.SignalingSettings m_settings;

        internal Unity.RenderStreaming.SignalingSettings settings
        {
            get => m_settings;
            set
            {
                m_settings = value;
                ApplySettings();
            }
        }

        private ObservableCollection<ICEServer> draft;
        private VisualElementCache cache;

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

            signalingUrlField.RegisterCallback<ChangeEvent<string>>(ev =>
            {
                if (settings != null)
                {
                    settings.urlSignaling = ev.newValue;
                }
            });

            iceServerCountField.RegisterCallback<ChangeEvent<int>>(ChangeSize);
        }

        private void ApplySettings()
        {
            signalingUrlField.value = settings.urlSignaling;
            draft.Clear();
            foreach (var iceServer in settings.iceServers)
            {
                draft.Add(iceServer);
            }
            iceServerCountField.value = settings.iceServers.Length;
        }

        public void ChangeSignalingType(Type newType)
        {
            if (!(Activator.CreateInstance(newType) is Unity.RenderStreaming.SignalingSettings newSettings))
            {
                throw new InvalidOperationException();
            }

            var oldSettings = m_settings;
            newSettings.runOnAwake = oldSettings.runOnAwake;
            newSettings.urlSignaling = oldSettings.urlSignaling;
            newSettings.iceServers = oldSettings.iceServers;
            m_settings = newSettings;

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
                    var addIce = i >= draft.Count;
                    var iceServer =  addIce ? new ICEServer() : draft[i];
                    if (addIce)
                    {
                        draft.Add(iceServer);
                    }

                    var iceServerSettings = new IceServerSettings(draft.Count);
                    iceServerSettings.iceServer = iceServer;
                    iceServerList.Add(iceServerSettings);
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
