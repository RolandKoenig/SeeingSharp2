using System;
using System.Numerics;

namespace SeeingSharp
{
    public partial struct Size2
    {
        public Size2(Tuple<int, int> values)
            : this(values.Item1, values.Item2)
        {

        }

        public Vector2 ToVector2()
        {
            return new Vector2(Width, Height);
        }
    }
}
