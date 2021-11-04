using System;
using System.Numerics;

namespace SeeingSharp.Core
{
    public class RotateQuaternionToAnimation : AnimationBase
    {
        // Parameters
        private IAnimatableObjectQuaternion _targetObject;
        private Quaternion _startQuaternion;
        private Quaternion _targetQuaternion;

        /// <summary>
        /// Initializes a new instance of the <see cref="RotateQuaternionToAnimation" /> class.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetQuaternion">The target quaternion.</param>
        /// <param name="timeSpan">The time span.</param>
        public RotateQuaternionToAnimation(IAnimatableObjectQuaternion targetObject, Quaternion targetQuaternion, TimeSpan timeSpan)
            : base(targetObject, AnimationType.FixedTime, timeSpan)
        {
            _targetObject = targetObject;
            _targetQuaternion = targetQuaternion;
        }

        /// <summary>
        /// Called when animation starts.
        /// </summary>
        protected override void OnStartAnimation()
        {
            _startQuaternion = _targetObject.RotationQuaternion;
        }

        /// <summary>
        /// Called each time the CurrentTime value gets updated.
        /// </summary>
        /// <param name="updateState"></param>
        /// <param name="animationState"></param>
        protected override void OnCurrentTimeUpdated(IAnimationUpdateState updateState, AnimationState animationState)
        {
            //how does Slerp work: --> http://en.wikipedia.org/wiki/Slerp
            var changeFactor = this.CurrentTime.Ticks / (float)this.FixedTime.Ticks;

            var slerpQuaternion = Quaternion.Slerp(_startQuaternion, _targetQuaternion, changeFactor);
            _targetObject.RotationQuaternion = slerpQuaternion;
        }

        /// <summary>
        /// Called when the FixedTime animation has finished.
        /// (Sets final state to the target object and clears all runtime values).
        /// </summary>
        protected override void OnFixedTimeAnimationFinished()
        {
            _targetObject.RotationQuaternion = _targetQuaternion;
        }
    }
}