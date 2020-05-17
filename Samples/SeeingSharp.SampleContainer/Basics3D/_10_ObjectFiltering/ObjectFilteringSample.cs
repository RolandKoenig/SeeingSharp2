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

using System.ComponentModel;
using System.Numerics;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;

namespace SeeingSharp.SampleContainer.Basics3D._10_ObjectFiltering
{
    [SampleDescription(
        "Object Filtering", 10, nameof(Basics3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Basics3D/_10_ObjectFiltering",
        typeof(ObjectFilteringSettings))]
    public class ObjectFilteringSample : SampleBase
    {
        private ObjectFilteringSettings _settings;

        public override async Task OnStartupAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            mainRenderLoop.EnsureNotNull(nameof(mainRenderLoop));

            _settings = (ObjectFilteringSettings) settings;

            await mainRenderLoop.Scene.ManipulateSceneAsync(manipulator =>
            {
                // Create floor
                this.BuildStandardFloor(
                    manipulator, Scene.DEFAULT_LAYER_NAME);

                // Create material
                var resMaterial = manipulator.AddStandardMaterialResource();

                // Create resources
                var resGeometryCube = manipulator.AddResource(
                    device => new GeometryResource(new CubeGeometryFactory()));
                var resGeometrySphere = manipulator.AddResource(
                    device => new GeometryResource(new GeosphereGeometryFactory()));
                var resGeometryPyramid = manipulator.AddResource(
                    device => new GeometryResource(new PyramidGeometryFactory()));

                // Create all objects and write a simple type name into the Tag1 property
                // We will use this Tag1 property for filter visible objects by the object type
                for (var loop = 0; loop < 3; loop++)
                {
                    var newMesh = new Mesh(resGeometrySphere, resMaterial);
                    newMesh.Color = Color4.BlueColor;
                    newMesh.Position = new Vector3(-1.5f, 1f, -1.5f + loop * 1.5f);
                    newMesh.Tag1 = "Sphere";
                    manipulator.AddObject(newMesh);

                    newMesh = new Mesh(resGeometryCube, resMaterial);
                    newMesh.Color = Color4.GreenColor;
                    newMesh.Position = new Vector3(0f, 1f, -1.5f + loop * 1.5f);
                    newMesh.Tag1 = "Cube";
                    manipulator.AddObject(newMesh);

                    newMesh = new Mesh(resGeometryPyramid, resMaterial);
                    newMesh.Color = Color4.RedColor;
                    newMesh.Position = new Vector3(1.5f, 1f, -1.5f + loop * 1.5f);
                    newMesh.Tag1 = "Pyramid";
                    manipulator.AddObject(newMesh);
                }
            });
        }

        public override Task OnInitRenderingWindowAsync(RenderLoop mainOrChildRenderLoop)
        {
            var camera = mainOrChildRenderLoop.Camera;

            // ConfigureLoading camera
            camera.Position = new Vector3(6f, 6f, 6f);
            camera.Target = new Vector3(0f, 0.5f, 0f);
            camera.UpdateCamera();

            // Append camera behavior
            mainOrChildRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());
            
            // Register our custom filter (first step) and the filter for viewbox culling (second step)
            mainOrChildRenderLoop.ObjectFilters.Add(_settings.Filter);
            mainOrChildRenderLoop.ObjectFilters.Add(new SceneViewboxObjectFilter());

            return Task.FromResult<object>(null);
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class ObjectFilteringSettings : SampleSettingsWith3D
        {
            [Category("Object Filtering")]
            public bool ShowSpheres
            {
                get => this.Filter.ShowSpheres;
                set => this.Filter.ShowSpheres = value;
            }

            [Category("Object Filtering")]
            public bool ShowCubes
            {
                get => this.Filter.ShowCubes;
                set => this.Filter.ShowCubes = value;
            }

            [Category("Object Filtering")]
            public bool ShowPyramids
            {
                get => this.Filter.ShowPyramids;
                set => this.Filter.ShowPyramids = value;
            }

            [Browsable(false)]
            public CustomByObjectFilter Filter { get; } = new CustomByObjectFilter();
        }

        /// <summary>
        /// Our custom filter implementation for this sample.
        /// </summary>
        private class CustomByObjectFilter : SceneObjectFilter
        {
            private bool _showSpheres = true;
            private bool _showCubes = true;
            private bool _showPyramids = true;

            public bool ShowSpheres
            {
                get => _showSpheres;
                set
                {
                    _showSpheres = value;
                    this.NotifyFilterConfigurationChanged();
                }
            }

            public bool ShowCubes
            {
                get => _showCubes;
                set
                {
                    _showCubes = value;
                    this.NotifyFilterConfigurationChanged();
                }
            }

            public bool ShowPyramids
            {
                get => _showPyramids;
                set
                {
                    _showPyramids = value;
                    this.NotifyFilterConfigurationChanged();
                }
            }

            /// <inheritdoc />
            public override bool IsObjectVisible(SceneObject input, ViewInformation viewInformation)
            {
                var objType = input.Tag1 as string;
                if (string.IsNullOrEmpty(objType)) { return true; }

                switch (objType)
                {
                    case "Sphere":
                        return _showSpheres;

                    case "Cube":
                        return _showCubes;
                    
                    case "Pyramid":
                        return _showPyramids;

                    default:
                        return false;
                }
            }
        }
    }
}