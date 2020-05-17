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

using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer.Basics3D._06_GeneratedBorder
{
    [SampleDescription(
        "Generated Border", 6, nameof(Basics3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Basics3D/_06_GeneratedBorder",
        typeof(GeneratedBorderSettings))]
    public class GeneratedBorderSample : SampleBase
    {
        private GeneratedBorderSettings _settings;
        private List<Mesh> _cubes = new List<Mesh>();
        private NamedOrGenericKey _resBorderMaterial;

        private Scene _scene;
        private volatile bool _isManipulatingInUpdate;

        public override async Task OnStartupAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            mainRenderLoop.EnsureNotNull(nameof(mainRenderLoop));

            _scene = mainRenderLoop.Scene;
            _settings = (GeneratedBorderSettings)settings;

            await mainRenderLoop.Scene.ManipulateSceneAsync(manipulator =>
            {
                // Create floor
                this.BuildStandardFloor(
                    manipulator, Scene.DEFAULT_LAYER_NAME);

                // Create resources
                var resGeometry = manipulator.AddResource(
                    device => new GeometryResource(new CubeGeometryFactory()));
                var resMaterial = manipulator.AddStandardMaterialResource();

                // Create a separate material for the cubes with borders
                _resBorderMaterial = manipulator.AddStandardMaterialResource();

                // Create cubes with border
                const float SPACE = 1.05f;
                for (var loop = 0; loop < 10; loop++)
                {
                    var cubeMesh = new Mesh(resGeometry, _resBorderMaterial);
                    cubeMesh.Color = Color4.GreenColor;
                    cubeMesh.Position = new Vector3(0f, 0.5f, loop * SPACE);
                    manipulator.AddObject(cubeMesh);
                    _cubes.Add(cubeMesh);

                    cubeMesh = new Mesh(resGeometry, _resBorderMaterial);
                    cubeMesh.Color = Color4.GreenColor;
                    cubeMesh.Position = new Vector3(-SPACE, 0.5f, loop * SPACE);
                    manipulator.AddObject(cubeMesh);
                    _cubes.Add(cubeMesh);
                }

                // Create cubes without border
                for (var loop = 0; loop < 10; loop++)
                {
                    var cubeMesh = new Mesh(resGeometry, resMaterial);
                    cubeMesh.Color = Color4.GreenColor;
                    cubeMesh.Position = new Vector3(SPACE, 0.5f, loop * SPACE);
                    manipulator.AddObject(cubeMesh);

                    cubeMesh = new Mesh(resGeometry, resMaterial);
                    cubeMesh.Color = Color4.GreenColor;
                    cubeMesh.Position = new Vector3(2 * SPACE, 0.5f, loop * SPACE);
                    manipulator.AddObject(cubeMesh);
                }
            });
        }

        public override Task OnInitRenderingWindowAsync(RenderLoop mainOrChildRenderLoop)
        {
            var camera = mainOrChildRenderLoop.Camera;

            // ConfigureLoading camera
            camera.Position = new Vector3(-3f, 3f, -3f);
            camera.Target = new Vector3(1f, -0.5f, 2f);
            camera.UpdateCamera();

            // Append camera behavior
            mainOrChildRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());

            // Add object filter for viewbox culling
            mainOrChildRenderLoop.ObjectFilters.Add(new SceneViewboxObjectFilter());

            return Task.FromResult<object>(null);
        }

        public override async void Update()
        {
            // This flag is simply guarding following scene manipulation. It ensures that scene manipulation runs only ones
            if (_isManipulatingInUpdate) { return; }
            _isManipulatingInUpdate = true;
            
            try
            {
                // Do changes on loaded materials using the scene manipulator to ensure thread safety
                await _scene.ManipulateSceneAsync(manipulator =>
                {
                    // Query over all loaded resources
                    // (This single resource may be loaded on different devices)
                    foreach (var actLoadedMaterial in manipulator.QueryResources<StandardMaterialResource>(
                        _resBorderMaterial))
                    {
                        if (_settings.BorderEnabled)
                        {
                            actLoadedMaterial.EnableShaderGeneratedBorder(_settings.BorderThickness);
                        }
                        else
                        {
                            actLoadedMaterial.DisableShaderGeneratedBorder();
                        }
                    }
                });
            }
            finally
            {
                // Reset the float so we call scene manipulate in the next update pass again
                _isManipulatingInUpdate = false;
            }
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class GeneratedBorderSettings : SampleSettingsWith3D
        {
            [Category("Generated Border")]
            public bool BorderEnabled { get; set; } = true;

            [Category("Generated Border")]
            public float BorderThickness { get; set; } = 2f;
        }
    }
}