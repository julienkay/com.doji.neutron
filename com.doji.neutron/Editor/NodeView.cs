using Unity.Sentis;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using OnnxLayer = Unity.Sentis.Layers.Layer;

namespace Neutron.Editor {
    public class NodeView : Node {

        public OnnxLayer Layer;

        public Port Inputs;
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
            CreateInputPorts();
            CreateOutputPorts();
            _orientation = orientation;
            Layer = null;
            title = name;
        }

        public NodeView(OnnxLayer layer, Orientation orientation) {
            CreateInputPorts();
            CreateOutputPorts();
            _orientation = orientation;
            Layer = layer;
            string type = 
            title = $"{layer.GetType().Name} ({layer.name})";
        }

        private void CreateInputPorts() {
            Inputs = InstantiatePort(_orientation, Direction.Input, Port.Capacity.Multi, typeof(bool));
            if (Inputs != null) {
                Inputs.portName = "";
                inputContainer.Add(Inputs);
            }
        }

        private void CreateOutputPorts() {
            Outputs = InstantiatePort(_orientation, Direction.Output, Port.Capacity.Multi, typeof(bool));
            if (Outputs != null) {
                Outputs.portName = "";
                outputContainer.Add(Outputs);
            }
        }

        /*public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);
            position = newPos.position;
        }*/
    }
}