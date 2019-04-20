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

using System;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Objects;
using SharpDX;

namespace SeeingSharp.SampleContainer.Postprocessing._01_EdgeDetect
{
    [SampleDescription(
        "Edge detect", 1, nameof(Postprocessing),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Postprocessing/_01_EdgeDetect",
        typeof(SampleSettingsWith3D))]
    public class EdgeDetectSample : SampleBase
    {
        /// <summary>
        /// Called when the sample has to startup.
        /// </summary>
        public override async Task OnStartupAsync(RenderLoop targetRenderLoop, SampleSettings settings)
        {
            targetRenderLoop.EnsureNotNull(nameof(targetRenderLoop));

            // Build dummy scene
            var scene = targetRenderLoop.Scene;
            var camera = targetRenderLoop.Camera;

            await targetRenderLoop.Scene.ManipulateSceneAsync(manipulator =>
            {
                // Create floor
                this.BuildStandardFloor(
                    manipulator, Scene.DEFAULT_LAYER_NAME);

                // Create edge detect resource
                var resEdgeDetect = manipulator.AddResource(() => new EdgeDetectPostprocessEffectResource
                {
                    BorderColor = Color4Ex.BlueColor,
                    Thickness = 5f
                });

                var edgeLayer = manipulator.AddLayer("EdgeDetectLayer");
                edgeLayer.PostprocessEffectKey = resEdgeDetect;

                // Create resources
                var resGeometry = manipulator.AddResource(
                    () => new GeometryResource(
                        new CubeGeometryFactory()));
                var resMaterial = manipulator.AddSimpleColoredMaterial();

                // Create object and put it on the EdgeDetectLayer
                var cubeMesh = new Mesh(resGeometry, resMaterial);
                cubeMesh.Color = Color4Ex.GreenColor;
                cubeMesh.Position = new Vector3(0f, 0.5f, 0f);
                cubeMesh.EnableShaderGeneratedBorder();
                cubeMesh.BuildAnimationSequence()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_180DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_360DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .CallAction(() => cubeMesh.RotationEuler = Vector3.Zero)
                    .ApplyAndRewind();
                manipulator.Add(cubeMesh, "EdgeDetectLayer");
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
