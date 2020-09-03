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
using System.ComponentModel;
using System.Reflection;

namespace SeeingSharp.UwpSamples.Controls
{
    public class ConfigurablePropertyMetadata
    {
        private object _hostObject;
        private PropertyInfo _propertyInfo;

        public object ValueAccessor
        {
            get => this.GetValue();
            set => this.SetValue(value);
        }

        public string CategoryName
        {
            get;
            set;
        }

        public string PropertyName
        {
            get;
            set;
        }

        public string PropertyDisplayName
        {
            get;
            set;
        }

        public PropertyValueType ValueType
        {
            get;
            set;
        }

        internal ConfigurablePropertyMetadata(PropertyInfo propertyInfo, object hostObject)
        {
            _propertyInfo = propertyInfo;
            _hostObject = hostObject;

            this.CategoryName = propertyInfo.GetCustomAttribute<CategoryAttribute>()?.Category ?? string.Empty;

            this.PropertyName = propertyInfo.Name;
            this.PropertyDisplayName = propertyInfo.Name;
            var attribDisplayName = propertyInfo.GetCustomAttribute<DisplayNameAttribute>();
            if (attribDisplayName != null)
            {
                this.PropertyDisplayName = attribDisplayName.DisplayName;
            }

            var propertyType = _propertyInfo.PropertyType;
            if (propertyType == typeof(bool))
            {
                this.ValueType = PropertyValueType.Bool;
            }
            else if (propertyType == typeof(string) ||
                    propertyType == typeof(double) || propertyType == typeof(float) || propertyType == typeof(decimal) ||
                    propertyType == typeof(int) || propertyType == typeof(uint) ||
                    propertyType == typeof(byte) ||
                    propertyType == typeof(short) || propertyType == typeof(ushort) ||
                    propertyType == typeof(long) || propertyType == typeof(ulong))
            {
                this.ValueType = PropertyValueType.String;
            }
            else if (propertyType.IsSubclassOf(typeof(Enum)))
            {
                this.ValueType = PropertyValueType.Enum;
            }
            else
            {
                throw new ApplicationException($"Unsupported property type {propertyType.FullName}!");
            }
        }

        public override string ToString()
        {
            return $"{this.CategoryName} - {this.PropertyDisplayName} (type {this.ValueType})";
        }

        public Array GetEnumMembers()
        {
            if (this.ValueType != PropertyValueType.Enum) { throw new InvalidOperationException($"Method {nameof(this.GetEnumMembers)} not supported on value type {this.ValueType}!"); }
            return Enum.GetValues(_propertyInfo.PropertyType);
        }

        public object GetValue()
        {
            return _propertyInfo.GetValue(_hostObject);
        }

        public void SetValue(object value)
        {
            var givenType = value.GetType();
            var targetType = _propertyInfo.PropertyType;
            if (givenType == targetType)
            {
                _propertyInfo.SetValue(_hostObject, value);
            }
            else
            {
                _propertyInfo.SetValue(_hostObject, Convert.ChangeType(value, targetType));
            }
        }
    }
}