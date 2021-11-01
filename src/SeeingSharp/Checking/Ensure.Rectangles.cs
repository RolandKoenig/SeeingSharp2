using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SeeingSharp.Checking
{
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
                throw new SeeingSharpCheckException(
                    $"Rectangle {checkedVariableName} within method {callerMethod} must not be empty (given rectangle: {rectangle})!");
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
                throw new SeeingSharpCheckException(
                    $"Rectangle {checkedVariableName} within method {callerMethod} must not be empty (given rectangle: {rectangle})!");
            }
        }
    }
}
