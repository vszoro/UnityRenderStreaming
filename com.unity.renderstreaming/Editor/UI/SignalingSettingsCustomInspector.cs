using UnityEngine.UIElements;

namespace Unity.RenderStreaming.Editor.UI
{
    public interface ISignalingSettingEditor
    {
        VisualElement ExtendInspectorGUI();
        void SetSignalingSettings(Unity.RenderStreaming.SignalingSettings settings);
    }

    [CustomSignalingSettingsEditor(typeof(HttpSignalingSettings))]
    internal class HttpSignalingSettingsEditor : ISignalingSettingEditor
    {
        public VisualElement ExtendInspectorGUI()
        {
            return new TextField("Interval");
        }

        public void SetSignalingSettings(Unity.RenderStreaming.SignalingSettings settings)
        {
            throw new System.NotImplementedException();
        }
    }
}
