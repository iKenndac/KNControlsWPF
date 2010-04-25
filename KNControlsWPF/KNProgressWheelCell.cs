using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Windows.Threading;

namespace KNControls {

    public enum ControlSize {
        Regular = 0,
        Small = 1
    }

    public class KNProgressWheelCell : KNCell {

        private const double kRegularWidth = 32.0;
        private const double kSmallWidth = 16.0;
        private Size kRegularBarSize = new Size(3.0, 9.0);
        private Size kSmallBarSize = new Size(1.5, 5.0);
        private Color kBarColor = Color.FromRgb(25, 25, 25);
        private byte kMinAlpha = 30;
        private TimeSpan kTickDuration = TimeSpan.FromSeconds(0.5 / 12);
        private DispatcherTimer timer;

        public override KNCell Copy() {
            return new KNProgressWheelCell();
        }

        public KNProgressWheelCell() {

            timer = new DispatcherTimer();
            timer.Interval = kTickDuration;
            timer.Tick += TimerTicked;
            timer.Start();

        }

        private void TimerTicked(object sender, EventArgs e) {
            leadBarAngle += 30.0;

            if (leadBarAngle >= 360.0) {
                leadBarAngle -= 360.0;
            }

            if (ParentControl != null) {
                ParentControl.UpdateCell(this);
            }
        }

        public override void RenderInFrame(DrawingContext context, Rect frame) {

            Rect drawingFrame;
            ControlSize effectiveSize;

            if (IndicatorSize == ControlSize.Regular && frame.Width >= 32.0 && frame.Height >= 32.0) {

                drawingFrame = new Rect(frame.X + ((frame.Width / 2) - kRegularWidth / 2),
                    frame.Y + ((frame.Height / 2) - kRegularWidth / 2),
                    kRegularWidth,
                    kRegularWidth);

                effectiveSize = ControlSize.Regular;

            } else {

                drawingFrame = new Rect(frame.X + ((frame.Width / 2) - kSmallWidth / 2),
                    frame.Y + ((frame.Height / 2) - kSmallWidth / 2),
                    kSmallWidth,
                    kSmallWidth);

                effectiveSize = ControlSize.Small;
            }

            drawingFrame.X = Math.Floor(drawingFrame.X);
            drawingFrame.Y = Math.Floor(drawingFrame.Y);

            Rect barRect = new Rect(drawingFrame.X + ((drawingFrame.Width / 2) - ((effectiveSize == ControlSize.Regular ? kRegularBarSize.Width : kSmallBarSize.Width) / 2)),
                drawingFrame.Y /* + ((drawingFrame.Height / 2) - ((effectiveSize == ControlSize.Regular ? kRegularBarSize.Height : kSmallBarSize.Height) / 2))*/,
                (effectiveSize == ControlSize.Regular ? kRegularBarSize.Width : kSmallBarSize.Width),
                (effectiveSize == ControlSize.Regular ? kRegularBarSize.Height : kSmallBarSize.Height));

            for (double degrees = 0.0; degrees < 360.0; degrees += 30.0) {

                context.PushTransform(new RotateTransform(degrees,
                    drawingFrame.Left + (drawingFrame.Width / 2),
                    drawingFrame.Top + (drawingFrame.Height / 2)));

                context.DrawRoundedRectangle(new SolidColorBrush(ColorForBarAtDegrees(degrees)), null, barRect, 1.0, 1.0);

                context.Pop();

            }
        }

        double leadBarAngle = 0.0;

        private Color ColorForBarAtDegrees(double degrees) {

            double alphaStepPerDegree = (255.0 - kMinAlpha) / 360.0;
            double effectiveAngleFromLeadBar = DifferenceBetweenAngles(leadBarAngle, degrees);

            return Color.FromArgb((byte)(255 - (effectiveAngleFromLeadBar * alphaStepPerDegree)), kBarColor.R, kBarColor.G, kBarColor.B);
        }

        private double DifferenceBetweenAngles(double firstAngle, double secondAngle) {
            double difference = secondAngle - firstAngle;
            while (difference < -180) difference += 360;
            while (difference > 180) difference -= 360;
            if (difference < 0) {
                difference += 360.0;
            }
            return 360.0 - difference;
        }


        public ControlSize IndicatorSize { get; set; }
    }
}
