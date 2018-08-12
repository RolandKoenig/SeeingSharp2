#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace SeeingSharp.Multimedia.Core
{
    public static partial class Drawing3DAnimationExtensions
    {
        /// <summary>
        /// Rotates the object by the given euler rotation vector.
        /// </summary>
        /// <typeparam name="TargetObject">The type of the arget object.</typeparam>
        /// <param name="sequenceBuilder">The sequence builder.</param>
        /// <param name="rotateVector">The rotate vector.</param>
        /// <param name="animationTime">The animation time.</param>
        public static IAnimationSequenceBuilder<TargetObject> RotateEulerAnglesBy<TargetObject>(this IAnimationSequenceBuilder<TargetObject> sequenceBuilder, Vector3 rotateVector, TimeSpan animationTime)
            where TargetObject : class, IAnimatableObjectEulerRotation
        {
            sequenceBuilder.Add(
                new RotateEulerAnglesAnimation(
                    sequenceBuilder.TargetObject, rotateVector, animationTime,
                    stateChangeMode: AnimationStateChangeMode.ChangeStateBy));
            return sequenceBuilder;
        }

        /// <summary>
        /// Rotates the object to the given euler rotation vector.
        /// </summary>
        /// <typeparam name="TargetObject">The type of the arget object.</typeparam>
        /// <param name="sequenceBuilder">The sequence builder.</param>
        /// <param name="rotateVector">The rotate vector.</param>
        /// <param name="animationTime">The animation time.</param>
        public static IAnimationSequenceBuilder<TargetObject> RotateEulerAnglesTo<TargetObject>(this IAnimationSequenceBuilder<TargetObject> sequenceBuilder, Vector3 rotateVector, TimeSpan animationTime)
            where TargetObject : class, IAnimatableObjectEulerRotation
        {
            sequenceBuilder.Add(
                new RotateEulerAnglesAnimation(
                    sequenceBuilder.TargetObject, rotateVector, animationTime,
                    stateChangeMode: AnimationStateChangeMode.ChangeStateTo));
            return sequenceBuilder;
        }

        /// <summary>
        /// Rotates the object by the given euler rotation vector.
        /// </summary>
        /// <typeparam name="HostObject">The type of the ost object.</typeparam>
        /// <typeparam name="TargetObject">The type of the arget object.</typeparam>
        /// <param name="sequenceBuilder">The sequence builder.</param>
        /// <param name="targetObject">The target object.</param>
        /// <param name="rotateVector">The rotate vector.</param>
        /// <param name="animationTime">The animation time.</param>
        public static IAnimationSequenceBuilder<HostObject> RotateEulerAnglesBy<HostObject, TargetObject>(this IAnimationSequenceBuilder<HostObject> sequenceBuilder, TargetObject targetObject, Vector3 rotateVector, TimeSpan animationTime)
            where HostObject : class
            where TargetObject : class, IAnimatableObjectEulerRotation
        {
            sequenceBuilder.Add(
                new RotateEulerAnglesAnimation(
                    targetObject, rotateVector, animationTime,
                    stateChangeMode: AnimationStateChangeMode.ChangeStateBy));
            return sequenceBuilder;
        }

        /// <summary>
        /// Rotates the object by the given euler rotation vector.
        /// </summary>
        /// <typeparam name="TargetObject">The type of the arget object.</typeparam>
        /// <param name="sequenceBuilder">The sequence builder.</param>
        /// <param name="targetYaw">The target value for the Yaw (Y) rotation component.</param>
        /// <param name="animationTime">The animation time.</param>
        public static IAnimationSequenceBuilder<TargetObject> RotateEulerAnglesYawBy<TargetObject>(this IAnimationSequenceBuilder<TargetObject> sequenceBuilder, float targetYaw, TimeSpan animationTime)
            where TargetObject : class, IAnimatableObjectEulerRotation
        {
            sequenceBuilder.Add(
                new RotateEulerAnglesAnimation(
                    sequenceBuilder.TargetObject, new Vector3(0f, targetYaw, 0f), animationTime,
                    calculationComponent: RotationCalculationComponent.Yaw,
                    stateChangeMode: AnimationStateChangeMode.ChangeStateBy));
            return sequenceBuilder;
        }

        /// <summary>
        /// Rotates the object by the given euler rotation vector.
        /// </summary>
        /// <typeparam name="HostObject">The type of the ost object.</typeparam>
        /// <typeparam name="TargetObject">The type of the arget object.</typeparam>
        /// <param name="sequenceBuilder">The sequence builder.</param>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetYaw">The target value for the Yaw (Y) rotation component.</param>
        /// <param name="animationTime">The animation time.</param>
        public static IAnimationSequenceBuilder<HostObject> RotateEulerAnglesYawBy<HostObject, TargetObject>(this IAnimationSequenceBuilder<HostObject> sequenceBuilder, TargetObject targetObject, float targetYaw, TimeSpan animationTime)
            where HostObject : class
            where TargetObject : class, IAnimatableObjectEulerRotation
        {
            sequenceBuilder.Add(
                new RotateEulerAnglesAnimation(
                    targetObject, new Vector3(0f, targetYaw, 0f), animationTime,
                    calculationComponent: RotationCalculationComponent.Yaw,
                    stateChangeMode: AnimationStateChangeMode.ChangeStateBy));
            return sequenceBuilder;
        }

        /// <summary>
        /// Rotates the object to the given quaternion.
        /// </summary>
        /// <typeparam name="TargetObject">The type of the arget object.</typeparam>
        /// <param name="sequenceBuilder">The sequence builder.</param>
        /// <param name="targetQuaternion">The target quaternion.</param>
        /// <param name="animationTime">The animation time.</param>
        /// <returns></returns>
        public static IAnimationSequenceBuilder<TargetObject> RotateQuaternionTo<TargetObject>(this IAnimationSequenceBuilder<TargetObject> sequenceBuilder, Quaternion targetQuaternion, TimeSpan animationTime)
            where TargetObject : class, IAnimatableObjectQuaternion
        {
            sequenceBuilder.Add(
                new RotateQuaternionToAnimation(sequenceBuilder.TargetObject, targetQuaternion, animationTime));
            return sequenceBuilder;
        }

        /// <summary>
        /// Rotates the object to the given quaternion.
        /// </summary>
        /// <typeparam name="HostObject">The type of the ost object.</typeparam>
        /// <typeparam name="TargetObject">The type of the arget object.</typeparam>
        /// <param name="sequenceBuilder">The sequence builder.</param>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetQuaternion">The target quaternion.</param>
        /// <param name="animationTime">The animation time.</param>
        public static IAnimationSequenceBuilder<HostObject> RotateQuaternionTo<HostObject, TargetObject>(this IAnimationSequenceBuilder<HostObject> sequenceBuilder, TargetObject targetObject, Quaternion targetQuaternion, TimeSpan animationTime)
            where HostObject : class
            where TargetObject : class, IAnimatableObjectQuaternion
        {
            sequenceBuilder.Add(
                new RotateQuaternionToAnimation(targetObject, targetQuaternion, animationTime));
            return sequenceBuilder;
        }
    }
}
