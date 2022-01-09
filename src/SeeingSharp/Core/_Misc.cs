using System;
using System.Collections.Generic;

namespace SeeingSharp.Core
{
    public class InternalCatchedExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// The cached exception object.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// The location inside SeeingSharp where the exception occurred.
        /// </summary>
        public InternalExceptionLocation Location { get; }

        internal InternalCatchedExceptionEventArgs(
            Exception ex,
            InternalExceptionLocation location)
        {
            this.Exception = ex;
            this.Location = location;
        }

        public override string ToString()
        {
            return $"{this.Location}: {this.Exception.Message} ({this.Exception.GetType().FullName})";
        }
    }

    public enum InternalExceptionLocation
    {
        Rendering3DObject,

        Rendering2DDrawingLayer,

        Loading3DObject,

        UnloadingInvalid3DObject,

        DisposeObject,

        DisposeGraphicsObject,

        LoadingTexture,

        CreateTextGeometry,

        RenderLoop_ManipulateFilterList,

        RenderLoop_PrepareRendering,

        RenderLoop_PrepareRendering_Update2DOverlay,

        RenderLoop_Unload,

        EngineMainLoop_PrepareRendering,

        EngineMainLoop_Render,

        EngineMainLoop_GenericInputEvent,

        EngineMainLoop_Loop
    }

    public class ManipulateFilterListArgs : EventArgs
    {
        public List<SceneObjectFilter> FilterList
        {
            get;
        }

        public ManipulateFilterListArgs(List<SceneObjectFilter> filterList)
        {
            this.FilterList = filterList;
        }
    }

    /// <summary>
    /// This structure holds information about dpi scaling factors.
    /// </summary>
    public struct DpiScaling
    {
        /// <summary>
        /// The default one on windows systems (96 Dpi, = Scaling 100%).
        /// </summary>
        public static readonly DpiScaling Default = new DpiScaling
        {
            DpiX = 96.0f,
            DpiY = 96.0f
        };

        public float DpiX;
        public float DpiY;

        public DpiScaling(float dpiX, float dpiY)
        {
            DpiX = dpiX;
            DpiY = dpiY;
        }

        public float ScaleFactorX => DpiX / 96.0f;

        public float ScaleFactorY => DpiY / 96.0f;
    }

    /// <summary>
    /// Enumeration containing supported target hardware
    /// </summary>
    public enum HardwareDriverLevel
    {
        /// <summary>
        /// Driver runs on Direct3D12 hardware.
        /// </summary>
        Direct3D12,

        /// <summary>
        /// Driver runs on Direct3D11 hardware.
        /// </summary>
        Direct3D11,

        /// <summary>
        /// Driver runs on Direct3D10 hardware.
        /// </summary>
        Direct3D10
    }

    /// <summary>
    /// Enumeration containing 3 levels of antialiasing quality.
    /// </summary>
    public enum AntialiasingQualityLevel
    {
        /// <summary>
        /// Do antialiasing, but try to configure it in low quality (depends on hardware support levels).
        /// </summary>
        Low,

        /// <summary>
        /// Do antialiasing, but try to configure it in medium quality (depends on hardware support levels).
        /// </summary>
        Medium,

        /// <summary>
        /// Do antialiasing, but try to configure it in high quality (depends on hardware support levels).
        /// </summary>
        High
    }

    /// <summary>
    /// Enumeration containing 3 levels of texture filtering quality.
    /// </summary>
    public enum TextureSamplerQualityLevel
    {
        /// <summary>
        /// Low quality texture sampler.
        /// </summary>
        Low,

        /// <summary>
        /// Medium quality texture sampler.
        /// </summary>
        Medium,

        /// <summary>
        /// High texture filtering quality (anisotropic, 16x)
        /// </summary>
        High
    }

    /// <summary>
    /// Describes a detail level
    /// </summary>
    [Flags]
    public enum DetailLevel
    {
        /// <summary>
        /// Describes low performance hardware (e. g. Software renderer).
        /// </summary>
        Low = 1,

        //Medium = 2,

        /// <summary>
        /// Describes high class hardware.
        /// </summary>
        High = 4,

        /// <summary>
        /// A detail level mentioned for all classes of hardware.
        /// </summary>
        All = 7
    }

    /// <summary>
    /// Enumeration containing all texture quality levels
    /// </summary>
    public enum TextureQuality
    {
        /// <summary>
        /// Low res textures
        /// </summary>
        Low,

        /// <summary>
        /// High res textures
        /// </summary>
        High
    }

    /// <summary>
    /// Enumeration containing geometry detail levels.
    /// </summary>
    public enum GeometryQuality
    {
        /// <summary>
        /// Low quality.
        /// </summary>
        Low,

        /// <summary>
        /// High quality.
        /// </summary>
        High
    }

    /// <summary>
    /// Describes the used coordinate system.
    /// Frozen Sky uses LeftHanded_UpY by default, all other systems need to be mapped.
    ///  see http://www.scratchapixel.com/lessons/3d-basic-lessons/lesson-4-geometry/coordinate-systems/
    /// </summary>
    public enum CoordinateSystem
    {
        /// <summary>
        /// Left handed system, y-axis goes upwards.
        /// </summary>
        LeftHanded_UpY,

        /// <summary>
        /// Right handed system, y-axis goes upwards.
        /// </summary>
        RightHanded_UpY,

        /// <summary>
        /// Left handed system, z-axis goes upwards.
        /// </summary>
        LeftHanded_UpZ,

        /// <summary>
        /// Right handed system, z-axis goes upwards.
        /// </summary>
        RightHanded_UpZ
    }

    public class PickingOptions
    {
        public bool OnlyCheckBoundingBoxes;
    }
}
