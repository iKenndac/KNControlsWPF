using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using KNFoundation.KNKVC;
using System.Windows.Media;

namespace KNControls {
    public class KNCell {

        public enum KNCellState {
            KNMixedState = -1,
            KNOffState = 0,
            KNOnState = 1
        }

        public interface KNCellContainer {
            void UpdateCell(KNCell cell);
            KNCellContainer Control();
        }

        private KNCellContainer parent;
        private Object objectValue;
        private bool highlighted;
        private KNCellState state;
        private bool enabled;

        public virtual void RenderInFrame(DrawingContext context, Rect frame) {
        }

        public virtual KNCell Copy() {
            return new KNCell();
        }

        public KNCellContainer ParentControl {
            get { return parent; }
            set {
                this.WillChangeValueForKey("ParentControl");
                parent = value;
                this.DidChangeValueForKey("ParentControl");
            }
        }

        public virtual Object ObjectValue {
            get { return objectValue; }
            set {

                if (objectValue != value) {
                    this.WillChangeValueForKey("ObjectValue");
                    objectValue = value;
                    this.DidChangeValueForKey("ObjectValue");
                }
            }
        }

        public virtual bool Highlighted {
            get { return highlighted; }
            set {
                this.WillChangeValueForKey("Highlighted");
                highlighted = value;
                this.DidChangeValueForKey("Highlighted");
            }
        }

        public virtual KNCellState State {
            get { return state; }
            set {
                this.WillChangeValueForKey("State");
                state = value;
                this.DidChangeValueForKey("State");
            }
        }

        public virtual bool Enabled {
            get { return enabled; }
            set {
                this.WillChangeValueForKey("Enabled");
                enabled = value;
                this.DidChangeValueForKey("Enabled");
            }
        }

    }
}
