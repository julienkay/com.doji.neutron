using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Barracuda;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static Neutron.Editor.PathUtils;

namespace Neutron.Editor {

    public class ModelGraphView : GraphView {

        public new class UxmlFactory : UxmlFactory<ModelGraphView, UxmlTraits> { }

        public bool Horizontal {
            get {
                return Orientation == Orientation.Horizontal;
            }
        }
        public Orientation Orientation = Orientation.Horizontal;

        private Model _nnModel;

        private Dictionary<string, NodeView> _nodeMap = new Dictionary<string, NodeView>();
        private List<NodeView> _outputNodes = new List<NodeView>();

        public ModelGraphView() {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());


            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.doji.unitron/Editor/ModelGraphEditor.uss");
            styleSheets.Add(styleSheet);
        }

        /// <summary>
        /// Creates an insternal graph representation of all the layers
        /// in the given <see cref="Model"/>
        /// </summary>
        private void CreateModelGraph(Model model) {

        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            return ports.Where(endPort =>
            endPort.direction != startPort.direction &&
            endPort.node != startPort.node).ToList();
        }

        internal void PopulateView(Model nnModel) {
            _nnModel = nnModel;

            DeleteElements(graphElements);
            _nodeMap.Clear();

            CreateGraphView();
        }

        private void CreateGraphView() {
            CreateNodes();
            CreateEdges();
            UpdateLayout();
        }

        private void CreateNodes() {
            // create input node
            foreach (var input in _nnModel.inputs) {
                CreateRootNode(input.name);
            }

            //create layer nodes
            Vector2 pos = Vector2.zero;
            foreach (var layer in _nnModel.layers) {
                NodeView node = CreateLayerNode(layer);
                //node.Position = pos;
                if (layer.inputs != null) {
                    Debug.Log(layer.name + " inputs: " + string.Join("][", layer.inputs));
                }
                if (layer.outputs != null) {
                    Debug.Log(layer.name + " outputs: " + string.Join("][", layer.outputs));
                }

                //int i = Horizontal ? 0 : 1;
                //Vector2 offset = new Vector2(0f, 0f);
                //offset[i] = 180;
                //pos += offset;
            }

            // get output nodes
            foreach (string output in _nnModel.outputs) {
                NodeView n = _nodeMap[output];
                _outputNodes.Add(n);
            }
        }

        private void CreateEdges() {
            foreach (var layer in _nnModel.layers) {
                NodeView node = _nodeMap[layer.name];

                // connect inputs
                foreach(string input in layer.inputs) {
                    NodeView inNode = _nodeMap[input];
                    Edge edge = inNode.Outputs.ConnectTo(node.Inputs);
                    edge.capabilities = 0;
                    AddElement(edge);
                }
            }
        }

        HashSet<NodeView> _finishedLayouting = new HashSet<NodeView>();

        /// <summary>
        /// Creates the layout for the GraphView bottom up.
        /// </summary>
        private void UpdateLayout() {
            int paralletlAxis = Horizontal ? 1 : 0;
            const float offset = 60;
            Vector2 pos = Vector2.zero;
            _finishedLayouting.Clear();

            foreach (var node in _outputNodes) {
                pos[paralletlAxis] += offset;
                UpdateLayoutRecursive(node, pos);
            }

            /*_outputNodes.ForEach(node => stack.Push(node));
            while (stack.Count != 0) {
                NodeView node = stack.Pop();

            }*/
        }

        private void UpdateLayoutRecursive(NodeView node, Vector2 pos) {
            int parallelAxis = Horizontal ? 1 : 0;
            int sequentialAxis = 1 - parallelAxis;
            const float offset = 80;
            pos[sequentialAxis] -= 180;
            node.Position = pos;
            _finishedLayouting.Add(node);

            pos[parallelAxis] += ((node.BarracudaLayer.inputs.Length - 1) / 2f) * offset;

            foreach (string layer in node.BarracudaLayer.inputs) {
                NodeView inNode = _nodeMap[layer];
                if (_finishedLayouting.Contains(inNode)) {
                    Vector2 tmp = inNode.Position;
                    tmp[sequentialAxis] = pos[sequentialAxis];
                    //inNode.Position = tmp;
                    continue;
                }
                Debug.Assert(inNode.title != node.title, $"Node {layer} has a self-refence.");

                if (node.BarracudaLayer.inputs.Length > 1) {
                    pos[parallelAxis] -= offset;
                }

                UpdateLayoutRecursive(inNode, pos);
            }
        }

        private NodeView CreateLayerNode(Unity.Barracuda.Layer layer) {
            NodeView node = new NodeView(layer, Orientation);
            AddElement(node);
            node.capabilities = Capabilities.Selectable | Capabilities.Movable;
            _nodeMap.Add(layer.name, node);
            return node;
        }

        private NodeView CreateRootNode(string name) {
            var dummyLayer = new Unity.Barracuda.Layer(name, Unity.Barracuda.Layer.Activation.None);
            dummyLayer.name = name;
            NodeView node = new NodeView(dummyLayer, Orientation);
            node.title = name;
            AddElement(node);
            _nodeMap.Add(name, node);
            return node;
        }
    }
}