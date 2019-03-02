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

using System;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Objects;
using SharpDX;

namespace SeeingSharp.SampleContainer.Basics3D._04_Text3D
{
    [SampleDescription(
        "Text 3D", 4, nameof(Basics3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Basics3D/_04_Text3D")]
    public class Text3DSample : SampleBase
    {
        /// <summary>
        ///     Called when the sample has to startup.
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
                BuildStandardFloor(
                    manipulator, Scene.DEFAULT_LAYER_NAME);

                // Configure text geometry
                var textOptions = TextGeometryOptions.Default;
                textOptions.FontSize = 50;
                textOptions.MakeVolumetricText = true;
                textOptions.SurfaceVertexColor = Color.Blue;
                textOptions.VolumetricSideSurfaceVertexColor = Color4Ex.CornflowerBlue;

                // Create text geometry and object
                var textObject = manipulator.Add3DText($"Seeing# 2 {Environment.NewLine} Text3D Sample", textOptions);
                textObject.YPos = textOptions.VolumetricTextDepth;
            });

            // Configure camera
            camera.Position = new Vector3(0.7f, 8.5f, -15f);
            camera.RelativeTarget = new Vector3(0.44f, -0.62f, 0.64f);
            camera.UpdateCamera();

            // Append camera behavior
            targetRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());
        }
    }
}