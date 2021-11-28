using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace SeeingSharp.WinUIDesktopSamples
{
    public class PropertyGridViewModel : ViewModelBase
    {
        private List<ConfigurablePropertyMetadata> _propertyMetadata;
        private object _selectedObject;

        public object SelectedObject
        {
            get => _selectedObject;
            set
            {
                if (_selectedObject != value)
                {
                    _selectedObject = value;
                    this.RaisePropertyChanged();

                    this.UpdatePropertyCollection();
                }
            }
        }

        public List<ConfigurablePropertyMetadata> PropertyMetadata
        {
            get => _propertyMetadata;
            set
            {
                if (_propertyMetadata != value)
                {
                    _propertyMetadata = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private void UpdatePropertyCollection()
        {
            var newPropertyMetadata = new List<ConfigurablePropertyMetadata>();

            var selectedObject = this.SelectedObject;

            if (selectedObject == null)
            {
                this.PropertyMetadata = newPropertyMetadata;
                return;
            }

            foreach (var actProperty in selectedObject.GetType().GetProperties())
            {
                // Check browsable attribute
                var browseAttrib = actProperty.GetCustomAttribute<BrowsableAttribute>();

                if (browseAttrib != null &&
                   !browseAttrib.Browsable)
                {
                    continue;
                }

                newPropertyMetadata.Add(new ConfigurablePropertyMetadata(actProperty, selectedObject));
            }

            this.PropertyMetadata = newPropertyMetadata;
        }
    }
}