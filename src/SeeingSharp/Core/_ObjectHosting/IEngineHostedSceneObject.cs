using System.Numerics;

namespace SeeingSharp.Core
{
    public interface IEngineHostedSceneObject
    {
        /// <summary>
        /// Gets the current position of the object.
        /// </summary>
        Vector3 Position
        {
            get;
        }

        /// <summary>
        /// Gets the current rotation of the object.
        /// </summary>
        Vector3 Rotation
        {
            get;
        }

        /// <summary>
        /// Gets the current scaling of the object.
        /// </summary>
        Vector3 Scaling
        {
            get;
        }

        /// <summary>
        /// Gets the associated AnimationHandler object (if any).
        /// </summary>
        AnimationHandler AnimationHandler
        {
            get;
        }

        /// <summary>
        /// Gets the display color of the object.
        /// </summary>
        Color4 DisplayColor
        {
            get;
        }
    }
}
