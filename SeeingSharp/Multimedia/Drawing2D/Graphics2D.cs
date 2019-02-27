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

#region using

// Some namespace mappings
using D2D = SharpDX.Direct2D1;
using DWrite = SharpDX.DirectWrite;
using SDXM = SharpDX.Mathematics.Interop;

#endregion

namespace SeeingSharp.Multimedia.Drawing2D
{
    #region using

    using System;
    using Checking;
    using Core;
    using SeeingSharp.Util;
    using SharpDX;

    #endregion

    public class Graphics2D
    {
        #region Main view related properties

        private D2D.RenderTarget m_renderTarget;
        private D2D.DeviceContext m_deviceContext;
        #endregion Main view related properties

        #region Transform settings
        private Graphics2DTransformSettings m_transformSettings;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Graphics2D"/> class.
        /// </summary>
        /// <param name="device">The hardware device which is used for rendering.</param>
        /// <param name="renderTarget">The render target which is used by this graphics object.</param>
        /// <param name="screenSize">The size of the screen in device independent pixels.</param>
        internal Graphics2D(EngineDevice device, D2D.RenderTarget renderTarget, Size2F screenSize)
        {
            m_transformSettings = Graphics2DTransformSettings.Default;
            TransformStack = new Matrix3x2Stack();

            Device = device;
            m_renderTarget = renderTarget;
            ScreenPixelSize = screenSize;
            m_deviceContext = m_renderTarget as D2D.DeviceContext;
        }

        /// <summary>
        /// Sets current transform settings on this graphics object.
        /// (be carefull, the state is changed on device level!)
        /// </summary>
        /// <param name="transformSettings">The settings to be set.</param>
        internal void PushTransformSettings(Graphics2DTransformSettings transformSettings)
        {
            m_transformSettings = transformSettings;

            switch(transformSettings.TransformMode)
            {
                    // Overtake given scaling matrix
                case Graphics2DTransformMode.Custom:
                    TransformStack.Push(transformSettings.CustomTransform);
                    break;

                    // Calculate scaling matrix here
                case Graphics2DTransformMode.AutoScaleToVirtualScreen:
                    float virtualWidth = m_transformSettings.VirtualScreenSize.Width;
                    float virtualHeight = m_transformSettings.VirtualScreenSize.Height;
                    if(virtualWidth == 0f) { virtualWidth = ScreenPixelSize.Width; }
                    if(virtualHeight == 0f) { virtualHeight = ScreenPixelSize.Height; }

                    float scaleFactorX = ScreenPixelSize.Width / virtualWidth;
                    float scaleFactorY = ScreenPixelSize.Height / virtualHeight;
                    float combinedScaleFactor = Math.Min(scaleFactorX, scaleFactorY);
                    float truePixelWidth = virtualWidth * combinedScaleFactor;
                    float truePixelHeight = virtualHeight * combinedScaleFactor;

                    TransformStack.Push();
                    TransformStack.TransformLocal(
                        Matrix3x2.Scaling(combinedScaleFactor) *
                        Matrix3x2.Translation(
                            ScreenPixelSize.Width / 2f - truePixelWidth / 2f,
                            ScreenPixelSize.Height / 2f - truePixelHeight / 2f));
                    break;

                default:
                    throw new SeeingSharpGraphicsException($"Unable to handle transform mode {transformSettings.TransformMode}");
            }

            // Apply current transform
            this.ApplyTransformStack();
        }

        /// <summary>
        /// Applies the top of the local transform stack.
        /// </summary>
        public unsafe void ApplyTransformStack()
        {
            if (m_renderTarget == null)
            {
                return;
            }

            var top = TransformStack.Top;
            m_renderTarget.Transform =
                *(SDXM.RawMatrix3x2*)&top;
        }

        /// <summary>
        /// Resets the transform setting son this graphics object.
        /// (be carefull, the state is changed on device level!)
        /// </summary>
        internal void PopTransformSettings()
        {
            TransformStack.Pop();

            this.ApplyTransformStack();
        }

        /// <summary>
        /// Pushes a new matrix to the TransformStack and pops it after Dispose has
        /// been called on the result object.
        /// </summary>
        /// <param name="customTransform">The custom transform matrix.</param>
        public IDisposable BlockForLocalTransform_ReplacePrevious(Matrix3x2 customTransform)
        {
            TransformStack.Push(customTransform);
            this.ApplyTransformStack();

            return new DummyDisposable(() =>
            {
                TransformStack.Pop();
                this.ApplyTransformStack();
            });
        }

