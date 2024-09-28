using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Sentis;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static Neutron.Editor.PathUtils;
using OnnxLayer = Unity.Sentis.Layer;

namespace Neutron.Editor {

    public class ModelGraphView : GraphView {

        public new class UxmlFactory : UxmlFactory<ModelGraphView, UxmlTraits> { }

        public bool Horizontal {
            get {
                return Orientation == Orientation.Horizontal;
            }
        }
        public Orientation Orientation = Orientation.Vertical;

        private Model _nnModel;

        private Dictionary<int, NodeView> _nodeMap = new Dictionary<int, NodeView>();
        private List<NodeView> _outputNodes = new List<NodeView>();

        private Graph _graph;
        private HashSet<int> _layerCPUFallback;

        public ModelGraphView() {
            //Insert(0, new GridBackground());

            SetupZoom(0.01f, 2f);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ModelGraphEditorUssPath);
            styleSheets.Add(styleSheet);
        }

        internal void PopulateView(Model nnModel) {
            _nnModel = nnModel;

            DeleteElements(graphElements);
            _nodeMap.Clear();
            _outputNodes.Clear();
            _layerCPUFallback?.Clear();

            CreateGraphView();
        }

        private void CreateGraphView() {
            CreateModelGraph(_nnModel);
            GetCpuFallbackNodes();
            CreateNodes();
            CreateEdges();
            UpdateLayout(_graph);
        }

        /// <summary>
        /// Creates an internal graph representation of all the layer
        /// in the given <see cref="Model"/>
        /// </summary>
        private void CreateModelGraph(Model model) {
            _graph = new Graph(model);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            return ports.Where(endPort =>
            endPort.direction != startPort.direction &&
            endPort.node != startPort.node).ToList();
        }

        private void GetCpuFallbackNodes() {
            using Worker w = new Worker(_nnModel, BackendType.GPUCompute);

            FieldInfo fieldInfo = typeof(Worker).GetField("m_LayerCPUFallback", BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldInfo != null) {
                // Access the value of the field from the model instance
                _layerCPUFallback = new HashSet<int>((HashSet<int>)fieldInfo.GetValue(w));
            } else {
                _layerCPUFallback = new HashSet<int>();
            }
        }

        private void CreateNodes() {
            // create input node
            foreach (var input in _nnModel.inputs) {
                CreateRootNode(input.index);
                //Debug.Log(input.name);
            }

            // create layer node
            Vector2 pos = Vector2.zero;
            foreach (OnnxLayer layer in _nnModel.layers) {
                NodeView node = CreateLayerNode(layer);
            }

            // get output node
            foreach (var output in _nnModel.outputs) {
                NodeView n = _nodeMap[output.index];
                _outputNodes.Add(n);
            }
        }

        private void CreateEdges() {
            foreach (OnnxLayer layer in _nnModel.layers) {

                NodeView node = _nodeMap[layer.outputs[0]];

                // connect inputs
                for (int i = 0; i < layer.inputs.Length; i++) {
                int input = layer.inputs[i];
                    if (!_nodeMap.TryGetValue(input, out NodeView inputNode)) {
                        //Debug.Log($"Skip const input: {input}");
                        continue;
                    }

                    //Edge edge = inputNode.Outputs.ConnectTo(node.Inputs);
                    Edge edge = node.Inputs.ConnectTo(inputNode.Outputs);
                    // why does setting capabilities not work?
                    //edge.capabilities = 0;
                    edge.SetEnabled(false);
                    AddElement(edge);
                }
            }
        }


        private void UpdateLayout(Graph graph) {
            int parallelAxis = Horizontal ? 1 : 0;
            int sequentialAxis = 1 - parallelAxis;
            const float parallelSpacing = 180;
            const float sequentialSpacing = 120;
            Vector2 pos = Vector2.zero;
            float currentY = 0f;

            int i = 0;
            float tmpXOffset = 0f;

            foreach (var layer in graph.Layers) {
                float layerHeight = layer.Count * sequentialSpacing;
                float startY = currentY - layerHeight / 2f;
                float endY = startY + layerHeight;
                float currentX = 0f;

                foreach (var node in layer) {
                    tmpXOffset -= parallelSpacing * _graph.GetParentCount(node);
                    var nodeView = _nodeMap[node.outputs[0]];
                    pos[parallelAxis] = currentX + tmpXOffset;
                    pos[sequentialAxis] = (startY + endY) / 2f;
                    nodeView.Position = pos;
                    currentX += parallelSpacing;
                    tmpXOffset += parallelSpacing * _graph.GetChildCount(node);
                }

                i++;
                currentY += sequentialSpacing;
            }
        }

        private NodeView CreateLayerNode(OnnxLayer layer) {
            NodeView node = new NodeView(layer, Orientation);
            node.title = _layerCPUFallback.Contains(layer.outputs[0]) ? $"{node.title} (CPU)" : node.title;
            node.CPUFallback = _layerCPUFallback.Contains(layer.outputs[0]);
            AddElement(node);
            node.capabilities = Capabilities.Selectable | Capabilities.Movable;
            _nodeMap.Add(layer.outputs[0], node);
            return node;
        }

        private NodeView CreateRootNode(int id) {
            NodeView node = new NodeView(name, Orientation);
            AddElement(node);
            _nodeMap.Add(id, node);
            return node;
        }
    }
}