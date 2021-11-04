using System;
using System.Collections.Generic;
using SeeingSharp.Util;

namespace SeeingSharp.Core
{
    public abstract partial class SceneObject
    {
        // This collection is synchronized with corresponding ViewRelatedSceneLayerSubsets
        // .. if differences are detected, then a SeeingSharpGraphicsException is thrown
        private IndexBasedDynamicCollection<List<RenderPassSubscription>> _viewRelatedSubscriptions
            = new IndexBasedDynamicCollection<List<RenderPassSubscription>>();

        /// <summary>
        /// Counts all render pass subscriptions related to the given view subset.
        /// </summary>
        /// <param name="layerViewSubset">The layer view subset.</param>
        protected internal int CountRenderPassSubscriptions(ViewRelatedSceneLayerSubset layerViewSubset)
        {
            return _viewRelatedSubscriptions[layerViewSubset.ViewIndex].Count;
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
            var subscriptionList = _viewRelatedSubscriptions[layerViewSubset.ViewIndex];

            subscriptionList.Add(layerViewSubset.SubscribeForPass(renderPass, this, renderAction, zOrder));
        }

        /// <summary>
        /// Unsubscribes from the given render pass.
        /// </summary>
        protected internal void UnsubscribeFromPass(RenderPassInfo passInfo, UpdateState updateState, ViewRelatedSceneLayerSubset layerViewSubset)
        {
            // Get the subscription list
            // (may be null if object was removed from the layer)
            var subscriptionList = _viewRelatedSubscriptions[layerViewSubset.ViewIndex];
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
        /// Unsubscribes from all passes.
        /// </summary>
        protected internal void UnsubsribeFromAllPasses(ViewRelatedSceneLayerSubset layerViewSubset)
        {
            var subscriptionList = _viewRelatedSubscriptions[layerViewSubset.ViewIndex];
            for (var loop = 0; loop < subscriptionList.Count; loop++)
            {
                subscriptionList[loop].Unsubscribe();
            }
            subscriptionList.Clear();
        }

        /// <summary>
        /// Updates all subscription info for given subscription row.
        /// This method is needed because 'RenderPassSubscription" is a struct and values are changed by host object.
        /// </summary>
        /// <param name="newSubscriptionInfo">The subscription information passed by host object.</param>
        /// <param name="layerViewSubset">The host object.</param>
        internal void UpdateSubscription(RenderPassSubscription newSubscriptionInfo, ViewRelatedSceneLayerSubset layerViewSubset)
        {
            // Get the subscription list
            // (may be null if object was removed from the layer)
            var subscriptionList = _viewRelatedSubscriptions[layerViewSubset.ViewIndex];
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
        /// Clears all subscriptions on the given view subset.
        /// Only for internal use.. this method gets called when a view is deregistered.
        /// </summary>
        internal void ClearSubscriptionsWithoutUnsubscribeCall(ViewRelatedSceneLayerSubset layerViewSubset, RenderPassSubscription subscription)
        {
            // Get the subscription list
            // (may be null if object was removed from the layer)
            var subscriptionList = _viewRelatedSubscriptions[layerViewSubset.ViewIndex];

            if (subscriptionList == null)
            {
                return;
            }

            // RemoveObject the given entry from subscription list
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
            var subscriptionList = _viewRelatedSubscriptions[layerViewSubset.ViewIndex];
            subscriptionList.Clear();
        }
    }
}
