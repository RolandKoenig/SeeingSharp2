using System.Numerics;
using SeeingSharp.Multimedia.Drawing2D;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Drawing3D
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

    /// <summary>
    /// Some options for text geometry creation.
    /// </summary>
    public struct TextGeometryOptions
    {
        public static readonly TextGeometryOptions Default = new TextGeometryOptions
        {
            FontSize = 20,
            FontFamily = "Sergoe UI",
            FontWeight = FontWeight.Normal,
            FontStyle = FontStyle.Normal,
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
            VertexTransform = Matrix4x4.Identity
        };

        public string FontFamily;
        public int FontSize;
        public FontWeight FontWeight;
        public FontStyle FontStyle;
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
        public Matrix4x4 VertexTransform;
    }
}