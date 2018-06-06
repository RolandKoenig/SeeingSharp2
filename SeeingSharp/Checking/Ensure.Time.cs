#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharp.Checking
{
    public static partial class Ensure
    {
        [Conditional("DEBUG")]
        public static void EnsureLongerOrEqualZero(
            this TimeSpan timeSpan, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (timeSpan < TimeSpan.Zero)
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Timespan {0} within method {1} must be longer or equal zero (given value is {2}!",
                    checkedVariableName, callerMethod,
                    timeSpan));
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureLongerOrEqualThan(
            this TimeSpan timeSpan, TimeSpan compareValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (timeSpan < compareValue)
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Timespan {0} within method {1} must be longer or equal {2} (given value is {3}!",
                    checkedVariableName, callerMethod,
                    compareValue, timeSpan));
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureShorterOrEqualThan(
            this TimeSpan timeSpan, TimeSpan compareValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (timeSpan > compareValue)
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Timespan {0} within method {1} must be shorter or equal {2} (given value is {3}!",
                    checkedVariableName, callerMethod,
                    compareValue, timeSpan));
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureLongerThanZero(
            this TimeSpan timeSpan, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (timeSpan <= TimeSpan.Zero)
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Timespan {0} within method {1} must be longer than zero (given value is {2}!",
                    checkedVariableName, callerMethod,
                    timeSpan));
            }
        }
    }
}
