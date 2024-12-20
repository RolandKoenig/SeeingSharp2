﻿using System.ComponentModel;
using System.Numerics;
using SeeingSharp.Core;
using SeeingSharp.Drawing3D;
using SeeingSharp.Drawing3D.Primitives;
using SeeingSharp.Drawing3D.Resources;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer.Primitives3D.Sphere
{
    [SampleDescription(
        "Sphere", 4, nameof(Primitives3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/src/Samples/SeeingSharp.SampleContainer/Primitives3D/Sphere",
        typeof(SphereSampleSettings))]
    public class SphereSample : Primitive3DSampleBase
    {
        protected override Mesh CreateMesh(SceneManipulator manipulator, SampleSettings sampleSettings, NamedOrGenericKey resMaterial)
        {
            var castedSettings = (SphereSampleSettings) sampleSettings;

            var resGeometry = manipulator.AddResource(
                device => new GeometryResource(
                    new SphereGeometryFactory
                    {
                        TDiv = castedSettings.TDiv,
                        PDiv = castedSettings.PDiv, 
                        Radius = castedSettings.Radius
                    }));

            var result = new Mesh(resGeometry, resMaterial);
            result.Position = new Vector3(0f, 0.5f + castedSettings.Radius, 0f);
            return result;
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class SphereSampleSettings : Primitive3DSampleSettings
        {
            private int _tDiv;
            private int _pDiv;
            private float _radius;

            public SphereSampleSettings()
            {
                // Set defaults
                var geoFactory = new SphereGeometryFactory();
                _tDiv = geoFactory.TDiv;
                _pDiv = geoFactory.PDiv;
                _radius = geoFactory.Radius;
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
        }
    }
}