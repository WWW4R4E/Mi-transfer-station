using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace Mibar
{
    public partial class App : Application
    {
        public static TaskbarIcon TaskbarIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            TaskbarIcon = (TaskbarIcon)FindResource("Taskbar");
            // 初始设置主题
            UpdateTheme();

            // 订阅系统主题变化事件
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }


        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                // 更新主题
                UpdateTheme();
            }
        }

        private void UpdateTheme()
        {
            bool isDarkMode = ThemeHelper.IsDarkModeEnabled();
            Color accentColor = ThemeHelper.GetSystemAccentColor();

            // 根据 isDarkMode 和 accentColor 更新应用程序的主题
            // 例如，可以加载不同的资源字典或更改样式
            if (isDarkMode)
            {
                Debug.WriteLine("当前是暗色主题");
                // 应用暗色主题
                App.Current.Resources["ForeColor"] = Brushes.Black;
            }
            else
            {
                Debug.WriteLine("当前是亮色主题");
                // 应用浅色主题
                App.Current.Resources["ForeColor"] = Brushes.White;
            }

            // 应用强调色
            Application.Current.Resources["SystemAccentColor"] = accentColor;
        }
    }
}
