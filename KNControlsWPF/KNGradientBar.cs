using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Effects;

using KNFoundation;
using KNFoundation.KNKVC;
using KNControls;

namespace KNControls {
    public class KNGradientBar : Canvas, KNKVOObserver {

        protected static Effect GradientBarTextEffect() {
            DropShadowEffect shadow = new DropShadowEffect();
            shadow.Color = Colors.White;
            shadow.Direction = 270;
            shadow.BlurRadius = 2.0;
            shadow.ShadowDepth = 1.0;
            shadow.Opacity = 0.75;
            return shadow;
        }

        private static ImageSource DropDownTriangle() {

            Rect triangleRect = new Rect(0, 0, 7, 4);
            PathFigure triangleFigure = new PathFigure();

            triangleFigure.StartPoint = triangleRect.TopLeft;
            triangleFigure.Segments.Add(new LineSegment(triangleRect.TopRight, true));
            triangleFigure.Segments.Add(new LineSegment(new Point(triangleRect.Width / 2, triangleRect.Bottom), true));
            triangleFigure.Segments.Add(new LineSegment(triangleRect.TopLeft, true));
            triangleFigure.IsClosed = true;

            //
            // Create a GeometryDrawing.
            //
            GeometryDrawing aGeometryDrawing = new GeometryDrawing();
            PathGeometry tabGeometry = new PathGeometry();
            tabGeometry.Figures.Add(triangleFigure);
            aGeometryDrawing.Geometry = tabGeometry;

            // Paint the drawing with a gradient.
            aGeometryDrawing.Brush = Brushes.Black;
            // Outline the drawing with a solid color.
            //aGeometryDrawing.Pen = new Pen(Brushes.Black, 10);

            DrawingImage geometryImage = new DrawingImage(aGeometryDrawing);

            // Freeze the DrawingImage for performance benefits.
            geometryImage.Freeze();

            return geometryImage;

        }

        public class ImageDependencyProperty {

            /// <summary>
            /// An attached dependency property which provides an
            /// <see cref="ImageSource" /> for arbitrary WPF elements.
            /// </summary>
            public static readonly DependencyProperty ImageProperty;

            /// <summary>
            /// Gets the <see cref="ImageProperty"/> for a given
            /// <see cref="DependencyObject"/>, which provides an
            /// <see cref="ImageSource" /> for arbitrary WPF elements.
            /// </summary>
            public static ImageSource GetImage(DependencyObject obj) {
                return (ImageSource)obj.GetValue(ImageProperty);
            }

            /// <summary>
            /// Sets the attached <see cref="ImageProperty"/> for a given
            /// <see cref="DependencyObject"/>, which provides an
            /// <see cref="ImageSource" /> for arbitrary WPF elements.
            /// </summary>
            public static void SetImage(DependencyObject obj, ImageSource value) {
                obj.SetValue(ImageProperty, value);
            }

            static ImageDependencyProperty() {
                //register attached dependency property
                var metadata = new FrameworkPropertyMetadata((ImageSource)null);
                ImageProperty = DependencyProperty.RegisterAttached("Image",
                                                                    typeof(ImageSource),
                                                                    typeof(ImageDependencyProperty), metadata);
            }
        }

        public static Style ButtonStyleWithTintColor(Color tintColor, Boolean shouldHaveDropdownIndicator) {

            new ImageDependencyProperty();

            Style buttonStyle = new Style();
            buttonStyle.TargetType = typeof(Button);
            buttonStyle.Setters.Add(new Setter(Control.OverridesDefaultStyleProperty, true));

            Setter templateSetter = new Setter();
            templateSetter.Property = Button.TemplateProperty;

            ControlTemplate template = new ControlTemplate(typeof(Button));


            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(Border));
            factory.Name = "Border";
            factory.SetValue(Control.NameProperty, "Border");
            factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(2.0));
            factory.SetValue(Border.BorderThicknessProperty, new Thickness(1.0));
            factory.SetValue(Border.BorderBrushProperty, new SolidColorBrush(tintColor.LighterColorBy(-0.5)));
            factory.SetValue(Border.BackgroundProperty, new LinearGradientBrush(tintColor.LighterColorBy(-0.00), tintColor.LighterColorBy(-0.15), 90));
            factory.SetValue(Control.UseLayoutRoundingProperty, true);

