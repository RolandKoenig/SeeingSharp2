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
using System;
using System.Text;
using SeeingSharp.Util;
using SharpDX;

namespace SeeingSharp.Multimedia.Objects
{
    public class MaterialProperties : IEquatable<MaterialProperties>
    {
        /// <summary>
        /// Gets an empty material.
        /// </summary>
        public static readonly MaterialProperties Empty = new MaterialProperties();

        /// <summary>
        /// Gets a key string which is equal for all materials which share the same properties.
        /// It is much unlikely that materials with different properties gets same results.
        /// </summary>
        public string GetDynamicResourceKey()
        {
            var resultBuilder = new StringBuilder(100);
            resultBuilder.Append("DyamicMaterial|");
            resultBuilder.Append(AmbientColor.GetHashCode().ToString());
            resultBuilder.Append(DiffuseColor.GetHashCode().ToString());
            resultBuilder.Append(EmissiveColor.GetHashCode().ToString());
            resultBuilder.Append(Key.GetHashCode().ToString());
            resultBuilder.Append(Shininess.GetHashCode().ToString());
            resultBuilder.Append(SpecularColor.GetHashCode().ToString());
            resultBuilder.Append(TextureKey.GetHashCode().ToString());
            return resultBuilder.ToString();
        }

        public MaterialProperties Clone()
        {
            return MemberwiseClone() as MaterialProperties;
        }

        public override bool Equals(object obj)
        {
            var other = obj as MaterialProperties;

            if (other == null)
            {
                return false;
            }

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return
                Key.GetHashCode() ^
                TextureKey.GetHashCode() ^
                DiffuseColor.GetHashCode() ^
                AmbientColor.GetHashCode() ^
                EmissiveColor.GetHashCode() ^
                SpecularColor.GetHashCode() ^
                Shininess.GetHashCode();
        }

        public static bool operator ==(MaterialProperties left, MaterialProperties right)
        {
            if(ReferenceEquals(left, right)) { return true; }
            if(ReferenceEquals(left, null)) { return false; }

            return left.Equals(right);
        }

        public static bool operator !=(MaterialProperties left, MaterialProperties right)
        {
            if (ReferenceEquals(left, right)) { return false; }
            if (ReferenceEquals(left, null)) { return true; }

            return !left.Equals(right);
        }

        public MaterialProperties()
        {
            DiffuseColor = Color4.White;
            Name = string.Empty;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        public bool Equals(MaterialProperties other)
        {
            if(other == null) { return false; }

            return
                Key == other.Key &&
                TextureKey == other.TextureKey &&
                DiffuseColor == other.DiffuseColor &&
                AmbientColor == other.AmbientColor &&
                EmissiveColor == other.EmissiveColor &&
                SpecularColor == other.SpecularColor &&
                Shininess == other.Shininess;
        }

        /// <summary>
        /// Gets or sets the name of this material.
        /// This value can be used internally by importers and exporters.
        /// Within seeing#, this property has no relevance.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the material.
        /// </summary>
        public NamedOrGenericKey Key
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the path to the texture.
        /// </summary>
        public NamedOrGenericKey TextureKey
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the diffuse color component of this material.
        /// </summary>
        public Color4 DiffuseColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ambient color component.
        /// </summary>
        public Color4 AmbientColor
        {
            get;
            set;
        }

        public Color4 EmissiveColor
        {
            get;
            set;
        }

        public Color4 SpecularColor
        {
            get;
            set;
        }

        public float Shininess
        {
            get;
            set;
        }
    }
}