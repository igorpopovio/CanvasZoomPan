using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CanvasZoomPan {
    public class ZoomContentPresenter : ContentPresenter {
        public event ContentSizeChangedHandler ContentSizeChanged;


        public ZoomContentPresenter() {
            var x = true;
        }

        public struct BoundingBox {
            public Point BottomLeft;
            public Point TopRight;
            public Size Size;
        }

        private Size _contentSize;
        public BoundingBox BoundingBoxProperty { get; private set; }

        public Size ContentSize {
            get { return _contentSize; }
            private set {
                if (value == _contentSize)
                    return;

                var oldSize = _contentSize;
                _contentSize = value;
                ContentSizeChanged?.Invoke(this, oldSize, value);
            }
        }

        protected override Size MeasureOverride(Size constraint) {
            base.MeasureOverride(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var max = 1000000000;
            var x = double.IsInfinity(constraint.Width) ? max : constraint.Width;
            var y = double.IsInfinity(constraint.Height) ? max : constraint.Height;
            return new Size(x, y);
        }

        protected override Size ArrangeOverride(Size arrangeBounds) {
            UIElement child = VisualChildrenCount > 0
                                  ? VisualTreeHelper.GetChild(this, 0) as UIElement
                                  : null;
            if (child == null) return arrangeBounds;

            

            UpdateContentSize();
            child.Arrange(new Rect());

            return arrangeBounds;
        }

        public void UpdateContentSize() {
            BoundingBoxProperty = CalculateBoundingBox();
            ContentSize = BoundingBoxProperty.Size;
        }

        public BoundingBox CalculateBoundingBox() {
            var maxX = Double.MinValue;
            var minX = Double.MaxValue;
            var maxY = Double.MinValue;
            var minY = Double.MaxValue;



            var bounds = VisualTreeHelper.GetDescendantBounds(this);

            //foreach (var room in Rooms)
            //    foreach (var point in room.RoomPoints) {
            //        minX = Math.Min(minX, point.X);
            //        maxX = Math.Max(maxX, point.X);
            //        minY = Math.Min(minY, point.Y);
            //        maxY = Math.Max(maxY, point.Y);
            //}



            return new BoundingBox {
                BottomLeft = bounds.TopLeft,
                TopRight = bounds.BottomRight,
                Size = bounds.Size,
            };
        }
    }
}
