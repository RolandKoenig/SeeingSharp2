using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace SeeingSharp.Checking
{
    public static partial class Ensure
    {
        [Conditional("DEBUG")]
        public static void EnsureNotEmpty(
            this Size size, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (size.Width <= 0 ||
                size.Height <= 0)
            {
                throw new SeeingSharpCheckException(
                    $"Size {checkedVariableName} within method {callerMethod} must not be empty (given size: {size})!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNotEmpty(
            this SizeF size, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (size.Width <= 0f ||
                size.Height <= 0f)
            {
                throw new SeeingSharpCheckException(
                    $"Size {checkedVariableName} within method {callerMethod} must not be empty (given size: {size})!");
            }
        }
    }
}
