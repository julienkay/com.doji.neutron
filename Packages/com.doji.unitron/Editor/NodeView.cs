using Unity.Barracuda;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Unitron.Editor {
    public class NodeView : UnityEditor.Experimental.GraphView.Node {

        public Layer BarracudaLayer;

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

        public NodeView(Layer layer, Orientation orientation) {
            CreateInputPorts();
            CreateOutputPorts();
            _orientation = orientation;
            BarracudaLayer = layer;
            title = $"{layer.type} ({layer.name})";
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