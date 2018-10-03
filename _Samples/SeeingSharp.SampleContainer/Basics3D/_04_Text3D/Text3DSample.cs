﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Util;
using SharpDX;

namespace SeeingSharp.SampleContainer.Basics3D._04_Text3D
{
    [SampleDescription(
        "Text 3D", 4, nameof(SeeingSharp.SampleContainer.Basics3D),
        sampleImageFileName: "PreviewImage.png",
        sourceCodeUrl: "https://github.com/RolandKoenig/SeeingSharp2/tree/master/_Samples/SeeingSharp.SampleContainer/Basics3D/_04_Text3D")]
    public class Text3DSample : SampleBase
    {
        /// <summary>
        /// Called when the sample has to startup.
        /// </summary>
        /// <param name="targetRenderLoop">The target render loop.</param>
        public override async Task OnStartupAsync(RenderLoop targetRenderLoop)
        {
            targetRenderLoop.EnsureNotNull(nameof(targetRenderLoop));

            // Build dummy scene
            Scene scene = targetRenderLoop.Scene;
            Camera3DBase camera = targetRenderLoop.Camera as Camera3DBase;

            await targetRenderLoop.Scene.ManipulateSceneAsync((manipulator) =>
            {
                // Create floor
                base.BuildStandardFloor(
                    manipulator, Scene.DEFAULT_LAYER_NAME);

                TextGeometryOptions textOptions = TextGeometryOptions.Default;
                textOptions.FontSize = 50;
                textOptions.MakeVolumetricText = true;
                textOptions.SurfaceVertexColor = Color.Blue;
                textOptions.VolumetricSideSurfaceVertexColor = Color4Ex.CornflowerBlue;

                GenericObject textObject = manipulator.Add3DText($"Seeing# 2 {Environment.NewLine} Text3D Sample", textOptions);
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
