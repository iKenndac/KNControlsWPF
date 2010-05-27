using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using KNFoundation.KNKVC;
using System.Threading;

namespace KNControls {
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:KNControls.TabView"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:KNControls.TabView;assembly=KNControls.TabView"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:KNTabView/>
    ///
    /// </summary>
    public class KNTabView : Canvas, KNKVOObserver {
       
        protected static Color kDefaultTintColor = Color.FromRgb(245, 245, 245);

        private Border contentCanvas;
        private double tabHeight;

        private const int kTabOverlap = 25;
        private const int kActiveTabZIndex = 2000;
        private const int kContentZIndex = 1000;
        private const int kTabInset = 6;
        private const double kMaxTabWidth = 250.0;

        private Dictionary<KNTabViewItem, KNTabViewTab> itemToTabCache = new Dictionary<KNTabViewItem, KNTabViewTab>();
        private KNTabViewItem[] items;
        private KNTabViewItem activeItem;

        public double TabHeight {
            get { return tabHeight; }
            set {
                this.WillChangeValueForKey("TabHeight");
                tabHeight = value;
                this.DidChangeValueForKey("TabHeight");
            }
        }

        public KNTabViewItem[] Items {
            get { return items; }
            set {
                this.WillChangeValueForKey("Items");
                items = value;
                this.DidChangeValueForKey("Items");
            }
        }

        public KNTabViewItem ActiveItem {
            get { return activeItem; }
            set {
                this.WillChangeValueForKey("ActiveItem");
                activeItem = value;
                this.DidChangeValueForKey("ActiveItem");
            }
        }

        public KNTabView()
            : base() {

            TabHeight = 40.0;

            SnapsToDevicePixels = true;

            contentCanvas = new Border();
            contentCanvas.Background = new SolidColorBrush(Color.FromRgb(245, 245, 245));
            contentCanvas.BorderBrush = new SolidColorBrush(Color.FromArgb(25, 0, 0, 0));
            contentCanvas.BorderThickness = new Thickness(1.0);
            Canvas.SetZIndex(contentCanvas, kContentZIndex);

            DropShadowEffect effect = new DropShadowEffect();
            effect.BlurRadius = 3.0;
            effect.Color = Colors.Black;
            effect.Direction = 90.0;
            effect.ShadowDepth = 0.0;

            contentCanvas.Effect = effect;

            this.Children.Add(contentCanvas);

            this.AddObserverToKeyPathWithOptions(this, "TabHeight", 0, null);
            this.AddObserverToKeyPathWithOptions(this, "ActiveItem", 0, null);
            this.AddObserverToKeyPathWithOptions(this,
                "Items",
                KNKeyValueObservingOptions.KNKeyValueObservingOptionNew |
                KNKeyValueObservingOptions.KNKeyValueObservingOptionOld,
                null);
        }

        private void TabClicked(object sender, EventArgs e) {
            ActiveItem = ((KNTabViewTab)sender).RepresentedObject;
        }

        private void TabResized(object sender, EventArgs e) {
            UpdateTabViewLayout();
            InvalidateArrange();
        }

        public void ObserveValueForKeyPathOfObject(string keyPath, object obj, Dictionary<string, object> change, object context) {
            if (keyPath.Equals("TabHeight")) {
                UpdateTabViewLayout();
                InvalidateArrange();
            } else if (keyPath.Equals("ActiveItem")) {

                if (ActiveItem != null && ActiveItem.ViewController != null) {
                    contentCanvas.Child = ActiveItem.ViewController.View;
                } else {
                    contentCanvas.Child = null;
                }

                if (ActiveItem.TintColor == Color.FromArgb(0,0,0,0)) {
                    contentCanvas.Background = new SolidColorBrush(kDefaultTintColor);
                } else {
                    contentCanvas.Background = new SolidColorBrush(ActiveItem.TintColor);
                }

                UpdateTabViewLayout();
                InvalidateArrange();

            } else if (keyPath.Equals("Items")) {

                KNTabViewItem[] oldItems = (KNTabViewItem[])change.ValueForKey(KNKVOConstants.KNKeyValueChangeOldKey);

                if (oldItems != null) {
                    foreach (KNTabViewItem item in oldItems) {
                        RemoveTabForItem(item);
                    }
                }

                KNTabViewItem[] newItems = (KNTabViewItem[])change.ValueForKey(KNKVOConstants.KNKeyValueChangeNewKey);
                if (newItems != null) {
                    foreach (KNTabViewItem item in newItems) {
                        CreateTabForItem(item);
                    }


                    if (newItems.Length > 0) {
                        ActiveItem = newItems[0];
                    }

                }
                UpdateTabViewLayout();
                InvalidateArrange();
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);

            UpdateTabViewLayout();
            InvalidateArrange();
        }

