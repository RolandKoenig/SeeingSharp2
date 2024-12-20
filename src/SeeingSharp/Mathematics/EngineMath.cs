using System;

namespace SeeingSharp.Mathematics
{
    public static class EngineMath
    {
        public const float PI = (float) Math.PI;
        public const float PI_2 = (float) Math.PI * 2f;

        public const float RAD_45DEG = (float)Math.PI * 0.25f;
        public const float RAD_90DEG = (float)Math.PI * 0.5f;
        public const float RAD_135DEG = (float)Math.PI * 0.75f;
        public const float RAD_180DEG = (float)Math.PI;
        public const float RAD_225DEG = (float)Math.PI * 1.15f;
        public const float RAD_270DEG = (float)Math.PI * 1.5f;
        public const float RAD_315DEG = (float)Math.PI * 1.75f;
        public const float RAD_360DEG = (float)Math.PI * 2f;

        public const float DEFAULT_DPI_X = 96f;
        public const float DEFAULT_DPI_Y = DEFAULT_DPI_X;

        public const float TOLERANCE_FLOAT_POSITIVE = 0.00001f;
        public const float TOLERANCE_FLOAT_NEGATIVE = -0.00001f;
        public const double TOLERANCE_DOUBLE_POSITIVE = 0.00001;
        public const double TOLERANCE_DOUBLE_NEGATIVE = -0.00001;
        public const decimal TOLERANCE_DECIMAL_POSITIVE = 0.00001M;
        public const decimal TOLERANCE_DECIMAL_NEGATIVE = -0.00001M;

        public static float AngleFromXY(float x, float y)
        {
            // Implemented with sample code from http://www.d3dcoder.net/d3d11.htm, Source Code Set II
            var theta = 0f;
            if (x >= 0f)
            {
                // Quadrant I or IV
                if (EqualsWithTolerance(x, 0f))
                {
                    // If x = 0, then atan(y/x) = +pi/2 if y > 0
                    //                atan(y/x) = -pi/2 if y < 0
                    if (y > 0f) { theta = PI / 2f; }
                    else { theta = PI / -2f; }
                }
                else
                {
                    theta = (float)Math.Atan(y / x);
                }

                if (theta < 0f)
                {
                    theta += 2f * PI;
                }
            }
            else
            {
                // Quadrant II or III
                theta = (float) (Math.Atan(y / x) + PI);
            }

            return theta;
        }

        /// <summary>
        /// Calculates the factorial of the given value.
        /// </summary>
        /// <param name="value">The value to calculate the factorial of.</param>
        public static long Factorial(int value)
        {
            if (value <= 1) { return 1; }

            var valueLong = value;
            long result = 0;

            for (long actValue = 1; actValue <= valueLong; actValue++)
            {
                result *= actValue;
            }

            return result;
        }

        /// <summary>
        /// Calculates the binomial coefficient out of the given two values.
        /// </summary>
        /// <param name="n">The upper value normally mentioned as 'n'.</param>
        /// <param name="k">The lower value normally mentioned as 'k'.</param>
        public static decimal BinomialCoefficient(decimal n, decimal k)
        {
            var upperValueInt = (int)n;
            var lowerValueInt = (int)k;

            return
                Factorial(upperValueInt) /
                (decimal)(Factorial(lowerValueInt) * Factorial(upperValueInt - lowerValueInt));
        }

        /// <summary>
        /// Converts the given angle value to an absolute value (e. g. -10� to 350�).
        /// </summary>
        /// <param name="angle">The angle to convert.</param>
        public static float GetAbsoluteAngleRadian(float angle)
        {
            var result = angle;

            result = result % ((float)Math.PI * 2f);
            if (result < 0) { result = (float)Math.PI * 2f + result; }

            return result;
        }

        /// <summary>
        /// Converts the given angle value to an absolute value (e. g. -10� to 350�).
        /// </summary>
        /// <param name="angle">The angle to convert.</param>
        public static float GetAbsoluteAngleDegree(float angle)
        {
            var result = angle;

            result %= 360f;
            if (result < 0) { result = 360f + result; }

            return result;
        }

