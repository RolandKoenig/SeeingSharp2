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
using SeeingSharp.Util;
using System;
using System.ComponentModel;
using System.Numerics;
using System.Threading.Tasks;

namespace SeeingSharp.SampleContainer.MassScenes._01_StaticCubes
{
    [SampleDescription(
        "Static Cubes", 1, nameof(MassScenes),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/MassScenes/_01_StaticCubes",
        typeof(StaticCubesSampleSettings))]
    public class StaticCubesSample : SampleBase
    {
        public override Task OnStartupAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            mainRenderLoop.EnsureNotNull(nameof(mainRenderLoop));

            return Task.FromResult<object>(null);
        }

        public override Task OnInitRenderingWindowAsync(RenderLoop mainOrChildRenderLoop)
        {
            var camera = mainOrChildRenderLoop.Camera;

            // ConfigureLoading camera
            camera.Position = new Vector3(40f, 30f, 40f);
            camera.Target = new Vector3(0f, 5f, 0f);
            camera.UpdateCamera();

            // Append camera behavior
            mainOrChildRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());

            return Task.FromResult<object>(null);
        }

        public override async Task OnReloadAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            var castedSettings = (StaticCubesSampleSettings)settings;

            await mainRenderLoop.Scene.ManipulateSceneAsync(manipulator =>
            {
                // Clear previous scene
                manipulator.Clear();

                // Create floor
                this.BuildStandardFloor(
                    manipulator, Scene.DEFAULT_LAYER_NAME);

                // Create resources
                var resGeometry = manipulator.AddGeometryResource(
                    new CubeGeometryFactory());
                var resColoredMaterial = manipulator.AddStandardMaterialResource();
                var resTexture = manipulator.AddTextureResource(
                    new AssemblyResourceLink(this.GetType(),
                        "SimpleTexture.png"));
                var resTexturedMaterial = manipulator.AddStandardMaterialResource(resTexture);

                // Create cubes
                var sideLength = castedSettings.CubeCountPerSide;
                var randomizer = new Random();
                for (var loopX = 0; loopX < sideLength; loopX++)
                {
                    for (var loopY = 0; loopY < sideLength; loopY++)
                    {
                        for (var loopZ = 0; loopZ < sideLength; loopZ++)
                        {
                            // Choose material
                            var material = resColoredMaterial;
                            if (castedSettings.HalfTextured)
                            {
                                if ((loopX + loopY + loopZ) % 2 == 1)
                                {
                                    material = resTexturedMaterial;
                                }
                            }

                            var cubeMesh = new Mesh(resGeometry, material);
                            cubeMesh.Color = Color4.GreenColor;
                            cubeMesh.Position = new Vector3(loopX * 1.5f, loopY * 1.5f, loopZ * 1.5f);
                            cubeMesh.EnableShaderGeneratedBorder();

                            if (castedSettings.Animated)
                            {
                                cubeMesh.BuildAnimationSequence()
                                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_180DEG, 0f),
                                        TimeSpan.FromSeconds(1.0 + randomizer.NextDouble() * 2.0))
                                    .WaitFinished()
                                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_360DEG, 0f),
                                        TimeSpan.FromSeconds(1.0 + randomizer.NextDouble() * 2.0))
                                    .WaitFinished()
                                    .CallAction(() => cubeMesh.RotationEuler = Vector3.Zero)
                                    .ApplyAndRewind();
                            }

                            manipulator.AddObject(cubeMesh);
                        }
                    }
                }
            });
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class StaticCubesSampleSettings : SampleSettingsWith3D
        {
            private int m_cubeCountPerSide = 15;
            private bool m_animated;
            private bool m_halfTextured;

            [Category("Static Cubes")]
            public int CubeCountPerSide
            {
                get => m_cubeCountPerSide;
                set
                {
                    var givenValue = value;
                    if (givenValue < 1) { givenValue = 1; }
                    if (givenValue > 50) { givenValue = 50; }

                    m_cubeCountPerSide = givenValue;
                    base.RaiseRecreateRequest();
                }
            }

            [Category("Static Cubes")]
            public bool Animated
            {
                get => m_animated;
                set
                {
                    m_animated = value;
                    base.RaiseRecreateRequest();
                }
            }

            [Category("Static Cubes")]
            public bool HalfTextured
            {
                get => m_halfTextured;
                set
                {
                    m_halfTextured = value;
                    base.RaiseRecreateRequest();
                }
            }
        }
    }
}