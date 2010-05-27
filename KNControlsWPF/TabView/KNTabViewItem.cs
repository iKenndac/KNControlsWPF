using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using KNFoundation.KNKVC;
using KNFoundation;

namespace KNControls {
    public class KNTabViewItem {

        string title;
        Color tintColor;
        BitmapImage icon;
        KNViewController viewController;

        public string Title {
            get { return title; }
            set {
                this.WillChangeValueForKey("Title");
                title = value;
                this.DidChangeValueForKey("Title");
            }
        }

        public Color TintColor {
            get { return tintColor; }
            set {
                this.WillChangeValueForKey("TintColor");
                tintColor = value;
                this.DidChangeValueForKey("TintColor");
            }
        }

        public BitmapImage Icon {
            get { return icon; }
            set {
                this.WillChangeValueForKey("Icon");
                icon = value;
                this.DidChangeValueForKey("Icon");
            }
        }

        public KNViewController ViewController {
            get { return viewController; }
            set {
                this.WillChangeValueForKey("ViewController");
                viewController = value;
                this.DidChangeValueForKey("ViewController");
            }
        }
    }
}
