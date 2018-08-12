#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
using SeeingSharp.Util;
using System;
using System.Reflection;
using System.Text;
using SharpDX;

namespace SeeingSharp.Multimedia.Objects
{
    public class MaterialProperties : IEquatable<MaterialProperties>
    {
        /// <summary>
        /// Gets an empty material.
        /// </summary>
        public static readonly MaterialProperties Empty = new MaterialProperties();

        public MaterialProperties()
        {
            this.DiffuseColor = Color4.White;
            this.Name = string.Empty;
        }

        /// <summary>
        /// Gets a key string which is equal for all materials which share the same properties.
        /// It is much unlikely that materials with different properties gets same results.
        /// </summary>
        public string GetDynamicResourceKey()
        {
            StringBuilder resultBuilder = new StringBuilder(100);
            resultBuilder.Append("DyamicMaterial|");
            resultBuilder.Append(this.AmbientColor.GetHashCode().ToString());
            resultBuilder.Append(this.DiffuseColor.GetHashCode().ToString());
            resultBuilder.Append(this.EmissiveColor.GetHashCode().ToString());
            resultBuilder.Append(this.Key.GetHashCode().ToString());
            resultBuilder.Append(this.Shininess.GetHashCode().ToString());
            resultBuilder.Append(this.SpecularColor.GetHashCode().ToString());
            resultBuilder.Append(this.TextureKey.GetHashCode().ToString());
            return resultBuilder.ToString();
        }

        public MaterialProperties Clone()
        {
            return base.MemberwiseClone() as MaterialProperties;
        }

        public override bool Equals(object obj)
        {
            MaterialProperties other = obj as MaterialProperties;
            if(other == null) { return false; }

            return this.Equals(other);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        public bool Equals(MaterialProperties other)
        {
            if(other == null) { return false; }

            return
                (this.Key == other.Key) &&
                (this.TextureKey == other.TextureKey) &&
                (this.DiffuseColor == other.DiffuseColor) &&
                (this.AmbientColor == other.AmbientColor) &&
                (this.EmissiveColor == other.EmissiveColor) &&
                (this.SpecularColor == other.SpecularColor) &&
                (this.Shininess == other.Shininess);
        }

        public override int GetHashCode()
        {
            return
                this.Key.GetHashCode() ^
                this.TextureKey.GetHashCode() ^
                this.DiffuseColor.GetHashCode() ^
                this.AmbientColor.GetHashCode() ^
                this.EmissiveColor.GetHashCode() ^
                this.SpecularColor.GetHashCode() ^
                this.Shininess.GetHashCode();
        }

        public static bool operator ==(MaterialProperties left, MaterialProperties right)
        {
            if(Object.ReferenceEquals(left, right)) { return true; }
            if(Object.ReferenceEquals(left, null)) { return false; }

            return left.Equals(right);
        }

        public static bool operator !=(MaterialProperties left, MaterialProperties right)
        {
            if (Object.ReferenceEquals(left, right)) { return false; }
            if (Object.ReferenceEquals(left, null)) { return true; }

            return !left.Equals(right);
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