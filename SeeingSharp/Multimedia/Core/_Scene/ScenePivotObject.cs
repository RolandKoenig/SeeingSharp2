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

    using SharpDX;

    #endregion

    /// <summary>
    /// This class represents a Pivot with no visualization at all.
    /// A Pivot is only used to transform more child objects dependent on it.
    /// </summary>
    public class ScenePivotObject : SceneSpacialObject
    {
        /// <summary>
        /// Tries to get the bounding box for the given render-loop.
        /// Returns BoundingBox.Empty if it is not available.
        /// </summary>
        /// <param name="viewInfo">The ViewInformation for which to get the BoundingBox.</param>
        public override BoundingBox TryGetBoundingBox(ViewInformation viewInfo)
        {
            return default(BoundingBox);
        }

        /// <summary>
        /// Tries to get the bounding sphere for the given render-loop.
        /// Returns BoundingSphere.Empty, if it is not available.
        /// </summary>
        /// <param name="viewInfo">The ViewInformation for which to get the BoundingSphere.</param>
        public override BoundingSphere TryGetBoundingSphere(ViewInformation viewInfo)
        {
            return default(BoundingSphere);
        }

        /// <summary>
        /// Loads all resources of the object.
        /// </summary>
        /// <param name="device">Current graphics device.</param>
        /// <param name="resourceDictionary">Current resource dicionary.</param>
        public override void LoadResources(EngineDevice device, ResourceDictionary resourceDictionary)
        {
        }

        /// <summary>
        /// Updates this object for the given view.
        /// </summary>
        /// <param name="updateState">Current state of the update pass.</param>
        /// <param name="layerViewSubset">The layer view subset wich called this update method.</param>
        protected override void UpdateForViewInternal(SceneRelatedUpdateState updateState, ViewRelatedSceneLayerSubset layerViewSubset)
        {
        }

        /// <summary>
        /// Are resources loaded for the given device?
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public override bool IsLoaded(EngineDevice device)
        {
            return true;
        }
    }
}
