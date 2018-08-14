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
using SeeingSharp.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharp.Multimedia.Core
{
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
            List<RenderPassSubscription> subscriptionList = m_viewRelatedSubscriptions[layerViewSubset.ViewIndex];

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
            List<RenderPassSubscription> subscriptionList = m_viewRelatedSubscriptions[layerViewSubset.ViewIndex];
            if (subscriptionList == null) { return; }

            // Update the corresponding subscription entry
            int subscriptionCount = subscriptionList.Count;
            int entryCount = 0;
            for (int loop = 0; loop < subscriptionCount; loop++)
            {
                RenderPassSubscription currentSubscriptionInfo = subscriptionList[loop];
                if ((currentSubscriptionInfo.SceneObject == newSubscriptionInfo.SceneObject) &&
                    (currentSubscriptionInfo.RenderPass == newSubscriptionInfo.RenderPass) &&
                    (currentSubscriptionInfo.RenderMethod == newSubscriptionInfo.RenderMethod))
                {
                    subscriptionList[loop] = newSubscriptionInfo;
                    entryCount++;
                }
            }
            if (entryCount > 1) { throw new SeeingSharpGraphicsException("Inconsistency: Too much subscriptions for SceneObject detected!"); }
        }

        /// <summary>
        /// Unsubscribes from the given render pass.
        /// </summary>
        protected internal void UnsubscribeFromPass(RenderPassInfo passInfo, UpdateState updateState, ViewRelatedSceneLayerSubset layerViewSubset)
        {
            // Get the subscription list
            // (may be null if object was removed from the layer)
            List<RenderPassSubscription> subscriptionList = m_viewRelatedSubscriptions[layerViewSubset.ViewIndex];
            if (subscriptionList == null) { return; }

            // Perform unsubscribe
            int entryCount = 0;
            for (int loop = 0; loop < subscriptionList.Count; loop++)
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
            List<RenderPassSubscription> subscriptionList = m_viewRelatedSubscriptions[layerViewSubset.ViewIndex];
            if (subscriptionList == null) { return; }

            // Remove the given entry from subscription list
            int subscriptionCount = subscriptionList.Count;
            int entryCount = 0;
            for (int loop = 0; loop < subscriptionCount; loop++)
            {
                RenderPassSubscription currentSubscriptionInfo = subscriptionList[loop];
                if ((currentSubscriptionInfo.SceneObject == subscription.SceneObject) &&
                   (currentSubscriptionInfo.RenderPass == subscription.RenderPass) &&
                   (currentSubscriptionInfo.RenderMethod == subscription.RenderMethod))
                {
                    subscriptionList.RemoveAt(loop);
                    loop--;
                    subscriptionCount--;
                    entryCount++;
                }
            }
            if (entryCount > 1) { throw new SeeingSharpGraphicsException("Inconsistency: Too much subscriptions for SceneObject detected!"); }
        }

        /// <summary>
        /// Clears all subscriptions on the given view subset.
        /// Only for internal use.. this method gets called when a view is deregistered.
        /// </summary>
        /// <param name="layerViewSubset">The view subset from which to clear all subscription entries.</param>
        internal void ClearSubscriptionsWithoutUnsubscribeCall(ViewRelatedSceneLayerSubset layerViewSubset)
        {
            List<RenderPassSubscription> subscriptionList = m_viewRelatedSubscriptions[layerViewSubset.ViewIndex];
            subscriptionList.Clear();
        }

        /// <summary>
        /// Unsubscribes from all passes.
        /// </summary>
        protected internal void UnsubsribeFromAllPasses(ViewRelatedSceneLayerSubset layerViewSubset)
        {
            List<RenderPassSubscription> subscriptionList = m_viewRelatedSubscriptions[layerViewSubset.ViewIndex];

            for (int loop = 0; loop < subscriptionList.Count; loop++)
            {
                subscriptionList[loop].Unsubscribe();
            }
            subscriptionList.Clear();
        }
    }
}
