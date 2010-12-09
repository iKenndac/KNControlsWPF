using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Threading;
using System.Windows.Controls;
using KNFoundation.KNKVC;

namespace KNControls {
    public class KNTextCell : DockPanel, KNCell, KNKVOObserver {

        private TextBlock textBlock;

        public KNTextCell() {

            textBlock = new TextBlock();
            this.Children.Add(textBlock);

            this.AddObserverToKeyPathWithOptions(this, "TextFont", 0, null);
            this.AddObserverToKeyPathWithOptions(this, "TextAlignment", 0, null);
            this.AddObserverToKeyPathWithOptions(this, "HorizontalPadding", 0, null);
            this.AddObserverToKeyPathWithOptions(this, "TextColor", 0, null);
            this.AddObserverToKeyPathWithOptions(this, "HighlightedTextColor", 0, null);
            this.AddObserverToKeyPathWithOptions(this, "ObjectValue", 0, null);

            TextFont = new FontFamily("Segoe UI");
            TextAlignment = TextAlignment.Left;
            TextSize = 11.75;
            HorizontalPadding = 5.0;

            textBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            
            TextColor = Colors.Black;
            HighlightedTextColor = Colors.Black;

        }

        public void PrepareForRecycling() {}
        public void PrepareForActivation() {}

        public void ObserveValueForKeyPathOfObject(string keyPath, object obj, Dictionary<string, object> change, object context) {
            
            if (keyPath.Equals("TextFont")) {
                textBlock.FontFamily = TextFont;
            }

            if (keyPath.Equals("TextAlignment")) {
                textBlock.TextAlignment = TextAlignment;
            }

            if (keyPath.Equals("TextColor")) {
                textBlock.Foreground = new SolidColorBrush(TextColor);
            }

            if (keyPath.Equals("HorizontalPadding")) {
                textBlock.Margin = new Thickness(HorizontalPadding, 0, HorizontalPadding, 0);
            }

            if (keyPath.Equals("ObjectValue")) {
                object objValue = KNCellDependencyProperty.GetObjectValue(this);

                if (objValue != null) {
                    textBlock.Text = objValue.ToString();
                } else {
                    textBlock.Text = "";
                }
            }


        }

        public object ObjectValue() {
            return KNCellDependencyProperty.GetObjectValue(this);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);
        }

        public KNCell Copy() {
            KNTextCell cell = new KNTextCell();
            cell.TextAlignment = this.TextAlignment;
            cell.TextSize = this.TextSize;
            cell.TextFont = this.TextFont;
            cell.TextColor = this.TextColor;
            cell.TextAlignment = this.TextAlignment;
            cell.HighlightedTextColor = this.HighlightedTextColor;
            cell.HorizontalPadding = this.HorizontalPadding;

            return cell;
        }

        private TextAlignment alignment;
        private double size;
        private FontFamily font;
        private Color color;
        private Color highlightedColor;
        private double horizontalPadding;

        public double HorizontalPadding {
            get { return horizontalPadding; }
            set {
                if (value != horizontalPadding) {
                    this.WillChangeValueForKey("HorizontalPadding");
                    horizontalPadding = value;
                    this.DidChangeValueForKey("HorizontalPadding");
                }
            }
        }

        public TextAlignment TextAlignment {
            get { return alignment; }
            set {
                if (value != alignment) {
                    this.WillChangeValueForKey("TextAlignment");
                    alignment = value;
                    this.DidChangeValueForKey("TextAlignment");
                }
            }
        }

        public double TextSize {
            get { return size; }
            set {
                if (size != value) {
                    this.WillChangeValueForKey("TextSize");
                    size = value;
                    this.DidChangeValueForKey("TextSize");
                }
            }
        }

        public FontFamily TextFont {
            get { return font; }
            set {
                if (font != value) {
                    this.WillChangeValueForKey("TextFont");
                    font = value;
                    this.DidChangeValueForKey("TextFont");
                }
            }
        }

        public Color TextColor {
            get { return color; }
            set {
                if (color != value) {
                    this.WillChangeValueForKey("TextColor");
                    color = value;
                    this.DidChangeValueForKey("TextColor");
                }
            }
        }

        public Color HighlightedTextColor {
            get { return highlightedColor; }
            set {
                if (highlightedColor != value) {
                    this.WillChangeValueForKey("HighlightedTextColor");
                    highlightedColor = value;
                    this.DidChangeValueForKey("HighlightedTextColor");
                }
            }
        }
    }
}
