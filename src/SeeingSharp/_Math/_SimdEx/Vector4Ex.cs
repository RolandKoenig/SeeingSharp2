using System;
using System.Numerics;

namespace SeeingSharp
{
    public static class Vector4Ex
    {
        public static bool EqualsWithTolerance(Vector4 left, Vector4 right, float tolerance = EngineMath.TOLERANCE_FLOAT_POSITIVE)
        {
            return
                EngineMath.EqualsWithTolerance(left.X, right.X, tolerance) &&
                EngineMath.EqualsWithTolerance(left.Y, right.Y, tolerance) &&
                EngineMath.EqualsWithTolerance(left.Z, right.Z, tolerance) &&
                EngineMath.EqualsWithTolerance(left.W, right.W, tolerance);
        }

        public static float GetValue(Vector4 vector, int index)
        {
            switch (index)
            {
                case 1: return vector.X;
                case 2: return vector.Y;
                case 3: return vector.Z;
                case 4: return vector.W;
                default: throw new ArgumentException("Invalid index!");
            }
        }

        public static void SetValue(Vector4 vector, int index, float value)
        {
            switch (index)
            {
                case 1: vector.X = value; break;
                case 2: vector.Y = value; break;
                case 3: vector.Z = value; break;
                case 4: vector.W = value; break;
                default: throw new ArgumentException("Invalid index!");
            }
        }
    }
}
