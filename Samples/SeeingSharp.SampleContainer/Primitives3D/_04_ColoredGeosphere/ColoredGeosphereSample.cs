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
using SeeingSharp.Multimedia.Objects;
using System;
using System.ComponentModel;
using System.Numerics;
using System.Threading.Tasks;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer.Primitives3D._04_ColoredGeosphere
{
    [SampleDescription(
        "Colored Geosphere", 4, nameof(Primitives3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Primitives3D/_04_ColoredGeosphere",
        typeof(ColoredGeosphereSampleSettings))]
    public class ColoredGeosphereSample : SampleBase
    {
        private ColoredGeosphereSampleSettings m_sampleSettings;

        public override Task OnStartupAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            mainRenderLoop.EnsureNotNull(nameof(mainRenderLoop));

            return Task.FromResult<object>(null);
        }

        public override async Task OnReloadAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            mainRenderLoop.EnsureNotNull(nameof(mainRenderLoop));

            m_sampleSettings = (ColoredGeosphereSampleSettings) settings;

            await mainRenderLoop.Scene.ManipulateSceneAsync(manipulator =>
            {
                // Clear previous scene
                manipulator.Clear();

                // Create floor
                this.BuildStandardFloor(
                    manipulator, Scene.DEFAULT_LAYER_NAME);

                // Create geometry resource
                var resGeometry = manipulator.AddResource(
                    device => new GeometryResource(
                        new GeosphereGeometryFactory
                        {
                            CountSubdivisions = m_sampleSettings.CountSubdivisions,
                            Radius = m_sampleSettings.Radius
                        }));

                // Create material resource
                NamedOrGenericKey resMaterial;
                if (m_sampleSettings.Textured)
                {
                    var resTexture = manipulator.AddTextureResource(
                        new AssemblyResourceLink(this.GetType(),
                            "SimpleTexture.png"));
                    resMaterial = manipulator.AddStandardMaterialResource(resTexture);
                }
                else
                {
                    resMaterial = manipulator.AddStandardMaterialResource();
                }

                // Create Sphere object
                var sphereMesh = new Mesh(resGeometry, resMaterial);
                sphereMesh.Color = Color4.GreenColor;
                sphereMesh.Position = new Vector3(0f, 0.5f + m_sampleSettings.Radius, 0f);
                sphereMesh.BuildAnimationSequence()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_180DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_360DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .CallAction(() => sphereMesh.RotationEuler = Vector3.Zero)
                    .ApplyAndRewind();
                manipulator.AddObject(sphereMesh);
            });
        }

        public override Task OnInitRenderingWindowAsync(RenderLoop mainOrChildRenderLoop)
        {
            var camera = mainOrChildRenderLoop.Camera;

            // Configure camera
            camera.Position = new Vector3(3f, 3f, 3f);
            camera.Target = new Vector3(0f, 0.5f, 0f);
            camera.UpdateCamera();

            // Append camera behavior
            mainOrChildRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());

            return Task.FromResult<object>(null);
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class ColoredGeosphereSampleSettings : SampleSettingsWith3D
        {
            private int m_countSubdivisions = 3;
            private float m_radius = 0.5f;
            private bool m_textured = false;

            [Category("Primitive")]
            public int CountSubdivisions
            {
                get => m_countSubdivisions;
                set
                {
                    if (m_countSubdivisions != value)
                    {
                        m_countSubdivisions = value;
                        base.RaiseRecreateRequest();
                    }
                }
            }

            [Category("Primitive")]
            public float Radius
            {
                get => m_radius;
                set
                {
                    if (m_radius != value)
                    {
                        m_radius = value;
                        base.RaiseRecreateRequest();
                    }
                }
            }

            [Category("Primitive")]
            public bool Textured
            {
                get => m_textured;
                set
                {
                    if (m_textured != value)
                    {
                        m_textured = value;
                        base.RaiseRecreateRequest();
                    }
                }
            }
        }
    }
}