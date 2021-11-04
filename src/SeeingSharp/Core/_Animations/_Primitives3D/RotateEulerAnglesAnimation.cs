using System;
using System.Numerics;

namespace SeeingSharp.Core
{
    public class RotateEulerAnglesAnimation : AnimationBase
    {
        // Parameters
        private IAnimatableObjectEulerRotation _targetObject;
        private RotationCalculationComponent _calculationComponents;
        private AnimationStateChangeMode _stateChangeMode;
        private Vector3 _paramRotation;
        private TimeSpan _duration;

        // Runtime values
        private Vector3 _startRotation;
        private Vector3 _targetRotation;
        private Vector3 _changeRotation;

        /// <summary>
        /// Rotates the object to the target rotation vector.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetVector">The target rotation vector.</param>
        /// <param name="duration">Total duration of the animation.</param>
        /// <param name="calculationComponent">The components which are to be modified.</param>
        /// <param name="stateChangeMode">The state-change mode (to or by).</param>
        public RotateEulerAnglesAnimation(
            IAnimatableObjectEulerRotation targetObject, Vector3 targetVector, TimeSpan duration,
            RotationCalculationComponent calculationComponent = RotationCalculationComponent.All,
            AnimationStateChangeMode stateChangeMode = AnimationStateChangeMode.ChangeStateTo)
            : base(targetObject, AnimationType.FixedTime, duration)
        {
            _targetObject = targetObject;
            _paramRotation = targetVector;
            _duration = duration;
            _calculationComponents = calculationComponent;
            _stateChangeMode = stateChangeMode;
        }

        /// <summary>
        /// Called when animation starts.
        /// </summary>
        protected override void OnStartAnimation()
        {
            // Prepare this animation
            _startRotation = _targetObject.RotationEuler;
            switch (_stateChangeMode)
            {
                case AnimationStateChangeMode.ChangeStateTo:
                    _changeRotation = _paramRotation - _startRotation;
                    _targetRotation = _paramRotation;
                    break;

                case AnimationStateChangeMode.ChangeStateBy:
                    _changeRotation = _paramRotation;
                    _targetRotation = _startRotation + _changeRotation;
                    break;

                default:
                    throw new SeeingSharpGraphicsException("Unknown AnimationStateChangeMode in RotateEulerAnglesAnimation: " + _stateChangeMode + "!");
            }


            // Some optimization logic to take the shortest way
            //  => e. g. object rotation 45° instead of 315°
            if (_changeRotation.X > EngineMath.RAD_180DEG) { _changeRotation.X = -(_changeRotation.X - EngineMath.RAD_180DEG); }
            if (_changeRotation.Y > EngineMath.RAD_180DEG) { _changeRotation.Y = -(_changeRotation.Y - EngineMath.RAD_180DEG); }
            if (_changeRotation.Z > EngineMath.RAD_180DEG) { _changeRotation.Z = -(_changeRotation.Z - EngineMath.RAD_180DEG); }
            if (_changeRotation.X < -EngineMath.RAD_180DEG) { _changeRotation.X = -(_changeRotation.X + EngineMath.RAD_180DEG); }
            if (_changeRotation.Y < -EngineMath.RAD_180DEG) { _changeRotation.Y = -(_changeRotation.Y + EngineMath.RAD_180DEG); }
            if (_changeRotation.Z < -EngineMath.RAD_180DEG) { _changeRotation.Z = -(_changeRotation.Z + EngineMath.RAD_180DEG); }

            // Set components to zero which should not not be changed using this animation
            if (!_calculationComponents.HasFlag(RotationCalculationComponent.Pitch))
            {
                _changeRotation.X = 0f;
            }
            if (!_calculationComponents.HasFlag(RotationCalculationComponent.Yaw))
            {
                _changeRotation.Y = 0f;
            }
            if (!_calculationComponents.HasFlag(RotationCalculationComponent.Roll))
            {
                _changeRotation.Z = 0f;
            }
        }

        /// <summary>
        /// Called each time the CurrentTime value gets updated.
        /// </summary>
        protected override void OnCurrentTimeUpdated(IAnimationUpdateState updateState, AnimationState animationState)
        {
            var percentagePassed = this.CurrentTime.Ticks / (float)_duration.Ticks;
            _targetObject.RotationEuler = _startRotation + _changeRotation * percentagePassed;
        }

        /// <summary>
        /// Called when the FixedTime animation has finished.
        /// </summary>
        protected override void OnFixedTimeAnimationFinished()
        {
            _targetObject.RotationEuler = _targetRotation;
            _startRotation = Vector3.Zero;
            _changeRotation = Vector3.Zero;
            _targetRotation = Vector3.Zero;
        }
    }
}