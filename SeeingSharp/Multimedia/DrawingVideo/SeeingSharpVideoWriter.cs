#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
using D3D11 = SharpDX.Direct3D11;

#endregion

namespace SeeingSharp.Multimedia.DrawingVideo
{
    #region using

    using System;
    using Checking;
    using Core;
    using SeeingSharp.Util;
    using SharpDX;

    #endregion

    /// <summary>
    /// A common base class for all video writers provided by the graphics engine.
    /// </summary>
    public abstract class SeeingSharpVideoWriter
    {
        #region Configuration

        #endregion

        #region Runtime values

        #endregion

        /// <summary>
        /// Occurs when recording was finished (by success or failure).
        /// </summary>
        public event EventHandler RecordingFinished;

        /// <summary>
        /// Initializes a new instance of the <see cref="SeeingSharpVideoWriter"/> class.
        /// </summary>
        /// <param name="targetFile">The target file to write to.</param>
        public SeeingSharpVideoWriter(ResourceLink targetFile)
        {
            TargetFile = targetFile;
        }

        /// <summary>
        /// Starts to render the video.
        /// </summary>
        internal void StartRendering(Size2 videoSize)
        {
            videoSize.EnsureNotEmpty(nameof(videoSize));
            if (HasStarted) { throw new SeeingSharpGraphicsException($"{nameof(SeeingSharpVideoWriter)} has already started before!"); }
            if (HasFinished) { throw new SeeingSharpGraphicsException($"{nameof(SeeingSharpVideoWriter)} has already finished before!"); }

            VideoSize = videoSize;

            // Reset exceptions
            LastDrawException = null;
            LastStartException = null;
            LastFinishException = null;

            // Ensure that the target directory exists
            try
            {
                StartRenderingInternal(VideoSize);
                HasStarted = true;
            }
            catch (Exception ex)
            {
                LastStartException = ex;
                HasStarted = false;
            }
        }

        /// <summary>
        /// Draws the given frame to the video.
        /// </summary>
        /// <param name="device">The device on which the given framebuffer is created.</param>
        /// <param name="uploadedTexture">The texture which should be added to the video.</param>
        public void DrawFrame(EngineDevice device, MemoryMappedTexture32bpp uploadedTexture)
        {
            try
            {
                device.EnsureNotNull(nameof(device));
                uploadedTexture.EnsureNotNull(nameof(uploadedTexture));
                if (!HasStarted) { throw new SeeingSharpGraphicsException($"{nameof(SeeingSharpVideoWriter)} is not started!"); }
                if (HasFinished) { throw new SeeingSharpGraphicsException($"{nameof(SeeingSharpVideoWriter)} has already finished before!"); }

                // Check for correct image size
                if (VideoSize != uploadedTexture.PixelSize)
                {
                    throw new SeeingSharpGraphicsException("Size has changed during recording!");
                }

                DrawFrameInternal(device, uploadedTexture);
            }
            catch (Exception ex)
            {
                LastDrawException = ex;
            }
        }

        /// <summary>
        /// Finished the rendered video.
        /// </summary>
        internal void FinishRendering()
        {
            try
            {
                if (!HasStarted) { throw new SeeingSharpGraphicsException($"{nameof(SeeingSharpVideoWriter)} is not started!"); }
                if (HasFinished) { throw new SeeingSharpGraphicsException($"{nameof(SeeingSharpVideoWriter)} has already finished before!"); }

                FinishRenderingInternal();
            }
            catch (Exception ex)
            {
                LastFinishException = ex;
            }
            finally
            {
                HasFinished = true;
                RecordingFinished.Raise(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Checks whether changes on the configuration of this object are valid currently.
        /// The method throws an exception, if not.
        /// </summary>
        protected void CheckWhetherChangesAreValid()
        {
            if (HasStarted || HasFinished) { throw new SeeingSharpGraphicsException("Unable to do changed when VideoWriter is running!"); }
        }

        /// <summary>
        /// Starts rendering to the target.
        /// </summary>
        /// <param name="videoPixelSize">The pixel size of the video.</param>
        protected abstract void StartRenderingInternal(Size2 videoPixelSize);

        /// <summary>
        /// Draws the given frame to the video.
        /// </summary>
        /// <param name="device">The device on which the given framebuffer is created.</param>
        /// <param name="uploadedTexture">The texture which should be added to the video.</param>
        protected abstract void DrawFrameInternal(EngineDevice device, MemoryMappedTexture32bpp uploadedTexture);

        /// <summary>
        /// Finishes rendering to the target (closes the video file).
        /// Video rendering can not be started again from this point on.
        /// </summary>
        protected abstract void FinishRenderingInternal();

        /// <summary>
        /// Gets the target file this VideoWriter is writing to.
        /// </summary>
        public ResourceLink TargetFile { get; }

        /// <summary>
        /// Has rendering started?
        /// </summary>
        public bool HasStarted { get; private set; }

        /// <summary>
        /// Has rendering finished?
        /// </summary>
        public bool HasFinished { get; private set; }

        /// <summary>
        /// The RenderLoop this VideoWriter is currently associated to.
        /// </summary>
        public RenderLoop AssociatedRenderLoop
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the current video size.
        /// </summary>
        public Size2 VideoSize { get; private set; }

        public Exception LastStartException { get; private set; }

        public Exception LastDrawException { get; private set; }

        public Exception LastFinishException { get; private set; }
    }
}