using System.Numerics;
using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;
using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Drawing3D
{
    public class Skybox : SceneObject
    {
        // Resources
        private IndexBasedDynamicCollection<SkyboxLocalResources> _localResources;

        /// <summary>
        /// Gets or sets the key of the cube texture.
        /// </summary>
        public NamedOrGenericKey CubeTextureKey { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Skybox"/> class.
        /// </summary>
        public Skybox(NamedOrGenericKey cubeTextureKey)
        {
            _localResources = new IndexBasedDynamicCollection<SkyboxLocalResources>();
            this.CubeTextureKey = cubeTextureKey;
        }

        /// <summary>
        /// Loads all resources of the object.
        /// </summary>
        /// <param name="device">Current graphics device.</param>
        /// <param name="resourceDictionary">Current resource dictionary.</param>
        public override void LoadResources(EngineDevice device, ResourceDictionary resourceDictionary)
        {
            // Define all vertices
            StandardVertex[] CreateVertices()
            {
                return new[] {
                    // Front side
                    new StandardVertex(new Vector3(-1f, +1f, -1f), new Vector2(0f, 0f)),
                    new StandardVertex(new Vector3(-1f, -1f, -1f), new Vector2(0f, 1f)),
                    new StandardVertex(new Vector3(+1f, -1f, -1f), new Vector2(1f, 1f)),
                    new StandardVertex(new Vector3(+1f, +1f, -1f), new Vector2(1f, 0f)),

                    // Right side
                    new StandardVertex(new Vector3(+1f, +1f, -1f), new Vector2(0f, 0f)),
                    new StandardVertex(new Vector3(+1f, -1f, -1f), new Vector2(0f, 1f)),
                    new StandardVertex(new Vector3(+1f, -1f, +1f), new Vector2(1f, 1f)),
                    new StandardVertex(new Vector3(+1f, +1f, +1f), new Vector2(1f, 0f)),

                    // Back side
                    new StandardVertex(new Vector3(+1f, +1f, +1f), new Vector2(0f, 0f)),
                    new StandardVertex(new Vector3(+1f, -1f, +1f), new Vector2(0f, 1f)),
                    new StandardVertex(new Vector3(-1f, -1f, +1f), new Vector2(1f, 1f)),
                    new StandardVertex(new Vector3(-1f, +1f, +1f), new Vector2(1f, 0f)),

                    // Left side
                    new StandardVertex(new Vector3(-1f, +1f, +1f), new Vector2(0f, 0f)),
                    new StandardVertex(new Vector3(-1f, -1f, +1f), new Vector2(0f, 1f)),
                    new StandardVertex(new Vector3(-1f, -1f, -1f), new Vector2(1f, 1f)),
                    new StandardVertex(new Vector3(-1f, +1f, -1f), new Vector2(1f, 0f)),

                    // Top side
                    new StandardVertex(new Vector3(-1f, +1f, +1f), new Vector2(0f, 0f)),
                    new StandardVertex(new Vector3(-1f, +1f, -1f), new Vector2(0f, 1f)),
                    new StandardVertex(new Vector3(+1f, +1f, -1f), new Vector2(1f, 1f)),
                    new StandardVertex(new Vector3(+1f, +1f, +1f), new Vector2(1f, 0f)),

                    // Down side
                    new StandardVertex(new Vector3(+1f, -1f, -1f), new Vector2(0f, 0f)),
                    new StandardVertex(new Vector3(-1f, -1f, -1f), new Vector2(1f, 0f)),
                    new StandardVertex(new Vector3(-1f, -1f, +1f), new Vector2(1f, 1f)),
                    new StandardVertex(new Vector3(+1f, -1f, +1f), new Vector2(0f, 1f))
                };
            }

            // Define all indices
            int[] CreateIndices()
            {
                return new[]
                {
                    0, 1, 2, 2, 3, 0, // Font side
                    4, 5, 6, 6, 7, 4, // Right side
                    8, 9, 10, 10, 11, 8, // Back side
                    12, 13, 14, 14, 15, 12, // Left side
                    16, 17, 18, 18, 19, 16, // Top side
                    20, 21, 22, 22, 23, 20 // Down side
                };
            }

            // Create and fill resource container object
            var localResources = new SkyboxLocalResources
            {
                CubeTexture = resourceDictionary.GetResourceAndEnsureLoaded<TextureResource>(this.CubeTextureKey),
                VertexBuffer = resourceDictionary.GetResourceAndEnsureLoaded(
                    ResourceKeys.RES_SKYBOX_VERTICES,
                    () => new ImmutableVertexBufferResource<StandardVertex>(CreateVertices)),
                IndexBuffer = resourceDictionary.GetResourceAndEnsureLoaded(
                    ResourceKeys.RES_SKYBOX_INDICES,
                    () => new ImmutableIndexBufferResource(CreateIndices)),
                VertexShader = resourceDictionary.GetResourceAndEnsureLoaded(
                    ResourceKeys.RES_SKYBOX_VERTEX_SHADER,
                    () => GraphicsHelper.Internals.GetVertexShaderResource(device, "SkyBox", "CommonVertexShader")),
                PixelShader = resourceDictionary.GetResourceAndEnsureLoaded(
                    ResourceKeys.RES_SKYBOX_PIXEL_SHADER,
                    () => GraphicsHelper.Internals.GetPixelShaderResource(device, "SkyBox", "CommonPixelShader"))
            };

            // Store resource container object
            _localResources.AddObject(localResources, device.DeviceIndex);
        }

        /// <summary>
        /// Unloads all resources of this object.
        /// </summary>
        public override void UnloadResources()
        {
            base.UnloadResources();

            // Dispose all locally created resources
            foreach (var actLocalResource in _localResources)
            {
                if (actLocalResource == null) { continue; }

                SeeingSharpUtil.SafeDispose(ref actLocalResource.IndexBuffer);
                SeeingSharpUtil.SafeDispose(ref actLocalResource.VertexBuffer);
            }

            // Clear local resource container
            _localResources.Clear();
        }

        /// <summary>
        /// Are resources loaded for the given device?
        /// </summary>
        /// <param name="device">The device for which to check.</param>
        public override bool IsLoaded(EngineDevice device)
        {
            if (!_localResources.HasObjectAt(device.DeviceIndex))
            {
                return false;
            }

            var geoResource = _localResources[device.DeviceIndex];
            if (geoResource.CubeTexture == null)
            {
                return false;
            }

            if (geoResource.CubeTexture.Key != this.CubeTextureKey)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Updates the object.
        /// </summary>
        /// <param name="updateState">Current update state.</param>
        protected override void UpdateInternal(SceneRelatedUpdateState updateState)
        {

        }

        /// <summary>
        /// Updates this object for the given view.
        /// </summary>
        /// <param name="updateState">Current state of the update pass.</param>
        /// <param name="layerViewSubset">The layer view subset which called this update method.</param>
        protected override void UpdateForViewInternal(SceneRelatedUpdateState updateState, ViewRelatedSceneLayerSubset layerViewSubset)
        {
            if (this.CountRenderPassSubscriptions(layerViewSubset) <= 0)
            {
                this.SubscribeToPass(RenderPassInfo.PASS_PLAIN_RENDER, layerViewSubset, this.Render);
            }
        }

        /// <summary>
        /// Renders this object.
        /// </summary>
        /// <param name="renderState">Render this object.</param>
        private void Render(RenderState renderState)
        {
            renderState.ClearCachedAppliedMaterial();

            var deviceContext = renderState.Device.DeviceImmediateContextD3D11;
            var localResources = _localResources[renderState.DeviceIndex];

            // Apply constants and shader resources
            deviceContext.VertexShader.Set(localResources.VertexShader.VertexShader);
            deviceContext.PixelShader.Set(localResources.PixelShader.PixelShader);
            deviceContext.PixelShader.SetShaderResource(0, localResources.CubeTexture.TextureView);

            // Bind index and vertex buffer
            deviceContext.InputAssembler.SetIndexBuffer(localResources.IndexBuffer.Buffer, Format.R32_UInt, 0);
            deviceContext.InputAssembler.SetVertexBuffers(0, new D3D11.VertexBufferBinding(
                localResources.VertexBuffer.Buffer,
                StandardVertex.Size, 0));

            // Draw the skybox
            deviceContext.DrawIndexed(6 * 6, 0, 0);
            renderState.CountDrawCallsInternal++;
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        /// <summary>
        /// Helper class that holds all resources of a skybox.
        /// </summary>
        private class SkyboxLocalResources
        {
            public TextureResource CubeTexture;
            public ImmutableIndexBufferResource IndexBuffer;
            public PixelShaderResource PixelShader;
            public ImmutableVertexBufferResource<StandardVertex> VertexBuffer;
            public VertexShaderResource VertexShader;
        }
    }
}