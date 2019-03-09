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

using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;
using SharpDX;
using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Objects
{
    public class SkyboxObject : SceneObject
    {
        //Resources
        private IndexBasedDynamicCollection<SkyboxLocalResources> m_localResources;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkyboxObject"/> class.
        /// </summary>
        public SkyboxObject(NamedOrGenericKey cubeTextureKey)
        {
            m_localResources = new IndexBasedDynamicCollection<SkyboxLocalResources>();
            CubeTextureKey = cubeTextureKey;
        }

        /// <summary>
        /// Loads all resources of the object.
        /// </summary>
        /// <param name="device">Current graphics device.</param>
        /// <param name="resourceDictionary">Current resource dicionary.</param>
        public override void LoadResources(EngineDevice device, ResourceDictionary resourceDictionary)
        {
            // Define all vertices
            StandardVertex[] vertices = {
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

            // Define all indices
            int[] indices = {
                0, 1, 2, 2, 3, 0,        // Font side
                4, 5, 6, 6, 7, 4,        // Right side
                8, 9, 10, 10, 11, 8,     // Back side
                12, 13, 14, 14, 15, 12,  // Left side
                16, 17, 18, 18, 19, 16,  // Top side
                20, 21, 22, 22, 23, 20   // Down side
            };

            // Create and fill resource container object
            var localResources = new SkyboxLocalResources
            {
                DefaultResources = resourceDictionary.DefaultResources,
                CubeTexture = resourceDictionary.GetResourceAndEnsureLoaded<TextureResource>(CubeTextureKey),
                VertexBuffer = GraphicsHelper.CreateImmutableVertexBuffer(device, vertices),
                IndexBuffer = GraphicsHelper.CreateImmutableIndexBuffer(device, indices),
                VertexShader = resourceDictionary.GetResourceAndEnsureLoaded(
                    ResourceKeys.RES_VERTEX_SHADER_SKYBOX,
                    () => GraphicsHelper.GetVertexShaderResource(device, "SkyBox", "CommonVertexShader")),
                PixelShader = resourceDictionary.GetResourceAndEnsureLoaded(
                    ResourceKeys.RES_PIXEL_SHADER_SKYBOX,
                    () => GraphicsHelper.GetPixelShaderResource(device, "SkyBox", "CommonPixelShader"))
            };

            // Store resource container object
            m_localResources.AddObject(localResources, device.DeviceIndex);
        }

        /// <summary>
        /// Unloads all resources of this object.
        /// </summary>
        public override void UnloadResources()
        {
            base.UnloadResources();

            // Dispose all locally created resources
            foreach(var actLocalResource in m_localResources)
            {
                if (actLocalResource == null) { continue; }

                SeeingSharpUtil.SafeDispose(ref actLocalResource.IndexBuffer);
                SeeingSharpUtil.SafeDispose(ref actLocalResource.VertexBuffer);
            }

            // Clear local resource container
            m_localResources.Clear();
        }

        /// <summary>
        /// Are resources loaded for the given device?
        /// </summary>
        /// <param name="device">The device for which to check.</param>
        public override bool IsLoaded(EngineDevice device)
        {
            if (!m_localResources.HasObjectAt(device.DeviceIndex))
            {
                return false;
            }

            var geoResource = m_localResources[device.DeviceIndex];

            if (geoResource.CubeTexture == null)
            {
                return false;
            }

            if (geoResource.CubeTexture.Key != CubeTextureKey)
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
        /// <param name="layerViewSubset">The layer view subset wich called this update method.</param>
        protected override void UpdateForViewInternal(SceneRelatedUpdateState updateState, ViewRelatedSceneLayerSubset layerViewSubset)
        {
            if(CountRenderPassSubscriptions(layerViewSubset) <= 0)
            {
                SubscribeToPass(RenderPassInfo.PASS_PLAIN_RENDER, layerViewSubset, Render);
            }
        }

        /// <summary>
        /// Renders this object.
        /// </summary>
        /// <param name="renderState">Render this object.</param>
        private void Render(RenderState renderState)
        {
            renderState.ClearChachedAppliedMaterial();

            var deviceContext = renderState.Device.DeviceImmediateContextD3D11;
            var localResources = m_localResources[renderState.DeviceIndex];

            // Apply constants and shader resources
            deviceContext.VertexShader.Set(localResources.VertexShader.VertexShader);
            deviceContext.PixelShader.Set(localResources.PixelShader.PixelShader);
            deviceContext.PixelShader.SetShaderResource(0, localResources.CubeTexture.TextureView);

            // Bind index and vertex buffer
            deviceContext.InputAssembler.SetIndexBuffer(localResources.IndexBuffer, Format.R32_UInt, 0);
            deviceContext.InputAssembler.SetVertexBuffers(0, new D3D11.VertexBufferBinding(
                localResources.VertexBuffer,
                StandardVertex.Size, 0));

            // Draw the skybox
            deviceContext.DrawIndexed(6 * 6, 0, 0);
        }

        /// <summary>
        /// Gets or sets the key of the cube texture.
        /// </summary>
        public NamedOrGenericKey CubeTextureKey { get; set; }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        /// <summary>
        /// Helper class that holds all resources of a skybox.
        /// </summary>
        private class SkyboxLocalResources
        {
            public TextureResource CubeTexture;
            public DefaultResources DefaultResources;
            public D3D11.Buffer IndexBuffer;
            public PixelShaderResource PixelShader;
            public D3D11.Buffer VertexBuffer;
            public VertexShaderResource VertexShader;
        }
    }
}