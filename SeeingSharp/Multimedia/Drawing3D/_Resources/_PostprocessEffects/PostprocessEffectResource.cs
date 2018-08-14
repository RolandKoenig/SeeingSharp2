#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public abstract class PostprocessEffectResource : ShaderEffectResourceBase
    {
        /// <summary>
        /// Notifies that rendering begins.
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        /// <param name="passID">The ID of the current pass (starting with 0)</param>
        internal abstract void NotifyBeforeRender(RenderState renderState, int passID);

        /// <summary>
        /// Notifies that rendering of the plain part has finished.
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        /// <param name="passID">The ID of the current pass (starting with 0)</param>
        /// <returns>True, if rendering should continue with next pass. False if postprocess effect is finished.</returns>
        internal abstract void NotifyAfterRenderPlain(RenderState renderState, int passID);

        /// <summary>
        /// Notifies that rendering has finished.
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        /// <param name="passID">The ID of the current pass (starting with 0)</param>
        /// <returns>True, if rendering should continue with next pass. False if postprocess effect is finished.</returns>
        internal abstract bool NotifyAfterRender(RenderState renderState, int passID);
    }
}
