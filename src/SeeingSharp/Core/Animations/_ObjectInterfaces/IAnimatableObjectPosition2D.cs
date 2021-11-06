using System.Numerics;

namespace SeeingSharp.Core.Animations
{
    public interface IAnimatableObjectPosition2D
    {
        /// <summary>
        /// Gets or sets the position of the object.
        /// </summary>
        Vector2 Position
        {
            get;
            set;
        }
    }
}
