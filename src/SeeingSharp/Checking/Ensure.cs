using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        [Conditional("DEBUG")]
        public static void EnsureEqual(
            this object toCompare, object other, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (toCompare != other)
            {
                throw new SeeingSharpCheckException(
                    $"Object {checkedVariableName} within method {callerMethod} has not the expected value!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureEqualComparable<T>(
            this T toCompare, T other, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
            where T : IComparable<T>
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (toCompare.CompareTo(other) != 0)
            {
                throw new SeeingSharpCheckException(
                    $"Object {checkedVariableName} within method {callerMethod} has not the expected value!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNotNullOrDisposed(
            this ICheckDisposed disposable, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (disposable == null ||
                disposable.IsDisposed)
            {
                throw new SeeingSharpCheckException(
                    $"Disposable onject {checkedVariableName} within method {callerMethod} must not be null or disposed!");
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
                throw new SeeingSharpCheckException(
                    $"Boolean {checkedVariableName} within method {callerMethod} must be false!");
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
                throw new SeeingSharpCheckException(
                    $"Boolean {checkedVariableName} within method {callerMethod} must be true!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNotNullOrEmpty<T>(
            this T[] array, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (array == null ||
                array.Length == 0)
            {
                throw new SeeingSharpCheckException(
                    $"Array {checkedVariableName} within method {callerMethod} must not be null or empty!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNotNullOrEmpty<T>(
            this ICollection<T> collection, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (collection == null ||
                collection.Count == 0)
            {
                throw new SeeingSharpCheckException(
                    $"Collection {checkedVariableName} within method {callerMethod} must not be null or empty!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNotNullOrEmpty<T>(
            this IReadOnlyCollection<T> collection, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (collection == null ||
                collection.Count == 0)
            {
                throw new SeeingSharpCheckException(
                    $"Collection {checkedVariableName} within method {callerMethod} must not be null or empty!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNotNullOrEmpty(
            this string stringParam, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (string.IsNullOrEmpty(stringParam))
            {
                throw new SeeingSharpCheckException(
                    $"String {checkedVariableName} within method {callerMethod} must not be null or empty!");
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
                throw new SeeingSharpCheckException(
                    $"String {checkedVariableName} within method {callerMethod} must not be null or empty!");
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
                throw new SeeingSharpCheckException(
                    $"Object {checkedVariableName} within method {callerMethod} must not be null!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNotNullWhenNullIsNotAllowed<T>(
            this object objParam, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (objParam == null &&
                default(T) != null)
            {
                throw new SeeingSharpCheckException(
                    $"Object {checkedVariableName} within method {callerMethod} must not be null because type argument is not by ref!");
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
                throw new SeeingSharpCheckException(
                    $"Object {checkedVariableName} within method {callerMethod} can not be assigned to type {genericTypeInfo.FullName}!");
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
                throw new SeeingSharpCheckException(
                    $"Object {checkedVariableName} within method {callerMethod} must be null!");
            }
        }
    }
}
