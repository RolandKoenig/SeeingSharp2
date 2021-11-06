using System;
using System.Numerics;
using SeeingSharp.Checking;
using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;
using D2D = SharpDX.Direct2D1;
using SDXM = SharpDX.Mathematics.Interop;

namespace SeeingSharp.Drawing2D
{
    public class Graphics2D
    {
        // Transform settings
        private Graphics2DTransformSettings _transformSettings;

        // Main view related properties
        private D2D.RenderTarget _renderTarget;
        private D2D.DeviceContext _deviceContext;

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
                var screenSize = this.ScreenSize;
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
                switch (_transformSettings.TransformMode)
                {
                    case Graphics2DTransformMode.AutoScaleToVirtualScreen:
                        return _transformSettings.VirtualScreenSize;

                    default:
                        return this.ScreenPixelSize;
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
                switch (_transformSettings.TransformMode)
                {
                    case Graphics2DTransformMode.AutoScaleToVirtualScreen:
                        return _transformSettings.VirtualScreenSize.Width;

                    default:
                        return this.ScreenPixelSize.Width;
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
                switch (_transformSettings.TransformMode)
                {
                    case Graphics2DTransformMode.AutoScaleToVirtualScreen:
                        return _transformSettings.VirtualScreenSize.Height;

                    default:
                        return this.ScreenPixelSize.Height;
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
                if (_renderTarget == null) { return Matrix3x2.Identity; }
                return SdxMathHelper.Matrix3x2FromRaw(_renderTarget.Transform);
            }
            set
            {
                if (_renderTarget == null) { return; }
                _renderTarget.Transform = SdxMathHelper.RawFromMatrix3x2(value);
            }
        }

        public Graphics2DInternals Internals { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Graphics2D"/> class.
        /// </summary>
        /// <param name="device">The hardware device which is used for rendering.</param>
        /// <param name="renderTarget">The render target which is used by this graphics object.</param>
        /// <param name="screenSize">The size of the screen in device independent pixels.</param>
        internal Graphics2D(EngineDevice device, D2D.RenderTarget renderTarget, Size2F screenSize)
        {
            _transformSettings = Graphics2DTransformSettings.Default;
            this.TransformStack = new Matrix3x2Stack();

            this.Device = device;
            _renderTarget = renderTarget;
            this.ScreenPixelSize = screenSize;
            _deviceContext = _renderTarget as D2D.DeviceContext;

            this.Internals = new Graphics2DInternals(this);
        }

        /// <summary>
        /// Applies the top of the local transform stack.
        /// </summary>
        public unsafe void ApplyTransformStack()
        {
            if (_renderTarget == null)
            {
                return;
            }

            var top = this.TransformStack.Top;
            _renderTarget.Transform =
                *(SDXM.RawMatrix3x2*)&top;
        }

        /// <summary>
        /// Pushes a new matrix to the TransformStack and pops it after Dispose has
        /// been called on the result object.
        /// </summary>
        /// <param name="customTransform">The custom transform matrix.</param>
        public IDisposable BlockForLocalTransfor_ReplacePrevious(Matrix3x2 customTransform)
        {
            this.TransformStack.Push(customTransform);
            this.ApplyTransformStack();

            return new DummyDisposable(() =>
            {
                this.TransformStack.Pop();
                this.ApplyTransformStack();
            });
        }

        /// <summary>
        /// Transform current matrix locally with the given matrix.
        /// </summary>
        /// <param name="customTransform">The custom transform matrix.</param>
        public IDisposable BlockForLocalTransfor_TransformLocal(Matrix3x2 customTransform)
        {
            this.TransformStack.Push();
            this.TransformStack.TransformLocal(customTransform);
            this.ApplyTransformStack();

            return new DummyDisposable(() =>
            {
                this.TransformStack.Pop();
                this.ApplyTransformStack();
            });
        }

        /// <summary>
        /// Clears the current view.
        /// </summary>
        /// <param name="clearColor">Color of the clear.</param>
        public void Clear(Color4 clearColor)
        {
            _renderTarget?.Clear(SdxMathHelper.RawFromColor4(clearColor));
        }

        /// <summary>
        /// Draws the given geometry.
        /// </summary>
        public void DrawGeometry(Geometry2DResourceBase geometry, BrushResource brush, float strokeWidth = 1f)
        {
            if (_renderTarget == null) { return; }

            geometry.EnsureNotNullOrDisposed(nameof(geometry));
            brush.EnsureNotNull(nameof(brush));
            strokeWidth.EnsurePositiveOrZero(nameof(strokeWidth));

            _renderTarget.DrawGeometry(
                geometry.GetGeometry(),
                brush.GetBrush(this.Device),
                strokeWidth);
        }

        /// <summary>
        /// Draws the given rectangle with the given brush.
        /// </summary>
        public void DrawRectangle(RectangleF rectangle, BrushResource brush, float strokeWidth = 1f)
        {
            if (_renderTarget == null) { return; }

            brush.EnsureNotNull(nameof(brush));
            rectangle.EnsureNotEmpty(nameof(rectangle));
            strokeWidth.EnsurePositiveOrZero(nameof(strokeWidth));

            _renderTarget.DrawRectangle(
                SdxMathHelper.RawFromRectangleF(rectangle),
                brush.GetBrush(this.Device),
                strokeWidth);
        }

        /// <summary>
        /// Draws the given rounded rectangle with the given brush.
        /// </summary>
        public void DrawRoundedRectangle(RectangleF rectangle, float radiusX, float radiusY, BrushResource brush, float strokeWidth = 1f)
        {
            if (_renderTarget == null)
            {
                return;
            }

            rectangle.EnsureNotEmpty(nameof(rectangle));
            brush.EnsureNotNull(nameof(brush));
            radiusX.EnsurePositiveOrZero(nameof(radiusX));
            radiusY.EnsurePositiveOrZero(nameof(radiusY));
            strokeWidth.EnsurePositiveOrZero(nameof(strokeWidth));

            var roundedRect = new D2D.RoundedRectangle
            {
                Rect = SdxMathHelper.RawFromRectangleF(rectangle),
                RadiusX = radiusX,
                RadiusY = radiusY
            };

            _renderTarget.DrawRoundedRectangle(
                roundedRect,
                brush.GetBrush(this.Device),
                strokeWidth);
        }

        /// <summary>
        /// Draws the given ellipse with the given brush.
        /// </summary>
        public void DrawEllipse(Vector2 center, float radiusX, float radiusY, BrushResource brush, float strokeWidth = 1f)
        {
            var ellipse = new D2D.Ellipse(SdxMathHelper.RawFromVector2(center), radiusX, radiusY);

            _renderTarget.DrawEllipse(
                ellipse,
                brush.GetBrush(this.Device),
                strokeWidth);
        }

        /// <summary>
        /// Draws the given ellipse with the given brush.
        /// </summary>
        public void DrawEllipse(RectangleF rectangle, BrushResource brush, float strokeWidth = 1f)
        {
            var radiusX = rectangle.Width / 2f;
            var radiusY = rectangle.Height / 2f;
            var center = new Vector2(
                rectangle.X + radiusX,
                rectangle.Y + radiusY);
            var ellipse = new D2D.Ellipse(SdxMathHelper.RawFromVector2(center), radiusX, radiusY);

            _renderTarget.DrawEllipse(
                ellipse,
                brush.GetBrush(this.Device),
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
            if (_renderTarget == null) { return; }

            brush.EnsureNotNull(nameof(brush));
            strokeWidth.EnsurePositiveAndNotZero(nameof(strokeWidth));

            _renderTarget.DrawLine(
                SdxMathHelper.RawFromVector2(start), SdxMathHelper.RawFromVector2(end),
                brush.GetBrush(this.Device),
                strokeWidth);
        }

        /// <summary>
        /// Fills the given rectangle with the given brush object.
        /// </summary>
        /// <param name="rectangle">The rectangle to be filled.</param>
        /// <param name="brush">The brush to be used.</param>
        public void FillRectangle(RectangleF rectangle, BrushResource brush)
        {
            if (_renderTarget == null) { return; }

            rectangle.EnsureNotEmpty(nameof(rectangle));
            brush.EnsureNotNull(nameof(brush));

            _renderTarget.FillRectangle(
                SdxMathHelper.RawFromRectangleF(rectangle),
                brush.GetBrush(this.Device));
        }

        /// <summary>
        /// Fills the given geometry.
        /// </summary>
        public void FillGeometry(Geometry2DResourceBase geometry, BrushResource brush)
        {
            if (_renderTarget == null) { return; }

            geometry.EnsureNotNullOrDisposed(nameof(geometry));
            brush.EnsureNotNull(nameof(brush));

            _renderTarget.FillGeometry(
                geometry.GetGeometry(),
                brush.GetBrush(this.Device));
        }

        /// <summary>
        /// Fills the given geometry.
        /// </summary>
        public void FillGeometry(Geometry2DResourceBase geometry, BrushResource brush, BrushResource opacityBrush)
        {
            if (_renderTarget == null) { return; }

            geometry.EnsureNotNullOrDisposed(nameof(geometry));
            brush.EnsureNotNull(nameof(brush));

            _renderTarget.FillGeometry(
                geometry.GetGeometry(),
                brush.GetBrush(this.Device),
                opacityBrush.GetBrush(this.Device));
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
            if (_renderTarget == null)
            {
                return;
            }

            rectangle.EnsureNotEmpty(nameof(rectangle));
            brush.EnsureNotNull(nameof(brush));
            radiusX.EnsurePositiveOrZero(nameof(radiusX));
            radiusY.EnsurePositiveOrZero(nameof(radiusY));

            var roundedRect = new D2D.RoundedRectangle
            {
                Rect = SdxMathHelper.RawFromRectangleF(rectangle),
                RadiusX = radiusX,
                RadiusY = radiusY
            };

            _renderTarget.FillRoundedRectangle(
                roundedRect,
                brush.GetBrush(this.Device));
        }

        /// <summary>
        /// Fills the given ellipse with the given brush.
        /// </summary>
        public void FillEllipse(Vector2 center, float radiusX, float radiusY, BrushResource brush)
        {
            var ellipse = new D2D.Ellipse(SdxMathHelper.RawFromVector2(center), radiusX, radiusY);

            _renderTarget.FillEllipse(
                ellipse,
                brush.GetBrush(this.Device));
        }

        /// <summary>
        /// Fills the given ellipse with the given brush.
        /// </summary>
        public void FillEllipse(RectangleF rectangle, BrushResource brush)
        {
            var radiusX = rectangle.Width / 2f;
            var radiusY = rectangle.Height / 2f;
            var center = new Vector2(
                rectangle.X + radiusX,
                rectangle.Y + radiusY);
            var ellipse = new D2D.Ellipse(SdxMathHelper.RawFromVector2(center), radiusX, radiusY);

            _renderTarget.FillEllipse(
                ellipse,
                brush.GetBrush(this.Device));
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
            if (_renderTarget == null) { return; }

            textToDraw.EnsureNotNull(nameof(textToDraw));
            targetRectangle.EnsureNotEmpty(nameof(targetRectangle));
            brush.EnsureNotNull(nameof(brush));

            _renderTarget.DrawText(
                textToDraw,
                textFormat.GetTextFormat(this.Device),
                SdxMathHelper.RawFromRectangleF(targetRectangle),
                brush.GetBrush(this.Device),
                (D2D.DrawTextOptions)drawOptions, (D2D.MeasuringMode)measuringMode);
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
            if (_renderTarget == null) { return; }

            bitmap.EnsureNotNull(nameof(bitmap));
            destinationRectangle.EnsureNotEmpty(nameof(destinationRectangle));
            opacity.EnsureInRange(0f, 1f, nameof(opacity));

            var bitmapFrameCount = bitmap.TotalFrameCount;
            frameIndex.EnsureInRange(0, bitmapFrameCount - 1, nameof(frameIndex));

            // Render the bitmap
            if (bitmapFrameCount > 1)
            {
                // Get the native bitmap object first
                // (if not, we may not have loaded it already and therefore
                //  missing size information)
                var nativeBitmap = bitmap.GetBitmap(this.Device);

                // Calculate source rectangle
                var framesX = bitmap.FrameCountX;
                var xFrameIndex = frameIndex % framesX;
                var yFrameIndex = (frameIndex - xFrameIndex) / framesX;
                var singleFrameWidth = bitmap.SingleFramePixelWidth;
                var singleFrameHeight = bitmap.SingleFramePixelHeight;
                var sourceRectangle = new RectangleF(
                    xFrameIndex * singleFrameWidth,
                    yFrameIndex * singleFrameHeight,
                    singleFrameWidth, singleFrameHeight);

                // Render tiled bitmap
                _renderTarget.DrawBitmap(
                    nativeBitmap,
                    SdxMathHelper.RawFromRectangleF(destinationRectangle),
                    opacity,
                    (D2D.BitmapInterpolationMode)interpolationMode,
                    SdxMathHelper.RawFromRectangleF(sourceRectangle));
            }
            else
            {
                // Render non-tiled bitmap
                _renderTarget.DrawBitmap(
                    bitmap.GetBitmap(this.Device),
                    SdxMathHelper.RawFromRectangleF(destinationRectangle),
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
            if (_renderTarget == null) { return; }

            bitmap.EnsureNotNull(nameof(bitmap));
            opacity.EnsureInRange(0f, 1f, nameof(opacity));

            var bitmapFrameCount = bitmap.TotalFrameCount;
            frameIndex.EnsureInRange(0, bitmapFrameCount - 1, nameof(frameIndex));

            // Render the bitmap
            if (bitmapFrameCount > 1)
            {
                // Get the native bitmap object first
                // (if not, we may not have loaded it already and therefore
                //  missing size information)
                var nativeBitmap = bitmap.GetBitmap(this.Device);

                // Calculate destination rectangle
                var singleFrameWidth = bitmap.SingleFramePixelWidth;
                var singleFrameHeight = bitmap.SingleFramePixelHeight;
                var destinationRectangle = new RectangleF(
                    destinationOrigin.X, destinationOrigin.Y,
                    singleFrameWidth, singleFrameHeight);

                // Calculate source rectangle
                var framesX = bitmap.FrameCountX;
                var xFrameIndex = frameIndex % framesX;
                var yFrameIndex = (frameIndex - xFrameIndex) / framesX;
                var sourceRectangle = new RectangleF(
                    xFrameIndex * singleFrameWidth,
                    yFrameIndex * singleFrameHeight,
                    singleFrameWidth, singleFrameHeight);

                // Render tiled bitmap
                _renderTarget.DrawBitmap(
                    nativeBitmap,
                    SdxMathHelper.RawFromRectangleF(destinationRectangle),
                    opacity,
                    (D2D.BitmapInterpolationMode)interpolationMode,
                    SdxMathHelper.RawFromRectangleF(sourceRectangle));
            }
            else
            {
                var nativeBitmap = bitmap.GetBitmap(this.Device);
                var destinationRectangle = new SDXM.RawRectangleF(
                    destinationOrigin.X, destinationOrigin.Y,
                    destinationOrigin.X + bitmap.PixelWidth, destinationOrigin.Y + bitmap.PixelHeight);

                // Render non-tiled bitmap
                _renderTarget.DrawBitmap(
                    nativeBitmap,
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
            if (_renderTarget == null) { return; }

            image.EnsureNotNull(nameof(image));

            var internalImage = (IImageInternal)image;
            internalImage.EnsureNotNull(nameof(internalImage));

            if (_deviceContext != null)
            {
                var d2dImage = internalImage.GetImageObject(this.Device) as D2D.Image;
                d2dImage.EnsureNotNull(nameof(d2dImage));

                _deviceContext.DrawImage(
                    d2dImage,
                    SdxMathHelper.RawFromVector2(destinationOrigin),
                    null,
                    D2D.InterpolationMode.Linear,
                    D2D.CompositeMode.SourceOver);
            }
            else
            {
                var bitmap = internalImage.TryGetSourceBitmap();

                if (bitmap != null)
                {
                    this.DrawBitmap(bitmap, destinationOrigin);
                }
            }
        }

        /// <summary>
        /// Sets current transform settings on this graphics object.
        /// (be careful, the state is changed on device level!)
        /// </summary>
        /// <param name="transformSettings">The settings to be set.</param>
        internal void PushTransformSettings(Graphics2DTransformSettings transformSettings)
        {
            _transformSettings = transformSettings;

            switch (transformSettings.TransformMode)
            {
                // Overtake given scaling matrix
                case Graphics2DTransformMode.Custom:
                    this.TransformStack.Push(transformSettings.CustomTransform);
                    break;

                // Calculate scaling matrix here
                case Graphics2DTransformMode.AutoScaleToVirtualScreen:
                    var virtualWidth = _transformSettings.VirtualScreenSize.Width;
                    var virtualHeight = _transformSettings.VirtualScreenSize.Height;
                    if (EngineMath.EqualsWithTolerance(virtualWidth, 0f)) { virtualWidth = this.ScreenPixelSize.Width; }
                    if (EngineMath.EqualsWithTolerance(virtualHeight, 0f)) { virtualHeight = this.ScreenPixelSize.Height; }

                    var scaleFactorX = this.ScreenPixelSize.Width / virtualWidth;
                    var scaleFactorY = this.ScreenPixelSize.Height / virtualHeight;
                    var combinedScaleFactor = Math.Min(scaleFactorX, scaleFactorY);
                    var truePixelWidth = virtualWidth * combinedScaleFactor;
                    var truePixelHeight = virtualHeight * combinedScaleFactor;

                    this.TransformStack.Push();
                    this.TransformStack.TransformLocal(
                        Matrix3x2.CreateScale(combinedScaleFactor) *
                        Matrix3x2.CreateTranslation(this.ScreenPixelSize.Width / 2f - truePixelWidth / 2f, this.ScreenPixelSize.Height / 2f - truePixelHeight / 2f));
                    break;

                default:
                    throw new SeeingSharpGraphicsException($"Unable to handle transform mode {transformSettings.TransformMode}");
            }

            // Apply current transform
            this.ApplyTransformStack();
        }

        /// <summary>
        /// Resets the transform setting son this graphics object.
        /// (be careful, the state is changed on device level!)
        /// </summary>
        internal void PopTransformSettings()
        {
            this.TransformStack.Pop();

            this.ApplyTransformStack();
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        public class Graphics2DInternals
        {
            private Graphics2D _owner;

            public D2D.RenderTarget RenderTarget => _owner._renderTarget;

            public D2D.DeviceContext DeviceContext => _owner._deviceContext;

            internal Graphics2DInternals(Graphics2D owner)
            {
                _owner = owner;
            }
        }
    }
}