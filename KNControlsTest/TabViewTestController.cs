using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using KNControls;
using KNFoundation;

namespace KNControlsTest {
    class TabViewTestController : KNViewController {

        private KNBasicTabViewItem tabViewItem;
        private TextBox titleField;
        private CheckBox hasButtonCheck;

        public TabViewTestController()
            : base(new TabViewTest()) {

                tabViewItem = new KNBasicTabViewItem();
                tabViewItem.Title = "KNTabView";
                tabViewItem.ViewController = this;

                titleField.Text = tabViewItem.Title;
        }

        private void TitleTextChanged(object sender, EventArgs e) {
            tabViewItem.Title = TitleField.Text;
        }

        private void RemoveTabButton(object sender, EventArgs e) {
            KNTabView tabView = (KNTabView)((FrameworkElement)View.Parent).Parent;
            tabView.LeftControl = null;
        }

        private void AddTabButton(object sender, EventArgs e) {
            KNTabView tabView = (KNTabView)((FrameworkElement)View.Parent).Parent;

            Button button = new Button();
            button.Content = "Button!";
            button.Width = 100.0;
            button.Height = 22.0;
            tabView.LeftControl = button;
        }

        public KNTabViewItem TabViewItem {
            get { return tabViewItem; }
        }

        public TextBox TitleField {
            get { return titleField; }
            set {
                value.TextChanged += TitleTextChanged;
                titleField = value;
            }
        }

        public CheckBox HasButtonCheckBox {
            get { return hasButtonCheck; }
            set {
                value.Checked += AddTabButton;
                value.Unchecked += RemoveTabButton;
                hasButtonCheck = value;
            }
        }

    }
}
