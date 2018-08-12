#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
using SharpDX;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Objects
{
    /// <summary>
    /// Delegate used for accessing a tesselation function
    /// </summary>
    public delegate Vector3 TesselationFunction(float u, float v);

    /// <summary>
    /// Enumeration containing all components of texture coordinate
    /// </summary>
    public enum TextureCoordinateComponent
    {
        /// <summary>
        /// U component of a texture coordinate
        /// </summary>
        U,

        /// <summary>
        /// V component of a texture coordinate
        /// </summary>
        V
    }

    public enum MaterialType
    {
        SeeingSharpCommon,

        LoadedMeshCommon
    }

    public enum TextureCoordinateCalculationAlignment
    {
        XAxis,

        YAxis,

        ZAxis
    }

    public enum FitToCuboidMode
    {
        MaintainAspectRatio,

        Stretch
    }

    public enum SpacialOriginLocation
    {
        Center,

        LowerCenter
    }

    public enum FontGeometryWeight
    {
        Bold,
        Normal
    }

    public enum FontGeometryStyle
    {
        Normal,
        Oblique,
        Italic
    }

    /// <summary>
    /// Some options for text geometry creation.
    /// </summary>
    public struct TextGeometryOptions
    {
        public static readonly TextGeometryOptions Default = new TextGeometryOptions()
        {
            FontSize = 20,
            FontFamily = "Sergoe UI",
            FontWeight = FontGeometryWeight.Normal,
            FontStyle = FontGeometryStyle.Normal,
            SimplificationFlatternTolerance = 0.1f,
            SurfaceMaterial = NamedOrGenericKey.Empty,
            VerticesScaleFactor = 0.05f,
            SurfaceVertexColor = Color4.White,
            MakeVolumetricText = true,
            MakeBackSurface = true,
            MakeSurface = true,
            VolumetricTextDepth = 0.2f,
            VolumetricSideSurfaceVertexColor = Color4.White,
            CalculateNormals = true,
            VertexTransform = Matrix.Identity
        };

        public string FontFamily;
        public int FontSize;
        public FontGeometryWeight FontWeight;
        public FontGeometryStyle FontStyle;
        public float VerticesScaleFactor;
        public float SimplificationFlatternTolerance;
        public NamedOrGenericKey SurfaceMaterial;
        public bool GenerateCubesOnVertices;
        public Color4 SurfaceVertexColor;
        public Color4 VolumetricSideSurfaceVertexColor;
        public bool MakeVolumetricText;
        public bool MakeBackSurface;
        public bool MakeSurface;
        public float VolumetricTextDepth;
        public bool CalculateNormals;
        public Matrix VertexTransform;
    }
}