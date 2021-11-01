using System.ComponentModel;
using System.Numerics;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
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
                        TorusDiameter = castedSettings.TorusDiameter,
                        TubeDiameter = castedSettings.TubeDiameter,
                    }));

            var result = new Mesh(resGeometry, resMaterial);
            result.Position = new Vector3(0f, 0.5f + (castedSettings.TubeDiameter / 2f) + (castedSettings.TorusDiameter / 2f), 0f);
            return result;
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class TorusSampleSettings : Primitive3DSampleSettings
        {
            private int _tDiv = 20;
            private int _pDiv = 12;
            private float _torusDiameter = 1f;
            private float _tubeDiameter = 0.2f;

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
            public float TorusDiameter
            {
                get => _torusDiameter;
                set
                {
                    if (!EngineMath.EqualsWithTolerance(_torusDiameter, value))
                    {
                        _torusDiameter = value;
                        this.RaiseRecreateRequest();
                    }
                }
            }

            [Category(CATEGORY_NAME)]
            public float TubeDiameter
            {
                get => _tubeDiameter;
                set
                {
                    if (!EngineMath.EqualsWithTolerance(_tubeDiameter, value))
                    {
                        _tubeDiameter = value;
                        this.RaiseRecreateRequest();
                    }
                }
            }
        }
    }
}