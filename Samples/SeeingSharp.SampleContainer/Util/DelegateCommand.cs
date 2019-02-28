﻿#region License information
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
    Copyright (C) 2018 Roland König (RolandK)
    
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
#endregion
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
#pragma warning disable
        public event EventHandler CanExecuteChanged;
#pragma warning restore

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
