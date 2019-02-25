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

namespace SeeingSharp.Multimedia.Core
{
    #region using

    using System.Collections.Generic;
    using Drawing3D;
    using SharpDX;

    #endregion

    public class ViewInformation
    {
        #region Runtime values
        private BoundingFrustum m_cameraFrustum;
        #endregion

        #region Object filters for the scene model
        private List<SceneObjectFilter> m_sceneObjectFilters;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewInformation" /> class.
        /// </summary>
        internal ViewInformation(RenderLoop owner)
        {
            this.Owner = owner;

            //Initialize scene object filter
            m_sceneObjectFilters = new List<SceneObjectFilter>();

            this.ViewIndex = -1;
        }

        /// <summary>
        /// Updates the bounding frustum.
        /// </summary>
        /// <param name="viewProjectionMatrix"></param>
        internal void UpdateFrustum(Matrix viewProjectionMatrix)
        {
            m_cameraFrustum = new BoundingFrustum(viewProjectionMatrix);
        }

        public static implicit operator ViewInformation(RenderLoop renderLoop)
        {
            return renderLoop.ViewInformation;
        }

        /// <summary>
        /// Gets the current view size.
        /// </summary>
        public Size2 CurrentViewSize
        {
            get { return Owner.CurrentViewSize; }
        }

        /// <summary>
        /// Gets the camera object that belongs to this view.
        /// </summary>
        public Camera3DBase Camera
        {
            get { return Owner.Camera; }
        }

        /// <summary>
        /// Gets the device this view is using.
        /// </summary>
        public EngineDevice Device
        {
            get { return Owner.Device; }
        }

        /// <summary>
        /// Gets the current scene rendered by this view.
        /// </summary>
        public Scene Scene
        {
            get { return Owner.Scene; }
        }

        /// <summary>
        /// Gets the bounding frustum defining the area the camera sees in the 3D wordl.
        /// </summary>
        public BoundingFrustum CameraBoundingFrustum
        {
            get { return m_cameraFrustum; }
        }

        /// <summary>
        /// Gets the configuration that belongs to this view.
        /// </summary>
        public GraphicsViewConfiguration ViewConfiguration
        {
            get { return Owner.ViewConfiguration; }
        }

        /// <summary>
        /// Gets the collection containing all filters.
        /// </summary>
        internal List<SceneObjectFilter> Filters
        {
            get { return Owner.Filters; }
        }

        /// <summary>
        /// Gets or sets the index of this view.
        /// Be careful: this value depends on the scene the view is attached to!
        ///             Its value is set by RegisterView and DeregisterView methods of the Scene class
        /// </summary>
        internal int ViewIndex;

        /// <summary>
        /// The owner of this ViewInformation object (standard field for fast access):
        /// </summary>
        internal RenderLoop Owner;
    }
}
