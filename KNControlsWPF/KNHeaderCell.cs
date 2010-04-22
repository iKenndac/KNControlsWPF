using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Threading;

namespace KNControls {
    public class KNHeaderCell : KNCell {

        private double padding = 10.0;

        private Color textColor = Color.FromRgb(76, 96, 122);
        private Color lineColor = Color.FromRgb(214, 229, 245);
        private Typeface font = new Typeface("Segoe UI");

        public KNTableColumn Column { get; set; }

        public override void RenderInFrame(DrawingContext context, Rect frame) {

            Rect drawableFrame = frame;
            drawableFrame.Inflate(-.5, -.5);

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

            // Line! 

            GradientStop start = new GradientStop(lineColor, 0);
            GradientStop middle = new GradientStop(lineColor, .4);
            GradientStop end = new GradientStop(Colors.Transparent, 1);

            Brush brush = new LinearGradientBrush(new GradientStopCollection(new GradientStop[] { start, middle, end }), 90);
            Pen gradientPen = new Pen(brush, 1);
            context.DrawLine(gradientPen, drawableFrame.TopRight, drawableFrame.BottomRight);
        }

    }
}
