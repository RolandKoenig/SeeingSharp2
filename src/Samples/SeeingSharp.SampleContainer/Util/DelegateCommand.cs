using System;
using System.Windows.Input;

namespace SeeingSharp.SampleContainer.Util
{
    public class DelegateCommand : ICommand
    {
        private Func<bool>? _canExecuteAction;

        private Action _executeAction;

#pragma warning disable
        public event EventHandler? CanExecuteChanged;
#pragma warning restore

        public DelegateCommand(Action executeAction)
        {
            _executeAction = executeAction;
        }

        public DelegateCommand(Action executeAction, Func<bool> canExecuteAction)
        {
            _executeAction = executeAction;
            _canExecuteAction = canExecuteAction;
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
