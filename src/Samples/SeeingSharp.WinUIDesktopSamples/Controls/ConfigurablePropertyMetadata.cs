using System;
using System.ComponentModel;
using System.Reflection;

namespace SeeingSharp.WinUIDesktopSamples
{
    public class ConfigurablePropertyMetadata
    {
        private object _hostObject;
        private PropertyInfo _propertyInfo;

        public object? ValueAccessor
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

        public object? GetValue()
        {
            return _propertyInfo.GetValue(_hostObject);
        }

        public void SetValue(object? value)
        {
            var givenType = value?.GetType();
            var targetType = _propertyInfo.PropertyType;

            if ((givenType != null) && (givenType == targetType))
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