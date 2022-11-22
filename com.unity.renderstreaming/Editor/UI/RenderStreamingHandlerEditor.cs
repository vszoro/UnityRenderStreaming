using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.RenderStreaming.Editor.UI
{
    [CustomEditor(typeof(RenderStreamingHandler))]
    internal class RenderStreamingHandlerEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            RenderStreamingHandler handler = serializedObject.targetObject as RenderStreamingHandler;
            if (handler.signalingSettings == null)
            {
                handler.signalingSettings = RenderStreaming.Settings.signalingSettings;
            }

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

                EditorGUILayout.PropertyField(serializedObject.FindProperty("handlers"));
                serializedObject.ApplyModifiedProperties();
            }));

            var signalingSettingsUI = new SignalingSettings();
            signalingSettingsUI.settings = handler.signalingSettings;
            var signalingSettingsType = handler.signalingSettings.GetType();
            var popupField = new SignalingTypePopup("Signaling Type", signalingSettingsType.Name);
            popupField.ChangeEvent += newType =>
            {
                handler.signalingSettings =
                    Activator.CreateInstance(newType) as Unity.RenderStreaming.SignalingSettings;
                signalingSettingsUI.ChangeSignalingType(newType);
            };
            signalingSettingsUI.ChangeSignalingType(signalingSettingsType);
            root.Add(popupField);


            root.Add(signalingSettingsUI);

            return root;
        }
    }
}
