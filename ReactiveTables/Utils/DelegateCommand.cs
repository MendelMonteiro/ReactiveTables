using System;
using System.Windows.Input;

namespace ReactiveTables.Utils
{
    public class DelegateCommand : ICommand
    {
        private readonly Action _executeMethod;
        public DelegateCommand(Action executeMethod)
        {
            _executeMethod = executeMethod;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged;
        public void Execute(object parameter)
        {
            _executeMethod.Invoke();
        }
    }
}