            FrameworkElementFactory childFactory = new FrameworkElementFactory(typeof(DockPanel));
            childFactory.SetValue(ContentPresenter.MarginProperty, new Thickness(kMinimumPadding / 2, 2.0, kMinimumPadding / 2, 2.0));
            //childFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            
            FrameworkElementFactory imageViewFactory = new FrameworkElementFactory(typeof(Image));
            imageViewFactory.Name = "Image";
            imageViewFactory.SetValue(Control.NameProperty, "Image");
            imageViewFactory.SetValue(Control.SnapsToDevicePixelsProperty, true);
            imageViewFactory.SetValue(Image.MarginProperty, new Thickness(0.0, 0.0, kMinimumPadding / 4, 0.0));
            imageViewFactory.SetValue(Image.StretchProperty, Stretch.None);
            imageViewFactory.SetValue(Image.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            imageViewFactory.SetValue(Image.VerticalAlignmentProperty, VerticalAlignment.Center);
            imageViewFactory.SetValue(DockPanel.DockProperty, Dock.Left);

            Binding imageBinding = new Binding();
            imageBinding.Path = new PropertyPath(ImageDependencyProperty.ImageProperty);
            imageBinding.RelativeSource = RelativeSource.TemplatedParent;
            imageViewFactory.SetBinding(Image.SourceProperty, imageBinding);


            FrameworkElementFactory contentViewFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentViewFactory.Name = "Content";
            contentViewFactory.SetValue(Control.NameProperty, "Content");
            contentViewFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            contentViewFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentViewFactory.SetValue(ContentPresenter.EffectProperty, GradientBarTextEffect());
            
            if (!shouldHaveDropdownIndicator) {
                contentViewFactory.SetValue(ContentPresenter.MarginProperty, new Thickness(0, 0, kMinimumPadding / 2, 0));
            }

            FrameworkElementFactory menuIndicatorFactory = new FrameworkElementFactory(typeof(Image));
            menuIndicatorFactory.SetValue(Image.StretchProperty, Stretch.None);
            menuIndicatorFactory.SetValue(Image.SourceProperty, DropDownTriangle());
            menuIndicatorFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Right);
            menuIndicatorFactory.SetValue(Image.MarginProperty, new Thickness(0.0, 0.0, kMinimumPadding / 4, 0.0));
            menuIndicatorFactory.SetValue(DockPanel.DockProperty, Dock.Right);

            childFactory.AppendChild(imageViewFactory);
            childFactory.AppendChild(contentViewFactory);

            if (shouldHaveDropdownIndicator) {
                childFactory.AppendChild(menuIndicatorFactory);
            }

            factory.AppendChild(childFactory);
            
            template.VisualTree = factory;

            // Triggers

            Trigger onMouseOver = new Trigger();
            onMouseOver.Property = Button.IsMouseOverProperty;
            onMouseOver.Value = true;
            onMouseOver.Setters.Add(new Setter(Button.BackgroundProperty,
                new LinearGradientBrush(tintColor.LighterColorBy(0.025), tintColor.LighterColorBy(-0.125), 90),
                "Border"));

            Trigger onMouseDown = new Trigger();
            onMouseDown.Property = Button.IsPressedProperty;
            onMouseDown.Value = true;
            onMouseDown.Setters.Add(new Setter(Button.BackgroundProperty,
                new LinearGradientBrush(tintColor.LighterColorBy(-0.15), tintColor.LighterColorBy(-0.30), -90),
                "Border"));
            onMouseDown.Setters.Add(new Setter(ContentPresenter.EffectProperty, null, "Content"));

            template.Triggers.Add(onMouseOver);
            template.Triggers.Add(onMouseDown);

            templateSetter.Value = template;

            buttonStyle.Setters.Add(templateSetter);

            return buttonStyle;
        }

        private Color tintColor;
        private WrapPanel wrapPanel;
        private Border border;

        private const string kGradientBarKVOContext = "kGradientBarKVOContext";
        protected const double kMinimumPadding = 10;

        public KNGradientBar()
            : base() {

            border = new Border();
            border.BorderThickness = new Thickness(0.0, 0.0, 0.0, 1.0);
            border.Padding = new Thickness(kMinimumPadding);
            wrapPanel = new WrapPanel();
            wrapPanel.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            border.Child = wrapPanel;

            Canvas.SetTop(border, 0.0);
            Canvas.SetLeft(border, 0.0);
            Children.Add(border);

            this.AddObserverToKeyPathWithOptions(this, "TintColor", 0, kGradientBarKVOContext);

            TintColor = Colors.White;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);

            border.Width = sizeInfo.NewSize.Width;
            border.Height = sizeInfo.NewSize.Height;

        }

        protected WrapPanel ChildrenContainer {
            get { return wrapPanel; }
        }
                

        public Color TintColor {
            get { return tintColor; }
            set {
                this.WillChangeValueForKey("TintColor");
                tintColor = value;
                this.DidChangeValueForKey("TintColor");
            }
        }


        public void ObserveValueForKeyPathOfObject(string keyPath, object obj, Dictionary<string, object> change, object context) {

            if (ReferenceEquals(context, kGradientBarKVOContext)) {
                if (keyPath.Equals("TintColor")) {

                    LinearGradientBrush background = new LinearGradientBrush(TintColor, TintColor.LighterColorBy(-0.1), 90);
                    this.Background = background;
                    border.BorderBrush = new SolidColorBrush(TintColor.LighterColorBy(-0.5));
                }
            }
        }

            
    }
}
