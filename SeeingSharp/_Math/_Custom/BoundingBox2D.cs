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
    public struct BoundingBox2D
    {
        public static readonly BoundingBox2D Empty = new BoundingBox2D(Vector2.Zero, Vector2.Zero);

        public Vector2 Location;
        public Vector2 Size;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundingBox2D" /> struct.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="size">The size.</param>
        public BoundingBox2D(Vector2 location, Vector2 size)
        {
            this.Location = location;
            this.Size = size;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "Pos: " + this.Location.ToString() + "; Size: " + this.Size.ToString();
        }

        /// <summary>
        /// Is this box contained by the given one?
        /// </summary>
        /// <param name="otherOne"></param>
        public bool IsContainedBy(BoundingBox2D otherOne)
        {
            Vector2 thisMinimum = this.Location;
            Vector2 thisMaximum = this.Location + this.Size;
            Vector2 otherMinimum = otherOne.Location;
            Vector2 otherMaximum = otherOne.Location + otherOne.Size;

            return (otherMinimum.X <= thisMinimum.X) &&
                   (otherMinimum.Y <= thisMinimum.Y) &&
                   (otherMaximum.X >= thisMaximum.X) &&
                   (otherMaximum.Y >= thisMaximum.Y);
        }

        /// <summary>
        /// Is the given smaller box contained by the given bigger one?
        /// </summary>
        /// <param name="smallerBox"></param>
        /// <param name="biggerBox"></param>
        public static bool IsContainedBy(BoundingBox2D smallerBox, BoundingBox2D biggerBox)
        {
            return smallerBox.IsContainedBy(biggerBox);
        }

        /// <summary>
        /// Is this box empty?
        /// </summary>
        public bool IsEmpty
        {
            get { return this.Location.IsEmpty() && this.Size.IsEmpty(); }
        }
    }
}
