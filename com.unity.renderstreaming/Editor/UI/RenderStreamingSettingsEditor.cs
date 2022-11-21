using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.RenderStreaming.Editor.UI
{
    [CustomEditor(typeof(RenderStreamingSettings))]
    internal class RenderStreamingSettingsEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            RenderStreamingSettings settings = serializedObject.targetObject as RenderStreamingSettings;

            // var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/SampleUI.uxml");
            // visualTree.CloneTree(root);

            // todo: use listview visual element
            root.Add(new IMGUIContainer(() =>
            {
                serializedObject.Update();
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    SerializedProperty prop = serializedObject.FindProperty("m_Script");
                    EditorGUILayout.PropertyField(prop, true, Array.Empty<GUILayoutOption>());
                }

                serializedObject.ApplyModifiedProperties();
            }));

            var signalingSettings = new SignalingSettings();
            var signalingSettingsType = RenderStreaming.Settings.signalingSettings.GetType();
            var popupField = new SignalingTypePopup("Signaling Type", signalingSettingsType.Name);
            popupField.ChangeEvent += newType =>
            {
                settings.signalingSettings = Activator.CreateInstance(newType) as Unity.RenderStreaming.SignalingSettings;
                signalingSettings.ChangeSignalingType(newType);
            };
            signalingSettings.ChangeSignalingType(signalingSettingsType);
            root.Add(popupField);
            root.Add(signalingSettings);

            return root;
        }
    }
}
