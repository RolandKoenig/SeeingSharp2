/*
    SeeingSharp and all applications distributed together with it. 
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
using System.Threading.Tasks;
using System.Numerics;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using D2D = SharpDX.Direct2D1;

namespace SeeingSharp.SampleContainer.Basics3D.ExtrudeGeometry
{
    [SampleDescription(
        "Extrude Geometry", 7, nameof(Basics3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Basics3D/ExtrudeGeometry",
        typeof(SampleSettingsWith3D))]
    public class ExtrudeGeometrySample : SampleBase
    {
        public override async Task OnStartupAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            mainRenderLoop.EnsureNotNull(nameof(mainRenderLoop));

            await mainRenderLoop.Scene.ManipulateSceneAsync(manipulator =>
            {
                // Create floor
                this.BuildStandardFloor(
                    manipulator, Scene.DEFAULT_LAYER_NAME);

                // Create material
                var resMaterial = manipulator.AddStandardMaterialResource();

                // Create geometry resource
                ExtrudeGeometryFactory geometryFactory = null;
                using (var pathGeo = new D2D.PathGeometry1(GraphicsCore.Current.Internals.FactoryD2D))
                {
                    // We are building the left mountain from this sample:
                    //  https://docs.microsoft.com/en-us/windows/desktop/direct2d/path-geometries-overview
                    var geoSink = pathGeo.Open();
                    geoSink.BeginFigure(
                        new SharpDX.Vector2(346f, 255f),
                        D2D.FigureBegin.Filled);
                    geoSink.AddLine(new SharpDX.Vector2(267f, 177f));
                    geoSink.AddLine(new SharpDX.Vector2(236f, 192f));
                    geoSink.AddLine(new SharpDX.Vector2(212f, 160f));
                    geoSink.AddLine(new SharpDX.Vector2(156f, 255f));
                    geoSink.EndFigure(D2D.FigureEnd.Closed);
                    geoSink.Close();

                    // Create the GeometryFactory
                    // We can dispose the PathGeometry after that, because the ExtrudeGeometryFactory
                    // extracts all information needed within the constructor
                    geometryFactory = new ExtrudeGeometryFactory();
                    geometryFactory.Internals.SetGeometry(
                        pathGeo, 0.1f,
                        ExtrudeGeometryOptions.RescaleToUnitSize | ExtrudeGeometryOptions.ChangeOriginToCenter);
                }
                var resGeometry = manipulator.AddGeometryResource(geometryFactory);

                // Create the 3D object
                var extrudedMesh = new Mesh(resGeometry, resMaterial);
                extrudedMesh.Color = Color4.GreenColor;
                extrudedMesh.Position = new Vector3(0f, 0.5f, 0f);
                extrudedMesh.Scaling = new Vector3(2f, 1f, 2f);
                manipulator.AddObject(extrudedMesh);
            });
        }

        public override Task OnInitRenderingWindowAsync(RenderLoop mainOrChildRenderLoop)
        {
            var camera = mainOrChildRenderLoop.Camera;

            // ConfigureLoading camera
            camera.Position = new Vector3(3f, 3f, 3f);
            camera.Target = new Vector3(0f, 0.5f, 0f);
            camera.UpdateCamera();

            // Append camera behavior
            mainOrChildRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());

            // Add object filter for viewbox culling
            mainOrChildRenderLoop.ObjectFilters.Add(new SceneViewboxObjectFilter());

            return Task.FromResult<object>(null);
        }
    }
}