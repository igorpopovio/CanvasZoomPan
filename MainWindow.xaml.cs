using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CanvasZoomPan {
    public partial class MainWindow {
        public ViewModel ViewModel { get; set; } = new ViewModel();

        public MainWindow() {
            InitializeComponent();
            var polyline = new Polyline {
                // Points = PointCollection.Parse("0,0 200,0 200,200 0,200 0,0"),
                Points = new PointCollection(new List<Point>() {
                    new Point(0,0),
                    new Point(200,0),
                    new Point(200,200),
                    new Point(0,200),
                    new Point(0,0),
                }),
            };
            polyline.Fill = new SolidColorBrush(Colors.Red);
            ViewModel.Polylines.Add(polyline);
            DataContext = ViewModel;
        }
    }
}
