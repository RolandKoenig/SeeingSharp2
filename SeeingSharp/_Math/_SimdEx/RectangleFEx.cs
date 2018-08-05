using SharpDX;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeeingSharp
{
    public static class RectangleFEx
    {
        /// <summary>Pushes the edges of the Rectangle out by the horizontal and vertical values specified.</summary>
        /// <param name="horizontalAmount">Value to push the sides out by.</param>
        /// <param name="verticalAmount">Value to push the top and bottom out by.</param>
        public static void Inflate(this ref RectangleF rectangle, float horizontalAmount, float verticalAmount)
        {
            rectangle.X -= horizontalAmount;
            rectangle.Y -= verticalAmount;
            rectangle.Width += horizontalAmount * 2;
            rectangle.Height += verticalAmount * 2;
        }
    }
}
