using System;
using System.ComponentModel;
using System.Numerics;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing2D;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Drawing3D.Primitives;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer.Basics3D.Direct2DTexture
{
    [SampleDescription(
        "Direct2D Texture", 4, nameof(Basics3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Basics3D/Direct2DTexture",
        typeof(Direct2DTextureSampleSettings))]
    public class Direct2DTextureSample : SampleBase
    {
        private SolidBrushResource _solidBrush;
        private SolidBrushResource _textBrush;
        private TextFormatResource _textFormat;

        public override async Task OnStartupAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            mainRenderLoop.EnsureNotNull(nameof(mainRenderLoop));

            var castedSettings = settings as Direct2DTextureSampleSettings ?? new Direct2DTextureSampleSettings();

            // 2D rendering is made here
            _solidBrush = new SolidBrushResource(Color4.Gray);
            _textFormat = new TextFormatResource("Arial", 36);
            _textBrush = new SolidBrushResource(Color4.RedColor);

            var d2DDrawingLayer = new Custom2DDrawingLayer(graphics =>
            {
                var d2DRectangle = new RectangleF(10, 10, 236, 236);
                graphics.Clear(Color4.LightBlue);
                graphics.FillRoundedRectangle(
                    d2DRectangle, 30, 30,
                    _solidBrush);

                d2DRectangle.Inflate(-10, -10);
                graphics.DrawText(
                    castedSettings.DisplayText,
                    _textFormat, d2DRectangle, _textBrush);
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
                var resD2DMaterial = manipulator.AddStandardMaterialResource(resD2DTexture, enableShaderGeneratedBorder: true);

                // Create cube geometry resource
                var resGeometry = manipulator.AddResource(
                    device => new GeometryResource(new CubeGeometryFactory()));

                // Create cube object
                var cubeMesh = new Mesh(resGeometry, resD2DMaterial);
                cubeMesh.Color = Color4.GreenColor;
                cubeMesh.YPos = 0.5f;
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

            // Add object filter for viewbox culling
            mainOrChildRenderLoop.ObjectFilters.Add(new SceneViewboxObjectFilter());

            return Task.FromResult<object>(null);
        }

        public override void OnSampleClosed()
        {
            base.OnSampleClosed();

            SeeingSharpUtil.SafeDispose(ref _solidBrush);
            SeeingSharpUtil.SafeDispose(ref _textBrush);
            SeeingSharpUtil.SafeDispose(ref _textFormat);
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