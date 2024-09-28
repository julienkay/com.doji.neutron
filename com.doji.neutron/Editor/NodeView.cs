using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using OnnxLayer = Unity.Sentis.Layer;
using static Neutron.Editor.PathUtils;

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

        public bool CPUFallback {
            get {
                return _cpuFallback;
            } set {
                _cpuFallback = value;
                if (value) {
                    AddToClassList("cpu-fallback");
                } else {
                    RemoveFromClassList("cpu-fallback");
                }
            }
        }
        private bool _cpuFallback;

        private Type _layerType;

        public NodeView(OnnxLayer layer, Orientation orientation) : base(NodeViewUxml) {
            _orientation = orientation;
            Layer = layer;
            title = layer != null ? $"{layer.GetType().Name}" : null;
            _layerType = layer?.GetType();
            CreateInputPorts();
            CreateOutputPorts();
            SetupClasses();
        }

        public NodeView(string name, Orientation orientation) : this((OnnxLayer)null, orientation) {
            title = name;
        }

        private void SetupClasses() {
            if (_layerType != null) {
                AddToClassList(_layerType.Name.ToLower());
            }
        }

        private void CreateInputPorts() {
            Port input = InstantiatePort(_orientation, Direction.Input, Port.Capacity.Multi, typeof(bool));
            if (input != null) {
                input.portName = "";
                input.style.flexDirection = FlexDirection.Column;
                input.pickingMode = PickingMode.Ignore;
                inputContainer.Add(input);
            }

            Inputs = input;
        }

        private void CreateOutputPorts() {
            Port output = InstantiatePort(_orientation, Direction.Output, Port.Capacity.Multi, typeof(bool));
            if (output != null) {
                output.portName = "";
                output.style.flexDirection = FlexDirection.ColumnReverse;
                output.pickingMode = PickingMode.Ignore;
                outputContainer.Add(output);
            }
            Outputs = output;
        }
    }
}