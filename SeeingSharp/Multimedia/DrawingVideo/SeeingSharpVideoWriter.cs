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
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using System;

namespace SeeingSharp.Multimedia.DrawingVideo
{
    /// <summary>
    /// A common base class for all video writers provided by the graphics engine.
    /// </summary>
    public abstract class SeeingSharpVideoWriter
    {
        // Runtime values
        private Size2 m_videoSize;
        private bool m_hasStarted;
        private bool m_hasFinished;
        private Exception m_startException;
        private Exception m_drawException;
        private Exception m_finishException;

        /// <summary>
        /// Occurs when recording was finished (by success or failure).
        /// </summary>
        public event EventHandler RecordingFinished;

        /// <summary>
        /// Initializes a new instance of the <see cref="SeeingSharpVideoWriter"/> class.
        /// </summary>
        /// <param name="targetFile">The target file to write to.</param>
        protected SeeingSharpVideoWriter(ResourceLink targetFile)
        {
            this.TargetFile = targetFile;
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
                if (!m_hasStarted) { throw new SeeingSharpGraphicsException($"{nameof(SeeingSharpVideoWriter)} is not started!"); }
                if (m_hasFinished) { throw new SeeingSharpGraphicsException($"{nameof(SeeingSharpVideoWriter)} has already finished before!"); }

                // Check for correct image size
                if (m_videoSize != uploadedTexture.PixelSize)
                {
                    throw new SeeingSharpGraphicsException("Size has changed during recording!");
                }

                this.DrawFrameInternal(device, uploadedTexture);
            }
            catch (Exception ex)
            {
                m_drawException = ex;
            }
        }

        /// <summary>
        /// Checks whether changes on the configuration of this object are valid currently.
        /// The method throws an exception, if not.
        /// </summary>
        protected void CheckWhetherChangesAreValid()
        {
            if (m_hasStarted || m_hasFinished) { throw new SeeingSharpGraphicsException("Unable to do changed when VideoWriter is running!"); }
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
        /// Starts to render the video.
        /// </summary>
        internal void StartRendering(Size2 videoSize)
        {
            videoSize.EnsureNotEmpty(nameof(videoSize));
            if (m_hasStarted) { throw new SeeingSharpGraphicsException($"{nameof(SeeingSharpVideoWriter)} has already started before!"); }
            if (m_hasFinished) { throw new SeeingSharpGraphicsException($"{nameof(SeeingSharpVideoWriter)} has already finished before!"); }

            m_videoSize = videoSize;

            // Reset exceptions
            m_drawException = null;
            m_startException = null;
            m_finishException = null;

            // Ensure that the target directory exists
            try
            {
                this.StartRenderingInternal(m_videoSize);
                m_hasStarted = true;
            }
            catch (Exception ex)
            {
                m_startException = ex;
                m_hasStarted = false;
            }
        }

        /// <summary>
        /// Finished the rendered video.
        /// </summary>
        internal void FinishRendering()
        {
            try
            {
                if (!m_hasStarted) { throw new SeeingSharpGraphicsException($"{nameof(SeeingSharpVideoWriter)} is not started!"); }
                if (m_hasFinished) { throw new SeeingSharpGraphicsException($"{nameof(SeeingSharpVideoWriter)} has already finished before!"); }

                this.FinishRenderingInternal();
            }
            catch (Exception ex)
            {
                m_finishException = ex;
            }
            finally
            {
                m_hasFinished = true;
                this.RecordingFinished.Raise(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the target file this VideoWriter is writing to.
        /// </summary>
        public ResourceLink TargetFile { get; }

        /// <summary>
        /// Has rendering started?
        /// </summary>
        public bool HasStarted => m_hasStarted;

        /// <summary>
        /// Has rendering finished?
        /// </summary>
        public bool HasFinished => m_hasFinished;

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
        public Size2 VideoSize => m_videoSize;

        public Exception LastStartException => m_startException;

        public Exception LastDrawException => m_drawException;

        public Exception LastFinishException => m_finishException;
    }
}
