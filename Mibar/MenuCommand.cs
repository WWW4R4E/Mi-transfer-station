using System;
using System.ComponentModel;
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

        // private bool _adjustHeightChecked;
        // public bool AdjustHeightChecked 
        // { 
        //     get => Properties.Settings.Default.lower; 
        //     set 
        //     { 
        //         Properties.Settings.Default.lower = value;
        //         Properties.Settings.Default.Save();
        //         OnPropertyChanged();
        //     } 
        // }

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
            new DelegateCommand(() =>
            {
                Application.Current.Shutdown();
            });

        // public ICommand AdjustHeightCommand => 
        //     new DelegateCommand(() => 
        //     {
        //         AdjustHeightChecked = !AdjustHeightChecked;
        //     });

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
    }
}