using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Namespace mappings
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
            TaskCompletionSource<GDI.Bitmap> result = new TaskCompletionSource<GDI.Bitmap>();

            renderLoop.EnqueueAfterPresentAction(() =>
            {
                try
                {
                    GDI.Bitmap resultBitmap = GetScreenshotGdiInternal(renderLoop);
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
            int width = currentViewSize.Width;
            int height = currentViewSize.Height;
            if (width <= 0) { throw new InvalidOperationException("View not initialized correctly!"); }
            if (height <= 0) { throw new InvalidOperationException("View not initialized correctly!"); }

            // Get and read data from the gpu (create copy helper texture on demand)
            if (renderLoop.Internals.CopyHelperTextureStaging == null)
            {
                renderLoop.Internals.CopyHelperTextureStaging = GraphicsHelper.CreateStagingTexture(currentDevice, width, height);
                renderLoop.Internals.CopyHelperTextureStandard = GraphicsHelper.CreateTexture(currentDevice, width, height);
            }

            // Copy resources
            currentDevice.DeviceImmediateContextD3D11.ResolveSubresource(
                renderLoop.Internals.RenderTarget, 0,
                renderLoop.Internals.CopyHelperTextureStandard, 0, 
                GraphicsHelper.DEFAULT_TEXTURE_FORMAT);
            currentDevice.DeviceImmediateContextD3D11.CopyResource(
                renderLoop.Internals.CopyHelperTextureStandard,
                renderLoop.Internals.CopyHelperTextureStaging);

            // Load the bitmap
            GDI.Bitmap resultBitmap = GraphicsHelperWinForms.LoadBitmapFromStagingTexture(currentDevice, renderLoop.Internals.CopyHelperTextureStaging, width, height);
            return resultBitmap;
        }
    }
}
