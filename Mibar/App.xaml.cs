using Hardcodet.Wpf.TaskbarNotification;
using System.Windows;

namespace Mibar
{
    public partial class App : Application
    {
        public TaskbarIcon TaskbarIcon { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            TaskbarIcon = (TaskbarIcon)FindResource("Taskbar");
        }
    }
}