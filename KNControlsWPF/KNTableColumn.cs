using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using KNFoundation.KNKVC;

namespace KNControls {
    public class KNTableColumn : Canvas, KNActionCellDelegate, KNKVOObserver {

        public const double kResizeAreaWidth = 10.0;

        public enum SortPriority {
            Primary = 0,
            Secondary = 1,
            NotUsed = 2
        }

        public enum SortDirection {
            Ascending = 0,
            Descending = 1
        }

        public interface KNTableColumnDelegate {
            void ActionCellPerformedAction(KNActionCell cell, KNTableColumn column);
            object ObjectForRow(int row, KNTableColumn column);
            int RowCountForColumn(KNTableColumn column);
            void HeaderWasClicked(KNTableColumn column);
        }

        private double rowHeight;
        private double scrollOffset;
        private double headerHeight;
        private Thickness contentPadding;

        private KNCell dataCell;
        private KNHeaderCell headerCell;
        
        private string identifier;
        
        private bool userResizable;
        private double minSize;
        private double maxSize;
        private SortPriority sortPriority;
        private SortDirection sortDirection;        

        private KNTableColumnDelegate del;

        private ArrayList cellCache;
        private Dictionary<int, KNCell> activeCells;

        // --


        public KNTableColumn() : this(null, null, null, null) {
        }

        public KNTableColumn(string anIdentifier, string aTitle, KNCell aDataCell, KNTableColumnDelegate aDelegate) {

            ClipToBounds = true;

            activeCells = new Dictionary<int, KNCell>();
            cellCache = new ArrayList();

            sortPriority = SortPriority.NotUsed;
            sortDirection = SortDirection.Ascending;

            if (aDataCell != null) {
                dataCell = aDataCell;
            } else {
                dataCell = new KNTextCell();
            }

            minSize = 100;
            maxSize = 2000;
            Width = 100;
            userResizable = true;
            HeaderCell = new KNHeaderCell();
            headerCell.Column = this;

            del = aDelegate;

            if (anIdentifier != null) {
                identifier = anIdentifier;
            } else {
                identifier = "";
            }

            KNActionCellDependencyProperty.SetDelegate((DependencyObject)headerCell, this);
            if (aTitle != null) {
                KNCellDependencyProperty.SetObjectValue((DependencyObject)headerCell, aTitle);
            } else {
                KNCellDependencyProperty.SetObjectValue((DependencyObject)headerCell, "Column Title");
            }

            this.AddObserverToKeyPathWithOptions(this, "VerticalOffset", 0, null);
            this.AddObserverToKeyPathWithOptions(this, "RowHeight", 0, null);
            this.AddObserverToKeyPathWithOptions(this, "DataCell", 0, null);

            LayoutCompletely();
        }

        public void ObserveValueForKeyPathOfObject(string keyPath, object obj, Dictionary<string, object> change, object context) {
            if (keyPath.Equals("VerticalOffset") || keyPath.Equals("RowHeight")) {
                LayoutCompletely();
            }

            if (keyPath.Equals("DataCell")) {
                
                foreach (KNCell cell in activeCells.Values) {

                    if (typeof(KNActionCell).IsAssignableFrom(cell.GetType())) {
                        KNActionCellDependencyProperty.SetDelegate((DependencyObject)cell, null);
                    }
                    KNCellDependencyProperty.SetObjectValue((DependencyObject)cell, null);
                    Children.Remove((UIElement)cell);
                    cell.PrepareForRecycling();
                }
                activeCells.Clear();
                cellCache.Clear();

                LayoutCompletely();
            }
        }

        private void RecycleCellAtRow(int rowIndex) {
            // This should get called when a cell is scrolled off-screen. 
            // The cell will be cached for later use.

            if (activeCells.ContainsKey(rowIndex)) {
                KNCell cell = activeCells[rowIndex];
                activeCells.Remove(rowIndex);
                Children.Remove((UIElement)cell);
                if (typeof(KNActionCell).IsAssignableFrom(cell.GetType())) {
                    KNActionCellDependencyProperty.SetDelegate((DependencyObject)cell, null);
                }
                KNCellDependencyProperty.SetObjectValue((DependencyObject)cell, null);
                cell.PrepareForRecycling();

                cellCache.Add(cell);
            }
        }

