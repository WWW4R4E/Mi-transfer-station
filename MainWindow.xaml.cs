using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using SharpHook;

namespace WpfMibar
{
    public partial class MainWindow : Window
    {
        private bool isFileDropped = false; // 标志变量，指示是否已经拖放了文件
        private TaskPoolGlobalHook hook; // 全局钩子对象

        public MainWindow()
        {
            InitializeComponent();
            // 在加载窗口时执行一些初始化操作
            Loaded += MainWindow_Loaded;

            // 初始化全局键盘钩子
            hook = new TaskPoolGlobalHook(); // 使用TaskPoolGlobalHook以异步处理事件

            Debug.WriteLine("hello");
            // 订阅鼠标拖动事件
            hook.MouseDragged += (sender, e) =>
            {
                Debug.WriteLine("鼠标拖动事件");

                // 获取鼠标当前位置
                var x = e.Data.X;
                var y = e.Data.Y;

                // 检查鼠标是否位于屏幕顶部
                if (y <= 200)
                {
                    // 使用 Dispatcher 确保在 UI 线程上更新 Visibility
                    Dispatcher.Invoke(() =>
                    {
                        // 鼠标拖动文件位于屏幕顶部时显示窗口
                        this.Visibility = Visibility.Visible;
                        Debug.WriteLine("移动到上方了，visible!");

                        // 启用WPF拖放事件
                        this.AllowDrop = true;
                        this.DragEnter += MainWindow_DragEnter;
                        this.Drop += MainWindow_Drop;
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        // 鼠标离开屏幕顶部时隐藏窗口
                        this.Visibility = Visibility.Collapsed;
                        Debug.WriteLine("离开了，hidden");

                        // 禁用WPF拖放事件
                        this.AllowDrop = false;
                        this.DragEnter -= MainWindow_DragEnter;
                        this.Drop -= MainWindow_Drop;
                    });
                }
            };

            // 启动钩子
            hook.Run();

            // 默认禁用拖放事件
            this.AllowDrop = false;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 获取主屏幕的宽度
            double screenWidth = SystemParameters.PrimaryScreenWidth;

            // 设置窗口的宽度为主屏幕宽度的一半
            this.Width = screenWidth / 2;

            // 设置窗口的高度为 100
            this.Height = 100;

            // 设置窗口的位置在屏幕水平中间的顶部
            this.Left = (screenWidth - this.Width) / 2;
            this.Top = 0;
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
                    // 发送
                    SendXiaoMi(files);
                    Debug.WriteLine("已经调用给小米互传");
                }
            }
        }

        // 使用小米互传发送文件
        private void SendXiaoMi(String[] files)
        {
            // 调用 SendToXiaomiPcManager 方法
            DllMain.SendToXiaomiPcManager(files);
        }

    }
}