        /// <summary>
        /// Transform current matrix locally with the given matrix.
        /// </summary>
        /// <param name="customTransform">The custom transform matrix.</param>
        public IDisposable BlockForLocalTransform_TransformLocal(Matrix3x2 customTransform)
        {
            TransformStack.Push();
            TransformStack.TransformLocal(customTransform);
            this.ApplyTransformStack();

            return new DummyDisposable(() =>
            {
                TransformStack.Pop();
                this.ApplyTransformStack();
            });
        }

        /// <summary>
        /// Clears the current view.
        /// </summary>
        /// <param name="clearColor">Color of the clear.</param>
        public void Clear(Color4 clearColor)
        {
            if (m_renderTarget == null) { return; }

            m_renderTarget.Clear(clearColor);
        }

        /// <summary>
        /// Draws the given geometry.
        /// </summary>
        public void DrawGeometry(Geometry2DResourceBase geometry, BrushResource brush, float strokeWidth = 1f)
        {
            if (m_renderTarget == null) { return; }

            geometry.EnsureNotNullOrDisposed(nameof(geometry));
            brush.EnsureNotNull(nameof(brush));
            strokeWidth.EnsurePositive(nameof(strokeWidth));

            m_renderTarget.DrawGeometry(
                geometry.GetGeometry(),
                brush.GetBrush(Device),
                strokeWidth);
        }

        /// <summary>
        /// Draws the given rectangle with the given brush.
        /// </summary>
        public void DrawRectangle(RectangleF rectangle, BrushResource brush, float strokeWidth = 1f)
        {
            if (m_renderTarget == null) { return; }

            brush.EnsureNotNull(nameof(brush));
            rectangle.EnsureNotEmpty(nameof(rectangle));
            strokeWidth.EnsurePositive(nameof(strokeWidth));

            m_renderTarget.DrawRectangle(
                rectangle,
                brush.GetBrush(Device),
                strokeWidth);
        }

        /// <summary>
        /// Draws the given rounded rectangle with the given brush.
        /// </summary>
        public void DrawRoundedRectangle(RectangleF rectangle, float radiusX, float radiusY, BrushResource brush, float strokeWidth = 1f)
        {
            if (m_renderTarget == null)
            {
                return;
            }

            rectangle.EnsureNotEmpty(nameof(rectangle));
            brush.EnsureNotNull(nameof(brush));
            radiusX.EnsurePositive(nameof(radiusX));
            radiusY.EnsurePositive(nameof(radiusY));
            strokeWidth.EnsurePositive(nameof(strokeWidth));

            var roundedRect = new D2D.RoundedRectangle
            {
                Rect = rectangle,
                RadiusX = radiusX,
                RadiusY = radiusY
            };

            m_renderTarget.DrawRoundedRectangle(
                roundedRect,
                brush.GetBrush(Device),
                strokeWidth);
        }

        /// <summary>
        /// Draws the given ellipse with the given brush.
        /// </summary>
        public void DrawEllipse(Vector2 center, float radiusX, float radiusY, BrushResource brush, float strokeWidth = 1f)
        {
            var ellipse = new D2D.Ellipse(center, radiusX, radiusY);

            m_renderTarget.DrawEllipse(
                ellipse,
                brush.GetBrush(Device),
                strokeWidth);
        }

        /// <summary>
        /// Draws the given ellipse with the given brush.
        /// </summary>
        public void DrawEllipse(RectangleF rectangle, BrushResource brush, float strokeWidth = 1f)
        {
            float radiusX = (rectangle.Width / 2f);
            float radiusY = (rectangle.Height / 2f);
            var center = new Vector2(
                rectangle.X + radiusX,
                rectangle.Y + radiusY);
            var ellipse = new D2D.Ellipse(center, radiusX, radiusY);

            m_renderTarget.DrawEllipse(
                ellipse,
                brush.GetBrush(Device),
                strokeWidth);
        }

        /// <summary>
        /// Draws a point at the given location.
        /// </summary>
        public void DrawPoint(Vector2 point, BrushResource brush)
        {
            this.DrawLine(
                point, new Vector2(point.X + 1f, point.Y),
                brush);
        }

        /// <summary>
        /// Draws the given line with the given brush.
        /// </summary>
        public void DrawLine(Vector2 start, Vector2 end, BrushResource brush, float strokeWidth = 1f)
        {
            if (m_renderTarget == null) { return; }

            brush.EnsureNotNull(nameof(brush));
            strokeWidth.EnsurePositiveAndNotZero(nameof(strokeWidth));

            m_renderTarget.DrawLine(
                start, end,
                brush.GetBrush(Device),
                strokeWidth);
        }

