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
using System.Numerics;

namespace SeeingSharp.Checking
{
    public static partial class Ensure
    {
        [Conditional("DEBUG")]
        public static void EnsureNotEqual(
            this Vector4 vectorValueLeft, Vector4 vectorValueRight, string checkedVariableName, string comparedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (EngineMath.EqualsWithTolerance(vectorValueLeft.X, vectorValueRight.X) &&
                EngineMath.EqualsWithTolerance(vectorValueLeft.Y, vectorValueRight.Y) &&
                EngineMath.EqualsWithTolerance(vectorValueLeft.Z, vectorValueRight.Z) &&
                EngineMath.EqualsWithTolerance(vectorValueLeft.W, vectorValueRight.W))
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Vector {0} within method {1} with value {2} musst not be equal with the vector {3}!",
                    checkedVariableName, callerMethod, vectorValueLeft, comparedVariableName));
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNotEqual(
            this Vector3 vectorValueLeft, Vector3 vectorValueRight, string checkedVariableName, string comparedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (EngineMath.EqualsWithTolerance(vectorValueLeft.X, vectorValueRight.X) &&
                EngineMath.EqualsWithTolerance(vectorValueLeft.Y, vectorValueRight.Y) &&
                EngineMath.EqualsWithTolerance(vectorValueLeft.Z, vectorValueRight.Z))
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Vector {0} within method {1} with value {2} musst not be equal with the vector {3}!",
                    checkedVariableName, callerMethod, vectorValueLeft, comparedVariableName));
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNotEqual(
            this Vector2 vectorValueLeft, Vector2 vectorValueRight, string checkedVariableName, string comparedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (EngineMath.EqualsWithTolerance(vectorValueLeft.X, vectorValueRight.X) &&
                EngineMath.EqualsWithTolerance(vectorValueLeft.Y, vectorValueRight.Y))
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Vector {0} within method {1} with value {2} musst not be equal with the vector {3}!",
                    checkedVariableName, callerMethod, vectorValueLeft, comparedVariableName));
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNormalized(
            this Vector3 vectorValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (!EngineMath.EqualsWithTolerance(vectorValue.Length(), 1f))
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Vector {0} within method {1} must be normalized!",
                    checkedVariableName, callerMethod, vectorValue));
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNormalized(
            this Vector2 vectorValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (!EngineMath.EqualsWithTolerance(vectorValue.Length(), 1f))
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Vector {0} within method {1} must be normalized!",
                    checkedVariableName, callerMethod, vectorValue));
            }
        }
    }
}
