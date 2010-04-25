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
    public class KNTableView : Canvas, KNCell.KNCellContainer, KNTableColumn.KNTableColumnDelegate, KNKVOObserver {
        static KNTableView() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(KNTableView), new FrameworkPropertyMetadata(typeof(KNTableView)));
        }

        public interface KNTableViewDataSource {
            int NumberOfItemsInTableView(KNTableView table);
            object ObjectForRow(KNTableView table, KNTableColumn column, int rowIndex);
            void CellPerformedAction(KNTableView view, KNTableColumn column, KNActionCell cell, int rowIndex);
        }

        public interface KNTableViewDelegate {
            bool TableViewShouldSelectRow(KNTableView table, int rowIndex);
            KNTableColumn.SortDirection TableViewWillSortByColumnWithSuggestedSortOrder(KNTableView table, KNTableColumn column, KNTableColumn.SortDirection suggestedNewSortOrder);

        }

        public enum SelectionStyle {
            Flat = 0,
            SourceList = 1,
            WindowsExplorer = 2
        }

        public void ObserveValueForKeyPathOfObject(string keyPath, object obj, Dictionary<string, object> change, object context) {
            if (keyPath.Equals("Width")) {
                RecalculateGeometry();
            }
        }

        public void UpdateCell(KNCell cell) {
            throw new NotImplementedException();
        }

        public KNCell.KNCellContainer Control() {
            return this;
        }

        public void ActionCellPerformedAction(KNActionCell cell, KNTableColumn column) {

            if (DataSource != null) {
                DataSource.CellPerformedAction(this, column, cell, column.RowForCell(cell));
                ReloadData();
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

        //IVARS

        ScrollBar verticalScrollbar;
        ScrollBar horizontalScrollbar;

        public KNTableView() {

            this.Focusable = true;
            this.Focus();

            AllowMultipleSelection = true;

            verticalScrollbar = new ScrollBar();
            horizontalScrollbar = new ScrollBar();

            verticalScrollbar.ValueChanged += ScrollbarDidScroll;
            horizontalScrollbar.ValueChanged += ScrollbarDidScroll;

            verticalScrollbar.Width = 16.0;
            horizontalScrollbar.Height = 16.0;
            horizontalScrollbar.Orientation = Orientation.Horizontal;

            Children.Add(verticalScrollbar);
            Children.Add(horizontalScrollbar);

            Canvas.SetBottom(horizontalScrollbar, 0.0);
            Canvas.SetRight(verticalScrollbar, 0.0);

            BackgroundColor = Colors.White;
            SelectedRows = new ArrayList();
            Columns = new KNTableColumn[] {};
            RowHeight = 22.0;
            AlternateRowColor = Color.FromRgb(237, 243, 254);
            //AlternatingRows = true;

            HeaderHeight = 24.0;
            RowSelectionStyle = SelectionStyle.WindowsExplorer;

            horizontalScrollbar.SmallChange = RowHeight;

            //Canvas.SetTop(verticalScrollbar, HeaderHeight + 1);

       
        }

        private double horizontalPadding = 10.0;

        private int actualRowCount = 0;
        private double virtualHeight = 0.0;
        private double virtualWidth = 0.0;
        Rect bounds = Rect.Empty;
        Rect headersArea = Rect.Empty;
        Rect contentArea = Rect.Empty;

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
        
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);

            if (sizeInfo.HeightChanged) {
                verticalScrollbar.Height = sizeInfo.NewSize.Height - horizontalScrollbar.ActualHeight;
            }

            if (sizeInfo.WidthChanged) {
                horizontalScrollbar.Width = sizeInfo.NewSize.Width - verticalScrollbar.ActualWidth;
            }

            RecalculateGeometry();
            InvalidateVisual();
        }

        private void AutoResizeColumns() {

            if (Columns.Count() > 0) {

                bool shouldResizeColumns = true;
                
                // TODO: Figure out if we should resize the columns?

                if (shouldResizeColumns) {
                    ArrayList remainingColumns = new ArrayList(Columns.ToArray());
                    ArrayList fixedSizeColumns = new ArrayList();
                    long widthLostToFixedColumns = 0;

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
                        double suggestedHeaderWidth = (contentArea.Width - widthLostToFixedColumns - (horizontalPadding * 2)) / remainingColumns.Count;
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

        public void ReloadData() {

            if (DataSource != null) {

                actualRowCount = DataSource.NumberOfItemsInTableView(this);
                virtualHeight = actualRowCount * RowHeight;

            }

            RecalculateGeometry();
            InvalidateVisual();
        }

        private void ScrollbarDidScroll(object sender, EventArgs e) {
            InvalidateVisual();
        }

        private void RecalculateGeometry() {

            bounds = new Rect(0, 0, ActualWidth, ActualHeight);
            headersArea = new Rect(0, 0, ActualWidth, HeaderHeight);
            contentArea = new Rect(0,
                HeaderHeight + 1, 
                ActualWidth - verticalScrollbar.ActualWidth, 
                ActualHeight - horizontalScrollbar.ActualHeight - HeaderHeight - 1.0);

            virtualWidth = horizontalPadding * 2;
            foreach (KNTableColumn column in Columns) {
                virtualWidth += column.Width;
            }

            verticalScrollbar.LargeChange = contentArea.Height; 
            verticalScrollbar.Maximum = virtualHeight - contentArea.Height;
            horizontalScrollbar.LargeChange = contentArea.Width;
            horizontalScrollbar.Maximum = virtualWidth - contentArea.Width;

            verticalScrollbar.ViewportSize = contentArea.Height;
            horizontalScrollbar.ViewportSize = contentArea.Width;

        }

        KNActionCell mouseEventsCell;
        Rect mouseEventsCellAbsoluteFrame;
        bool mouseEventsCellSwallowedEvents;
        Point lastMouseDownPoint;
        int hingedRow = -1; // The row a shift-select "hinges" round. Usually the last row selected without the shift key.
        int selectedRowIfNoDrag = -1;

        protected override void OnMouseDown(MouseButtonEventArgs e) {
            
            // If we're on a row, select it. 
            // If we're *also* on an action cell, see if it wants 
            // to swallow the mouse events. 

            // Haha, swallow. *High five*

            Point mouseLocationInControl = e.GetPosition(this);
            lastMouseDownPoint = mouseLocationInControl;

            if (contentArea.Contains(mouseLocationInControl)) {
                // We're over a row. Apply some selection rules
                int row = RowAtAbsoluteOffset(mouseLocationInControl.Y);

                if (DataSource != null) {

                    if (row < DataSource.NumberOfItemsInTableView(this) && row > -1) {

                        if (Keyboard.Modifiers == ModifierKeys.Control && SelectedRows.Contains(row)) {
                            SelectedRows.Remove(row);
                        } else if (SelectedRows.Contains(row)) {

                            // In this case, the user either wants to select the single row 
                            // OR drag them. Be smart. 

                            selectedRowIfNoDrag = row;

                        } else {

                            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 && AllowMultipleSelection) {

                                if (Delegate != null) {
                                    if (Delegate.TableViewShouldSelectRow(this, row)) {
                                        SelectedRows.Add(row);
                                        hingedRow = row;
                                    }
                                } else {
                                    SelectedRows.Add(row);
                                    hingedRow = row;
                                }

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

                                SelectedRows.Clear();

                                for (int currentRow = minRow; currentRow <= maxRow; currentRow++) {
                                    if (Delegate != null) {
                                        if (Delegate.TableViewShouldSelectRow(this, currentRow)) {
                                            SelectedRows.Add(currentRow);
                                        }
                                    } else {
                                        SelectedRows.Add(currentRow);
                                    }
                                }

                            } else {
                                // Replace selection 
                                if (Delegate != null) {
                                    if (Delegate.TableViewShouldSelectRow(this, row)) {
                                        SelectedRows.Clear();
                                        SelectedRows.Add(row);
                                        hingedRow = row;
                                    }
                                } else {
                                    SelectedRows.Clear();
                                    SelectedRows.Add(row);
                                    hingedRow = row;
                                }
                            }
                        }

                    } else {
                        // Clear selection
                        if (Delegate != null) {
                            if (Delegate.TableViewShouldSelectRow(this, row)) {
                                SelectedRows.Clear();
                            }
                        } else {
                            SelectedRows.Clear();
                        }
                    }
                }

                // The selection changed!
                //TODO: Fire events
                //TODO: Make stuff KVO compliant

            }

            Rect absoluteCellFrame;
            KNCell cell = CellAtAbsolutePoint(mouseLocationInControl, out absoluteCellFrame);

            if (cell != mouseEventsCell) {
                // User clicked a cell that the mouse hasn't moved over yet. Is this possible? Do we care?
                //throw new Exception("Inconsistent state");
            }
           
            if (cell != null && typeof(KNActionCell).IsAssignableFrom(cell.GetType())) {

                Rect mouseEventsCellRelativeFrame = new Rect(0, 0, mouseEventsCellAbsoluteFrame.Width, mouseEventsCellAbsoluteFrame.Height);
                Point mouseEventsCellRelativePoint = new Point(mouseLocationInControl.X - mouseEventsCellAbsoluteFrame.X,
                    mouseLocationInControl.Y - mouseEventsCellAbsoluteFrame.Y);

                if (mouseEventsCell.MouseDownInCell(mouseEventsCellRelativePoint, mouseEventsCellRelativeFrame)) {

                    mouseEventsCellSwallowedEvents = true;
                }

                
            }
            
            InvalidateVisual();

        }

        protected override void OnMouseMove(MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released) {
                // Just movin' around
                MouseMoved(e);
            } else if (e.LeftButton == MouseButtonState.Pressed) {
                MouseDragged(e);
            }
        }

        private void MouseDragged(MouseEventArgs e) {

            // If the mouse moves over x pixels from the start point
            // and a cell isn't swallowing events, start a drag!

            // If a cell is swallowing events, pass the drag through and reset selectedRowIfNoDrag

            Point mouseLocationInControl = e.GetPosition(this);

            if (mouseEventsCell != null && mouseEventsCellSwallowedEvents) {
                selectedRowIfNoDrag = -1;

                Rect mouseEventsCellRelativeFrame = new Rect(0, 0, mouseEventsCellAbsoluteFrame.Width, mouseEventsCellAbsoluteFrame.Height);
                Point mouseEventsCellRelativePoint = new Point(mouseLocationInControl.X - mouseEventsCellAbsoluteFrame.X,
                    mouseLocationInControl.Y - mouseEventsCellAbsoluteFrame.Y);

                mouseEventsCell.MouseDraggedInCell(mouseEventsCellRelativePoint, mouseEventsCellRelativeFrame);

                InvalidateVisual();

            } else {
                
                // We should start a drag. If The delegate allows it, do a proper OS drag. 
                // If not, select some rows.

                // TODO: This

            }

        }

        protected override void OnMouseUp(MouseButtonEventArgs e) {

            // Action and reset selectedRowIfNoDrag
            // Reset mouseEventsCell

            Point mouseLocationInControl = e.GetPosition(this);

            if (mouseEventsCell != null && mouseEventsCellSwallowedEvents) {

                Rect mouseEventsCellRelativeFrame = new Rect(0, 0, mouseEventsCellAbsoluteFrame.Width, mouseEventsCellAbsoluteFrame.Height);
                Point mouseEventsCellRelativePoint = new Point(mouseLocationInControl.X - mouseEventsCellAbsoluteFrame.X,
                    mouseLocationInControl.Y - mouseEventsCellAbsoluteFrame.Y);

                mouseEventsCell.MouseUpInCell(mouseEventsCellRelativePoint, mouseEventsCellRelativeFrame);

                InvalidateVisual();

                mouseEventsCellSwallowedEvents = false;
            }

            if (selectedRowIfNoDrag > -1) {
                SelectedRows.Clear();
                SelectedRows.Add(selectedRowIfNoDrag);
                selectedRowIfNoDrag = -1;
                InvalidateVisual();
            }

            // This will update state based on current cursor location
            MouseMoved(e);

        }

        private void MouseMoved(MouseEventArgs e) {

            Point mouseLocationInControl = e.GetPosition(this);
            Rect absoluteCellFrame;
            KNCell cell = CellAtAbsolutePoint(mouseLocationInControl, out absoluteCellFrame);

            // If we're on a column cell, check cursors

            if (cell != null &&
                typeof(KNHeaderCell).IsAssignableFrom(cell.GetType()) &&
                absoluteCellFrame.Right - mouseLocationInControl.X <= KNTableColumn.kResizeAreaWidth) {

                KNTableColumn column = ColumnAtAbsoluteOffset(mouseLocationInControl.X);
                if (column != null && column.UserResizable) {
                    Cursor = Cursors.SizeWE;
                } else {
                    Cursor = Cursors.Arrow;
                }
            } else {
                Cursor = Cursors.Arrow;
            }

            // Send a message to any existing cell.

            if (mouseEventsCell != null) {

                Rect mouseEventsCellRelativeFrame = new Rect(0, 0, mouseEventsCellAbsoluteFrame.Width, mouseEventsCellAbsoluteFrame.Height);
                Point mouseEventsCellRelativePoint = new Point(mouseLocationInControl.X - mouseEventsCellAbsoluteFrame.X,
                    mouseLocationInControl.Y - mouseEventsCellAbsoluteFrame.Y);

                if (mouseEventsCell.MouseDidMoveInCell(mouseEventsCellRelativePoint, mouseEventsCellRelativeFrame)) {
                    InvalidateVisual();
                }
            }

            // If we're not in the existing cell any more, find a new one!

            if (!mouseEventsCellAbsoluteFrame.Contains(mouseLocationInControl)) {

                mouseEventsCellAbsoluteFrame = Rect.Empty;
                mouseEventsCell = null;

                if (cell != null && typeof(KNActionCell).IsAssignableFrom(cell.GetType())) {
                    // We only care about action cells.

                    Point relPoint = new Point(mouseLocationInControl.X - absoluteCellFrame.X, mouseLocationInControl.Y - absoluteCellFrame.Y);
                    Rect relFrame = new Rect(0, 0, absoluteCellFrame.Width, absoluteCellFrame.Height);

                    mouseEventsCellAbsoluteFrame = absoluteCellFrame;
                    mouseEventsCell = (KNActionCell)cell;

                    if (mouseEventsCell.MouseDidMoveInCell(relPoint, relFrame)) {
                        InvalidateVisual();
                    }
                }
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e) {

            if (mouseEventsCell != null) {

                Rect relativeFrame = mouseEventsCellAbsoluteFrame;
                relativeFrame.X = 0;
                relativeFrame.Y = 0;

                if (mouseEventsCell.MouseDidMoveInCell(new Point(-1, -1), relativeFrame)) {
                    InvalidateVisual();
                }

                mouseEventsCell = null;
                mouseEventsCellAbsoluteFrame = Rect.Empty;
            }
        }

        private KNCell CellAtAbsolutePoint(Point point) {
            Rect frame;
            return CellAtAbsolutePoint(point, out frame);
        }

        private KNCell CellAtAbsolutePoint(Point point, out Rect absoluteFrame) {

            if (headersArea.Contains(point)) {

                int colStartX = (int)horizontalPadding;
                double xOffset = horizontalScrollbar.Value;
                double yOffset = verticalScrollbar.Value;

                foreach (KNTableColumn column in Columns) {

                    Rect headerRect = new Rect(colStartX - xOffset, headersArea.Y, column.Width, headersArea.Height);
                    if (headerRect.Contains(point)) {
                        absoluteFrame = headerRect;
                        return column.HeaderCell;
                    }

                    colStartX += column.Width;

                }

            } else if (contentArea.Contains(point)) {

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

            int colStartX = (int)horizontalPadding;
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

            if (y < contentArea.Top || y > contentArea.Bottom) {
                absoluteYOffset = -RowHeight;
                return -1;
            }

            double yOffset = verticalScrollbar.Value;
            int row = (int)Math.Floor((yOffset + (y - contentArea.Top)) / RowHeight);

            absoluteYOffset = (row * RowHeight) + contentArea.Top - yOffset;
            return row;
        }

        protected override void OnRender(DrawingContext drawingContext) {

            if (contentArea.IsEmpty) {
                return;
            }

            double xOffset = horizontalScrollbar.Value;
            double yOffset = verticalScrollbar.Value;

            // Draw! 
            drawingContext.DrawRectangle(new SolidColorBrush(BackgroundColor), null, bounds);

            int firstRow = (int)Math.Floor(yOffset / RowHeight);
            double firstRowOffsetInContentArea = (firstRow * RowHeight) - yOffset; // Should be 0 or less
            int visibleRowCount = (int)Math.Ceiling(contentArea.Height / RowHeight);
            int lastRow = firstRow + visibleRowCount;

            drawingContext.PushClip(new RectangleGeometry(contentArea));

            for (int currentRow = firstRow; currentRow <= lastRow; currentRow++) {

                Rect rowRect = new Rect(0,
                    contentArea.Top + firstRowOffsetInContentArea + ((currentRow - firstRow) * RowHeight),
                    contentArea.Width,
                    RowHeight);


                if (SelectedRows.Contains(currentRow)) {
                    // Draw selection background

                    Rect rowContentRect = new Rect(contentArea.X + horizontalPadding - xOffset,
                        rowRect.Y,
                        virtualWidth - (2 * horizontalPadding),
                        RowHeight);

                    DrawRowHighlightInRect(drawingContext, rowContentRect, rowRect);

                } else {
                    if (AlternatingRows && currentRow % 2 == 0) {
                        drawingContext.DrawRectangle(new SolidColorBrush(AlternateRowColor), null, rowRect);
                    }
                }

                // Draw rows

                int columnStartX = (int)horizontalPadding;

                if (currentRow < actualRowCount) {

                    foreach (KNTableColumn column in Columns) {

                        Rect columnRect = new Rect(columnStartX - xOffset, rowRect.Y, column.Width, rowRect.Height);
                        columnStartX += column.Width;

                        if (contentArea.IntersectsWith(columnRect)) {

                            KNCell cell = column.CellForRow(currentRow);

                            if (DataSource != null) {
                                cell.ObjectValue = DataSource.ObjectForRow(this, column, currentRow);
                            }

                            cell.Highlighted = SelectedRows.Contains(currentRow);
                            cell.ParentControl = this;

                            drawingContext.PushClip(new RectangleGeometry(columnRect));
                            cell.RenderInFrame(drawingContext, columnRect);
                                
                            drawingContext.Pop();
                        }
                    }

                }

                if (DrawHorizontalGridLines) {

                    drawingContext.DrawLine(new Pen(new SolidColorBrush(GridColor), 1.0),
                        new Point(0.0, rowRect.Y + rowRect.Height),
                        new Point(rowRect.Width, rowRect.Y + rowRect.Height));
                }

            }

            // Pop the contentArea clip
            drawingContext.Pop();

            // Go through columns again, this time drawig vertical lines and headers

            int colStartX = (int)horizontalPadding;

            foreach (KNTableColumn column in Columns) {

                Rect headerRect = new Rect(colStartX - xOffset, headersArea.Y, column.Width, headersArea.Height);

                if (headerRect.IntersectsWith(headersArea)) {

                    drawingContext.PushClip(new RectangleGeometry(headerRect));
                    column.HeaderCell.RenderInFrame(drawingContext, headerRect);
                    drawingContext.Pop();
                }

                colStartX += column.Width;

                if (DrawVerticalGridLines) {
                    drawingContext.DrawLine(new Pen(new SolidColorBrush(GridColor), 1.0),
                        new Point(colStartX - 1.0, contentArea.Y),
                        new Point(colStartX - 1.0, contentArea.Y + contentArea.Height));
                }

                if ((headersArea.Width - colStartX) > 0) {
                    if (CornerCell != null) {
                        CornerCell.RenderInFrame(drawingContext, new Rect(colStartX, headersArea.Y, headersArea.Width - colStartX + 1, headersArea.Height));
                    }
                }
            }
        }


        private void DrawRowHighlightInRect(DrawingContext drawingContext, Rect rowContentRect, Rect visibleRowRect) {
            
            switch (RowSelectionStyle) {

                case SelectionStyle.WindowsExplorer:

                    Rect rowRect = rowContentRect;
                    rowRect.Inflate(-.5, -.5);

                    Color outerLineColor, innerLineStartColor, innerLineEndColor, gradientStartColor, gradientEndColor;

                    if (IsFocused) {

                        outerLineColor = Color.FromRgb(125, 162, 206);
                        innerLineStartColor = Color.FromRgb(235, 244, 253);
                        innerLineEndColor = Color.FromRgb(219, 234, 253);
                        gradientStartColor = Color.FromRgb(220, 235, 252);
                        gradientEndColor = Color.FromRgb(193, 219, 252);

                    } else {
                        outerLineColor = Color.FromRgb(217,217,217);
                        innerLineStartColor = Color.FromRgb(250,250,250);
                        innerLineEndColor = Color.FromRgb(240,240,240);
                        gradientStartColor = Color.FromRgb(248,248,248);
                        gradientEndColor = Color.FromRgb(229,229,229);
                    }

                    LinearGradientBrush fill = new LinearGradientBrush(gradientStartColor, gradientEndColor, 90);

                    drawingContext.DrawRoundedRectangle(fill, new Pen(new SolidColorBrush(outerLineColor), 1), rowRect, 2, 2);
                    Rect innerRect = rowRect;
                    innerRect.Inflate(-1, -1);
                    drawingContext.DrawRoundedRectangle(null, new Pen(new LinearGradientBrush(innerLineStartColor, innerLineEndColor, 90), 1), innerRect, 2, 2);
                    break;

                case SelectionStyle.SourceList:

                    Color startColor, endColor;

                    if (IsFocused) {
                        startColor = Color.FromRgb(15, 94, 217);
                        endColor = Color.FromRgb(77, 153, 235);
                    } else {
                        startColor = Color.FromRgb(107, 107, 107);
                        endColor = Color.FromRgb(152, 152, 152);
                    }

                    LinearGradientBrush gradientBrush = new LinearGradientBrush(startColor, endColor, 0);


                    drawingContext.DrawRectangle(gradientBrush, null, visibleRowRect);
                    drawingContext.DrawLine(new Pen(new SolidColorBrush(startColor), 1), new Point(visibleRowRect.X, visibleRowRect.Y), new Point(visibleRowRect.X + visibleRowRect.Width, visibleRowRect.Y));

                    break;
                case SelectionStyle.Flat:
                default:

                    if (IsFocused) {
                        drawingContext.DrawRectangle(SystemColors.HighlightBrush, null, visibleRowRect);
                    } else {
                        drawingContext.DrawRectangle(SystemColors.ControlDarkBrush, null, visibleRowRect);
                    }

                    break;
            }

        }

        private KNTableColumn[] columns;

        public double HeaderHeight { get; set; }

        public Color BackgroundColor { get; set; }

        public KNTableViewDataSource DataSource { get; set; }

        public KNTableViewDelegate Delegate { get; set; }

        public double RowHeight { get; set; }

        public KNTableColumn[] Columns {
            get { return columns; }
            set {
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
                        col.Delegate = this ;
                    }
                }
            }
        }

        public ArrayList SelectedRows { get; set; }

        public bool AlternatingRows { get; set; }

        public Color AlternateRowColor { get; set; }

        public SelectionStyle RowSelectionStyle { get; set; }
     
        public bool DrawHorizontalGridLines { get; set; }

        public Color GridColor { get; set; }

        public bool DrawVerticalGridLines { get; set; }

        public KNCell CornerCell{ get; set; }

        public bool AllowMultipleSelection { get; set; }


        
    }
}
