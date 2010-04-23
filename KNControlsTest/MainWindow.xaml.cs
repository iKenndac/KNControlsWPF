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
    public partial class MainWindow : Window, KNTableView.KNTableViewDataSource, KNTableView.KNTableViewDelegate {
        public MainWindow() {
            InitializeComponent();

            Loaded += DidLoadYay;

        }

        private string[] elements = { "a", "b", "c" };

        private void DidLoadYay(object sender, EventArgs e) {

            kNTableView1.Columns = new KNTableColumn[] { new KNTableColumn("test", new KNTextCell(), null),
                new KNTableColumn("test", new KNTextCell(), null),
                new KNTableColumn("test", new KNTextCell(), null) };

            kNTableView1.DataSource = this;
            kNTableView1.Delegate = this;
            kNTableView1.ReloadData();
        }


        public int NumberOfItemsInTableView(KNTableView table) {
            return elements.Length;
        }

        public object ObjectForRow(KNTableView table, KNTableColumn column, int rowIndex) {
            return elements[rowIndex];
        }

        public bool TableViewShouldSelectRow(KNTableView table, int rowIndex) {
            return true;
        }

        public KNTableColumn.SortDirection TableViewWillSortByColumnWithSuggestedSortOrder(KNTableView table, KNTableColumn column, KNTableColumn.SortDirection suggestedNewSortOrder) {

            Array.Sort(elements);

            if (suggestedNewSortOrder == KNTableColumn.SortDirection.Descending) {
                Array.Reverse(elements);
            }
            
            return suggestedNewSortOrder;
        }
    }
}
