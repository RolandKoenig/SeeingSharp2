﻿#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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

namespace SeeingSharp.UwpSamples.Controls
{
    #region using

    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;

    #endregion

    public class PropertyGridViewModel : ViewModelBase
    {
        private object m_selectedObject;
        private List<ConfigurablePropertyMetadata> m_propertyMetadata;

        private void UpdatePropertyCollection()
        {
            var newPropertyMetadata = new List<ConfigurablePropertyMetadata>();

            var selectedObject = SelectedObject;

            if (selectedObject == null)
            {
                PropertyMetadata = newPropertyMetadata;
                return;
            }

            foreach (var actProperty in selectedObject.GetType().GetProperties())
            {
                // Check browsable attribute
                var browseAttrib = actProperty.GetCustomAttribute<BrowsableAttribute>();

                if ((browseAttrib != null) &&
                   (!browseAttrib.Browsable))
                {
                    continue;
                }

                newPropertyMetadata.Add(new ConfigurablePropertyMetadata(actProperty, selectedObject));
            }

            PropertyMetadata = newPropertyMetadata;
        }

        public object SelectedObject
        {
            get => m_selectedObject;
            set
            {
                if (m_selectedObject == value)
                {
                    return;
                }

                m_selectedObject = value;
                RaisePropertyChanged();

                UpdatePropertyCollection();
            }
        }

        public List<ConfigurablePropertyMetadata> PropertyMetadata
        {
            get => m_propertyMetadata;
            set
            {
                if (m_propertyMetadata == value)
                {
                    return;
                }

                m_propertyMetadata = value;
                RaisePropertyChanged();
            }
        }
    }
}