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
using SeeingSharp.Checking;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
{
    /// <summary>
    /// The default structure for sending vertex data to the GPU.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct StandardVertex
    {
        // Constants
        public static readonly int Size = Marshal.SizeOf<StandardVertex>();
        public static readonly D3D11.InputElement[] InputElements = {
            new D3D11.InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new D3D11.InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
            new D3D11.InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, 24, 0),
            new D3D11.InputElement("TEXCOORD", 0, Format.R32G32_Float, 28, 0),
            new D3D11.InputElement("TEXCOORD", 1, Format.R32_Float, 36, 0)
        };

        // All vertex elements
        public Vector3 Position;
        public Vector3 Normal;
        public int Color;
        public Vector2 Texture;
        public float TextureFactor;

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardVertex"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="color">The color.</param>
        public StandardVertex(Vector3 position, int color)
        {
            Position = position;
            Normal = new Vector3(0, 1, 0);
            Texture = new Vector2(0f, 0f);
            Color = color;
            TextureFactor = 0f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardVertex"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="texCoord">The texture coordinate.</param>
        public StandardVertex(Vector3 position, Vector2 texCoord)
        {
            Position = position;
            Normal = new Vector3();
            Texture = texCoord;
            Color = 0;
            TextureFactor = 0f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardVertex"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="texCoord">The texture coordinate.</param>
        /// <param name="normal">The normal.</param>
        public StandardVertex(Vector3 position, Vector2 texCoord, Vector3 normal)
        {
            Position = position;
            Normal = normal;
            Texture = texCoord;
            Color = 0;
            TextureFactor = 0f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardVertex"/> struct.
        /// </summary>
        /// <param name="vertex">Source vertex data.</param>
        public StandardVertex(VertexBasic vertex)
        {
            Position = vertex.Position;
            Normal = vertex.Normal;
            Texture = vertex.TexCoord1;
            Color = vertex.Color.ToRgba();
            TextureFactor = vertex.TextureFactor;
        }

        /// <summary>
        /// Gets an array of StandardVertices from the given Geometry object.
        /// </summary>
        public static StandardVertex[] FromGeometry(Geometry source)
        {
            source.EnsureNotNull(nameof(source));

            var vertexCount = source.CountVertices;

            //Create result array
            var result = new StandardVertex[vertexCount];

            for (var loop = 0; loop < source.CountVertices; loop++)
            {
                var vertex = source.Vertices[loop];
                result[loop] = new StandardVertex(vertex);
            }

            return result;
        }

        /// <summary>
        /// Gets an array of StandardVertices from the given Geometry objects.
        /// </summary>
        public static StandardVertex[] FromGeometry(Geometry[] geometries)
        {
            geometries.EnsureNotNullOrEmpty(nameof(geometries));

            //Get total vertex count
            var vertexCount = 0;

            for (var loop = 0; loop < geometries.Length; loop++)
            {
                vertexCount += geometries[loop].CountVertices;
            }

            //create result array
            var result = new StandardVertex[vertexCount];
            var actVertexPos = 0;

            for (var loop = 0; loop < geometries.Length; loop++)
            {
                var actGeometry = geometries[loop];
                var geometryVertexCount = actGeometry.CountVertices;

                for (var innerLoop = 0; innerLoop < geometryVertexCount; innerLoop++)
                {
                    result[actVertexPos] = new StandardVertex(actGeometry.Vertices[innerLoop]);
                    actVertexPos++;
                }
            }

            return result;
        }

        /// <summary>
        /// Creates an array containing all vertices needed to display a list generated by given function.
        /// </summary>
        /// <param name="lineColor">Color of the line.</param>
        /// <param name="lineListCreator">Function that creates each line.</param>
        public static StandardVertex[] FromLineList(Color4 lineColor, Func<List<Vector3>> lineListCreator)
        {
            return FromLineList(lineColor, lineListCreator());
        }

        /// <summary>
        /// Creates an array containing all vertices needed to display the given list of lines.
        /// </summary>
        /// <param name="lineColor">Color of the line.</param>
        /// <param name="lineList">List containing the lines.</param>
        public static StandardVertex[] FromLineList(Color4 lineColor, List<Vector3> lineList)
        {
            return FromLineList(lineColor, lineList.ToArray());
        }

        /// <summary>
        /// Creates an array containing all vertices needed to display the given list of lines.
        /// </summary>
        /// <param name="lineColor">Color of the line.</param>
        /// <param name="lineList">List containing the lines.</param>
        public static StandardVertex[] FromLineList(Color4 lineColor, params Vector3[] lineList)
        {
            if (lineList == null || lineList.Length == 0)
            {
                return new StandardVertex[0];
            }

            var result = new StandardVertex[lineList.Length];

            for (var loop = 0; loop < lineList.Length; loop++)
            {
                var actVertex = new StandardVertex(lineList[loop], lineColor.ToRgba());
                result[loop] = actVertex;
            }

            return result;
        }
    }
}