using SeeingSharp.Core;

namespace SeeingSharp.Drawing3D
{
    public abstract class PostprocessEffectResource : ShaderEffectResourceBase
    {
        /// <summary>
        /// Notifies that rendering begins.
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        /// <param name="layerName">The name of the <see cref="SceneLayer"/> which we are rendering currently. This parameter is meant for debugging.</param>
        /// <param name="passId">The Id of the current pass (starting with 0)</param>
        internal abstract void NotifyBeforeRender(RenderState renderState, string layerName, int passId);

        /// <summary>
        /// Notifies that rendering of the plain part has finished.
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        /// <param name="layerName">The name of the <see cref="SceneLayer"/> which we are rendering currently. This parameter is meant for debugging.</param>
        /// <param name="passId">The Id of the current pass (starting with 0)</param>
        /// <returns>True, if rendering should continue with next pass. False if postprocess effect is finished.</returns>
        internal abstract void NotifyAfterRenderPlain(RenderState renderState, string layerName, int passId);

        /// <summary>
        /// Notifies that rendering has finished.
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        /// <param name="layerName">The name of the <see cref="SceneLayer"/> which we are rendering currently. This parameter is meant for debugging.</param>
        /// <param name="passId">The Id of the current pass (starting with 0)</param>
        /// <returns>True, if rendering should continue with next pass. False if postprocess effect is finished.</returns>
        internal abstract bool NotifyAfterRender(RenderState renderState, string layerName, int passId);
    }
}
