/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/

using System;
using System.Windows.Input;

namespace SeeingSharp.ModelViewer.Util
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

        public bool CanExecute(object parameter)
        {
            if (_canExecuteAction == null) { return true; }
            return _canExecuteAction();
        }

        public void Execute(object parameter)
        {
            _executeAction();
        }
    }
}
