using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using KNControls;
using KNFoundation;

namespace KNControlsTest {
    class TabViewTestController : KNViewController {

        private KNTabViewItem tabViewItem;
        private TextBox titleField;

        public TabViewTestController()
            : base(new TabViewTest()) {

                tabViewItem = new KNTabViewItem();
                tabViewItem.Title = "KNTabView";
                tabViewItem.ViewController = this;

                titleField.Text = tabViewItem.Title;
        }

        private void TitleTextChanged(object sender, EventArgs e) {
            tabViewItem.Title = TitleField.Text;
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

    }
}
