using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace KNControls {
    public class KNImageCell : KNCell {

        public override KNCell Copy() {
            return new KNImageCell();
        }

        public override void RenderInFrame(DrawingContext context, Rect frame) {

            ImageSource image = (ImageSource)ObjectValue;

            if (image != null) {

                Rect imageRect;

                double pixelWidth = (image.Width / 96.0) * 72.0;
                double pixelHeight = (image.Height / 96.0) * 72.0;

                if (pixelWidth <= frame.Width && pixelHeight <= frame.Height) {

                   // Image fits!!

                    imageRect = new Rect(frame.X + ((frame.Width / 2) - (pixelWidth / 2)),
                    frame.Y + ((frame.Height / 2) - (pixelHeight / 2)),
                    pixelWidth,
                    pixelHeight);

                } else {

                    if (image.Width > image.Height) {

                        double widthToHeightRatio = image.Height / image.Width;
                        double newHeight = frame.Width * widthToHeightRatio;
                        
                        if (newHeight > frame.Height) {
                            newHeight = frame.Height;
                        }

                        double newWidth = newHeight / widthToHeightRatio;

                        imageRect = new Rect(frame.X + ((frame.Width / 2) - (newWidth / 2)),
                            frame.Y + ((frame.Height / 2) - (newHeight / 2)),
                            newWidth,
                            newHeight);

                    } else {

                        double heightToWidthRatio = image.Width / image.Height;
                        double newWidth = frame.Height * heightToWidthRatio;

                        if (newWidth > frame.Width) {
                            newWidth = frame.Width;
                        }

                        double newHeight = newWidth / heightToWidthRatio;

                        imageRect = new Rect(frame.X + ((frame.Width / 2) - (newWidth / 2)),
                            frame.Y + ((frame.Height / 2) - (newHeight / 2)),
                            newWidth,
                            newHeight);
                    }

                }

                imageRect.X = Math.Floor(imageRect.X);
                imageRect.Y = Math.Floor(imageRect.Y);

                context.DrawImage(image, imageRect);
            }

        }

    }
}
