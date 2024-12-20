﻿using System.Numerics;

namespace SeeingSharp.Mathematics
{
    public struct BoundingBox2D
    {
        public static readonly BoundingBox2D Empty = new BoundingBox2D(Vector2.Zero, Vector2.Zero);

        public Vector2 Location;
        public Vector2 Size;

        /// <summary>
        /// Is this box empty?
        /// </summary>
        public bool IsEmpty => Vector2Ex.IsEmpty(ref this.Location) && Vector2Ex.IsEmpty(ref this.Size);

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
    }
}
