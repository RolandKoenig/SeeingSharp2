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
using System.Collections.Generic;
using System.Numerics;

namespace SeeingSharp
{
    public partial struct BoundingBox
    {
        public static BoundingBox Empty;

        public BoundingBox(IEnumerable<Vector3> containedLocations)
        {
            var minimum = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var maximum = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            var anyInteraction = false;
            foreach (var actContainedLocation in containedLocations)
            {
                anyInteraction = true;

                if (minimum.X > actContainedLocation.X) { minimum.X = actContainedLocation.X; }
                if (minimum.Y > actContainedLocation.Y) { minimum.Y = actContainedLocation.Y; }
                if (minimum.Z > actContainedLocation.Z) { minimum.Z = actContainedLocation.Z; }

                if (maximum.X < actContainedLocation.X) { maximum.X = actContainedLocation.X; }
                if (maximum.Y < actContainedLocation.Y) { maximum.Y = actContainedLocation.Y; }
                if (maximum.Z < actContainedLocation.Z) { maximum.Z = actContainedLocation.Z; }
            }

            if (!anyInteraction) { throw new SeeingSharpException("No vectors given!"); }

            Minimum = minimum;
            Maximum = maximum;
        }

        public void Transform(Matrix4x4 matrix)
        {
            if(this == BoundingBox.Empty){ return; }

            var corners = this.GetCorners();
            for (var loop = 0; loop < corners.Length; loop++)
            {
                corners[loop] = Vector3.Transform(corners[loop], matrix);
            }

            this.Redefine(corners);
        }

        /// <summary>
        /// Redefines this bounding box based on given points.
        /// </summary>
        /// <param name="points">All points to apply.</param>
        public void Redefine(Vector3[] points)
        {
            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);
            for (var i = 0; i < points.Length; ++i)
            {
                min = Vector3.Min(min, points[i]);
                max = Vector3.Max(max, points[i]);
            }

            Minimum = min;
            Maximum = max;
        }

        /// <summary>
        /// Expands this AxisAlignedBox so that it contains the given location.
        /// </summary>
        /// <param name="newLocation">New location to be merged to this AxisAlignedBox.</param>
        public void MergeWith(Vector3 newLocation)
        {
            //Handle x axis
            if (newLocation.X < Minimum.X) { Minimum.X = newLocation.X; }
            else if (newLocation.X > Maximum.X) { Maximum.X = newLocation.X; }

            //Handle y axis
            if (newLocation.Y < Minimum.Y) { Minimum.Y = newLocation.Y; }
            else if (newLocation.Y > Maximum.Y) { Maximum.Y = newLocation.Y; }

            //Handle z axis
            if (newLocation.Z < Minimum.Z) { Minimum.Z = newLocation.Z; }
            else if (newLocation.Z > Maximum.Z) { Maximum.Z = newLocation.Z; }
        }

        /// <summary>
        /// Merges this box with the given one
        /// </summary>
        public void MergeWith(BoundingBox other)
        {
            var minimum1 = Minimum;
            var minimum2 = other.Minimum;
            var maximum1 = Maximum;
            var maximum2 = other.Maximum;

            var newMinimum = Vector3.Min(minimum1, minimum2);
            var newMaximum = Vector3.Max(maximum1, maximum2);

            Minimum = newMinimum;
            Maximum = newMaximum;
        }

