using System;
using System.Collections.Generic;
using System.Text;

namespace SeeingSharp
{
    public class SeeingSharpResourcesNotLoadedException : SeeingSharpGraphicsException
    {
        public SeeingSharpResourcesNotLoadedException(Type resourceHolder, string? callerMethod = null)
            : base(FormatErrorString(resourceHolder, callerMethod))
        {

        }

        public static string FormatErrorString(Type resourceHolder, string? callerMethod)
        {
            if (string.IsNullOrEmpty(callerMethod))
            {
                return $"Resources for object of class {resourceHolder.FullName} not loaded!";
            }
            else
            {
                return $"Resources for object of class {resourceHolder.FullName} not loaded (method: {callerMethod})!";
            }
        }
    }
}
