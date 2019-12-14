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
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharp
{
    public static class Vector2Ex
    {
#if DESKTOP
        public static Vector2 FromWpfPoint(System.Windows.Point point)
        {
            return new Vector2(
                (float)point.X,
                (float)point.Y);
        }

        public static Vector2 FromWpfSize(System.Windows.Size size)
        {
            return new Vector2(
                (float)size.Width,
                (float)size.Height);
        }

        public static Vector2 FromWpfVector(System.Windows.Vector vector)
        {
            return new Vector2(
                (float)vector.X,
                (float)vector.Y);
        }
#endif

        public static Vector2 FromSize2(Size2 size)
        {
            return new Vector2(size.Width, size.Height);
        }

        public static Vector2 FromSize2(Size2F size)
        {
            return new Vector2(size.Width, size.Height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetValue(Vector2 vector, int index)
        {
            switch (index)
            {
                case 1: return vector.X;
                case 2: return vector.Y;
                default: throw new ArgumentException("Invalid index!");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue(Vector2 vector, int index, float value)
        {
            switch (index)
            {
                case 1: vector.X = value; break;
                case 2: vector.Y = value; break;
                default: throw new ArgumentException("Invalid index!");
            }
        }

    }
}
