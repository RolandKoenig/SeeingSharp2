using System;
using System.Numerics;
using SeeingSharp.Checking;
using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;
using D2D = Vortice.Direct2D1;

namespace SeeingSharp.Drawing2D.Resources
{
    public class SolidBrushResource : BrushResource
    {
        // Resources
        private D2D.ID2D1SolidColorBrush[] _loadedBrushes;

        public Color4 Color { get; }

        public float Opacity { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SolidBrushResource" /> class.
        /// </summary>
        /// <param name="singleColor">Color of the single.</param>
        /// <param name="opacity">The opacity value of the brush.</param>
        public SolidBrushResource(
            Color4 singleColor,
            float opacity = 1f)
        {
            opacity.EnsureInRange(0f, 1f, nameof(opacity));

            _loadedBrushes = new D2D.ID2D1SolidColorBrush[GraphicsCore.Current.DeviceCount];

            this.Opacity = opacity;
            this.Color = singleColor;
        }

        /// <summary>
        /// Unloads all resources loaded on the given device.
        /// </summary>
        /// <param name="engineDevice">The device for which to unload the resource.</param>
        internal override void UnloadResources(EngineDevice engineDevice)
        {
            D2D.ID2D1Brush brush = _loadedBrushes[engineDevice.DeviceIndex];
            if (brush != null)
            {
                engineDevice.DeregisterDeviceResource(this);

                SeeingSharpUtil.DisposeObject(brush);
                _loadedBrushes[engineDevice.DeviceIndex] = null;
            }
        }

        /// <summary>
        /// Gets the brush for the given device.
        /// </summary>
        /// <param name="engineDevice">The device for which to get the brush.</param>
        internal override D2D.ID2D1Brush GetBrush(EngineDevice engineDevice)
        {
            // Check for disposed state
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            var result = _loadedBrushes[engineDevice.DeviceIndex];

            if (result == null)
            {
                // Load the brush
                result = engineDevice.FakeRenderTarget2D.CreateSolidColorBrush(
                    MathConverter.RawFromColor4(this.Color),
                    new D2D.BrushProperties
                    {
                        Opacity = this.Opacity,
                        Transform = Matrix3x2.Identity
                    });
                _loadedBrushes[engineDevice.DeviceIndex] = result;
                engineDevice.RegisterDeviceResource(this);
            }

            return result;
        }
    }
}