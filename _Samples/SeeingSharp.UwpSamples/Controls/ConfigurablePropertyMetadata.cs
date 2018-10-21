using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharp.UwpSamples.Controls
{
    public class ConfigurablePropertyMetadata
    {
        private PropertyInfo m_propertyInfo;
        private Type m_hostType;
        private object m_hostObject;

        internal ConfigurablePropertyMetadata(PropertyInfo propertyInfo, object hostObject)
        {
            m_propertyInfo = propertyInfo;
            m_hostType = hostObject.GetType();
            m_hostObject = hostObject;

            this.CategoryName = propertyInfo.GetCustomAttribute<CategoryAttribute>()?.Category ?? string.Empty;

            this.PropertyName = propertyInfo.Name;
            this.PropertyDisplayName = propertyInfo.Name;
            var attribDisplayName = propertyInfo.GetCustomAttribute<DisplayNameAttribute>();
            if(attribDisplayName != null)
            {
                this.PropertyDisplayName = attribDisplayName.DisplayName;
            }

            var propertyType = m_propertyInfo.PropertyType;
            if(propertyType == typeof(bool))
            {
                this.ValueType = PropertyValueType.Bool;
            }
            else if(propertyType == typeof(string))
            {
                this.ValueType = PropertyValueType.String;
            }
            else if(propertyType.IsSubclassOf(typeof(Enum)))
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
            if(this.ValueType != PropertyValueType.Enum) { throw new InvalidOperationException($"Method {nameof(GetEnumMembers)} not supported on value type {this.ValueType}!"); }
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
