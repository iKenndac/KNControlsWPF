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
using System.Windows.Navigation;
using System.Windows.Shapes;
using KNControls;
using KNFoundation;
using System.Reflection;

using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace KNControlsTest {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            Loaded += DidLoadYay;

        }


        


        private void DidLoadYay(object sender, EventArgs e) {

            System.Windows.Forms.Application.EnableVisualStyles();

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = Assembly.GetExecutingAssembly().GetManifestResourceStream("KNControlsTest.SourcePlaylist.png");
            image.EndInit();

            KNViewController tableController = new KNViewController(new TableView());

            KNBasicTabViewItem tableItem = new KNBasicTabViewItem();
            tableItem.ViewController = tableController;
            tableItem.Title = "KNTableView";
            tableItem.TintColor = Colors.White;
            tableItem.Icon = image;

            TabViewTestController tabTest = new TabViewTestController();

            kNTabView1.Items = new KNTabViewItem[] { tableItem, tabTest.TabViewItem };
        }

    }
}
