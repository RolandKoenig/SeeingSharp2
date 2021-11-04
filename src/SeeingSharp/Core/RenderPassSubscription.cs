using System;

namespace SeeingSharp.Core
{
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
        /// <param name="zOrder">The z-order for sorting if the subscriptions of this pass get sorted by it.</param>
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
