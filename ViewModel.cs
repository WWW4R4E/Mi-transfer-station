using System.Windows;
using System.Windows.Input;
using SharpHook;
using System;
using Hardcodet.Wpf.TaskbarNotification.Interop;

namespace Mibar
{
    public class ViewModel
    {
        private IDisposable _hook;

        public ViewModel()
        {
        }

        public ICommand ShowWindowCommand =>
            new DelegateCommand
            {
                CommandAction = () =>
                {
                    Application.Current.MainWindow.Visibility = Visibility.Visible;
                }
            };

        /// <summary>
        ///     隐藏窗口
        /// </summary>
        //public ICommand HideWindowCommand =>
        //    new DelegateCommand { CommandAction = () => Application.Current.MainWindow.Hide() };

        /// <summary>
        ///     切换钩子
        /// </summary>
        public ICommand ToggleHookCommand =>
            new DelegateCommand
            {
                CommandAction = () =>
                {
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    if (mainWindow != null)
                    {
                        // 切换钩子状态
                        mainWindow.ToggleHook();
                    }
                }

            };

        /// <summary>
        ///     关闭软件
        /// </summary>
        public ICommand ExitApplicationCommand =>
            new DelegateCommand
            {
                CommandAction = () =>
                {
                    // 获取 MainWindow 实例
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    if (mainWindow != null)
                    {
                        // 调用退出
                        mainWindow.WindowClose();
                    }
                }
            };
    }
}