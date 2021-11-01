using System.Numerics;

namespace SeeingSharp.Multimedia.Core
{
    public interface IAnimatableObjectEulerRotation
    {
        /// <summary>
        /// Gets the or sets the euler rotation vector.
        /// </summary>
        Vector3 RotationEuler
        {
            get;
            set;
        }
    }
}
