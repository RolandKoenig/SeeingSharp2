using System.ComponentModel;
using System.Numerics;
using SeeingSharp.Core;
using SeeingSharp.Drawing3D;
using SeeingSharp.Drawing3D.Primitives;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer.Primitives3D.Pyramid
{
    [SampleDescription(
        "Pyramid", 6, nameof(Primitives3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/src/Samples/SeeingSharp.SampleContainer/Primitives3D/Pyramid",
        typeof(PyramidSampleSettings))]
    public class PyramidSample : Primitive3DSampleBase
    {
        protected override Mesh CreateMesh(SceneManipulator manipulator, SampleSettings sampleSettings, NamedOrGenericKey resMaterial)
        {
            var castedSettings = (PyramidSampleSettings) sampleSettings;

            var resGeometry = manipulator.AddResource(
                device => new GeometryResource(
                    new PyramidGeometryFactory
                    { 
                        Width = castedSettings.Width,
                        Height = castedSettings.Height
                    }));

            var result = new Mesh(resGeometry, resMaterial);
            result.Position = new Vector3(0f, 0.5f + castedSettings.Height / 2f, 0f);
            return result;
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class PyramidSampleSettings : Primitive3DSampleSettings
        {
            private float _width;
            private float _height;

            public PyramidSampleSettings()
            {
                // Set defaults
                var geoFactory = new PyramidGeometryFactory();
                _width = geoFactory.Width;
                _height = geoFactory.Height;
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
        }
    }
}