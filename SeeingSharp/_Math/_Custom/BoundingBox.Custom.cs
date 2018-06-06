#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace SeeingSharp
{
    public partial struct BoundingBox
    {
        public static BoundingBox Empty = new BoundingBox();

        public BoundingBox(IEnumerable<Vector3> containedLocations)
        {
            Vector3 minimum = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 maximum = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            bool anyInteration = false;
            foreach(Vector3 actContainedLocation in containedLocations)
            {
                anyInteration = true;

                if (minimum.X > actContainedLocation.X) { minimum.X = actContainedLocation.X; }
                if (minimum.Y > actContainedLocation.Y) { minimum.Y = actContainedLocation.Y; }
                if (minimum.Z > actContainedLocation.Z) { minimum.Z = actContainedLocation.Z; }

                if (maximum.X < actContainedLocation.X) { maximum.X = actContainedLocation.X; }
                if (maximum.Y < actContainedLocation.Y) { maximum.Y = actContainedLocation.Y; }
                if (maximum.Z < actContainedLocation.Z) { maximum.Z = actContainedLocation.Z; }
            }

            if (!anyInteration) { throw new SeeingSharpException("No vectors given!"); }

            this.Minimum = minimum;
            this.Maximum = maximum;
        }

        public void Transform(Matrix4x4 matrix)
        {
            Vector3[] corners = this.GetCorners();
            for (int loop = 0; loop < corners.Length; loop++)
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
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);
            for (int i = 0; i < points.Length; ++i)
            {
                min = Vector3.Min(min, points[i]);
                max = Vector3.Max(max, points[i]);
            }

            this.Minimum = min;
            this.Maximum = max;
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
            Vector3 minimum1 = this.Minimum;
            Vector3 minimum2 = other.Minimum;
            Vector3 maximum1 = this.Maximum;
            Vector3 maximum2 = other.Maximum;

            Vector3 newMinimum = Vector3.Min(minimum1, minimum2);
            Vector3 newMaximum = Vector3.Max(maximum1, maximum2);

            this.Minimum = newMinimum;
            this.Maximum = newMaximum;
        }

        /// <summary>
        /// Gets the coordinate of middle of bottom-left border.
        /// </summary>
        public Vector3 GetBottomLeftMiddleCoordinate()
        {
            Vector3 result = Minimum + (Maximum - Minimum) / 2f;
            result.Y = Minimum.Y;
            result.X = Minimum.X;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of bottom-right border.
        /// </summary>
        public Vector3 GetBottomRightMiddleCoordinate()
        {
            Vector3 result = Minimum + (Maximum - Minimum) / 2f;
            result.Y = Minimum.Y;
            result.X = Maximum.X;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of bottom-front border.
        /// </summary>
        public Vector3 GetBottomFrontMiddleCoordinate()
        {
            Vector3 result = Minimum + (Maximum - Minimum) / 2f;
            result.Y = Minimum.Y;
            result.Z = Minimum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of bottom-back border.
        /// </summary>
        public Vector3 GetBottomBackMiddleCoordinate()
        {
            Vector3 result = Minimum + (Maximum - Minimum) / 2f;
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
            Vector3 result = Minimum + (Maximum - Minimum) / 2f;
            result.Y = Maximum.Y;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of top-left border.
        /// </summary>
        public Vector3 GetTopLeftMiddleCoordinate()
        {
            Vector3 result = Minimum + (Maximum - Minimum) / 2f;
            result.Y = Maximum.Y;
            result.X = Minimum.X;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of top-right border.
        /// </summary>
        public Vector3 GetTopRightMiddleCoordinate()
        {
            Vector3 result = Minimum + (Maximum - Minimum) / 2f;
            result.Y = Maximum.Y;
            result.X = Maximum.X;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of top-front border.
        /// </summary>
        public Vector3 GetTopFrontMiddleCoordinate()
        {
            Vector3 result = Minimum + (Maximum - Minimum) / 2f;
            result.Y = Maximum.Y;
            result.Z = Minimum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of top-back border.
        /// </summary>
        public Vector3 GetTopBackMiddleCoordinate()
        {
            Vector3 result = Minimum + (Maximum - Minimum) / 2f;
            result.Y = Maximum.Y;
            result.Z = Maximum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of front rectangle.
        /// </summary>
        public Vector3 GetFrontMiddleCoordinate()
        {
            Vector3 result = Minimum + (Maximum - Minimum) / 2f;
            result.Z = Minimum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of back rectangle.
        /// </summary>
        public Vector3 GetBackMiddleCoordinate()
        {
            Vector3 result = Minimum + (Maximum - Minimum) / 2f;
            result.Z = Maximum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of left rectangle.
        /// </summary>
        public Vector3 GetLeftMiddleCoordinate()
        {
            Vector3 result = Minimum + (Maximum - Minimum) / 2f;
            result.X = Minimum.X;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of left-front border.
        /// </summary>
        public Vector3 GetLeftFrontMiddleCoordinate()
        {
            Vector3 result = Minimum + (Maximum - Minimum) / 2f;
            result.X = Minimum.X;
            result.Z = Minimum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of left-back border.
        /// </summary>
        public Vector3 GetLeftBackMiddleCoordinate()
        {
            Vector3 result = Minimum + (Maximum - Minimum) / 2f;
            result.X = Minimum.X;
            result.Z = Maximum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of right rectangle.
        /// </summary>
        public Vector3 GetRightMiddleCoordinate()
        {
            Vector3 result = Minimum + (Maximum - Minimum) / 2f;
            result.X = Maximum.X;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of right-front border.
        /// </summary>
        public Vector3 GetRightFrontMiddleCoordinate()
        {
            Vector3 result = Minimum + (Maximum - Minimum) / 2f;
            result.X = Maximum.X;
            result.Z = Minimum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of right-back border.
        /// </summary>
        public Vector3 GetRightBackMiddleCoordinate()
        {
            Vector3 result = Minimum + (Maximum - Minimum) / 2f;
            result.X = Maximum.X;
            result.Z = Maximum.Z;
            return result;
        }

        /// <summary>
        /// Gets the middle center of the box.
        /// </summary>
        public Vector3 GetMiddleCenter()
        {
            Vector3 size = this.GetSize();
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
            Vector3 size = this.GetSize();
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
            List<Vector3> result = new List<Vector3>();
            Vector3 size = this.GetSize();

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
        public Vector3 CornerA
        {
            get { return Minimum; }
        }

        /// <summary>
        /// Gets the corner B (lower right front).
        /// </summary>
        public Vector3 CornerB
        {
            get { return new Vector3(Maximum.X, Minimum.Y, Minimum.Z); }
        }

        /// <summary>
        /// Gets the corner C (lower right back).
        /// </summary>
        public Vector3 CornerC
        {
            get { return new Vector3(Maximum.X, Minimum.Y, Maximum.Z); }
        }

        /// <summary>
        /// Gets the corner D (lower left back).
        /// </summary>
        public Vector3 CornerD
        {
            get { return new Vector3(Minimum.X, Minimum.Y, Maximum.Z); }
        }

        /// <summary>
        /// Gets the corner E (upper left front).
        /// </summary>
        public Vector3 CornerE
        {
            get { return new Vector3(Minimum.X, Maximum.Y, Minimum.Z); }
        }

        /// <summary>
        /// Gets the corner F (upper right front).
        /// </summary>
        public Vector3 CornerF
        {
            get { return new Vector3(Maximum.X, Maximum.Y, Minimum.Z); }
        }

        /// <summary>
        /// Gets the corner G (upper right back).
        /// </summary>
        public Vector3 CornerG
        {
            get { return new Vector3(Maximum.X, Maximum.Y, Maximum.Z); }
        }

        /// <summary>
        /// Gets the corner H (upper left back).
        /// </summary>
        public Vector3 CornerH
        {
            get { return new Vector3(Minimum.X, Maximum.Y, Maximum.Z); }
        }

        /// <summary>
        ///
        /// </summary>
        public Vector3 LowerA
        {
            get { return this.Minimum; }
        }

        /// <summary>
        ///
        /// </summary>
        public Vector3 LowerB
        {
            get { return this.CornerB; }
        }

        /// <summary>
        ///
        /// </summary>
        public Vector3 LowerC
        {
            get { return this.CornerC; }
        }

        /// <summary>
        ///
        /// </summary>
        public Vector3 LowerD
        {
            get { return this.CornerD; }
        }

        /// <summary>
        ///
        /// </summary>
        public Vector3 UpperA
        {
            get { return this.CornerE; }
        }

        /// <summary>
        ///
        /// </summary>
        public Vector3 UpperB
        {
            get { return this.CornerF; }
        }

        /// <summary>
        ///
        /// </summary>
        public Vector3 UpperC
        {
            get { return this.CornerG; }
        }

        /// <summary>
        ///
        /// </summary>
        public Vector3 UpperD
        {
            get { return this.CornerH; }
        }

        public float Width
        {
            get { return this.Maximum.X - this.Minimum.X; }
        }

        public float Height
        {
            get { return this.Maximum.Y - this.Minimum.Y; }
        }

        public float Depth
        {
            get { return this.Maximum.Y - this.Minimum.Y; }
        }
    }
}
