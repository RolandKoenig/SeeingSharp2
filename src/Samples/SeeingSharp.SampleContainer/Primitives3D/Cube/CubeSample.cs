using System.ComponentModel;
using System.Numerics;
using SeeingSharp.Core;
using SeeingSharp.Drawing3D;
using SeeingSharp.Drawing3D.Primitives;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer.Primitives3D.Cube
{
    [SampleDescription(
        "Cube", 1, nameof(Primitives3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/src/Samples/SeeingSharp.SampleContainer/Primitives3D/Cube",
        typeof(CubeSampleSettings))]
    public class CubeSample : Primitive3DSampleBase
    {
        protected override Mesh CreateMesh(SceneManipulator manipulator, SampleSettings sampleSettings, NamedOrGenericKey resMaterial)
        {
            var castedSettings = (CubeSampleSettings) sampleSettings;

            var resGeometry = manipulator.AddResource(
                device => new GeometryResource(
                    new CubeGeometryFactory
                    {
                        Width = castedSettings.Width,
                        Height = castedSettings.Height,
                        Depth = castedSettings.Depth
                    }));

            var result = new Mesh(resGeometry, resMaterial);
            result.Position = new Vector3(0f, 0.5f + castedSettings.Height / 2f, 0f);
            return result;
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class CubeSampleSettings : Primitive3DSampleSettings
        {
            private float _width;
            private float _height;
            private float _depth;

            public CubeSampleSettings()
            {
                // Set defaults
                var geoFactory = new CubeGeometryFactory();
                _width = geoFactory.Width;
                _height = geoFactory.Height;
                _depth = geoFactory.Height;
            }

            [Category(CATEGORY_NAME)]
            public float Width
            {
                get => _width;
                set
                {
                    if (!EngineMath.EqualsWithTolerance(_width, value))
                    {
                        _width = value;
                        this.RaiseRecreateRequest();
                    }
                }
            }

            [Category(CATEGORY_NAME)]
            public float Height
            {
                get => _height;
                set
                {
                    if (!EngineMath.EqualsWithTolerance(_height, value))
                    {
                        _height = value;
                        this.RaiseRecreateRequest();
                    }
                }
            }

            [Category(CATEGORY_NAME)]
            public float Depth
            {
                get => _depth;
                set
                {
                    if (!EngineMath.EqualsWithTolerance(_depth, value))
                    {
                        _depth = value;
                        this.RaiseRecreateRequest();
                    }
                }
            }
        }
    }
}