  using UnityEngine.UIElements;

namespace Unitron.Editor {
    public class InspectorView : VisualElement {
        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits> { }
        public InspectorView() { }
    }
}