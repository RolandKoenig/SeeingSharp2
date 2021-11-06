using System.Numerics;

namespace SeeingSharp.Core.Animations
{
    public interface IAnimatableObjectQuaternion
    {
        /// <summary>
        /// Gets or sets the quaternion used for object rotation calculation.
        /// </summary>
        Quaternion RotationQuaternion
        {
            get;
            set;
        }
    }
}