        /// <summary>
        /// Fills the given rectangle with the given brush object.
        /// </summary>
        /// <param name="rectangle">The rectangle to be filled.</param>
        /// <param name="brush">The brush to be used.</param>
        public void FillRectangle(RectangleF rectangle, BrushResource brush)
        {
            if (m_renderTarget == null) { return; }

            rectangle.EnsureNotEmpty(nameof(rectangle));
            brush.EnsureNotNull(nameof(brush));

            m_renderTarget.FillRectangle(
                rectangle,
                brush.GetBrush(Device));
        }

        /// <summary>
        /// Fills the given geometry.
        /// </summary>
        public void FillGeometry(Geometry2DResourceBase geometry, BrushResource brush)
        {
            if (m_renderTarget == null) { return; }

            geometry.EnsureNotNullOrDisposed(nameof(geometry));
            brush.EnsureNotNull(nameof(brush));

            m_renderTarget.FillGeometry(
                geometry.GetGeometry(),
                brush.GetBrush(Device));
        }

        /// <summary>
        /// Fills the given geometry.
        /// </summary>
        public void FillGeometry(Geometry2DResourceBase geometry, BrushResource brush, BrushResource opacityBrush)
        {
            if (m_renderTarget == null) { return; }

            geometry.EnsureNotNullOrDisposed(nameof(geometry));
            brush.EnsureNotNull(nameof(brush));

            m_renderTarget.FillGeometry(
                geometry.GetGeometry(),
                brush.GetBrush(Device),
                opacityBrush.GetBrush(Device));
        }

        /// <summary>
        /// Fills the given rectangle with the given brush object.
        /// </summary>
        /// <param name="radiusX">The x radius of the rectangle's corners.</param>
        /// <param name="radiusY">The y radius of the rectangle's corners.</param>
        /// <param name="rectangle">The rectangle to be filled.</param>
        /// <param name="brush">The brush to be used.</param>
        public void FillRoundedRectangle(RectangleF rectangle, float radiusX, float radiusY, BrushResource brush)
        {
            if (m_renderTarget == null)
            {
                return;
            }

            rectangle.EnsureNotEmpty(nameof(rectangle));
            brush.EnsureNotNull(nameof(brush));
            radiusX.EnsurePositive(nameof(radiusX));
            radiusY.EnsurePositive(nameof(radiusY));

            var roundedRect = new D2D.RoundedRectangle
            {
                Rect = rectangle,
                RadiusX = radiusX,
                RadiusY = radiusY
            };

            m_renderTarget.FillRoundedRectangle(
                roundedRect,
                brush.GetBrush(Device));
        }

        /// <summary>
        /// Fills the given ellipse with the given brush.
        /// </summary>
        public void FillEllipse(Vector2 center, float radiusX, float radiusY, BrushResource brush)
        {
            var ellipse = new D2D.Ellipse(center, radiusX, radiusY);

            m_renderTarget.FillEllipse(
                ellipse,
                brush.GetBrush(Device));
        }

        /// <summary>
        /// Fills the given ellipse with the given brush.
        /// </summary>
        public void FillEllipse(RectangleF rectangle, BrushResource brush)
        {
            float radiusX = (rectangle.Width / 2f);
            float radiusY = (rectangle.Height / 2f);
            var center = new Vector2(
                rectangle.X + radiusX,
                rectangle.Y + radiusY);
            var ellipse = new D2D.Ellipse(center, radiusX, radiusY);

            m_renderTarget.FillEllipse(
                ellipse,
                brush.GetBrush(Device));
        }

        /// <summary>
        /// Draws the given text on the screen.
        /// </summary>
        /// <param name="textToDraw">The text to draw.</param>
        /// <param name="textFormat">The TextFormat to be used.</param>
        /// <param name="targetRectangle">The target rectangle.</param>
        /// <param name="brush">The brush.</param>
        /// <param name="drawOptions">Some draw options to be passed to Direct2D.</param>
        /// <param name="measuringMode">Sets the measuring mode to be passed to Direct2D.</param>
        public void DrawText(
            string textToDraw, TextFormatResource textFormat, RectangleF targetRectangle, BrushResource brush,
            DrawTextOptions drawOptions = DrawTextOptions.None,
            MeasuringMode measuringMode = MeasuringMode.Natural)
        {
            if (m_renderTarget == null) { return; }

            textToDraw.EnsureNotNull(nameof(textToDraw));
            targetRectangle.EnsureNotEmpty(nameof(targetRectangle));
            brush.EnsureNotNull(nameof(brush));

            var drawOptionsD2D = (D2D.DrawTextOptions)drawOptions;
            var measuringModeD2D = (D2D.MeasuringMode)measuringMode;

            m_renderTarget.DrawText(
                textToDraw,
                textFormat.GetTextFormat(Device),
                targetRectangle,
                brush.GetBrush(Device),
                drawOptionsD2D);
        }

