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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SeeingSharp.Multimedia.Core
{
    internal class AnimationSequenceBuilder<TargetType> : IAnimationSequenceBuilder<TargetType>
        where TargetType : class
    {
        private AnimationHandler m_ownerAnimationHandler;
        private TargetType m_animatedObject;
        private List<IAnimation> m_sequenceList;
        private bool m_applied;

        /// <summary>
        /// Initializes a new instance of the AnimationSequenceBuilder class.
        /// </summary>
        /// <param name="owner">The owner object.</param>
        /// <exception cref="System.ArgumentException">Unable to cast target object of this AnimationSequenceBuilder to the generic type parameter!</exception>
        internal AnimationSequenceBuilder(AnimationHandler owner)
            : this()
        {
            m_ownerAnimationHandler = owner;
            m_animatedObject = owner.Owner as TargetType;
        }

        /// <summary>
        /// Initializes a new instance of the AnimationSequenceBuilder class.
        /// </summary>
        /// <param name="owner">The owner object.</param>
        /// <param name="animatedObject">The object which gets animated.</param>
        /// <exception cref="System.ArgumentException">Unable to cast target object of this AnimationSequenceBuilder to the generic type parameter!</exception>
        internal AnimationSequenceBuilder(AnimationHandler owner, TargetType animatedObject)
            : this()
        {
            m_ownerAnimationHandler = owner;
            m_animatedObject = animatedObject;
        }


        /// <summary>
        /// Initializes a new instance of the AnimationSequenceBuilder class.
        /// </summary>
        private AnimationSequenceBuilder()
        {
            m_sequenceList = new List<IAnimation>();
        }

        /// <summary>
        /// Adds an AnimationSequence to the builder.
        /// </summary>
        public IAnimationSequenceBuilder<TargetType> Add(IAnimation animationSequence)
        {
            if (m_applied) { throw new SeeingSharpGraphicsException("Unable to add a new AnimationSequence to a finished AnimationSequenceBuilder!"); }
            m_sequenceList.Add(animationSequence);

            return this;
        }

        /// <summary>
        /// Finishes the AnimationSequence and adds it to the AninationHandler it was created with.
        /// </summary>
        /// <param name="actionToCall">The action to be called after animation has finished.</param>
        /// <param name="cancelAction">The action to be called when the animation gets canceled.</param>
        /// <param name="ignorePause">Should this animation ignore pause stateß</param>
        public void Apply(Action actionToCall = null, Action cancelAction = null, bool? ignorePause = null)
        {
            if (m_ownerAnimationHandler == null) { throw new SeeingSharpGraphicsException("Unable to finish AnimationSequenceBuilder: No default AnimationHandler found!"); }

            // Append 'CallAction' on demand
            if ((actionToCall != null) || (cancelAction != null))
            {
                this.WaitFinished()
                    .CallAction(actionToCall, cancelAction);
            }

            // Change the 'Ignore pause state'
            if(ignorePause != null)
            {
                foreach(IAnimation actAnimation in m_sequenceList)
                {
                    actAnimation.IgnorePauseState = ignorePause.Value;
                }
            }

            m_ownerAnimationHandler.BeginAnimation(m_sequenceList);
            m_applied = true;
        }

        /// <summary>
        /// Finishes the AnimationSequence and adds it to the AninationHandler it was created with.
        /// </summary>
        /// <param name="actionToCall">The action to be called after animation has finished.</param>
        /// <param name="cancelAction">The action to be called when the animation gets canceled.</param>
        /// <param name="ignorePause">Should this animation ignore pause stateß</param>
        public void ApplyAsSecondary(Action actionToCall, Action cancelAction, bool? ignorePause = null)
        {
            if (m_ownerAnimationHandler == null) { throw new SeeingSharpGraphicsException("Unable to finish AnimationSequenceBuilder: No default AnimationHandler found!"); }

            // Append 'CallAction' on demand
            if ((actionToCall != null) || (cancelAction != null))
            {
                this.WaitFinished()
                    .CallAction(actionToCall, cancelAction);
            }

            // Change the 'Ignore pause state'
            if (ignorePause != null)
            {
                foreach (IAnimation actAnimation in m_sequenceList)
                {
                    actAnimation.IgnorePauseState = ignorePause.Value;
                }
            }

            m_ownerAnimationHandler.BeginSecondaryAnimation(m_sequenceList);
            m_applied = true;
        }

        /// <summary>
        /// Finishes the AnimationSequence and adds it to the AninationHandler it was created with.
        /// The caller can await the finish of this animation using the returned task object.
        /// </summary>
        public Task ApplyAsync()
        {
            TaskCompletionSource<bool> taskComplSource = new TaskCompletionSource<bool>();

            this.Apply(
                () => taskComplSource.TrySetResult(true),
                () => taskComplSource.TrySetCanceled());

            return taskComplSource.Task;
        }

        /// <summary>
        /// Finishes the AnimationSequence and adds it to the AninationHandler it was created with.
        /// The caller can await the finish of this animation using the returned task object.
        /// </summary>
        public Task ApplyAsSecondaryAsync()
        {
            TaskCompletionSource<bool> taskComplSource = new TaskCompletionSource<bool>();

            this.ApplyAsSecondary(
                () => taskComplSource.TrySetResult(true),
                () => taskComplSource.TrySetCanceled());

            return taskComplSource.Task;
        }

        /// <summary>
        /// Finishes the animation and starts from beginning.
        /// </summary>
        /// <param name="ignorePause">Should this animation ignore pause stateß</param>
        public void ApplyAndRewind(bool? ignorePause = null)
        {
            if (m_ownerAnimationHandler == null) { throw new SeeingSharpGraphicsException("Unable to finish AnimationSequenceBuilder: No default AnimationHandler found!"); }

            // Define rewind action
            //  a bit complicated because there a problems with the finished flag
            Action rewindAction = null;
            rewindAction = () =>
            {
                List<IAnimation> newAnimationList = new List<IAnimation>(m_sequenceList.Count);
                foreach (var actAnimationStep in m_sequenceList)
                {
                    actAnimationStep.Reset();
                    newAnimationList.Add(actAnimationStep);
                }
                newAnimationList[newAnimationList.Count - 1] = new CallActionAnimation(rewindAction);
                m_ownerAnimationHandler.BeginAnimation(newAnimationList);
            };

            // Apend rewind action to the sequence
            this.WaitFinished()
                .CallAction(rewindAction);

            // Change the 'Ignore pause state'
            if (ignorePause != null)
            {
                foreach (IAnimation actAnimation in m_sequenceList)
                {
                    actAnimation.IgnorePauseState = ignorePause.Value;
                }
            }

            // Start the animation
            m_ownerAnimationHandler.BeginAnimation(m_sequenceList);
            m_applied = true;
        }

        /// <summary>
        /// Finishes the animation and starts from beginning.
        /// </summary>
        public void ApplyAsSecondaryAndRewind()
        {
            if (m_ownerAnimationHandler == null) { throw new SeeingSharpGraphicsException("Unable to finish AnimationSequenceBuilder: No default AnimationHandler found!"); }

            // Define rewind action
            //  a bit complicated because there a problems with the finished flag
            Action rewindAction = null;
            rewindAction = () =>
            {
                List<IAnimation> newAnimationList = new List<IAnimation>(m_sequenceList.Count);
                foreach (var actAnimationStep in m_sequenceList)
                {
                    actAnimationStep.Reset();
                    newAnimationList.Add(actAnimationStep);
                }
                newAnimationList[newAnimationList.Count - 1] = new CallActionAnimation(rewindAction);
                m_ownerAnimationHandler.BeginSecondaryAnimation(newAnimationList);
            };

            // Apend rewind action to the sequence
            this.WaitFinished()
                .CallAction(rewindAction);

            // Start the animation
            m_ownerAnimationHandler.BeginSecondaryAnimation(m_sequenceList);
            m_applied = true;
        }

        /// <summary>
        /// Gets the corresponding animation handler.
        /// </summary>
        public AnimationHandler AnimationHandler
        {
            get { return m_ownerAnimationHandler; }
        }

        /// <summary>
        /// Gets the target object of this animation
        /// </summary>
        public TargetType TargetObject
        {
            get { return m_animatedObject; }
        }

        /// <summary>
        /// Gets the item count.
        /// </summary>
        public int ItemCount
        {
            get { return m_sequenceList.Count; }
        }

        /// <summary>
        /// Was apply called already?
        /// </summary>
        public bool Applied
        {
            get { return m_applied; }
        }
    }
}