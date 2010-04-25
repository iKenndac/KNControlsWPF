using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Reflection;

namespace KNControls {
    public class KNHeaderCell : KNActionCell {

        private static BitmapImage ascendingImage, descendingImage;

        private double padding = 10.0;

        private Color textColor = Color.FromRgb(76, 96, 122);
        private Color lineColor = Color.FromRgb(214, 229, 245);
        private Typeface font = new Typeface("Segoe UI");
        private double resizeDragOffset;

        private bool mouseOver;
        private bool mouseDownForHeaderPress;

        public KNTableColumn Column { get; set; }

        public override void RenderInFrame(DrawingContext context, Rect frame) {

            Rect drawableFrame = frame;
            drawableFrame.Inflate(-.5, -.5);

            if (mouseDownForHeaderPress && mouseOver) {

                Color outerBoxTopColor = Color.FromRgb(193, 204, 218);
                Color outerBoxBottomColor = Color.FromRgb(192,203,217);
                Color firstShadowLineColor = Color.FromRgb(215,222,231);
                Color secondShadowLineColor = Color.FromRgb(235,238,242);
                Color fillColor = Color.FromRgb(246,247,248);

                Rect headerHighlightFrame = drawableFrame;

                context.DrawRectangle(new SolidColorBrush(fillColor),
                    new Pen(new LinearGradientBrush(outerBoxTopColor, outerBoxBottomColor, 90), 1.0),
                    headerHighlightFrame);

                headerHighlightFrame.Inflate(0, -1);
                context.DrawLine(new Pen(new SolidColorBrush(firstShadowLineColor), 1.0), headerHighlightFrame.TopLeft, headerHighlightFrame.TopRight);

                headerHighlightFrame.Inflate(0, -1);
                context.DrawLine(new Pen(new SolidColorBrush(secondShadowLineColor), 1.0), headerHighlightFrame.TopLeft, headerHighlightFrame.TopRight);


            } else if (mouseDownForHeaderPress || mouseOver) {

                Color outerBoxTopColor = Color.FromRgb(222, 233, 247);
                Color outerBoxBottomColor = Color.FromRgb(227, 232, 238);
                Color innerBoxColor = Color.FromRgb(253, 254, 255);
                Color fillTopColor = Color.FromRgb(243, 248, 253);
                Color fillBottomColor = Color.FromRgb(239, 243, 249);

                Rect headerHighlightFrame = drawableFrame;
                // The top line of the header highlight box is offscreen.
                headerHighlightFrame.Y -= 1.0;
                headerHighlightFrame.Height += 1.0;

                context.DrawRectangle(null,
                    new Pen(new LinearGradientBrush(outerBoxTopColor, outerBoxBottomColor, 90), 1.0), 
                    headerHighlightFrame);

                headerHighlightFrame.Inflate(-1.0, -1.0);

                context.DrawRectangle(new LinearGradientBrush(fillTopColor, fillBottomColor, 90),
                    new Pen(new SolidColorBrush(innerBoxColor), 1.0),
                    headerHighlightFrame);

            } else {
                // Vertical line

                GradientStop start = new GradientStop(lineColor, 0);
                GradientStop middle = new GradientStop(lineColor, .5);
                GradientStop end = new GradientStop(Colors.Transparent, 1);

                Brush brush = new LinearGradientBrush(new GradientStopCollection(new GradientStop[] { start, middle, end }), 90);
                Pen gradientPen = new Pen(brush, 1);
                context.DrawLine(gradientPen, drawableFrame.TopRight, drawableFrame.BottomRight);
            }
            
            // Text

            FormattedText text = new FormattedText((string)ObjectValue,
                Thread.CurrentThread.CurrentCulture,
                FlowDirection.LeftToRight,
                font,
                11.75,
                new SolidColorBrush(textColor));

            text.MaxTextWidth = frame.Width - padding * 2;
            text.MaxTextHeight = frame.Height;

            
            Point origin = new Point(frame.X + padding, (frame.Y + (frame.Height / 2)) - (text.Height / 2));

            context.DrawText(text, origin);

            // Triangle

            if (Column.SortingPriority == KNTableColumn.SortPriority.Primary) {

                ImageSource image;

                if (Column.SortingDirection == KNTableColumn.SortDirection.Ascending) {

                    if (ascendingImage == null) {
                        ascendingImage = new BitmapImage();
                        ascendingImage.BeginInit();
                        ascendingImage.StreamSource = Assembly.GetExecutingAssembly().GetManifestResourceStream("KNControls.Images.Ascending.png");
                        ascendingImage.EndInit();
                    }

                    image = ascendingImage;

                } else {

                    if (descendingImage == null) {
                        descendingImage = new BitmapImage();
                        descendingImage.BeginInit();
                        descendingImage.StreamSource = Assembly.GetExecutingAssembly().GetManifestResourceStream("KNControls.Images.Descending.png");
                        descendingImage.EndInit();
                    }
                    image = descendingImage;

                }

                Rect imageFrame = new Rect();
                imageFrame.Y = 2.0;
                imageFrame.Width = image.Width;
                imageFrame.Height = image.Height;
                imageFrame.X = frame.X + (frame.Width / 2) - (imageFrame.Width / 2);

                context.DrawImage(image, imageFrame);

            }
        }

        public override bool MouseDidMoveInCell(Point relativePoint, Rect relativeFrame) {

            if (relativeFrame.Contains(relativePoint) && !mouseOver) {
                mouseOver = true;
                return true;
            }

            if (!relativeFrame.Contains(relativePoint) && mouseOver) {
                mouseOver = false;
                return true;
            }

            return false;
        }

        public override bool MouseDownInCell(Point relativePoint, Rect relativeFrame) {

            double offset = relativeFrame.Right - relativePoint.X;

            if (offset <= KNTableColumn.kResizeAreaWidth) {
                resizeDragOffset = offset;
            } else {
                mouseDownForHeaderPress = true;
            }

            return true;
        }

        public override bool MouseDraggedInCell(Point relativePoint, Rect relativeFrame) {

            if (!mouseDownForHeaderPress) {
                if (Column.UserResizable) {

                    double suggestedWidth = (relativePoint.X - relativeFrame.X) + resizeDragOffset;

                    if (suggestedWidth < Column.MinimumWidth) {
                        suggestedWidth = Column.MinimumWidth;
                    }

                    if (suggestedWidth > Column.MaximumWidth) {
                        suggestedWidth = Column.MaximumWidth;
                    }

                    if (Column.Width != suggestedWidth) {
                        Column.Width = (int)suggestedWidth;
                        return true;
                    }
                }
            } else {
                if (relativeFrame.Contains(relativePoint) && !mouseOver) {
                    mouseOver = true;
                    return true;
                }

                if (!relativeFrame.Contains(relativePoint) && mouseOver) {
                    mouseOver = false;
                    return true;
                }
            }

            return false;
        }

        public override bool MouseUpInCell(Point relativePoint, Rect relativeFrame) {

            if (relativeFrame.Contains(relativePoint)) {
                if (mouseDownForHeaderPress) {

                    // Clicked! Do something...

                    if (Delegate != null) {
                        Delegate.CellPerformedAction(this);
                    }
                }
            }
            mouseDownForHeaderPress = false;
            return false;
        }

    }
}
