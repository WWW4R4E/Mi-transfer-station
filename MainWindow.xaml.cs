using Microsoft.Win32;
using SharpHook;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mibar
{
    public partial class MainWindow : Window
    {
        /*定义一些结构体和枚举来与 Windows API 进行交互
        ACCENTPOLICY: 描述了亚克力效果的状态和其他属性。
        AccentState: 表示亚克力效果的状态，例如启用或禁用。
        AccentFlags: 其他标志，通常设为 0。
        GradientColor: 设置亚克力效果的颜色和不透明度，格式为 ARGB。
        AnimationId: 动画标识符，通常设为 0。
        WINDOWCOMPOSITIONATTRIBDATA: 包含要设置的属性及其数据。
        Attribute: 要设置的属性类型，这里设置为 WCA_ACCENT_POLICY。
        Data: 指向包含属性数据的指针。
        SizeOfData: 数据的大小。
        WindowCompositionAttribute: 定义了不同的窗口组合属性。
        WCA_ACCENT_POLICY: 表示亚克力效果策略。
        AccentState: 定义了不同的亚克力效果状态。
        ACCENT_DISABLE: 禁用亚克力效果。
        ACCENT_ENABLE_GRADIENT: 启用渐变效果。
        ACCENT_ENABLE_TRANSPARENTGRADIENT: 启用透明渐变效果。
        ACCENT_ENABLE_BLURBEHIND: 启用模糊效果。
        ACCENT_INVALID_STATE: 无效状态。
        */
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
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_INVALID_STATE = 4
        }

        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WINDOWCOMPOSITIONATTRIBDATA data);

        private readonly TaskPoolGlobalHook hook; // 全局钩子对象

        private bool _hookEnabled = true; // 钩子默认为开启状态

        public MainWindow()
        {
            InitializeComponent();
            // 在加载窗口时执行一些初始化操作
            Loaded += MainWindow_Loaded;
            // 初始化全局键盘钩子
            hook = new TaskPoolGlobalHook(); // 使用TaskPoolGlobalHook以异步处理事件
            // 订阅鼠标拖动事件
            hook.MouseDragged += (sender, e) =>
            {

                // 获取鼠标当前位置
                short x = e.Data.X;
                short y = e.Data.Y;

                // 检查鼠标是否位于屏幕顶部 
                if (y <= 140)
                {
                    // 使用 Dispatcher 确保在 UI 线程上更新 Visibility
                    Dispatcher.Invoke(() =>
                    {
                        Debug.WriteLine("移动到上方了，visible!");

                        // 鼠标拖动文件位于屏幕顶部时显示窗口
                        Visibility = Visibility.Visible;

                        // 启用WPF拖放事件
                        AllowDrop = true;
                        DragEnter += MainWindow_DragEnter;
                        Drop += MainWindow_Drop;
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        Debug.WriteLine("离开了，hidden");

                        // 鼠标离开屏幕顶部时隐藏窗口
                        Visibility = Visibility.Collapsed;

                        // 禁用WPF拖放事件
                        AllowDrop = false;
                        DragEnter -= MainWindow_DragEnter;
                        Drop -= MainWindow_Drop;
                    });
                }
            };

            // 启动钩子
            hook.Run();

            // 默认禁用拖放事件
            AllowDrop = false;
        }

        // 读取注册表获取是否为深色模式
        //private bool IsDarkThemeEnabled()
        //{
        //    try
        //    {
        //        // 打开注册表键
        //        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
        //        {
        //            if (key != null)
        //            {
        //                // 读取 "AppsUseLightTheme" 值
        //                object value = key.GetValue("AppsUseLightTheme");
        //                if (value != null)
        //                {
        //                    // 如果值为 0，则表示启用了暗色模式
        //                    return (int)value == 0;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"检查暗色模式时发生错误: {ex.Message}");
        //    }

        //    // 默认返回 false，表示未启用暗色模式
        //    return false;
        //}

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 获取主屏幕的宽度
            double screenWidth = SystemParameters.PrimaryScreenWidth;

            // 设置窗口的宽度为主屏幕宽度的一半
            Width = screenWidth / 2;

            // 设置窗口的高度为 100
            Height = 100;

            // 设置窗口的位置在屏幕水平中间的顶部
            Left = (screenWidth - Width) / 2;
            Top = 20;

            // 设置窗口亚力克效果
            EnableAcrylicEffect();

            // 设置初始状态为隐藏
            Visibility = Visibility.Collapsed;
        }

        private void EnableAcrylicEffect()
        {
            var accent = new ACCENTPOLICY();
            accent.AccentState = (uint)AccentState.ACCENT_ENABLE_BLURBEHIND;
            accent.GradientColor = 0x99FFFFFF; // ARGB for 60% opacity white

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

        private void MainWindow_DragEnter(object sender, DragEventArgs e)
        {
            // 检查拖入的数据是否是文件列表
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
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
                    // Ensure the window is hidden after sending files
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
            DllMain.SendToXiaomiPcManager(files);
        }

        //private void AutoSwitchThemeMode()
        //{
        //    //检查是否启用了暗色模式
        //    if (IsDarkThemeEnabled())
        //    {
        //        Debug.WriteLine("当前是暗色主题");
        //        App.Current.Resources["ForeColor"] = Brushes.Black;
        //    }
        //    else
        //    {
        //        Debug.WriteLine("当前是亮色主题");
        //        App.Current.Resources["ForeColor"] = Brushes.White;
        //    }
        //}

        // 释放进程
        public void WindowClose()
        {
            hook.Dispose();
            foreach (Window window in Application.Current.Windows)
            {
                window.Close();
            }

            // 关闭应用程序
            Application.Current.Shutdown();
        }
        // 切换钩子状态
        public void ToggleHook()
        {
            if (_hookEnabled)
            {
                hook.Dispose();
                _hookEnabled = false;
                Debug.WriteLine("Hook disabled");
            }
            else
            {
                hook.Run();
                _hookEnabled = true;
                Debug.WriteLine("Hook enabled");
            }
        }
    }
}






