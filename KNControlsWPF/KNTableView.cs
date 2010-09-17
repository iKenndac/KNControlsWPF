using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using KNFoundation.KNKVC;

namespace KNControls {
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:KNControls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:KNControls;assembly=KNControls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:KNTableView/>
    ///
    /// </summary>
    public class KNTableView : Canvas, KNCellContainer, KNTableColumn.KNTableColumnDelegate, KNKVOObserver {
        static KNTableView() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(KNTableView), new FrameworkPropertyMetadata(typeof(KNTableView)));
        }

        public interface KNTableViewDataSource {
            int NumberOfItemsInTableView(KNTableView table);
            object ObjectForRow(KNTableView table, KNTableColumn column, int rowIndex);
            void CellPerformedAction(KNTableView view, KNTableColumn column, KNActionCell cell, int rowIndex);
            bool ShouldDeleteObjectsAtRows(KNTableView table, ArrayList rowIndexes);
        }

        public interface KNTableViewDelegate {
            bool TableViewShouldSelectRow(KNTableView table, int rowIndex);
            KNTableColumn.SortDirection TableViewWillSortByColumnWithSuggestedSortOrder(KNTableView table, KNTableColumn column, KNTableColumn.SortDirection suggestedNewSortOrder);
            bool TableViewDelegateShouldBeginDragOperationWithObjectsAtIndexes(KNTableView table, ArrayList rowIndexes);
            bool TableViewDelegateShouldShowContextualMenuWithObjectsAtIndexes(KNTableView table, ArrayList rowIndexes);
        }

        public enum SelectionStyle {
            Flat = 0,
            SourceList = 1,
            WindowsExplorer = 2
        }

        public enum ScrollBarVisibility {
            Visible = 0,
            Hidden = 1, 
            Automatic = 2
        }

        private const string kColumnWidthKVOContext = "kColumnWidthKVOContext";
        private const int kBackgroundZIndex = 10;
        private const int kSelectionZIndex = 20;
        private const int kContentZIndex = 30;
        private const int kOverlayZIndex = 999;
        //IVARS

        ScrollBar verticalScrollbar;
        ScrollBar horizontalScrollbar;
        double headerHeight;
        double rowHeight;
        ScrollBarVisibility horizontalScrollbarVisibility;
        ScrollBarVisibility verticalScrollbarVisibility;
        ArrayList visibleColumns = new ArrayList();
        ArrayList selectionLayers = new ArrayList();
        private KNTableColumn[] columns;
        StackPanel columnStack;
        private Thickness contentPadding;

        private int actualRowCount = 0;
        private double virtualHeight = 0.0;
        private double virtualWidth = 0.0;
        Rect availableContentArea = Rect.Empty;

        public KNTableView() {

            this.Focusable = true;
            this.Focus();
            this.ClipToBounds = true;
            this.SnapsToDevicePixels = true;

            columnStack = new StackPanel();
            columnStack.Orientation = Orientation.Horizontal;
            Canvas.SetZIndex(columnStack, kContentZIndex);
            columnStack.ClipToBounds = true;

            Children.Add(columnStack);

            AllowMultipleSelection = true;

            verticalScrollbar = new ScrollBar();
            horizontalScrollbar = new ScrollBar();

            Canvas.SetZIndex(verticalScrollbar, kOverlayZIndex);
            Canvas.SetZIndex(horizontalScrollbar, kOverlayZIndex);

            verticalScrollbar.ValueChanged += VerticalScrollbarDidScroll;
            horizontalScrollbar.ValueChanged += HorizontalScrollBarDidScroll;

            verticalScrollbar.Width = 18.0;
            horizontalScrollbar.Height = 18.0;
            horizontalScrollbar.Orientation = Orientation.Horizontal;

            Children.Add(verticalScrollbar);
            Children.Add(horizontalScrollbar);

            Canvas.SetBottom(horizontalScrollbar, 0.0);
            Canvas.SetRight(verticalScrollbar, 0.0);

            BackgroundColor = Colors.White;
            SelectedRows = new ArrayList();
            RowHeight = 22.0;
            Columns = new KNTableColumn[] {};
            AlternateRowColor = Color.FromRgb(237, 243, 254);

            HeaderHeight = 24.0;
            RowSelectionStyle = SelectionStyle.WindowsExplorer;

            horizontalScrollbar.SmallChange = RowHeight;

            VerticalScrollBarVisibility = ScrollBarVisibility.Automatic;
            HorizontalScrollBarVisibility = ScrollBarVisibility.Automatic;

            ContentPadding = new Thickness(20.0);

            this.AddObserverToKeyPathWithOptions(this, "HeaderHeight", 0, null);
            this.AddObserverToKeyPathWithOptions(this, "VerticalScrollBarVisibility", 0, null);
            this.AddObserverToKeyPathWithOptions(this, "HorizontalScrollBarVisibility", 0, null);
            this.AddObserverToKeyPathWithOptions(this, "ContentPadding", 0, null);
            this.AddObserverToKeyPathWithOptions(this, "RowHeight", 0, null);
            this.AddObserverToKeyPathWithOptions(this, "SelectedRows", 0, null);

            this.AddObserverToKeyPathWithOptions(this,
                "Columns",
                KNKeyValueObservingOptions.KNKeyValueObservingOptionNew | KNKeyValueObservingOptions.KNKeyValueObservingOptionOld,
                null);
        }

        #region Delegates

        public void ObserveValueForKeyPathOfObject(string keyPath, object obj, Dictionary<string, object> change, object context) {

            if (context != null && context.Equals(kColumnWidthKVOContext)) {
                RebuildBasicLayout();
                RebuildColumnLayout();
                return;
            }

            if (keyPath.Equals("SelectedRows")) {
                RebuildSelectionLayout();
            }

            if (keyPath.Equals("HeaderHeight")) {
                foreach (KNTableColumn column in Columns) {
                    column.HeaderHeight = HeaderHeight;
                }
            }

            if (keyPath.Equals("RowHeight")) {
                foreach (KNTableColumn column in Columns) {
                    column.RowHeight = RowHeight;
                }
            }

            if (keyPath.Equals("ContentPadding")) {
                foreach (KNTableColumn column in Columns) {
                    column.RowHeight = RowHeight;
                }
            }

            if (keyPath.Equals("ContentPadding") || keyPath.Equals("Width") || keyPath.Equals("RowHeight") || keyPath.Equals("HeaderHeight") || keyPath.Equals("VerticalScrollBarVisibility") || keyPath.Equals("HorizontalScrollBarVisibility")) {
                RebuildBasicLayout();
            }

            if (keyPath.Equals("Columns")) {

                KNTableColumn[] oldColumns = (KNTableColumn[])change.ValueForKey(KNKVOConstants.KNKeyValueChangeOldKey);

                if (oldColumns != null) {
                    foreach (KNTableColumn column in oldColumns) {
                        column.RemoveObserverFromKeyPath(this, "Width");
                    }
                }

                KNTableColumn[] newColumns = (KNTableColumn[])change.ValueForKey(KNKVOConstants.KNKeyValueChangeNewKey);

                if (newColumns != null) {
                    foreach (KNTableColumn column in oldColumns) {
                        column.AddObserverToKeyPathWithOptions(this, "Width", 0, kColumnWidthKVOContext);
                    }
                }

                foreach (KNTableColumn column in Columns) {
                    column.RowHeight = RowHeight;
                    column.HeaderHeight = HeaderHeight;
                    column.ContentPadding = ContentPadding;
                }

                RebuildColumnLayout();
            }

        }

        public KNCellContainer Control() {
            return this;
        }

        public void ActionCellPerformedAction(KNActionCell cell, KNTableColumn column) {

            if (DataSource != null) {
                DataSource.CellPerformedAction(this, column, cell, column.RowForCell(cell));
                ReloadData();
            }
        }

        public object ObjectForRow(int row, KNTableColumn column) {

            if (DataSource != null) {
                return DataSource.ObjectForRow(this, column, row);
            } else {
                return null;
            }
        }

        public int RowCountForColumn(KNTableColumn column) {
            if (DataSource != null) {
                return DataSource.NumberOfItemsInTableView(this);
            } else {
                return 0;
            }
        }

        public void HeaderWasClicked(KNTableColumn column) {

            if (Delegate != null) {

                KNTableColumn.SortDirection newDirection = KNTableColumn.SortDirection.Ascending;

                foreach (KNTableColumn aColumn in Columns) {

                    if (aColumn.SortingPriority == KNTableColumn.SortPriority.Primary) {
                        aColumn.SortingPriority = KNTableColumn.SortPriority.Secondary;

                        if (aColumn == column) {
                            // Flip direction

                            if (aColumn.SortingDirection == KNTableColumn.SortDirection.Ascending) {
                                newDirection = KNTableColumn.SortDirection.Descending;
                            } else {
                                newDirection = KNTableColumn.SortDirection.Ascending;
                            }

                        } else {
                            newDirection = aColumn.SortingDirection;
                        }
                    } else {
                        aColumn.SortingPriority = KNTableColumn.SortPriority.NotUsed;
                    }

                }

                newDirection = Delegate.TableViewWillSortByColumnWithSuggestedSortOrder(this, column, newDirection);

                column.SortingDirection = newDirection;
                column.SortingPriority = KNTableColumn.SortPriority.Primary;

                ReloadData();

            }
        }

        #endregion

        public void ReloadData() {

            if (DataSource != null) {

                //Check selections

                ArrayList updatedSelection = new ArrayList(SelectedRows);

                foreach (int index in SelectedRows) {
                    if (index >= actualRowCount) {
                        updatedSelection.Remove(index);
                    }
                }

                if (updatedSelection.Count != SelectedRows.Count) {
                    SelectedRows = updatedSelection;
                }

            }

            RebuildBasicLayout();
            RebuildColumnLayout();
            RebuildSelectionLayout();

            foreach (KNTableColumn column in visibleColumns) {
                column.ReloadData();
            }

            InvalidateVisual();
        }

        #region Events
        
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);

            RebuildBasicLayout();
            RebuildColumnLayout();
            RebuildSelectionLayout();

            InvalidateVisual();
        }

        private void VerticalScrollbarDidScroll(object sender, EventArgs e) {
            foreach (KNTableColumn column in Columns) {
                column.VerticalOffset = verticalScrollbar.Value;
            }
            RebuildSelectionLayout();
            InvalidateVisual();
        }

        private void HorizontalScrollBarDidScroll(object sender, EventArgs e) {
            RebuildColumnLayout();
            RebuildSelectionLayout();
            InvalidateVisual();
        }
        
        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);

            if (e.Key == Key.Up) {
                SelectUpOneRow((e.KeyboardDevice.Modifiers & ModifierKeys.Shift) != 0);
                e.Handled = true;
            }

            if (e.Key == Key.Down) {
                SelectDownOneRow((e.KeyboardDevice.Modifiers & ModifierKeys.Shift) != 0);
                e.Handled = true;
            }

            if (e.Key == Key.Home) {
                EnsureRowIsVisible(0);
                e.Handled = true;
            }

            if (e.Key == Key.End) {
                if (DataSource != null) {
                    EnsureRowIsVisible(DataSource.NumberOfItemsInTableView(this) - 1);
                    e.Handled = true;
                }
            }

            if (e.Key == Key.PageUp) {
                verticalScrollbar.Value -= verticalScrollbar.LargeChange;
                e.Handled = true;
            }

            if (e.Key == Key.PageDown) {
                verticalScrollbar.Value += verticalScrollbar.LargeChange;
                e.Handled = true;
            }

            if (e.Key == Key.Delete) {
                if (DataSource != null & SelectedRows.Count > 0) {
                    if (DataSource.ShouldDeleteObjectsAtRows(this, SelectedRows)) {
                        // We should handle the selection
                        if (SelectedRows.Count > 0) {
                            if ((int)SelectedRows[0] < DataSource.NumberOfItemsInTableView(this)) {
                                SelectRowAtIndex((int)SelectedRows[0], false);
                            } else {
                                SelectRowAtIndex(DataSource.NumberOfItemsInTableView(this) - 1, false);
                            }
                        }
                        ReloadData();
                    }
                    e.Handled = true;
                }
            }
        }

        #endregion

        #region Layout

        private void RebuildBasicLayout() {

            if (this.ActualHeight == 0.0 && this.ActualWidth == 0.0) {
                return;
            }

            if (DataSource == null) {
                return;
            }

            actualRowCount = DataSource.NumberOfItemsInTableView(this);
            virtualHeight = (actualRowCount * RowHeight) + ContentPadding.Top + ContentPadding.Bottom + HeaderHeight;

            virtualWidth = ContentPadding.Left + ContentPadding.Right;
            foreach (KNTableColumn column in Columns) {
                virtualWidth += column.Width;
            }

            // Calculate whether to show scroll bar(s) or not!

            Boolean horizontalScrollBarWillBeShown = false;
            Boolean verticalScrollBarWillBeShown = false;

            Rect proposedContentAreaWithScrollBars = new Rect(0,
                   0,
                   ActualWidth - verticalScrollbar.ActualWidth,
                   ActualHeight - horizontalScrollbar.ActualHeight);

            Rect proposedContentAreaWithoutScrollBars = new Rect(0,
               0,
               ActualWidth,
               ActualHeight);

            if (VerticalScrollBarVisibility == ScrollBarVisibility.Automatic &&
                HorizontalScrollBarVisibility == ScrollBarVisibility.Automatic) {

                    if (virtualWidth <= proposedContentAreaWithScrollBars.Width && virtualHeight <= proposedContentAreaWithScrollBars.Height) {
                        horizontalScrollBarWillBeShown = false;
                        verticalScrollBarWillBeShown = false;

                    } else if (virtualWidth > proposedContentAreaWithoutScrollBars.Width && virtualHeight <= proposedContentAreaWithScrollBars.Height) {
                        horizontalScrollBarWillBeShown = true;
                        verticalScrollBarWillBeShown = false;

                    } else if (virtualWidth <= proposedContentAreaWithScrollBars.Width && virtualHeight > proposedContentAreaWithoutScrollBars.Height) {
                        horizontalScrollBarWillBeShown = false;
                        verticalScrollBarWillBeShown = true;

                        // At this point in the tree, at least one bound is within where scroll bars might be!
                       } else if (virtualWidth > proposedContentAreaWithoutScrollBars.Width && virtualHeight > proposedContentAreaWithoutScrollBars.Height) {
                        horizontalScrollBarWillBeShown = true;
                        verticalScrollBarWillBeShown = true;

                    } else if (virtualWidth > proposedContentAreaWithScrollBars.Width) {

                        // Virtual width is where a vertical scroll bar might be. Do we need to show a horizontal scroll bar?

                        if (virtualHeight > proposedContentAreaWithoutScrollBars.Height) {
                            horizontalScrollBarWillBeShown = true;
                            verticalScrollBarWillBeShown = true;

                        } else if (virtualHeight <= proposedContentAreaWithScrollBars.Height) {
                            horizontalScrollBarWillBeShown = false;
                            verticalScrollBarWillBeShown = false;

                        } else {
                            horizontalScrollBarWillBeShown = true;
                            verticalScrollBarWillBeShown = true;

                        }


                    } else if (virtualHeight > proposedContentAreaWithScrollBars.Height) {

                        // Virtual height is where a horizontal scroll bar might be. Do we need to show a vertical scroll bar?

                        if (virtualWidth > proposedContentAreaWithoutScrollBars.Width) {
                            horizontalScrollBarWillBeShown = true;
                            verticalScrollBarWillBeShown = true;

                        } else if (virtualWidth <= proposedContentAreaWithScrollBars.Width) {
                            horizontalScrollBarWillBeShown = false;
                            verticalScrollBarWillBeShown = false;

                        } else {
                            horizontalScrollBarWillBeShown = true;
                            verticalScrollBarWillBeShown = true;

                        }

                    } else {
                        horizontalScrollBarWillBeShown = false;
                        verticalScrollBarWillBeShown = false;
                    }



            } else {

                Boolean showingHorizontalBarWouldRequireVerticalBar = virtualHeight > proposedContentAreaWithScrollBars.Height &&
                    virtualHeight <= proposedContentAreaWithoutScrollBars.Height;
                Boolean showingVerticalBarWouldRequireHorizontalBar = virtualWidth > proposedContentAreaWithScrollBars.Width &&
                    virtualWidth <= proposedContentAreaWithoutScrollBars.Width;

                
                if (VerticalScrollBarVisibility == ScrollBarVisibility.Visible) {
                    verticalScrollBarWillBeShown = true;
                } else if (VerticalScrollBarVisibility == ScrollBarVisibility.Automatic) {
                    verticalScrollBarWillBeShown = (virtualHeight > proposedContentAreaWithoutScrollBars.Height) ||
                        (HorizontalScrollBarVisibility == ScrollBarVisibility.Visible && showingHorizontalBarWouldRequireVerticalBar);
                }

                if (HorizontalScrollBarVisibility == ScrollBarVisibility.Visible) {
                    horizontalScrollBarWillBeShown = true;
                } else if (HorizontalScrollBarVisibility == ScrollBarVisibility.Automatic) {
                    horizontalScrollBarWillBeShown = (virtualWidth > proposedContentAreaWithoutScrollBars.Width) ||
                         (VerticalScrollBarVisibility == ScrollBarVisibility.Visible && showingVerticalBarWouldRequireHorizontalBar);
                }

            }

            availableContentArea = new Rect(0,
                0, 
                ActualWidth - (verticalScrollBarWillBeShown ? verticalScrollbar.Width : 0.0), 
                ActualHeight - (horizontalScrollBarWillBeShown ? horizontalScrollbar.Height : 0.0));

            double newVerticalScrollBarLargeChange = availableContentArea.Height - HeaderHeight;
            if (verticalScrollbar.LargeChange != newVerticalScrollBarLargeChange) {
                verticalScrollbar.LargeChange = newVerticalScrollBarLargeChange;
            }

            double newVerticalScrollBarMaximum = virtualHeight - availableContentArea.Height;
            if (verticalScrollbar.Maximum != newVerticalScrollBarMaximum) {
                verticalScrollbar.Maximum = newVerticalScrollBarMaximum;
            }

            System.Windows.Visibility newVerticalScrollBarVisibility = verticalScrollBarWillBeShown ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            if (verticalScrollbar.Visibility != newVerticalScrollBarVisibility) {
                verticalScrollbar.Visibility = newVerticalScrollBarVisibility;
            }

            double newVerticalScrollBarViewPortSize = availableContentArea.Height;
            if (verticalScrollbar.ViewportSize != newVerticalScrollBarViewPortSize) {
                verticalScrollbar.ViewportSize = newVerticalScrollBarViewPortSize;
            }

            double newVerticalScrollBarHeight = availableContentArea.Height;
            if (verticalScrollbar.Height != newVerticalScrollBarViewPortSize) {
                verticalScrollbar.Height = newVerticalScrollBarHeight;
            }

            double newHorizontalScrollBarLargeChange = availableContentArea.Width;
            if (verticalScrollbar.LargeChange != newHorizontalScrollBarLargeChange) {
                verticalScrollbar.LargeChange = newHorizontalScrollBarLargeChange;
            }

            double newHorizontalScrollBarMaximum = virtualWidth - availableContentArea.Width;
            if (horizontalScrollbar.Maximum != newHorizontalScrollBarMaximum) {
                horizontalScrollbar.Maximum = newHorizontalScrollBarMaximum;
            }

            System.Windows.Visibility newHorizontalScrollBarVisibility = horizontalScrollBarWillBeShown ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            if (horizontalScrollbar.Visibility != newHorizontalScrollBarVisibility) {
                horizontalScrollbar.Visibility = newHorizontalScrollBarVisibility;
            }

            double newHorizontalScrollBarViewPortSize = availableContentArea.Width;
            if (horizontalScrollbar.ViewportSize != newHorizontalScrollBarViewPortSize) {
                horizontalScrollbar.ViewportSize = newHorizontalScrollBarViewPortSize;
            }

            double newHorizontalScrollBarWidth = availableContentArea.Width;
            if (horizontalScrollbar.Width != newHorizontalScrollBarWidth) {
                horizontalScrollbar.Width = newHorizontalScrollBarWidth;
            }

            if (columnStack.Height != availableContentArea.Height) {
                columnStack.Height = availableContentArea.Height;
            }

        }

        private void RebuildColumnLayout() {

            double firstVisibleXColumn = horizontalScrollbar.Value;
            double lastVisibleXColumn = horizontalScrollbar.Value + availableContentArea.Width;

            double firstXColumnOfFirstVisibleColumn = Columns.Length == 0 ? 0.0 : double.MaxValue;
            double lastXColumnOfLastVisibleColumn = Columns.Length == 0 ? 0.0 : double.MinValue;

            double currentColumnStartX = ContentPadding.Left;

            ArrayList newVisibleColumns = new ArrayList();

            foreach (KNTableColumn column in Columns) {

                if (currentColumnStartX <= lastVisibleXColumn ||
                    currentColumnStartX + column.Width >= firstVisibleXColumn) {

                    if (currentColumnStartX < firstXColumnOfFirstVisibleColumn) {
                        firstXColumnOfFirstVisibleColumn = currentColumnStartX;
                    }

                    if (currentColumnStartX + column.Width > lastXColumnOfLastVisibleColumn) {
                        lastXColumnOfLastVisibleColumn = currentColumnStartX + column.Width;
                    }

                    newVisibleColumns.Add(column);
                    currentColumnStartX += column.Width;
                }
            }

            double stackPanelLeft = firstXColumnOfFirstVisibleColumn - firstVisibleXColumn;
            double stackPanelWidth = lastXColumnOfLastVisibleColumn - firstXColumnOfFirstVisibleColumn;

            if (stackPanelWidth < availableContentArea.Width) {
                stackPanelWidth = availableContentArea.Width;
            }

            if (columnStack.Width != stackPanelWidth) {
                columnStack.Width = stackPanelWidth;
            }

            if (Canvas.GetLeft(columnStack) != stackPanelLeft) {
                Canvas.SetLeft(columnStack, stackPanelLeft);
            }

            double newColumnHeight = columnStack.Height;
            
            if (!CompareArrayLists(newVisibleColumns, visibleColumns)) {

                columnStack.Children.Clear();

                foreach (KNTableColumn column in newVisibleColumns) {
                    if (column.Height != newColumnHeight) {
                        column.Height = newColumnHeight;
                    }
                    columnStack.Children.Add(column);
                }

                visibleColumns = newVisibleColumns;

            } else {
                // Just make sure the heights are correct
                foreach (KNTableColumn column in newVisibleColumns) {
                    if (column.Height != newColumnHeight) {
                        column.Height = newColumnHeight;
                    }
                }
            }
        }

        private void RebuildSelectionLayout() {

            if (selectionLayers.Count == 0 && SelectedRows.Count == 0) {
                return;
            }

            // Figure out visible, selected rows

            int firstVisibleRow, lastVisibleRow;
            double firstVisibleRowVerticalOffset;
            GetVisibleRows(out firstVisibleRow, out lastVisibleRow, out firstVisibleRowVerticalOffset);

            ArrayList visibleSelectedRows = new ArrayList();

            for (int currentRow = firstVisibleRow; currentRow <= lastVisibleRow; currentRow++) {
                if (SelectedRows.Contains(currentRow)) {
                    visibleSelectedRows.Add(currentRow);
                }
            }

            // Compare to our current selection layers
            // If the number differs, add/remove

            while (selectionLayers.Count < visibleSelectedRows.Count) {

                KNTableViewRowSelectionLayer newLayer = new KNTableViewRowSelectionLayer();
                Canvas.SetZIndex(newLayer, kSelectionZIndex);
                Children.Add(newLayer);
                selectionLayers.Add(newLayer);
            }

            while (selectionLayers.Count > visibleSelectedRows.Count) {

                KNTableViewRowSelectionLayer layer = (KNTableViewRowSelectionLayer)selectionLayers[0];
                Children.Remove(layer);
                selectionLayers.Remove(layer);
            }

            // Setup style, position, etc



            double selectionRowLeft = 0.0;
            double selectionRowWidth = availableContentArea.Width ;

            double virtualContentLeft = ContentPadding.Left - horizontalScrollbar.Value;
            double virtualContentWidth = virtualWidth - ContentPadding.Left - ContentPadding.Right;
            // ^ Relative to actual positioning.

            for (int rowPointer = 0; rowPointer < selectionLayers.Count; rowPointer++) {

                int row = (int)visibleSelectedRows[rowPointer];
                KNTableViewRowSelectionLayer layer = (KNTableViewRowSelectionLayer)selectionLayers[rowPointer];

                if (layer.Width != selectionRowWidth) {
                    layer.Width = selectionRowWidth;
                }

                if (layer.Height != RowHeight) {
                    layer.Height = RowHeight;
                }

                if (Canvas.GetLeft(layer) != selectionRowLeft) {
                    Canvas.SetLeft(layer, selectionRowLeft);
                }

                double selectionRowTop = (firstVisibleRowVerticalOffset + ((row - firstVisibleRow) * RowHeight));
                if (Canvas.GetTop(layer) != selectionRowTop) {
                    Canvas.SetTop(layer, selectionRowTop);
                }

                layer.ContentStart = virtualContentLeft;
                layer.ContentLength = virtualContentWidth;
                layer.SelectionStyle = RowSelectionStyle;

            }

        }
        
        private void AutoResizeColumns() {

            if (Columns.Count() > 0) {

                bool shouldResizeColumns = true;
                
                // TODO: Figure out if we should resize the columns?

                if (shouldResizeColumns) {
                    ArrayList remainingColumns = new ArrayList(Columns.ToArray());
                    ArrayList fixedSizeColumns = new ArrayList();
                    double widthLostToFixedColumns = 0;

                    foreach (KNTableColumn column in Columns) {
                        if (column.MinimumWidth == column.MaximumWidth) {
                            fixedSizeColumns.Add(column);
                            widthLostToFixedColumns += column.MaximumWidth;
                            column.Width = column.MaximumWidth;
                            remainingColumns.Remove(column);
                        }
                    }

                    while (remainingColumns.Count > 0) {

                        bool allColumnsFit = true;
                        double suggestedHeaderWidth = (availableContentArea.Width - widthLostToFixedColumns - (ContentPadding.Left  + ContentPadding.Right)) / remainingColumns.Count;
                        ArrayList columnsThatDidntFit = new ArrayList();

                        foreach (KNTableColumn column in remainingColumns) {

                            if (column.MinimumWidth > suggestedHeaderWidth || column.MaximumWidth < suggestedHeaderWidth) {
                                columnsThatDidntFit.Add(column);
                                allColumnsFit = false;
                            }
                        }

                        foreach (KNTableColumn column in columnsThatDidntFit) {

                            if (suggestedHeaderWidth < column.MinimumWidth) {
                                column.Width = column.MinimumWidth;
                            } else {
                                column.Width = column.MaximumWidth;
                            }

                            widthLostToFixedColumns += column.Width;
                            fixedSizeColumns.Add(column);
                            remainingColumns.Remove(column);

                        }

                        if (allColumnsFit) {
                            foreach (KNTableColumn column in remainingColumns) {

                                column.Width = (int)suggestedHeaderWidth;
                                fixedSizeColumns.Add(column);
                                widthLostToFixedColumns += column.Width;

                            }
                            remainingColumns.Clear();
                        }
                    }
                }
            }
        }

        private bool CompareArrayLists(ArrayList anArray, ArrayList anotherArray) {

            if (anArray.Count == anotherArray.Count) {

                int count = anArray.Count;

                for (int i = 0; i < count; i++) {
                    if (!anArray[i].Equals(anotherArray[i])) {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        private void GetVisibleRows(out int firstRowIndex, out int lastRowIndex, out double firstRowAbsolutePixelOffset) {

            double yOffset = verticalScrollbar.Value; // This includes any vertical padding

            int firstRow = (int)Math.Floor((yOffset - ContentPadding.Top) / RowHeight);
            if (firstRow < 0) firstRow = 0; // This happens when there's vertical padding

            double firstRowOffsetInContentArea = (HeaderHeight + (firstRow * RowHeight) + ContentPadding.Top) - yOffset;
            // ^ Can be negative

            int visibleRowCount = (int)Math.Ceiling((availableContentArea.Height - firstRowOffsetInContentArea) / RowHeight);
            int lastRow = firstRow + visibleRowCount;

            firstRowIndex = firstRow;
            lastRowIndex = lastRow;
            firstRowAbsolutePixelOffset = firstRowOffsetInContentArea;
        }

        #endregion

        #region Selection Convenience Methods

        public void SelectRowAtIndex(int rowIndex, bool extendSelection) {

            if (!AllowMultipleSelection) {
                extendSelection = false;
            }

            if (DataSource != null) {

                int lastRow = DataSource.NumberOfItemsInTableView(this) - 1;
                int firstRow = 0;

                if (rowIndex >= firstRow && rowIndex <= lastRow) {

                    ArrayList newSelection = new ArrayList();

                    if (extendSelection) {
                        newSelection = SelectedRows;
                    }

                    if (Delegate != null) {
                        if (Delegate.TableViewShouldSelectRow(this, rowIndex)) {
                            newSelection.Add(rowIndex);
                        }
                    } else {
                        newSelection.Add(rowIndex);
                    }

                    SelectedRows = newSelection;
                    EnsureRowIsVisible(rowIndex);

                }

            }

        }

        public void SelectUpOneRow(bool extendSelection) {

            if (!AllowMultipleSelection) {
                extendSelection = false;
            }

            if (DataSource != null) {

                int lastRow = DataSource.NumberOfItemsInTableView(this) - 1;
                int minRow = lastRow + 1;
                int maxRow = -1;

                foreach (int index in SelectedRows) {

                    if (index < minRow) {
                        minRow = index;
                    }

                    if (index > maxRow) {
                        maxRow = index;
                    }
                }

                if (extendSelection && SelectedRows.Count > 0) {

                    // If the hinged row is before or equal MaxRow,
                    // Our target is maxRow - 1. Otherwise, it's minRow - 1.

                    int newMinRow = -1;
                    int newMaxRow = -1;

                    if (hingedRow < maxRow) {
                        newMaxRow = maxRow - 1;
                        newMinRow = minRow;
                    } else {
                        newMaxRow = maxRow;
                        newMinRow = minRow - 1;
                    }

                    if (newMaxRow > lastRow) {
                        newMaxRow = lastRow;
                    }

                    if (newMinRow < 0) {
                        newMinRow = 0;
                    }

                    ArrayList newSelection = new ArrayList();

                    for (int row = newMinRow; row <= newMaxRow; row++) {
                        if (Delegate != null) {
                            if (Delegate.TableViewShouldSelectRow(this, row)) {
                                newSelection.Add(row);
                            }
                        } else {
                            newSelection.Add(row);
                        }
                    }

                    SelectedRows = newSelection;
                    EnsureRowIsVisible(newMinRow);

                } else {

                    // Our target is minRow - 1

                    int targetRow = minRow - 1;
                    if (targetRow < 0) {
                        targetRow = 0;
                    }

                    while (targetRow > -1) {

                        if (Delegate != null) {
                            if (Delegate.TableViewShouldSelectRow(this, targetRow)) {
                                ArrayList newSelection = new ArrayList();
                                newSelection.Add(targetRow);
                                hingedRow = targetRow;
                                SelectedRows = newSelection;
                                EnsureRowIsVisible(targetRow);
                                break;
                            }
                        } else {
                            ArrayList newSelection = new ArrayList();
                            newSelection.Add(targetRow);
                            hingedRow = targetRow;
                            SelectedRows = newSelection;
                            EnsureRowIsVisible(targetRow);
                            break;
                        }

                        targetRow--;
                    }
                }
            }
        }

        public void SelectDownOneRow(bool extendSelection) {

            if (!AllowMultipleSelection) {
                extendSelection = false;
            }

            if (DataSource != null) {

                int lastRow = DataSource.NumberOfItemsInTableView(this) - 1;
                int minRow = lastRow;
                int maxRow = -1;

                foreach (int index in SelectedRows) {

                    if (index < minRow) {
                        minRow = index;
                    }

                    if (index > maxRow) {
                        maxRow = index;
                    }
                }

                if (extendSelection && SelectedRows.Count > 0) {

                    // If the hinged row is before or equal MaxRow,
                    // Our target is maxRow + 1. Otherwise, it's minRow + 1.

                    int newMinRow = -1;
                    int newMaxRow = -1;

                    if (hingedRow > minRow) {
                        newMaxRow = maxRow;
                        newMinRow = minRow + 1;
                    } else {
                        newMaxRow = maxRow + 1;
                        newMinRow = minRow;
                    }

                    if (newMaxRow > lastRow) {
                        newMaxRow = lastRow;
                    }

                    if (newMinRow < 0) {
                        newMinRow = 0;
                    }

                    ArrayList newSelection = new ArrayList();

                    for (int row = newMinRow; row <= newMaxRow; row++) {
                        if (Delegate != null) {
                            if (Delegate.TableViewShouldSelectRow(this, row)) {
                                newSelection.Add(row);
                            }
                        } else {
                            newSelection.Add(row);
                        }
                    }

                    SelectedRows = newSelection;
                    EnsureRowIsVisible(newMinRow);

                } else {

                    // Our target is maxRow + 1

                    int targetRow = maxRow + 1;
                    if (targetRow > lastRow) {
                        targetRow = lastRow;
                    }

                    while (targetRow <= lastRow) {

                        if (Delegate != null) {
                            if (Delegate.TableViewShouldSelectRow(this, targetRow)) {
                                ArrayList newSelection = new ArrayList();
                                newSelection.Add(targetRow);
                                hingedRow = targetRow;
                                SelectedRows = newSelection;
                                EnsureRowIsVisible(targetRow);
                                break;
                            }
                        } else {
                            ArrayList newSelection = new ArrayList();
                            newSelection.Add(targetRow);
                            hingedRow = targetRow;
                            SelectedRows = newSelection;
                            EnsureRowIsVisible(targetRow);
                            break;
                        }

                        targetRow++;
                    }
                }
            }
        }

        public void EnsureRowIsVisible(int row) {

            double yOffset = row * RowHeight;

            if (verticalScrollbar.Value > yOffset) {
                verticalScrollbar.Value = yOffset;
            }

            if ((verticalScrollbar.Value + verticalScrollbar.LargeChange) < (yOffset + RowHeight)) {
                verticalScrollbar.Value = yOffset + RowHeight - verticalScrollbar.LargeChange;
            }

        }

        #endregion

        #region Mouse

        Point lastMouseDownPoint;
        int hingedRow = -1; // The row a shift-select "hinges" round. Usually the last row selected without the shift key.
        int selectedRowIfNoDrag = -1;

        protected override void OnMouseDown(MouseButtonEventArgs e) {
            
            // If we're on a row, select it. 

            e.Handled = true;
            this.Focus();
            dragDecision = MouseDragDecision.NoDecisionMade;

            Point mouseLocationInControl = e.GetPosition(this);
            lastMouseDownPoint = mouseLocationInControl;

            if (availableContentArea.Contains(mouseLocationInControl)) {
                // We're over a row. Apply some selection rules
                int row = RowAtAbsoluteOffset(mouseLocationInControl.Y);

                if (DataSource != null) {

                    if (row < DataSource.NumberOfItemsInTableView(this) && row > -1) {

                        if (Keyboard.Modifiers == ModifierKeys.Control && SelectedRows.Contains(row)) {

                            ArrayList newSelection = new ArrayList(SelectedRows);
                            newSelection.Remove(row);
                            SelectedRows = newSelection;

                        } else if (SelectedRows.Contains(row)) {

                            // In this case, the user either wants to select the single row 
                            // OR drag them. Be smart later. 

                            selectedRowIfNoDrag = row;

                        } else {

                            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 && AllowMultipleSelection) {

                                ArrayList newSelection = new ArrayList(SelectedRows);

                                if (Delegate != null) {
                                    if (Delegate.TableViewShouldSelectRow(this, row)) {
                                        newSelection.Add(row);
                                        hingedRow = row;
                                    }
                                } else {
                                    newSelection.Add(row);
                                    hingedRow = row;
                                }

                                SelectedRows = newSelection;

                            } else if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0 && AllowMultipleSelection && SelectedRows.Count > 0 && hingedRow > -1) {

                                // Shift logic

                                // Start at the hinged row, then go select the current row.

                                int maxRow, minRow;

                                if (row > hingedRow) {
                                    minRow = hingedRow;
                                    maxRow = row;
                                } else {
                                    minRow = row;
                                    maxRow = hingedRow;
                                }

                                ArrayList newSelection = new ArrayList();
                                                     
                                for (int currentRow = minRow; currentRow <= maxRow; currentRow++) {
                                    if (Delegate != null) {
                                        if (Delegate.TableViewShouldSelectRow(this, currentRow)) {
                                            newSelection.Add(currentRow);
                                        }
                                    } else {
                                        newSelection.Add(currentRow);
                                    }
                                }

                                SelectedRows = newSelection;

                            } else {
                                // Replace selection 
                                if (Delegate != null) {
                                    if (Delegate.TableViewShouldSelectRow(this, row)) {

                                        ArrayList newSelection = new ArrayList();
                                        newSelection.Add(row);
                                        SelectedRows = newSelection;
                                        hingedRow = row;
                                    }
                                } else {
                                    ArrayList newSelection = new ArrayList();
                                    newSelection.Add(row);
                                    SelectedRows = newSelection;
                                    hingedRow = row;
                                }
                            }
                        }

                    } else {
                        // Clear selection
                        if (Delegate != null) {
                            if (Delegate.TableViewShouldSelectRow(this, row)) {
                                SelectedRows = new ArrayList();
                            }
                        } else {
                            SelectedRows = new ArrayList();
                        }
                    }
                }
            }

            Rect absoluteCellFrame;
            KNCell cell = CellAtAbsolutePoint(mouseLocationInControl, out absoluteCellFrame);

            if (e.RightButton == MouseButtonState.Pressed) {
                if (Delegate != null) {
                    if (Delegate.TableViewDelegateShouldShowContextualMenuWithObjectsAtIndexes(this, SelectedRows)) {
                        selectedRowIfNoDrag = -1;
                    }
                }
            }


            this.CaptureMouse();

        }

        protected override void OnMouseMove(MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released) {
                // Just movin' around
                MouseMoved(e);
                e.Handled = true;
            } else if (e.LeftButton == MouseButtonState.Pressed) {
                MouseDragged(e);
                e.Handled = true;
            }
        }

        private enum MouseDragDecision {
            NoDecisionMade,
            SelectionDecisionMade,
            DragDecisionMade
        }

        private MouseDragDecision dragDecision = MouseDragDecision.NoDecisionMade;

        private void MouseDragged(MouseEventArgs e) {

            // If the mouse moves over x pixels from the start point
            // and a cell isn't swallowing events, start a drag!
            Point mouseLocationInControl = e.GetPosition(this);


            // We should start a drag. If The delegate allows it, do a proper OS drag. 
            // If not, select some rows.

            double verticalMotion = mouseLocationInControl.Y - lastMouseDownPoint.Y;
            double horizontalMotion = mouseLocationInControl.X - lastMouseDownPoint.X;

            if (verticalMotion < 0.0) {
                verticalMotion = 0.0 - verticalMotion;
            }

            if (horizontalMotion < 0.0) {
                horizontalMotion = 0.0 - horizontalMotion;
            }

            if (availableContentArea.Contains(mouseLocationInControl) &&
                (verticalMotion > SystemParameters.MinimumVerticalDragDistance ||
                horizontalMotion > SystemParameters.MinimumHorizontalDragDistance)) {

                if (dragDecision == MouseDragDecision.NoDecisionMade) {

                    if (AllowMultipleSelection) {
                        dragDecision = MouseDragDecision.SelectionDecisionMade;
                    }

                    if (Delegate != null) {

                        // Before asking the delegate, figure out if the drag is up/down
                        // before asking. If up/down, select anyway.

                        if ((horizontalMotion > verticalMotion) || !AllowMultipleSelection) {
                            if (Delegate.TableViewDelegateShouldBeginDragOperationWithObjectsAtIndexes(this, SelectedRows)) {
                                dragDecision = MouseDragDecision.DragDecisionMade;
                                selectedRowIfNoDrag = -1;
                            }
                        }
                    }
                }

                if (dragDecision == MouseDragDecision.SelectionDecisionMade) {

                    int maxAllowableRow = 0;
                    if (DataSource != null) {
                        maxAllowableRow = DataSource.NumberOfItemsInTableView(this);
                    }

                    int row = RowAtAbsoluteOffset(mouseLocationInControl.Y);
                    if (selectedRowIfNoDrag > -1 && selectedRowIfNoDrag <= maxAllowableRow) {
                        hingedRow = selectedRowIfNoDrag;
                        selectedRowIfNoDrag = -1;
                    }

                    int minRow = -1;
                    int maxRow = -1;

                    if (hingedRow < row) {
                        minRow = hingedRow;
                        maxRow = row;
                    } else {
                        minRow = row;
                        maxRow = hingedRow;
                    }

                    if (minRow < 0) {
                        minRow = 0;
                    }

                    if (maxRow > maxAllowableRow) {
                        maxRow = maxAllowableRow;
                    }

                    ArrayList newSelection = new ArrayList();

                    for (int thisRow = minRow; thisRow <= maxRow; thisRow++) {

                        if (Delegate != null) {
                            if (Delegate.TableViewShouldSelectRow(this, thisRow)) {
                                newSelection.Add(thisRow);
                            }
                        } else {
                            newSelection.Add(thisRow);
                        }
                    }

                    SelectedRows = newSelection;
                }

            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e) {

            // Action and reset selectedRowIfNoDrag
            // Reset mouseEventsCell
            e.Handled = true;
            Point mouseLocationInControl = e.GetPosition(this);
            dragDecision = MouseDragDecision.NoDecisionMade;
            this.ReleaseMouseCapture();

            if (selectedRowIfNoDrag > -1) {
                SelectedRows.Clear();
                SelectedRows.Add(selectedRowIfNoDrag);
                selectedRowIfNoDrag = -1;
            }

            // This will update state based on current cursor location
            MouseMoved(e);

        }

        private void MouseMoved(MouseEventArgs e) {
            // We don't care any more. Hooray!!!
        }

        protected override void OnMouseLeave(MouseEventArgs e) {
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e) {


            int newScrollValue = ((int)verticalScrollbar.Value) - ((e.Delta * System.Windows.Forms.SystemInformation.MouseWheelScrollLines / 120) * (int)RowHeight);

            if (newScrollValue < verticalScrollbar.Minimum) {
                verticalScrollbar.Value = verticalScrollbar.Minimum;
            } else if (newScrollValue > verticalScrollbar.Maximum) {
                verticalScrollbar.Value = verticalScrollbar.Maximum;
            } else {
                verticalScrollbar.Value = newScrollValue;
            }

            e.Handled = true;

        }

        #endregion

        #region Region <-> Point Conversion

        private KNCell CellAtAbsolutePoint(Point point) {
            Rect frame;
            return CellAtAbsolutePoint(point, out frame);
        }

        private KNCell CellAtAbsolutePoint(Point point, out Rect absoluteFrame) {

            if (availableContentArea.Contains(point)) {

                // Find the cell!!

                if (DataSource != null) {

                    int columnX;
                    double rowY;

                    KNTableColumn column = ColumnAtAbsoluteOffset(point.X, out columnX);
                    int row = RowAtAbsoluteOffset(point.Y, out rowY);

                    if (row < DataSource.NumberOfItemsInTableView(this) && row > -1) {

                        if (column != null) {

                            Rect cellRect = new Rect(columnX, rowY, column.Width, RowHeight);
                            absoluteFrame = cellRect;
                            return column.CellForRow(row);
                        }
                    }

                }

            }

            absoluteFrame = Rect.Empty;
            return null;

        }

        private KNTableColumn ColumnAtAbsoluteOffset(double x) {
            int offset;
            return ColumnAtAbsoluteOffset(x, out offset);
        }

        private KNTableColumn ColumnAtAbsoluteOffset(double x, out int absoluteXOffset) {

            double colStartX = ContentPadding.Left;
            double xOffset = horizontalScrollbar.Value;
            double yOffset = verticalScrollbar.Value;

            foreach (KNTableColumn column in Columns) {

                double colStart = colStartX - xOffset;
                double colEnd = colStart + column.Width;

                if (x >= colStart && x <= colEnd) {
                    absoluteXOffset = (int)colStart;
                    return column;
                }
                colStartX += column.Width;
            }
            absoluteXOffset = 0;
            return null;
        }

        private int RowAtAbsoluteOffset(double y) {
            double offset;
            return RowAtAbsoluteOffset(y, out offset);
        }

        private int RowAtAbsoluteOffset(double y, out double absoluteYOffset) {

            if (y < availableContentArea.Top || y > availableContentArea.Bottom) {
                absoluteYOffset = -RowHeight;
                return -1;
            }

            double yOffset = verticalScrollbar.Value;
            int row = (int)Math.Floor((yOffset + (y - availableContentArea.Top - ContentPadding.Top - HeaderHeight)) / RowHeight);

            absoluteYOffset = (row * RowHeight) + ContentPadding.Top + availableContentArea.Top - yOffset;
            return row;
        }

        #endregion

        #region Drawing 

        protected override void OnRender(DrawingContext drawingContext) {

            if (availableContentArea.IsEmpty) {
                return;
            }

            // Draw! 
            drawingContext.DrawRectangle(new SolidColorBrush(BackgroundColor), null, availableContentArea);

            int firstRow, lastRow;
            double firstRowOffsetInContentArea;
            GetVisibleRows(out firstRow, out lastRow, out firstRowOffsetInContentArea);

            drawingContext.PushClip(new RectangleGeometry(availableContentArea));

            for (int currentRow = firstRow; currentRow <= lastRow; currentRow++) {

                Rect rowRect = new Rect(0,
                    firstRowOffsetInContentArea + ((currentRow - firstRow) * RowHeight),
                    availableContentArea.Width,
                    RowHeight);

                if (AlternatingRows && currentRow % 2 == 0) {
                    drawingContext.DrawRectangle(new SolidColorBrush(AlternateRowColor), null, rowRect);
                }
                
                if (DrawHorizontalGridLines) {

                    drawingContext.DrawLine(new Pen(new SolidColorBrush(GridColor), 1.0),
                        new Point(0.0, rowRect.Y + rowRect.Height),
                        new Point(rowRect.Width, rowRect.Y + rowRect.Height));
                }

            }

            // Pop the contentArea clip
            drawingContext.Pop();

        }

        #endregion

        #region Properties
      
        public double HeaderHeight {
            get { return headerHeight; }
            set {
                this.WillChangeValueForKey("HeaderHeight");
                headerHeight = value;
                this.DidChangeValueForKey("HeaderHeight");
            }
        }
        public Thickness ContentPadding {
            get { return contentPadding; }
            set {
                this.WillChangeValueForKey("ContentPadding");
                contentPadding = value;
                this.DidChangeValueForKey("ContentPadding");
            }
        }

        public ScrollBarVisibility VerticalScrollBarVisibility {
            get { return verticalScrollbarVisibility; }
            set {
                this.WillChangeValueForKey("VerticalScrollBarVisibility");
                verticalScrollbarVisibility = value;
                this.DidChangeValueForKey("VerticalScrollBarVisibility");
            }
        }

        public ScrollBarVisibility HorizontalScrollBarVisibility {
            get { return horizontalScrollbarVisibility; }
            set {
                this.WillChangeValueForKey("HorizontalScrollBarVisibility");
                horizontalScrollbarVisibility = value;
                this.DidChangeValueForKey("HorizontalScrollBarVisibility");
            }
        }

        public Color BackgroundColor { get; set; }

        public KNTableViewDataSource DataSource { get; set; }

        public KNTableViewDelegate Delegate { get; set; }

        public double RowHeight {
            get { return rowHeight; }
            set {
                this.WillChangeValueForKey("RowHeight");
                rowHeight = value;
                this.DidChangeValueForKey("RowHeight");
            }
        }

        public KNTableColumn[] Columns {
            get { return columns; }
            set {
                this.WillChangeValueForKey("Columns");
                if (columns != null) {
                    foreach (KNTableColumn col in columns) {
                        col.RemoveObserverFromKeyPath(this, "Width");
                        col.Delegate = null;
                    }
                }
                columns = value;
                if (columns != null) {
                    foreach (KNTableColumn col in columns) {
                        col.AddObserverToKeyPathWithOptions(this, "Width", 0, null);
                        col.Delegate = this;
                    }
                }
                this.DidChangeValueForKey("Columns");
            }
        }

        private ArrayList selectedRows;

        public ArrayList SelectedRows {
            get { return selectedRows; }
            set {
                this.WillChangeValueForKey("SelectedRows");
                selectedRows = value;
                this.DidChangeValueForKey("SelectedRows");
            }
        }

        public bool AlternatingRows { get; set; }

        public Color AlternateRowColor { get; set; }

        public SelectionStyle RowSelectionStyle { get; set; }

        public bool DrawHorizontalGridLines { get; set; }

        public Color GridColor { get; set; }

        public bool DrawVerticalGridLines { get; set; }

        public KNCell CornerCell { get; set; }

        public bool AllowMultipleSelection { get; set; }



        #endregion

        private class KNTableViewRowSelectionLayer : Canvas, KNKVOObserver {

            SelectionStyle style;
            double contentStart;
            double contentLength;

            public KNTableViewRowSelectionLayer() {
                style = KNTableView.SelectionStyle.WindowsExplorer;
                this.AddObserverToKeyPathWithOptions(this, "SelectionStyle", 0, null);
                this.AddObserverToKeyPathWithOptions(this, "ContentStart", 0, null);
                this.AddObserverToKeyPathWithOptions(this, "ContentLength", 0, null);
                
            }

            public void ObserveValueForKeyPathOfObject(string keyPath, object obj, Dictionary<string, object> change, object context) {
                if (keyPath.Equals("SelectionStyle") || keyPath.Equals("ContentStart") || keyPath.Equals("ContentLength")) {
                    InvalidateVisual();
                }
            }

            public SelectionStyle SelectionStyle {
                get { return style; }
                set {
                    if (style != value) {
                        this.WillChangeValueForKey("SelectionStyle");
                        style = value;
                        this.DidChangeValueForKey("SelectionStyle");
                    }
                }
            }

            public double ContentStart {
                get { return contentStart; }
                set {
                    if (contentStart != value) {
                        this.WillChangeValueForKey("ContentStart");
                        contentStart = value;
                        this.DidChangeValueForKey("ContentStart");
                    }
                }
            }

            public double ContentLength {
                get { return contentLength; }
                set {
                    if (contentLength != value) {
                        this.WillChangeValueForKey("ContentLength");
                        contentLength = value;
                        this.DidChangeValueForKey("ContentLength");
                    }
                }
            }

            protected override void OnRender(DrawingContext drawingContext) {
                base.OnRender(drawingContext);

                Rect rowRect = new Rect(0, 0, Width, Height);

                bool drawFocused = IsFocused || (Parent != null && ((UIElement)Parent).IsFocused);

                switch (style) {
                    case SelectionStyle.WindowsExplorer:

                        rowRect = new Rect(contentStart, 0, contentLength, Height);
                        rowRect.Inflate(-.5, -.5);

                        Color outerLineColor, innerLineStartColor, innerLineEndColor, gradientStartColor, gradientEndColor;

                        if (drawFocused) {

                            outerLineColor = Color.FromRgb(125, 162, 206);
                            innerLineStartColor = Color.FromRgb(235, 244, 253);
                            innerLineEndColor = Color.FromRgb(219, 234, 253);
                            gradientStartColor = Color.FromRgb(220, 235, 252);
                            gradientEndColor = Color.FromRgb(193, 219, 252);

                        } else {
                            outerLineColor = Color.FromRgb(217, 217, 217);
                            innerLineStartColor = Color.FromRgb(250, 250, 250);
                            innerLineEndColor = Color.FromRgb(240, 240, 240);
                            gradientStartColor = Color.FromRgb(248, 248, 248);
                            gradientEndColor = Color.FromRgb(229, 229, 229);
                        }

                        LinearGradientBrush fill = new LinearGradientBrush(gradientStartColor, gradientEndColor, 90);

                        drawingContext.DrawRoundedRectangle(fill, new Pen(new SolidColorBrush(outerLineColor), 1), rowRect, 2, 2);
                        Rect innerRect = rowRect;
                        innerRect.Inflate(-1, -1);
                        drawingContext.DrawRoundedRectangle(null, new Pen(new LinearGradientBrush(innerLineStartColor, innerLineEndColor, 90), 1), innerRect, 2, 2);
                        break;

                    case SelectionStyle.SourceList:

                        rowRect.Inflate(-.5, -.5);
                        Color startColor, endColor;

                        if (drawFocused) {
                            endColor = Color.FromRgb(15, 94, 217);
                            startColor = Color.FromRgb(77, 153, 235);
                        } else {
                            endColor = Color.FromRgb(107, 107, 107);
                            startColor = Color.FromRgb(152, 152, 152);
                        }

                        LinearGradientBrush gradientBrush = new LinearGradientBrush(startColor, endColor, 90);


                        drawingContext.DrawRectangle(gradientBrush, null, rowRect);
                        drawingContext.DrawLine(new Pen(new SolidColorBrush(startColor), 1), new Point(rowRect.X, rowRect.Y), new Point(rowRect.X + rowRect.Width, rowRect.Y));

                        break;
                    case SelectionStyle.Flat:
                    default:

                        if (drawFocused) {
                            drawingContext.DrawRectangle(SystemColors.HighlightBrush, null, rowRect);
                        } else {
                            drawingContext.DrawRectangle(SystemColors.ControlDarkBrush, null, rowRect);
                        }

                        break;
                }
            }
        }
    }  
}
