/*
    SeeingSharp and all applications distributed together with it. 
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
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.DrawingVideo
{
    /// <summary>
    /// A common base class for all video writers provided by the graphics engine.
    /// </summary>
    public abstract class SeeingSharpVideoWriter
    {
        // Runtime values
        private Size2 _videoSize;
        private bool _hasStarted;
        private bool _hasFinished;
        private Exception _startException;
        private Exception _drawException;
        private Exception _finishException;

        /// <summary>
        /// Gets the target file this VideoWriter is writing to.
        /// </summary>
        public ResourceLink TargetFile { get; }

        /// <summary>
        /// Has rendering started?
        /// </summary>
        public bool HasStarted => _hasStarted;

        /// <summary>
        /// Has rendering finished?
        /// </summary>
        public bool HasFinished => _hasFinished;

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
        public Size2 VideoSize => _videoSize;

        public Exception LastStartException => _startException;

        public Exception LastDrawException => _drawException;

        public Exception LastFinishException => _finishException;

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
        public void DrawFrame(EngineDevice device, MemoryMappedTexture<int> uploadedTexture)
        {
            try
            {
                device.EnsureNotNull(nameof(device));
                uploadedTexture.EnsureNotNull(nameof(uploadedTexture));
                if (!_hasStarted) { throw new SeeingSharpGraphicsException($"{nameof(SeeingSharpVideoWriter)} is not started!"); }
                if (_hasFinished) { throw new SeeingSharpGraphicsException($"{nameof(SeeingSharpVideoWriter)} has already finished before!"); }

                // Check for correct image size
                if (_videoSize != uploadedTexture.PixelSize)
                {
                    throw new SeeingSharpGraphicsException("Size has changed during recording!");
                }

                this.DrawFrameInternal(device, uploadedTexture);
            }
            catch (Exception ex)
            {
                _drawException = ex;
            }
        }

        /// <summary>
        /// Checks whether changes on the configuration of this object are valid currently.
        /// The method throws an exception, if not.
        /// </summary>
        protected void CheckWhetherChangesAreValid()
        {
            if (_hasStarted || _hasFinished) { throw new SeeingSharpGraphicsException("Unable to do changed when VideoWriter is running!"); }
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
        protected abstract void DrawFrameInternal(EngineDevice device, MemoryMappedTexture<int> uploadedTexture);

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
            if (_hasStarted) { throw new SeeingSharpGraphicsException($"{nameof(SeeingSharpVideoWriter)} has already started before!"); }
            if (_hasFinished) { throw new SeeingSharpGraphicsException($"{nameof(SeeingSharpVideoWriter)} has already finished before!"); }

            _videoSize = videoSize;

            // Reset exceptions
            _drawException = null;
            _startException = null;
            _finishException = null;

            // Ensure that the target directory exists
            try
            {
                this.StartRenderingInternal(_videoSize);
                _hasStarted = true;
            }
            catch (Exception ex)
            {
                _startException = ex;
                _hasStarted = false;
            }
        }

        /// <summary>
        /// Finished the rendered video.
        /// </summary>
        internal void FinishRendering()
        {
            try
            {
                if (!_hasStarted) { throw new SeeingSharpGraphicsException($"{nameof(SeeingSharpVideoWriter)} is not started!"); }
                if (_hasFinished) { throw new SeeingSharpGraphicsException($"{nameof(SeeingSharpVideoWriter)} has already finished before!"); }

                this.FinishRenderingInternal();
            }
            catch (Exception ex)
            {
                _finishException = ex;
            }
            finally
            {
                _hasFinished = true;
                this.RecordingFinished.Raise(this, EventArgs.Empty);
            }
        }
    }
}