        public KNCell CellForRow(int rowIndex) {

            if (activeCells.ContainsKey(rowIndex)) {
                return activeCells[rowIndex];
            }

            if (cellCache.Count > 0) {
                KNCell cachedCell = (KNCell)cellCache[0];
                Canvas.SetZIndex((UIElement)cachedCell, 0);
                activeCells.Add(rowIndex, cachedCell);
                cellCache.RemoveAt(0);
                cachedCell.PrepareForActivation();
                Children.Add((UIElement)cachedCell);
                if (typeof(KNActionCell).IsAssignableFrom(cachedCell.GetType())) {
                    KNActionCellDependencyProperty.SetDelegate((DependencyObject)cachedCell, null);
                }
                return cachedCell;
            }

            KNCell cell = dataCell.Copy();
            activeCells.Add(rowIndex, cell);
            Canvas.SetZIndex((UIElement)cell, 0);
            cell.PrepareForActivation();
            Children.Add((UIElement)cell);

            if (typeof(KNActionCell).IsAssignableFrom(cell.GetType())) {
                KNActionCellDependencyProperty.SetDelegate((DependencyObject)cell, this);
            }

            return cell;
        }

        private void PrepareForNewRowRange(int firstRow, int lastRow) {
            // Cache active cells that fall outside this range. 

            ArrayList keys = new ArrayList(activeCells.Keys);

            foreach (int rowIndex in keys) {
                if (rowIndex < firstRow || rowIndex > lastRow) {
                    RecycleCellAtRow(rowIndex);
                }
            }
        }
       

        public int RowForCell(KNCell cell) {

            if (!activeCells.ContainsValue(cell)) {
                return -1;
            } else {
                foreach (int row in activeCells.Keys) {
                    if (activeCells[row].Equals(cell)) {
                        return row;
                    }
                }
            }
            return -1;
        }

        public void CellPerformedAction(KNActionCell cell) {

            if (cell == HeaderCell) {

                if (Delegate != null) {
                    Delegate.HeaderWasClicked(this);
                }

            } else {

                if (Delegate != null) {
                    Delegate.ActionCellPerformedAction(cell, this);
                }
            }
        }

        public void ReloadData() {
            LayoutCompletely();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);

            if (sizeInfo.WidthChanged && !sizeInfo.HeightChanged) {
                LayoutHorizontal();
            } else if (sizeInfo.HeightChanged) {
                LayoutCompletely();
            }
        }

        private void LayoutHorizontal() {
            // Nice and simple - make sure everything is the right width.

            HeaderCell.Width = this.Width;

            foreach (FrameworkElement cell in activeCells.Values) {
                cell.Width = this.Width;
            }
        }

        private void LayoutCompletely() {
            // More complex - make sure all cells are correctly positioned.

            // Header isn't affected by scroll position.


            HeaderCell.Height = HeaderHeight;
            HeaderCell.Width = Width;

            int firstRow = (int)Math.Floor((VerticalOffset - ContentPadding.Top) / RowHeight);
            if (firstRow < 0) { firstRow = 0; }

            double firstRowOffset = ((firstRow * RowHeight) - VerticalOffset) + ContentPadding.Top + HeaderCell.Height;
            int visibleRowCount = (int)Math.Ceiling((Height - firstRowOffset) / RowHeight);
            int lastRow = firstRow + visibleRowCount;
            

            if (Delegate != null) {
                int lastAvailableRow = Delegate.RowCountForColumn(this) - 1;
                if (lastRow > lastAvailableRow) {
                    lastRow = lastAvailableRow;
                }
            } else {
                lastRow = 0;
            }

            if (lastRow < firstRow) {
                lastRow = firstRow;
            }

            visibleRowCount = lastRow - firstRow;

            PrepareForNewRowRange(firstRow, lastRow);

            for (int currentVisibleRowIndex = 0; currentVisibleRowIndex <= visibleRowCount; currentVisibleRowIndex++) {

                int currentActualRow = firstRow + currentVisibleRowIndex;

                KNCell cell = CellForRow(currentActualRow);
                Canvas.SetTop((UIElement)cell, firstRowOffset + (currentVisibleRowIndex * RowHeight));
                ((FrameworkElement)cell).Height = RowHeight;
                ((FrameworkElement)cell).Width = Width;

                if (Delegate != null) {
                    KNCellDependencyProperty.SetObjectValue((DependencyObject)cell, Delegate.ObjectForRow(currentActualRow, this));
                }
            } 
        }

