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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using SharpDX;

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
                throw new SeeingSharpCheckException(
                    $"Vector {checkedVariableName} within method {callerMethod} with value {vectorValueLeft} musst not be equal with the vector {comparedVariableName}!");
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
                throw new SeeingSharpCheckException(
                    $"Vector {checkedVariableName} within method {callerMethod} with value {vectorValueLeft} musst not be equal with the vector {comparedVariableName}!");
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
                throw new SeeingSharpCheckException(
                    $"Vector {checkedVariableName} within method {callerMethod} with value {vectorValueLeft} musst not be equal with the vector {comparedVariableName}!");
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
