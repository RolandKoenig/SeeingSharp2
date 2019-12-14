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
using System;
using System.Threading.Tasks;
using GDI = System.Drawing;

namespace SeeingSharp.Multimedia.Core
{
    public static class RenderLoopEx
    {
        /// <summary>
        /// Takes a screenshot and returns it as a gdi bitmap.
        /// </summary>
        public static Task<GDI.Bitmap> GetScreenshotGdiAsync(this RenderLoop renderLoop)
        {
            var result = new TaskCompletionSource<GDI.Bitmap>();

            renderLoop.EnqueueAfterPresentAction(() =>
            {
                try
                {
                    var resultBitmap = GetScreenshotGdiInternal(renderLoop);
                    result.SetResult(resultBitmap);
                }
                catch (Exception ex)
                {
                    result.SetException(ex);
                }
            });

            return result.Task;
        }

        /// <summary>
        /// Gets a screenshot in form of a gdi bitmap.
        /// (be careful. This call is executed synchronous.
        /// </summary>
        private static GDI.Bitmap GetScreenshotGdiInternal(RenderLoop renderLoop)
        {
            var currentViewSize = renderLoop.CurrentViewSize;
            var currentDevice = renderLoop.Device;

            // Concept behind this see http://www.rolandk.de/wp/2013/06/inhalt-der-rendertarget-textur-in-ein-bitmap-kopieren/
            var width = currentViewSize.Width;
            var height = currentViewSize.Height;
            if (width <= 0) { throw new InvalidOperationException("View not initialized correctly!"); }
            if (height <= 0) { throw new InvalidOperationException("View not initialized correctly!"); }

            // Get and read data from the gpu (create copy helper texture on demand)
            if (renderLoop.Internals.CopyHelperTextureStaging == null)
            {
                renderLoop.Internals.CopyHelperTextureStaging = GraphicsHelper.CreateStagingTexture(currentDevice, width, height);
                renderLoop.Internals.CopyHelperTextureStandard = GraphicsHelper.CreateTexture(currentDevice, width, height);
            }

            // Copy resources
            currentDevice.Internals.DeviceImmediateContextD3D11.ResolveSubresource(
                renderLoop.Internals.RenderTarget, 0,
                renderLoop.Internals.CopyHelperTextureStandard, 0,
                GraphicsHelper.DEFAULT_TEXTURE_FORMAT);
            currentDevice.Internals.DeviceImmediateContextD3D11.CopyResource(
                renderLoop.Internals.CopyHelperTextureStandard,
                renderLoop.Internals.CopyHelperTextureStaging);

            // Load the bitmap
            var resultBitmap = GraphicsHelperWinForms.LoadBitmapFromStagingTexture(currentDevice, renderLoop.Internals.CopyHelperTextureStaging, width, height);
            return resultBitmap;
        }
    }
}