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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
                    throw new SeeingSharpCheckException(string.Format(
                        "Stream {0} within method {1} must be readable!",
                        checkedVariableName, callerMethod));
                }
            }
            catch(ObjectDisposedException)
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Stream {0} within method {1} must not be disposed!",
                    checkedVariableName, callerMethod));
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
                    throw new SeeingSharpCheckException(string.Format(
                        "Stream {0} within method {1} must be writable!",
                        checkedVariableName, callerMethod));
                }
            }
            catch (ObjectDisposedException)
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Stream {0} within method {1} must not be disposed!",
                    checkedVariableName, callerMethod));
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
                    throw new SeeingSharpCheckException(string.Format(
                        "Stream {0} within method {1} must be seekable!",
                        checkedVariableName, callerMethod));
                }
            }
            catch (ObjectDisposedException)
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Stream {0} within method {1} must not be disposed!",
                    checkedVariableName, callerMethod));
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureFileExists(
            this string filePath, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

#if DESKTOP
            if (!File.Exists(filePath))
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Filepath {0} within method {1} could not be resolved (value: {2})!",
                    checkedVariableName, callerMethod,
                    filePath));
            }
#else
            // Not possible on WinRT at all (no direct filesystem access)
            throw new SeeingSharpCheckException(string.Format(
                "Filepath {0} within method {1} could not be resolved (value: {2})!",
                checkedVariableName, callerMethod,
                filePath));
#endif
        }

    }
}
