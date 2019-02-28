#region License information
/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
#endregion
namespace SeeingSharp.SampleContainer.Primitives3D._01_ColoredCylinder
{
    #region using

    using System;
    using System.Threading.Tasks;
    using Checking;
    using Multimedia.Components;
    using Multimedia.Core;
    using Multimedia.Drawing3D;
    using Multimedia.Objects;
    using SharpDX;

    #endregion

    [SampleDescription(
        "Colored Cylinder", 1, nameof(Primitives3D),
        sampleImageFileName: "PreviewImage.png",
        sourceCodeUrl: "https://github.com/RolandKoenig/SeeingSharp2/tree/master/_Samples/SeeingSharp.SampleContainer/Primitives3D/_01_ColoredCylinder")]
    public class ColoredCylinderSample : SampleBase
    {
        public override async Task OnStartupAsync(RenderLoop targetRenderLoop, SampleSettings settings)
        {
            targetRenderLoop.EnsureNotNull(nameof(targetRenderLoop));

            // Build dummy scene
            var scene = targetRenderLoop.Scene;
            var camera = targetRenderLoop.Camera as Camera3DBase;

            await targetRenderLoop.Scene.ManipulateSceneAsync((manipulator) =>
            {
                // Create floor
                base.BuildStandardFloor(
                    manipulator, Scene.DEFAULT_LAYER_NAME);

                // Create cylinder geometry resource
                var cylinderType = new CylinderType { CountOfSegments = 50 };

                var resPalletGeometry = manipulator.AddResource<GeometryResource>(
                    () => new GeometryResource(cylinderType));

                // Create cylinder object
                var cylinderObject = manipulator.AddGeneric(resPalletGeometry);
                cylinderObject.Color = Color4Ex.BlueColor;
                cylinderObject.Position = new Vector3(0f, 0.5f, 0f);
                cylinderObject.EnableShaderGeneratedBorder();
                cylinderObject.BuildAnimationSequence()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_180DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_360DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .CallAction(() => cylinderObject.RotationEuler = Vector3.Zero)
                    .ApplyAndRewind();
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