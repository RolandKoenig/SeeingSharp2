using System;
using SeeingSharp.Mathematics;

namespace SeeingSharp.Core
{
    public static class MemoryMappedTextureExtensions
    {
        // Some query steps, relevant for QueryForObjectId method
        private static readonly Point[] s_queryObjectIdSteps = {
            new Point(1, 0),new Point(1, 0),
            new Point(0, 1),
            new Point(-1, 0), new Point(-1, 0), new Point(-1, 0), new Point(-1, 0),
            new Point(0, -1), new Point(0, -1),
            new Point(1, 0), new Point(1, 0), new Point(1, 0), new Point(1, 0)
        };

        /// <summary>
        /// Queries for the ObjectId at the given location.
        /// </summary>
        /// <param name="texture">The uploaded texture.</param>
        /// <param name="xPos">The x position where to start.</param>
        /// <param name="yPos">The y position where to start.</param>
        public static unsafe float QueryForObjectId(this MemoryMappedTexture<float> texture, int xPos, int yPos)
        {
            var pixelSize = texture.PixelSize;
            if (xPos < 0) { throw new ArgumentException("xPos"); }
            if (xPos >= pixelSize.Width) { throw new ArgumentException("xPos"); }
            if (yPos < 0) { throw new ArgumentException("yPos"); }
            if (yPos >= pixelSize.Height) { throw new ArgumentException("yPos"); }

            // Loop over more pixels to be sure, that we are directly facing one object
            //  => This is needed because of manipulations done by multisampling (=Antialiasing)
            var pointerNative = texture.GetNativePointer();
            var currentX = xPos;
            var currentY = yPos;
            var lastObjId = pointerNative[currentY * pixelSize.Width + currentX];
            for (var loopActQueryStep = 0; loopActQueryStep < s_queryObjectIdSteps.Length; loopActQueryStep++)
            {
                // Calculate current query location
                var currentStep = s_queryObjectIdSteps[loopActQueryStep];
                currentX += currentStep.X;
                currentY += currentStep.Y;

                // Check whether we are still in a valid pixel coordinate
                if (currentX < 0) { continue; }
                if (currentX >= pixelSize.Width) { continue; }
                if (currentY < 0) { continue; }
                if (currentY >= pixelSize.Height) { continue; }

                // Read current value and compare with last one
                //  (If equal, than at least two pixels are the same => Return this ObjectId)
                var currObjId = pointerNative[currentY * pixelSize.Width + currentX];
                if (EngineMath.EqualsWithTolerance(currObjId, lastObjId)) { return currObjId; }

                // No match found, continue with next one
                lastObjId = currObjId;
            }

            // No clear match found
            return 0f;
        }
    }
}
