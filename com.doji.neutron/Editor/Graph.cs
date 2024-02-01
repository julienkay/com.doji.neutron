using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Sentis;

// in this class we're calling them layers  to disambiguate between the 
// 'Layers' that each graph node is assigned to as part of the layouting process.
using Node = Unity.Sentis.Layers.Layer;

namespace Neutron.Editor {

    /// <summary>
    /// A graph representation for Sentis <see cref="Model"/>s.
    /// Stores edge representations explicitly and allows topological sorting.
    /// </summary>
    public class Graph {

        public Dictionary<string, Node> Nodes { get; private set; } = new Dictionary<string, Node>();

        public Dictionary<Node, List<Node>> Edges { get; private set; } = new Dictionary<Node, List<Node>>();

        public Dictionary<Node, List<Node>> ReverseEdges { get; private set; } = new Dictionary<Node, List<Node>>();

        public List<List<Node>> Layers { get; private set; } = new List<List<Node>>();
        
        public Dictionary<Node, int> LayersByNode { get; private set; } = new Dictionary<Node, int>();

        private List<Node> _sortedNodes = new List<Node>();

        private List<Model.Input> _inputs = new List<Model.Input>();
        private List<string> _outputs = new List<string>();

        public Graph(Model model) {
            _inputs = model.inputs;
            _outputs = model.outputs;

            // get all nodes
            foreach (Node node in model.layers) {
                Nodes.Add(node.name, node);
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

            foreach (Node node in model.layers) {
                foreach (var input in node.inputs) {
                    if (!Nodes.TryGetValue(input, out Node inputNode)) {
                        continue;
                    }
                    if (!Edges.TryGetValue(inputNode, out List<Node> edges)) {
                        Edges[inputNode] = new List<Node>();
                    }
                    Edges[inputNode].Add(node);
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
                if (name == output) {
                    return true;
                }
            }
            return false;
        }
    }
}