        public new double Width {
            get { return base.Width; }
            set {
                this.WillChangeValueForKey("Width");
                base.Width = value;
                this.DidChangeValueForKey("Width");
            }
        }

        public KNCell DataCell {
            get { return dataCell; }
            set {
                this.WillChangeValueForKey("DataCell");
                dataCell = value;
                this.DidChangeValueForKey("DataCell");
            }
        }

        public string Identifier {
            get { return identifier; }
            set {
                this.WillChangeValueForKey("Identifier");
                identifier = value;
                this.WillChangeValueForKey("Identifier");
            }
        }

        public KNHeaderCell HeaderCell {
            get { return headerCell; }
            set {
                this.WillChangeValueForKey("HeaderCell");

                if (headerCell != null) {
                    Children.Remove(headerCell);
                    headerCell.Column = null;
                    KNActionCellDependencyProperty.SetDelegate((DependencyObject)headerCell, null);
                }
                headerCell = value;
                headerCell.Column = this;
                headerCell.Width = Width;
                Canvas.SetZIndex(headerCell, 999);
                Children.Add(headerCell);
                KNActionCellDependencyProperty.SetDelegate((DependencyObject)headerCell, null);
            }
        }

        public bool UserResizable {
            get { return userResizable; }
            set {
                this.WillChangeValueForKey("UserResizable");
                userResizable = value;
                this.DidChangeValueForKey("UserResizable");
            }
        }

        public double RowHeight {
            get { return rowHeight; }
            set {
                this.WillChangeValueForKey("RowHeight");
                rowHeight = value;
                this.DidChangeValueForKey("RowHeight");
            }
        }

        public double HeaderHeight {
            get { return headerHeight; }
            set {
                this.WillChangeValueForKey("HeaderHeight");
                headerHeight = value;
                this.DidChangeValueForKey("HeaderHeight");
            }
        }

        public double MinimumWidth {
            get { return minSize; }
            set {
                this.WillChangeValueForKey("MinimumWidth");
                minSize = value;
                this.DidChangeValueForKey("MinimumWidth");
            }
        }

        public double MaximumWidth {
            get { return maxSize; }
            set {
                this.WillChangeValueForKey("MaximumWidth");
                maxSize = value;
                this.DidChangeValueForKey("MaximumWidth");
            }
        }

        public double VerticalOffset {
            get { return scrollOffset; }
            set {
                this.WillChangeValueForKey("VerticalOffset");
                scrollOffset = value;
                this.DidChangeValueForKey("VerticalOffset");
            }
        }

        public  Thickness ContentPadding {
            get { return contentPadding; }
            set {
                this.WillChangeValueForKey("ContentPadding");
                contentPadding = value;
                this.DidChangeValueForKey("ContentPadding");
            }
        }

        public SortPriority SortingPriority {
            get { return sortPriority; }
            set {
                this.WillChangeValueForKey("SortingPriority");
                sortPriority = value;
                this.DidChangeValueForKey("SortingPriority");
            }
        }

        public SortDirection SortingDirection {
            get { return sortDirection; }
            set {
                this.WillChangeValueForKey("SortingDirection");
                sortDirection = value;
                this.DidChangeValueForKey("SortingDirection");
            }
        }

        public KNTableColumnDelegate Delegate {
            get { return del; }
            set {
                this.WillChangeValueForKey("Delegate");
                del = value;
                this.DidChangeValueForKey("Delegate");
            }
        }

        
    }
}