        /// <summary>
        /// Gets the coordinate of middle of bottom-left border.
        /// </summary>
        public Vector3 GetBottomLeftMiddleCoordinate()
        {
            var result = Minimum + (Maximum - Minimum) / 2f;
            result.Y = Minimum.Y;
            result.X = Minimum.X;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of bottom-right border.
        /// </summary>
        public Vector3 GetBottomRightMiddleCoordinate()
        {
            var result = Minimum + (Maximum - Minimum) / 2f;
            result.Y = Minimum.Y;
            result.X = Maximum.X;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of bottom-front border.
        /// </summary>
        public Vector3 GetBottomFrontMiddleCoordinate()
        {
            var result = Minimum + (Maximum - Minimum) / 2f;
            result.Y = Minimum.Y;
            result.Z = Minimum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of bottom-back border.
        /// </summary>
        public Vector3 GetBottomBackMiddleCoordinate()
        {
            var result = Minimum + (Maximum - Minimum) / 2f;
            result.Y = Minimum.Y;
            result.Z = Maximum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of the middle of the box.
        /// </summary>
        public Vector3 GetMiddleCoordinate()
        {
            return Minimum + (Maximum - Minimum) / 2f;
        }

        /// <summary>
        /// Gets the coordinate of middle of top rectangle.
        /// </summary>
        public Vector3 GetTopMiddleCoordinate()
        {
            var result = Minimum + (Maximum - Minimum) / 2f;
            result.Y = Maximum.Y;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of top-left border.
        /// </summary>
        public Vector3 GetTopLeftMiddleCoordinate()
        {
            var result = Minimum + (Maximum - Minimum) / 2f;
            result.Y = Maximum.Y;
            result.X = Minimum.X;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of top-right border.
        /// </summary>
        public Vector3 GetTopRightMiddleCoordinate()
        {
            var result = Minimum + (Maximum - Minimum) / 2f;
            result.Y = Maximum.Y;
            result.X = Maximum.X;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of top-front border.
        /// </summary>
        public Vector3 GetTopFrontMiddleCoordinate()
        {
            var result = Minimum + (Maximum - Minimum) / 2f;
            result.Y = Maximum.Y;
            result.Z = Minimum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of top-back border.
        /// </summary>
        public Vector3 GetTopBackMiddleCoordinate()
        {
            var result = Minimum + (Maximum - Minimum) / 2f;
            result.Y = Maximum.Y;
            result.Z = Maximum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of front rectangle.
        /// </summary>
        public Vector3 GetFrontMiddleCoordinate()
        {
            var result = Minimum + (Maximum - Minimum) / 2f;
            result.Z = Minimum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of back rectangle.
        /// </summary>
        public Vector3 GetBackMiddleCoordinate()
        {
            var result = Minimum + (Maximum - Minimum) / 2f;
            result.Z = Maximum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of left rectangle.
        /// </summary>
        public Vector3 GetLeftMiddleCoordinate()
        {
            var result = Minimum + (Maximum - Minimum) / 2f;
            result.X = Minimum.X;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of left-front border.
        /// </summary>
        public Vector3 GetLeftFrontMiddleCoordinate()
        {
            var result = Minimum + (Maximum - Minimum) / 2f;
            result.X = Minimum.X;
            result.Z = Minimum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of left-back border.
        /// </summary>
        public Vector3 GetLeftBackMiddleCoordinate()
        {
            var result = Minimum + (Maximum - Minimum) / 2f;
            result.X = Minimum.X;
            result.Z = Maximum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of right rectangle.
        /// </summary>
        public Vector3 GetRightMiddleCoordinate()
        {
            var result = Minimum + (Maximum - Minimum) / 2f;
            result.X = Maximum.X;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of right-front border.
        /// </summary>
        public Vector3 GetRightFrontMiddleCoordinate()
        {
            var result = Minimum + (Maximum - Minimum) / 2f;
            result.X = Maximum.X;
            result.Z = Minimum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of right-back border.
        /// </summary>
        public Vector3 GetRightBackMiddleCoordinate()
        {
            var result = Minimum + (Maximum - Minimum) / 2f;
            result.X = Maximum.X;
            result.Z = Maximum.Z;
            return result;
        }

        /// <summary>
        /// Gets the middle center of the box.
        /// </summary>
        public Vector3 GetMiddleCenter()
        {
            var size = this.GetSize();
            return new Vector3(
                Minimum.X + size.X / 2f,
                Minimum.Y + size.Y / 2f,
                Minimum.Z + size.Z / 2f);
        }

        /// <summary>
        /// Gets the bottom center of the box.
        /// </summary>
        public Vector3 GetBottomCenter()
        {
            var size = this.GetSize();
            return new Vector3(
                Minimum.X + size.X / 2f,
                Minimum.Y,
                Minimum.Z + size.Z / 2f);
        }

        /// <summary>
        /// Builds a line list containing lines for all borders of this box.
        /// </summary>
        public List<Vector3> BuildLineListForBorders()
        {
            var result = new List<Vector3>();
            var size = this.GetSize();

            //Add front face
            result.Add(Minimum);
            result.Add(Minimum + new Vector3(size.X, 0f, 0f));
            result.Add(Minimum + new Vector3(size.X, 0f, 0f));
            result.Add(Minimum + new Vector3(size.X, size.Y, 0f));
            result.Add(Minimum + new Vector3(size.X, size.Y, 0f));
            result.Add(Minimum + new Vector3(0f, size.Y, 0f));
            result.Add(Minimum + new Vector3(0f, size.Y, 0f));
            result.Add(Minimum);

            //Add back face
            result.Add(Minimum + new Vector3(0f, 0f, size.Z));
            result.Add(Minimum + new Vector3(size.X, 0f, size.Z));
            result.Add(Minimum + new Vector3(size.X, 0f, size.Z));
            result.Add(Minimum + new Vector3(size.X, size.Y, size.Z));
            result.Add(Minimum + new Vector3(size.X, size.Y, size.Z));
            result.Add(Minimum + new Vector3(0f, size.Y, size.Z));
            result.Add(Minimum + new Vector3(0f, size.Y, size.Z));
            result.Add(Minimum + new Vector3(0f, 0f, size.Z));

            //Add connections
            result.Add(Minimum);
            result.Add(Minimum + new Vector3(0f, 0f, size.Z));
            result.Add(Minimum + new Vector3(size.X, 0f, 0f));
            result.Add(Minimum + new Vector3(size.X, 0f, size.Z));
            result.Add(Minimum + new Vector3(size.X, size.Y, 0f));
            result.Add(Minimum + new Vector3(size.X, size.Y, size.Z));
            result.Add(Minimum + new Vector3(0f, size.Y, 0f));
            result.Add(Minimum + new Vector3(0f, size.Y, size.Z));

            return result;
        }

        /// <summary>
        /// Is this box empty?
        /// </summary>
        public bool IsEmpty()
        {
            return Minimum.IsEmpty() && Maximum.IsEmpty();
        }

        /// <summary>
        /// Gets the size of this box.
        /// </summary>
        public Vector3 GetSize()
        {
            return Maximum - Minimum;
        }


        /// <summary>
        /// Gets the corner A (lower left front).
        /// </summary>
        public Vector3 CornerA => Minimum;

        /// <summary>
        /// Gets the corner B (lower right front).
        /// </summary>
        public Vector3 CornerB => new Vector3(Maximum.X, Minimum.Y, Minimum.Z);

        /// <summary>
        /// Gets the corner C (lower right back).
        /// </summary>
        public Vector3 CornerC => new Vector3(Maximum.X, Minimum.Y, Maximum.Z);

        /// <summary>
        /// Gets the corner D (lower left back).
        /// </summary>
        public Vector3 CornerD => new Vector3(Minimum.X, Minimum.Y, Maximum.Z);

        /// <summary>
        /// Gets the corner E (upper left front).
        /// </summary>
        public Vector3 CornerE => new Vector3(Minimum.X, Maximum.Y, Minimum.Z);

        /// <summary>
        /// Gets the corner F (upper right front).
        /// </summary>
        public Vector3 CornerF => new Vector3(Maximum.X, Maximum.Y, Minimum.Z);

        /// <summary>
        /// Gets the corner G (upper right back).
        /// </summary>
        public Vector3 CornerG => new Vector3(Maximum.X, Maximum.Y, Maximum.Z);

        /// <summary>
        /// Gets the corner H (upper left back).
        /// </summary>
        public Vector3 CornerH => new Vector3(Minimum.X, Maximum.Y, Maximum.Z);

        public Vector3 LowerA => Minimum;

        public Vector3 LowerB => this.CornerB;

        public Vector3 LowerC => this.CornerC;

        public Vector3 LowerD => this.CornerD;

        public Vector3 UpperA => this.CornerE;

        public Vector3 UpperB => this.CornerF;

        public Vector3 UpperC => this.CornerG;

        public Vector3 UpperD => this.CornerH;

        public float Width => Maximum.X - Minimum.X;

        public float Height => Maximum.Y - Minimum.Y;

        public float Depth => Maximum.Z - Minimum.Z;
    }
}
