using System.ComponentModel;
using System.Numerics;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer.Primitives3D.Pyramid
{
    [SampleDescription(
        "Pyramid", 6, nameof(Primitives3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Primitives3D/Pyramid",
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
            result.Position = new Vector3(0f, 0.5f, 0f);
            return result;
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class PyramidSampleSettings : Primitive3DSampleSettings
        {
            private float _width = 1f;
            private float _height = 1f;

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