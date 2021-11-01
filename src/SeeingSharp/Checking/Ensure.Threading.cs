using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SeeingSharp.Checking
{
    public static partial class Ensure
    {
        [Conditional("DEBUG")]
        public static void IsThreadPoolThread(
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (!Thread.CurrentThread.IsThreadPoolThread)
            {
                throw new SeeingSharpCheckException(
                    $"Current Thread is not a Threadpool Thread within method {callerMethod} !");
            }
        }

        [Conditional("DEBUG")]
        public static void IsThread(string threadName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (Thread.CurrentThread.Name != threadName)
            {
                throw new SeeingSharpCheckException(
                    $"Current Thread is not the Thread with name {threadName} within method {callerMethod} !");
            }
        }
    }
}
