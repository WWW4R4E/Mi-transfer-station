using System;
using System.Runtime.InteropServices;
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

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;

        private MainWindow _mainWindow;

        public HelpWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            SourceInitialized += OnSourceInitialized;
            _mainWindow = mainWindow;
            Loaded += MainWindow_Loaded;
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;

            // 设置层叠窗口样式
            const int WS_EX_LAYERED = 0x80000;
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_LAYERED);

            uint colorKey = 0xFF0000;
            byte opacity = 255;
            SetLayeredWindowAttributes(hwnd, colorKey, opacity, 2);
        }


        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TOOLWINDOW);

            double screenWidth = SystemParameters.PrimaryScreenWidth;
            Width = screenWidth / 3;
            Left = (screenWidth - Width) / 2;
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