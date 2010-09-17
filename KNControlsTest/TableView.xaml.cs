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
using System.Reflection;

namespace KNControlsTest {
    /// <summary>
    /// Interaction logic for TableView.xaml
    /// </summary>
    public partial class TableView : UserControl, KNTableView.KNTableViewDataSource, KNTableView.KNTableViewDelegate {

        
        private string[] elements = { "a", "b", "c", "a", "b", "c", "a", "b", "c", "a", "b", "c" };
        private bool[] bools = { true, false, true, true, false, true, true, false, true, true, false, true };
        private BitmapImage image;

        public TableView() {
            InitializeComponent();
            this.Loaded += ViewDidLoad;
        }

        private void ViewDidLoad(object sender, EventArgs e) {

            image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = Assembly.GetExecutingAssembly().GetManifestResourceStream("KNControlsTest.iPodGeneration1.png");
            image.EndInit();

            kNTableView1.Columns = new KNTableColumn[] { new KNTableColumn("progress", "Progress Wheel", new KNProgressWheelCell(), null),
                new KNTableColumn("image", "Image",new KNImageCell(), null),
                new KNTableColumn("bool", "Checkbox",new KNCheckboxCell(), null),
                new KNTableColumn("test", "Text",new KNTextCell(), null),
                new KNTableColumn("test", "Text",new KNTextCell(), null) };

            kNTableView1.Columns[0].MinimumWidth = 10;
            //kNTableView1.RowHeight = 40.0;
            kNTableView1.DataSource = this;
            kNTableView1.Delegate = this;
            kNTableView1.ReloadData();
            kNTableView1.AlternatingRows = true;
            //kNTableView1.HeaderHeight = 0;
            kNTableView1.RowSelectionStyle = KNTableView.SelectionStyle.WindowsExplorer;
            kNTableView1.VerticalScrollBarVisibility = KNTableView.ScrollBarVisibility.Automatic;
            kNTableView1.HorizontalScrollBarVisibility = KNTableView.ScrollBarVisibility.Automatic;

            kNTableView1.Focus();

        }

        public int NumberOfItemsInTableView(KNTableView table) {
            return elements.Length;
        }

        public object ObjectForRow(KNTableView table, KNTableColumn column, int rowIndex) {

            if (column.Identifier.Equals("image")) {
                return image;
            } else if (column.Identifier.Equals("bool")) {
                return bools[rowIndex];
            } else {
                return elements[rowIndex];
            }
        }

        public void CellPerformedAction(KNTableView view, KNTableColumn column, KNActionCell cell, int rowIndex) {

            if (column.Identifier.Equals("bool")) {
                bools[rowIndex] = (bool)KNCellDependencyProperty.GetObjectValue((DependencyObject)cell);
            }
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


        public bool TableViewDelegateShouldBeginDragOperationWithObjectsAtIndexes(KNTableView table, System.Collections.ArrayList rowIndexes) {
            //MessageBox.Show("Drag");

            DragDrop.DoDragDrop(table, "test", DragDropEffects.Copy);

            return true;
        }


        public bool TableViewDelegateShouldShowContextualMenuWithObjectsAtIndexes(KNTableView table, System.Collections.ArrayList rowIndexes) {
            return false;
        }

        public bool ShouldDeleteObjectsAtRows(KNTableView table, System.Collections.ArrayList rowIndexes) {
            return false;
        }
    }
}
