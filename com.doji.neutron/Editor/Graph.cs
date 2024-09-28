using System;
using System.Collections.Generic;
using Unity.Sentis;

// in this class we're calling them nodes to disambiguate between the 
// 'Layers' that each graph node is assigned to as part of the layouting process.
using Node = Unity.Sentis.Layer;

namespace Neutron.Editor {

    /// <summary>
    /// A graph representation for Sentis <see cref="Model"/>s.
    /// Stores edge representations explicitly and allows topological sorting.
    /// </summary>
    public class Graph {

        public Dictionary<int, Node> Nodes { get; private set; } = new Dictionary<int, Node>();

        public Dictionary<Node, List<Node>> Edges { get; private set; } = new Dictionary<Node, List<Node>>();

        public Dictionary<Node, List<Node>> ReverseEdges { get; private set; } = new Dictionary<Node, List<Node>>();

        public List<List<Node>> Layers { get; private set; } = new List<List<Node>>();
        
        public Dictionary<Node, int> LayersByNode { get; private set; } = new Dictionary<Node, int>();

        private List<Node> _sortedNodes = new List<Node>();

        private List<Model.Input> _inputs = new List<Model.Input>();
        private List<Model.Output> _outputs = new List<Model.Output>();

        public Graph(Model model) {
            _inputs = model.inputs;
            _outputs = model.outputs;

            // get all nodes
            foreach (Node node in model.layers) {
                //if (node is ConstantOfShape) {
                //    // skip const inputs since we don't display them
                //    continue;
                //}
                Nodes.Add(node.outputs[0], node);
            }

            // get all edges
            foreach (Node node in model.layers) {
                foreach (var input in node.inputs) {
                    if (!Nodes.TryGetValue(input, out Node inputNode)) {
                        continue;
                    }
                    if (!Edges.ContainsKey(inputNode)) {
                        Edges[inputNode] = new List<Node>();
                    }
                    Edges[inputNode].Add(node);

                    if (!ReverseEdges.ContainsKey(node)) {
                        ReverseEdges[node] = new List<Node>();
                    }
                    ReverseEdges[node].Add(inputNode);
                }
            }

            _sortedNodes = TopologicalSortUtils.TopologicalSort(this);
            Layers = Layering(_sortedNodes);
            LayersByNode = new Dictionary<Node, int>();
            int i = 0;
            foreach (List<Node> layer in Layers) {
                foreach(Node node in layer) {
                    LayersByNode[node] = i;
                }
                i++;
            }
        }

        public List<List<Node>> Layering(List<Node> topologicalOrder) {
            List<List<Node>> layers = new List<List<Node>>();

            foreach (var node in topologicalOrder) {
                int NodeIndex = GetNodeIndex(node, layers);
                if (NodeIndex == layers.Count) {
                    layers.Add(new List<Node>());
                }
                layers[NodeIndex].Add(node);
            }

            return layers;
        }

        private int GetNodeIndex(Node node, List<List<Node>> layers) {
            int maxPredecessorNode = -1;
            if (ReverseEdges.ContainsKey(node)) {
                foreach (Node predecessor in ReverseEdges[node]) {
                    foreach (List<Node> layer in layers) {
                        if (layer.Contains(predecessor)) {
                            maxPredecessorNode = Math.Max(maxPredecessorNode, layers.IndexOf(layer));
                            break;
                        }
                    }
                }
            }
            return maxPredecessorNode + 1;
        }

        public int GetChildCount(Node node) {
            if (!Nodes.ContainsKey(node.outputs[0])) {
                throw new ArgumentException($"The node'{node}' was not found in the graph.");
            }
            if (!Edges.ContainsKey(node)) {
                //throw new ArgumentException($"No child nodes where found for node '{node}'.");
                //TODO: insert custom dummy nodes (inheriting from Sentis.Layer) for input/output nodes and insert them into graph
                return 0;
            }

            return Edges[node].Count;
        }

        public int GetParentCount(Node node) {
            if (!Nodes.ContainsKey(node.outputs[0])) {
                throw new ArgumentException($"The node '{node}' was not found in the graph.");
            }
            if (!ReverseEdges.ContainsKey(node)) {
                //throw new ArgumentException($"No parent nodes where found for node '{node}'.");
                return 0;
            }

            return ReverseEdges[node].Count;
        }

        public int GetParent(Node node) {
            if (!Nodes.ContainsKey(node.outputs[0])) {
                throw new ArgumentException($"The node '{node}' was not found in the graph.");
            }
            if (!ReverseEdges.ContainsKey(node)) {
                throw new ArgumentException($"No parent nodes where found for node '{node}'.");
            }

            return ReverseEdges[node].Count;
        }

        public bool IsInput(string name) {
            foreach(var input in _inputs) {
                if (name == input.name) { 
                    return true;
                }
            }
            return false;
        }

        public bool IsOutput(string name) {
            foreach (var output in _outputs) {
                if (name == output.name) {
                    return true;
                }
            }
            return false;
        }
    }
}