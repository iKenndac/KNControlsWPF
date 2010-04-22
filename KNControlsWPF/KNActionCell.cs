using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using KNFoundation.KNKVC;

namespace KNControls {

    public interface KNActionCellDelegate {
        void CellPerformedAction(KNActionCell cell);
    }

    public class KNActionCell : KNCell {

        private KNActionCellDelegate del;

        public override KNCell Copy() {
            return new KNActionCell();
        }


        public KNActionCellDelegate Delegate {
            get { return del; }
            set {
                this.WillChangeValueForKey("Delegate");
                del = value;
                this.DidChangeValueForKey("Delegate");
            }
        }        

        /// <summary>
        /// Called when the mouse is moved over the cell.
        /// </summary>
        /// <param name="relativePoint">The mouse location relative the the given frame.</param>
        /// <param name="relativeFrame">The relative frame of the cell (i.e., origin is always 0,0).</param>
        /// <returns>A boolean defining whether the cell should be redrawn.</returns>
        public virtual bool MouseDidMoveInCell(Point relativePoint, Rect relativeFrame) {
            return false;
        }

        /// <summary>
        /// Called when the mouse is clicked in the cell. Cell is always redrawn following this call.
        /// </summary>
        /// <param name="relativePoint">The mouse location relative the the given frame.</param>
        /// <param name="relativeFrame">The relative frame of the cell (i.e., origin is always 0,0).</param>
        /// <returns>A boolean defining whether the cell claims ownership of the following mouse drag and up events.</returns>
        public virtual bool MouseDownInCell(Point relativePoint, Rect relativeFrame) {
            return false;
        }

        /// <summary>
        /// Called when the mouse is dragged within the cell. Only called if the prior MouseDownInCell() returned true.
        /// </summary>
        /// <param name="relativePoint">The mouse location relative the the given frame.</param>
        /// <param name="relativeFrame">The relative frame of the cell (i.e., origin is always 0,0).</param>
        /// <returns>A boolean defining whether the cell should be redrawn.</returns>
        public virtual bool MouseDraggedInCell(Point relativePoint, Rect relativeFrame) {
            return false;
        }

        /// <summary>
        /// Called when the mouse button is released within the cell. Only called if the prior MouseDownInCell() returned true.
        /// </summary>
        /// <param name="relativePoint">The mouse location relative the the given frame.</param>
        /// <param name="relativeFrame">The relative frame of the cell (i.e., origin is always 0,0).</param>
        /// <returns>A boolean defining whether the cell should be redrawn.</returns>
        public virtual bool MouseUpInCell(Point relativePoint, Rect relativeFrame) {
            return false;
        }
    }
}
