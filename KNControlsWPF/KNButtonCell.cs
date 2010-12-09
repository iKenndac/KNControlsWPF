using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using KNControls;
using KNFoundation;
using KNFoundation.KNKVC;

namespace KNControls {
    public class KNButtonCell : Canvas, KNActionCell, KNKVOObserver {

        private Button button;

        public void PrepareForRecycling() { }
        public void PrepareForActivation() { }

        public KNCell Copy() {
            return new KNButtonCell();
        }

        public KNButtonCell() {
            button = new Button();
            button.Height = 23.0;
            button.Width = 20.0;
            button.Click += ButtonWasClicked;
            Children.Add(button);

            this.AddObserverToKeyPathWithOptions(this, "ObjectValue", 0, null);
        }

        private void ButtonWasClicked(object sender, EventArgs e) {

            if (KNActionCellDependencyProperty.GetDelegate(this) != null) {
                KNActionCellDependencyProperty.GetDelegate(this).CellPerformedAction(this);
            }
        }



        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);

            button.Width = sizeInfo.NewSize.Width - 20.0;

            Canvas.SetTop(button, (sizeInfo.NewSize.Height / 2) - (button.Height / 2));
            Canvas.SetLeft(button, (sizeInfo.NewSize.Width / 2) - (button.Width / 2));

        }

        public void ObserveValueForKeyPathOfObject(string keyPath, object obj, Dictionary<string, object> change, object context) {
            if (keyPath.Equals("ObjectValue")) {
                button.Content = ObjectValue();
            }
        }

        private string ObjectValue() {
            if (KNCellDependencyProperty.GetObjectValue(this) == null) {
                return "";
            } else {
                return (string)KNCellDependencyProperty.GetObjectValue(this);
            }
        }
    }
}
