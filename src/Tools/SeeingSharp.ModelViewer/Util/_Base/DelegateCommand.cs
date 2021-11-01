using System;
using System.Windows.Input;

namespace SeeingSharp.ModelViewer.Util
{
    public class DelegateCommand : ICommand
    {
        private Func<bool>? _canExecuteAction;
        private Action _executeAction;

        public event EventHandler? CanExecuteChanged;

        public DelegateCommand(Action executeAction)
        {
            _executeAction = executeAction;
        }

        public DelegateCommand(Action executeAction, Func<bool> canExecuteAction)
        {
            _executeAction = executeAction;
            _canExecuteAction = canExecuteAction;
        }

        public void RaiseCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(object? parameter)
        {
            if (_canExecuteAction == null) { return true; }
            return _canExecuteAction();
        }

        public void Execute(object? parameter)
        {
            _executeAction();
        }
    }
}
