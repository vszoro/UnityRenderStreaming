using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.RenderStreaming.Editor.UI
{
    internal class CodecSettings : VisualElement
    {
        const string kTemplatePath = "Packages/com.unity.renderstreaming/Editor/UXML/CodecSettings.uxml";
        const string kStylePath = "Packages/com.unity.renderstreaming/Editor/Styles/CodecSettings.uss";

        internal new class UxmlFactory : UxmlFactory<CodecSettings>
        {
        }

        //todo: change codecs model class
        internal List<string> sourceList = new List<string> {"VP8", "VP9", "H264", "AV1"};

#if UNITY_2021_3_OR_NEWER
        internal List<string> draft;
#else
        internal ObservableCollection<string> draft;
#endif

        internal VisualElementCache cache;

        public IList<string> CodecSetting => draft;

        /// <summary>
        /// Invoke if change CodecSetting list (Add, Remove, Change order)
        /// </summary>
        public event Action<IEnumerable<string>> onChangeCodecs;

        public CodecSettings()
        {
            var styleSheet = EditorGUIUtility.Load(kStylePath) as StyleSheet;
            styleSheets.Add(styleSheet);

            var template = EditorGUIUtility.Load(kTemplatePath) as VisualTreeAsset;
            var newVisualElement = new VisualElement();
            template.CloneTree(newVisualElement);
            this.Add(newVisualElement);

            cache = new VisualElementCache(newVisualElement);

#if UNITY_2021_3_OR_NEWER
            draft = new List<string>(sourceList);
            Func<VisualElement> makeItem = () =>
            {
                return new Label {style = {unityTextAlign = TextAnchor.MiddleCenter}};
            };
#else
            // workaround for unity 2020.3
            // if unity 2021.3 later, prefer using ListView.itemIndexChanged event
            draft = new ObservableCollection<string>(sourceList);
            draft.CollectionChanged += (sender, args) => NotifyChangeCodecList();
            Func<VisualElement> makeItem = () =>
            {
                var itemRoot = new VisualElement();
                itemRoot.style.flexDirection= FlexDirection.Row;
                var icon = new Image();
                icon.style.alignSelf = Align.Center;
                icon.style.height = 10;
                icon.style.width = 10;
                icon.style.backgroundImage = EditorGUIUtility.Load("align_vertically_center_active") as Texture2D;
                itemRoot.Add(icon);
                var label = new Label {style = {unityTextAlign = TextAnchor.MiddleCenter}};
                itemRoot.Add(label);
                return itemRoot;
            };
 #endif
            Action<VisualElement, int> bindItem = (e, i) =>
            {
                e.contentContainer.Q<Label>().text = draft[i];
            };
            codecList.makeItem = makeItem;
            codecList.bindItem = bindItem;
            codecList.itemsSource = draft;
            codecList.selectionType = SelectionType.Single;
            codecList.itemHeight = 16;
            codecList.style.height = codecList.itemHeight * draft.Count;
            codecList.reorderable = true;
#if UNITY_2021_3_OR_NEWER
            codecList.reorderMode = ListViewReorderMode.Animated;
            codecList.itemIndexChanged += (b, a) => NotifyChangeCodecList();
            codecList.itemsAdded += addItems => NotifyChangeCodecList();
            codecList.itemsRemoved += removeItems => NotifyChangeCodecList();
#endif
            codecList.onSelectionChange += objects =>
            {
                removeCodecButton.SetEnabled(objects.Any() && codecList.itemsSource.Count > 1);
            };

            var contextualMenuManipulator = new ContextualMenuManipulator((evt) =>
            {
                foreach (var item in sourceList)
                {
                    evt.menu.AppendAction($"Add {item}", AddCodec, ValidateCodecStatus, item);
                }
            });
            contextualMenuManipulator.activators.Add(new ManipulatorActivationFilter {button = MouseButton.LeftMouse});
            addCodecButton.AddManipulator(contextualMenuManipulator);

            removeCodecButton.SetEnabled(false);
            removeCodecButton.clickable.clicked += RemoveCodec;
        }

        private void AddCodec(DropdownMenuAction menuAction)
        {
            if (menuAction.userData is string data && !string.IsNullOrEmpty(data))
            {
                draft.Add(data);
            }

            UpdateCodecList();
        }

        private void RemoveCodec()
        {
            foreach (var selectItem in codecList.selectedItems.Cast<string>())
            {
                draft.Remove(selectItem);
            }

            UpdateCodecList();
        }

        private void UpdateCodecList()
        {
            codecList.ClearSelection();
            codecList.style.height = codecList.itemHeight * codecList.itemsSource.Count;
            codecList.Refresh();
        }

        private void NotifyChangeCodecList()
        {
            onChangeCodecs?.Invoke(draft);
        }

        private DropdownMenuAction.Status ValidateCodecStatus(DropdownMenuAction menuAction)
        {
            if (menuAction.userData is string data && !string.IsNullOrEmpty(data))
            {
                return draft.Contains(data) ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal;
            }

            return DropdownMenuAction.Status.Normal;
        }

        private ListView codecList => cache.Get<ListView>("codecList");
        private Button addCodecButton => cache.Get<Button>("addCodecButton");
        private Button removeCodecButton => cache.Get<Button>("removeCodecButton");
    }
}
