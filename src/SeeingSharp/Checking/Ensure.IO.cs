using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace SeeingSharp.Checking
{
    public static partial class Ensure
    {
        [Conditional("DEBUG")]
        public static void EnsureReadable(
            this Stream stream, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            try
            {
                if (!stream.CanRead)
                {
                    throw new SeeingSharpCheckException(
                        $"Stream {checkedVariableName} within method {callerMethod} must be readable!");
                }
            }
            catch (ObjectDisposedException)
            {
                throw new SeeingSharpCheckException(
                    $"Stream {checkedVariableName} within method {callerMethod} must not be disposed!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureWritable(
            this Stream stream, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            try
            {
                if (!stream.CanWrite)
                {
                    throw new SeeingSharpCheckException(
                        $"Stream {checkedVariableName} within method {callerMethod} must be writable!");
                }
            }
            catch (ObjectDisposedException)
            {
                throw new SeeingSharpCheckException(
                    $"Stream {checkedVariableName} within method {callerMethod} must not be disposed!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureSeekable(
            this Stream stream, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            try
            {
                if (!stream.CanSeek)
                {
                    throw new SeeingSharpCheckException(
                        $"Stream {checkedVariableName} within method {callerMethod} must be seekable!");
                }
            }
            catch (ObjectDisposedException)
            {
                throw new SeeingSharpCheckException(
                    $"Stream {checkedVariableName} within method {callerMethod} must not be disposed!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureFileExists(
            this string filePath, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (!File.Exists(filePath))
            {
                throw new SeeingSharpCheckException(
                    $"Filepath {checkedVariableName} within method {callerMethod} could not be resolved (value: {filePath})!");
            }
        }
    }
}
