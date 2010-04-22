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

namespace KNControlsTest {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, KNTableView.KNTableViewDataSource {
        public MainWindow() {
            InitializeComponent();

            Loaded += DidLoadYay;

        }

       

        private void DidLoadYay(object sender, EventArgs e) {

            kNTableView1.Columns = new KNTableColumn[] { new KNTableColumn("test", new KNTextCell(), null),
                new KNTableColumn("test", new KNTextCell(), null),
                new KNTableColumn("test", new KNTextCell(), null) };

            kNTableView1.SelectedRows = new int[] { 5 };
            kNTableView1.DataSource = this;
            kNTableView1.ReloadData();
        }


        public int NumberOfItemsInTableView(KNTableView table) {
            return 100;
        }

        public object ObjectForRow(KNTableView table, KNTableColumn column, int rowIndex) {
            return "This is a fairly long string to test";
        }
    }
}
