using System;
using System.Drawing.Imaging;
using SeeingSharp.Checking;
using GDI = System.Drawing;

namespace SeeingSharp.Tests
{
    /// <summary>
    /// This class is for comparing two pictures and to calculate the difference between these.
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
        private const decimal MAX_PER_PIXEL_DIFF_DEC = MAX_PER_PIXEL_DIFF;

        /// <summary>
        /// Determines whether the given bitmaps are near equality.
        /// </summary>
        /// <param name="bitmapLeft">The bitmap left.</param>
        /// <param name="bitmapRight">The bitmap right.</param>
        public static bool IsNearEqual(GDI.Bitmap bitmapLeft, GDI.Bitmap bitmapRight)
        {
            return CalculatePercentageDifference(bitmapLeft, bitmapRight) < 0.05;
        }

        /// <summary>
        /// Calculate a difference between two pictures
        /// </summary>
        /// <param name="bitmapLeft">The first bitmap for the comparison.</param>
        /// <param name="bitmapRight">The second bitmap for the comparison.</param>
        /// <returns>Difference of both bitmaps in percent</returns>
        public static float CalculatePercentageDifference(GDI.Bitmap bitmapLeft, GDI.Bitmap bitmapRight)
        {
            bitmapLeft.EnsureNotNull(nameof(bitmapLeft));
            bitmapRight.EnsureNotNull(nameof(bitmapRight));
            if (bitmapLeft.Size != bitmapRight.Size) { throw new SeeingSharpException("Both bitmaps musst have the same size!"); }

            var totalDiffPercent = 0M;

            var lockRect = new GDI.Rectangle(
                new GDI.Point(0, 0),
                bitmapLeft.Size);
            var dataLeft = bitmapLeft.LockBits(lockRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var dataRight = bitmapRight.LockBits(lockRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                unsafe
                {
                    var dataLeftP = (byte*)dataLeft.Scan0.ToPointer();
                    var dataRightP = (byte*)dataRight.Scan0.ToPointer();
                    decimal pixelCount = lockRect.Width * lockRect.Height;

                    for (var loopY = 0; loopY < lockRect.Height; loopY++)
                    {
                        for (var loopX = 0; loopX < lockRect.Width; loopX++)
                        {
                            // Calculate the memory location of the current pixel
                            var currentLoc = loopY * lockRect.Height * 4 + loopX * 4;

                            // Get the maximum difference on this pixel on one color channel
                            var diffA = Math.Abs(dataLeftP[currentLoc] - dataRightP[currentLoc]);
                            var diffR = Math.Abs(dataLeftP[currentLoc + 1] - dataRightP[currentLoc + 1]);
                            var diffG = Math.Abs(dataLeftP[currentLoc + 2] - dataRightP[currentLoc + 2]);
                            var diffB = Math.Abs(dataLeftP[currentLoc + 3] - dataRightP[currentLoc + 3]);
                            var maxDiff = Math.Max(diffA, Math.Max(diffR, Math.Max(diffG, diffB)));
                            if (maxDiff > MAX_PER_PIXEL_DIFF) { maxDiff = MAX_PER_PIXEL_DIFF; }

                            // Calculate the difference factor on this pixel and add it to the total difference value
                            var pixelDiffPercent =
                                maxDiff / MAX_PER_PIXEL_DIFF_DEC;
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