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
using SeeingSharp.Util;
using System;
using SeeingSharp.Checking;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;

namespace SeeingSharp.Multimedia.Core
{
    public class TextureUploader : IDisposable, ICheckDisposed
    {
        // Given parameters
        private EngineDevice m_device;
        private D3D11.Texture2D m_texture;
        private int m_width;
        private int m_height;
        private DXGI.Format m_format;
        private bool m_isMultisampled;
        private bool m_isDisposed;

        //Direct3D resources for render target capturing
        // A staging texture for reading contents by Cpu
        // A standard texture for copying data from multisample texture to standard one
        // see http://www.rolandk.de/wp/2013/06/inhalt-der-rendertarget-textur-in-ein-bitmap-kopieren/
        private D3D11.Texture2D m_copyHelperTextureStaging;
        private D3D11.Texture2D m_copyHelperTextureStandard;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureUploader"/> class.
        /// </summary>
        /// <param name="device">The device on which the texture was created.</param>
        /// <param name="texture">The texture which is to be uploaded to system memory.</param>
        internal TextureUploader(EngineDevice device, D3D11.Texture2D texture)
        {
            var textureDesc = texture.Description;

            m_device = device;
            m_texture = texture;
            m_width = textureDesc.Width;
            m_height = textureDesc.Height;
            m_format = textureDesc.Format;
            m_isMultisampled = textureDesc.SampleDescription.Count > 1 || textureDesc.SampleDescription.Quality > 0;
        }

        /// <summary>
        /// Upload a texture from the graphics hardware.
        /// </summary>
        public MemoryMappedTexture<T> UploadToMemoryMappedTexture<T>()
            where T : unmanaged
        {
            if (m_isDisposed) { throw new ObjectDisposedException(nameof(TextureUploader)); }

            var result = new MemoryMappedTexture<T>(
                new Size2(m_width, m_height));
            this.UploadToMemoryMappedTexture(result);
            return result;
        }

        /// <summary>
        /// Upload a texture from the graphics hardware.
        /// </summary>
        /// <param name="targetFloatBuffer">The target buffer to which to copy all data.</param>
        public unsafe void UploadToMemoryMappedTexture<T>(MemoryMappedTexture<T> targetFloatBuffer) 
            where T : unmanaged
        {
            if (m_isDisposed) { throw new ObjectDisposedException(nameof(TextureUploader)); }
            this.EnsureNotNullOrDisposed(nameof(targetFloatBuffer));

            // Check source and destination size
            var targetPixelSize = targetFloatBuffer.PixelSize;
            if (targetPixelSize.Width != m_width)
            {
                throw new SeeingSharpGraphicsException("The width of the textures during texture upload does not match!");
            }
            if (targetPixelSize.Height != m_height)
            {
                throw new SeeingSharpGraphicsException("The height of the textures during texture upload does not match!");
            }

            // Check format compatibility
            var textureFormatByteSize = DXGI.FormatHelper.SizeOfInBytes(m_format);
            if (textureFormatByteSize != sizeof(T))
            {
                throw new SeeingSharpGraphicsException(
                    $"Format of the texture to upload and the destination buffer does not match " +
                    $"(source: {m_format} / {textureFormatByteSize} bytes, target: {typeof(T).Name} / {sizeof(T)} bytes)!");
            }

            // Upload the texture
            this.CopyTextureToStagingResource();

            // Read the data into the .Net data block
            var dataBox = m_device.DeviceImmediateContextD3D11.MapSubresource(
                m_copyHelperTextureStaging, 0, D3D11.MapMode.Read, D3D11.MapFlags.None);
            try
            {
                var rowPitchSource = dataBox.RowPitch;
                var rowPitchDestination = targetFloatBuffer.Width * sizeof(T);
                if ((rowPitchSource > 0) && (rowPitchDestination > 0))
                {
                    for (var loopY = 0; loopY < m_height; loopY++)
                    {
                        SeeingSharpUtil.CopyMemory(
                            dataBox.DataPointer + loopY * rowPitchSource,
                            targetFloatBuffer.Pointer + loopY * rowPitchDestination,
                            (uint)rowPitchDestination);
                    }
                }
                else
                {
                    throw new SeeingSharpGraphicsException($"Invalid row pitch (source: {rowPitchSource}, destination: {rowPitchDestination})!");
                }
            }
            finally
            {
                m_device.DeviceImmediateContextD3D11.UnmapSubresource(m_copyHelperTextureStaging, 0);
            }
        }

        public void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref m_copyHelperTextureStaging);
            SeeingSharpUtil.SafeDispose(ref m_copyHelperTextureStandard);
            m_isDisposed = true;
        }

        /// <summary>
        /// Loads the target texture int a staging texture.
        /// </summary>
        private void CopyTextureToStagingResource(bool handleMultiSampling = true)
        {
            // Prepare needed textures
            if (m_copyHelperTextureStaging == null)
            {
                m_copyHelperTextureStaging = GraphicsHelper.Internals.CreateStagingTexture(m_device, m_width, m_height, m_format);
                if (m_isMultisampled && handleMultiSampling)
                {
                    m_copyHelperTextureStandard = GraphicsHelper.Internals.CreateTexture(m_device, m_width, m_height, m_format);
                }
            }

            // Copy contents of the texture
            //  .. execute a ResolveSubresource before if the source texture is multisampled
            if (m_isMultisampled && handleMultiSampling)
            {
                m_device.DeviceImmediateContextD3D11.ResolveSubresource(m_texture, 0, m_copyHelperTextureStandard, 0, m_format);
                m_device.DeviceImmediateContextD3D11.CopyResource(m_copyHelperTextureStandard, m_copyHelperTextureStaging);
            }
            else
            {
                m_device.DeviceImmediateContextD3D11.CopyResource(m_texture, m_copyHelperTextureStaging);
            }
        }

        /// <inheritdoc />
        public bool IsDisposed => m_isDisposed;
    }
}
