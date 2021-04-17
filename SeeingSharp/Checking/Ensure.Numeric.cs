/*
    SeeingSharp and all applications distributed together with it. 
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
        //---------------------------------------------------------------------
        // Method 'EnsureInRange' for all common numeric variables

        [Conditional("DEBUG")]
        public static void EnsureInRange(
            this byte numValue, byte min, byte max, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue < min ||
                numValue > max)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be between {min} and {max} (given value is {numValue}!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureInRange(
            this short numValue, short min, short max, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue < min ||
                numValue > max)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be between {min} and {max} (given value is {numValue}!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureInRange(
            this int numValue, int min, int max, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue < min ||
                numValue > max)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be between {min} and {max} (given value is {numValue}!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureInRange(
            this long numValue, long min, long max, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue < min ||
                numValue > max)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be between {min} and {max} (given value is {numValue}!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureInRange(
            this ushort numValue, ushort min, ushort max, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue < min ||
                numValue > max)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be between {min} and {max} (given value is {numValue}!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureInRange(
            this uint numValue, uint min, uint max, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue < min ||
                numValue > max)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be between {min} and {max} (given value is {numValue}!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureInRange(
            this ulong numValue, ulong min, ulong max, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue < min ||
                numValue > max)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be between {min} and {max} (given value is {numValue}!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureInRange(
            this float numValue, float min, float max, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue < min ||
                numValue > max)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be between {min} and {max} (given value is {numValue}!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureInRange(
            this double numValue, double min, double max, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue < min ||
                numValue > max)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be between {min} and {max} (given value is {numValue}!");
            }
        }

        //---------------------------------------------------------------------
        // Method 'EnsurePowerOfTwo' 

        [Conditional("DEBUG")]
        public static void EnsurePowerOfTwo(
            this int numValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (Math.Abs((double)numValue / 2) > EngineMath.TOLERANCE_DOUBLE_POSITIVE)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be a power of 2 (value: {numValue})!");
            }
        }

        //---------------------------------------------------------------------
        // Method 'EnsureNegativeAndNotZero' for all common numeric variables

        [Conditional("DEBUG")]
        public static void EnsureNegativeAndNotZero(
            this int numValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue >= 0)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be a negative value and not zero (value: {numValue})!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNegativeAndNotZero(
            this short numValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue >= 0)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be a negative value and not zero (value: {numValue})!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNegativeAndNotZero(
            this long numValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue >= 0)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be a negative value and not zero (value: {numValue})!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNegativeAndNotZero(
            this float numValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue >= 0f)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be a negative value and not zero (value: {numValue})!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNegativeAndNotZero(
            this double numValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue >= 0)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be a negative value and not zero (value: {numValue})!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNegativeAndNotZero(
            this decimal numValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue >= 0)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be a negative value and not zero (value: {numValue})!");
            }
        }

        //---------------------------------------------------------------------
        // Method 'EnsurePositiveOrZero' for all common numeric variables

        [Conditional("DEBUG")]
        public static void EnsurePositiveOrZero(
            this int numValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue < 0)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be positive (value: {numValue})!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsurePositiveOrZero(
            this short numValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue < 0)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be positive (value: {numValue})!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsurePositiveOrZero(
            this long numValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue < 0)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be positive (value: {numValue})!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsurePositiveOrZero(
            this float numValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue < 0)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be positive (value: {numValue})!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsurePositiveOrZero(
            this double numValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue < 0)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be positive (value: {numValue})!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsurePositiveOrZero(
            this decimal numValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue < 0)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be positive (value: {numValue})!");
            }
        }

        //---------------------------------------------------------------------
        // Method 'EnsurePositiveAndNotZero' for all common numeric variables

        [Conditional("DEBUG")]
        public static void EnsurePositiveAndNotZero(
            this int numValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue <= 0)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be positive and not zero (value: {numValue})!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsurePositiveAndNotZero(
            this short numValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue <= 0)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be positive and not zero (value: {numValue})!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsurePositiveAndNotZero(
            this long numValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue <= 0)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be positive and not zero (value: {numValue})!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsurePositiveAndNotZero(
            this float numValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue <= 0)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be positive and not zero (value: {numValue})!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsurePositiveAndNotZero(
            this double numValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue <= 0)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be positive and not zero (value: {numValue})!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsurePositiveAndNotZero(
            this decimal numValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (numValue <= 0)
            {
                throw new SeeingSharpCheckException(
                    $"Value {checkedVariableName} within method {callerMethod} must be positive and not zero (value: {numValue})!");
            }
        }
    }
}