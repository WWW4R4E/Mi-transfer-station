using System;
using System.Windows;
using System.Windows.Input;

namespace Mibar
{
    public class MenuCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter)
        {
            Application.Current.Shutdown();
        }

        public static ICommand ExitApplicationCommand => new MenuCommand();
    }
}
