namespace SeeingSharp.Multimedia.Core
{
    /// <summary>
    /// A base interface for each objects which is able to manage animations.
    /// </summary>
    public interface IAnimatableObject
    {
        /// <summary>
        /// Gets the animation handler of this object.
        /// </summary>
        AnimationHandler AnimationHandler
        {
            get;
        }
    }
}
