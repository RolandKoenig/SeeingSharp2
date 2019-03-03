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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using PropertyTools.DataAnnotations;
using PropertyTools.Wpf;

namespace SeeingSharp.WpfSamples
{
    public class PropertyGridControlFactory : PropertyTools.Wpf.PropertyGridControlFactory
    {
        public override FrameworkElement CreateControl(PropertyItem property, PropertyControlFactoryOptions options)
        {
            if(property.ActualPropertyType.IsAssignableFrom(typeof(ICommand)))
            {
                property.HeaderPlacement = HeaderPlacement.Hidden;

                var result = new Button
                {
                    Content = property.DisplayName
                };
                result.SetBinding(ButtonBase.CommandProperty, property.CreateBinding());
                return result;
            }

            return base.CreateControl(property, options);
        }

        protected override FrameworkElement CreateDefaultControl(PropertyItem property)
        {
            property.AutoUpdateText = true;
            return base.CreateDefaultControl(property);
        }
    }
}