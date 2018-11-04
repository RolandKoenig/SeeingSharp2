#region License information
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
using System.Diagnostics;
using System.Text;

namespace SeeingSharp.SampleContainer
{
    public static class PlatformDependentMethods
    {
        private static Action<string> s_showUrlInBrowser;

        public static void SetOpenUrlInBrowser(Action<string> showUrlInBrowser)
        {
            s_showUrlInBrowser = showUrlInBrowser;
        }

        public static void OpenUrlInBrowser(string url)
        {
            if(s_showUrlInBrowser != null) { s_showUrlInBrowser(url); }
            else
            {
                // Default implementation
                Process.Start(url);
            }
        }
    }
}
