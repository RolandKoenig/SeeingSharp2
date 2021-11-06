using System.ComponentModel;
using System.Numerics;
using SeeingSharp.Core;
using SeeingSharp.Drawing3D;
using SeeingSharp.Drawing3D.Primitives;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer.Primitives3D.Torus
{
    [SampleDescription(
        "Torus", 4, nameof(Primitives3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Primitives3D/Torus",
        typeof(TorusSampleSettings))]
    public class TorusSample : Primitive3DSampleBase
    {
        protected override Mesh CreateMesh(SceneManipulator manipulator, SampleSettings sampleSettings, NamedOrGenericKey resMaterial)
        {
            var castedSettings = (TorusSampleSettings) sampleSettings;

            var resGeometry = manipulator.AddResource(
                device => new GeometryResource(
                    new TorusGeometryFactory()
                    {
                        TDiv = castedSettings.TDiv,
                        PDiv = castedSettings.PDiv,
                        TorusRadius = castedSettings.TorusRadius,
                        TubeRadius = castedSettings.TubeRadius,
                    }));

            var result = new Mesh(resGeometry, resMaterial);
            result.Position = new Vector3(0f, 0.5f + castedSettings.TubeRadius  + castedSettings.TorusRadius, 0f);
            return result;
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class TorusSampleSettings : Primitive3DSampleSettings
        {
            private int _tDiv;
            private int _pDiv;
            private float _torusRadius;
            private float _tubeRadius;

            public TorusSampleSettings()
            {
                // Set defaults
                var geoFactory = new TorusGeometryFactory();
                _tDiv = geoFactory.TDiv;
                _pDiv = geoFactory.PDiv;
                _torusRadius = geoFactory.TorusRadius;
                _tubeRadius = geoFactory.TubeRadius;
            }

            [Category(CATEGORY_NAME)]
            public int TDiv
            {
                get => _tDiv;
                set
                {
                    if (_tDiv != value)
                    {
                        _tDiv= value;
                        this.RaiseRecreateRequest();
                    }
                }
            }

            [Category(CATEGORY_NAME)]
            public int PDiv
            {
                get => _pDiv;
                set
                {
                    if (_pDiv != value)
                    {
                        _pDiv= value;
                        this.RaiseRecreateRequest();
                    }
                }
            }

            [Category(CATEGORY_NAME)]
            public float TorusRadius
            {
                get => _torusRadius;
                set
                {
                    if (!EngineMath.EqualsWithTolerance(_torusRadius, value))
                    {
                        _torusRadius = value;
                        this.RaiseRecreateRequest();
                    }
                }
            }

            [Category(CATEGORY_NAME)]
            public float TubeRadius
            {
                get => _tubeRadius;
                set
                {
                    if (!EngineMath.EqualsWithTolerance(_tubeRadius, value))
                    {
                        _tubeRadius = value;
                        this.RaiseRecreateRequest();
                    }
                }
            }
        }
    }
}