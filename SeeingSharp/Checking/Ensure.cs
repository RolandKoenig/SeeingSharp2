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
using System.Reflection;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Util;

namespace SeeingSharp.Checking
{
    /// <summary>
    /// This class contains some helper methods which can be used
    /// to check method parameters.
    /// Note: All methods are only executing in DebugBuilds.
    /// </summary>
    public static partial class Ensure
    {
        public static void EnsureEqual(
            this object toCompare, object other, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (toCompare != other)
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Object {0} within method {1} has not the expected value!",
                    checkedVariableName, callerMethod));
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNotNullOrDisposed(
            this ICheckDisposed disposable, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if ((disposable == null) ||
                (disposable.IsDisposed))
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Disposable onject {0} within method {1} must not be null or disposed!",
                    checkedVariableName, callerMethod));
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureFalse(
            this bool boolValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (boolValue)
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Boolean {0} within method {1} must be false!",
                    checkedVariableName, callerMethod));
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureTrue(
            this bool boolValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (!boolValue)
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Boolean {0} within method {1} must be true!",
                    checkedVariableName, callerMethod));
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNotNullOrEmpty<T>(
            this T[] array, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if ((array == null) ||
                (array.Length == 0))
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Array {0} within method {1} must not be null or empty!",
                    checkedVariableName, callerMethod));
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNotNullOrEmpty<T>(
            this ICollection<T> collection, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if ((collection == null) ||
                (collection.Count == 0))
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Collection {0} within method {1} must not be null or empty!",
                    checkedVariableName, callerMethod));
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNotNullOrEmpty<T>(
            this IReadOnlyCollection<T> collection, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if ((collection == null) ||
                (collection.Count == 0))
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Collection {0} within method {1} must not be null or empty!",
                    checkedVariableName, callerMethod));
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNotNullOrEmpty(
            this string stringParam, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if(string.IsNullOrEmpty(stringParam))
            {
                throw new SeeingSharpCheckException(string.Format(
                    "String {0} within method {1} must not be null or empty!",
                    checkedVariableName, callerMethod));
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNotNullOrEmptyOrWhiteSpace(
            this string stringParam, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (string.IsNullOrWhiteSpace(stringParam))
            {
                throw new SeeingSharpCheckException(string.Format(
                    "String {0} within method {1} must not be null or empty!",
                    checkedVariableName, callerMethod));
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNotNull(
            this object objParam, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (objParam == null)
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Object {0} within method {1} must not be null!",
                    checkedVariableName, callerMethod));
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNotNullWhenNullIsNotAllowed<T>(
            this object objParam, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if ((objParam == null) && 
                (default(T) != null))
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Object {0} within method {1} must not be null because type argument is not by ref!",
                    checkedVariableName, callerMethod));
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureAssignableTo<T>(
            this object objParam, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (objParam == null) { return; }
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            var genericTypeInfo = typeof(T).GetTypeInfo();
            var argTypeInfo = objParam.GetType().GetTypeInfo();
            if (!genericTypeInfo.IsAssignableFrom(argTypeInfo))
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Object {0} within method {1} can not be assigned to type {2}!",
                    checkedVariableName, callerMethod, genericTypeInfo.FullName));
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNull(
            this object objParam, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (objParam != null)
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Object {0} within method {1} must be null!",
                    checkedVariableName, callerMethod));
            }
        }
    }
}
