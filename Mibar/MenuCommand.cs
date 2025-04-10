using System;
using System.ComponentModel;
using System.IO;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Mibar
{
    public class DelegateCommand : ICommand
    {
        private readonly Action _execute;
        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action execute)
        {
            _execute = execute;
        }

        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _execute?.Invoke();
    }

    public class MenuCommand : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _adjustHeightChecked;

        public bool AdjustHeightChecked
        {
            get => Properties.Settings.Default.lower;
            set
            {
                Properties.Settings.Default.lower = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        private bool _toggleWindowEnabledChecked;

        public bool ToggleWindowEnabledChecked
        {
            get => Properties.Settings.Default.show;
            set
            {
                Properties.Settings.Default.show = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public ICommand ExitApplicationCommand =>
            new DelegateCommand(() => { Application.Current.Shutdown(); });

        public ICommand AdjustHeightCommand =>
            new DelegateCommand(() =>
            {
                AdjustHeightChecked = !AdjustHeightChecked; // 切换并保存设置
                RestartApplication(); // 重启应用
            });

        public ICommand ToggleWindowEnabledCommand =>
            new DelegateCommand(() =>
            {
                ToggleWindowEnabledChecked = !ToggleWindowEnabledChecked;
                var mainWindow = App.Current.MainWindow as MainWindow;
                var helpWindow = mainWindow.HelpWindow;
                helpWindow.Visibility = ToggleWindowEnabledChecked ? Visibility.Visible : Visibility.Hidden;
            });

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RestartApplication()
        {
            try
            {
                // 方法一：直接获取当前进程的 EXE 路径（优先）
                string exePath = null;
                try
                {
                    exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                }
                catch
                {
                    // 方法二：备用方案（需手动指定 EXE 名称）
                    var exeName = "Mibar.exe"; // 根据实际项目名称修改
                    exePath = System.IO.Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        exeName
                    );
                }

                if (string.IsNullOrEmpty(exePath) || !System.IO.File.Exists(exePath))
                {
                    throw new FileNotFoundException("无法定位可执行文件路径");
                }

                System.Diagnostics.Process.Start(exePath);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                string errorMessage = ex?.ToString() ?? "未知异常";
                Console.WriteLine($"重启失败: {errorMessage}");
                MessageBox.Show(
                    $"应用重启失败，错误信息：{errorMessage}\n请手动关闭并重新启动程序",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}