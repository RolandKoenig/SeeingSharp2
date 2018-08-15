#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2018 Roland König (RolandK)
    
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
using SharpDX;
using System.Collections.Generic;
using System.Numerics;

#if DESKTOP
using System.Windows.Media.Media3D;
#endif

namespace SeeingSharp
{
    public static class MathExtensions
    {
        public static bool IsEmpty(this ref Vector2 vector)
        {
            return vector.Equals(Vector2.Zero);
        }

        /// <summary>
        /// Gets all points contained in given line collection.
        /// </summary>
        /// <param name="lines">A list containing all lines.</param>
        public static IEnumerable<Vector3> GetAllPoints(this IEnumerable<Line> lines)
        {
            Vector3 lastVector = Vector3Ex.MinValue;
            foreach (Line actLine in lines)
            {
                if (lastVector != actLine.StartPosition) { yield return actLine.StartPosition; }

                yield return actLine.EndPosition;

                lastVector = actLine.EndPosition;
            }
        }
    }
}
