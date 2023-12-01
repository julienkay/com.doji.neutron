using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Barracuda;

namespace Neutron.Editor {

    public class ModelGraphEditor : EditorWindow {

        private ModelGraphView _graphView;
        private InspectorView _inspectorView;

        [MenuItem("Neutron/ModelGraphEditor")]
        public static void OpenWindow() {
            ModelGraphEditor wnd = GetWindow<ModelGraphEditor>();
            wnd.titleContent = new GUIContent("ModelGraphEditor");
        }

        public void CreateGUI() {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.doji.unitron/Editor/ModelGraphEditor.uxml");
            visualTree.CloneTree(root);

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.doji.unitron/Editor/ModelGraphEditor.uss");
            root.styleSheets.Add(styleSheet);

            _graphView = root.Q<ModelGraphView>();
            _inspectorView = root.Q<InspectorView>();

            OnSelectionChange();
        }

        private void OnSelectionChange() {
            NNModel nnModel = Selection.activeObject as NNModel;
            if (nnModel) {
                Model model = ModelLoader.Load(nnModel);
                _graphView.PopulateView(model);
            }
        }
    }
}