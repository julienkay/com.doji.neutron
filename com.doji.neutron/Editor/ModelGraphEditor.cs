using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Sentis;
using static Neutron.Editor.PathUtils;

namespace Neutron.Editor {

    public class ModelGraphEditor : EditorWindow {

        private ModelGraphView _graphView;
        private InspectorView _inspectorView;

        [MenuItem("Window/Neutron")]
        public static void OpenWindow() {
            ModelGraphEditor wnd = GetWindow<ModelGraphEditor>();
            wnd.titleContent = new GUIContent("Neutron");
        }

        public void CreateGUI() {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;
             
            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ModelGraphEditorUxmlPath);
            visualTree.CloneTree(root);

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ModelGraphEditorUssPath);
            root.styleSheets.Add(styleSheet);

            _graphView = root.Q<ModelGraphView>();
            _inspectorView = root.Q<InspectorView>();

            OnSelectionChange();
        }

        private void OnSelectionChange() {
            ModelAsset nnModel = Selection.activeObject as ModelAsset;
            if (nnModel) {
                Model model = ModelLoader.Load(nnModel);
                _graphView.PopulateView(model);
                _graphView.FrameOrigin();
            }
        }
    }
}