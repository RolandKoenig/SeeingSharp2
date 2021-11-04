namespace SeeingSharp.Drawing3D
{
    /// <summary>
    /// A Triangle inside a Geometry object
    /// </summary>
    public struct Triangle
    {
        public int Index1;
        public int Index2;
        public int Index3;

        /// <summary>
        /// Creates a new triangle
        /// </summary>
        /// <param name="index1">Index of the first vertex</param>
        /// <param name="index2">Index of the second vertex</param>
        /// <param name="index3">Index of the third vertex</param>
        public Triangle(int index1, int index2, int index3)
        {
            Index1 = index1;
            Index2 = index2;
            Index3 = index3;
        }

        /// <summary>
        /// Gets all edges defined by this triangle.
        /// </summary>
        /// <param name="sourceGeometry">The source geometry.</param>
        public Line[] GetEdges(Geometry sourceGeometry)
        {
            return new[]
            {
                new Line(
                    sourceGeometry.Vertices[Index1].Position,
                    sourceGeometry.Vertices[Index2].Position),
                new Line(
                    sourceGeometry.Vertices[Index2].Position,
                    sourceGeometry.Vertices[Index3].Position),
                new Line(
                    sourceGeometry.Vertices[Index3].Position,
                    sourceGeometry.Vertices[Index1].Position)
            };
        }
    }
}