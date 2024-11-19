using Microsoft.Win32;
using SharpHook;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Mibar
{
    public partial class MainWindow : Window
    {
        private readonly TaskPoolGlobalHook hook; // 全局钩子对象

        public MainWindow()
        {
            InitializeComponent();
            // 在加载窗口时执行一些初始化操作
            Loaded += MainWindow_Loaded;
            // 自行修改颜色主题
            AutoSwitchThemeMode();
            // 初始化全局键盘钩子
            hook = new TaskPoolGlobalHook(); // 使用TaskPoolGlobalHook以异步处理事件
            // 订阅鼠标拖动事件
            hook.MouseDragged += (sender, e) =>
            {

                // 获取鼠标当前位置
                short x = e.Data.X;
                short y = e.Data.Y;

                // 检查鼠标是否位于屏幕顶部 
                if (y <= 100)
                {
                    // 使用 Dispatcher 确保在 UI 线程上更新 Visibility
                    Dispatcher.Invoke(() =>
                    {
                        // 开始淡入动画
                        // BeginStoryboard((Storyboard)FindResource("FadeIn"));
                        Debug.WriteLine("移动到上方了，visible!");

                        // 鼠标拖动文件位于屏幕顶部时显示窗口
                        Visibility = Visibility.Visible;
                        
                        // 启用WPF拖放事件
                        //AllowDrop = true;
                        DragEnter += MainWindow_DragEnter;
                        Drop += MainWindow_Drop;
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        // 开始淡出动画
                        // BeginStoryboard((Storyboard)FindResource("FadeOut"));
                        Debug.WriteLine("离开了，hidden");

                        // 鼠标离开屏幕顶部时隐藏窗口
                        Visibility = Visibility.Collapsed;

                        // 禁用WPF拖放事件
                        //AllowDrop = false;
                        DragEnter -= MainWindow_DragEnter;
                        Drop -= MainWindow_Drop;
                    });
                }
            };

            // 启动钩子
            hook.Run();

            // 默认禁用拖放事件
            //AllowDrop = false;
        }

        // 读取注册表获取是否为深色模式
        private bool IsDarkThemeEnabled()
        {
            try
            {
                // 打开注册表键
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        // 读取 "AppsUseLightTheme" 值
                        object value = key.GetValue("AppsUseLightTheme");
                        if (value != null)
                        {
                            // 如果值为 0，则表示启用了暗色模式
                            return (int)value == 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"检查暗色模式时发生错误: {ex.Message}");
            }

            // 默认返回 false，表示未启用暗色模式
            return false;
        }

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
            Top = 0;

            // 设置初始状态为隐藏
            Visibility = Visibility.Collapsed;
        }

        private void MainWindow_DragEnter(object sender, DragEventArgs e)
        {
            // 检查拖入的数据是否是文件列表
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy; // 允许拖放
            }
            else
            {
                e.Effects = DragDropEffects.None; // 不允许拖放
            }
        }

        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    Debug.Write("已经获取到了文件");
                    // 发送
                    SendXiaoMi(files);
                    Debug.WriteLine("已经调用给小米互传");
                }
            }
        }

        // 使用小米互传发送文件
        private void SendXiaoMi(string[] files)
        {
            // 调用 SendToXiaomiPcManager 方法
            DllMain.SendToXiaomiPcManager(files);
        }

        private void AutoSwitchThemeMode()
        {
            // 检查是否启用了暗色模式
            if (IsDarkThemeEnabled())
            {
                Debug.WriteLine("当前是暗色主题");
                // 进行相应的操作，例如切换到暗色主题
                App.Current.Resources["ForeColor"] = Brushes.Black;
            }
            else
            {
                Debug.WriteLine("当前是亮色主题");
                // 进行相应的操作，例如切换到亮色主题
                App.Current.Resources["ForeColor"] = Brushes.White;
            }
        }

        private void OnFadeOutCompleted(object sender, EventArgs e)
        {
            // 动画完成后将窗口设置为 Hidden
            Visibility = Visibility.Collapsed;
        }
    }
}