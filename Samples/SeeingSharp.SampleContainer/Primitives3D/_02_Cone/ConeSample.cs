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
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer.Primitives3D._02_Cone
{
    [SampleDescription(
        "Cone", 2, nameof(Primitives3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Primitives3D/_02_Cone",
        typeof(ConeSampleSettings))]
    public class ConeSample : Primitive3DSampleBase
    {
        protected override Mesh CreateMesh(SceneManipulator manipulator, SampleSettings sampleSettings, NamedOrGenericKey resMaterial)
        {
            var castedSettings = (ConeSampleSettings) sampleSettings;

            var resGeometry = manipulator.AddResource(
                device => new GeometryResource(
                    new ConeGeometryFactory()
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
        private class ConeSampleSettings : Primitive3DSampleSettings
        {
            private float m_radius = 0.5f;
            private float m_height = 1f;
            private int m_countOfSegments = 10;

            [Category(CATEGORY_NAME)]
            public float Radius
            {
                get => m_radius;
                set
                {
                    if (!EngineMath.EqualsWithTolerance(m_radius, value))
                    {
                        m_radius = value;
                        this.RaiseRecreateRequest();
                    }
                }
            }

            [Category(CATEGORY_NAME)]
            public float Height
            {
                get => m_height;
                set
                {
                    if (!EngineMath.EqualsWithTolerance(m_height, value))
                    {
                        m_height = value;
                        this.RaiseRecreateRequest();
                    }
                }
            }

            [Category(CATEGORY_NAME)]
            public int CountOfSegments
            {
                get => m_countOfSegments;
                set
                {
                    if (m_countOfSegments != value)
                    {
                        m_countOfSegments = value;
                        this.RaiseRecreateRequest();
                    }
                }
            }
        }
    }
}