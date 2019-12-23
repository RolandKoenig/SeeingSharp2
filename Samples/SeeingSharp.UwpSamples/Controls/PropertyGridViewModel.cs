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

using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace SeeingSharp.UwpSamples.Controls
{
    public class PropertyGridViewModel : ViewModelBase
    {
        private List<ConfigurablePropertyMetadata> m_propertyMetadata;
        private object m_selectedObject;

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

        public object SelectedObject
        {
            get => m_selectedObject;
            set
            {
                if (m_selectedObject != value)
                {
                    m_selectedObject = value;
                    this.RaisePropertyChanged();

                    this.UpdatePropertyCollection();
                }
            }
        }

        public List<ConfigurablePropertyMetadata> PropertyMetadata
        {
            get => m_propertyMetadata;
            set
            {
                if (m_propertyMetadata != value)
                {
                    m_propertyMetadata = value;
                    this.RaisePropertyChanged();
                }
            }
        }
    }
}