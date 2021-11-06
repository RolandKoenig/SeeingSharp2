using SeeingSharp.Checking;

namespace SeeingSharp.Mathematics
{
    public partial struct Color4
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> class.
        /// </summary>
        /// <param name="red">The red.</param>
        /// <param name="green">The green.</param>
        /// <param name="blue">The blue.</param>
        /// <param name="alpha">The alpha.</param>
        public Color4(int red, int green, int blue, int alpha)
        {
            Alpha = alpha / 255f;
            Red = red / 255f;
            Green = green / 255f;
            Blue = blue / 255f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> class.
        /// </summary>
        /// <param name="red">The red.</param>
        /// <param name="green">The green.</param>
        /// <param name="blue">The blue.</param>
        public Color4(int red, int green, int blue)
            : this(red, green, blue, 255)
        {

        }

        public Color4 ChangeAlphaTo(float newAlpha)
        {
            Alpha = newAlpha;
            return this;
        }

        public Color4 ChangeColorByLight(float changeFactor)
        {
            changeFactor.EnsureInRange(0.000001f, 0.4999999f, nameof(changeFactor));

            Red = Red < 0.5f ? Red + changeFactor : Red - changeFactor;
            Green = Green < 0.5f ? Green + changeFactor : Green - changeFactor;
            Blue = Blue < 0.5f ? Blue + changeFactor : Blue - changeFactor;

            return this;
        }

        public bool EqualsWithTolerance(Color4 other)
        {
            return
                EngineMath.EqualsWithTolerance(Red, other.Red) &&
                EngineMath.EqualsWithTolerance(Green, other.Green) &&
                EngineMath.EqualsWithTolerance(Blue, other.Blue) &&
                EngineMath.EqualsWithTolerance(Alpha, other.Alpha);
        }
    }
}
