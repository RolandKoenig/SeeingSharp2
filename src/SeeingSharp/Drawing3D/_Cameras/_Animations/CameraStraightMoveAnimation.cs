using System;
using SeeingSharp.Checking;
using SeeingSharp.Core;
using SeeingSharp.Core.Animations;

namespace SeeingSharp.Drawing3D
{
    public class CameraStraightMoveAnimation : AnimationBase
    {
        // Configuration
        private Camera3DBase _camera;
        private OrthographicCamera3D _cameraOrthographic;
        private Camera3DViewPoint _viewPointSource;
        private Camera3DViewPoint _viewPointTarget;

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

            _camera = targetCamera;
            _cameraOrthographic = _camera as OrthographicCamera3D;

            _viewPointSource = _camera.GetViewPoint();
            _viewPointTarget = targetViewPoint;
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
            var maxMilliseconds = this.FixedTime.TotalMilliseconds;
            var currentMillis = this.CurrentTime.TotalMilliseconds;
            var actFrameFactor = (float)(currentMillis / maxMilliseconds);

            // Transform position and rotation
            var moveVector = _viewPointTarget.Position - _viewPointSource.Position;
            var rotationVector = _viewPointTarget.Rotation - _viewPointSource.Rotation;
            _camera.Position = _viewPointSource.Position + moveVector * actFrameFactor;
            _camera.TargetRotation = _viewPointSource.Rotation + rotationVector * actFrameFactor;

            // Special handling for orthographic cameras
            if (_cameraOrthographic != null)
            {
                var zoomValue = _viewPointTarget.OrthographicZoomFactor - _viewPointSource.OrthographicZoomFactor;
                _cameraOrthographic.ZoomFactor = _viewPointSource.OrthographicZoomFactor + zoomValue * actFrameFactor;
            }
        }

        /// <summary>
        /// Called when the FixedTime animation has finished.
        /// (Sets final state to the target object and clears all runtime values).
        /// </summary>
        protected override void OnFixedTimeAnimationFinished()
        {
            base.OnFixedTimeAnimationFinished();

            _camera.ApplyViewPoint(_viewPointTarget);
        }
    }
}