        /// <summary>
        /// Draws the given bitmap.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="destinationRectangle">The target rectangle where to draw the bitmap.</param>
        /// <param name="opacity">The opacity.</param>
        /// <param name="interpolationMode">The interpolation mode.</param>
        /// <param name="frameIndex">The frame of the bitmap to be rendered.</param>
        public void DrawBitmap(
            BitmapResource bitmap,
            RectangleF destinationRectangle,
            float opacity = 1f,
            BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.NearestNeighbor,
            int frameIndex = 0)
        {
            if (m_renderTarget == null) { return; }

            bitmap.EnsureNotNull(nameof(bitmap));
            destinationRectangle.EnsureNotEmpty(nameof(destinationRectangle));
            opacity.EnsureInRange(0f, 1f, nameof(opacity));

            int bitmapFrameCount = bitmap.TotalFrameCount;
            frameIndex.EnsureInRange(0, bitmapFrameCount - 1, nameof(frameIndex));

            // Render the bitmap
            if (bitmapFrameCount > 1)
            {
                // Get the native bitmap object first
                // (if not, we may not have loaded it already and therefore
                //  missing size information)
                var nativeBitmap = bitmap.GetBitmap(Device);

                // Calculate source rectangle
                int framesX = bitmap.FrameCountX;
                int xFrameIndex = frameIndex % framesX;
                int yFrameIndex = (frameIndex - xFrameIndex) / framesX;
                int singleFrameWidth = bitmap.SingleFramePixelWidth;
                int singleFrameHeight = bitmap.SingleFramePixelHeight;
                var sourceRectangle = new RectangleF(
                    xFrameIndex * singleFrameWidth,
                    yFrameIndex * singleFrameHeight,
                    singleFrameWidth, singleFrameHeight);

                // Render tiled bitmap
                m_renderTarget.DrawBitmap(
                    nativeBitmap,
                    destinationRectangle,
                    opacity,
                    (D2D.BitmapInterpolationMode)interpolationMode,
                    sourceRectangle);
            }
            else
            {
                // Render non-tiled bitmap
                m_renderTarget.DrawBitmap(
                    bitmap.GetBitmap(Device),
                    destinationRectangle,
                    opacity,
                    (D2D.BitmapInterpolationMode)interpolationMode);
            }
        }

        /// <summary>
        /// Draws the given bitmap.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="destinationOrigin">The point where to start rendering.</param>
        /// <param name="opacity">The opacity.</param>
        /// <param name="interpolationMode">The interpolation mode.</param>
        /// <param name="frameIndex">The frame of the bitmap to be rendered.</param>
        public void DrawBitmap(
            BitmapResource bitmap,
            Vector2 destinationOrigin,
            float opacity = 1f,
            BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.NearestNeighbor,
            int frameIndex = 0)
        {
            if (m_renderTarget == null) { return; }

            bitmap.EnsureNotNull(nameof(bitmap));
            opacity.EnsureInRange(0f, 1f, nameof(opacity));

            int bitmapFrameCount = bitmap.TotalFrameCount;
            frameIndex.EnsureInRange(0, bitmapFrameCount - 1, nameof(frameIndex));

            // Render the bitmap
            if (bitmapFrameCount > 1)
            {
                // Get the native bitmap object first
                // (if not, we may not have loaded it already and therefore
                //  missing size information)
                var nativeBitmap = bitmap.GetBitmap(Device);

                // Calculate destination rectangle
                int singleFrameWidth = bitmap.SingleFramePixelWidth;
                int singleFrameHeight = bitmap.SingleFramePixelHeight;
                var destinationRectangle = new RectangleF(
                    destinationOrigin.X, destinationOrigin.Y,
                    singleFrameWidth, singleFrameHeight);

                // Calculate source rectangle
                int framesX = bitmap.FrameCountX;
                int xFrameIndex = frameIndex % framesX;
                int yFrameIndex = (frameIndex - xFrameIndex) / framesX;
                var sourceRectangle = new RectangleF(
                    xFrameIndex * singleFrameWidth,
                    yFrameIndex * singleFrameHeight,
                    singleFrameWidth, singleFrameHeight);

                // Render tiled bitmap
                m_renderTarget.DrawBitmap(
                    nativeBitmap,
                    destinationRectangle,
                    opacity,
                    (D2D.BitmapInterpolationMode)interpolationMode,
                    sourceRectangle);
            }
            else
            {
                var destinationRectangle = new SharpDX.Mathematics.Interop.RawRectangleF(
                    destinationOrigin.X, destinationOrigin.Y,
                    destinationOrigin.X + bitmap.PixelWidth, destinationOrigin.Y + bitmap.PixelHeight);

                // Render non-tiled bitmap
                m_renderTarget.DrawBitmap(
                    bitmap.GetBitmap(Device),
                    destinationRectangle,
                    opacity,
                    (D2D.BitmapInterpolationMode)interpolationMode);
            }
        }

