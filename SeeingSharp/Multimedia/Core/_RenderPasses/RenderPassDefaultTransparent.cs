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
using SeeingSharp.Multimedia.Drawing3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Some namespace mappings
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Core
{
    public class RenderPassDefaultTransparent : RenderPassBase
    {
        private DefaultResources m_defaultResources;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderPassDefaultTransparent" /> class.
        /// </summary>
        public RenderPassDefaultTransparent()
        {

        }

        /// <summary>
        /// Applies this RenderPass (called before starting rendering first objects with it).
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        public override void Apply(RenderState renderState)
        {
            D3D11.DeviceContext deviceContext = renderState.Device.DeviceImmediateContextD3D11;

            deviceContext.OutputMerger.BlendState = m_defaultResources.AlphaBlendingBlendState;
            deviceContext.OutputMerger.DepthStencilState = m_defaultResources.DepthStencilStateDisableZWrites;
        }

        /// <summary>
        /// Discards this RenderPass (called after rendering all objects of this pass).
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        public override void Discard(RenderState renderState)
        {
            D3D11.DeviceContext deviceContext = renderState.Device.DeviceImmediateContextD3D11;

            deviceContext.OutputMerger.BlendState = m_defaultResources.DefaultBlendState;
            deviceContext.OutputMerger.DepthStencilState = m_defaultResources.DepthStencilStateDefault;
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        /// <param name="device">The target device.</param>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            //Get default resources
            m_defaultResources = resources.GetResourceAndEnsureLoaded(
                DefaultResources.RESOURCE_KEY,
                () => new DefaultResources());
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        /// <param name="device">The target device.</param>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            m_defaultResources = null;
        }

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded
        {
            get { return m_defaultResources != null; }
        }
    }
}
