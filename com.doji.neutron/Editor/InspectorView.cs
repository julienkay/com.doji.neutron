  using UnityEngine.UIElements;

namespace Neutron.Editor {
    public class InspectorView : VisualElement {
        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits> { }
        public InspectorView() { }
    }
}