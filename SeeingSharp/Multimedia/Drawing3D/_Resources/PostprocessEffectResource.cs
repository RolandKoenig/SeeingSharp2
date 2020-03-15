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
using SeeingSharp.Multimedia.Core;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public abstract class PostprocessEffectResource : ShaderEffectResourceBase
    {
        /// <summary>
        /// Notifies that rendering begins.
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        /// <param name="layerName">The name of the <see cref="SceneLayer"/> which we are rendering currently. This parameter is meant for debugging.</param>
        /// <param name="passId">The Id of the current pass (starting with 0)</param>
        internal abstract void NotifyBeforeRender(RenderState renderState, string layerName, int passId);

        /// <summary>
        /// Notifies that rendering of the plain part has finished.
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        /// <param name="layerName">The name of the <see cref="SceneLayer"/> which we are rendering currently. This parameter is meant for debugging.</param>
        /// <param name="passId">The Id of the current pass (starting with 0)</param>
        /// <returns>True, if rendering should continue with next pass. False if postprocess effect is finished.</returns>
        internal abstract void NotifyAfterRenderPlain(RenderState renderState, string layerName, int passId);

        /// <summary>
        /// Notifies that rendering has finished.
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        /// <param name="layerName">The name of the <see cref="SceneLayer"/> which we are rendering currently. This parameter is meant for debugging.</param>
        /// <param name="passId">The Id of the current pass (starting with 0)</param>
        /// <returns>True, if rendering should continue with next pass. False if postprocess effect is finished.</returns>
        internal abstract bool NotifyAfterRender(RenderState renderState, string layerName, int passId);
    }
}
