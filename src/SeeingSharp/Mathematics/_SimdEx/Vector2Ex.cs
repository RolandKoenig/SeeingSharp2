using System;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace SeeingSharp.Mathematics
{
    public static class Vector2Ex
    {
        public static bool EqualsWithTolerance(Vector2 left, Vector2 right, float tolerance = EngineMath.TOLERANCE_FLOAT_POSITIVE)
        {
            return
                EngineMath.EqualsWithTolerance(left.X, right.X, tolerance) &&
                EngineMath.EqualsWithTolerance(left.Y, right.Y, tolerance);
        }

        public static Vector2 FromSize2(Size size)
        {
            return new Vector2(size.Width, size.Height);
        }

        public static Vector2 FromSize2(SizeF size)
        {
            return new Vector2(size.Width, size.Height);
        }

        public static float GetValue(Vector2 vector, int index)
        {
            return index switch
            {
                1 => vector.X,
                2 => vector.Y,
                _ => throw new ArgumentException("Invalid index!")
            };
        }

        public static void SetValue(Vector2 vector, int index, float value)
        {
            switch (index)
            {
                case 1: vector.X = value; break;
                case 2: vector.Y = value; break;
                default: throw new ArgumentException("Invalid index!");
            }
        }

        public static bool IsEmpty(ref Vector2 vector)
        {
            return vector.Equals(Vector2.Zero);
        }
    }
}
