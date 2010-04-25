using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.Drawing;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using KNFoundation;

namespace KNControls {
    public class KNCheckboxCell : KNActionCell {

        private static BitmapSource checkBoxOff, checkBoxOn, checkBoxOffOver, checkBoxOnOver, checkBoxOffDown, checkBoxOnDown;

        private Rect drawnCheckRelativeFrame;
        private BitmapSource checkBoxImage;
        private bool mouseOverCheck;

        

        public override void RenderInFrame(System.Windows.Media.DrawingContext context, System.Windows.Rect frame) {

            if (checkBoxOff == null) {
                DrawCheckBoxes();
            }

            if (checkBoxImage == null) {
                checkBoxImage = (bool)ObjectValue ? checkBoxOn : checkBoxOff;
            }

            Rect checkBoxRect = new Rect(Math.Floor(frame.X + ((frame.Width / 2) - (checkBoxImage.Width / 2))),
                Math.Floor(frame.Y + ((frame.Height / 2) - (checkBoxImage.Height / 2))),
                checkBoxImage.Width,
                checkBoxImage.Height);

            
            context.DrawImage(checkBoxImage, checkBoxRect);
            
            drawnCheckRelativeFrame = checkBoxRect;
            drawnCheckRelativeFrame.X -= frame.X;
            drawnCheckRelativeFrame.Y -= frame.Y;

        
        }

        public override bool MouseDidMoveInCell(System.Windows.Point relativePoint, Rect relativeFrame) {

            if (drawnCheckRelativeFrame.Contains(relativePoint) && !mouseOverCheck) {
                mouseOverCheck = true;
                checkBoxImage = (bool)ObjectValue ? checkBoxOnOver : checkBoxOffOver;
                return true;
            }

            if (!drawnCheckRelativeFrame.Contains(relativePoint) && mouseOverCheck) {
                mouseOverCheck = false;
                checkBoxImage = (bool)ObjectValue ? checkBoxOn : checkBoxOff;
                return true;
            }

            return false;
        }

        public override bool MouseDownInCell(System.Windows.Point relativePoint, Rect relativeFrame) {

            if (drawnCheckRelativeFrame.Contains(relativePoint)) {
                mouseOverCheck = true;
                checkBoxImage = (bool)ObjectValue ? checkBoxOnDown : checkBoxOffDown;
                return true;
            }
            return false;
        }

        public override bool MouseDraggedInCell(System.Windows.Point relativePoint, Rect relativeFrame) {

            if (drawnCheckRelativeFrame.Contains(relativePoint) && !mouseOverCheck) {
                mouseOverCheck = true;
                checkBoxImage = (bool)ObjectValue ? checkBoxOnDown : checkBoxOffDown;
                return true;
            }

            if (!drawnCheckRelativeFrame.Contains(relativePoint) && mouseOverCheck) {
                mouseOverCheck = false;
                checkBoxImage = (bool)ObjectValue ? checkBoxOnOver : checkBoxOffOver;
                return true;
            }

            return false;

        }

        public override bool MouseUpInCell(System.Windows.Point relativePoint, Rect relativeFrame) {

            if (drawnCheckRelativeFrame.Contains(relativePoint)) {
                mouseOverCheck = true;
                ObjectValue = !(bool)ObjectValue;
                if (Delegate != null) {
                    Delegate.CellPerformedAction(this);
                }
                checkBoxImage = (bool)ObjectValue ? checkBoxOnOver : checkBoxOffOver;
                return true;

            } else {
                checkBoxImage = (bool)ObjectValue ? checkBoxOn : checkBoxOff;
                mouseOverCheck = false;
                return true;
            }

        }

        void DrawCheckBoxes() {

            Bitmap im = new Bitmap(1, 1);
            Graphics g = Graphics.FromImage(im);

            System.Drawing.Size checkSize = CheckBoxRenderer.GetGlyphSize(g, CheckBoxState.CheckedNormal);

            Bitmap checkImage = new Bitmap(checkSize.Width, checkSize.Height);
            g = Graphics.FromImage(checkImage);

            // Checked

            g.Clear(Color.Transparent);
            CheckBoxRenderer.DrawCheckBox(g, new System.Drawing.Point(0, 0), CheckBoxState.CheckedNormal);

            checkBoxOn = checkImage.ToBitmapSource();

            // Checked Over

            g.Clear(Color.Transparent);
            CheckBoxRenderer.DrawCheckBox(g, new System.Drawing.Point(0, 0), CheckBoxState.CheckedHot);

            checkBoxOnOver = checkImage.ToBitmapSource();

            // Checked Pressed

            g.Clear(Color.Transparent);
            CheckBoxRenderer.DrawCheckBox(g, new System.Drawing.Point(0, 0), CheckBoxState.CheckedPressed);

            checkBoxOnDown = checkImage.ToBitmapSource();

            // Unchecked

            g.Clear(Color.Transparent);
            CheckBoxRenderer.DrawCheckBox(g, new System.Drawing.Point(0, 0), CheckBoxState.UncheckedNormal);

            checkBoxOff = checkImage.ToBitmapSource();

            // Unchecked Over

            g.Clear(Color.Transparent);
            CheckBoxRenderer.DrawCheckBox(g, new System.Drawing.Point(0, 0), CheckBoxState.UncheckedHot);

            checkBoxOffOver = checkImage.ToBitmapSource();

            // Unchecked Pressed

            g.Clear(Color.Transparent);
            CheckBoxRenderer.DrawCheckBox(g, new System.Drawing.Point(0, 0), CheckBoxState.UncheckedPressed);

            checkBoxOffDown = checkImage.ToBitmapSource();


        }

        public override KNCell Copy() {
            return new KNCheckboxCell();
        }

    }
}
