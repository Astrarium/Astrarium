using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Planetarium
{
    public class Command<T> : ICommand
    {
        private Action<T> action;

        public Command(Action<T> action)
        {
            this.action = action;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            action.Invoke((T)parameter);
        }
    }

    public class Command : ICommand
    {
        private Action action;

        public Command(Action action)
        {
            this.action = action;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            action.Invoke();
        }
    }
}
