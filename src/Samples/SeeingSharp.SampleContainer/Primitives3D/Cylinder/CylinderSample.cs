﻿using System.ComponentModel;
using System.Numerics;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Drawing3D.Primitives;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer.Primitives3D.Cylinder
{
    [SampleDescription(
        "Cylinder", 2, nameof(Primitives3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Primitives3D/Cylinder",
        typeof(CylinderSampleSettings))]
    public class CylinderSample : Primitive3DSampleBase
    {
        protected override Mesh CreateMesh(SceneManipulator manipulator, SampleSettings sampleSettings, NamedOrGenericKey resMaterial)
        {
            var castedSettings = (CylinderSampleSettings) sampleSettings;

            var resGeometry = manipulator.AddResource(
                device => new GeometryResource(
                    new CylinderGeometryFactory
                    { 
                        Radius = castedSettings.Radius,
                        Height = castedSettings.Height,
                        CountOfSegments = castedSettings.CountOfSegments
                    }));

            var result = new Mesh(resGeometry, resMaterial);
            result.Position = new Vector3(0f, 0.5f, 0f);
            return result;
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class CylinderSampleSettings : Primitive3DSampleSettings
        {
            private float _radius = 0.5f;
            private float _height = 1f;
            private int _countOfSegments = 10;

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