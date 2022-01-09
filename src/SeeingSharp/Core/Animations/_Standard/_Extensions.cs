using System;
using System.Threading.Tasks;

namespace SeeingSharp.Core.Animations
{
    public static class Extensions
    {
        /// <summary>
        /// Waits some time before continuing with next animation sequence.
        /// </summary>
        /// <param name="builder">The AnimationSequenceBuilder object.</param>
        /// <param name="milliseconds">The total milliseconds to wait.</param>
        public static IAnimationSequenceBuilder<TObjectType> Delay<TObjectType>(this IAnimationSequenceBuilder<TObjectType> builder, int milliseconds)
            where TObjectType : class
        {
            builder.Add(new DelayAnimation(TimeSpan.FromMilliseconds(milliseconds)));
            return builder;
        }

        /// <summary>
        /// Waits some time before continuing with next animation sequence.
        /// </summary>
        /// <param name="builder">The AnimationSequenceBuilder object.</param>
        /// <param name="duration">Total duration to wait.</param>
        public static IAnimationSequenceBuilder<TObjectType> Delay<TObjectType>(this IAnimationSequenceBuilder<TObjectType> builder, TimeSpan duration)
            where TObjectType : class
        {
            builder.Add(new DelayAnimation(duration));
            return builder;
        }

        /// <summary>
        /// Waits until given task has finished executing.
        /// </summary>
        /// <param name="builder">The AnimationSequenceBuilder object.</param>
        /// <param name="blockingTask">The Task for which we have to wait.</param>
        public static IAnimationSequenceBuilder<TObjectType> WaitTaskFinished<TObjectType>(this IAnimationSequenceBuilder<TObjectType> builder, Task blockingTask)
            where TObjectType : class
        {
            builder.Add(new WaitTaskFinishedAnimation(blockingTask));
            return builder;
        }

        /// <summary>
        /// Waits until the given condition returns true.
        /// </summary>
        /// <param name="builder">The AnimationSequenceBuilder object.</param>
        /// <param name="checkFunction">Return true to continue with the animation.</param>
        public static IAnimationSequenceBuilder<TObjectType> WaitForCondition<TObjectType>(this IAnimationSequenceBuilder<TObjectType> builder, Func<bool> checkFunction)
            where TObjectType : class
        {
            builder.Add(new WaitForConditionPassedAnimation(checkFunction));
            return builder;
        }

        /// <summary>
        /// Waits until previous animation steps are finished.
        /// </summary>
        /// <param name="builder">The AnimationSequenceBuilder object.</param>
        public static IAnimationSequenceBuilder<TObjectType> WaitFinished<TObjectType>(this IAnimationSequenceBuilder<TObjectType> builder)
            where TObjectType : class
        {
            builder.Add(new WaitFinishedAnimation());
            return builder;
        }

        /// <summary>
        /// Wait until given time has passed.
        /// </summary>
        /// <typeparam name="TObjectType">The type of the object to be animated.</typeparam>
        /// <param name="builder">The AnimationSequenceBuilder object.</param>
        /// <param name="waittime">The total time to wait.</param>
        public static IAnimationSequenceBuilder<TObjectType> WaitUntilTimePassed<TObjectType>(this IAnimationSequenceBuilder<TObjectType> builder, TimeSpan waittime)
            where TObjectType : class
        {
            builder.Add(new WaitTimePassedAnimation(waittime));
            return builder;
        }

        /// <summary>
        /// Adds a lazy animation object.
        /// </summary>
        /// <param name="builder">The AnimationSequenceBuilder object.</param>
        /// <param name="animationCreator">A lambda that creates the animation.</param>
        public static IAnimationSequenceBuilder<TObjectType> Lazy<TObjectType>(this IAnimationSequenceBuilder<TObjectType> builder, Func<IAnimation> animationCreator)
            where TObjectType : class
        {
            builder.Add(new LazyAnimation(animationCreator));
            return builder;
        }

