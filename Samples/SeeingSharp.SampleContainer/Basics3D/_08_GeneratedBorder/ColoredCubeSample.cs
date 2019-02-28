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
namespace SeeingSharp.SampleContainer.Basics3D._08_GeneratedBorder
{
    #region using

    using System.Threading.Tasks;
    using Checking;
    using Multimedia.Components;
    using Multimedia.Core;
    using Multimedia.Drawing3D;
    using Multimedia.Objects;
    using SharpDX;

    #endregion

    [SampleDescription(
        "Generated Border", 8, nameof(SeeingSharp.SampleContainer.Basics3D),
        sampleImageFileName:"PreviewImage.png",
        sourceCodeUrl: "https://github.com/RolandKoenig/SeeingSharp2/tree/master/_Samples/SeeingSharp.SampleContainer/Basics3D/_08_GeneratedBorder")]
    public class GeneratedBorderSample : SampleBase
    {
        /// <summary>
        /// Called when the sample has to startup.
        /// </summary>
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

                // Create pallet geometry resource
                var cubeType = new CubeType();
                var resPalletGeometry = manipulator.AddResource<GeometryResource>(
                    () => new GeometryResource(cubeType));

                float space = 1.05f;

                // Create cubes with border
                for (var loop = 0; loop < 10; loop++)
                {
                    var cubeObject = manipulator.AddGeneric(resPalletGeometry);
                    cubeObject.Color = Color4Ex.GreenColor;
                    cubeObject.Position = new Vector3(0f, 0.5f, loop * space);
                    cubeObject.EnableShaderGeneratedBorder(borderThicknes: 2f);

                    cubeObject = manipulator.AddGeneric(resPalletGeometry);
                    cubeObject.Color = Color4Ex.GreenColor;
                    cubeObject.Position = new Vector3(-space, 0.5f, loop * space);
                    cubeObject.EnableShaderGeneratedBorder(borderThicknes: 2f);
                }

                // Create cubes without border
                for (var loop = 0; loop < 10; loop++)
                {
                    var cubeObject = manipulator.AddGeneric(resPalletGeometry);
                    cubeObject.Color = Color4Ex.GreenColor;
                    cubeObject.Position = new Vector3(space, 0.5f, loop * space);

                    cubeObject = manipulator.AddGeneric(resPalletGeometry);
                    cubeObject.Color = Color4Ex.GreenColor;
                    cubeObject.Position = new Vector3(2* space, 0.5f, loop * space);
                }
            });

            // Configure camera
            camera.Position = new Vector3(-3f, 3f, -3f);
            camera.Target = new Vector3(1f, -0.5f, 2f);
            camera.UpdateCamera();

            // Append camera behavior
            targetRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());
        }
    }
}