#region License information
/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwhise.
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

    using System;

    #endregion

    public struct RenderPassSubscription
    {
        internal ViewRelatedSceneLayerSubset LayerViewSubset;
        internal RenderPassInfo RenderPass;
        internal SceneObject SceneObject;
        internal Action<RenderState> RenderMethod;
        internal int ZOrder;

        /// <summary>
        /// Very important variable to find the subscription in the host collection!
        /// </summary>
        internal int SubscriptionIndex;

        /// <summary>
        /// A special flag for optimizing unsubscription process.
        /// </summary>
        internal bool IsSubscribed;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderPassSubscription" /> struct.
        /// </summary>
        /// <param name="layerViewSubset">The ViewRelatedSceneLayerSubset object this subscription belongs to.</param>
        /// <param name="renderPass">The render pass on which to register.</param>
        /// <param name="sceneObject">The scene object which should be registered.</param>
        /// <param name="renderMethod">The render method which is to be registered.</param>
        /// <param name="zOrder">The z-order for sorting if the subsciptions of this pass get sorted by it.</param>
        internal RenderPassSubscription(
            ViewRelatedSceneLayerSubset layerViewSubset, RenderPassInfo renderPass,
            SceneObject sceneObject, Action<RenderState> renderMethod,
            int zOrder)
        {
            LayerViewSubset = layerViewSubset;
            RenderPass = renderPass;
            SceneObject = sceneObject;
            RenderMethod = renderMethod;
            IsSubscribed = true;
            SubscriptionIndex = 0;
            ZOrder = zOrder;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "{" + SceneObject + "} => {" + RenderPass + "}";
        }

        /// <summary>
        /// Unsubscribes this subscription.
        /// </summary>
        public void Unsubscribe()
        {
            LayerViewSubset.UnsubscribeForPass(this);
        }
    }
}
