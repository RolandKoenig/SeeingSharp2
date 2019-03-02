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

using System;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using SharpDX.Mathematics.Interop;
using D2D = SharpDX.Direct2D1;

namespace SeeingSharp.Multimedia.Drawing2D
{
    public abstract class EffectResource : Drawing2DResourceBase, IImage, IImageInternal
    {
        // Configuration
        private IImageInternal[] m_effectInputs;

        // Resources
        private D2D.Effect[] m_loadedEffects;

        /// <summary>
        /// Initializes a new instance of the <see cref="EffectResource"/> class.
        /// </summary>
        public EffectResource(params IImage[] effectInputs)
        {
            m_loadedEffects = new D2D.Effect[GraphicsCore.Current.DeviceCount];

            // Get all effect inputs
            m_effectInputs = new IImageInternal[effectInputs.Length];
            for(var loop=0; loop<effectInputs.Length; loop++)
            {
                m_effectInputs[loop] = effectInputs[loop] as IImageInternal;
                if(m_effectInputs[loop] == null)
                {
                    throw new SeeingSharpGraphicsException("Unable to process effectinput at index " + loop + "!");
                }
            }
        }

        /// <summary>
        /// Builds the effect.
        /// </summary>
        /// <param name="device">The device on which to load the effect instance.</param>
        protected abstract D2D.Effect BuildEffect(EngineDevice device);

        /// <summary>
        /// Unloads all resources loaded on the given device.
        /// </summary>
        /// <param name="engineDevice">The device for which to unload the resource.</param>
        internal override void UnloadResources(EngineDevice engineDevice)
        {
            var actEffect = m_loadedEffects[engineDevice.DeviceIndex];

            if (actEffect != null)
            {
                SeeingSharpTools.DisposeObject(actEffect);
                m_loadedEffects[engineDevice.DeviceIndex] = null;
            }
        }

        /// <summary>
        /// Tries to get the <see cref="BitmapResource"/> which is the source of this image.
        /// </summary>
        BitmapResource IImageInternal.TryGetSourceBitmap()
        {
            if(m_effectInputs.Length > 0)
            {
                return m_effectInputs[0].TryGetSourceBitmap();
            }

            return null;
        }

        /// <summary>
        /// Gets the input object for an effect.
        /// </summary>
        /// <param name="device">The device for which to get the input.</param>
        IDisposable IImageInternal.GetImageObject(EngineDevice device)
        {
            var effect = m_loadedEffects[device.DeviceIndex];

            if(effect == null)
            {
                // Create the effect
                effect = BuildEffect(device);

                // Set input values
                for(var loop =0; loop<m_effectInputs.Length; loop++)
                {
                    using (var actInput = m_effectInputs[loop].GetImageObject(device) as D2D.Image)
                    {
                        effect.SetInput(loop, actInput, new RawBool(false));
                    }
                }

                // Store loaded effect
                m_loadedEffects[device.DeviceIndex] = effect;
            }

            return effect.Output;
        }
    }
}