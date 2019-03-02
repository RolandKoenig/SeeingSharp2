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
using System.ComponentModel;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing2D;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Util;
using SharpDX;

namespace SeeingSharp.SampleContainer.Basics3D._06_Direct2DTexture
{
    [SampleDescription(
        "Direct2D Texture", 6, nameof(Basics3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Basics3D/_06_Direct2DTexture",
        typeof(Direct2DTextureSampleSettings))]
    public class Direct2DTextureSample : SampleBase
    {
        private SolidBrushResource m_solidBrush;
        private SolidBrushResource m_textBrush;
        private TextFormatResource m_textFormat;

        /// <summary>
        ///     Called when the sample has to startup.
        /// </summary>
        public override async Task OnStartupAsync(RenderLoop targetRenderLoop, SampleSettings settings)
        {
            targetRenderLoop.EnsureNotNull(nameof(targetRenderLoop));

            var castedSettings = settings as Direct2DTextureSampleSettings ?? new Direct2DTextureSampleSettings();
            
            // Build dummy scene
            var scene = targetRenderLoop.Scene;
            var camera = targetRenderLoop.Camera;

            // 2D rendering is made here
            m_solidBrush = new SolidBrushResource(Color4Ex.Gray);
            m_textFormat = new TextFormatResource("Arial", 36);
            m_textBrush = new SolidBrushResource(Color4Ex.RedColor);

            var d2dDrawingLayer = new Custom2DDrawingLayer(graphics =>
            {
                var d2dRectangle = new RectangleF(10, 10, 236, 236);
                graphics.Clear(Color4Ex.LightBlue);
                graphics.FillRoundedRectangle(
                    d2dRectangle, 30, 30,
                    m_solidBrush);

                d2dRectangle.Inflate(-10, -10);
                graphics.DrawText(
                    castedSettings.DisplayText,
                    m_textFormat, d2dRectangle, m_textBrush);
            });

            // Build 3D scene
            await targetRenderLoop.Scene.ManipulateSceneAsync(manipulator =>
            {
                // Create floor
                BuildStandardFloor(
                    manipulator, Scene.DEFAULT_LAYER_NAME);

                // Define Direct2D texture resource
                var resD2DTexture = manipulator.AddResource(
                    () => new Direct2DTextureResource(d2dDrawingLayer, 256, 256));
                var resD2DMaterial = manipulator.AddSimpleColoredMaterial(resD2DTexture);

                // Create cube geometry resource
                var cubeGeometry = new CubeGeometryFactory
                {
                    Material = resD2DMaterial
                };

                var resPalletGeometry = manipulator.AddResource(
                    () => new GeometryResource(cubeGeometry));

                // Create cube object
                var cubeObject = manipulator.AddGeneric(resPalletGeometry);
                cubeObject.Color = Color4Ex.GreenColor;
                cubeObject.YPos = 0.5f;
                cubeObject.EnableShaderGeneratedBorder();
                cubeObject.BuildAnimationSequence()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_180DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_360DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .CallAction(() => cubeObject.RotationEuler = Vector3.Zero)
                    .ApplyAndRewind();
            });

            // Configure camera
            camera.Position = new Vector3(3f, 3f, 3f);
            camera.Target = new Vector3(0f, 0.5f, 0f);
            camera.UpdateCamera();

            // Append camera behavior
            targetRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());
        }

        public override void NotifyClosed()
        {
            base.NotifyClosed();

            SeeingSharpUtil.SafeDispose(ref m_solidBrush);
            SeeingSharpUtil.SafeDispose(ref m_textBrush);
            SeeingSharpUtil.SafeDispose(ref m_textFormat);
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class Direct2DTextureSampleSettings : SampleSettings
        {
            [Category("Direct2D Texture")]
            public string DisplayText { get; } = "Hello Direct2D!";
        }
    }
}