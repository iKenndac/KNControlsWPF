using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using System.Windows.Media;

namespace KNControls {
    public class KNTextGradientBar : KNGradientBar {

        private TextBlock textBlock;

        public KNTextGradientBar()
            : base() {

            textBlock = new TextBlock();
            textBlock.Effect = GradientBarTextEffect();
            textBlock.FontSize = 13.0;
            textBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            ChildrenContainer.Children.Add(textBlock);
        }

        public string Text {
            get { return textBlock.Text; }
            set {
                textBlock.Text = value;
            }
        }

        public void AddButton(Button control) {
            control.Style = ButtonStyleWithTintColor(this.TintColor, false);
            ChildrenContainer.Children.Add(control);
        }

        public void RemoveButton(Button control) {
            ChildrenContainer.Children.Remove(control);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);

            if (sizeInfo.WidthChanged) {
                double textWidth = sizeInfo.NewSize.Width - kMinimumPadding * 2;

                foreach (FrameworkElement element in ChildrenContainer.Children) {
                    if (!element.Equals(textBlock)) {
                        textWidth -= element.DesiredSize.Width;
                    }
                }

                if (textWidth > 0.0) {
                    textBlock.Width = textWidth;
                } else {
                    textBlock.Width = 0.0;
                }
            
            }    
        }

    }
}
