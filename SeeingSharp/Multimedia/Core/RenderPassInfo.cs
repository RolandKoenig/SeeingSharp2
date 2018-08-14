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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SeeingSharp.Multimedia.Core
{
    public class RenderPassInfo
    {
        #region All available render passes
        public static readonly RenderPassInfo PASS_PLAIN_RENDER = new RenderPassInfo("DefaultPlainRender");
        public static readonly RenderPassInfo PASS_LINE_RENDER = new RenderPassInfo("LineRender");
        public static readonly RenderPassInfo PASS_TRANSPARENT_RENDER = new RenderPassInfo("DefaultTransparentRender");
        public static readonly RenderPassInfo PASS_SPRITE_BATCH = new RenderPassInfo("SpriteBatchRender");
        public static readonly RenderPassInfo PASS_2D_OVERLAY = new RenderPassInfo("2D-Overlay", isSorted: true);
        #endregion

        #region Static collection which holds all render passes
        private static List<RenderPassInfo> s_renderPasses;
        private static ReadOnlyCollection<RenderPassInfo> s_renderPassesPublic;
        #endregion

        #region Render pass properties
        private string m_name;
        private bool m_isSorted;
        #endregion

        /// <summary>
        /// Initializes the <see cref="RenderPassInfo" /> class.
        /// </summary>
        static RenderPassInfo()
        {
            s_renderPasses = new List<RenderPassInfo>();
            s_renderPassesPublic = new ReadOnlyCollection<RenderPassInfo>(s_renderPasses);

            s_renderPasses.Add(PASS_PLAIN_RENDER);
            s_renderPasses.Add(PASS_LINE_RENDER);
            s_renderPasses.Add(PASS_TRANSPARENT_RENDER);
            s_renderPasses.Add(PASS_SPRITE_BATCH);
            s_renderPasses.Add(PASS_2D_OVERLAY);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderPassInfo" /> class.
        /// </summary>
        internal RenderPassInfo(string name, bool isSorted = false)
        {
            m_name = name;
            m_isSorted = isSorted;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return m_name;
        }

        /// <summary>
        /// Gets a collection containing all render passes.
        /// </summary>
        public static ReadOnlyCollection<RenderPassInfo> AllRenderPasses
        {
            get { return s_renderPassesPublic; }
        }

        /// <summary>
        /// Gets the name of this pass.
        /// </summary>
        public string Name
        {
            get { return m_name; }
        }

        public bool IsSorted
        {
            get { return m_isSorted; }
        }
    }
}
