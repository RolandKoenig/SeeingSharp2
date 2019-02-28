#region License information
/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
#endregion
namespace SeeingSharp.Multimedia.Objects
{
    #region using

    using SharpDX;

    #endregion

    public struct BuiltVerticesRange
    {
        public VertexStructure Structure;
        public int StartVertex;
        public int VertexCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltVerticesRange" /> struct.
        /// </summary>
        /// <param name="structure">The structure.</param>
        public BuiltVerticesRange(VertexStructure structure)
        {
            this.Structure = structure;
            this.StartVertex = 0;
            this.VertexCount = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltVerticesRange" /> struct.
        /// </summary>
        /// <param name="structure">The structure.</param>
        /// <param name="startVertex">The start vertex.</param>
        /// <param name="vertexCount">The vertex count.</param>
        public BuiltVerticesRange(VertexStructure structure, int startVertex, int vertexCount)
        {
            this.Structure = structure;
            this.StartVertex = startVertex;
            this.VertexCount = vertexCount;
        }

        /// <summary>
        /// Merges this structure with the given one.
        /// This method does only work if the given structure does begin after the end of the current one.
        /// </summary>
        /// <param name="otherRange">The other range to merge with.</param>
        public void Merge(BuiltVerticesRange otherRange)
        {
            if (VertexCount <= 0)
            {
                StartVertex = otherRange.StartVertex;
                VertexCount = otherRange.VertexCount;
            }
            else
            {
                if(StartVertex + VertexCount != otherRange.StartVertex)
                {
                    throw new SeeingSharpGraphicsException("Unable to merge given vertex ranges!");
                }
                VertexCount = (int)(VertexCount + otherRange.VertexCount);
            }
        }

        public BuiltVerticesRange DisableTexture()
        {
            int lastVertex = StartVertex + VertexCount;

            for (var loop = StartVertex; loop < lastVertex; loop++)
            {
                var actVertex = this.Structure.Vertices[loop];
                actVertex.TextureFactor = -100;
                this.Structure.Vertices[loop] = actVertex;
            }

            return this;
        }

        public BuiltVerticesRange FlipTextureCoordinatesX()
        {
            var lastVertex = StartVertex + VertexCount;

            for (var loop = StartVertex; loop < lastVertex; loop++)
            {
                var actVertex = this.Structure.Vertices[loop];
                actVertex.TexCoord = new Vector2(
                    -actVertex.TexCoord.X,
                    actVertex.TexCoord.Y);
                this.Structure.Vertices[loop] = actVertex;
            }

            return this;
        }

        public BuiltVerticesRange FlipTextureCoordinatesY()
        {
            var lastVertex = StartVertex + VertexCount;

            for (var loop = StartVertex; loop < lastVertex; loop++)
            {
                var actVertex = this.Structure.Vertices[loop];
                actVertex.TexCoord = new Vector2(
                    actVertex.TexCoord.X,
                    -actVertex.TexCoord.Y);
                this.Structure.Vertices[loop] = actVertex;
            }

            return this;
        }

        public BuiltVerticesRange FlipTextureCoordinatesXY()
        {
            var lastVertex = StartVertex + VertexCount;

            for (var loop = StartVertex; loop < lastVertex; loop++)
            {
                var actVertex = this.Structure.Vertices[loop];
                actVertex.TexCoord = new Vector2(
                    -actVertex.TexCoord.X,
                    -actVertex.TexCoord.Y);
                this.Structure.Vertices[loop] = actVertex;
            }

            return this;
        }

        public BuiltVerticesRange RotateTextureCoordinates()
        {
            var lastVertex = StartVertex + VertexCount;

            for (var loop = StartVertex; loop < lastVertex; loop++)
            {
                var actVertex = this.Structure.Vertices[loop];
                actVertex.TexCoord = new Vector2(
                    actVertex.TexCoord.Y,
                    actVertex.TexCoord.X);
                this.Structure.Vertices[loop] = actVertex;
            }

            return this;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return this.VertexCount <= 0; }
        }
    }
}