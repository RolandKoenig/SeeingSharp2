using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SeeingSharp.SampleContainer.Util
{
    public class DelegateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Action m_executeAction;
        private Func<bool> m_canExecuteAction;

        public DelegateCommand(Action executeAction)
        {
            m_executeAction = executeAction;
        }

        public DelegateCommand(Action executeAction, Func<bool> canExecuteAction)
        {
            m_executeAction = executeAction;
            m_canExecuteAction = canExecuteAction;
        }

        public bool CanExecute(object parameter)
        {
            if(m_canExecuteAction == null) { return true; }
            else { return m_canExecuteAction(); }
        }

        public void Execute(object parameter)
        {
            m_executeAction();
        }
    }
}
