using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KNControls {
    /// <summary>
    /// Interaction logic for KNAlertPanel.xaml
    /// </summary>
    public partial class KNAlertPanel : Window {

        public const int KNAlertPanelDefaultReturn = 0;
        public const int KNAlertPanelAlternateReturn = 1;
        public const int KNAlertPanelOtherReturn = 2;
        public const int KNAlertPanelCloseWidgetReturn = 3;

        private int returnCode = -1;

        public static KNAlertPanel AlertWithMessageTextAndButtons(
            string messageText, 
            string descriptionText,
            string defaultButtonTitle,
            string alternateButtonTitle,
            string otherButtonTitle
            ) {

                KNAlertPanel panel = new KNAlertPanel();
                panel.TitleText.Text = messageText;
                panel.DescriptionText.Text = descriptionText;

                if (String.IsNullOrWhiteSpace(defaultButtonTitle)) {
                    panel.DefaultButton.Content = "OK";
                } else {
                    panel.DefaultButton.Content = defaultButtonTitle;
                }

                if (String.IsNullOrWhiteSpace(alternateButtonTitle)) {
                    panel.AlternateButton.Visibility = Visibility.Collapsed;
                } else {
                    panel.AlternateButton.Content = alternateButtonTitle;
                }

                if (String.IsNullOrWhiteSpace(otherButtonTitle)) {
                    panel.OtherButton.Visibility = Visibility.Collapsed;
                } else {
                    panel.OtherButton.Content = otherButtonTitle;
                }

                return panel;
        }

        public KNAlertPanel() {
            InitializeComponent();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
            base.OnClosing(e);
            if (returnCode == -1) {
                returnCode = KNAlertPanelCloseWidgetReturn;
            }
        }

        public new int ShowDialog() {

            base.ShowDialog();
            return returnCode;

        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e) {
            returnCode = KNAlertPanelDefaultReturn;
            Close();
        }

        private void AlternateButton_Click(object sender, RoutedEventArgs e) {
            returnCode = KNAlertPanelAlternateReturn;
            Close();
        }

        private void OtherButton_Click(object sender, RoutedEventArgs e) {
            returnCode = KNAlertPanelOtherReturn;
            Close();
        }
    }
}
