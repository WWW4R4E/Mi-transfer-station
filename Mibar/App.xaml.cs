using Hardcodet.Wpf.TaskbarNotification;
using System.Windows;
using System.Threading;

namespace Mibar
{
    public partial class App : Application
    {
        private Mutex _mutex;

        public TaskbarIcon TaskbarIcon { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "MibarApp";
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("应用程序已经在运行。");
                Application.Current.Shutdown();
                return;
            }

            base.OnStartup(e);
            TaskbarIcon = (TaskbarIcon)FindResource("Taskbar");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // 释放互斥体
            _mutex?.ReleaseMutex();
            base.OnExit(e);
        }
    }
}
