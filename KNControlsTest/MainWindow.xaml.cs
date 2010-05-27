using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using KNControls;
using KNFoundation;
using System.Reflection;

using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace KNControlsTest {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            Loaded += DidLoadYay;

        }


        [StructLayout(LayoutKind.Sequential)]
        struct MARGINS {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [DllImport("dwmapi.dll")]
        static extern int
           DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("dwmapi.dll")]
        extern static int DwmIsCompositionEnabled(ref int en);

        public static void ExtendGlass(Window window, Thickness thikness) {
            try {
                int isGlassEnabled = 0;
                DwmIsCompositionEnabled(ref isGlassEnabled);
                if (Environment.OSVersion.Version.Major > 5 && isGlassEnabled > 0) {
                    // Get the window handle
                    WindowInteropHelper helper = new WindowInteropHelper(window);
                    HwndSource mainWindowSrc = (HwndSource)HwndSource.
                        FromHwnd(helper.Handle);
                    mainWindowSrc.CompositionTarget.BackgroundColor =
                        Colors.Transparent;

                    // Get the dpi of the screen
                    System.Drawing.Graphics desktop =
                       System.Drawing.Graphics.FromHwnd(mainWindowSrc.Handle);
                    float dpiX = desktop.DpiX / 96;
                    float dpiY = desktop.DpiY / 96;

                    // Set Margins
                    MARGINS margins = new MARGINS();
                    margins.cxLeftWidth = (int)(thikness.Left * dpiX);
                    margins.cxRightWidth = (int)(thikness.Right * dpiX);
                    margins.cyBottomHeight = (int)(thikness.Bottom * dpiY);
                    margins.cyTopHeight = (int)(thikness.Top * dpiY);

                    window.Background = Brushes.Transparent;

                    int hr = DwmExtendFrameIntoClientArea(mainWindowSrc.Handle,
                                ref margins);
                } else {
                    window.Background = SystemColors.WindowBrush;
                }
            } catch (DllNotFoundException) {

            }
        }


        private void DidLoadYay(object sender, EventArgs e) {

            ExtendGlass(this, new Thickness(-1.0));

            System.Windows.Forms.Application.EnableVisualStyles();

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = Assembly.GetExecutingAssembly().GetManifestResourceStream("KNControlsTest.SourcePlaylist.png");
            image.EndInit();

            KNViewController tableController = new KNViewController(new TableView());

            KNTabViewItem tableItem = new KNTabViewItem();
            tableItem.ViewController = tableController;
            tableItem.Title = "KNTableView";
            tableItem.TintColor = Colors.White;
            tableItem.Icon = image;

            TabViewTestController tabTest = new TabViewTestController();

            kNTabView1.TabHeight = 30.0;
            kNTabView1.Items = new KNTabViewItem[] { tableItem, tabTest.TabViewItem };
        }

    }
}
