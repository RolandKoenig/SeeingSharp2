﻿using System.Numerics;

namespace SeeingSharp.Multimedia.Core
{
    public interface IAnimatableObjectPosition
    {
        /// <summary>
        /// Gets or sets the position of the object.
        /// </summary>
        Vector3 Position
        {
            get;
            set;
        }
    }
}