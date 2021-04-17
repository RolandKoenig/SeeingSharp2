/*
    SeeingSharp and all applications distributed together with it. 
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
using System.Collections.Generic;
using System.Numerics;
using SeeingSharp.Multimedia.Drawing3D;

namespace SeeingSharp.Multimedia.Core
{
    public class ViewInformation
    {
        // Runtime values
        private BoundingFrustum _cameraFrustum;

        /// <summary>
        /// Gets the current view size.
        /// </summary>
        public Size2 CurrentViewSize => Owner.CurrentViewSize;

        /// <summary>
        /// Gets the camera object that belongs to this view.
        /// </summary>
        public Camera3DBase Camera => Owner.Camera;

        /// <summary>
        /// Gets the device this view is using.
        /// </summary>
        public EngineDevice Device => Owner.Device;

        /// <summary>
        /// Gets the current scene rendered by this view.
        /// </summary>
        public Scene Scene => Owner.Scene;

        /// <summary>
        /// Gets the bounding frustum defining the area the camera sees in the 3D world.
        /// </summary>
        public BoundingFrustum CameraBoundingFrustum => _cameraFrustum;

        /// <summary>
        /// Gets the configuration that belongs to this view.
        /// </summary>
        public GraphicsViewConfiguration ViewConfiguration => Owner.Configuration;

        /// <summary>
        /// Gets the collection containing all filters.
        /// </summary>
        internal List<SceneObjectFilter> FiltersInternal => Owner.FiltersInternal;

        /// <summary>
        /// The owner of this ViewInformation object (standard field for fast access):
        /// </summary>
        internal RenderLoop Owner;

        /// <summary>
        /// Gets or sets the index of this view.
        /// Be careful: this value depends on the scene the view is attached to!
        ///             Its value is set by RegisterView and DeregisterView methods of the Scene class
        /// </summary>
        internal int ViewIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewInformation" /> class.
        /// </summary>
        internal ViewInformation(RenderLoop owner)
        {
            Owner = owner;

            ViewIndex = -1;
        }

        public static implicit operator ViewInformation(RenderLoop renderLoop)
        {
            return renderLoop.ViewInformation;
        }

        /// <summary>
        /// Updates the bounding frustum.
        /// </summary>
        /// <param name="viewProjectionMatrix"></param>
        internal void UpdateFrustum(Matrix4x4 viewProjectionMatrix)
        {
            _cameraFrustum = new BoundingFrustum(viewProjectionMatrix);
        }
    }
}
