using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace SeeingSharp.UwpSamples.Controls
{
    public class PropertyGridViewModel : ViewModelBase
    {
        private object m_selectedObject;
        private List<ConfigurablePropertyMetadata> m_propertyMetadata;

        private void UpdatePropertyCollection()
        {
            var newPropertyMetadata = new List<ConfigurablePropertyMetadata>();

            var selectedObject = this.SelectedObject;
            if(selectedObject == null)
            {
                this.PropertyMetadata = newPropertyMetadata;
                return;
            }

            foreach(var actProperty in selectedObject.GetType().GetProperties())
            {
                // Check browsable attribute
                BrowsableAttribute browseAttrib = actProperty.GetCustomAttribute<BrowsableAttribute>();
                if((browseAttrib != null) &&
                   (!browseAttrib.Browsable))
                {
                    continue;
                }

                newPropertyMetadata.Add(new ConfigurablePropertyMetadata(actProperty, selectedObject));
            }

            this.PropertyMetadata = newPropertyMetadata;
        }

        public object SelectedObject
        {
            get => m_selectedObject;
            set
            {
                if(m_selectedObject != value)
                {
                    m_selectedObject = value;
                    RaisePropertyChanged();

                    UpdatePropertyCollection();
                }
            }            
        }

        public List<ConfigurablePropertyMetadata> PropertyMetadata
        {
            get => m_propertyMetadata;
            set
            {
                if(m_propertyMetadata != value)
                {
                    m_propertyMetadata = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
