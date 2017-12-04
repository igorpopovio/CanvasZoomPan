using System.Collections.Generic;
using System.Windows.Shapes;
using PropertyChanged;

namespace CanvasZoomPan {
    [AddINotifyPropertyChangedInterface]
    public class ViewModel {
        public List<Polyline> Polylines { get; set; } = new List<Polyline>();
    }
}
