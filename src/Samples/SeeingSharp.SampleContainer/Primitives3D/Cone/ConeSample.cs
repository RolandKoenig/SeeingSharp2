using System.ComponentModel;
using System.Numerics;
using SeeingSharp.Core;
using SeeingSharp.Drawing3D;
using SeeingSharp.Drawing3D.Primitives;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer.Primitives3D.Cone
{
    [SampleDescription(
        "Cone", 3, nameof(Primitives3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/src/Samples/SeeingSharp.SampleContainer/Primitives3D/Cone",
        typeof(ConeSampleSettings))]
    public class ConeSample : Primitive3DSampleBase
    {
        protected override Mesh CreateMesh(SceneManipulator manipulator, SampleSettings sampleSettings, NamedOrGenericKey resMaterial)
        {
            var castedSettings = (ConeSampleSettings) sampleSettings;

            var resGeometry = manipulator.AddResource(
                device => new GeometryResource(
                    new ConeGeometryFactory
                    { 
                        Radius = castedSettings.Radius,
                        Height = castedSettings.Height,
                        CountOfSegments = castedSettings.CountOfSegments
                    }));

            var result = new Mesh(resGeometry, resMaterial);
            result.Position = new Vector3(0f, 0.5f + castedSettings.Height / 2f, 0f);
            return result;
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class ConeSampleSettings : Primitive3DSampleSettings
        {
            private float _radius;
            private float _height;
            private int _countOfSegments;

            public ConeSampleSettings()
            {
                // Set defaults
                var geoFactory = new ConeGeometryFactory();
                _radius = geoFactory.Radius;
                _height = geoFactory.Height;
                _countOfSegments = geoFactory.CountOfSegments;
            }

            [Category(CATEGORY_NAME)]
            public float Radius
            {
                get => _radius;
                set
                {
                    if (!EngineMath.EqualsWithTolerance(_radius, value))
                    {
                        _radius = value;
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
            public int CountOfSegments
            {
                get => _countOfSegments;
                set
                {
                    if (_countOfSegments != value)
                    {
                        _countOfSegments = value;
                        this.RaiseRecreateRequest();
                    }
                }
            }
        }
    }
}