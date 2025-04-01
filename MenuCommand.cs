using System.Windows;
using System.Windows.Input;

namespace Mibar
{
    public class MenuCommand
    {
        public static ICommand ExitApplicationCommand =>
            new DelegateCommand { CommandAction = () => Application.Current.Shutdown() };
    }
}