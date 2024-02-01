using System.Collections.Generic;
using Unity.Sentis.Layers;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using OnnxLayer = Unity.Sentis.Layers.Layer;

namespace Neutron.Editor {
    public class NodeView : Node {

        public OnnxLayer Layer;

        public List<Port> Inputs = new List<Port>();
        public Port Outputs;

        private Orientation _orientation;

        public Vector2 Position {
            get {
                return _position;
            }
            set {
                _position = value;
                style.left = _position.x;
                style.top = _position.y;
            }
        }
        public Vector2 _position;

        public NodeView(string name, Orientation orientation) {
            _orientation = orientation;
            Layer = null;
            title = name;
            CreateInputPorts();
            CreateOutputPorts();
        }

        public NodeView(OnnxLayer layer, Orientation orientation) {
            _orientation = orientation;
            Layer = layer;
            title = $"{layer.GetType().Name} ({layer.name})";
            CreateInputPorts();
            CreateOutputPorts();
        }

        private void CreateInputPorts() {
            if (Layer == null || Layer.inputs == null) {
                return;
            }
            foreach (var layer in Layer.inputs) {
                Inputs.Add(CreateInputPort());
            }
        }

        private Port CreateInputPort() {
            Port input = InstantiatePort(_orientation, Direction.Input, Port.Capacity.Multi, typeof(bool));
            if (input != null) {
                input.portName = "";
                inputContainer.Add(input);
            }
            return input;
        }

        private void CreateOutputPorts() {
            Port output = InstantiatePort(_orientation, Direction.Output, Port.Capacity.Multi, typeof(bool));
            if (output != null) {
                output.portName = "";
                outputContainer.Add(output);
            }
            Outputs = output;
        }

        /*public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);
            position = newPos.position;
        }*/
    }
}