        private void UpdateTabViewLayout() {
            if (this.ActualWidth > 6.0 && this.ActualHeight > (TabHeight + 1.0)) {

                contentCanvas.Width = this.ActualWidth - 6.0;
                contentCanvas.Height = this.ActualHeight - (TabHeight + 2.0);
                Canvas.SetTop(contentCanvas, TabHeight - 1.0);
                Canvas.SetLeft(contentCanvas, 3.0);

                // Tabs

                if (items != null) {

                    int currentZIndex = kContentZIndex - 1;
                    double tabOffset = kTabInset + kTabOverlap;
                    double tabWidth = 0.0;

                    foreach (KNTabViewItem item in items) {

                        if (!itemToTabCache.ContainsKey(item)) {
                            CreateTabForItem(item);
                        }

                        KNTabViewTab tab = itemToTabCache[item];

                        double idealWidth = tab.IdealWidth();

                        if (idealWidth > tabWidth) {
                            tabWidth = idealWidth;
                        }
                    }

                    if (tabWidth > kMaxTabWidth) {
                        tabWidth = kMaxTabWidth;
                    }

                    double extraWidthFromOverlaps = 0.0;

                    if (items.Length > 1) {
                        extraWidthFromOverlaps = kTabOverlap * (items.Length - 1.0);
                    }

                    double allowableWidth = this.ActualWidth - (kTabInset * 2) + extraWidthFromOverlaps;

                    if ((tabWidth * items.Length) > allowableWidth) {
                        tabWidth = Math.Floor(allowableWidth / items.Length);
                    }

                    foreach (KNTabViewItem item in items) {

                        KNTabViewTab tab = itemToTabCache[item];

                        tab.Height = TabHeight;
                        tab.Width = tabWidth;
                        Canvas.SetLeft(tab, Math.Floor(tabOffset - kTabOverlap));

                        if (item.Equals(activeItem)) {
                            Canvas.SetZIndex(tab, kActiveTabZIndex);
                            tab.IsActive = true;
                        } else {
                            Canvas.SetZIndex(tab, currentZIndex);
                            tab.IsActive = false;
                        }

                        tabOffset += tab.Width - kTabOverlap;
                        currentZIndex--;
                    }
                }
            }
        }


        private void CreateTabForItem(KNTabViewItem item) {

            KNTabViewTab tab = new KNTabViewTab();
            this.Children.Add(tab);
            tab.RepresentedObject = item;
            tab.TabWasClicked += TabClicked;
            tab.TabMayWantNewSize += TabResized;
            itemToTabCache.Add(item, tab);
        }

        private void RemoveTabForItem(KNTabViewItem item) {

            if (itemToTabCache.ContainsKey(item)) {

                KNTabViewTab tab = itemToTabCache[item];
                this.Children.Remove(tab);
                tab.RepresentedObject = null;
                itemToTabCache.Remove(item);
            }
        }

        internal class KNTabViewTab : Canvas, KNKVOObserver {

            private const double kTabCurveWidth = 20.0;
            private const double kContentPadding = 2.0;

            private PathGeometry tabPath;

            private bool active;
            private bool mouseOver;
            private bool mouseDown;
            private FormattedText formattedText;

            private KNTabViewItem representedObject;

            public event EventHandler<EventArgs> TabWasClicked;
            public event EventHandler<EventArgs> TabMayWantNewSize;

            public KNTabViewTab()
                : base() {

                this.Width = 150.0;
                this.Height = 20.0;

                this.ClipToBounds = true;
                this.AddObserverToKeyPathWithOptions(this, "IsActive", KNKeyValueObservingOptions.KNKeyValueObservingOptionInitial, null);
                this.AddObserverToKeyPathWithOptions(this, "RepresentedObject", KNKeyValueObservingOptions.KNKeyValueObservingOptionOld | KNKeyValueObservingOptions.KNKeyValueObservingOptionNew, null);
            }

            public KNTabViewItem RepresentedObject {
                get { return representedObject; }
                set {
                    this.WillChangeValueForKey("RepresentedObject");
                    representedObject = value;
                    this.DidChangeValueForKey("RepresentedObject");
                }
            }

            public bool IsActive {
                get { return active; }
                set {
                    if (active != value) {
                        this.WillChangeValueForKey("IsActive");
                        active = value;
                        this.DidChangeValueForKey("IsActive");
                    }
                }
            }

