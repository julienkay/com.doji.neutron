using System.Collections.Generic;
using Unity.Sentis;

namespace Neutron.Editor {

    public static class TopologicalSortUtils {

        public static List<Layer> TopologicalSort(Graph graph) {

            List<Layer> result = new List<Layer>();
            HashSet<Layer> visited = new HashSet<Layer>();
            Stack<Layer> stack = new Stack<Layer>();

            foreach (var node in graph.Nodes) {
                if (!visited.Contains(node.Value)) {
                    InternalTopologicalSort(graph, node.Value, visited, stack);
                }
            }

            while (stack.Count > 0) {
                result.Add(stack.Pop());
            }

            return result;
        }

        private static void InternalTopologicalSort(
            Graph graph,
            Layer node,
            HashSet<Layer> visited,
            Stack<Layer> stack)
        {
            visited.Add(node);

            graph.Edges.TryGetValue(node, out var edges);

            if (edges == null) {
                stack.Push(node);
                return;
            }

            foreach (var neighbor in edges) {
                if (!visited.Contains(neighbor)) {
                    InternalTopologicalSort(graph, neighbor, visited, stack);
                }
            }

            stack.Push(node);
        }

    }
}