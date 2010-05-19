using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Threading;
using KNFoundation.KNKVC;

namespace KNControls {
    public class KNTextCell : KNCell, KNKVOObserver {

        private FormattedText formattedText;
        private Rect lastFrame = Rect.Empty;
        private Point lastTextPoint;

        public KNTextCell() {

            TextFont = new Typeface("Segoe UI");
            TextAlignment = TextAlignment.Left;
            TextSize = 11.75;
            HorizontalPadding = 5.0;
            
            TextColor = Colors.Black;
            HighlightedTextColor = Colors.Black;

            this.AddObserverToKeyPathWithOptions(this, "TextFont", 0, null);
            this.AddObserverToKeyPathWithOptions(this, "TextAlignment", 0, null);
            this.AddObserverToKeyPathWithOptions(this, "HorizontalPadding", 0, null);
            this.AddObserverToKeyPathWithOptions(this, "TextColor", 0, null);
            this.AddObserverToKeyPathWithOptions(this, "HighlightedTextColor", 0, null);
            this.AddObserverToKeyPathWithOptions(this, "ObjectValue", 0, null);
        }

        public void ObserveValueForKeyPathOfObject(string keyPath, object obj, Dictionary<string, object> change, object context) {
            formattedText = null;
            lastFrame = Rect.Empty;
        }

        public override void RenderInFrame(DrawingContext context, Rect frame) {

            if (ObjectValue != null && ObjectValue.GetType().Equals(typeof(String))) {

                if (formattedText == null) {
                    formattedText = new FormattedText((string)ObjectValue,
                       Thread.CurrentThread.CurrentCulture,
                       FlowDirection.LeftToRight,
                       TextFont,
                       TextSize,
                       new SolidColorBrush(Highlighted ? HighlightedTextColor : TextColor));
                    
                    formattedText.TextAlignment = TextAlignment;
                }

                if (lastFrame.IsEmpty || lastFrame != frame) {
                    lastFrame = frame;

                    formattedText.MaxTextWidth = frame.Width - HorizontalPadding * 2;
                    formattedText.MaxTextHeight = frame.Height;

                    double textHeight = formattedText.Height;
                    lastTextPoint = new Point(frame.X + HorizontalPadding, frame.Y + ((frame.Height / 2) - (textHeight / 2)));
                }

                context.DrawText(formattedText, lastTextPoint);

            }
        }

        public override KNCell Copy() {
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
        private Typeface font;
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

        public Typeface TextFont {
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
