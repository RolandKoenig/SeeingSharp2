using System;

namespace SeeingSharp
{
    public class SeeingSharpException : Exception
    {
        /// <summary>
        /// Creates a new CommonLibraryException object
        /// </summary>
        public SeeingSharpException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new CommonLibraryException object
        /// </summary>
        public SeeingSharpException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}