            public double IdealWidth() {

                double idealWidth = kTabCurveWidth * 2;
                idealWidth += kContentPadding;

                if (RepresentedObject != null) {

                    if (RepresentedObject.Icon != null) {
                        idealWidth += ImageSize().Width + (kContentPadding / 2);
                    }

                    if (RepresentedObject.Title != null) {
                        FormattedText txt = new FormattedText(RepresentedObject.Title,
                               Thread.CurrentThread.CurrentCulture,
                               FlowDirection.LeftToRight,
                               new Typeface("Segoe UI"),
                               12,
                               Brushes.Black);

                        txt.TextAlignment = TextAlignment.Left;
                        txt.MaxTextWidth = kMaxTabWidth - idealWidth - (kContentPadding * 2);
                        idealWidth += txt.Width;
                    }

                }

                idealWidth += kContentPadding * 2;

                return idealWidth;
            }

            public void UpdateGeometry() {

                Rect tabRect = new Rect(2.0, 3.0, this.ActualWidth - 4.0, this.ActualHeight - 2.0);
                tabRect.Inflate(-.5, -.5);
                Rect leftTabCurveRect = new Rect(tabRect.Left, tabRect.Top, kTabCurveWidth, tabRect.Height);
                Rect rightTabCurveRect = new Rect(tabRect.Left + tabRect.Width - kTabCurveWidth, tabRect.Top, kTabCurveWidth, tabRect.Height);

                PathFigure tabFigure = new PathFigure();

                tabFigure.StartPoint = tabRect.BottomLeft;

                // Left curve
                tabFigure.Segments.Add(new BezierSegment(new Point(leftTabCurveRect.Left + (kTabCurveWidth / 2), leftTabCurveRect.Bottom),
                    new Point(leftTabCurveRect.Right - (kTabCurveWidth / 2), leftTabCurveRect.Top),
                    leftTabCurveRect.TopRight,
                    true));

                // Top line
                tabFigure.Segments.Add(new LineSegment(rightTabCurveRect.TopLeft, true));

                // Right curve
                tabFigure.Segments.Add(new BezierSegment(new Point(rightTabCurveRect.Left + (kTabCurveWidth / 2), rightTabCurveRect.Top),
                    new Point(rightTabCurveRect.Right - (kTabCurveWidth / 2), rightTabCurveRect.Bottom),
                    rightTabCurveRect.BottomRight,
                    true));

                PathGeometry tabGeometry = new PathGeometry();
                tabGeometry.Figures.Add(tabFigure);

                tabPath = tabGeometry;
            }

            public void ObserveValueForKeyPathOfObject(string keyPath, object obj, Dictionary<string, object> change, object context) {
                if (keyPath.Equals("IsActive")) {

                    if (IsActive) {

                        DropShadowEffect effect = new DropShadowEffect();
                        effect.BlurRadius = 3.0;
                        effect.Color = Colors.Black;
                        effect.Direction = 90.0;
                        effect.ShadowDepth = 0.0;

                        Effect = effect;

                    } else {

                        DropShadowEffect effect = new DropShadowEffect();
                        effect.BlurRadius = 2.0;
                        effect.Color = Colors.DarkGray;
                        effect.Direction = 90.0;
                        effect.ShadowDepth = 0.0;

                        Effect = null;

                    }
                    InvalidateVisual();

                } else if (keyPath.Equals("RepresentedObject")) {

                    KNTabViewItem oldItem = (KNTabViewItem)change.ValueForKey(KNKVOConstants.KNKeyValueChangeOldKey);
                    if (oldItem != null) {
                        oldItem.RemoveObserverFromKeyPath(this, "Title");
                        oldItem.RemoveObserverFromKeyPath(this, "Icon");
                        oldItem.RemoveObserverFromKeyPath(this, "TintColor");
                    }

                    KNTabViewItem newItem = (KNTabViewItem)change.ValueForKey(KNKVOConstants.KNKeyValueChangeNewKey);
                    if (newItem != null) {
                        newItem.AddObserverToKeyPathWithOptions(this, "Title", 0, null);
                        newItem.AddObserverToKeyPathWithOptions(this, "Icon", 0, null);
                        newItem.AddObserverToKeyPathWithOptions(this, "TintColor", 0, null);
                    }

                    formattedText = null;
                    InvalidateVisual();

                } else if (keyPath.Equals("Title")) {

                    EventHandler<EventArgs> handler = TabMayWantNewSize;
                    if (handler != null) {
                        handler(this, null);
                    }

                    formattedText = null;
                    InvalidateVisual();

                } else if (keyPath.Equals("Icon")) {
                    
                    EventHandler<EventArgs> handler = TabMayWantNewSize;
                    if (handler != null) {
                        handler(this, null);
                    }
                    InvalidateVisual();
                } else if (keyPath.Equals("TintColor")) {
                    InvalidateVisual();
                }
            }