        /// <summary>
        /// Builds a lazy animation object using the given child sequence.
        /// </summary>
        /// <param name="builder">The AnimationSequenceBuilder object.</param>
        /// <param name="childSequenceBuilder">A SequenceBuilder building a child sequence.</param>
        public static IAnimationSequenceBuilder<TObjectType> Lazy<TObjectType>(this IAnimationSequenceBuilder<TObjectType> builder, Action<IAnimationSequenceBuilder<TObjectType>> childSequenceBuilder)
            where TObjectType : class
        {
            return builder.Lazy(() =>
            {
                var result = new AnimationHandler(builder.AnimationHandler.Owner);
                IAnimationSequenceBuilder<TObjectType> childBuilder = new AnimationSequenceBuilder<TObjectType>(result);
                childSequenceBuilder(childBuilder);

                if (childBuilder.ItemCount == 0 && !childBuilder.Applied)
                {
                    childBuilder.Apply();
                }

                if (!childBuilder.Applied)
                {
                    throw new InvalidOperationException("Child sequence was not correctly applied (call to Apply is missing!)");
                }

                return result;
            });
        }

        /// <summary>
        /// Calls the given action.
        /// </summary>
        /// <param name="builder">The AnimationSequenceBuilder object.</param>
        /// <param name="actionToCall">The action to call on this step of the animation.</param>
        public static IAnimationSequenceBuilder<TObjectType> CallAction<TObjectType>(this IAnimationSequenceBuilder<TObjectType> builder, Action actionToCall)
            where TObjectType : class
        {
            if (actionToCall == null) { throw new ArgumentNullException("actionToCall"); }

            builder.Add(new CallActionAnimation(actionToCall));
            return builder;
        }

        /// <summary>
        /// Calls the given action.
        /// </summary>
        /// <param name="builder">The AnimationSequenceBuilder object.</param>
        /// <param name="actionToCall">The action to call on this step of the animation.</param>
        /// <param name="cancelAction">The action to be called when this animation would be canceled.</param>
        public static IAnimationSequenceBuilder<TObjectType> CallAction<TObjectType>(this IAnimationSequenceBuilder<TObjectType> builder, Action? actionToCall, Action? cancelAction)
            where TObjectType : class
        {
            if (actionToCall == null) { throw new ArgumentNullException("actionToCall"); }

            builder.Add(new CallActionAnimation(actionToCall, cancelAction));
            return builder;
        }

        /// <summary>
        /// Increases a float value by a given total increase value over the given duration.
        /// </summary>
        /// <param name="builder">The AnimationSequenceBuilder object.</param>
        /// <param name="valueGetter">The value getter.</param>
        /// <param name="valueSetter">The value setter.</param>
        /// <param name="totalIncrease">The value to increase in total.</param>
        /// <param name="duration">Total duration to wait.</param>
        public static IAnimationSequenceBuilder<TObjectType> ChangeFloatBy<TObjectType>(this IAnimationSequenceBuilder<TObjectType> builder, Func<float> valueGetter, Action<float> valueSetter, float totalIncrease, TimeSpan duration)
            where TObjectType : class
        {
            builder.Add(new ChangeFloatByAnimation(builder.TargetObject, valueGetter, valueSetter, totalIncrease, duration));
            return builder;
        }

        /// <summary>
        /// Increases a int value by a given total increase value over the given duration.
        /// </summary>
        /// <param name="builder">The AnimationSequenceBuilder object.</param>
        /// <param name="valueGetter">The value getter.</param>
        /// <param name="valueSetter">The value setter.</param>
        /// <param name="totalIncrease">The value to increase in total.</param>
        /// <param name="duration">Total duration to wait.</param>
        public static IAnimationSequenceBuilder<TObjectType> ChangeIntBy<TObjectType>(this IAnimationSequenceBuilder<TObjectType> builder, Func<int> valueGetter, Action<int> valueSetter, int totalIncrease, TimeSpan duration)
            where TObjectType : class
        {
            builder.Add(new ChangeIntByAnimation(builder.TargetObject, valueGetter, valueSetter, totalIncrease, duration));
            return builder;
        }
    }
}