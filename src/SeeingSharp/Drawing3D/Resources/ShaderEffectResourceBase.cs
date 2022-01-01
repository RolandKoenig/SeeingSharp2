using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;
using D3D11 = Vortice.Direct3D11;

namespace SeeingSharp.Drawing3D.Resources
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
        protected void ApplySpriteRendering(D3D11.ID3D11DeviceContext deviceContext)
        {
            deviceContext.VSSetShader(_vertexShader.VertexShader);
            deviceContext.OMSetDepthStencilState(_defaultResources.DepthStencilStateAlwaysPassDepth);
        }

        /// <summary>
        /// Discards alpha based sprite rendering.
        /// </summary>
        /// <param name="deviceContext">The target device context.</param>
        protected void DiscardSpriteRendering(D3D11.ID3D11DeviceContext deviceContext)
        {
            deviceContext.OMSetDepthStencilState(_defaultResources.DepthStencilStateDefault);
        }

        /// <summary>
        /// Applies alpha based sprite rendering.
        /// </summary>
        /// <param name="deviceContext">The target device context.</param>
        protected void ApplyAlphaBasedSpriteRendering(D3D11.ID3D11DeviceContext deviceContext)
        {
            deviceContext.VSSetShader(_vertexShader.VertexShader);

            deviceContext.OMSetDepthStencilState(_defaultResources.DepthStencilStateAlwaysPassDepth);
            deviceContext.OMSetBlendState(_defaultResources.AlphaBlendingBlendState);
        }

        /// <summary>
        /// Discards alpha based sprite rendering.
        /// </summary>
        /// <param name="deviceContext">The target device context.</param>
        protected void DiscardAlphaBasedSpriteRendering(D3D11.ID3D11DeviceContext deviceContext)
        {
            deviceContext.OMSetBlendState(_defaultResources.DefaultBlendState);
            deviceContext.OMSetDepthStencilState(_defaultResources.DepthStencilStateDefault);
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
