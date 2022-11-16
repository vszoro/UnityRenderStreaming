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
            var popupField = new SignalingTypePopup("Signaling Type", 0);
            popupField.ChangeEvent += newType =>
            {
                serializedObject.FindProperty("handlers");
                signalingSettingsUI.ChangeSignalingType(newType);
            };
            root.Add(popupField);
            root.Add(signalingSettingsUI);

            return root;
        }
    }
}
