using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public abstract class ShaderEffectResourceBase : Resource
    {
        private static readonly NamedOrGenericKey s_resKeyVertexShader = GraphicsCore.GetNextGenericResourceKey();

        // Resources
        private VertexShaderResource _vertexShader;
        private DefaultResources _defaultResources;

        /// <summary>
        /// Applies alpha based sprite rendering.
        /// </summary>
        /// <param name="deviceContext">The target device context.</param>
        protected void ApplySpriteRendering(D3D11.DeviceContext deviceContext)
        {
            deviceContext.VertexShader.Set(_vertexShader.VertexShader);

            deviceContext.OutputMerger.DepthStencilState = _defaultResources.DepthStencilStateAlwaysPassDepth;
        }

        /// <summary>
        /// Discards alpha based sprite rendering.
        /// </summary>
        /// <param name="deviceContext">The target device context.</param>
        protected void DiscardSpriteRendering(D3D11.DeviceContext deviceContext)
        {
            deviceContext.OutputMerger.DepthStencilState = _defaultResources.DepthStencilStateDefault;
        }

        /// <summary>
        /// Applies alpha based sprite rendering.
        /// </summary>
        /// <param name="deviceContext">The target device context.</param>
        protected void ApplyAlphaBasedSpriteRendering(D3D11.DeviceContext deviceContext)
        {
            deviceContext.VertexShader.Set(_vertexShader.VertexShader);

            deviceContext.OutputMerger.DepthStencilState = _defaultResources.DepthStencilStateAlwaysPassDepth;
            deviceContext.OutputMerger.BlendState = _defaultResources.AlphaBlendingBlendState;
        }

        /// <summary>
        /// Discards alpha based sprite rendering.
        /// </summary>
        /// <param name="deviceContext">The target device context.</param>
        protected void DiscardAlphaBasedSpriteRendering(D3D11.DeviceContext deviceContext)
        {
            deviceContext.OutputMerger.BlendState = _defaultResources.DefaultBlendState;
            deviceContext.OutputMerger.DepthStencilState = _defaultResources.DepthStencilStateDefault;
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _defaultResources = resources.DefaultResources;
            _vertexShader = resources.GetResourceAndEnsureLoaded(
                s_resKeyVertexShader,
                () => GraphicsHelper.Internals.GetVertexShaderResource(device, "Postprocessing", "PostprocessVertexShader"));
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _defaultResources = null;
            _vertexShader = null;
        }
    }
}