        /// <summary>
        /// Draws the given image.
        /// </summary>
        /// <param name="image">The source of pixel data to be rendered.</param>
        /// <param name="destinationOrigin">The origin point where to draw the image.</param>
        public void DrawImage(
            IImage image,
            Vector2 destinationOrigin)
        {
            if (m_renderTarget == null) { return; }

            image.EnsureNotNull(nameof(image));

            var internalImage = image as IImageInternal;
            internalImage.EnsureNotNull(nameof(internalImage));

            if (m_deviceContext != null)
            {
                var d2dImage = internalImage.GetImageObject(Device) as D2D.Image;
                d2dImage.EnsureNotNull(nameof(d2dImage));

                m_deviceContext.DrawImage(
                    d2dImage,
                    destinationOrigin,
                    null,
                    D2D.InterpolationMode.Linear,
                    D2D.CompositeMode.SourceOver);
            }
            else
            {
                var bitmap = internalImage.TryGetSourceBitmap();

                if(bitmap != null)
                {
                    this.DrawBitmap(bitmap, destinationOrigin);
                }
            }
        }

        /// <summary>
        /// Gets the device which is used for rendering.
        /// </summary>
        public EngineDevice Device { get; }

        /// <summary>
        /// Gets the bounds of the screen.
        /// </summary>
        public RectangleF ScreenBounds
        {
            get
            {
                Size2F screenSize = this.ScreenSize;
                return new RectangleF(
                    0f, 0f,
                    screenSize.Width, screenSize.Height);
            }
        }

        /// <summary>
        /// Gets the total size of pixels (already scaled by DPI).
        /// </summary>
        public Size2F ScreenPixelSize { get; }

        /// <summary>
        /// Gets the total size of this screen.
        /// This value may be a virtual screen size (see TransformMode).
        /// </summary>
        public Size2F ScreenSize
        {
            get
            {
                switch (m_transformSettings.TransformMode)
                {
                    case Graphics2DTransformMode.AutoScaleToVirtualScreen:
                        return m_transformSettings.VirtualScreenSize;

                    default:
                        return ScreenPixelSize;
                }

            }
        }

        /// <summary>
        /// Gets the width of the screen.
        /// This value may be a virtual screen size (see TransformMode).
        /// </summary>
        public float ScreenWidth
        {
            get
            {
                switch(m_transformSettings.TransformMode)
                {
                    case Graphics2DTransformMode.AutoScaleToVirtualScreen:
                        return m_transformSettings.VirtualScreenSize.Width;

                    default:
                        return ScreenPixelSize.Width;
                }
            }
        }

        /// <summary>
        /// Gets the height of the screen.
        /// This value may be a virtual screen size (see TransformMode).
        /// </summary>
        public float ScreenHeight
        {
            get
            {
                switch (m_transformSettings.TransformMode)
                {
                    case Graphics2DTransformMode.AutoScaleToVirtualScreen:
                        return m_transformSettings.VirtualScreenSize.Height;

                    default:
                        return ScreenPixelSize.Height;
                }

            }
        }

        /// <summary>
        /// Gets the current transform stack.
        /// Call 'ApplyTransformStack' to apply the current top matrix of this stack.
        /// </summary>
        public Matrix3x2Stack TransformStack { get; }

        public Matrix3x2 Transform
        {
            get
            {
                if (m_renderTarget == null) { return Matrix3x2.Identity; }
                return m_renderTarget.Transform;
            }
            set
            {
                if (m_renderTarget == null) { return; }
                m_renderTarget.Transform = value;
            }
        }
    }
}