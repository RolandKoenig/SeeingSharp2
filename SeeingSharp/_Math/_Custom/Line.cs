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
using System.Runtime.InteropServices;
using SharpDX;

namespace SeeingSharp
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Line
    {
        public Vector3 StartPosition;
        public Vector3 EndPosition;

        public Line(Vector3 start, Vector3 end)
        {
            StartPosition = start;
            EndPosition = end;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "From " + StartPosition + " to " + EndPosition;
        }

        /// <summary>
        /// Equality test with a small tolerance.
        /// </summary>
        /// <param name="otherLine">The other line to check.</param>
        public bool EqualsWithTolerance(Line otherLine)
        {
            //Check in both directions
            return
                StartPosition.Equals(otherLine.StartPosition) && EndPosition.Equals(otherLine.EndPosition) ||
                EndPosition.Equals(otherLine.StartPosition) && StartPosition.Equals(otherLine.EndPosition);
        }

        /// <summary>
        /// Overrides the x location of all coordinates.
        /// </summary>
        /// <param name="xLocation">The location to set.</param>
        public void SetAllXLocations(float xLocation)
        {
            StartPosition.X = xLocation;
            EndPosition.X = xLocation;
        }

        /// <summary>
        /// Overrides the y location of all coordinates.
        /// </summary>
        /// <param name="yLocation">The location to set.</param>
        public void SetAllYLocations(float yLocation)
        {
            StartPosition.Y = yLocation;
            EndPosition.Y = yLocation;
        }

        /// <summary>
        /// Overrides the z location of all coordinates.
        /// </summary>
        /// <param name="zLocation">The location to set.</param>
        public void SetAllZLocations(float zLocation)
        {
            StartPosition.Z = zLocation;
            EndPosition.Z = zLocation;
        }
    }
}
