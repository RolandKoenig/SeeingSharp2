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

using System;
using System.ComponentModel;
using System.Reflection;

namespace SeeingSharp.UwpSamples.Controls
{
    #region using
    #endregion

    public class ConfigurablePropertyMetadata
    {
        private object m_hostObject;
        private Type m_hostType;
        private PropertyInfo m_propertyInfo;

        public override string ToString()
        {
            return $"{CategoryName} - {PropertyDisplayName} (type {ValueType})";
        }

        public Array GetEnumMembers()
        {
            if(ValueType != PropertyValueType.Enum) { throw new InvalidOperationException($"Method {nameof(GetEnumMembers)} not supported on value type {ValueType}!"); }
            return Enum.GetValues(m_propertyInfo.PropertyType);
        }

        public object GetValue()
        {
            return m_propertyInfo.GetValue(m_hostObject);
        }

        public void SetValue(object value)
        {
            m_propertyInfo.SetValue(m_hostObject, value);
        }

        internal ConfigurablePropertyMetadata(PropertyInfo propertyInfo, object hostObject)
        {
            m_propertyInfo = propertyInfo;
            m_hostType = hostObject.GetType();
            m_hostObject = hostObject;

            CategoryName = propertyInfo.GetCustomAttribute<CategoryAttribute>()?.Category ?? string.Empty;

            PropertyName = propertyInfo.Name;
            PropertyDisplayName = propertyInfo.Name;
            var attribDisplayName = propertyInfo.GetCustomAttribute<DisplayNameAttribute>();
            if(attribDisplayName != null)
            {
                PropertyDisplayName = attribDisplayName.DisplayName;
            }

            var propertyType = m_propertyInfo.PropertyType;
            if(propertyType == typeof(bool))
            {
                ValueType = PropertyValueType.Bool;
            }
            else if(propertyType == typeof(string))
            {
                ValueType = PropertyValueType.String;
            }
            else if(propertyType.IsSubclassOf(typeof(Enum)))
            {
                ValueType = PropertyValueType.Enum;
            }
            else
            {
                throw new ApplicationException($"Unsupported property type {propertyType.FullName}!");
            }
        }

        public object ValueAccessor
        {
            get => GetValue();
            set => SetValue(value);
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
    }
}