using UnityEngine.UIElements;

namespace Neutron.Editor {

    public class SplitView : TwoPaneSplitView {
        public new class UxmlFactory : UxmlFactory<SplitView, UxmlTraits> { }

        public SplitView() { }
    }
}