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
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Core;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class CameraStraightMoveAnimation : AnimationBase
    {
        // Configuration
        private Camera3DBase m_camera;
        private PerspectiveCamera3D m_cameraPerspective;
        private OrthographicCamera3D m_cameraOrthographic;
        private Camera3DViewPoint m_viewPointSource;
        private Camera3DViewPoint m_viewPointTarget;

        /// <summary>
        /// Initializes a new instance of the <see cref="CameraStraightMoveAnimation"/> class.
        /// </summary>
        /// <param name="targetCamera">The camera to be animated.</param>
        /// <param name="targetViewPoint">The target view point.</param>
        /// <param name="animationTime">The animation time.</param>
        public CameraStraightMoveAnimation(Camera3DBase targetCamera, Camera3DViewPoint targetViewPoint, TimeSpan animationTime)
            : base(targetCamera, AnimationType.FixedTime, animationTime)
        {
            targetCamera.EnsureNotNull(nameof(targetCamera));
            targetViewPoint.EnsureNotNull(nameof(targetViewPoint));

            m_camera = targetCamera;
            m_cameraPerspective = m_camera as PerspectiveCamera3D;
            m_cameraOrthographic = m_camera as OrthographicCamera3D;

            m_viewPointSource = m_camera.GetViewPoint();
            m_viewPointTarget = targetViewPoint;
        }

        /// <summary>
        /// Called each time the CurrentTime value gets updated.
        /// </summary>
        /// <param name="updateState">The current state of update processing.</param>
        /// <param name="animationState">The current state of the animation.</param>
        protected override void OnCurrentTimeUpdated(IAnimationUpdateState updateState, AnimationState animationState)
        {
            base.OnCurrentTimeUpdated(updateState, animationState);

            // Calculate factor by what to transform all coordinates
            var maxMilliseconds = FixedTime.TotalMilliseconds;
            var currentMillis = CurrentTime.TotalMilliseconds;
            var actFrameFactor = (float)(currentMillis / maxMilliseconds);

            // Transform position and rotation
            var moveVector = m_viewPointTarget.Position - m_viewPointSource.Position;
            var rotationVector = m_viewPointTarget.Rotation - m_viewPointSource.Rotation;
            m_camera.Position = m_viewPointSource.Position + moveVector * actFrameFactor;
            m_camera.TargetRotation = m_viewPointSource.Rotation + rotationVector * actFrameFactor;

            // Special handling for orthographics cameras
            if (m_cameraOrthographic != null)
            {
                var zoomValue = m_viewPointTarget.OrthographicZoomFactor - m_viewPointSource.OrthographicZoomFactor;
                m_cameraOrthographic.ZoomFactor = m_viewPointSource.OrthographicZoomFactor + zoomValue * actFrameFactor;
            }
        }

        /// <summary>
        /// Called when the FixedTime animation has finished.
        /// (Sets final state to the target object and clears all runtime values).
        /// </summary>
        protected override void OnFixedTimeAnimationFinished()
        {
            base.OnFixedTimeAnimationFinished();

            m_camera.ApplyViewPoint(m_viewPointTarget);
        }
    }
}
