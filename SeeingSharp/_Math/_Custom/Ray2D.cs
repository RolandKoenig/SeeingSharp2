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
    public struct Ray2D
    {
        public Vector2 Origin;
        public Vector2 Direction;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ray2D" /> struct.
        /// </summary>
        public Ray2D(Vector2 origin, Vector2 direction)
        {
            Origin = origin;
            Direction = direction;
        }

        /// <summary>
        /// Calculates the intersection point to the intersection seen from local origin position.
        /// </summary>
        /// <param name="other">The ray to calculate intersection with</param>
        public Tuple<bool, Vector2> Intersect(Ray2D other)
        {
            //Intersection method taken from http://stackoverflow.com/questions/4543506/algorithm-for-intersection-of-2-lines

            var a1 = this.A;
            var b1 = this.B;
            var c1 = this.C;
            var a2 = other.A;
            var b2 = other.B;
            var c2 = other.C;

            var delta = a1 * b2 - a2 * b1;
            if (EngineMath.EqualsWithTolerance(delta, 0))
            {
                return Tuple.Create(false, Vector2.Zero);
            }

            var intersectionX = (b2 * c1 - b1 * c2) / delta;
            var intersectionY = (a1 * c2 - a2 * c1) / delta;

            return Tuple.Create(true, new Vector2(intersectionX, intersectionY));
        }

        /// <summary>
        /// Performs a equality check with a slight tolerance.
        /// </summary>
        /// <param name="otherRay">The other ray to check.</param>
        public bool EqualsWithTolerance(Ray2D otherRay)
        {
            return Origin.Equals(otherRay.Origin) &&
                   Direction.Equals(otherRay.Direction);
        }

        public float A
        {
            get
            {
                var start = Origin;
                var end = Origin + Direction;
                return end.Y - start.Y;
            }
        }

        public float B
        {
            get
            {
                var start = Origin;
                var end = Origin + Direction;
                return start.X - end.X;
            }
        }

        public float C
        {
            get
            {
                var start = Origin;
                var end = Origin + Direction;
                return this.A * start.X + this.B * start.Y;
            }
        }
    }
}