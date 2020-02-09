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
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing2D;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;
using System;
using System.ComponentModel;
using System.Numerics;
using System.Threading.Tasks;

namespace SeeingSharp.SampleContainer.Basics3D._04_Direct2DTexture
{
    [SampleDescription(
        "Direct2D Texture", 4, nameof(Basics3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Basics3D/_04_Direct2DTexture",
        typeof(Direct2DTextureSampleSettings))]
    public class Direct2DTextureSample : SampleBase
    {
        private SolidBrushResource m_solidBrush;
        private SolidBrushResource m_textBrush;
        private TextFormatResource m_textFormat;

        public override async Task OnStartupAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            mainRenderLoop.EnsureNotNull(nameof(mainRenderLoop));

            var castedSettings = settings as Direct2DTextureSampleSettings ?? new Direct2DTextureSampleSettings();

            // 2D rendering is made here
            m_solidBrush = new SolidBrushResource(Color4.Gray);
            m_textFormat = new TextFormatResource("Arial", 36);
            m_textBrush = new SolidBrushResource(Color4.RedColor);

            var d2DDrawingLayer = new Custom2DDrawingLayer(graphics =>
            {
                var d2DRectangle = new RectangleF(10, 10, 236, 236);
                graphics.Clear(Color4.LightBlue);
                graphics.FillRoundedRectangle(
                    d2DRectangle, 30, 30,
                    m_solidBrush);

                d2DRectangle.Inflate(-10, -10);
                graphics.DrawText(
                    castedSettings.DisplayText,
                    m_textFormat, d2DRectangle, m_textBrush);
            });

            // Build 3D scene
            await mainRenderLoop.Scene.ManipulateSceneAsync(manipulator =>
            {
                // Create floor
                this.BuildStandardFloor(
                    manipulator, Scene.DEFAULT_LAYER_NAME);

                // Define Direct2D texture resource
                var resD2DTexture = manipulator.AddResource(
                    device => new Direct2DTextureResource(d2DDrawingLayer, 256, 256));
                var resD2DMaterial = manipulator.AddStandardMaterialResource(resD2DTexture);

                // Create cube geometry resource
                var resGeometry = manipulator.AddResource(
                    device => new GeometryResource(new CubeGeometryFactory()));

                // Create cube object
                var cubeMesh = new Mesh(resGeometry, resD2DMaterial);
                cubeMesh.Color = Color4.GreenColor;
                cubeMesh.YPos = 0.5f;
                cubeMesh.EnableShaderGeneratedBorder();
                cubeMesh.BuildAnimationSequence()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_180DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_360DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .CallAction(() => cubeMesh.RotationEuler = Vector3.Zero)
                    .ApplyAndRewind();
                manipulator.AddObject(cubeMesh);
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

            return Task.FromResult<object>(null);
        }

        public override void OnClosed()
        {
            base.OnClosed();

            SeeingSharpUtil.SafeDispose(ref m_solidBrush);
            SeeingSharpUtil.SafeDispose(ref m_textBrush);
            SeeingSharpUtil.SafeDispose(ref m_textFormat);
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class Direct2DTextureSampleSettings : SampleSettingsWith3D
        {
            [Category("Direct2D Texture")]
            public string DisplayText { get; set; } = "Hello Direct2D!";
        }
    }
}