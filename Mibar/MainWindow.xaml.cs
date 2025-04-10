using EleCho.WpfSuite.Helpers;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace Mibar
{
    public partial class MainWindow : Window
    {
        
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;

        [StructLayout(LayoutKind.Sequential)]
        private struct ACCENTPOLICY
        {
            public uint AccentState;
            public int AccentFlags;
            public uint GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWCOMPOSITIONATTRIBDATA
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        private enum WindowCompositionAttribute
        {
            WCA_ACCENT_POLICY = 19
        }

        private enum AccentState
        {
            ACCENT_ENABLE_BLURBEHIND = 3,
        }

        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WINDOWCOMPOSITIONATTRIBDATA data);

        public HelpWindow HelpWindow;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += (s, e) => Application.Current.Shutdown();
            HelpWindow = new HelpWindow(this);
            HelpWindow.Show();
            
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TOOLWINDOW);
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            Width = (screenWidth / 3);
            Height = 100;
            Left = (screenWidth - Width) / 2;
            Top = -10;
            Visibility = Visibility.Hidden;
            // 启用圆角效果
            WindowOption.SetCorner(this, WindowCorner.Round);
           
            // 启用Acrylic效果
            EnableAcrylicEffect();
        }

        private void EnableAcrylicEffect()
        {
            var accent = new ACCENTPOLICY();
            accent.AccentState = (uint)AccentState.ACCENT_ENABLE_BLURBEHIND;
            accent.GradientColor = 0x99FFFFFF;

            var accentStructSize = Marshal.SizeOf(accent);
            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WINDOWCOMPOSITIONATTRIBDATA();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.Data = accentPtr;
            data.SizeOfData = accentStructSize;

            SetWindowCompositionAttribute(new System.Windows.Interop.WindowInteropHelper(this).Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        internal void ShowWithAnimation()
        {
            Visibility = Visibility.Visible;
            var storyboard = Resources["CombinedEmergenceAnimation"] as Storyboard;
            storyboard?.Begin(this, true);
        }


        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    Debug.WriteLine("已经获取到了文件");
                    SendXiaoMi(files);
                    Debug.WriteLine("已经调用给小米互传");
                    Dispatcher.Invoke(() =>
                    {
                        Debug.WriteLine("尝试隐藏窗口");
                        Visibility = Visibility.Collapsed;
                    });
                }
            }
        }

        // 使用小米互传发送文件
        private void SendXiaoMi(string[] files)
        {
           Misend.DllMain.SendToXiaomiPcManager(files);
        }

        private void MainWindow_DragLeave(object sender, DragEventArgs e)
        {
            if (Resources["CombinedHideAnimation"] is Storyboard hideStoryboard)
            {
                // 动画完成后隐藏窗口
                hideStoryboard.Completed += (s, _) => Visibility = Visibility.Collapsed;
                hideStoryboard.Begin(this, true);
            }
        }
    }
}