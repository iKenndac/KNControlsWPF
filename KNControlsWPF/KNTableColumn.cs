using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KNFoundation.KNKVC;

namespace KNControls {
    public class KNTableColumn : KNActionCellDelegate {

        public enum SortPriority {
            Primary = 0,
            Secondary = 1,
            NotUsed = 2
        }

        public interface KNTableColumnDelegate {
            void ActionCellPerformedAction(KNActionCell cell, KNTableColumn column);
        }

        private KNCell cell;
        private KNHeaderCell headerCell;
        private int width;
        private string identifier;

        private bool userResizable;
        private int minSize;
        private int maxSize;
        private SortPriority sortPriority;

        private KNTableColumnDelegate del;

        private Dictionary<int, KNCell> cells;

        public KNTableColumn() {

            cells = new Dictionary<int, KNCell>();
            sortPriority = SortPriority.NotUsed;
            cell = new KNTextCell();
            width = 100;
            minSize = 100;
            maxSize = 2000;
            userResizable = true;
            headerCell = new KNHeaderCell();
            headerCell.ObjectValue = "Column Title";

        }

        public KNTableColumn(string anIdentifier, KNCell aDataCell, KNTableColumnDelegate aDelegate)
            : this() {

                identifier = anIdentifier;
                cell = aDataCell;
                del = aDelegate;
        }

        public KNCell CellForRow(int row) {

            if (!cells.ContainsKey(row)) {

                KNCell newCell = cell.Copy();

                if (typeof(KNActionCell).IsAssignableFrom(newCell.GetType())) {
                    ((KNActionCell)newCell).Delegate = this;
                }

                cells.Add(row, cell);
            }
            return cells[row];
        }

        public int RowForCell(KNCell cell) {

            if (!cells.ContainsValue(cell)) {
                return -1;
            } else {
                foreach (int row in cells.Keys) {
                    if (cells[row].Equals(cell)) {
                        return row;
                    }
                }
            }
            return -1;
        }

        public void CellPerformedAction(KNActionCell cell) {
            if (Delegate != null) {
                Delegate.ActionCellPerformedAction(cell, this);
            }
        }

        // -----

        public int Width {
            get { return width; }
            set {
                this.WillChangeValueForKey("Width");
                width = value;
                this.DidChangeValueForKey("Width");
            }
        }

        public KNCell DataCell {
            get { return cell; }
            set {
                this.WillChangeValueForKey("DataCell");
                cell = value;
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
                    headerCell.Column = null;
                }
                headerCell = value;
                headerCell.Column = this;
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

        public int MinimumWidth {
            get { return minSize; }
            set {
                this.WillChangeValueForKey("MinimumWidth");
                minSize = value;
                this.DidChangeValueForKey("MinimumWidth");
            }
        }

        public int MaximumWidth {
            get { return maxSize; }
            set {
                this.WillChangeValueForKey("MaximumWidth");
                maxSize = value;
                this.DidChangeValueForKey("MaximumWidth");
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
