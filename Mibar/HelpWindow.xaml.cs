using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace Mibar
{
    public partial class HelpWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        private const int GWL_EXSTYLE = -20;

        private MainWindow _mainWindow;

        public HelpWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            SourceInitialized += OnSourceInitialized;
            _mainWindow = mainWindow;
            Loaded += MainWindow_Loaded;
            Closing += (s, e) => Application.Current.Shutdown();
        }


        private void OnSourceInitialized(object sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            const int WS_EX_TOOLWINDOW = 0x00000080;
            const int WS_EX_LAYERED = 0x80000;
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            if (Properties.Settings.Default.lower)
            {
                Console.WriteLine("设置非工具窗口样式");
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_LAYERED);
            }
            else
            {
                Console.WriteLine("设置工具窗口样式");
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TOOLWINDOW | WS_EX_LAYERED);
            }

            uint colorKey = 0xFF0000;
            byte opacity = 255;
            SetLayeredWindowAttributes(hwnd, colorKey, opacity, 2);
        }


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            Width = screenWidth / 3;
            Left = (screenWidth - Width) / 2;
            this.Visibility = Properties.Settings.Default.show ? Visibility.Visible : Visibility.Hidden;
        }

        private async void HelpWindow_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;

                await Task.Delay(600);

                _mainWindow.ShowWithAnimation();
                _mainWindow.Visibility = Visibility.Visible;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }
    }
}