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
using SharpDX;

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
            Location = location;
            Size = size;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "Pos: " + Location + "; Size: " + Size;
        }

        /// <summary>
        /// Is this box contained by the given one?
        /// </summary>
        /// <param name="otherOne"></param>
        public bool IsContainedBy(BoundingBox2D otherOne)
        {
            var thisMinimum = Location;
            var thisMaximum = Location + Size;
            var otherMinimum = otherOne.Location;
            var otherMaximum = otherOne.Location + otherOne.Size;

            return otherMinimum.X <= thisMinimum.X &&
                   otherMinimum.Y <= thisMinimum.Y &&
                   otherMaximum.X >= thisMaximum.X &&
                   otherMaximum.Y >= thisMaximum.Y;
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
        public bool IsEmpty => Location.IsEmpty() && Size.IsEmpty();
    }
}