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
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
                throw new SeeingSharpCheckException(
                    $"Timespan {checkedVariableName} within method {callerMethod} must be longer or equal zero (given value is {timeSpan}!");
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
                throw new SeeingSharpCheckException(
                    $"Timespan {checkedVariableName} within method {callerMethod} must be longer or equal {compareValue} (given value is {timeSpan}!");
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
                throw new SeeingSharpCheckException(
                    $"Timespan {checkedVariableName} within method {callerMethod} must be shorter or equal {compareValue} (given value is {timeSpan}!");
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
                throw new SeeingSharpCheckException(
                    $"Timespan {checkedVariableName} within method {callerMethod} must be longer than zero (given value is {timeSpan}!");
            }
        }
    }
}
