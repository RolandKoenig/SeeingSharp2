#region License information
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
#endregion

using System.Diagnostics;
using System.Runtime.CompilerServices;
using SharpDX;

namespace SeeingSharp.Checking
{
    #region using
    #endregion

    public static partial class Ensure
    {
        [Conditional("DEBUG")]
        public static void EnsureNotEmpty(
            this RectangleF rectangle, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (rectangle.Width <= 0f ||
                rectangle.Height <= 0f)
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Rectangle {0} within method {1} must not be empty (given rectangle: {2})!",
                    checkedVariableName, callerMethod, rectangle));
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNotEmpty(
            this Rectangle rectangle, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (rectangle.Width <= 0f ||
                rectangle.Height <= 0f)
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Rectangle {0} within method {1} must not be empty (given rectangle: {2})!",
                    checkedVariableName, callerMethod, rectangle));
            }
        }
    }
}
