using System;
using System.Threading.Tasks;
using GDI = System.Drawing;

namespace SeeingSharp.Core
{
    public static class RenderLoopEx
    {
        /// <summary>
        /// Takes a screenshot and returns it as a gdi bitmap.
        /// </summary>
        public static Task<GDI.Bitmap> GetScreenshotGdiAsync(this RenderLoop renderLoop)
        {
            var result = new TaskCompletionSource<GDI.Bitmap>(TaskCreationOptions.RunContinuationsAsynchronously);

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
            if (currentDevice == null)
            {
                throw new SeeingSharpException("No rendering device set!");
            }

            // Concept behind this see http://www.rolandk.de/wp/2013/06/inhalt-der-rendertarget-textur-in-ein-bitmap-kopieren/
            var width = currentViewSize.Width;
            var height = currentViewSize.Height;
            if (width <= 0) { throw new SeeingSharpException("View not initialized correctly!"); }
            if (height <= 0) { throw new SeeingSharpException("View not initialized correctly!"); }

            // Get and read data from the gpu (create copy helper texture on demand)
            if (renderLoop.Internals.CopyHelperTextureStaging == null)
            {
                renderLoop.Internals.CopyHelperTextureStaging = GraphicsHelper.Internals.CreateStagingTexture(currentDevice, width, height);
                renderLoop.Internals.CopyHelperTextureStandard = GraphicsHelper.Internals.CreateTexture(currentDevice, width, height);
            }

            // Copy resources
            currentDevice.Internals.DeviceImmediateContextD3D11.ResolveSubresource(
                renderLoop.Internals.CopyHelperTextureStandard, 0,
                renderLoop.Internals.RenderTarget, 0,
                GraphicsHelper.Internals.DEFAULT_TEXTURE_FORMAT);
            currentDevice.Internals.DeviceImmediateContextD3D11.CopyResource(
                renderLoop.Internals.CopyHelperTextureStaging, 
                renderLoop.Internals.CopyHelperTextureStandard);

            // Load the bitmap
            var resultBitmap = GraphicsHelperWinForms.LoadBitmapFromStagingTexture(currentDevice, renderLoop.Internals.CopyHelperTextureStaging, width, height);
            return resultBitmap;
        }
    }
}