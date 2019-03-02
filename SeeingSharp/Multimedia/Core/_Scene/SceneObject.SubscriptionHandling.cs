#region License information
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
#endregion

using System;
using System.Collections.Generic;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Core
{
    #region using
    #endregion

    public abstract partial class SceneObject
    {
        // This collection is synchronized with corresponding ViewRelatedSceneLayerSubsets
        // .. if differences are detected, then a SeeingSharpGraphicsException is thrown
        private IndexBasedDynamicCollection<List<RenderPassSubscription>> m_viewRelatedSubscriptions
            = new IndexBasedDynamicCollection<List<RenderPassSubscription>>();

        /// <summary>
        /// Counts all render pass subscriptions related to the given view subset.
        /// </summary>
        /// <param name="layerViewSubset">The layer view subset.</param>
        protected internal int CountRenderPassSubscriptions(ViewRelatedSceneLayerSubset layerViewSubset)
        {
            return m_viewRelatedSubscriptions[layerViewSubset.ViewIndex].Count;
        }

        /// <summary>
        /// Subscribes on the given render pass with the given action.
        /// </summary>
        /// <param name="renderPass">The render pass to which to subscribe.</param>
        /// <param name="renderAction">The action which performs rendering.</param>
        /// <param name="layerViewSubset">The ViewRelatedSceneLayerSubset to which to subscribe.</param>
        /// <param name="zOrder">The z order if sorting is enabled for this pass (default = 0).</param>
        protected internal void SubscribeToPass(
            RenderPassInfo renderPass,
            ViewRelatedSceneLayerSubset layerViewSubset, Action<RenderState> renderAction,
            int zOrder = 0)
        {
            var subscriptionList = m_viewRelatedSubscriptions[layerViewSubset.ViewIndex];

            subscriptionList.Add(layerViewSubset.SubscribeForPass(renderPass, this, renderAction, zOrder));
        }

        /// <summary>
        /// Updades all subscription info for given subscription row.
        /// This method is needed because 'RenderPassSubscription" is a struct and values are changed by host object.
        /// </summary>
        /// <param name="newSubscriptionInfo">The subscription information passed by host object.</param>
        /// <param name="layerViewSubset">The host object.</param>
        internal void UpdateSubscription(RenderPassSubscription newSubscriptionInfo, ViewRelatedSceneLayerSubset layerViewSubset)
        {
            // Get the subscription list
            // (may be null if object was removed from the layer)
            var subscriptionList = m_viewRelatedSubscriptions[layerViewSubset.ViewIndex];
            if (subscriptionList == null) { return; }

            // Update the corresponding subscription entry
            var subscriptionCount = subscriptionList.Count;
            var entryCount = 0;

            for (var loop = 0; loop < subscriptionCount; loop++)
            {
                var currentSubscriptionInfo = subscriptionList[loop];

                if (currentSubscriptionInfo.SceneObject == newSubscriptionInfo.SceneObject &&
                    currentSubscriptionInfo.RenderPass == newSubscriptionInfo.RenderPass &&
                    currentSubscriptionInfo.RenderMethod == newSubscriptionInfo.RenderMethod)
                {
                    subscriptionList[loop] = newSubscriptionInfo;
                    entryCount++;
                }
            }

            if (entryCount > 1)
            {
                throw new SeeingSharpGraphicsException("Inconsistency: Too much subscriptions for SceneObject detected!");
            }
        }

        /// <summary>
        /// Unsubscribes from the given render pass.
        /// </summary>
        protected internal void UnsubscribeFromPass(RenderPassInfo passInfo, UpdateState updateState, ViewRelatedSceneLayerSubset layerViewSubset)
        {
            // Get the subscription list
            // (may be null if object was removed from the layer)
            var subscriptionList = m_viewRelatedSubscriptions[layerViewSubset.ViewIndex];
            if (subscriptionList == null) { return; }

            // Perform unsubscribe
            var entryCount = 0;
            for (var loop = 0; loop < subscriptionList.Count; loop++)
            {
                if (subscriptionList[loop].RenderPass == passInfo)
                {
                    subscriptionList[loop].Unsubscribe();
                    subscriptionList.RemoveAt(loop);
                    entryCount++;
                    loop--;
                }
            }
            if (entryCount > 1) { throw new SeeingSharpGraphicsException("Inconsistency: Too much subscriptions for SceneObject detected!"); }
            if (entryCount == 0) { throw new SeeingSharpGraphicsException("Inconsistency: No subscription found on SceneObject!"); }
        }

        /// <summary>
        /// Clears all subscriptions on the given view subset.
        /// Only for internal use.. this method gets called when a view is deregistered.
        /// </summary>
        internal void ClearSubscriptionsWithoutUnsubscribeCall(ViewRelatedSceneLayerSubset layerViewSubset, RenderPassSubscription subscription)
        {
            // Get the subscription list
            // (may be null if object was removed from the layer)
            var subscriptionList = m_viewRelatedSubscriptions[layerViewSubset.ViewIndex];

            if (subscriptionList == null)
            {
                return;
            }

            // Remove the given entry from subscription list
            var subscriptionCount = subscriptionList.Count;
            var entryCount = 0;

            for (var loop = 0; loop < subscriptionCount; loop++)
            {
                var currentSubscriptionInfo = subscriptionList[loop];

                if (currentSubscriptionInfo.SceneObject == subscription.SceneObject &&
                   currentSubscriptionInfo.RenderPass == subscription.RenderPass &&
                   currentSubscriptionInfo.RenderMethod == subscription.RenderMethod)
                {
                    subscriptionList.RemoveAt(loop);
                    loop--;
                    subscriptionCount--;
                    entryCount++;
                }
            }

            if (entryCount > 1)
            {
                throw new SeeingSharpGraphicsException("Inconsistency: Too much subscriptions for SceneObject detected!");
            }
        }

        /// <summary>
        /// Clears all subscriptions on the given view subset.
        /// Only for internal use.. this method gets called when a view is deregistered.
        /// </summary>
        /// <param name="layerViewSubset">The view subset from which to clear all subscription entries.</param>
        internal void ClearSubscriptionsWithoutUnsubscribeCall(ViewRelatedSceneLayerSubset layerViewSubset)
        {
            var subscriptionList = m_viewRelatedSubscriptions[layerViewSubset.ViewIndex];
            subscriptionList.Clear();
        }

        /// <summary>
        /// Unsubscribes from all passes.
        /// </summary>
        protected internal void UnsubsribeFromAllPasses(ViewRelatedSceneLayerSubset layerViewSubset)
        {
            var subscriptionList = m_viewRelatedSubscriptions[layerViewSubset.ViewIndex];

            for (var loop = 0; loop < subscriptionList.Count; loop++)
            {
                subscriptionList[loop].Unsubscribe();
            }
            subscriptionList.Clear();
        }
    }
}
