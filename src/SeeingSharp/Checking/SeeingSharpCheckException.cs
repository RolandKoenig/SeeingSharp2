using System;

namespace SeeingSharp.Checking
{
    public class SeeingSharpCheckException : SeeingSharpException
    {
        /// <summary>
        /// Creates a new CommonLibraryException object
        /// </summary>
        public SeeingSharpCheckException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new CommonLibraryException object
        /// </summary>
        public SeeingSharpCheckException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}