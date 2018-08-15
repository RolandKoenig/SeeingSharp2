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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SharpDX;

namespace SeeingSharp.Multimedia.Objects
{
    public class StackedObjectType : ObjectType
    {
        #region Main parameters
        private ObjectType m_objTypeToStack;
        private int m_stackSize;
        #endregion

        public StackedObjectType(ObjectType objTypeToStack, int stackSize)
        {
            objTypeToStack.EnsureNotNull(nameof(objTypeToStack));
            stackSize.EnsurePositiveAndNotZero(nameof(stackSize));

            m_objTypeToStack = objTypeToStack;
            m_stackSize = stackSize;
        }

        /// <summary>
        /// Builds all vertex structures for the given detail level.
        /// </summary>
        /// <param name="buildOptions">Some generic options for structure building</param>
        public override VertexStructure BuildStructure(StructureBuildOptions buildOptions)
        {
            VertexStructure structureFromChild = m_objTypeToStack.BuildStructure(buildOptions);
            structureFromChild.EnsureNotNull(nameof(structureFromChild));

            BoundingBox childStructBox = structureFromChild.GenerateBoundingBox();
            Vector3 correctionVector = -childStructBox.GetBottomCenter();

            // Copy metadata infomration of the VertexStructures
            VertexStructure result = structureFromChild.Clone(
                copyGeometryData: false,
                capacityMultiplier: m_stackSize);

            // Build geometry
            for (int loop = 0; loop < m_stackSize; loop++)
            {
                float actYCorrection = childStructBox.Height * loop;
                Vector3 localCorrection = new Vector3(correctionVector.X, correctionVector.Y + actYCorrection, correctionVector.Z);

                int baseVertex = loop * structureFromChild.CountVertices;
                foreach (Vertex actVertex in structureFromChild.Vertices)
                {
                    // Change vertex properties based on stack position
                    Vertex changedVertex = actVertex;
                    changedVertex.Position = changedVertex.Position + localCorrection;
                    if (loop % 2 == 1)
                    {
                        var color = changedVertex.Color; 
                        color.ChangeColorByLight(0.05f);
                        changedVertex.Color = color;
                    }

                    // Add the vertex
                    result.AddVertex(changedVertex);
                }

                // Clone all surfaces
                foreach (VertexStructureSurface actSurfaceFromChild in structureFromChild.Surfaces)
                {
                    VertexStructureSurface newSurface = result.CreateSurface(actSurfaceFromChild.CountTriangles);
                    foreach (Triangle actTriangle in actSurfaceFromChild.Triangles)
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
