/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using System;
using System.ComponentModel;
using System.Numerics;
using System.Threading.Tasks;
using SeeingSharp.SampleContainer.Primitives3D._04_Geosphere;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer.Primitives3D._03_Sphere
{
    [SampleDescription(
        "Sphere", 3, nameof(Primitives3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Primitives3D/_03_Sphere",
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
            private int m_tDiv = 30;
            private int m_pDiv = 30;
            private float m_radius = 0.5f;

            [Category(CATEGORY_NAME)]
            public int TDiv
            {
                get => m_tDiv;
                set
                {
                    if (m_tDiv != value)
                    {
                        m_tDiv= value;
                        base.RaiseRecreateRequest();
                    }
                }
            }

            [Category(CATEGORY_NAME)]
            public int PDiv
            {
                get => m_pDiv;
                set
                {
                    if (m_pDiv != value)
                    {
                        m_pDiv= value;
                        base.RaiseRecreateRequest();
                    }
                }
            }

            [Category(CATEGORY_NAME)]
            public float Radius
            {
                get => m_radius;
                set
                {
                    if (!EngineMath.EqualsWithTolerance(m_radius, value))
                    {
                        m_radius = value;
                        base.RaiseRecreateRequest();
                    }
                }
            }
        }
    }
}