        /// <summary>
        /// Converts the given degree value to radian.
        /// </summary>
        /// <param name="degreeValue">A angle in degree.</param>
        /// <returns>The radian value of the angle.</returns>
        public static float DegreeToRadian(float degreeValue)
        {
            return degreeValue / 360f * RAD_360DEG;
        }

        /// <summary>
        /// Converts the given degree value to radian.
        /// </summary>
        /// <param name="degreeValue">A angle in degree.</param>
        /// <returns>The radian value of the angle.</returns>
        public static float DegreeToRadian(int degreeValue)
        {
            return degreeValue / 360f * RAD_360DEG;
        }

        /// <summary>
        /// Converts the given radian value to degree.
        /// </summary>
        /// <param name="radianValue">A angle in radian.</param>
        /// <returns>The degree value of the angle.</returns>
        public static float RadianToDegree(float radianValue)
        {
            return radianValue / RAD_360DEG * 360f;
        }

        /// <summary>
        /// Ensures that the given value is between given min and max value.
        /// </summary>
        /// <param name="valueToClamp">The value that should be in range between min and max value.</param>
        /// <param name="minValue">The min value.</param>
        /// <param name="maxValue">The max value.</param>
        public static float Clamp(float valueToClamp, float minValue, float maxValue)
        {
            if (valueToClamp < minValue) { return minValue; }
            if (valueToClamp > maxValue) { return maxValue; }
            return valueToClamp;
        }

        /// <summary>
        /// Ensures that the given value is between given min and max value.
        /// </summary>
        /// <param name="valueToClamp">The value that should be in range between min and max value.</param>
        /// <param name="minValue">The min value.</param>
        /// <param name="maxValue">The max value.</param>
        public static int Clamp(int valueToClamp, int minValue, int maxValue)
        {
            if (valueToClamp < minValue) { return minValue; }
            if (valueToClamp > maxValue) { return maxValue; }
            return valueToClamp;
        }

        /// <summary>
        /// Ensures that the given value is between given min and max value.
        /// </summary>
        /// <param name="valueToClamp">The value that should be in range between min and max value.</param>
        /// <param name="minValue">The min value.</param>
        /// <param name="maxValue">The max value.</param>
        public static double Clamp(double valueToClamp, double minValue, double maxValue)
        {
            if (valueToClamp < minValue) { return minValue; }
            if (valueToClamp > maxValue) { return maxValue; }
            return valueToClamp;
        }

        /// <summary>
        /// Check for equality between given value with a given tolerance.
        /// </summary>
        /// <param name="left">One of the value to be compared.</param>
        /// <param name="right">One of the value to be compared.</param>
        /// <param name="tolerance">The tolerance for the comparision.</param>
        public static bool EqualsWithTolerance(double left, double right, double tolerance = TOLERANCE_DOUBLE_POSITIVE)
        {
            return Math.Abs(left - right) < tolerance;
        }

        /// <summary>
        /// Check for equality between given value with a given tolerance.
        /// </summary>
        /// <param name="left">One of the value to be compared.</param>
        /// <param name="right">One of the value to be compared.</param>
        /// <param name="tolerance">The tolerance for the comparision.</param>
        public static bool EqualsWithTolerance(float left, float right, float tolerance = TOLERANCE_FLOAT_POSITIVE)
        {
            return Math.Abs(left - right) < tolerance;
        }

        /// <summary>
        /// Check for equality between given value with a given tolerance.
        /// </summary>
        /// <param name="left">One of the value to be compared.</param>
        /// <param name="right">One of the value to be compared.</param>
        /// <param name="tolerance">The tolerance for the comparision.</param>
        public static bool EqualsWithTolerance(decimal left, decimal right, decimal tolerance = TOLERANCE_DECIMAL_POSITIVE)
        {
            return Math.Abs(left - right) < tolerance;
        }

        /// <summary>
        /// Force the given value to be negative.
        /// </summary>
        /// <param name="floatValue">The value to be changed.</param>
        internal static float ForceNegative(float floatValue)
        {
            if (floatValue > 0) { return -floatValue; }
            return floatValue;
        }

        /// <summary>
        /// Force the given value to be positive.
        /// </summary>
        /// <param name="floatValue">The value to be changed.</param>
        internal static float ForcePositive(float floatValue)
        {
            if (floatValue < 0) { return -floatValue; }
            return floatValue;
        }
    }
}