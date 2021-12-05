using SharpGen.Runtime;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SeeingSharp.Checking
{
    public static partial class EnsureMultimedia
    {
        [Conditional("DEBUG")]
        internal static void EnsureNotNullOrDisposed(
            this DisposeBase disposeBase, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (disposeBase == null ||
                disposeBase.IsDisposed)
            {
                throw new SeeingSharpCheckException(
                    $"Resource {checkedVariableName} within method {callerMethod} musst not be null or disposed!");
            }
        }

        [Conditional("DEBUG")]
        internal static void EnsureNotDisposed(
            this DisposeBase disposeBase, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (disposeBase.IsDisposed)
            {
                throw new SeeingSharpCheckException(
                    $"Resource {checkedVariableName} within method {callerMethod} musst not be disposed!");
            }
        }
    }
}
