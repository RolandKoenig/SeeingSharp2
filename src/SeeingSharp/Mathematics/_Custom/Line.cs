using System.Numerics;
using System.Runtime.InteropServices;

namespace SeeingSharp.Mathematics
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Line
    {
        public Vector3 StartPosition;
        public Vector3 EndPosition;

        public Line(Vector3 start, Vector3 end)
        {
            StartPosition = start;
            EndPosition = end;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "From " + StartPosition + " to " + EndPosition;
        }

        /// <summary>
        /// Equality test with a small tolerance.
        /// </summary>
        /// <param name="otherLine">The other line to check.</param>
        /// <param name="tolerance">The tolerance for the equality test.</param>
        public bool EqualsWithTolerance(Line otherLine, float tolerance = EngineMath.TOLERANCE_FLOAT_POSITIVE)
        {
            return
                Vector3Ex.EqualsWithTolerance(StartPosition, otherLine.StartPosition, tolerance) &&
                Vector3Ex.EqualsWithTolerance(EndPosition, otherLine.EndPosition, tolerance);
        }

        /// <summary>
        /// Overrides the x location of all coordinates.
        /// </summary>
        /// <param name="xLocation">The location to set.</param>
        public void SetAllXLocations(float xLocation)
        {
            StartPosition.X = xLocation;
            EndPosition.X = xLocation;
        }

        /// <summary>
        /// Overrides the y location of all coordinates.
        /// </summary>
        /// <param name="yLocation">The location to set.</param>
        public void SetAllYLocations(float yLocation)
        {
            StartPosition.Y = yLocation;
            EndPosition.Y = yLocation;
        }

        /// <summary>
        /// Overrides the z location of all coordinates.
        /// </summary>
        /// <param name="zLocation">The location to set.</param>
        public void SetAllZLocations(float zLocation)
        {
            StartPosition.Z = zLocation;
            EndPosition.Z = zLocation;
        }
    }
}
