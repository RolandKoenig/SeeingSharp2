#region License information
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
    Copyright (C) 2018 Roland König (RolandK)
    
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
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GDI = System.Drawing;

namespace SeeingSharp.Tests.Util
{
    /// <summary>
    /// This class is for comparing two pictueres and to calculate the difference between these.
    /// Originally, this algorithm was based on http://www.codeproject.com/Articles/374386/Simple-image-comparison-in-NET.
    /// 
    /// In order to apply ghe GPLv3 to this project, I reimplemented this class and made following main changes:
    ///  - No scaling of the images to smaller representations (not needed for my use-cases)
    ///  - No use of List of float during comparison algorithm
    ///  - No GetPixel!
    /// </summary>
    public class BitmapComparison
    {
        private const int MAX_PER_PIXEL_DIFF = 128;
        private const decimal MAX_PER_PIXEL_DIFF_DEC = (decimal)MAX_PER_PIXEL_DIFF;

        /// <summary>
        /// Determines whether the given bitmaps are near equality.
        /// </summary>
        /// <param name="bitmapLeft">The bitmap left.</param>
        /// <param name="bitmapRight">The bitmap right.</param>
        public static bool IsNearEqual(Bitmap bitmapLeft, Bitmap bitmapRight)
        {
            return CalculatePercentageDifference(bitmapLeft, bitmapRight) < 0.05;
        }

        /// <summary>
        /// Caculate a differece between two pictures
        /// </summary>
        /// <param name="bitmapLeft">The first bitmap for the comparison.</param>
        /// <param name="bitmapRight">The second bitmap for the comparison.</param>
        /// <returns>Difference of both bitmaps in precent</returns>
        public static float CalculatePercentageDifference(Bitmap bitmapLeft, Bitmap bitmapRight)
        {
            if (bitmapLeft == null) { throw new ArgumentNullException("bitmapLeft"); }
            if (bitmapRight == null) { throw new ArgumentNullException("bitmapRight"); }
            if (bitmapLeft.Size != bitmapRight.Size) { throw new SeeingSharpException("Both bitmaps musst have the same size!"); }

            decimal totalDiffPercent = 0M;

            GDI.Rectangle lockRect = new GDI.Rectangle(
                new GDI.Point(0, 0),
                bitmapLeft.Size);
            BitmapData dataLeft = bitmapLeft.LockBits(lockRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData dataRight = bitmapRight.LockBits(lockRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                unsafe
                {
                    byte* dataLeftP = (byte*)dataLeft.Scan0.ToPointer();
                    byte* dataRightP = (byte*)dataRight.Scan0.ToPointer();
                    decimal pixelCount = lockRect.Width * lockRect.Height;

                    for (int loopY = 0; loopY < lockRect.Height; loopY++)
                    {
                        for (int loopX = 0; loopX < lockRect.Width; loopX++)
                        {
                            // Calculate the memory location of the current pixel
                            int currentLoc = loopY * lockRect.Height * 4 + loopX * 4;

                            // Get the maximum difference on this pixel on one color channel
                            int diffA = Math.Abs(dataLeftP[currentLoc] - dataRightP[currentLoc]);
                            int diffR = Math.Abs(dataLeftP[currentLoc + 1] - dataRightP[currentLoc + 1]);
                            int diffG = Math.Abs(dataLeftP[currentLoc + 2] - dataRightP[currentLoc + 2]);
                            int diffB = Math.Abs(dataLeftP[currentLoc + 3] - dataRightP[currentLoc + 3]);
                            int maxDiff = Math.Max(diffA, Math.Max(diffR, Math.Max(diffG, diffB)));
                            if (maxDiff > MAX_PER_PIXEL_DIFF) { maxDiff = MAX_PER_PIXEL_DIFF; }

                            // Calculate the difference factor on this pixel and add it to the total difference value
                            decimal pixelDiffPercent =
                                (decimal)((decimal)maxDiff / MAX_PER_PIXEL_DIFF_DEC);
                            totalDiffPercent += pixelDiffPercent / pixelCount;
                        }
                    }
                }
            }
            finally
            {
                bitmapLeft.UnlockBits(dataLeft);
                bitmapRight.UnlockBits(dataRight);
            }

            return (float)totalDiffPercent;
        }
    }
}
