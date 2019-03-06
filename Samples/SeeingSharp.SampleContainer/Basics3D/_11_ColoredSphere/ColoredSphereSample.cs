#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
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

namespace SeeingSharp.SampleContainer.Basics3D._11_ColoredSphere
{
    #region using

    using System;
    using System.Threading.Tasks;
    using Checking;
    using Multimedia.Components;
    using Multimedia.Core;
    using Multimedia.Drawing3D;
    using Multimedia.Objects._ObjectTypes;
    using SharpDX;

    #endregion

    [SampleDescription(
        "Colored Sphere", 1, nameof(Basics3D),
        sampleImageFileName: "PreviewImage.png",
        sourceCodeUrl: "https://github.com/RolandKoenig/SeeingSharp2/tree/master/_Samples/SeeingSharp.SampleContainer/Basics3D/_11_ColoredSphere")]
    public class ColoredSphereSample : SampleBase
    {
        public override async Task OnStartupAsync(RenderLoop targetRenderLoop, SampleSettings settings)
        {
            targetRenderLoop.EnsureNotNull(nameof(targetRenderLoop));

            // Build dummy scene
            var scene = targetRenderLoop.Scene;
            var camera = targetRenderLoop.Camera;

            await targetRenderLoop.Scene.ManipulateSceneAsync((manipulator) =>
            {
                // Create floor
                BuildStandardFloor(
                    manipulator, Scene.DEFAULT_LAYER_NAME);

                // Create Sphere geometry resource
                var sphereType = new SphereType
                {
                    tDiv = 30,
                    pDiv = 30,
                    Radius = 1
                };

                var resPalletGeometry = manipulator.AddResource(
                    () => new GeometryResource(sphereType));

                // Create Sphere object
                var sphereObject = manipulator.AddGeneric(resPalletGeometry);
                sphereObject.Color = Color4Ex.GreenColor;
                sphereObject.Position = new Vector3(0f, 2.5f, 0f);
                sphereObject.EnableShaderGeneratedBorder();
                sphereObject.BuildAnimationSequence()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_180DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_360DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .CallAction(() => sphereObject.RotationEuler = Vector3.Zero)
                    .ApplyAndRewind();
            });

            // Configure camera
            camera.Position = new Vector3(3f, 3f, 3f);
            camera.Target = new Vector3(0f, 2.5f, 0f);
            camera.UpdateCamera();

            // Append camera behavior
            targetRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());
        }
    }
}