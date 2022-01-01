using System.Numerics;
using SeeingSharp.Checking;

namespace SeeingSharp.Drawing3D.Geometries
{
    public class StackedGeometryFactory : GeometryFactory
    {
        // Main parameters
        private GeometryFactory _geometryToStack;
        private int _stackSize;

        public StackedGeometryFactory(GeometryFactory geometryToStack, int stackSize)
        {
            geometryToStack.EnsureNotNull(nameof(geometryToStack));
            stackSize.EnsurePositiveAndNotZero(nameof(stackSize));

            _geometryToStack = geometryToStack;
            _stackSize = stackSize;
        }

        /// <summary>
        /// Builds the geometry for the given detail level.
        /// </summary>
        /// <param name="buildOptions">Some generic options for geometry building</param>
        public override Geometry BuildGeometry(GeometryBuildOptions buildOptions)
        {
            var geometryFromChild = _geometryToStack.BuildGeometry(buildOptions);
            geometryFromChild.EnsureNotNull(nameof(geometryFromChild));

            var childStructBox = geometryFromChild.GenerateBoundingBox();
            var correctionVector = -childStructBox.GetBottomCenter();

            // Copy metadata information of the Geometry
            var result = geometryFromChild.Clone(
                false,
                _stackSize);

            // Build geometry
            for (var loop = 0; loop < _stackSize; loop++)
            {
                var actYCorrection = childStructBox.Height * loop;
                var localCorrection = new Vector3(correctionVector.X, correctionVector.Y + actYCorrection, correctionVector.Z);

                var baseVertex = loop * geometryFromChild.CountVertices;

                foreach (var actVertex in geometryFromChild.Vertices)
                {
                    // Change vertex properties based on stack position
                    var changedVertex = actVertex;
                    changedVertex.Position = changedVertex.Position + localCorrection;

                    if (loop % 2 == 1)
                    {
                        var color = changedVertex.Color;
                        color.ChangeColorByLight(0.05f);
                        changedVertex.Color = color;
                    }

                    // AddObject the vertex
                    result.AddVertex(changedVertex);
                }

                // Clone all surfaces
                foreach (var actSurfaceFromChild in geometryFromChild.Surfaces)
                {
                    var newSurface = result.CreateSurface(actSurfaceFromChild.CountTriangles);

                    foreach (var actTriangle in actSurfaceFromChild.Triangles)
                    {
                        newSurface.AddTriangle(
                            baseVertex + actTriangle.Index1,
                            baseVertex + actTriangle.Index2,
                            baseVertex + actTriangle.Index3);
                    }
                }
            }

            return result;
        }
    }
}