using System;
using Unity.RenderStreaming;
using Unity.RenderStreaming.Editor.UI;
using Unity.RenderStreaming.Signaling;
using UnityEngine.UIElements;

namespace Prototype
{
    public class TestSignalingSettings : SignalingSettings
    {
        public string username;
        public string apitoken;
        public override Type signalingClass => typeof(WebSocketSignaling);
    }

    [CustomSignalingSettingsEditor(typeof(TestSignalingSettings))]
    public class TestSignalingSettingsEditor : ISignalingSettingEditor
    {
        public VisualElement ExtendInspectorGUI()
        {
            var root = new VisualElement();
            root.Add(new TextField("User Name"));
            root.Add(new TextField("Api Token"));
            return root;
        }

        public void SetSignalingSettings(SignalingSettings settings)
        {
            throw new System.NotImplementedException();
        }
    }
}
