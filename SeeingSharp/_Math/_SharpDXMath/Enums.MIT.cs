using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace SeeingSharp
{
    /*
     * The enumerations defined in this file are in alphabetical order. When
     * adding new enumerations or renaming existing ones, please make sure
     * the ordering is maintained.
    */

    /// <summary>
    /// Describes the type of angle.
    /// </summary>
    public enum AngleType
    {
        /// <summary>
        /// Specifies an angle measurement in revolutions.
        /// </summary>
        Revolution,

        /// <summary>
        /// Specifies an angle measurement in degrees.
        /// </summary>
        Degree,

        /// <summary>
        /// Specifies an angle measurement in radians.
        /// </summary>
        Radian,

        /// <summary>
        /// Specifies an angle measurement in gradians.
        /// </summary>
        Gradian
    }

	/// <summary>
	/// Describes how one bounding volume contains another.
	/// </summary>
	public enum ContainmentType
	{
		/// <summary>
		/// The two bounding volumes don't intersect at all.
		/// </summary>
		Disjoint,

		/// <summary>
		/// One bounding volume completely contains another.
		/// </summary>
		Contains,

		/// <summary>
		/// The two bounding volumes overlap.
		/// </summary>
		Intersects
	};
	
	/// <summary>
	/// Describes the result of an intersection with a plane in three dimensions.
	/// </summary>
	public enum PlaneIntersectionType
	{
		/// <summary>
		/// The object is behind the plane.
		/// </summary>
		Back,

		/// <summary>
		/// The object is in front of the plane.
		/// </summary>
		Front,

		/// <summary>
		/// The object is intersecting the plane.
		/// </summary>
		Intersecting
	};
}
