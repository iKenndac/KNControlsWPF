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
    public class KNTableView : Canvas, KNCell.KNCellContainer {
        static KNTableView() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(KNTableView), new FrameworkPropertyMetadata(typeof(KNTableView)));
        }

        public interface KNTableViewDataSource {
            int NumberOfItemsInTableView(KNTableView table);
            object ObjectForRow(KNTableView table, KNTableColumn column, int rowIndex);
        }

        public enum SelectionStyle {
            Flat = 0,
            SourceList = 1,
            WindowsExplorer = 2
        }


        public void UpdateCell(KNCell cell) {
            throw new NotImplementedException();
        }

        public KNCell.KNCellContainer Control() {
            return this;
        }

        //IVARS

        ScrollBar verticalScrollbar;
        ScrollBar horizontalScrollbar;

        public KNTableView() {

            this.Focusable = true;
            this.Focus();

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
            SelectedRows = new int[] {};
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

            AutoResizeColumns();

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

        protected override void OnLostFocus(RoutedEventArgs e) {
            base.OnLostFocus(e);
        }

        protected override void OnGotFocus(RoutedEventArgs e) {
            base.OnGotFocus(e);
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e) {
            base.OnGotKeyboardFocus(e);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e) {
            base.OnLostKeyboardFocus(e);
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

        public double HeaderHeight { get; set; }

        public Color BackgroundColor { get; set; }

        public KNTableViewDataSource DataSource { get; set; }

        public double RowHeight { get; set; }

        public IEnumerable<KNTableColumn> Columns { get; set; }

        public int[] SelectedRows { get; set; }

        public bool AlternatingRows { get; set; }

        public Color AlternateRowColor { get; set; }

        public SelectionStyle RowSelectionStyle { get; set; }
     
        public bool DrawHorizontalGridLines { get; set; }

        public Color GridColor { get; set; }

        public bool DrawVerticalGridLines { get; set; }

        public KNCell CornerCell{ get; set; }
    }
}
