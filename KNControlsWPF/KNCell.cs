using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using KNFoundation.KNKVC;
using System.Windows.Media;

namespace KNControls {

    public interface KNCellContainer {
        KNCellContainer Control();
    }

    public enum KNCellState {
        KNMixedState = -1,
        KNOffState = 0,
        KNOnState = 1
    }

    public interface KNCell {
        KNCell Copy();
        void PrepareForRecycling();
        void PrepareForActivation();
    }

    public class KNCellDependencyProperty {

        public static readonly DependencyProperty ParentControlProperty;
        public static readonly DependencyProperty ObjectValueProperty;
        public static readonly DependencyProperty HighlightedProperty;
        public static readonly DependencyProperty StateProperty;
        public static readonly DependencyProperty EnabledProperty;

        static KNCellDependencyProperty() {
            //register attached dependency property
            ParentControlProperty = DependencyProperty.RegisterAttached("ParentControl",
                                                                typeof(KNCellContainer),
                                                                typeof(KNCellDependencyProperty),
                                                                new FrameworkPropertyMetadata(null));

            ObjectValueProperty = DependencyProperty.RegisterAttached("ObjectValue",
                                                                typeof(Object),
                                                                typeof(KNCellDependencyProperty),
                                                                new FrameworkPropertyMetadata(null));

            HighlightedProperty = DependencyProperty.RegisterAttached("Highlighted",
                                                               typeof(bool),
                                                               typeof(KNCellDependencyProperty),
                                                               new FrameworkPropertyMetadata(false));

            StateProperty = DependencyProperty.RegisterAttached("State",
                                                               typeof(KNCellState),
                                                               typeof(KNCellDependencyProperty),
                                                               new FrameworkPropertyMetadata(KNCellState.KNOffState));

            EnabledProperty = DependencyProperty.RegisterAttached("Enabled",
                                                               typeof(bool),
                                                               typeof(KNCellDependencyProperty),
                                                               new FrameworkPropertyMetadata(false));
        }

        public static KNCellContainer GetParentControl(DependencyObject obj) {
            return (KNCellContainer)obj.GetValue(ParentControlProperty);
        }

        public static void SetParentControl(DependencyObject obj, KNCellContainer value) {
            obj.WillChangeValueForKey("ParentControl");
            obj.SetValue(ParentControlProperty, value);
            obj.DidChangeValueForKey("ParentControl");
        }

        // --

        public static Object GetObjectValue(DependencyObject obj) {
            return (Object)obj.GetValue(ObjectValueProperty);
        }

        public static void SetObjectValue(DependencyObject obj, Object value) {
            obj.WillChangeValueForKey("ObjectValue");
            obj.SetValue(ObjectValueProperty, value);
            obj.DidChangeValueForKey("ObjectValue");
        }

        // --

        public static bool GetHighlighted(DependencyObject obj) {
            return (bool)obj.GetValue(HighlightedProperty);
        }

        public static void SetHighlighted(DependencyObject obj, bool value) {
            obj.WillChangeValueForKey("Highlighted");
            obj.SetValue(HighlightedProperty, value);
            obj.DidChangeValueForKey("Highlighted");
        }

        // --

        public static bool GetEnabled(DependencyObject obj) {
            return (bool)obj.GetValue(EnabledProperty);
        }

        public static void SetEnabled(DependencyObject obj, bool value) {
            obj.WillChangeValueForKey("Enabled");
            obj.SetValue(EnabledProperty, value);
            obj.DidChangeValueForKey("Enabled");
        }

        // --

        public static KNCellState GetState(DependencyObject obj) {
            return (KNCellState)obj.GetValue(StateProperty);
        }

        public static void SetState(DependencyObject obj, KNCellState value) {
            obj.WillChangeValueForKey("State");
            obj.SetValue(StateProperty, value);
            obj.DidChangeValueForKey("State");
        }
    }
}