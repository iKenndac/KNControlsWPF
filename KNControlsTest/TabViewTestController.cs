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
        private Button alertPanelButton;

        public TabViewTestController()
            : base(new TabViewTest()) {

                tabViewItem = new KNBasicTabViewItem();
                tabViewItem.Title = "KNTabView";
                tabViewItem.ViewController = this;

                titleField.Text = tabViewItem.Title;
        }

        private void AlertPanelButtonClicked(object sender, EventArgs e) {

            KNAlertPanel panel = KNAlertPanel.AlertWithMessageTextAndButtons(
                "The document A Document has unsaved changes. Woul you like to save them before closing?",
                "Is you close without saving, your changes will be lost.",
                "Save",
                "Alternate",
                "Other"
                );

            int returnCode = panel.ShowDialog();

            if (returnCode == KNAlertPanel.KNAlertPanelDefaultReturn) {
                MessageBox.Show("Default");
            } else if (returnCode == KNAlertPanel.KNAlertPanelAlternateReturn) {
                MessageBox.Show("Alternate");
            } else if (returnCode == KNAlertPanel.KNAlertPanelOtherReturn) {
                MessageBox.Show("Other");
            } else if (returnCode == KNAlertPanel.KNAlertPanelCloseWidgetReturn) {
                MessageBox.Show("Close Widget");
            }

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

        public Button AlertPanelButton {
            get { return alertPanelButton;  }
            set {
                value.Click += AlertPanelButtonClicked;
                alertPanelButton = value;
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
