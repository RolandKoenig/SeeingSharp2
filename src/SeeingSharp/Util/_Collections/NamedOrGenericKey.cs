﻿using System;

namespace SeeingSharp.Util
{
    public struct NamedOrGenericKey : IEquatable<NamedOrGenericKey>, IComparable<NamedOrGenericKey>
    {
        public static readonly NamedOrGenericKey Empty = new();

        private string? _hint;
        private readonly int _hashCode;

        /// <summary>
        /// Gets the named key.
        /// </summary>
        public string? NameKey { get; }

        /// <summary>
        /// Gets the generic key.
        /// </summary>
        public long GenericKey { get; }

        /// <summary>
        /// Private constructor - Just to be used for generated keys.
        /// </summary>
        /// <param name="keyValue">The key value.</param>
        internal NamedOrGenericKey(long keyValue)
        {
            if (keyValue == 0) { throw new ArgumentException("Key value can not be 0!"); }

            this.GenericKey = keyValue;
            this.NameKey = null;
            _hashCode = keyValue.GetHashCode();
            _hint = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedOrGenericKey" /> struct.
        /// </summary>
        /// <param name="nameKey">A key in form of a name.</param>
        public NamedOrGenericKey(string nameKey)
        {
            if (string.IsNullOrEmpty(nameKey)) { throw new ArgumentException("Path key can not be null!"); }

            this.GenericKey = 0;
            this.NameKey = nameKey;
            _hashCode = nameKey.GetHashCode();
            _hint = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedOrGenericKey" /> struct.
        /// </summary>
        /// <param name="targetType">The type which to bind to the key (FullTypeName is used).</param>
        public NamedOrGenericKey(Type targetType)
            : this(targetType.FullName ?? string.Empty)
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
            if (this.NameKey != null)
            {
                if (other.NameKey == null) { return false; }
                return this.NameKey == other.NameKey;
            }
            return this.GenericKey == other.GenericKey;
        }

        /// <summary>
        /// Compares this structure with another one of the same type.
        /// </summary>
        /// <param name="other"></param>
        public int CompareTo(NamedOrGenericKey other)
        {
            var result = _hashCode.CompareTo(other._hashCode);
            if (result == 0)
            {
                if (this.NameKey != null)
                {
                    if (other.NameKey == null) { result = -1; }
                    else { result = string.Compare(this.NameKey, other.NameKey, StringComparison.Ordinal); }
                }
                else
                {
                    if (other.NameKey != null) { result = 1; }
                    else { result = this.GenericKey.CompareTo(other.GenericKey); }
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
        public override bool Equals(object? obj)
        {
            if (obj is NamedOrGenericKey other)
            {
                return this.Equals(other);
            }
            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return _hashCode;
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
        public bool IsEmpty => this.GenericKey == 0 && this.NameKey == null;

        /// <summary>
        /// Gets or sets a hint for this resource key (a custom description which helps identifying what is behind this key).
        /// </summary>
        public string? Hint
        {
            get
            {
                var result = _hint;

                if (string.IsNullOrEmpty(result))
                {
                    result = this.NameKey;
                }

                if (string.IsNullOrEmpty(result))
                {
                    result = string.Empty;
                }

                return result;
            }
            set => _hint = value;
        }

        /// <summary>
        /// Gets a short description of this object.
        /// </summary>
        public string Description
        {
            get
            {
                if (this.NameKey != null) { return "Name: " + this.NameKey; }
                return "Generic Id: " + this.GenericKey;
            }
        }
    }
}