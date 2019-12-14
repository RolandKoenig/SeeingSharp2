using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Numerics;

namespace SeeingSharp
{
    /// <summary>
    /// OrientedBoundingBox (OBB) is a rectangular block, much like an AABB (BoundingBox) but with an arbitrary orientation.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct OrientedBoundingBox : IEquatable<OrientedBoundingBox>, IFormattable
    {
        /// <summary>
        /// Half lengths of the box along eaxh axis.
        /// </summary>
        public Vector3 Extents;

        /// <summary>
        /// The matrix which aligns and scales the box, and its tarnslation vector represents the center of the box.
        /// </summary>
        public Matrix4x4 Transformation;

        /// <summary>
        /// Creates an <see cref="SeeingSharp.OrientedBoundingBox"/> from a BoundingBox.
        /// </summary>
        /// <param name="bb">The BoundingBox to create from.</param>
        /// <remarks>
        /// Initially, the OBB is axis-aligned box, but it can be rotated and transformed later.
        /// </remarks>
        public OrientedBoundingBox(BoundingBox bb)
        {
            var Center = bb.Minimum + (bb.Maximum - bb.Minimum) / 2f;
            Extents = bb.Maximum - Center;
            Transformation = Matrix4x4.CreateTranslation(Center);
        }

        /// <summary>
        /// Creates an <see cref="SeeingSharp.OrientedBoundingBox"/> which contained between two minimum and maximum points.
        /// </summary>
        /// <param name="minimum">The minimum vertex of the bounding box.</param>
        /// <param name="maximum">The maximum vertex of the bounding box.</param>
        /// <remarks>
        /// Initially, the OrientedBoundingBox is axis-aligned box, but it can be rotated and transformed later.
        /// </remarks>
        public OrientedBoundingBox(Vector3 minimum, Vector3 maximum)
        {
            var Center = minimum + (maximum - minimum) / 2f;
            Extents = maximum - Center;
            Transformation = Matrix4x4.CreateTranslation(Center);
        }

        /// <summary>
        /// Creates an <see cref="SeeingSharp.OrientedBoundingBox"/> that fully contains the given points.
        /// </summary>
        /// <param name="points">The points that will be contained by the box.</param>
        /// <remarks>
        /// This method is not for computing the best tight-fitting OrientedBoundingBox.
        /// And initially, the OrientedBoundingBox is axis-aligned box, but it can be rotated and transformed later.
        /// </remarks>
        public OrientedBoundingBox(Vector3[] points)
        {
            if (points == null || points.Length == 0)
                throw new ArgumentNullException("points");

            Vector3 minimum = new Vector3(float.MaxValue);
            Vector3 maximum = new Vector3(float.MinValue);

            for (int i = 0; i < points.Length; ++i)
            {
                minimum = Vector3.Min(minimum,points[i]);
                maximum = Vector3.Max(maximum,points[i]);
            }

            var Center = minimum + (maximum - minimum) / 2f;
            Extents = maximum - Center;
            Transformation = Matrix4x4.CreateTranslation(Center);
        }

        /// <summary>
        /// Retrieves the eight corners of the bounding box.
        /// </summary>
        /// <returns>An array of points representing the eight corners of the bounding box.</returns>
        public Vector3[] GetCorners()
        {
            var xv = new Vector3(Extents.X, 0, 0);
            var yv = new Vector3(0, Extents.Y, 0);
            var zv = new Vector3(0, 0, Extents.Z);
            xv = Vector3.TransformNormal(xv,Transformation);
            yv = Vector3.TransformNormal(yv,Transformation);
            zv = Vector3.TransformNormal(zv,Transformation);

            var center = Transformation.Translation;

            var corners = new Vector3[8];
            corners[0] = center + xv + yv + zv;
            corners[1] = center + xv + yv - zv;
            corners[2] = center - xv + yv - zv;
            corners[3] = center - xv + yv + zv;
            corners[4] = center + xv - yv + zv;
            corners[5] = center + xv - yv - zv;
            corners[6] = center - xv - yv - zv;
            corners[7] = center - xv - yv + zv;

            return corners;
        }

        /// <summary>
        /// Transforms this box using a transformation matrix.
        /// </summary>
        /// <param name="mat">The transformation matrix.</param>
        /// <remarks>
        /// While any kind of transformation can be applied, it is recommended to apply scaling using scale method instead, which
        /// scales the Extents and keeps the Transformation matrix for rotation only, and that preserves collision detection accuracy.
        /// </remarks>
        public void Transform(ref Matrix4x4 mat)
        {
            Transformation *= mat;
        }

        /// <summary>
        /// Transforms this box using a transformation matrix.
        /// </summary>
        /// <param name="mat">The transformation matrix.</param>
        /// <remarks>
        /// While any kind of transformation can be applied, it is recommended to apply scaling using scale method instead, which
        /// scales the Extents and keeps the Transformation matrix for rotation only, and that preserves collision detection accuracy.
        /// </remarks>
        public void Transform(Matrix4x4 mat)
        {
            Transformation *= mat;
        }

        /// <summary>
        /// Scales the <see cref="SeeingSharp.OrientedBoundingBox"/> by scaling its Extents without affecting the Transformation matrix,
        /// By keeping Transformation matrix scaling-free, the collision detection methods will be more accurate.
        /// </summary>
        /// <param name="scaling"></param>
        public void Scale(ref Vector3 scaling)
        {
            Extents *= scaling;
        }

        /// <summary>
        /// Scales the <see cref="SeeingSharp.OrientedBoundingBox"/> by scaling its Extents without affecting the Transformation matrix,
        /// By keeping Transformation matrix scaling-free, the collision detection methods will be more accurate.
        /// </summary>
        /// <param name="scaling"></param>
        public void Scale(Vector3 scaling)
        {
            Extents *= scaling;
        }

        /// <summary>
        /// Scales the <see cref="SeeingSharp.OrientedBoundingBox"/> by scaling its Extents without affecting the Transformation matrix,
        /// By keeping Transformation matrix scaling-free, the collision detection methods will be more accurate.
        /// </summary>
        /// <param name="scaling"></param>
        public void Scale(float scaling)
        {
            Extents *= scaling;
        }

        /// <summary>
        /// Translates the <see cref="SeeingSharp.OrientedBoundingBox"/> to a new position using a translation vector;
        /// </summary>
        /// <param name="translation">the translation vector.</param>
        public void Translate(ref Vector3 translation)
        {
            Transformation.Translation = Transformation.Translation + translation;
        }

        /// <summary>
        /// Translates the <see cref="SeeingSharp.OrientedBoundingBox"/> to a new position using a translation vector;
        /// </summary>
        /// <param name="translation">the translation vector.</param>
        public void Translate(Vector3 translation)
        {
            Transformation.Translation = Transformation.Translation + translation;
        }

        /// <summary>
        /// The size of the <see cref="SeeingSharp.OrientedBoundingBox"/> if no scaling is applied to the transformation matrix.
        /// </summary>
        /// <remarks>
        /// The property will return the actual size even if the scaling is applied using Scale method, 
        /// but if the scaling is applied to transformation matrix, use GetSize Function instead.
        /// </remarks>
        public Vector3 Size
        {
            get
            {
                return Extents * 2;
            }
        }

        /// <summary>
        /// Returns the size of the <see cref="SeeingSharp.OrientedBoundingBox"/> taking into consideration the scaling applied to the transformation matrix.
        /// </summary>
        /// <returns>The size of the consideration</returns>
        /// <remarks>
        /// This method is computationally expensive, so if no scale is applied to the transformation matrix
        /// use <see cref="SeeingSharp.OrientedBoundingBox.Size"/> property instead.
        /// </remarks>
        public Vector3 GetSize()
        {
            var xv = new Vector3(Extents.X * 2, 0, 0);
            var yv = new Vector3(0, Extents.Y * 2, 0);
            var zv = new Vector3(0, 0, Extents.Z * 2);
            xv = Vector3.TransformNormal(xv, Transformation);
            yv = Vector3.TransformNormal(yv, Transformation);
            zv = Vector3.TransformNormal(zv, Transformation);

            return new Vector3(xv.Length(), yv.Length(), zv.Length());
        }

        /// <summary>
        /// Returns the square size of the <see cref="SeeingSharp.OrientedBoundingBox"/> taking into consideration the scaling applied to the transformation matrix.
        /// </summary>
        /// <returns>The size of the consideration</returns>
        public Vector3 GetSizeSquared()
        {
            var xv = new Vector3(Extents.X * 2, 0, 0);
            var yv = new Vector3(0, Extents.Y * 2, 0);
            var zv = new Vector3(0, 0, Extents.Z * 2);
            xv = Vector3.TransformNormal(xv, Transformation);
            yv = Vector3.TransformNormal(yv, Transformation);
            zv = Vector3.TransformNormal(zv, Transformation);

            return new Vector3(xv.LengthSquared(), yv.LengthSquared(), zv.LengthSquared());
        }

        /// <summary>
        /// Returns the center of the <see cref="SeeingSharp.OrientedBoundingBox"/>.
        /// </summary>
        public Vector3 Center
        {
            get
            {
                return Transformation.Translation;
            }
        }

        /// <summary>
        /// Determines whether a <see cref="SeeingSharp.OrientedBoundingBox"/> contains a point. 
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public ContainmentType Contains(ref Vector3 point)
        {
            // Transform the point into the obb coordinates
            Matrix4x4 invTrans;
            Matrix4x4.Invert(Transformation, out invTrans);

            Vector3 locPoint = Vector3.Transform(point, invTrans);

            locPoint.X = Math.Abs(locPoint.X);
            locPoint.Y = Math.Abs(locPoint.Y);
            locPoint.Z = Math.Abs(locPoint.Z);

            //Simple axes-aligned BB check
            if (Math.Abs(locPoint.X - Extents.X) < MathUtil.ZeroTolerance &&
                Math.Abs(locPoint.Y - Extents.Y) < MathUtil.ZeroTolerance &&
                Math.Abs(locPoint.Z - Extents.Z) < MathUtil.ZeroTolerance)
                return ContainmentType.Intersects;
            if (locPoint.X < Extents.X && locPoint.Y < Extents.Y && locPoint.Z < Extents.Z)
                return ContainmentType.Contains;
            else
                return ContainmentType.Disjoint;
        }

        /// <summary>
        /// Determines whether a <see cref="SeeingSharp.OrientedBoundingBox"/> contains a point. 
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public ContainmentType Contains(Vector3 point)
        {
            return Contains(ref point);
        }

        /// <summary>
        /// Determines whether a <see cref="SeeingSharp.OrientedBoundingBox"/> contains an array of points>.
        /// </summary>
        /// <param name="points">The points array to test.</param>
        /// <returns>The type of containment.</returns>
        public ContainmentType Contains(Vector3[] points)
        {
            Matrix4x4 invTrans;
            Matrix4x4.Invert(Transformation, out invTrans);

            var containsAll = true;
            var containsAny = false;

            for (int i = 0; i < points.Length; i++)
            {
                Vector3 locPoint = Vector3.Transform(points[i], invTrans);

                locPoint.X = Math.Abs(locPoint.X);
                locPoint.Y = Math.Abs(locPoint.Y);
                locPoint.Z = Math.Abs(locPoint.Z);

                //Simple axes-aligned BB check
                if (Math.Abs(locPoint.X - Extents.X) < MathUtil.ZeroTolerance &&
                    Math.Abs(locPoint.Y - Extents.Y) < MathUtil.ZeroTolerance &&
                    Math.Abs(locPoint.Z - Extents.Z) < MathUtil.ZeroTolerance)
                    containsAny = true;
                if (locPoint.X < Extents.X && locPoint.Y < Extents.Y && locPoint.Z < Extents.Z)
                    containsAny = true;
                else
                    containsAll = false;
            }

            if (containsAll)
                return ContainmentType.Contains;
            else if (containsAny)
                return ContainmentType.Intersects;
            else
                return ContainmentType.Disjoint;
        }

        /// <summary>
        /// Determines whether a <see cref="SeeingSharp.OrientedBoundingBox"/> contains a <see cref="SeeingSharp.BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="IgnoreScale">Optimize the check operation by assuming that <see cref="SeeingSharp.OrientedBoundingBox"/> has no scaling applied</param>
        /// <returns>The type of containment the two objects have.</returns>
        /// <remarks>
        /// This method is not desgined for <see cref="SeeingSharp.OrientedBoundingBox"/> which has a non-uniform scaling appliend to its transformation matrix.
        /// But any type of scaling applied using Scale method will keep this method accurate.
        /// </remarks>
        public ContainmentType Contains(BoundingSphere sphere, bool IgnoreScale = false)
        {
            Matrix4x4 invTrans;
            Matrix4x4.Invert(Transformation, out invTrans);

            // Transform sphere center into the obb coordinates
            Vector3 locCenter = Vector3.Transform(sphere.Center, invTrans);

            float locRadius;
            if (IgnoreScale)
                locRadius = sphere.Radius;
            else
            {
                // Transform sphere radius into the obb coordinates
                Vector3 vRadius = Vector3.TransformNormal(Vector3.UnitX * sphere.Radius, invTrans);
                locRadius = vRadius.Length();
            }

            //Perform regualr BoundingBox to BoundingSphere containment check
            Vector3 minusExtens = -Extents;
            Vector3 vector = Vector3.Clamp(locCenter, minusExtens, Extents);
            float distance = Vector3.DistanceSquared(locCenter, vector);

            if (distance > locRadius * locRadius)
                return ContainmentType.Disjoint;

            if ((((minusExtens.X + locRadius <= locCenter.X) && (locCenter.X <= Extents.X - locRadius)) && ((Extents.X - minusExtens.X > locRadius) &&
                (minusExtens.Y + locRadius <= locCenter.Y))) && (((locCenter.Y <= Extents.Y - locRadius) && (Extents.Y - minusExtens.Y > locRadius)) &&
                (((minusExtens.Z + locRadius <= locCenter.Z) && (locCenter.Z <= Extents.Z - locRadius)) && (Extents.X - minusExtens.X > locRadius))))
            {
                return ContainmentType.Contains;
            }

            return ContainmentType.Intersects;
        }

        private static Vector3[] GetRows(ref Matrix4x4 mat)
        {
            return new Vector3[] {
                new Vector3(mat.M11,mat.M12,mat.M13),
                new Vector3(mat.M21,mat.M22,mat.M23),
                new Vector3(mat.M31,mat.M32,mat.M33)
            };
        }

        /// <summary>
        /// Check the intersection between two <see cref="SeeingSharp.OrientedBoundingBox"/>
        /// </summary>
        /// <param name="obb">The OrientedBoundingBoxs to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        /// <remarks>
        /// For accuracy, The transformation matrix for both <see cref="SeeingSharp.OrientedBoundingBox"/> must not have any scaling applied to it.
        /// Anyway, scaling using Scale method will keep this method accurate.
        /// </remarks>
        public ContainmentType Contains(ref OrientedBoundingBox obb)
        {
            var cornersCheck = Contains(obb.GetCorners());
            if (cornersCheck != ContainmentType.Disjoint)
                return cornersCheck;

            //http://www.3dkingdoms.com/weekly/bbox.cpp
            var SizeA = Extents;
            var SizeB = obb.Extents;
            var RotA = GetRows(ref Transformation);
            var RotB = GetRows(ref obb.Transformation);

            var R = new Matrix4x4();       // Rotation from B to A
            var AR = new Matrix4x4();      // absolute values of R matrix, to use with box extents

            float ExtentA, ExtentB, Separation;
            int i, k;

            // Calculate B to A rotation matrix
            for (i = 0; i < 3; i++)
                for (k = 0; k < 3; k++)
                {
                    Matrix4x4Ex.SetValue(R, i, k, Vector3.Dot(RotA[i], RotB[k]));
                    Matrix4x4Ex.SetValue(AR, i, k, Math.Abs(Matrix4x4Ex.GetValue(R, i, k)));
                }


            // Vector separating the centers of Box B and of Box A	
            var vSepWS = obb.Center - Center;
            // Rotated into Box A's coordinates
            var vSepA = new Vector3(Vector3.Dot(vSepWS, RotA[0]), Vector3.Dot(vSepWS, RotA[1]), Vector3.Dot(vSepWS, RotA[2]));

            // Test if any of A's basis vectors separate the box
            for (i = 0; i < 3; i++)
            {
                ExtentA = Vector3Ex.GetValue(SizeA, i);
                ExtentB = Vector3.Dot(SizeB, new Vector3(
                    Matrix4x4Ex.GetValue(AR, i, 0), 
                    Matrix4x4Ex.GetValue(AR, i, 1), 
                    Matrix4x4Ex.GetValue(AR, i, 2)));
                Separation = Math.Abs(Vector3Ex.GetValue(vSepA, i));

                if (Separation > ExtentA + ExtentB)
                    return ContainmentType.Disjoint;
            }

            // Test if any of B's basis vectors separate the box
            for (k = 0; k < 3; k++)
            {
                ExtentA = Vector3.Dot(SizeA, new Vector3(
                    Matrix4x4Ex.GetValue(AR, 0, k), 
                    Matrix4x4Ex.GetValue(AR, 1, k), 
                    Matrix4x4Ex.GetValue(AR, 2, k)));
                ExtentB = Vector3Ex.GetValue(SizeB, k);
                Separation = Math.Abs(Vector3.Dot(vSepA, new Vector3(
                    Matrix4x4Ex.GetValue(R, 0, k), 
                    Matrix4x4Ex.GetValue(R, 1, k), 
                    Matrix4x4Ex.GetValue(R, 2, k))));

                if (Separation > ExtentA + ExtentB)
                    return ContainmentType.Disjoint;
            }

            // Now test Cross Products of each basis vector combination ( A[i], B[k] )
            for (i = 0; i < 3; i++)
                for (k = 0; k < 3; k++)
                {
                    int i1 = (i + 1) % 3, i2 = (i + 2) % 3;
                    int k1 = (k + 1) % 3, k2 = (k + 2) % 3;
                    ExtentA = 
                        Vector3Ex.GetValue(SizeA, i1) * Matrix4x4Ex.GetValue(AR, i2, k) + 
                        Vector3Ex.GetValue(SizeA, i2) * Matrix4x4Ex.GetValue(AR, i1, k);
                    ExtentB = 
                        Vector3Ex.GetValue(SizeB, k1) * Matrix4x4Ex.GetValue(AR, i, k2) + 
                        Vector3Ex.GetValue(SizeB, k2) * Matrix4x4Ex.GetValue(AR, i, k1);
                    Separation = Math.Abs(
                        Vector3Ex.GetValue(vSepA, i2) * Matrix4x4Ex.GetValue(R, i1, k) - 
                        Vector3Ex.GetValue(vSepA, i1) * Matrix4x4Ex.GetValue(R, i2, k));
                    if (Separation > ExtentA + ExtentB)
                        return ContainmentType.Disjoint;
                }

            // No separating axis found, the boxes overlap	
            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Check the intersection between an <see cref="SeeingSharp.OrientedBoundingBox"/> and a line defined by two points
        /// </summary>
        /// <param name="L1">The first point in the line.</param>
        /// <param name="L2">The second point in the line.</param>
        /// <returns>The type of containment the two objects have.</returns>
        /// <remarks>
        /// For accuracy, The transformation matrix for the <see cref="SeeingSharp.OrientedBoundingBox"/> must not have any scaling applied to it.
        /// Anyway, scaling using Scale method will keep this method accurate.
        /// </remarks>
        public ContainmentType ContainsLine(ref Vector3 L1, ref Vector3 L2)
        {
            var cornersCheck = Contains(new Vector3[] { L1, L2 });
            if (cornersCheck != ContainmentType.Disjoint)
                return cornersCheck;

            //http://www.3dkingdoms.com/weekly/bbox.cpp
            // Put line in box space
            Matrix4x4 invTrans;
            Matrix4x4.Invert(Transformation, out invTrans);

            Vector3 LB1 = Vector3.Transform(L1,invTrans);
            Vector3 LB2 = Vector3.Transform(L1,invTrans);

            // Get line midpoint and extent
            var LMid = (LB1 + LB2) * 0.5f;
            var L = (LB1 - LMid);
            var LExt = new Vector3(Math.Abs(L.X), Math.Abs(L.Y), Math.Abs(L.Z));

            // Use Separating Axis Test
            // Separation vector from box center to line center is LMid, since the line is in box space
            if (Math.Abs(LMid.X) > Extents.X + LExt.X) return ContainmentType.Disjoint;
            if (Math.Abs(LMid.Y) > Extents.Y + LExt.Y) return ContainmentType.Disjoint;
            if (Math.Abs(LMid.Z) > Extents.Z + LExt.Z) return ContainmentType.Disjoint;
            // Crossproducts of line and each axis
            if (Math.Abs(LMid.Y * L.Z - LMid.Z * L.Y) > (Extents.Y * LExt.Z + Extents.Z * LExt.Y)) return ContainmentType.Disjoint;
            if (Math.Abs(LMid.X * L.Z - LMid.Z * L.X) > (Extents.X * LExt.Z + Extents.Z * LExt.X)) return ContainmentType.Disjoint;
            if (Math.Abs(LMid.X * L.Y - LMid.Y * L.X) > (Extents.X * LExt.Y + Extents.Y * LExt.X)) return ContainmentType.Disjoint;
            // No separating axis, the line intersects
            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Check the intersection between an <see cref="SeeingSharp.OrientedBoundingBox"/> and <see cref="SeeingSharp.BoundingBox"/>
        /// </summary>
        /// <param name="box">The BoundingBox to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        /// <remarks>
        /// For accuracy, The transformation matrix for the <see cref="SeeingSharp.OrientedBoundingBox"/> must not have any scaling applied to it.
        /// Anyway, scaling using Scale method will keep this method accurate.
        /// </remarks>
        public ContainmentType Contains(ref BoundingBox box)
        {
            var cornersCheck = Contains(box.GetCorners());
            if (cornersCheck != ContainmentType.Disjoint)
                return cornersCheck;

            var boxCenter = box.Minimum + (box.Maximum - box.Minimum) / 2f;
            var boxExtents = box.Maximum - boxCenter;

            var SizeA = Extents;
            var SizeB = boxExtents;
            var RotA = GetRows(ref Transformation);

            float ExtentA, ExtentB, Separation;
            int i, k;

            Matrix4x4 R;                   // Rotation from B to A
            Matrix4x4.Invert(Transformation, out R);
            var AR = new Matrix4x4();      // absolute values of R matrix, to use with box extents

            for (i = 0; i < 3; i++)
                for (k = 0; k < 3; k++)
                {
                    Matrix4x4Ex.SetValue(AR, i, k, Math.Abs(Matrix4x4Ex.GetValue(R, i, k)));
                }


            // Vector separating the centers of Box B and of Box A	
            var vSepWS = boxCenter - Center;
            // Rotated into Box A's coordinates
            var vSepA = new Vector3(Vector3.Dot(vSepWS, RotA[0]), Vector3.Dot(vSepWS, RotA[1]), Vector3.Dot(vSepWS, RotA[2]));

            // Test if any of A's basis vectors separate the box
            for (i = 0; i < 3; i++)
            {
                ExtentA = Vector3Ex.GetValue(SizeA, i);
                ExtentB = Vector3.Dot(SizeB, new Vector3(
                    Matrix4x4Ex.GetValue(AR, i, 0), 
                    Matrix4x4Ex.GetValue(AR, i, 1), 
                    Matrix4x4Ex.GetValue(AR, i, 2)));
                Separation = Math.Abs(Vector3Ex.GetValue(vSepA, i));

                if (Separation > ExtentA + ExtentB)
                    return ContainmentType.Disjoint;
            }

            // Test if any of B's basis vectors separate the box
            for (k = 0; k < 3; k++)
            {
                ExtentA = Vector3.Dot(SizeA, new Vector3(
                    Matrix4x4Ex.GetValue(AR, 0, k), 
                    Matrix4x4Ex.GetValue(AR, 1, k), 
                    Matrix4x4Ex.GetValue(AR, 2, k)));
                ExtentB = Vector3Ex.GetValue(SizeB, k);
                Separation = Math.Abs(Vector3.Dot(vSepA, new Vector3(
                    Matrix4x4Ex.GetValue(R, 0, k), 
                    Matrix4x4Ex.GetValue(R, 1, k), 
                    Matrix4x4Ex.GetValue(R, 2, k))));

                if (Separation > ExtentA + ExtentB)
                    return ContainmentType.Disjoint;
            }

            // Now test Cross Products of each basis vector combination ( A[i], B[k] )
            for (i = 0; i < 3; i++)
                for (k = 0; k < 3; k++)
                {
                    int i1 = (i + 1) % 3, i2 = (i + 2) % 3;
                    int k1 = (k + 1) % 3, k2 = (k + 2) % 3;
                    ExtentA = 
                        Vector3Ex.GetValue(SizeA, i1) * Matrix4x4Ex.GetValue(AR, i2, k) + 
                        Vector3Ex.GetValue(SizeA, i2) * Matrix4x4Ex.GetValue(AR, i1, k);
                    ExtentB = 
                        Vector3Ex.GetValue(SizeB, k1) * Matrix4x4Ex.GetValue(AR, i, k2) + 
                        Vector3Ex.GetValue(SizeB, k2) * Matrix4x4Ex.GetValue(AR, i, k1);
                    Separation = Math.Abs(
                        Vector3Ex.GetValue(vSepA, i2) * Matrix4x4Ex.GetValue(R, i1, k) - 
                        Vector3Ex.GetValue(vSepA, i1) * Matrix4x4Ex.GetValue(R, i2, k));
                    if (Separation > ExtentA + ExtentB)
                        return ContainmentType.Disjoint;
                }

            // No separating axis found, the boxes overlap	
            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="SeeingSharp.Ray"/> and a <see cref="SeeingSharp.OrientedBoundingBox"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="System.Numerics.Vector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref Ray ray, out Vector3 point)
        {
            // Put ray in box space
            Matrix4x4 invTrans;
            Matrix4x4.Invert(Transformation, out invTrans);

            Ray bRay;
            bRay.Direction = Vector3.TransformNormal(ray.Direction, invTrans);
            bRay.Position = Vector3.Transform(ray.Position, invTrans);

            //Perform a regualr ray to BoundingBox check
            var bb = new BoundingBox(-Extents, Extents);
            var intersects = Collision.RayIntersectsBox(ref bRay, ref bb, out point);

            //Put the result intersection back to world
            if (intersects)
            {
                point = Vector3.Transform(point, Transformation);
            }

            return intersects;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="SeeingSharp.Ray"/> and a <see cref="SeeingSharp.OrientedBoundingBox"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref Ray ray)
        {
            Vector3 point;
            return Intersects(ref ray, out point);
        }

        private Vector3[] GetLocalCorners()
        {
            var xv = new Vector3(Extents.X, 0, 0);
            var yv = new Vector3(0, Extents.Y, 0);
            var zv = new Vector3(0, 0, Extents.Z);

            var corners = new Vector3[8];
            corners[0] = xv + yv + zv;
            corners[1] = xv + yv - zv;
            corners[2] = -xv + yv - zv;
            corners[3] = -xv + yv + zv;
            corners[4] = xv - yv + zv;
            corners[5] = xv - yv - zv;
            corners[6] = -xv - yv - zv;
            corners[7] = -xv - yv + zv;

            return corners;
        }

        /// <summary>
        /// Get the axis-aligned <see cref="SeeingSharp.BoundingBox"/> which contains all <see cref="SeeingSharp.OrientedBoundingBox"/> corners.
        /// </summary>
        /// <returns>The axis-aligned BoundingBox of this OrientedBoundingBox.</returns>
        public BoundingBox GetBoundingBox()
        {
            return BoundingBox.FromPoints(GetCorners());
        }

        /// <summary>
        /// Caculates the matrix required to transfer any point from one <see cref="SeeingSharp.OrientedBoundingBox"/> local corrdinates to another.
        /// </summary>
        /// <param name="A">The source OrientedBoundingBox.</param>
        /// <param name="B">The target OrientedBoundingBox.</param>
        /// <param name="NoMatrixScaleApplied">
        /// If true, the method will use a fast algorithm which is inapplicable if a scale is applied to the transformation matrix of the OrientedBoundingBox.
        /// </param>
        /// <returns></returns>
        public static Matrix4x4 GetBoxToBoxMatrix(ref OrientedBoundingBox A, ref OrientedBoundingBox B, bool NoMatrixScaleApplied = false)
        {
            Matrix4x4 AtoB_Matrix;

            // Calculate B to A transformation matrix
            if (NoMatrixScaleApplied)
            {
                var RotA = GetRows(ref A.Transformation);
                var RotB = GetRows(ref B.Transformation);
                AtoB_Matrix = new Matrix4x4();
                int i, k;
                for (i = 0; i < 3; i++)
                {
                    for (k = 0; k < 3; k++)
                    {
                        Matrix4x4Ex.SetValue(AtoB_Matrix, i, k, Vector3.Dot(RotB[i], RotA[k]));
                    }
                }

                var v = B.Center - A.Center;
                AtoB_Matrix.M41 = Vector3.Dot(v, RotA[0]);
                AtoB_Matrix.M42 = Vector3.Dot(v, RotA[1]);
                AtoB_Matrix.M43 = Vector3.Dot(v, RotA[2]);
                AtoB_Matrix.M44 = 1;
            }
            else
            {
                Matrix4x4 AInvMat;
                Matrix4x4.Invert(A.Transformation, out AInvMat);
                AtoB_Matrix = B.Transformation * AInvMat;
            }

            return AtoB_Matrix;
        }

        /// <summary>
        /// Merge an OrientedBoundingBox B into another OrientedBoundingBox A, by expanding A to contain B and keeping A orientation.
        /// </summary>
        /// <param name="A">The <see cref="SeeingSharp.OrientedBoundingBox"/> to merge into it.</param>
        /// <param name="B">The <see cref="SeeingSharp.OrientedBoundingBox"/> to be merged</param>
        /// <param name="NoMatrixScaleApplied">
        /// If true, the method will use a fast algorithm which is inapplicable if a scale is applied to the transformation matrix of the OrientedBoundingBox.
        /// </param>
        /// <remarks>
        /// Unlike merging axis aligned boxes, The operation is not interchangeable, because it keeps A orientation and merge B into it.
        /// </remarks>
        public static void Merge(ref OrientedBoundingBox A, ref OrientedBoundingBox B, bool NoMatrixScaleApplied = false)
        {
            Matrix4x4 AtoB_Matrix = GetBoxToBoxMatrix(ref A, ref B, NoMatrixScaleApplied);

            //Get B corners in A Space
            var bCorners = B.GetLocalCorners();
            for (int loop = 0; loop < bCorners.Length; loop++)
            {
                bCorners[loop] = Vector3.Transform(bCorners[loop], AtoB_Matrix);
            }

            //Get A local Bounding Box
            var A_LocalBB = new BoundingBox(-A.Extents, A.Extents);

            //Find B BoundingBox in A Space
            var B_LocalBB = BoundingBox.FromPoints(bCorners);

            //Merger A and B local Bounding Boxes
            BoundingBox mergedBB;
            BoundingBox.Merge(ref B_LocalBB, ref A_LocalBB, out mergedBB);

            //Find the new Extents and Center, Transform Center back to world
            var newCenter = mergedBB.Minimum + (mergedBB.Maximum - mergedBB.Minimum) / 2f;
            A.Extents = mergedBB.Maximum - newCenter;

            newCenter = Vector3.Transform(newCenter, A.Transformation);
            A.Transformation.Translation = newCenter;
        }

        /// <summary>
        /// Merge this OrientedBoundingBox into another OrientedBoundingBox, keeping the other OrientedBoundingBox orientation.
        /// </summary>
        /// <param name="OBB">The other <see cref="SeeingSharp.OrientedBoundingBox"/> to merge into.</param>
        /// <param name="NoMatrixScaleApplied">
        /// If true, the method will use a fast algorithm which is inapplicable if a scale is applied to the transformation matrix of the OrientedBoundingBox.
        /// </param>
        public void MergeInto(ref OrientedBoundingBox OBB, bool NoMatrixScaleApplied = false)
        {
            Merge(ref OBB, ref this, NoMatrixScaleApplied);
        }

        /// <summary>
        /// Merge another OrientedBoundingBox into this OrientedBoundingBox.
        /// </summary>
        /// <param name="OBB">The other <see cref="SeeingSharp.OrientedBoundingBox"/> to merge into this OrientedBoundingBox.</param>
        /// <param name="NoMatrixScaleApplied">
        /// If true, the method will use a fast algorithm which is inapplicable if a scale is applied to the transformation matrix of the OrientedBoundingBox.
        /// </param>
        public void Add(ref OrientedBoundingBox OBB, bool NoMatrixScaleApplied = false)
        {
            Merge(ref this, ref OBB, NoMatrixScaleApplied);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Numerics.Vector4"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="System.Numerics.Vector4"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Numerics.Vector4"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(OrientedBoundingBox value)
        {
            return Extents == value.Extents && Transformation == value.Transformation;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (value == null)
                return false;

            if (!ReferenceEquals(value.GetType(), typeof(OrientedBoundingBox)))
                return false;

            return Equals((OrientedBoundingBox)value);
        }

        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator ==(OrientedBoundingBox left, OrientedBoundingBox right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Tests for inequality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator !=(OrientedBoundingBox left, OrientedBoundingBox right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return Extents.GetHashCode() + Transformation.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return String.Format(CultureInfo.CurrentCulture, "Center: {0}, Extents: {1}", Center, Extents);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "Center: {0}, Extents: {1}", Center.ToString(format, CultureInfo.CurrentCulture),
                Extents.ToString(format, CultureInfo.CurrentCulture));
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "Center: {0}, Extents: {1}", Center.ToString(), Extents.ToString());
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, "Center: {0}, Extents: {1}", Center.ToString(format, formatProvider),
                Extents.ToString(format, formatProvider));
        }
    }
}

