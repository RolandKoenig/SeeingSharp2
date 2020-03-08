/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
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
using System;

namespace SeeingSharp.Multimedia.Core
{
    public static class MemoryMappedTextureExtensions
    {
        // Some query steps, relevant for QueryForObjectID method
        private static readonly Point[] QUERY_OBJECT_ID_STEPS = {
            new Point(1, 0),new Point(1, 0),
            new Point(0, 1),
            new Point(-1, 0), new Point(-1, 0), new Point(-1, 0), new Point(-1, 0),
            new Point(0, -1), new Point(0, -1),
            new Point(1, 0), new Point(1, 0), new Point(1, 0), new Point(1, 0)
        };

        /// <summary>
        /// Queries for the ObjectID at the given location.
        /// </summary>
        /// <param name="texture">The uploaded texture.</param>
        /// <param name="xPos">The x position where to start.</param>
        /// <param name="yPos">The y position where to start.</param>
        public static unsafe float QueryForObjectID(this MemoryMappedTexture<float> texture, int xPos, int yPos)
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
            var lastObjID = pointerNative[currentY * pixelSize.Width + currentX];
            for (var loopActQueryStep = 0; loopActQueryStep < QUERY_OBJECT_ID_STEPS.Length; loopActQueryStep++)
            {
                // Calculate current query location
                var currentStep = QUERY_OBJECT_ID_STEPS[loopActQueryStep];
                currentX += currentStep.X;
                currentY += currentStep.Y;

                // Check whether we are still in a valid pixel coordinate
                if (currentX < 0) { continue; }
                if (currentX >= pixelSize.Width) { continue; }
                if (currentY < 0) { continue; }
                if (currentY >= pixelSize.Height) { continue; }

                // Read current value and compare with last one
                //  (If equal, than at least two pixels are the same => Return this ObjectID)
                var currObjID = pointerNative[currentY * pixelSize.Width + currentX];
                if (EngineMath.EqualsWithTolerance(currObjID, lastObjID)) { return currObjID; }

                // No match found, continue with next one
                lastObjID = currObjID;
            }

            // No clear match found
            return 0f;
        }
    }
}
