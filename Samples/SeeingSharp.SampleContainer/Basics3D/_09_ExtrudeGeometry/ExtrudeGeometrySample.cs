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

using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Multimedia.Objects._GeometryFactories;
using SharpDX;
using SharpDX.Direct2D1;

namespace SeeingSharp.SampleContainer.Basics3D._09_ExtrudeGeometry
{
    [SampleDescription(
        "Extrude Geometry", 9, nameof(Basics3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Basics3D/_09_ExtrudeGeometry",
        typeof(SampleSettingsWith3D))]
    public class ExtrudeGeometrySample : SampleBase
    {
        /// <summary>
        /// Called when the sample has to startup.
        /// </summary>
        public override async Task OnStartupAsync(RenderLoop targetRenderLoop, SampleSettings settings)
        {
            targetRenderLoop.EnsureNotNull(nameof(targetRenderLoop));

            // Get scene and camera
            var scene = targetRenderLoop.Scene;
            var camera = targetRenderLoop.Camera;

            await targetRenderLoop.Scene.ManipulateSceneAsync(manipulator =>
            {
                // Create floor
                BuildStandardFloor(
                    manipulator, Scene.DEFAULT_LAYER_NAME);

                // Create geometry resource
                ExtrudeGeometryFactory geometryFactory = null;
                using (var pathGeo = new PathGeometry1(GraphicsCore.Current.FactoryD2D))
                {
                    // We are building the left mountain from this sample:
                    //  https://docs.microsoft.com/en-us/windows/desktop/direct2d/path-geometries-overview
                    var geoSink = pathGeo.Open();
                    geoSink.BeginFigure(
                        new Vector2(346f, 255f), 
                        FigureBegin.Filled);
                    geoSink.AddLine(new Vector2(267f, 177f));
                    geoSink.AddLine(new Vector2(236f, 192f));
                    geoSink.AddLine(new Vector2(212f, 160f));
                    geoSink.AddLine(new Vector2(156f, 255f));
                    geoSink.EndFigure(FigureEnd.Closed);
                    geoSink.Close();

                    // Create the GeometryFactory
                    // We can dispose the PathGeometry after that, because the ExtrudeGeometryFactory
                    // extracts all information needed within the constructor
                    geometryFactory = new ExtrudeGeometryFactory(
                        pathGeo, 0.1f, 
                        ExtrudeGeometryOptions.RescaleToUnitSize | ExtrudeGeometryOptions.ChangeOriginToCenter);
                }
                var resGeometry = manipulator.AddGeometry(geometryFactory);

                // Create the 3D object
                var extrudedObject = manipulator.AddGeneric(resGeometry);
                extrudedObject.Color = Color4Ex.GreenColor;
                extrudedObject.Position = new Vector3(0f, 0.5f, 0f);
                extrudedObject.Scaling = new Vector3(2f, 1f, 2f);
            });

            // Configure camera
            camera.Position = new Vector3(3f, 3f, 3f);
            camera.Target = new Vector3(0f, 0.5f, 0f);
            camera.UpdateCamera();

            // Append camera behavior
            targetRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());
        }
    }
}