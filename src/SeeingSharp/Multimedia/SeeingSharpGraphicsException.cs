using System;

namespace SeeingSharp.Multimedia
{
    public class SeeingSharpGraphicsException : SeeingSharpException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SeeingSharpGraphicsException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public SeeingSharpGraphicsException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SeeingSharpGraphicsException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public SeeingSharpGraphicsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}