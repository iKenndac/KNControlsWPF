using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using KNFoundation.KNKVC;

namespace KNControls {

    public class KNActionCellDependencyProperty {

        public static readonly DependencyProperty DelegateProperty;
        static KNActionCellDependencyProperty() {
            //register attached dependency property
            var nullMetadata = new FrameworkPropertyMetadata(null);

            DelegateProperty = DependencyProperty.RegisterAttached("Delegate",
                                                                typeof(KNActionCellDelegate),
                                                                typeof(KNActionCellDependencyProperty),
                                                                nullMetadata);
        }

        public static KNActionCellDelegate GetDelegate(DependencyObject obj) {
            return (KNActionCellDelegate)obj.GetValue(DelegateProperty);
        }

        public static void SetDelegate(DependencyObject obj, KNActionCellDelegate value) {
            obj.WillChangeValueForKey("Delegate");
            obj.SetValue(DelegateProperty, value);
            obj.DidChangeValueForKey("Delegate");
        }
    }

    public interface KNActionCellDelegate {
        void CellPerformedAction(KNActionCell cell);
    }

    public interface KNActionCell : KNCell {       

        /// <summary>
        /// Called when the mouse is moved over the cell.
        /// </summary>
        /// <param name="relativePoint">The mouse location relative the the given frame.</param>
        /// <param name="relativeFrame">The relative frame of the cell (i.e., origin is always 0,0).</param>
        /// <returns>A boolean defining whether the cell should be redrawn.</returns>
        //bool MouseDidMoveInCell(Point relativePoint, Rect relativeFrame);

        /// <summary>
        /// Called when the mouse is clicked in the cell. Cell is always redrawn following this call.
        /// </summary>
        /// <param name="relativePoint">The mouse location relative the the given frame.</param>
        /// <param name="relativeFrame">The relative frame of the cell (i.e., origin is always 0,0).</param>
        /// <returns>A boolean defining whether the cell claims ownership of the following mouse drag and up events.</returns>
        //bool MouseDownInCell(Point relativePoint, Rect relativeFrame);

        /// <summary>
        /// Called when the mouse is dragged within the cell. Only called if the prior MouseDownInCell() returned true.
        /// </summary>
        /// <param name="relativePoint">The mouse location relative the the given frame.</param>
        /// <param name="relativeFrame">The relative frame of the cell (i.e., origin is always 0,0).</param>
        /// <returns>A boolean defining whether the cell should be redrawn.</returns>
        //bool MouseDraggedInCell(Point relativePoint, Rect relativeFrame);

        /// <summary>
        /// Called when the mouse button is released within the cell. Only called if the prior MouseDownInCell() returned true.
        /// </summary>
        /// <param name="relativePoint">The mouse location relative the the given frame.</param>
        /// <param name="relativeFrame">The relative frame of the cell (i.e., origin is always 0,0).</param>
        /// <returns>A boolean defining whether the cell should be redrawn.</returns>
        //bool MouseUpInCell(Point relativePoint, Rect relativeFrame);
    }
}
