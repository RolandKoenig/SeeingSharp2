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
using SharpDX;

namespace SeeingSharp
{
    public struct Line2D
    {
        public Vector2 StartPosition;
        public Vector2 EndPosition;

        /// <summary>
        /// Initializes a new instance of the <see cref="Line2D" /> struct.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        public Line2D(Vector2 start, Vector2 end)
        {
            StartPosition = start;
            EndPosition = end;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Line2D" /> struct.
        /// </summary>
        /// <param name="x1">X coordinate of the start point.</param>
        /// <param name="y1">Y coordinate of the start point.</param>
        /// <param name="x2">X coordinate of the end point.</param>
        /// <param name="y2">Y coordinate of the end point.</param>
        public Line2D(float x1, float y1, float x2, float y2)
            : this(new Vector2(x1, y1), new Vector2(x2, y2))
        {

        }

        /// <summary>
        /// Gets a ray out of this line.
        /// </summary>
        public Ray2D ToRay()
        {
            return new Ray2D(
                StartPosition,
                Vector2.Normalize(EndPosition - StartPosition));
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
        /// Calculates the intersection point and distance to the intersection seen from local start position.
        /// </summary>
        /// <param name="other">The line to calculate intersection with</param>
        public Tuple<bool, Vector2> Intersect(Line2D other)
        {
            //Perform simple ray to ray intersection first
            var ray1 = this.ToRay();
            var ray2 = other.ToRay();
            var intersectionResult = ray1.Intersect(ray2);

            if (!intersectionResult.Item1)
            {
                return intersectionResult;
            }

            //Is intersection point within line 1?
            var distanceTo1 = Vector2.Distance(ray1.Origin, intersectionResult.Item2);

            if (distanceTo1 > this.Length)
            {
                return Tuple.Create(false, Vector2.Zero);
            }

            //Is intersection point within line 2?
            var distanceTo2 = Vector2.Distance(ray2.Origin, intersectionResult.Item2);

            if (distanceTo2 > other.Length)
            {
                return Tuple.Create(false, Vector2.Zero);
            }

            return intersectionResult;
        }

        /// <summary>
        /// Calculates the intersection point and distance to the intersection seen from local start position.
        /// </summary>
        /// <param name="other">The ray to calculate intersection with</param>
        public Tuple<bool, Vector2> Intersect(Ray2D other)
        {
            //Perform simple ray to ray intersection first
            var ray1 = this.ToRay();
            var intersectionResult = ray1.Intersect(other);
            if (!intersectionResult.Item1) { return intersectionResult; }

            //Is intersection point within line 1?
            var distanceTo1 = Vector2.Distance(ray1.Origin, intersectionResult.Item2);
            if (distanceTo1 > this.Length) { return Tuple.Create(false, Vector2.Zero); }

            return intersectionResult;
        }

        /// <summary>
        /// Gets the length of this line.
        /// </summary>
        public float Length
        {
            get
            {
                var lengthVector = EndPosition - StartPosition;
                return lengthVector.Length();
            }
        }
    }
}