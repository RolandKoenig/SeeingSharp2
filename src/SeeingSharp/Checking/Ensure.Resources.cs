using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using SeeingSharp.Core;
using SeeingSharp.Util;
using SharpGen.Runtime;

namespace SeeingSharp.Checking
{
    public static partial class Ensure
    {
        [Conditional("DEBUG")]
        public static void EnsureResourceLoaded(
            this ComObject? resource, Type resourceHolder,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (resource == null ||
                resource.IsDisposed)
            {
                throw new SeeingSharpResourcesNotLoadedException(resourceHolder, callerMethod);
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureResourceLoaded(
            this Resource? resource, Type resourceHolder,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if ((resource == null) ||
                (!resource.IsLoaded))
            {
                throw new SeeingSharpResourcesNotLoadedException(resourceHolder, callerMethod);
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureResourceLoaded(
            this ICheckDisposed? resource, Type resourceHolder,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (resource == null ||
                resource.IsDisposed)
            {
                throw new SeeingSharpResourcesNotLoadedException(resourceHolder, callerMethod);
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureResourceLoaded<T>(
            this Lazy<T>? resource, Type resourceHolder,
            [CallerMemberName]
            string callerMethod = "")
            where T : ComObject
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if ((resource == null) ||
                (resource.IsValueCreated && resource.Value.IsDisposed))
            {
                throw new SeeingSharpResourcesNotLoadedException(resourceHolder, callerMethod);
            }
        }
    }
}
