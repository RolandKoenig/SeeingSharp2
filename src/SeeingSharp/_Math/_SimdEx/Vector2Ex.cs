﻿/*
    SeeingSharp and all applications distributed together with it. 
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
using System.Numerics;
using System.Text;

namespace SeeingSharp
{
    public static class Vector2Ex
    {
        public static bool EqualsWithTolerance(Vector2 left, Vector2 right, float tolerance = EngineMath.TOLERANCE_FLOAT_POSITIVE)
        {
            return
                EngineMath.EqualsWithTolerance(left.X, right.X, tolerance) &&
                EngineMath.EqualsWithTolerance(left.Y, right.Y, tolerance);
        }

        public static Vector2 FromSize2(Size2 size)
        {
            return new Vector2(size.Width, size.Height);
        }

        public static Vector2 FromSize2(Size2F size)
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