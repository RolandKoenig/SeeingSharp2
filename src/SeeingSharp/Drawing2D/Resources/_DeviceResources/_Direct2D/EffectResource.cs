using System;
using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;
using SharpGen.Runtime;
using D2D = Vortice.Direct2D1;

namespace SeeingSharp.Drawing2D.Resources
{
    public abstract class EffectResource : Drawing2DResourceBase, IImage, IImageInternal
    {
        // Configuration
        private IImageInternal[] _effectInputs;

        // Resources
        private D2D.ID2D1Effect?[] _loadedEffects;

        /// <summary>
        /// Initializes a new instance of the <see cref="EffectResource"/> class.
        /// </summary>
        protected EffectResource(params IImage[] effectInputs)
        {
            _loadedEffects = new D2D.ID2D1Effect[GraphicsCore.Current.DeviceCount];

            // Get all effect inputs
            _effectInputs = new IImageInternal[effectInputs.Length];
            for (var loop = 0; loop < effectInputs.Length; loop++)
            {
                var castedInput = effectInputs[loop] as IImageInternal;
                if (castedInput == null)
                {
                    throw new SeeingSharpGraphicsException("Unable to process effect input at index " + loop + "!");
                }

                _effectInputs[loop] = castedInput;
            }
        }

        /// <summary>
        /// Builds the effect.
        /// </summary>
        /// <param name="device">The device on which to load the effect instance.</param>
        protected abstract D2D.ID2D1Effect BuildEffect(EngineDevice device);

        /// <summary>
        /// Unloads all resources loaded on the given device.
        /// </summary>
        /// <param name="engineDevice">The device for which to unload the resource.</param>
        internal override void UnloadResources(EngineDevice engineDevice)
        {
            var actEffect = _loadedEffects[engineDevice.DeviceIndex];

            if (actEffect != null)
            {
                engineDevice.DeregisterDeviceResource(this);

                SeeingSharpUtil.DisposeObject(actEffect);
                _loadedEffects[engineDevice.DeviceIndex] = null;
            }
        }

        /// <summary>
        /// Tries to get the <see cref="BitmapResource"/> which is the source of this image.
        /// </summary>
        BitmapResource? IImageInternal.TryGetSourceBitmap()
        {
            if (_effectInputs.Length > 0)
            {
                return _effectInputs[0].TryGetSourceBitmap();
            }

            return null;
        }

        /// <summary>
        /// Gets the input object for an effect.
        /// </summary>
        /// <param name="device">The device for which to get the input.</param>
        IDisposable IImageInternal.GetImageObject(EngineDevice device)
        {
            var effect = _loadedEffects[device.DeviceIndex];

            if (effect == null)
            {
                // Create the effect
                effect = this.BuildEffect(device);

                // Set input values
                for (var loop = 0; loop < _effectInputs.Length; loop++)
                {
                    using (var actInput = _effectInputs[loop].GetImageObject(device) as D2D.ID2D1Image)
                    {
                        effect.SetInput(loop, actInput, new RawBool(false));
                    }
                }

                // Store loaded effect
                _loadedEffects[device.DeviceIndex] = effect;
                device.RegisterDeviceResource(this);
            }

            return effect.Output;
        }
    }
}