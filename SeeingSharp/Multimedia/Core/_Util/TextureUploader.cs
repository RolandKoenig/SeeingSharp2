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
#region using

// Some namespace mappings
using System;
using SeeingSharp.Util;
using SharpDX;
using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

#endregion

namespace SeeingSharp.Multimedia.Core
{
    #region using
    #endregion

    public class TextureUploader : IDisposable
    {
        // Given parameters
        private EngineDevice m_device;
        private D3D11.Texture2D m_texture;
        private int m_width;
        private int m_height;
        private Format m_format;
        private bool m_isMultisampled;

        //Direct3D resources for rendertarget capturing
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

#if DESKTOP
        /// <summary>
        /// Takes a screenshot and returns it as a gdi bitmap.
        /// </summary>
        public GDI.Bitmap UploadToGdiBitmap()
        {
            // Check current format
            if ((m_format != GraphicsHelper.DEFAULT_TEXTURE_FORMAT) &&
               (m_format != GraphicsHelper.DEFAULT_TEXTURE_FORMAT_SHARING) &&
               (m_format != GraphicsHelper.DEFAULT_TEXTURE_FORMAT_SHARING_D2D))
            {
                throw new SeeingSharpGraphicsException("Invalid format for texture uploading to gdi bitmap (" + m_format + ")!");
            }

            // Upload the texture
            CopyTextureToStagingResource();

            // Load the bitmap
            GDI.Bitmap resultBitmap = GraphicsHelper.LoadBitmapFromStagingTexture(
                m_device,
                m_copyHelperTextureStaging, m_width, m_height);

            return resultBitmap;
        }
#endif

        /// <summary>
        /// Takes a color texture and uploads it to the given buffer.
        /// </summary>
        /// <param name="intBuffer">The target int buffer to which to copy all pixel data.</param>
        public void UploadToIntBuffer(MemoryMappedTexture32bpp intBuffer)
        {
            // Check current format
            if (m_format != GraphicsHelper.DEFAULT_TEXTURE_FORMAT &&
                m_format != GraphicsHelper.DEFAULT_TEXTURE_FORMAT_SHARING &&
                m_format != GraphicsHelper.DEFAULT_TEXTURE_FORMAT_SHARING_D2D)
            {
                throw new SeeingSharpGraphicsException(string.Format("Invalid format for texture uploading to a color map ({0})!", m_format));
            }

            // Upload the texture
            CopyTextureToStagingResource();

            // Read the data into the .Net data block
            var dataBox = m_device.DeviceImmediateContextD3D11.MapSubresource(
                m_copyHelperTextureStaging, 0, D3D11.MapMode.Read, D3D11.MapFlags.None);
            try
            {
                var rowPitchSource = dataBox.RowPitch;
                var rowPitchDestination = intBuffer.Width * 4;

                if (rowPitchSource > 0 && rowPitchSource < 20000 &&
                    rowPitchDestination > 0 && rowPitchDestination < 20000)
                {
                    for (var loopY = 0; loopY < m_height; loopY++)
                    {
                        SeeingSharpTools.CopyMemory(
                            dataBox.DataPointer + loopY * rowPitchSource,
                            intBuffer.Pointer + loopY * rowPitchDestination,
                            (ulong)rowPitchDestination);
                    }
                }
            }
            finally
            {
                m_device.DeviceImmediateContextD3D11.UnmapSubresource(m_copyHelperTextureStaging, 0);
            }
        }

        /// <summary>
        /// Upload a floatingpoint texture from the graphics hardware.
        /// This method is only valid for resources of type R32_Floatfloat.
        /// </summary>
        public MemoryMappedTextureFloat UploadToFloatBuffer()
        {
            var result = new MemoryMappedTextureFloat(
                new Size2(m_width, m_height));
            UploadToFloatBuffer(result);
            return result;
        }

        /// <summary>
        /// Upload a floatingpoint texture from the graphics hardware.
        /// This method is only valid for resources of type R32_Floatfloat.
        /// </summary>
        /// <param name="floatBuffer">The target float buffer to which to copy all ObjectIDs.</param>
        public void UploadToFloatBuffer(MemoryMappedTextureFloat floatBuffer)
        {
            // Check current format
            if (m_format != GraphicsHelper.DEFAULT_TEXTURE_FORMAT_OBJECT_ID)
            {
                throw new SeeingSharpGraphicsException("Invalid format for texture uploading to gdi bitmap (" + m_format + ")!");
            }

            if (floatBuffer.Width != m_width)
            {
                throw new SeeingSharpGraphicsException("The width of the textures during texture upload does not match!");
            }

            if (floatBuffer.Height != m_height)
            {
                throw new SeeingSharpGraphicsException("The height of the textures during texture upload does not match!");
            }

            // Upload the texture
            CopyTextureToStagingResource();

            // Read the data into the .Net data block
            var dataBox = m_device.DeviceImmediateContextD3D11.MapSubresource(
                m_copyHelperTextureStaging, 0, D3D11.MapMode.Read, D3D11.MapFlags.None);
            try
            {

                var rowPitchSource = dataBox.RowPitch;
                var rowPitchDestination = floatBuffer.Width * 4;

                if (rowPitchSource > 0 && rowPitchSource < 20000 &&
                    rowPitchDestination > 0 && rowPitchDestination < 20000)
                {
                    for (var loopY = 0; loopY < m_height; loopY++)
                    {
                        SeeingSharpTools.CopyMemory(
                            dataBox.DataPointer + loopY * rowPitchSource,
                            floatBuffer.Pointer + loopY * rowPitchDestination,
                            (ulong)rowPitchDestination);
                    }
                }
            }
            finally
            {
                m_device.DeviceImmediateContextD3D11.UnmapSubresource(m_copyHelperTextureStaging, 0);
            }
        }

        /// <summary>
        /// Loads the target texture int a staging texture.
        /// </summary>
        private void CopyTextureToStagingResource(bool handleMultiSampling = true)
        {
            // Prepare needed textures
            if (m_copyHelperTextureStaging == null)
            {
                m_copyHelperTextureStaging = GraphicsHelper.CreateStagingTexture(m_device, m_width, m_height, m_format);
                if (m_isMultisampled && handleMultiSampling)
                {
                    m_copyHelperTextureStandard = GraphicsHelper.CreateTexture(m_device, m_width, m_height, m_format);
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

        /// <summary>
        /// Führt anwendungsspezifische Aufgaben aus, die mit dem Freigeben, Zurückgeben oder Zurücksetzen von nicht verwalteten Ressourcen zusammenhängen.
        /// </summary>
        public void Dispose()
        {
            SeeingSharpTools.SafeDispose(ref m_copyHelperTextureStaging);
            SeeingSharpTools.SafeDispose(ref m_copyHelperTextureStandard);
        }
    }
}
