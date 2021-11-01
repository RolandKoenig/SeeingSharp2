﻿using System.ComponentModel;
using System.Numerics;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer.Primitives3D.Geosphere
{
    [SampleDescription(
        "Geosphere", 5, nameof(Primitives3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Primitives3D/Geosphere",
        typeof(GeosphereSampleSettings))]
    public class GeosphereSample : Primitive3DSampleBase
    {
        protected override Mesh CreateMesh(SceneManipulator manipulator, SampleSettings sampleSettings, NamedOrGenericKey resMaterial)
        {
            var castedSettings = (GeosphereSampleSettings) sampleSettings;

            var resGeometry = manipulator.AddResource(
                device => new GeometryResource(
                    new GeosphereGeometryFactory
                    {
                        CountSubdivisions = castedSettings.CountSubdivisions,
                        Radius = castedSettings.Radius
                    }));

            var result = new Mesh(resGeometry, resMaterial);
            result.Position = new Vector3(0f, 0.5f + castedSettings.Radius, 0f);
            return result;
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class GeosphereSampleSettings : Primitive3DSampleSettings
        {
            private int _countSubdivisions = 3;
            private float _radius = 0.5f;

            [Category(CATEGORY_NAME)]
            public int CountSubdivisions
            {
                get => _countSubdivisions;
                set
                {
                    if (_countSubdivisions != value)
                    {
                        _countSubdivisions = value;
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