            protected override void OnMouseEnter(MouseEventArgs e) {
                base.OnMouseEnter(e);
                mouseOver = true;
                InvalidateVisual();

                e.Handled = true;
            }

            protected override void OnMouseLeave(MouseEventArgs e) {
                base.OnMouseLeave(e);
                mouseOver = false;
                InvalidateVisual();

                e.Handled = true;
            }

            protected override void OnMouseDown(MouseButtonEventArgs e) {
                base.OnMouseDown(e);
                mouseDown = true;
                InvalidateVisual();

                e.Handled = true;
            }

            protected override void OnMouseUp(MouseButtonEventArgs e) {
                base.OnMouseUp(e);
                mouseDown = false;

                Rect myFrame = new Rect(0.0, 0.0, ActualWidth, ActualHeight);

                if (myFrame.Contains(e.GetPosition(this))) {

                    EventHandler<EventArgs> handler = TabWasClicked;
                    if (handler != null) {
                        handler(this, null);
                    }
                }

                e.Handled = true;

                InvalidateVisual();
            }

            private Size ImageSize() {
                Size imageSize;

                if (ActualHeight < 36.0) {
                    imageSize = new Size(16.0, 16.0);
                } else if (ActualHeight < 50.0) {
                    imageSize = new Size(32.0, 32.0);
                } else {
                    imageSize = new Size(48.0, 48.0);
                }
                return imageSize;
            }

            protected override void OnRender(DrawingContext dc) {
                base.OnRender(dc);

                if (tabPath == null) {
                    UpdateGeometry();
                }

                Color baseColor = RepresentedObject.TintColor;
                if (baseColor == Color.FromArgb(0,0,0,0)) {
                    baseColor = kDefaultTintColor;
                }

                if (IsActive) {

                    Color topColor = baseColor.LighterColorBy(0.15);

                    GradientBrush brush = new LinearGradientBrush(topColor, baseColor, 90.0);
                    dc.DrawGeometry(brush, new Pen(new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)), 1.0), tabPath);

                } else {

                    if (mouseOver && mouseDown) {

                        Color downColor = baseColor.LighterColorBy(0);
                        downColor.A = 225;

                        dc.DrawGeometry(new SolidColorBrush(downColor),
                            new Pen(new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)), 1.0),
                            tabPath);

                    } else if (mouseDown || mouseOver) {

                        Color overColor = baseColor.LighterColorBy(.1);
                        overColor.A = 225;

                        dc.DrawGeometry(new SolidColorBrush(overColor),
                            new Pen(new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)), 1.0),
                            tabPath);
                    
                    } else {

                        Color background = baseColor.LighterColorBy(.2);
                        background.A = 225;

                        dc.DrawGeometry(new SolidColorBrush(background),
                            new Pen(new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)), 1.0),
                            tabPath);
                    }
                }

                // Contents

                if (RepresentedObject != null) {

                    double contentXOffset = kTabCurveWidth + kContentPadding;

                    // Icon

                    if (RepresentedObject.Icon != null) {

                        Size imageSize = ImageSize();

                        Rect imageRect = new Rect(new Point(contentXOffset, 2.0 + ((ActualHeight / 2) - (imageSize.Height / 2))),
                            imageSize);

                        dc.DrawImage(RepresentedObject.Icon, imageRect);

                        contentXOffset += imageSize.Width + (kContentPadding / 2);

                    }

                    // Text

                    if (RepresentedObject.Title != null) {

                        if (formattedText == null) {
                            formattedText = new FormattedText(RepresentedObject.Title,
                               Thread.CurrentThread.CurrentCulture,
                               FlowDirection.LeftToRight,
                               new Typeface("Segoe UI"),
                               12,
                               Brushes.Black);

                            formattedText.TextAlignment = TextAlignment.Left;
                        }

                        formattedText.MaxTextWidth = ActualWidth - kTabCurveWidth - contentXOffset - kContentPadding;
                        formattedText.MaxTextHeight = ActualHeight;

                        double textHeight = formattedText.Height;
                        Point textPoint = new Point(contentXOffset, 2.0 +((ActualHeight / 2) - (textHeight / 2)));


                        dc.DrawText(formattedText, textPoint);

                    }
                }
            }

            protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
                base.OnRenderSizeChanged(sizeInfo);

                UpdateGeometry();

                InvalidateVisual();
            }

            
        }
    }


}
