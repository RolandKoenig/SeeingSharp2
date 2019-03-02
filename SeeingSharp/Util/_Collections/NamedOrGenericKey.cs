#region License information
/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
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
namespace SeeingSharp.Util
{
    #region using

    using System;

    #endregion

    public struct NamedOrGenericKey : IEquatable<NamedOrGenericKey>, IComparable<NamedOrGenericKey>
    {
        public static readonly NamedOrGenericKey Empty = new NamedOrGenericKey();

        private string m_hint;
        private int m_hashCode;

        /// <summary>
        /// Private constructor - Just to be used for generated keys.
        /// </summary>
        /// <param name="keyValue">The key value.</param>
        internal NamedOrGenericKey(long keyValue)
        {
            if (keyValue == 0) { throw new ArgumentException("Key value can not be 0!"); }

            GenericKey = keyValue;
            NameKey = null;
            m_hashCode = keyValue.GetHashCode();
            m_hint = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedOrGenericKey" /> struct.
        /// </summary>
        /// <param name="nameKey">A key in form of a name.</param>
        public NamedOrGenericKey(string nameKey)
        {
            if (string.IsNullOrEmpty(nameKey)) { throw new ArgumentException("Path key can not be null!"); }

            GenericKey = 0;
            NameKey = nameKey;
            m_hashCode = nameKey.GetHashCode();
            m_hint = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedOrGenericKey" /> struct.
        /// </summary>
        /// <param name="targetType">The type which to bind to the key (FullTypeName is used).</param>
        public NamedOrGenericKey(Type targetType)
            : this(targetType.FullName)
        {
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Description;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(NamedOrGenericKey other)
        {
            if (NameKey != null)
            {
                if (other.NameKey == null) { return false; }
                return NameKey == other.NameKey;
            }
            else
            {
                return GenericKey == other.GenericKey;
            }
        }

        /// <summary>
        /// Compares this structure with another one of the same type.
        /// </summary>
        /// <param name="other"></param>
        public int CompareTo(NamedOrGenericKey other)
        {
            int result = m_hashCode.CompareTo(other.m_hashCode);
            if(result == 0)
            {
                if (NameKey != null)
                {
                    if (other.NameKey == null) { result = -1; }
                    else { result = NameKey.CompareTo(other.NameKey); }
                }
                else
                {
                    if (other.NameKey != null) { result = 1; }
                    else { result = GenericKey.CompareTo(other.GenericKey); }
                }
            }
            return result;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is NamedOrGenericKey)
            {
                var other = (NamedOrGenericKey)obj;
                return this.Equals(other);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return m_hashCode;
        }

        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator ==(NamedOrGenericKey left, NamedOrGenericKey right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator !=(NamedOrGenericKey left, NamedOrGenericKey right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Is this key empty?
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return (GenericKey == 0) && (NameKey == null);
            }
        }

        /// <summary>
        /// Gets or sets a hint for this resource key (a custom description which helps identifying what is behind this key).
        /// </summary>
        public string Hint
        {
            get
            {
                var result = m_hint;

                if (string.IsNullOrEmpty(result))
                {
                    result = NameKey;
                }

                if (string.IsNullOrEmpty(result))
                {
                    result = string.Empty;
                }

                return result;
            }
            set
            {
                m_hint = value;
            }
        }

        /// <summary>
        /// Gets a short description of this object.
        /// </summary>
        public string Description
        {
            get
            {
                if (NameKey != null) { return "Name: " + NameKey; }
                else { return "Generic ID: " + GenericKey; }
            }
        }

        /// <summary>
        /// Gets the named key.
        /// </summary>
        public string NameKey { get; }

        /// <summary>
        /// Gets the generic key.
        /// </summary>
        public long GenericKey { get; }
    }
}