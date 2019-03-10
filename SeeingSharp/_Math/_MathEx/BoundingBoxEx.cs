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
using SharpDX;

namespace SeeingSharp
{
    public static class BoundingBoxEx
    {
        public static BoundingBox Create(IEnumerable<Vector3> containedLocations)
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

            return new BoundingBox(minimum, maximum);
        }

        public static void Transform(this ref BoundingBox bBox, Matrix matrix)
        {
            var corners = bBox.GetCorners();
            for (var loop = 0; loop < corners.Length; loop++)
            {
                corners[loop] = Vector3.Transform(corners[loop], matrix).ToXYZ();
            }

            bBox.Redefine(corners);
        }

        /// <summary>
        /// Redefines this bounding box based on given points.
        /// </summary>
        public static void Redefine(this ref BoundingBox boundingBox, Vector3[] points)
        {
            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);

            for (var i = 0; i < points.Length; ++i)
            {
                min = Vector3.Min(min, points[i]);
                max = Vector3.Max(max, points[i]);
            }

            boundingBox.Minimum = min;
            boundingBox.Maximum = max;
        }

        /// <summary>
        /// Expands this AxisAlignedBox so that it contains the given location.
        /// </summary>
        public static void MergeWith(this ref BoundingBox boundingBox, Vector3 newLocation)
        {
            //Handle x axis
            if (newLocation.X < boundingBox.Minimum.X) { boundingBox.Minimum.X = newLocation.X; }
            else if (newLocation.X > boundingBox.Maximum.X) { boundingBox.Maximum.X = newLocation.X; }

            //Handle y axis
            if (newLocation.Y < boundingBox.Minimum.Y) { boundingBox.Minimum.Y = newLocation.Y; }
            else if (newLocation.Y > boundingBox.Maximum.Y) { boundingBox.Maximum.Y = newLocation.Y; }

            //Handle z axis
            if (newLocation.Z < boundingBox.Minimum.Z) { boundingBox.Minimum.Z = newLocation.Z; }
            else if (newLocation.Z > boundingBox.Maximum.Z) { boundingBox.Maximum.Z = newLocation.Z; }
        }

        /// <summary>
        /// Merges this box with the given one
        /// </summary>
        public static void MergeWith(this ref BoundingBox boundingBox, BoundingBox other)
        {
            var minimum1 = boundingBox.Minimum;
            var minimum2 = other.Minimum;
            var maximum1 = boundingBox.Maximum;
            var maximum2 = other.Maximum;

            var newMinimum = Vector3.Min(minimum1, minimum2);
            var newMaximum = Vector3.Max(maximum1, maximum2);

            boundingBox.Minimum = newMinimum;
            boundingBox.Maximum = newMaximum;
        }

        /// <summary>
        /// Gets the coordinate of middle of bottom-left border.
        /// </summary>
        public static Vector3 GetBottomLeftMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            var result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.Y = boundingBox.Minimum.Y;
            result.X = boundingBox.Minimum.X;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of bottom-right border.
        /// </summary>
        public static Vector3 GetBottomRightMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            var result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.Y = boundingBox.Minimum.Y;
            result.X = boundingBox.Maximum.X;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of bottom-front border.
        /// </summary>
        public static Vector3 GetBottomFrontMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            var result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.Y = boundingBox.Minimum.Y;
            result.Z = boundingBox.Minimum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of bottom-back border.
        /// </summary>
        public static Vector3 GetBottomBackMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            var result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.Y = boundingBox.Minimum.Y;
            result.Z = boundingBox.Maximum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of the middle of the box.
        /// </summary>
        public static Vector3 GetMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            return boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
        }

        /// <summary>
        /// Gets the coordinate of middle of top rectangle.
        /// </summary>
        public static Vector3 GetTopMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            var result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.Y = boundingBox.Maximum.Y;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of top-left border.
        /// </summary>
        public static Vector3 GetTopLeftMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            var result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.Y = boundingBox.Maximum.Y;
            result.X = boundingBox.Minimum.X;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of top-right border.
        /// </summary>
        public static Vector3 GetTopRightMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            var result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.Y = boundingBox.Maximum.Y;
            result.X = boundingBox.Maximum.X;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of top-front border.
        /// </summary>
        public static Vector3 GetTopFrontMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            var result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.Y = boundingBox.Maximum.Y;
            result.Z = boundingBox.Minimum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of top-back border.
        /// </summary>
        public static Vector3 GetTopBackMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            var result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.Y = boundingBox.Maximum.Y;
            result.Z = boundingBox.Maximum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of front rectangle.
        /// </summary>
        public static Vector3 GetFrontMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            var result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.Z = boundingBox.Minimum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of back rectangle.
        /// </summary>
        public static Vector3 GetBackMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            var result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.Z = boundingBox.Maximum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of left rectangle.
        /// </summary>
        public static Vector3 GetLeftMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            var result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.X = boundingBox.Minimum.X;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of left-front border.
        /// </summary>
        public static Vector3 GetLeftFrontMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            var result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.X = boundingBox.Minimum.X;
            result.Z = boundingBox.Minimum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of left-back border.
        /// </summary>
        public static Vector3 GetLeftBackMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            var result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.X = boundingBox.Minimum.X;
            result.Z = boundingBox.Maximum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of right rectangle.
        /// </summary>
        public static Vector3 GetRightMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            var result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.X = boundingBox.Maximum.X;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of right-front border.
        /// </summary>
        public static Vector3 GetRightFrontMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            var result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.X = boundingBox.Maximum.X;
            result.Z = boundingBox.Minimum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of right-back border.
        /// </summary>
        public static Vector3 GetRightBackMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            var result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.X = boundingBox.Maximum.X;
            result.Z = boundingBox.Maximum.Z;
            return result;
        }

        /// <summary>
        /// Gets the middle center of the box.
        /// </summary>
        public static Vector3 GetMiddleCenter(this ref BoundingBox boundingBox)
        {
            var size = boundingBox.Size;
            return new Vector3(
                boundingBox.Minimum.X + size.X / 2f,
                boundingBox.Minimum.Y + size.Y / 2f,
                boundingBox.Minimum.Z + size.Z / 2f);
        }

        /// <summary>
        /// Gets the bottom center of the box.
        /// </summary>
        public static Vector3 GetBottomCenter(this ref BoundingBox boundingBox)
        {
            var size = boundingBox.Size;
            return new Vector3(
                boundingBox.Minimum.X + size.X / 2f,
                boundingBox.Minimum.Y,
                boundingBox.Minimum.Z + size.Z / 2f);
        }

        /// <summary>
        /// Builds a line list containing lines for all borders of this box.
        /// </summary>
        public static List<Vector3> BuildLineListForBorders(this ref BoundingBox boundingBox)
        {
            var result = new List<Vector3>();
            var size = boundingBox.Size;

            //Add front face
            result.Add(boundingBox.Minimum);
            result.Add(boundingBox.Minimum + new Vector3(size.X, 0f, 0f));
            result.Add(boundingBox.Minimum + new Vector3(size.X, 0f, 0f));
            result.Add(boundingBox.Minimum + new Vector3(size.X, size.Y, 0f));
            result.Add(boundingBox.Minimum + new Vector3(size.X, size.Y, 0f));
            result.Add(boundingBox.Minimum + new Vector3(0f, size.Y, 0f));
            result.Add(boundingBox.Minimum + new Vector3(0f, size.Y, 0f));
            result.Add(boundingBox.Minimum);

            //Add back face
            result.Add(boundingBox.Minimum + new Vector3(0f, 0f, size.Z));
            result.Add(boundingBox.Minimum + new Vector3(size.X, 0f, size.Z));
            result.Add(boundingBox.Minimum + new Vector3(size.X, 0f, size.Z));
            result.Add(boundingBox.Minimum + new Vector3(size.X, size.Y, size.Z));
            result.Add(boundingBox.Minimum + new Vector3(size.X, size.Y, size.Z));
            result.Add(boundingBox.Minimum + new Vector3(0f, size.Y, size.Z));
            result.Add(boundingBox.Minimum + new Vector3(0f, size.Y, size.Z));
            result.Add(boundingBox.Minimum + new Vector3(0f, 0f, size.Z));

            //Add connections
            result.Add(boundingBox.Minimum);
            result.Add(boundingBox.Minimum + new Vector3(0f, 0f, size.Z));
            result.Add(boundingBox.Minimum + new Vector3(size.X, 0f, 0f));
            result.Add(boundingBox.Minimum + new Vector3(size.X, 0f, size.Z));
            result.Add(boundingBox.Minimum + new Vector3(size.X, size.Y, 0f));
            result.Add(boundingBox.Minimum + new Vector3(size.X, size.Y, size.Z));
            result.Add(boundingBox.Minimum + new Vector3(0f, size.Y, 0f));
            result.Add(boundingBox.Minimum + new Vector3(0f, size.Y, size.Z));

            return result;
        }

        /// <summary>
        /// Is this box empty?
        /// </summary>
        public static bool IsEmpty(this ref BoundingBox boundingBox)
        {
            return boundingBox.Minimum.IsEmpty() && boundingBox.Maximum.IsEmpty();
        }

        /// <summary>
        /// Gets the corner A (lower left front).
        /// </summary>
        public static Vector3 GetCornerA(this ref BoundingBox boundingBox)
        {
            return boundingBox.Minimum;
        }

        /// <summary>
        /// Gets the corner B (lower right front).
        /// </summary>
        public static Vector3 GetCornerB(this ref BoundingBox boundingBox)
        {
            return new Vector3(boundingBox.Maximum.X, boundingBox.Minimum.Y, boundingBox.Minimum.Z);
        }

        /// <summary>
        /// Gets the corner C (lower right back).
        /// </summary>
        public static Vector3 GetCornerC(this ref BoundingBox boundingBox)
        {
            return new Vector3(boundingBox.Maximum.X, boundingBox.Minimum.Y, boundingBox.Maximum.Z);
        }

        /// <summary>
        /// Gets the corner D (lower left back).
        /// </summary>
        public static Vector3 GetCornerD(this ref BoundingBox boundingBox)
        {
            return new Vector3(boundingBox.Minimum.X, boundingBox.Minimum.Y, boundingBox.Maximum.Z);
        }

        /// <summary>
        /// Gets the corner E (upper left front).
        /// </summary>
        public static Vector3 GetCornerE(this ref BoundingBox boundingBox)
        {
            return new Vector3(boundingBox.Minimum.X, boundingBox.Maximum.Y, boundingBox.Minimum.Z);
        }

        /// <summary>
        /// Gets the corner F (upper right front).
        /// </summary>
        public static Vector3 GetCornerF(this ref BoundingBox boundingBox)
        {
            return new Vector3(boundingBox.Maximum.X, boundingBox.Maximum.Y, boundingBox.Minimum.Z);
        }

        /// <summary>
        /// Gets the corner G (upper right back).
        /// </summary>
        public static Vector3 GetCornerG(this ref BoundingBox boundingBox)
        {
            return new Vector3(boundingBox.Maximum.X, boundingBox.Maximum.Y, boundingBox.Maximum.Z);
        }

        /// <summary>
        /// Gets the corner H (upper left back).
        /// </summary>
        public static Vector3 GetCornerH(this ref BoundingBox boundingBox)
        {
            return new Vector3(boundingBox.Minimum.X, boundingBox.Maximum.Y, boundingBox.Maximum.Z);
        }

        public static Vector3 GetLowerA(this ref BoundingBox boundingBox)
        {
            return boundingBox.Minimum;
        }

        public static Vector3 GetLowerB(this ref BoundingBox boundingBox)
        {
            return boundingBox.GetCornerB();
        }

        public static Vector3 GetLowerC(this ref BoundingBox boundingBox)
        {
            return boundingBox.GetCornerC();
        }

        public static Vector3 GetLowerD(this ref BoundingBox boundingBox)
        {
            return boundingBox.GetCornerD();
        }

        public static Vector3 GetUpperA(this ref BoundingBox boundingBox)
        {
            return boundingBox.GetCornerE();
        }

        public static Vector3 GetUpperB(this ref BoundingBox boundingBox)
        {
            return boundingBox.GetCornerF();
        }

        public static Vector3 GetUpperC(this ref BoundingBox boundingBox)
        {
            return boundingBox.GetCornerG();
        }

        public static Vector3 GetUpperD(this ref BoundingBox boundingBox)
        {
            return boundingBox.GetCornerH();
        }
    }
}