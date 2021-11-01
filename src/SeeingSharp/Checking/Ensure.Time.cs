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
