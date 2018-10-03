using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
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
        "Direct2D Texture", 6, nameof(SeeingSharp.SampleContainer.Basics3D),
        sampleImageFileName:"PreviewImage.png",
        sourceCodeUrl: "https://github.com/RolandKoenig/SeeingSharp2/tree/master/_Samples/SeeingSharp.SampleContainer/Basics3D/_06_Direct2DTexture",
        settingsType: typeof(Direct2DTextureSampleSettings))]
    public class Direct2DTextureSample : SampleBase
    {
        private SolidBrushResource m_solidBrush;
        private TextFormatResource m_textFormat;
        private SolidBrushResource m_textBrush;

        /// <summary>
        /// Called when the sample has to startup.
        /// </summary>
        public override async Task OnStartupAsync(RenderLoop targetRenderLoop, SampleSettings settings)
        {
            targetRenderLoop.EnsureNotNull(nameof(targetRenderLoop));

            Direct2DTextureSampleSettings castedSettings = settings as Direct2DTextureSampleSettings;
            if(castedSettings == null) { castedSettings = new Direct2DTextureSampleSettings(); }

            // Build dummy scene
            Scene scene = targetRenderLoop.Scene;
            Camera3DBase camera = targetRenderLoop.Camera as Camera3DBase;

            // 2D rendering is made here
            m_solidBrush = new SolidBrushResource(Color4Ex.Gray);
            m_textFormat = new TextFormatResource("Arial", 36);
            m_textBrush = new SolidBrushResource(Color4Ex.RedColor);
            Custom2DDrawingLayer d2dDrawingLayer = new Custom2DDrawingLayer((graphics) =>
            {
                RectangleF d2dRectangle = new RectangleF(10, 10, 236, 236);
                graphics.Clear(Color4Ex.LightBlue);
                graphics.FillRoundedRectangle(
                    d2dRectangle, 30, 30,
                    m_solidBrush);

                d2dRectangle.Inflate(-10, -10);
                graphics.DrawText(
                    castedSettings.DisplayText.Replace("\\n", Environment.NewLine), 
                    m_textFormat, d2dRectangle, m_textBrush);
            });

            // Build 3D scene
            await targetRenderLoop.Scene.ManipulateSceneAsync((manipulator) =>
            {
                // Create floor
                base.BuildStandardFloor(
                    manipulator, Scene.DEFAULT_LAYER_NAME);

                // Define Direct2D texture resource
                var resD2DTexture = manipulator.AddResource<Direct2DTextureResource>(
                    () => new Direct2DTextureResource(d2dDrawingLayer, 256, 256));
                var resD2DMaterial = manipulator.AddSimpleColoredMaterial(resD2DTexture);

                // Create pallet geometry resource
                var pType = new CubeType();
                pType.Material = resD2DMaterial;
                var resPalletGeometry = manipulator.AddResource<GeometryResource>(
                    () => new GeometryResource(pType));

                // Create pallet object
                GenericObject cubeObject = manipulator.AddGeneric(resPalletGeometry);
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
            public string DisplayText
            {
                get;
                set;
            } = "Hello Direct2D!";
        }
    }
}
