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
using System.Numerics;

namespace SeeingSharp
{
    public static class EngineMath
    {
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

        /// <summary>
        /// Calculates the factorial of the given value.
        /// </summary>
        /// <param name="value">The value to calculate the factorial of.</param>
        public static long Factorial(int value)
        {
            if (value <= 1) { return 1; }

            long valueLong = (long)value;
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
        /// <param name="n">The upper vlaue normally mentioned as 'n'.</param>
        /// <param name="k">The lower vlaue normally mentioned as 'k'.</param>
        public static decimal BinomialCoefficient(decimal n, decimal k)
        {
            int upperValueInt = (int)n;
            int lowerValueInt = (int)k;

            return
                (decimal)Factorial(upperValueInt) /
                (decimal)(Factorial(lowerValueInt) * Factorial(upperValueInt - lowerValueInt));
        }

        /// <summary>
        /// Converts the given angle value to an absolute value (e. g. -10° to 350°).
        /// </summary>
        /// <param name="angle">The angle to convert.</param>
        public static float GetAbsoluteAngleRadian(float angle)
        {
            float result = angle;

            result = result % ((float)Math.PI * 2f);
            if (result < 0) { result = ((float)Math.PI * 2f) + result; }

            return result;
        }

        /// <summary>
        /// Converts the given angle value to an absolute value (e. g. -10° to 350°).
        /// </summary>
        /// <param name="angle">The angle to convert.</param>
        public static float GetAboluteAngleDegree(float angle)
        {
            float result = angle;

            result = result % 360f;
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
            return (degreeValue / 360f) * RAD_360DEG;
        }

        /// <summary>
        /// Converts the given degree value to radian.
        /// </summary>
        /// <param name="degreeValue">A angle in degree.</param>
        /// <returns>The radian value of the angle.</returns>
        public static float DegreeToRadian(int degreeValue)
        {
            return (degreeValue / 360f) * RAD_360DEG;
        }

        /// <summary>
        /// Converts the given radian vlaue to degree.
        /// </summary>
        /// <param name="radianValue">A angle in radian.</param>
        /// <returns>The degree value of the angle.</returns>
        public static float RadianToDegree(float radianValue)
        {
            return (radianValue / RAD_360DEG) * 360f;
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
        public static bool EqualsWithTolerance(double left, double right, double tolerance = 0.00001)
        {
            return Math.Abs(left - right) < tolerance;
        }

        /// <summary>
        /// Check for equality between given value with a given tolerance.
        /// </summary>
        /// <param name="left">One of the value to be compared.</param>
        /// <param name="right">One of the value to be compared.</param>
        /// <param name="tolerance">The tolerance for the comparision.</param>
        public static bool EqualsWithTolerance(float left, float right, float tolerance = 0.00001f)
        {
            return Math.Abs(left - right) < tolerance;
        }

        /// <summary>
        /// Check for equality between given value with a given tolerance.
        /// </summary>
        /// <param name="left">One of the value to be compared.</param>
        /// <param name="right">One of the value to be compared.</param>
        /// <param name="tolerance">The tolerance for the comparision.</param>
        public static bool EqualsWithTolerance(decimal left, decimal right, decimal tolerance = 0.00001M)
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