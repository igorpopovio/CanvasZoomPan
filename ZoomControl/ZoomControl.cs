using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CanvasZoomPan {
    [TemplatePart(Name = PART_Presenter, Type = typeof(ZoomContentPresenter))]
    public class ZoomControl : ContentControl {
        private const string PART_Presenter = "PART_Presenter";

        public static readonly DependencyProperty AnimationLengthProperty =
            DependencyProperty.Register("AnimationLength", typeof(TimeSpan), typeof(ZoomControl),
                                        new UIPropertyMetadata(TimeSpan.Zero));

        public static readonly DependencyProperty MaxZoomDeltaProperty =
            DependencyProperty.Register("MaxZoomDelta", typeof(double), typeof(ZoomControl),
                                        new UIPropertyMetadata(5.0));

        public static readonly DependencyProperty MaxZoomProperty =
            DependencyProperty.Register("MaxZoom", typeof(double), typeof(ZoomControl), new UIPropertyMetadata(100.0));

        public static readonly DependencyProperty MinZoomProperty =
            DependencyProperty.Register("MinZoom", typeof(double), typeof(ZoomControl), new UIPropertyMetadata(0.01));

        public bool FitWidthOnly {
            get { return (bool)GetValue(FitWidthOnlyProperty); }
            set { SetValue(FitWidthOnlyProperty, value); }
        }

        public static readonly DependencyProperty FitWidthOnlyProperty =
            DependencyProperty.Register("FitWidthOnly", typeof(bool), typeof(ZoomControl), new PropertyMetadata(false));

        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(ZoomControlModes), typeof(ZoomControl),
                                        new UIPropertyMetadata(ZoomControlModes.Custom, Mode_PropertyChanged));

        private static void Mode_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var zc = (ZoomControl)d;
            var mode = (ZoomControlModes)e.NewValue;
            switch (mode) {
                case ZoomControlModes.Fill:
                    zc.DoZoomToFill();
                    break;
                case ZoomControlModes.Original:
                    zc.DoZoomToOriginal();
                    break;
                case ZoomControlModes.Custom:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static readonly DependencyProperty ModifierModeProperty =
            DependencyProperty.Register("ModifierMode", typeof(ZoomViewModifierMode), typeof(ZoomControl),
                                        new UIPropertyMetadata(ZoomViewModifierMode.None));

        public static readonly DependencyProperty TranslateXProperty =
            DependencyProperty.Register("TranslateX", typeof(double), typeof(ZoomControl),
                                        new UIPropertyMetadata(0.0, TranslateX_PropertyChanged, TranslateX_Coerce));

        public static readonly DependencyProperty TranslateYProperty =
            DependencyProperty.Register("TranslateY", typeof(double), typeof(ZoomControl),
                                        new UIPropertyMetadata(0.0, TranslateY_PropertyChanged, TranslateY_Coerce));





        public double MinTranslateX {
            get { return (double)GetValue(MinTranslateXProperty); }
            set { SetValue(MinTranslateXProperty, value); }
        }

        public static readonly DependencyProperty MinTranslateXProperty =
            DependencyProperty.Register("MinTranslateX", typeof(double), typeof(ZoomControl), new PropertyMetadata(-10000d));



        public double MaxTranslateX {
            get { return (double)GetValue(MaxTranslateXProperty); }
            set { SetValue(MaxTranslateXProperty, value); }
        }

        public static readonly DependencyProperty MaxTranslateXProperty =
            DependencyProperty.Register("MaxTranslateX", typeof(double), typeof(ZoomControl), new PropertyMetadata(10000d));


        public double MinTranslateY {
            get { return (double)GetValue(MinTranslateYProperty); }
            set { SetValue(MinTranslateYProperty, value); }
        }

        public static readonly DependencyProperty MinTranslateYProperty =
            DependencyProperty.Register("MinTranslateY", typeof(double), typeof(ZoomControl), new PropertyMetadata(-10000d));


        public double MaxTranslateY {
            get { return (double)GetValue(MaxTranslateYProperty); }
            set { SetValue(MaxTranslateYProperty, value); }
        }

        public static readonly DependencyProperty MaxTranslateYProperty =
            DependencyProperty.Register("MaxTranslateY", typeof(double), typeof(ZoomControl), new PropertyMetadata(10000d));


        public static readonly DependencyProperty ZoomBoxBackgroundProperty =
            DependencyProperty.Register("ZoomBoxBackground", typeof(Brush), typeof(ZoomControl),
                                        new UIPropertyMetadata(null));


        public static readonly DependencyProperty ZoomBoxBorderBrushProperty =
            DependencyProperty.Register("ZoomBoxBorderBrush", typeof(Brush), typeof(ZoomControl),
                                        new UIPropertyMetadata(null));


        public static readonly DependencyProperty ZoomBoxBorderThicknessProperty =
            DependencyProperty.Register("ZoomBoxBorderThickness", typeof(Thickness), typeof(ZoomControl),
                                        new UIPropertyMetadata(null));


        public static readonly DependencyProperty ZoomBoxOpacityProperty =
            DependencyProperty.Register("ZoomBoxOpacity", typeof(double), typeof(ZoomControl),
                                        new UIPropertyMetadata(0.5));


        public static readonly DependencyProperty ZoomBoxProperty =
            DependencyProperty.Register("ZoomBox", typeof(Rect), typeof(ZoomControl),
                                        new UIPropertyMetadata(new Rect()));

        public static readonly DependencyProperty ZoomDeltaMultiplierProperty =
            DependencyProperty.Register("ZoomDeltaMultiplier", typeof(double), typeof(ZoomControl),
                                        new UIPropertyMetadata(100.0));

        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register("Zoom", typeof(double), typeof(ZoomControl),
                                        new UIPropertyMetadata(1.0, Zoom_PropertyChanged));

        private Point _mouseDownPos;


        /// <summary>
        /// Applied to the presenter.
        /// </summary>
        private ScaleTransform _scaleTransform;
        private Vector _startTranslate;
        private TransformGroup _transformGroup;

        /// <summary>
        /// Applied to the scrollviewer.
        /// </summary>
        private TranslateTransform _translateTransform;

        private int _zoomAnimCount;
        private bool _isZooming = false;

        static ZoomControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomControl),
                                                     new FrameworkPropertyMetadata(typeof(ZoomControl)));
        }

        public ZoomControl() {
            PreviewMouseWheel += ZoomControl_MouseWheel;
            PreviewMouseDown += ZoomControl_PreviewMouseDown;
            MouseDown += ZoomControl_MouseDown;
            MouseUp += ZoomControl_MouseUp;
        }

        public Brush ZoomBoxBackground {
            get { return (Brush)GetValue(ZoomBoxBackgroundProperty); }
            set { SetValue(ZoomBoxBackgroundProperty, value); }
        }

        public Brush ZoomBoxBorderBrush {
            get { return (Brush)GetValue(ZoomBoxBorderBrushProperty); }
            set { SetValue(ZoomBoxBorderBrushProperty, value); }
        }

        public Thickness ZoomBoxBorderThickness {
            get { return (Thickness)GetValue(ZoomBoxBorderThicknessProperty); }
            set { SetValue(ZoomBoxBorderThicknessProperty, value); }
        }

        public double ZoomBoxOpacity {
            get { return (double)GetValue(ZoomBoxOpacityProperty); }
            set { SetValue(ZoomBoxOpacityProperty, value); }
        }

        public Rect ZoomBox {
            get { return (Rect)GetValue(ZoomBoxProperty); }
            set { SetValue(ZoomBoxProperty, value); }
        }

        public Point OrigoPosition {
            get { return new Point(ActualWidth / 2, ActualHeight / 2); }
        }

        public double TranslateX {
            get { return (double)GetValue(TranslateXProperty); }
            set {
                BeginAnimation(TranslateXProperty, null);
                SetValue(TranslateXProperty, value);
            }
        }

        public double TranslateY {
            get { return (double)GetValue(TranslateYProperty); }
            set {
                BeginAnimation(TranslateYProperty, null);
                SetValue(TranslateYProperty, value);
            }
        }

        public TimeSpan AnimationLength {
            get { return (TimeSpan)GetValue(AnimationLengthProperty); }
            set { SetValue(AnimationLengthProperty, value); }
        }

        public double MinZoom {
            get { return (double)GetValue(MinZoomProperty); }
            set { SetValue(MinZoomProperty, value); }
        }

        public double MaxZoom {
            get { return (double)GetValue(MaxZoomProperty); }
            set { SetValue(MaxZoomProperty, value); }
        }

        public double MaxZoomDelta {
            get { return (double)GetValue(MaxZoomDeltaProperty); }
            set { SetValue(MaxZoomDeltaProperty, value); }
        }

        public double ZoomDeltaMultiplier {
            get { return (double)GetValue(ZoomDeltaMultiplierProperty); }
            set { SetValue(ZoomDeltaMultiplierProperty, value); }
        }

        public double Zoom {
            get { return (double)GetValue(ZoomProperty); }
            set {
                if (value == (double)GetValue(ZoomProperty))
                    return;
                BeginAnimation(ZoomProperty, null);
                SetValue(ZoomProperty, value);
            }
        }

        protected ZoomContentPresenter Presenter {
            get { return GetTemplateChild(PART_Presenter) as ZoomContentPresenter; }
            set {
                //add the ScaleTransform to the presenter
                _transformGroup = new TransformGroup();
                _scaleTransform = new ScaleTransform();
                _translateTransform = new TranslateTransform();
                _transformGroup.Children.Add(_scaleTransform);
                _transformGroup.Children.Add(_translateTransform);
                if (Presenter != null) {
                    Presenter.RenderTransform = _transformGroup;
                    Presenter.RenderTransformOrigin = new Point(0.5, 0.5);
                }
            }
        }

        /// <summary>
        /// Gets or sets the active modifier mode.
        /// </summary>
        public ZoomViewModifierMode ModifierMode {
            get { return (ZoomViewModifierMode)GetValue(ModifierModeProperty); }
            set { SetValue(ModifierModeProperty, value); }
        }

        /// <summary>
        /// Gets or sets the mode of the zoom control.
        /// </summary>
        public ZoomControlModes Mode {
            get { return (ZoomControlModes)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        private static object TranslateX_Coerce(DependencyObject d, object basevalue) {
            var zc = (ZoomControl)d;
            return zc.GetCoercedTranslateX((double)basevalue, zc.Zoom);
        }

        private double GetCoercedTranslateX(double baseValue, double zoom) {
            if (Presenter == null)
                return 0.0;
            // return baseValue.EnsureIsBetween(MinTranslateX * globalDeltaZoom, MaxTranslateX * globalDeltaZoom);
            return baseValue;
        }

        private static object TranslateY_Coerce(DependencyObject d, object basevalue) {
            var zc = (ZoomControl)d;
            return zc.GetCoercedTranslateY((double)basevalue, zc.Zoom);
        }

        private double GetCoercedTranslateY(double baseValue, double zoom) {
            if (Presenter == null)
                return 0.0;
            // return baseValue.EnsureIsBetween(MinTranslateY * globalDeltaZoom, MaxTranslateY * globalDeltaZoom);
            return baseValue;
        }


        private void ZoomControl_MouseUp(object sender, MouseButtonEventArgs e) {
            switch (ModifierMode) {
                case ZoomViewModifierMode.None:
                    return;
                case ZoomViewModifierMode.Pan:
                    break;
                case ZoomViewModifierMode.ZoomIn:
                    break;
                case ZoomViewModifierMode.ZoomOut:
                    break;
                case ZoomViewModifierMode.ZoomBox:
                    ZoomTo(ZoomBox);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ModifierMode = ZoomViewModifierMode.None;
            PreviewMouseMove -= ZoomControl_PreviewMouseMove;
            ReleaseMouseCapture();
        }

        public void ZoomTo(Rect rect) {
            var deltaZoom = Math.Min(
                ActualWidth / rect.Width,
                ActualHeight / rect.Height);

            var startHandlePosition = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);

            DoZoom(deltaZoom, OrigoPosition, startHandlePosition, OrigoPosition);
            ZoomBox = new Rect();
        }

        private void ZoomControl_PreviewMouseMove(object sender, MouseEventArgs e) {
            switch (ModifierMode) {
                case ZoomViewModifierMode.None:
                    return;
                case ZoomViewModifierMode.Pan:
                    var translate = _startTranslate + (e.GetPosition(this) - _mouseDownPos);
                    TranslateX = translate.X;
                    TranslateY = translate.Y;
                    break;
                case ZoomViewModifierMode.ZoomIn:
                    break;
                case ZoomViewModifierMode.ZoomOut:
                    break;
                case ZoomViewModifierMode.ZoomBox:
                    var pos = e.GetPosition(this);
                    var x = Math.Min(_mouseDownPos.X, pos.X);
                    var y = Math.Min(_mouseDownPos.Y, pos.Y);
                    var sizeX = Math.Abs(_mouseDownPos.X - pos.X);
                    var sizeY = Math.Abs(_mouseDownPos.Y - pos.Y);
                    ZoomBox = new Rect(x, y, sizeX, sizeY);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ZoomControl_MouseDown(object sender, MouseButtonEventArgs e) {
            OnMouseDown(e, false);
        }

        private void ZoomControl_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            OnMouseDown(e, true);
        }

        private void OnMouseDown(MouseButtonEventArgs e, bool isPreview) {
            if (ModifierMode != ZoomViewModifierMode.None)
                return;

            switch (Keyboard.Modifiers) {
                case ModifierKeys.None:
                    if (!isPreview)
                        ModifierMode = ZoomViewModifierMode.Pan;
                    break;
                case ModifierKeys.Alt:
                    ModifierMode = ZoomViewModifierMode.ZoomBox;
                    break;
                case ModifierKeys.Control:
                    break;
                case ModifierKeys.Shift:
                    ModifierMode = ZoomViewModifierMode.Pan;
                    break;
                case ModifierKeys.Windows:
                    break;
                default:
                    return;
            }

            if (ModifierMode == ZoomViewModifierMode.None)
                return;

            _mouseDownPos = e.GetPosition(this);
            _startTranslate = new Vector(TranslateX, TranslateY);
            Mouse.Capture(this);
            PreviewMouseMove += ZoomControl_PreviewMouseMove;
        }

        private static void TranslateX_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var zc = (ZoomControl)d;
            if (zc._translateTransform == null)
                return;
            zc._translateTransform.X = (double)e.NewValue;
            if (!zc._isZooming)
                zc.Mode = ZoomControlModes.Custom;
        }

        private static void TranslateY_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var zc = (ZoomControl)d;
            if (zc._translateTransform == null)
                return;
            zc._translateTransform.Y = (double)e.NewValue;
            if (!zc._isZooming)
                zc.Mode = ZoomControlModes.Custom;
        }

        private static void Zoom_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var zc = (ZoomControl)d;

            if (zc._scaleTransform == null)
                return;

            double zoom = (double)e.NewValue;
            // zoom = zc.CalculateConstrainedZoom(zoom);
            zc._scaleTransform.ScaleX = zoom;
            zc._scaleTransform.ScaleY = zoom;
            if (!zc._isZooming) {
                double delta = (double)e.NewValue / (double)e.OldValue;
                zc.TranslateX *= delta;
                zc.TranslateY *= delta;
                zc.Mode = ZoomControlModes.Custom;
            }
        }

        private void ZoomControl_MouseWheel(object sender, MouseWheelEventArgs e) {
            e.Handled = true;
            Point origoPosition = new Point(ActualWidth / 2, ActualHeight / 2);
            Point mousePosition = e.GetPosition(this);

            DoZoom(
                Math.Max(1 / MaxZoomDelta, Math.Min(MaxZoomDelta, e.Delta / 20000.0 * ZoomDeltaMultiplier + 1)),
                origoPosition,
                mousePosition,
                mousePosition);
        }


        double globalDeltaZoom = 1;

        private void DoZoom(double deltaZoom, Point origoPosition, Point startHandlePosition, Point targetHandlePosition) {
            globalDeltaZoom = deltaZoom;
            AnimationLength = TimeSpan.FromMilliseconds(500);
            LimitZoomingAndPanning();

            double startZoom = Zoom;
            double currentZoom = startZoom * deltaZoom;
            currentZoom = currentZoom.EnsureIsBetween(MinZoom, MaxZoom);

            if (Math.Abs(currentZoom - MinZoom) < 0.0001) {
                ZoomToFill();
                AnimationLength = TimeSpan.Zero;
                return;
            }

            var startTranslate = new Vector(TranslateX, TranslateY);

            var v = (startHandlePosition - origoPosition);
            var vTarget = (targetHandlePosition - origoPosition);

            var targetPoint = (v - startTranslate) / startZoom;
            var zoomedTargetPointPos = targetPoint * currentZoom + startTranslate;
            var endTranslate = vTarget - zoomedTargetPointPos;

            double transformX = GetCoercedTranslateX(TranslateX + endTranslate.X, currentZoom);
            double transformY = GetCoercedTranslateY(TranslateY + endTranslate.Y, currentZoom);

            DoZoomAnimation(currentZoom, transformX, transformY);
            Mode = ZoomControlModes.Custom;
            AnimationLength = TimeSpan.Zero;
        }

        public void LimitZoomingAndPanning() {
            if (Presenter == null) return;
            Presenter.UpdateContentSize();
            MinZoom = GetZoomToFitSize();
            MaxZoom = 15;

            var centerTranslation = GetCenterIntoViewTranslation();
            var box = Presenter.CalculateBoundingBox();
            // MinTranslateX = -box.BottomLeft.X;
            // MinTranslateY = -box.BottomLeft.Y;

            // MaxTranslateX = (-box.BottomLeft.X + Presenter.ContentSize.Width);
            // MaxTranslateY = (-box.BottomLeft.Y + Presenter.ContentSize.Height);

            //MaxTranslateX = 1000;
            //MaxTranslateY = 1000;


            // DoZoomAnimation(deltaZoom, centerTranslation.X * deltaZoom, centerTranslation.Y * deltaZoom);
        }

        private double GetZoomToFitSize() {
            var padding = 10;
            var box = Presenter.CalculateBoundingBox();
            var minWidth = ActualWidth / (box.Size.Width + padding);
            var minHeight = ActualHeight / (box.Size.Height + padding);
            var minBoth = Math.Min(minWidth, minHeight);
            return FitWidthOnly ? minWidth : minBoth;
        }

        private void DoZoomAnimation(double targetZoom, double transformX, double transformY) {
            DoZoomAnimation(targetZoom, transformX, transformY, AnimationLength);
        }

        private void DoZoomAnimation(double targetZoom, double transformX, double transformY, TimeSpan animationLength) {
            _isZooming = true;
            var duration = new Duration(animationLength);
            DoTranslationAnimation(transformX, transformY, duration);
            StartAnimation(ZoomProperty, targetZoom, duration);
        }

        private void DoTranslationAnimation(double transformX, double transformY, Duration duration) {
            StartAnimation(TranslateXProperty, transformX, duration);
            StartAnimation(TranslateYProperty, transformY, duration);
        }

        private void StartAnimation(DependencyProperty dp, double toValue, Duration duration) {
            if (double.IsNaN(toValue) || double.IsInfinity(toValue)) {
                if (dp == ZoomProperty) {
                    _isZooming = false;
                }
                return;
            }
            var animation = new DoubleAnimation(toValue, duration);
            if (dp == ZoomProperty) {
                _zoomAnimCount++;
                animation.Completed += (s, args) => {
                    _zoomAnimCount--;
                    if (_zoomAnimCount > 0)
                        return;
                    var zoom = Zoom;
                    BeginAnimation(ZoomProperty, null);
                    SetValue(ZoomProperty, zoom);
                    _isZooming = false;
                };
            }
            BeginAnimation(dp, animation, HandoffBehavior.Compose);
        }

        public void ZoomToOriginal() {
            Mode = ZoomControlModes.Original;
        }

        private void DoZoomToOriginal() {
            if (Presenter == null)
                return;

            var initialTranslate = GetCenterIntoViewTranslation();
            DoZoomAnimation(1.0, initialTranslate.X, initialTranslate.Y, TimeSpan.Zero);
        }

        private Vector GetCenterIntoViewTranslation() {
            if (Presenter == null) return new Vector(0, 0);
            var box = Presenter.BoundingBoxProperty;
            var boundingBoxCenter = new Point(box.BottomLeft.X + box.Size.Width / 2, box.BottomLeft.Y + box.Size.Height / 2);
            var canvasCenter = new Point(ActualWidth / 2, ActualHeight / 2);
            return canvasCenter - boundingBoxCenter;
        }

        public void ZoomToFill() {
            Mode = ZoomControlModes.Fill;
        }

        public void DoZoomToFill() {
            if (Presenter == null) return;

            var deltaZoom = GetZoomToFitSize();
            var centerTranslation = GetCenterIntoViewTranslation();
            DoZoomAnimation(deltaZoom, centerTranslation.X * deltaZoom, centerTranslation.Y * deltaZoom);
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            //get the presenter, and initialize
            Presenter = GetTemplateChild(PART_Presenter) as ZoomContentPresenter;

            if (Presenter != null) {
                LimitZoomingAndPanning();
                ZoomToOriginal();
                ZoomToFill();
                if (Mode == ZoomControlModes.Fill)
                    DoZoomToFill();

                Presenter.SizeChanged += (s, a) => {
                    UpdateSize(a.PreviousSize, a.NewSize);
                };
                Presenter.ContentSizeChanged += (s, oldSize, newSize) => {
                    UpdateSize(oldSize, newSize);
                };
            }
        }

        private void UpdateSize(Size oldSize, Size newSize) {
            LimitZoomingAndPanning();

            if (Mode == ZoomControlModes.Fill)
                DoZoomToFill();
        }
    }
}
