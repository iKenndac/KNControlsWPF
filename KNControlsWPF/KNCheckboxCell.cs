using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using KNFoundation;
using KNFoundation.KNKVC;

namespace KNControls {
    public class KNCheckboxCell : Canvas, KNActionCell, KNKVOObserver {

        private CheckBox checkBox;

        public void PrepareForRecycling() {}
        public void PrepareForActivation() {}

        public KNCell Copy() {
            return new KNCheckboxCell();
        }

        public KNCheckboxCell() {
            checkBox = new CheckBox();
            checkBox.Width = 13.0;
            checkBox.Height = 13.0;
            checkBox.Checked += CheckBoxWasChecked;
            checkBox.Unchecked += CheckBoxWasUnchecked;
            Children.Add(checkBox);

            this.AddObserverToKeyPathWithOptions(this, "ObjectValue", 0, null);
        }

        private void CheckBoxWasChecked(object sender, EventArgs e) {
            
            KNCellDependencyProperty.SetObjectValue(this, true);
            if (KNActionCellDependencyProperty.GetDelegate(this) != null) {
                KNActionCellDependencyProperty.GetDelegate(this).CellPerformedAction(this);
            }
        }

        private void CheckBoxWasUnchecked(object sender, EventArgs e) {

            KNCellDependencyProperty.SetObjectValue(this, false);
            if (KNActionCellDependencyProperty.GetDelegate(this) != null) {
                KNActionCellDependencyProperty.GetDelegate(this).CellPerformedAction(this);
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);

            Canvas.SetTop(checkBox, (sizeInfo.NewSize.Height / 2) - (checkBox.Height / 2));
            Canvas.SetLeft(checkBox, (sizeInfo.NewSize.Width / 2) - (checkBox.Width / 2));
        }

        public void ObserveValueForKeyPathOfObject(string keyPath, object obj, Dictionary<string, object> change, object context) {
            if (keyPath.Equals("ObjectValue")) {
                checkBox.IsChecked = ObjectValue();
            }
        }

        private bool ObjectValue() {
            if (KNCellDependencyProperty.GetObjectValue(this) == null) {
                return false;
            } else {
                return (bool)KNCellDependencyProperty.GetObjectValue(this);
            }
        }

        /*
        public void RenderInFrame(System.Windows.Media.DrawingContext context, System.Windows.Rect frame) {

            if (checkBoxOff == null) {
                DrawCheckBoxes();
            }
            
            if (checkBoxImage == null) {
                checkBoxImage = (bool)KNCellDependencyProperty.GetObjectValue(this) ? checkBoxOn : checkBoxOff;
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

        public bool MouseDidMoveInCell(System.Windows.Point relativePoint, Rect relativeFrame) {

            if (drawnCheckRelativeFrame.Contains(relativePoint) && !mouseOverCheck) {
                mouseOverCheck = true;
                checkBoxImage = (bool)KNCellDependencyProperty.GetObjectValue(this) ? checkBoxOnOver : checkBoxOffOver;
                return true;
            }

            if (!drawnCheckRelativeFrame.Contains(relativePoint) && mouseOverCheck) {
                mouseOverCheck = false;
                checkBoxImage = (bool)KNCellDependencyProperty.GetObjectValue(this) ? checkBoxOn : checkBoxOff;
                return true;
            }

            return false;
        }

        public bool MouseDownInCell(System.Windows.Point relativePoint, Rect relativeFrame) {

            if (drawnCheckRelativeFrame.Contains(relativePoint)) {
                mouseOverCheck = true;
                checkBoxImage = (bool)KNCellDependencyProperty.GetObjectValue(this) ? checkBoxOnDown : checkBoxOffDown;
                return true;
            }
            return false;
        }

        public bool MouseDraggedInCell(System.Windows.Point relativePoint, Rect relativeFrame) {

            if (drawnCheckRelativeFrame.Contains(relativePoint) && !mouseOverCheck) {
                mouseOverCheck = true;
                checkBoxImage = (bool)KNCellDependencyProperty.GetObjectValue(this) ? checkBoxOnDown : checkBoxOffDown;
                return true;
            }

            if (!drawnCheckRelativeFrame.Contains(relativePoint) && mouseOverCheck) {
                mouseOverCheck = false;
                checkBoxImage = (bool)KNCellDependencyProperty.GetObjectValue(this) ? checkBoxOnOver : checkBoxOffOver;
                return true;
            }

            return false;

        }

        public bool MouseUpInCell(System.Windows.Point relativePoint, Rect relativeFrame) {

            if (drawnCheckRelativeFrame.Contains(relativePoint)) {
                mouseOverCheck = true;
                KNCellDependencyProperty.SetObjectValue(this, !(bool)KNCellDependencyProperty.GetObjectValue(this));
                if (KNActionCellDependencyProperty.GetDelegate(this) != null) {
                    KNActionCellDependencyProperty.GetDelegate(this).CellPerformedAction(this);
                }
                checkBoxImage = (bool)KNCellDependencyProperty.GetObjectValue(this) ? checkBoxOnOver : checkBoxOffOver;
                return true;

            } else {
                checkBoxImage = (bool)KNCellDependencyProperty.GetObjectValue(this) ? checkBoxOn : checkBoxOff;
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
          
         */

    }
}
