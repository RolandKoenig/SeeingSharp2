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

// Namespace mappings
using D2D = SharpDX.Direct2D1;

#endregion

namespace SeeingSharp.Multimedia.Drawing2D
{
    #region using

    using Core;

    #endregion

    public class GaussianBlurEffectResource : EffectResource
    {
        public GaussianBlurEffectResource(IImage sourceImage)
            : base(sourceImage)
        {
            this.StandardDeviation = 1f;
        }

        /// <summary>
        /// Builds the effect.
        /// </summary>
        /// <param name="device">The device on which to load the effect instance.</param>
        protected override D2D.Effect BuildEffect(EngineDevice device)
        {
            D2D.Effects.GaussianBlur blurEffect = new D2D.Effects.GaussianBlur(device.DeviceContextD2D);
            blurEffect.BorderMode = D2D.BorderMode.Soft;
            blurEffect.Optimization = D2D.GaussianBlurOptimization.Quality;
            blurEffect.StandardDeviation = this.StandardDeviation;
            return blurEffect;
        }

        public float StandardDeviation
        {
            get;
            set;
        }
    }
}